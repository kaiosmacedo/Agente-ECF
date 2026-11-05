using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Aplicacao.Execucoes;

/// <summary>
/// Envelope versionado do resultado, preenchido só pelo sistema (REQ-RESULT-001, GD-02). É o que fica guardado
/// e o que <c>GET /analises/{id}</c> devolve; <c>ResultadoAnaliseSofia</c> é uma projeção dele.
/// </summary>
internal sealed record EnvelopeDoResultado(
    string VersaoContrato,
    Guid IdExecucao,
    Guid IdExecucaoSofia,
    Guid IdSolicitacao,
    string? IdCorrelacao,
    string Status,
    bool RevisaoHumana,
    string Sintese,
    IReadOnlyList<EtapaDoEnvelope> Etapas,
    IReadOnlyList<AchadoDoEnvelope> Achados,
    IReadOnlyList<string> Limitacoes,
    IReadOnlyList<DocumentoRecebido> Documentos,
    IdentidadeDoEnvelope Identidade,
    VersoesDoEnvelope Versoes,
    DateTimeOffset CriadaEm,
    DateTimeOffset? ConcluidaEm,
    double DuracaoMs,
    ErroDoEnvelope? Erro);

internal sealed record EtapaDoEnvelope(int Numero, string Nome, string Estado, string? Justificativa);

internal sealed record AchadoDoEnvelope(
    string IdAchado,
    string Categoria,
    string Origem,
    string Fundamento,
    string Descricao,
    IReadOnlyList<RegistroEnvolvido> Registros,
    IReadOnlyList<string> Evidencias,
    string Severidade);

/// <summary>Identidade como veio declarada: nesta versão, nunca é verificada (SQ-12).</summary>
internal sealed record IdentidadeDoEnvelope(string UsuarioPseudonimo, string Organizacao, bool Verificada);

/// <summary>Versões usadas na execução, para explicar o resultado depois (P19, REQ-EXEC-006).</summary>
internal sealed record VersoesDoEnvelope(string Contrato, string Servico, string Validadores);

internal sealed record ErroDoEnvelope(string Codigo, string Mensagem);

/// <summary>Monta o envelope a partir da execução, com os nomes públicos de status, categorias e severidades.</summary>
internal static class MontadorDoEnvelope
{
    public const string VersaoDoContrato = "1.0";

    /// <summary>Nenhum validador existe ainda: entram nas mudanças 08 e 09.</summary>
    private const string VersaoDosValidadores = "nenhum";

    private static readonly Dictionary<Etapa, string> NomeDaEtapa = new()
    {
        [Etapa.Recepcao] = "Recepção",
        [Etapa.ValidacaoDaEntrada] = "Validação da entrada",
        [Etapa.IdentificacaoDoLeiaute] = "Identificação do leiaute",
        [Etapa.ValidacaoEstrutural] = "Validação estrutural",
        [Etapa.ConsultaAoManual] = "Consulta ao manual",
        [Etapa.FerramentasExternas] = "Ferramentas externas",
        [Etapa.AnaliseComLlm] = "Análise com LLM",
        [Etapa.ValidacaoDaSaida] = "Validação da saída",
        [Etapa.RetornoDoResultado] = "Retorno do resultado",
    };

    public static EnvelopeDoResultado Montar(
        Execucao execucao, string? correlacao, string versaoDoServico, string sintese, ErroDoEnvelope? erro) =>
        new(
            VersaoDoContrato,
            execucao.Id,
            execucao.IdExecucaoSofia,
            execucao.IdSolicitacao,
            correlacao,
            NomeDoStatus(execucao.Status),
            execucao.ExigeRevisaoHumana,
            sintese,
            [.. execucao.Etapas.Select(etapa => new EtapaDoEnvelope((int)etapa.Etapa, NomeDaEtapa[etapa.Etapa], NomeDoEstado(etapa.Estado), etapa.Justificativa))],
            [.. execucao.Achados.Select(Achado)],
            [.. ValidacoesNaoRealizadas(execucao), .. execucao.Limitacoes],
            execucao.Documentos,
            new IdentidadeDoEnvelope(execucao.Identidade.UsuarioPseudonimo, execucao.Identidade.Organizacao, Verificada: false),
            new VersoesDoEnvelope(VersaoDoContrato, versaoDoServico, VersaoDosValidadores),
            execucao.CriadaEm,
            execucao.ConcluidaEm,
            execucao.ConcluidaEm is { } fim ? (fim - execucao.CriadaEm).TotalMilliseconds : 0,
            erro);

    public static string NomeDoStatus(StatusDaExecucao status) => status switch
    {
        StatusDaExecucao.EntradaRejeitada => "entrada-rejeitada",
        StatusDaExecucao.Concluida => "concluida",
        StatusDaExecucao.Parcial => "parcial",
        StatusDaExecucao.FalhaTecnica => "falha-tecnica",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    /// <summary>Cada validação obrigatória não realizada, com o motivo (REQ-RESULT-006).</summary>
    private static IEnumerable<string> ValidacoesNaoRealizadas(Execucao execucao)
    {
        var etapas = execucao.Etapas.ToDictionary(etapa => etapa.Etapa);
        var falhou = execucao.Etapas.Any(etapa => etapa.Estado == EstadoDaEtapa.ComFalha);
        foreach (var obrigatoria in FluxoDeEtapas.Obrigatorias.Where(etapa => etapas[etapa].Estado == EstadoDaEtapa.NaoExecutada))
        {
            var motivo = falhou ? "uma etapa anterior falhou" : "ainda não implementada nesta versão";
            yield return $"Etapa não realizada: {NomeDaEtapa[obrigatoria]}. Motivo: {motivo}.";
        }
    }

    private static string NomeDoEstado(EstadoDaEtapa estado) => estado switch
    {
        EstadoDaEtapa.NaoExecutada => "nao-executada",
        EstadoDaEtapa.Concluida => "concluida",
        EstadoDaEtapa.NaoAplicavel => "nao-aplicavel",
        EstadoDaEtapa.ComFalha => "com-falha",
        _ => throw new ArgumentOutOfRangeException(nameof(estado)),
    };

    private static AchadoDoEnvelope Achado(Achado achado) => new(
        achado.IdAchado,
        achado.Categoria switch
        {
            CategoriaDoAchado.ErroEstrutural => "erro-estrutural",
            CategoriaDoAchado.InconsistenciaDeNegocio => "inconsistencia-de-negocio",
            CategoriaDoAchado.IndicioDaIa => "indicio-da-ia",
            CategoriaDoAchado.AusenciaDeEvidencias => "ausencia-de-evidencias",
            CategoriaDoAchado.FalhaTecnica => "falha-tecnica",
            _ => throw new ArgumentOutOfRangeException(nameof(achado)),
        },
        achado.Origem switch
        {
            OrigemDoAchado.Validador => "validador",
            OrigemDoAchado.Ferramenta => "ferramenta",
            OrigemDoAchado.Llm => "llm",
            OrigemDoAchado.Sistema => "sistema",
            _ => throw new ArgumentOutOfRangeException(nameof(achado)),
        },
        achado.Fundamento,
        achado.Descricao,
        achado.Registros,
        achado.Evidencias,
        achado.Severidade switch
        {
            SeveridadeDoAchado.Baixa => "baixa",
            SeveridadeDoAchado.Media => "media",
            SeveridadeDoAchado.Alta => "alta",
            SeveridadeDoAchado.Critica => "critica",
            _ => throw new ExcecaoDeDominio("Severidade fora da escala."),
        });
}
