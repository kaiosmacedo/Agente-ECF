namespace Api.Agente.Dominio.Consumo;

/// <summary>Tipo da chamada ao provedor de IA, como no serviço de conhecimento (REQ-CONSUMO-001; RA-10).</summary>
internal enum TipoDeChamada
{
    Geracao,
    Retentativa,
}

internal enum StatusDaChamada
{
    Sucesso,
    Falha,
}

/// <summary>De onde veio o valor: informado pelo provedor, estimado pela tabela de preços ou desconhecido, nunca zero.</summary>
internal enum OrigemDoValor
{
    Informado,
    Estimado,
    Desconhecido,
}

/// <summary>Para que serviu a chamada: a pergunta (mudança 17) ou a análise (mudança 10).</summary>
internal enum FinalidadeDaChamada
{
    Pergunta,
    Analise,
}

/// <summary>Uma tentativa de chamada a um provedor de IA. Só metadados: sem texto, prompt ou credencial (REQ-CONSUMO-005).</summary>
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
    FinalidadeDaChamada Finalidade);
