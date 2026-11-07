namespace Api.Agente.Dominio.Execucoes;

/// <summary>As nove etapas obrigatórias do fluxo de análise (P03), na ordem da constitution.</summary>
internal enum Etapa
{
    Recepcao = 1,
    ValidacaoDaEntrada = 2,
    IdentificacaoDoLeiaute = 3,
    ValidacaoEstrutural = 4,
    ConsultaAoManual = 5,
    FerramentasExternas = 6,
    AnaliseComLlm = 7,
    ValidacaoDaSaida = 8,
    RetornoDoResultado = 9,
}

/// <summary>Estado de uma etapa em uma execução (P03).</summary>
internal enum EstadoDaEtapa
{
    NaoExecutada,
    Concluida,
    NaoAplicavel,
    ComFalha,
}

/// <summary>Estado atual de uma etapa e a justificativa, quando não for aplicável.</summary>
internal sealed record EtapaDaExecucao(Etapa Etapa, EstadoDaEtapa Estado, string? Justificativa = null);

/// <summary>Dependências entre as etapas (REQ-EXEC-002) e quais validações são obrigatórias (P05).</summary>
internal static class FluxoDeEtapas
{
    /// <summary>A etapa 5 só consulta o manual para fundamentar achados (REQ-CONSULTA-005).</summary>
    public const string JustificativaSemAchadoAFundamentar = "Nenhum achado a fundamentar no manual.";

    private static readonly Dictionary<Etapa, Etapa[]> Dependentes = new()
    {
        [Etapa.Recepcao] = [Etapa.ValidacaoDaEntrada],
        [Etapa.ValidacaoDaEntrada] = [Etapa.IdentificacaoDoLeiaute],
        [Etapa.IdentificacaoDoLeiaute] = [Etapa.ValidacaoEstrutural, Etapa.ConsultaAoManual, Etapa.FerramentasExternas],
        [Etapa.ValidacaoEstrutural] = [Etapa.AnaliseComLlm],
        [Etapa.ConsultaAoManual] = [Etapa.AnaliseComLlm],
        [Etapa.FerramentasExternas] = [Etapa.AnaliseComLlm],
        [Etapa.AnaliseComLlm] = [Etapa.ValidacaoDaSaida],
    };

    /// <summary>Justificativas previstas para uma etapa não se aplicar (REQ-EXEC-003).</summary>
    private static readonly Dictionary<Etapa, string[]> JustificativasPrevistas = new()
    {
        [Etapa.ConsultaAoManual] = [JustificativaSemAchadoAFundamentar],
        [Etapa.FerramentasExternas] = ["Nenhuma ferramenta externa é necessária nesta análise."],
    };

    /// <summary>Etapas que precisam terminar para o status ser concluído. A etapa 9 é o próprio retorno.</summary>
    public static IReadOnlyList<Etapa> Obrigatorias { get; } =
    [
        Etapa.Recepcao,
        Etapa.ValidacaoDaEntrada,
        Etapa.IdentificacaoDoLeiaute,
        Etapa.ValidacaoEstrutural,
        Etapa.ConsultaAoManual,
        Etapa.FerramentasExternas,
        Etapa.AnaliseComLlm,
        Etapa.ValidacaoDaSaida,
    ];

    public static IEnumerable<Etapa> DependentesDe(Etapa etapa)
    {
        foreach (var dependente in Dependentes.GetValueOrDefault(etapa, []))
        {
            yield return dependente;
            foreach (var indireto in DependentesDe(dependente))
            {
                yield return indireto;
            }
        }
    }

    public static bool JustificativaPrevista(Etapa etapa, string? justificativa) =>
        justificativa is not null && JustificativasPrevistas.GetValueOrDefault(etapa, []).Contains(justificativa, StringComparer.Ordinal);
}
