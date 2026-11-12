using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Api.Agente.Aplicacao.Conhecimento;
using Api.Agente.Aplicacao.Consumo;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Dominio.Consumo;
using Api.Agente.Dominio.Perguntas;

namespace Api.Agente.Aplicacao.Perguntas;

/// <summary>
/// Responde uma pergunta sobre a ECF só com os trechos de uma versão do manual (REQ-PERGUNTA-001 a 009; mudança 17, D1).
/// A ordem é sempre a mesma: validar, conferir o limite, resolver a versão, buscar, redigir, montar e registrar.
/// </summary>
internal sealed class ServicoDePerguntas(
    ConfiguracaoDasPerguntas configuracao,
    IConsultaAoManual consulta,
    IRepositorioDePerguntas repositorio,
    TimeProvider relogio,
    IRedatorDeRespostas? redator = null,
    RegistradorDeConsumo? consumo = null,
    CalculadoraDeCusto? calculadora = null)
{
    public const string TipoDoDocumento = "manual-ecf";
    public const int TamanhoMaximoDaPergunta = 500;

    public bool Habilitada => configuracao.Habilitada;

    public async Task<RespostaDaPergunta> ResponderAsync(PedidoDePergunta pedido, CancellationToken cancelamento)
    {
        var inicio = relogio.GetUtcNow();
        var cronometro = Stopwatch.StartNew();
        var id = Guid.NewGuid();
        var pergunta = pedido.Pergunta.Trim();
        var montagem = new Montagem(id, inicio, configuracao.VersaoDoServico);
        try
        {
            var resposta = await ResponderSemRegistroAsync(pergunta, pedido, montagem, inicio, cancelamento);
            await repositorio.RegistrarAsync(Registro(resposta, montagem, pedido, pergunta), cancelamento);
            return Finalizar(resposta, cronometro);
        }
        catch (FalhaDoRepositorio)
        {
            return Finalizar(montagem.Erro("falha_tecnica", "O registro da pergunta não respondeu; tente novamente."), cronometro);
        }
    }

    private async Task<RespostaDaPergunta> ResponderSemRegistroAsync(
        string pergunta, PedidoDePergunta pedido, Montagem montagem, DateTimeOffset inicio, CancellationToken cancelamento)
    {
        if (pergunta.Length == 0)
        {
            return montagem.Erro("pergunta_invalida", "Envie uma pergunta com texto.");
        }

        if (pergunta.Length > TamanhoMaximoDaPergunta)
        {
            return montagem.Erro("pergunta_acima_do_limite", $"A pergunta passa de {TamanhoMaximoDaPergunta} caracteres; envie uma pergunta mais curta.");
        }

        var inicioDoDia = new DateTimeOffset(inicio.UtcDateTime.Date, TimeSpan.Zero);
        if (await repositorio.ContarRespondidasDesdeAsync(inicioDoDia, cancelamento) >= configuracao.LimitePorDia)
        {
            return montagem.Erro("limite_de_uso", "O limite diário de perguntas foi atingido; tente amanhã.");
        }

        return await ResponderComFonteAsync(pergunta, pedido, montagem, cancelamento);
    }

    private async Task<RespostaDaPergunta> ResponderComFonteAsync(
        string pergunta, PedidoDePergunta pedido, Montagem montagem, CancellationToken cancelamento)
    {
        var versao = ResolvedorDeVersao.Informada(pedido.VersaoInformada) ?? ResolvedorDeVersao.Resolver(pergunta);
        if (versao.Versao is null)
        {
            return montagem.Parcial(TextosDaPergunta.VersaoForaDaBase, null, [], []);
        }

        montagem.VersaoDoManual = versao.Versao;
        if (versao.Origem == OrigemDaVersao.MaisRecenteDasCitadas)
        {
            montagem.Observacoes.Add(TextosDaPergunta.ComparacaoNaoAtendida);
        }

        var busca = await consulta.BuscarAsync(
            new PedidoDeTrechos(pergunta, TipoDoDocumento, versao.Versao, ExtratorDeCodigo.Extrair(pergunta), montagem.Id, pedido.Correlacao),
            cancelamento);
        switch (busca.Desfecho)
        {
            case DesfechoDaConsulta.Trechos:
                break;
            case DesfechoDaConsulta.VersaoIndisponivel:
                return montagem.Parcial(TextosDaPergunta.VersaoForaDaBase, versao.Versao, [], []);
            default:
                return montagem.Erro("consulta_indisponivel", "A consulta ao manual está indisponível; a pergunta não pôde ser respondida.");
        }

        montagem.Trechos = busca.Trechos;
        IReadOnlyList<TrechoDoManual> trechos = configuracao.RelevanciaMinima is { } minima
            ? [.. busca.Trechos.Where(trecho => trecho.Relevancia >= minima)]
            : busca.Trechos;
        if (trechos.Count == 0)
        {
            return montagem.Parcial(TextosDaPergunta.ForaDosManuais(versao.Versao), versao.Versao, [], []);
        }

        if (!configuracao.RedacaoHabilitada || redator is null)
        {
            return Reserva(montagem, versao.Versao, trechos);
        }

        montagem.Modelo = redator.Modelo;
        var aceita = await RedigirAsync(new MaterialDaRedacao(pergunta, versao.Versao, trechos), montagem, pedido.Correlacao, cancelamento);
        if (aceita is null)
        {
            return Reserva(montagem, versao.Versao, trechos);
        }

        montagem.Redacao = DesfechoDaRedacao.Aceita;
        return aceita.Respondida
            ? montagem.Concluida(aceita.Resposta, versao.Versao, aceita.Citados)
            : montagem.Parcial(TextosDaPergunta.ForaDosManuais(versao.Versao), versao.Versao, [], []);
    }

    private static RespostaDaPergunta Reserva(Montagem montagem, string versao, IReadOnlyList<TrechoDoManual> trechos)
    {
        var (texto, usados) = TextosDaPergunta.Reserva(trechos);
        montagem.Redacao = DesfechoDaRedacao.Reserva;
        return montagem.Parcial(texto, versao, usados, []);
    }

    /// <summary>Até 1 + <c>Retentativas</c> tentativas, só para falha transitória; cada uma registra o consumo (D5, D10).</summary>
    private async Task<RedacaoAceita?> RedigirAsync(MaterialDaRedacao material, Montagem montagem, string? correlacao, CancellationToken cancelamento)
    {
        for (var tentativa = 1; ; tentativa++)
        {
            var inicio = relogio.GetUtcNow();
            var cronometro = Stopwatch.StartNew();
            try
            {
                var saida = await redator!.RedigirAsync(material, cancelamento);
                await RegistrarConsumoAsync(montagem.Id, correlacao, tentativa, StatusDaChamada.Sucesso, saida.TokensEntrada, saida.TokensSaida, inicio, cronometro, cancelamento);
                return ValidadorDaRedacao.Validar(saida.Texto, material.Trechos);
            }
            catch (FalhaDoRedator falha)
            {
                await RegistrarConsumoAsync(montagem.Id, correlacao, tentativa, StatusDaChamada.Falha, falha.TokensEntrada, falha.TokensSaida, inicio, cronometro, cancelamento);
                if (!falha.Transitoria || tentativa > configuracao.Retentativas)
                {
                    return null;
                }
            }
        }
    }

    private async Task RegistrarConsumoAsync(
        Guid idPergunta, string? correlacao, int tentativa, StatusDaChamada status, int? tokensEntrada, int? tokensSaida,
        DateTimeOffset inicio, Stopwatch cronometro, CancellationToken cancelamento)
    {
        if (consumo is null)
        {
            return;
        }

        var custo = calculadora?.Calcular(redator!.Modelo, tokensEntrada, tokensSaida, inicio) ?? new CustoCalculado(null, OrigemDoValor.Desconhecido, null);
        await consumo.RegistrarAsync(
            new RegistroDeConsumo(
                Guid.NewGuid(), idPergunta, correlacao, redator!.Provedor, redator.Modelo,
                tentativa == 1 ? TipoDeChamada.Geracao : TipoDeChamada.Retentativa, status, inicio, cronometro.Elapsed.TotalMilliseconds, tentativa,
                tokensEntrada, tokensEntrada is null ? OrigemDoValor.Desconhecido : OrigemDoValor.Informado,
                tokensSaida, tokensSaida is null ? OrigemDoValor.Desconhecido : OrigemDoValor.Informado,
                custo.CustoUsd, custo.Origem, custo.VersaoDaTabela, FinalidadeDaChamada.Pergunta),
            cancelamento);
    }

    private RegistroDaPergunta Registro(RespostaDaPergunta resposta, Montagem montagem, PedidoDePergunta pedido, string pergunta) => new(
        resposta.IdPergunta,
        pedido.IdSolicitacao,
        pedido.Canal,
        resposta.Status,
        resposta.Erro?.Codigo,
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(pergunta))),
        pergunta.Length,
        resposta.VersaoDoManual,
        [.. montagem.Trechos.Select(trecho => new TrechoRegistrado(trecho.IdTrecho, trecho.Pagina, trecho.Relevancia))],
        [.. resposta.Fontes.Select(fonte => fonte.IdTrecho)],
        montagem.Redacao,
        montagem.Modelo,
        resposta.CriadaEm,
        relogio.GetUtcNow());

    private static RespostaDaPergunta Finalizar(RespostaDaPergunta resposta, Stopwatch cronometro) =>
        resposta with { DuracaoMs = cronometro.ElapsedMilliseconds };

    /// <summary>Estado da montagem de uma resposta; os metadados vêm do sistema (P13).</summary>
    private sealed class Montagem(Guid id, DateTimeOffset criadaEm, string versaoDoServico)
    {
        public Guid Id { get; } = id;

        public string? VersaoDoManual { get; set; }

        public string? Modelo { get; set; }

        public DesfechoDaRedacao Redacao { get; set; } = DesfechoDaRedacao.NaoChamada;

        public IReadOnlyList<TrechoDoManual> Trechos { get; set; } = [];

        public List<string> Observacoes { get; } = [];

        public RespostaDaPergunta Concluida(string texto, string versao, IReadOnlyList<TrechoDoManual> citados) =>
            Montar(StatusDaPergunta.Concluido, texto, versao, citados, null);

        public RespostaDaPergunta Parcial(string texto, string? versao, IReadOnlyList<TrechoDoManual> usados, IReadOnlyList<string> extras) =>
            Montar(StatusDaPergunta.Parcial, texto, versao, usados, null, extras);

        public RespostaDaPergunta Erro(string codigo, string mensagem) =>
            Montar(StatusDaPergunta.Erro, mensagem, VersaoDoManual, [], new ErroDaPergunta(codigo, mensagem));

        private RespostaDaPergunta Montar(
            string status, string texto, string? versao, IReadOnlyList<TrechoDoManual> trechos, ErroDaPergunta? erro, IReadOnlyList<string>? extras = null) => new(
            RespostaDaPergunta.Contrato,
            Id,
            status,
            texto,
            versao,
            [.. trechos.Select(trecho => new FonteDaResposta(trecho.Titulo, trecho.Versao, trecho.Pagina, trecho.IdTrecho))],
            [.. trechos.Select(TextosDaPergunta.Fonte), .. Observacoes, .. extras ?? [], TextosDaPergunta.Limitacao],
            erro,
            new VersoesDaPergunta(RespostaDaPergunta.Contrato, versaoDoServico, PromptDaPergunta.Versao, Modelo),
            criadaEm,
            0);
    }
}
