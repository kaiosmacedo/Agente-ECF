using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Api.Agente.Aplicacao.Analise;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Aplicacao.Perguntas;
using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Aplicacao.ContratoSofia;

/// <summary>Desfecho do processamento: um resultado do contrato ou o motivo de não haver como correlacionar um.</summary>
internal sealed record DesfechoDaSolicitacao(ResultadoAnaliseSofia? Resultado, string? MotivoDaRejeicao);

/// <summary>
/// Entrada do canal A2A (REQ-A2A-002): lê o contrato SOFIA, valida os identificadores e o contrato, entrega o pedido
/// normalizado ao serviço de análise, registra a execução e projeta o resultado. O conteúdo é dado não confiável (P10).
/// </summary>
internal sealed class ProcessadorDeSolicitacoes(IRepositorioDeExecucoes repositorio, ServicoDeAnalise servico, ServicoDePerguntas? perguntas = null)
{
    /// <summary>Sem catálogo nem consulta ao manual: as etapas 4 e 5 não rodam. Para os testes do contrato.</summary>
    public ProcessadorDeSolicitacoes(IRepositorioDeExecucoes repositorio, AmbienteDaAnalise ambiente, TimeProvider relogio)
        : this(repositorio, new ServicoDeAnalise(ambiente, relogio))
    {
    }

    public async Task<DesfechoDaSolicitacao> ProcessarAsync(string json, string? correlacao, CancellationToken cancelamento)
    {
        SolicitacaoAnaliseSofia? solicitacao;
        try
        {
            solicitacao = JsonSerializer.Deserialize<SolicitacaoAnaliseSofia>(json, ContratoSofiaV1.Json);
        }
        catch (JsonException)
        {
            return new DesfechoDaSolicitacao(null, "json_invalido");
        }

        if (solicitacao is null || !ValidadorDaSolicitacao.IdentificadoresValidos(solicitacao))
        {
            return new DesfechoDaSolicitacao(null, "identificadores_invalidos");
        }

        // Pedido que só traz a pergunta, como o da Sofia central sem arquivo (REQ-PERGUNTA-001; mudança 17, D2).
        if (perguntas is { Habilitada: true } && ValidarContrato(solicitacao) is null && SoTrazAPergunta(solicitacao))
        {
            var resposta = await perguntas.ResponderAsync(
                new PedidoDePergunta(solicitacao.Pergunta!, "a2a", Guid.Parse(solicitacao.IdSolicitacao!), null, correlacao), cancelamento);
            return new DesfechoDaSolicitacao(ProjecaoDaPerguntaParaSofia.Projetar(solicitacao, resposta), null);
        }

        try
        {
            // Retentativa da mesma chamada devolve o que já foi registrado (D7).
            var envelope = await repositorio.BuscarPorSolicitacaoAsync(Guid.Parse(solicitacao.IdSolicitacao!), cancelamento)
                ?? await repositorio.RegistrarAsync(await AnalisarAsync(solicitacao, correlacao, cancelamento), cancelamento);
            return new DesfechoDaSolicitacao(ProjecaoParaSofia.Projetar(envelope), null);
        }
        catch (FalhaDoRepositorio)
        {
            return new DesfechoDaSolicitacao(FalhaTecnica(solicitacao), null);
        }
    }

    private Task<EnvelopeDoResultado> AnalisarAsync(SolicitacaoAnaliseSofia solicitacao, string? correlacao, CancellationToken cancelamento)
    {
        var erro = ValidarContrato(solicitacao);
        IReadOnlyList<DocumentoDoPedido> documentos = erro is null
            ? [.. solicitacao.Documentos!.Select(documento => new DocumentoDoPedido(documento.IdDocumento!, documento.Texto!, Hash(documento.Texto!)))]
            : [];
        return servico.AnalisarAsync(
            new PedidoDeAnalise(
                Guid.Parse(solicitacao.IdExecucao!), Guid.Parse(solicitacao.IdSolicitacao!), CanalDaAnalise.A2A, solicitacao.Usuario?.Nome, documentos, erro),
            correlacao,
            cancelamento);
    }

    private static ErroDoEnvelope? ValidarContrato(SolicitacaoAnaliseSofia solicitacao)
    {
        if (solicitacao.VersaoContrato != ContratoSofiaV1.Versao)
        {
            return new ErroDoEnvelope("versao_contrato_nao_suportada", $"Este agente aceita somente a versão {ContratoSofiaV1.Versao} do contrato.");
        }

        var camposInvalidos = ValidadorDaSolicitacao.CamposInvalidos(solicitacao);
        return camposInvalidos.Count > 0
            ? new ErroDoEnvelope("solicitacao_invalida", $"Campos ausentes ou inválidos: {string.Join(", ", camposInvalidos)}.")
            : null;
    }

    private static ResultadoAnaliseSofia FalhaTecnica(SolicitacaoAnaliseSofia solicitacao)
    {
        const string Mensagem = "O registro da execução não respondeu; tente novamente.";
        return new ResultadoAnaliseSofia(
            ContratoSofiaV1.Versao,
            solicitacao.IdExecucao!,
            solicitacao.IdSolicitacao!,
            ContratoSofiaV1.Agente,
            ContratoSofiaV1.StatusErro,
            $"A solicitação não pôde ser analisada. {Mensagem}",
            [],
            [],
            new ErroSofia("falha_tecnica", Mensagem),
            Veredito: MontadorDoEnvelope.NomeDoVeredito(VereditoDaExecucao.NaoAvaliado));
    }

    /// <summary>Um único documento cujo texto, sem espaços nas pontas, é a própria pergunta (D2).</summary>
    public static bool SoTrazAPergunta(SolicitacaoAnaliseSofia solicitacao) =>
        solicitacao is { Pergunta: { } pergunta, Documentos: [{ Texto: { } texto }] }
        && string.Equals(texto.Trim(), pergunta.Trim(), StringComparison.Ordinal);

    private static string Hash(string texto) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
}
