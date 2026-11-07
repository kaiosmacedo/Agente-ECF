namespace Api.Agente.Aplicacao.Conhecimento;

/// <summary>Como terminou uma consulta ao manual (mudança 06, D7).</summary>
internal enum DesfechoDaConsulta
{
    Trechos,
    BuscaDesativada,
    LimiteDeUsoAtingido,
    VersaoIndisponivel,
    FalhaTecnica,
}

/// <summary>O que a etapa 5 pede à base de conhecimento. Tipo e versão são os identificadores da base, como manual-ecf e leiaute-12.</summary>
internal sealed record PedidoDeTrechos(string Consulta, string TipoDocumento, string Versao, string? Codigo, Guid IdExecucao, string? Correlacao);

/// <summary>Um trecho recebido. O texto é dado não confiável (P10) e só é usado delimitado (<see cref="DelimitadorDeFontes"/>).</summary>
internal sealed record TrechoDoManual(
    Guid IdTrecho, Guid IdDocumento, string Titulo, string Versao, int Pagina, string? Secao, double Relevancia, bool ContemCodigo, string Texto);

/// <summary>Resposta de uma consulta: trechos, ou o motivo de não haver trechos.</summary>
internal sealed record RespostaDaConsulta(DesfechoDaConsulta Desfecho, IReadOnlyList<TrechoDoManual> Trechos)
{
    public static RespostaDaConsulta Sem(DesfechoDaConsulta desfecho) => new(desfecho, []);
}

/// <summary>
/// A consulta à base de conhecimento, que só acontece pela ferramenta MCP do serviço de conhecimento (P15, REQ-CONSULTA-001).
/// Nunca lança por falha da chamada: devolve <see cref="DesfechoDaConsulta.FalhaTecnica"/>.
/// </summary>
internal interface IConsultaAoManual
{
    Task<RespostaDaConsulta> BuscarAsync(PedidoDeTrechos pedido, CancellationToken cancelamento);
}
