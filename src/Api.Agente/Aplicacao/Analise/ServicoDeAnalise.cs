using System.Globalization;
using Api.Agente.Aplicacao.Conhecimento;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Dominio.Execucoes;
using Api.Agente.Dominio.Leiaute;

namespace Api.Agente.Aplicacao.Analise;

/// <summary>Por onde o pedido chegou (mudança 07, D1).</summary>
internal enum CanalDaAnalise
{
    A2A,
    Rest,
}

/// <summary>Um documento do pedido, com o hash calculado pelo canal: sobre o texto no A2A, sobre os bytes no REST (GD-06).</summary>
internal sealed record DocumentoDoPedido(string IdDocumento, string Texto, string Hash);

/// <summary>
/// Pedido normalizado, o mesmo para os dois canais (REQ-RECEPCAO-008; REQ-A2A-002). <paramref name="ErroDeEntrada"/>
/// traz uma rejeição já detectada pelo canal, como contrato inválido ou corpo acima do limite.
/// </summary>
internal sealed record PedidoDeAnalise(
    Guid IdExecucaoExterna,
    Guid IdSolicitacao,
    CanalDaAnalise Canal,
    string? NomeDoUsuario,
    IReadOnlyList<DocumentoDoPedido> Documentos,
    ErroDoEnvelope? ErroDeEntrada = null);

/// <summary>O que o serviço precisa saber do ambiente, sem depender da infraestrutura (mudança 07, D4).</summary>
internal sealed record AmbienteDaAnalise(
    long TamanhoMaximoDosDocumentosEmBytes,
    string OrganizacaoPadrao,
    string VersaoDoServico,
    int TamanhoMaximoDaLinhaEmCaracteres = int.MaxValue,
    int QuantidadeMaximaDeLinhas = int.MaxValue);

