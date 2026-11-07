namespace Api.RAG.Dominio.Consumo;

/// <summary>Tipo da chamada ao provedor (GD-10).</summary>
internal enum TipoDeChamada
{
    Geracao,
    Embedding,
    Retentativa,
    Auxiliar,
}

/// <summary>Resultado de uma chamada ou tentativa.</summary>
internal enum StatusDaChamada
{
    Sucesso,
    Falha,
}

/// <summary>De onde vem um valor (P13, GD-10). Desconhecido nunca vira zero (REQ-CONSUMO-002).</summary>
internal enum OrigemDoValor
{
    Informado,
    Estimado,
    Desconhecido,
    NaoAplicavel,
}

/// <summary>Para que serviu a chamada. A busca conta só as consultas no limite diário (mudança 06, D5).</summary>
internal enum FinalidadeDaChamada
{
    Ingestao,
    Consulta,
    Avaliacao,
}

/// <summary>Uma chamada ou tentativa a um provedor de IA, sem texto, prompt ou credencial (REQ-CONSUMO-001).</summary>
internal sealed record RegistroDeConsumo(
    Guid Id,
    Guid? IdOperacao,
    string? Correlacao,
    string Provedor,
    string Modelo,
    TipoDeChamada Tipo,
    StatusDaChamada Status,
    DateTimeOffset CriadaEm,
    double DuracaoMs,
    int Tentativa,
    int? TokensEntrada,
    OrigemDoValor TokensEntradaOrigem,
    int? TokensSaida,
    OrigemDoValor TokensSaidaOrigem,
    decimal? CustoUsd,
    OrigemDoValor CustoOrigem,
    string? TabelaPrecosVersao,
    FinalidadeDaChamada? Finalidade = null);

/// <summary>Totais de um modelo. Chamadas com valor desconhecido são contadas à parte, nunca como zero.</summary>
internal sealed record TotalDoModelo(
    string Modelo,
    long Chamadas,
    long TokensEntrada,
    long TokensSaida,
    decimal CustoUsd,
    long ChamadasSemTokens,
    long ChamadasSemCusto);