/// <summary>
/// Serviço de aplicação comum aos canais: executa as etapas 1 a 3 do fluxo (P03) sobre o pedido normalizado e monta o
/// envelope. O conteúdo recebido é dado não confiável (P10): ele só é lido, nunca interpretado como instrução.
/// </summary>
internal sealed class ServicoDeAnalise(
    AmbienteDaAnalise ambiente,
    TimeProvider relogio,
    Func<string, IdentificacaoDoLeiaute>? identificador = null,
    IReadOnlyDictionary<int, CatalogoDoLeiaute>? catalogos = null,
    EtapaDeConsultaAoManual? consultaAoManual = null,
    ParametrosDaValidacao? validacao = null)
{
    private readonly Func<string, IdentificacaoDoLeiaute> identificar = identificador ?? IdentificadorDeLeiaute.Identificar;

    public const string LimitacaoDocumentoCortado = "documento cortado antes de chegar ao agente";

    public async Task<EnvelopeDoResultado> AnalisarAsync(PedidoDeAnalise pedido, string? correlacao, CancellationToken cancelamento)
    {
        var execucao = Execucao.Criar(
            pedido.IdExecucaoExterna,
            pedido.IdSolicitacao,
            IdentidadeDaRequisicao.Declarada(pedido.NomeDoUsuario, ambiente.OrganizacaoPadrao),
            relogio.GetUtcNow());
        execucao.ConcluirEtapa(Etapa.Recepcao);

        var erro = pedido.ErroDeEntrada;
        DocumentoDoPedido? ecf = null;
        if (erro is null)
        {
            foreach (var documento in pedido.Documentos)
            {
                execucao.RegistrarDocumento(new DocumentoRecebido(documento.IdDocumento, documento.Hash, documento.Texto.Length));
            }

            (erro, ecf) = ValidarEntrada(pedido.Documentos);
        }

        string sintese;
        if (erro is not null)
        {
            execucao.FalharEtapa(Etapa.ValidacaoDaEntrada, CausaDaFalha.Documento);
            sintese = $"A solicitação não pôde ser analisada. {erro.Mensagem}";
        }
        else
        {
            execucao.ConcluirEtapa(Etapa.ValidacaoDaEntrada);
            foreach (var outro in pedido.Documentos.Where(documento => !ReferenceEquals(documento, ecf)))
            {
                execucao.AdicionarObservacao($"Documento {outro.IdDocumento} não analisado por este agente: não é uma ECF.");
            }

            Identificar(execucao, ecf!);
            if (execucao.Leiaute is { Situacao: SituacaoDaIdentificacao.Identificado, Leiaute: { } leiaute } && !ReconhecedorDeEcf.CortadoPelaSofia(ecf!.Texto))
            {
                ValidarEstrutura(execucao, ecf, leiaute);
                if (consultaAoManual is not null && execucao.Etapas.Any(etapa => etapa.Etapa == Etapa.ValidacaoEstrutural && etapa.Estado == EstadoDaEtapa.Concluida))
                {
                    await consultaAoManual.ExecutarAsync(execucao, ManualDaVersao.DaEcf(leiaute), correlacao, cancelamento);
                }
            }

            sintese = Sintese(execucao);
        }

        execucao.Concluir(relogio.GetUtcNow());
        return MontadorDoEnvelope.Montar(execucao, correlacao, ambiente.VersaoDoServico, sintese, erro, pedido.Canal);
    }

    /// <summary>Etapa 2, na ordem de D2: tamanho total, ECF entre os documentos, quantidade de ECF e limites de linha.</summary>
    private (ErroDoEnvelope? Erro, DocumentoDoPedido? Ecf) ValidarEntrada(IReadOnlyList<DocumentoDoPedido> documentos)
    {
        var tamanho = documentos.Sum(documento => (long)System.Text.Encoding.UTF8.GetByteCount(documento.Texto));
        if (tamanho > ambiente.TamanhoMaximoDosDocumentosEmBytes)
        {
            return (ErroDeTamanho(ambiente.TamanhoMaximoDosDocumentosEmBytes), null);
        }

        var ecfs = documentos.Where(documento => ReconhecedorDeEcf.EhEcf(documento.Texto)).ToList();
        if (ecfs.Count == 0)
        {
            return (new ErroDoEnvelope(
                "nenhuma_ecf",
                $"Formato não aceito: nenhum documento é uma ECF. A primeira linha de uma ECF começa com {ReconhecedorDeEcf.Abertura}."), null);
        }

        if (ecfs.Count > 1)
        {
            return (new ErroDoEnvelope(
                "mais_de_uma_ecf",
                string.Create(CultureInfo.InvariantCulture, $"Envie uma ECF por pedido; este pedido tem {ecfs.Count}.")), null);
        }

        return (ValidarLinhas(ecfs[0].Texto), ecfs[0]);
    }

    /// <summary>Limites de linha contra esgotamento de recursos (REQ-RECEPCAO-004; SQ-05).</summary>
    private ErroDoEnvelope? ValidarLinhas(string texto)
    {
        var linhas = 0;
        foreach (var linha in texto.AsSpan().EnumerateLines())
        {
            linhas++;
            if (linhas > ambiente.QuantidadeMaximaDeLinhas)
            {
                return new ErroDoEnvelope(
                    "linhas_acima_do_limite",
                    string.Create(CultureInfo.InvariantCulture, $"Linhas acima do limite: a ECF passa de {ambiente.QuantidadeMaximaDeLinhas} linhas; nada foi analisado."));
            }

            if (linha.Length > ambiente.TamanhoMaximoDaLinhaEmCaracteres)
            {
                return new ErroDoEnvelope(
                    "linha_acima_do_limite",
                    string.Create(CultureInfo.InvariantCulture, $"Linha acima do limite: a linha {linhas} passa de {ambiente.TamanhoMaximoDaLinhaEmCaracteres} caracteres; nada foi analisado."));
            }
        }

        return null;
    }

    /// <summary>Etapa 3 (REQ-LEIAUTE-001 a 004) e o documento cortado pela Sofia (REQ-RECEPCAO-007; D6).</summary>
    private void Identificar(Execucao execucao, DocumentoDoPedido ecf)
    {
        IdentificacaoDoLeiaute identificacao;
        try
        {
            identificacao = identificar(ReconhecedorDeEcf.PrimeiraLinha(ecf.Texto));
        }
        catch (Exception erro) when (erro is not OutOfMemoryException)
        {
            // Defeito do sistema, não do documento: falha técnica, sem validação dependente de versão (REQ-LEIAUTE-003; D7).
            execucao.FalharEtapa(Etapa.IdentificacaoDoLeiaute, CausaDaFalha.Tecnica);
            execucao.AdicionarAchado(Achado.Criar(
                "ach-falha-identificacao",
                CategoriaDoAchado.FalhaTecnica,
                OrigemDoAchado.Sistema,
                "REQ-LEIAUTE-003",
                "A identificação do leiaute falhou por erro interno; nenhuma validação dependente de versão foi executada.",
                SeveridadeDoAchado.Media));
            execucao.AdicionarLimitacao("leiaute não identificado por falha técnica");
            return;
        }

        execucao.RegistrarLeiaute(identificacao);
        switch (identificacao.Situacao)
        {
            case SituacaoDaIdentificacao.Identificado:
                execucao.ConcluirEtapa(Etapa.IdentificacaoDoLeiaute);
                execucao.AdicionarObservacao(string.Create(
                    CultureInfo.InvariantCulture,
                    $"Leiaute identificado no documento {ecf.IdDocumento}: {identificacao.Leiaute} (COD_VER {identificacao.CodVer}, DT_FIN em {identificacao.AnoDoPeriodo})."));
                break;
            case SituacaoDaIdentificacao.VersaoNaoSuportada:
                // A versão fica registrada; as validações que dependem dela não rodam (REQ-LEIAUTE-002).
                execucao.ConcluirEtapa(Etapa.IdentificacaoDoLeiaute);
                execucao.AdicionarLimitacao($"versão do leiaute não suportada: COD_VER {identificacao.CodVer}");
                break;
            default:
                execucao.FalharEtapa(Etapa.IdentificacaoDoLeiaute, CausaDaFalha.Documento);
                execucao.AdicionarLimitacao($"leiaute não identificado: {identificacao.Motivo}");
                break;
        }

        if (ReconhecedorDeEcf.CortadoPelaSofia(ecf.Texto))
        {
            execucao.AdicionarLimitacao(LimitacaoDocumentoCortado);
        }
    }

    /// <summary>Etapa 4 (mudança 08): o catálogo do leiaute, ou a limitação para os leiautes sem catálogo (D7).</summary>
    private void ValidarEstrutura(Execucao execucao, DocumentoDoPedido ecf, int leiaute)
    {
        if (catalogos?.GetValueOrDefault(leiaute) is not { } catalogo)
        {
            execucao.AdicionarLimitacao(string.Create(CultureInfo.InvariantCulture, $"catálogo de regras do leiaute {leiaute} ainda não cadastrado"));
            return;
        }

        var resultado = new ValidadorEstrutural(catalogo, validacao ?? new ParametrosDaValidacao()).Validar(ecf.Texto, ecf.IdDocumento);
        execucao.RegistrarValidadores(catalogo.VersaoDoCatalogo);
        foreach (var achado in resultado.Achados)
        {
            execucao.AdicionarAchado(achado);
        }

        foreach (var (regra, quantidade) in resultado.NaoListadosPorRegra.OrderBy(par => par.Key, StringComparer.Ordinal))
        {
            execucao.AdicionarLimitacao(string.Create(CultureInfo.InvariantCulture, $"{quantidade} achado(s) da regra {regra} não listados, pelo limite da lista"));
        }

        foreach (var indicio in resultado.Indicios)
        {
            execucao.AdicionarObservacao(indicio);
        }

        foreach (var naoRealizada in resultado.NaoRealizadas)
        {
            execucao.AdicionarNaoRealizada(naoRealizada);
        }

        if (resultado.RegrasComFalha.Count == 0)
        {
            execucao.ConcluirEtapa(Etapa.ValidacaoEstrutural);
            return;
        }

        // Falha de uma regra é do sistema, não do documento (REQ-ESTRUT-006; D8).
        execucao.FalharEtapa(Etapa.ValidacaoEstrutural, CausaDaFalha.Tecnica);
        foreach (var regra in resultado.RegrasComFalha)
        {
            execucao.AdicionarAchado(Achado.Criar(
                $"ach-falha-{regra}",
                CategoriaDoAchado.FalhaTecnica,
                OrigemDoAchado.Sistema,
                regra,
                $"A regra {regra} falhou por erro interno e não foi verificada; nenhum erro foi atribuído ao documento por essa falha.",
                SeveridadeDoAchado.Media));
            execucao.AdicionarLimitacao($"regra {regra} não verificada por falha técnica");
        }
    }

    private static string Sintese(Execucao execucao) => execucao.Leiaute switch
    {
        { Situacao: SituacaoDaIdentificacao.Identificado } leiaute when execucao.VersaoDosValidadores is not null => string.Create(
            CultureInfo.InvariantCulture,
            $"ECF do leiaute {leiaute.Leiaute} recebida e validada na estrutura: {execucao.Achados.Count(achado => achado.Deterministico)} achado(s) estrutural(is). As regras de conteúdo e a análise ainda não existem, então nenhuma conformidade é declarada."),
        { Situacao: SituacaoDaIdentificacao.Identificado } leiaute => string.Create(
            CultureInfo.InvariantCulture,
            $"ECF do leiaute {leiaute.Leiaute} recebida. A validação estrutural não rodou para ela: nenhum achado foi produzido e nenhuma conformidade é declarada."),
        _ => "ECF recebida, mas o leiaute não pôde ser identificado ou não é suportado: nenhuma validação dependente de versão foi executada.",
    };

    public static ErroDoEnvelope ErroDeTamanho(long limite) => new(
        "documentos_acima_do_limite",
        string.Create(CultureInfo.InvariantCulture, $"O texto dos documentos ultrapassa o limite de {limite} bytes; nada foi analisado."));
}
