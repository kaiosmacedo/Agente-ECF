using Api.Agente.Aplicacao.Execucoes;

namespace Api.Agente.Aplicacao.ContratoSofia;

/// <summary>Projeta o envelope no contrato SOFIA v1.0 (RA-07; design da mudança 03, D2 e D4).</summary>
internal static class ProjecaoParaSofia
{
    public static ResultadoAnaliseSofia Projetar(EnvelopeDoResultado envelope) => new(
        ContratoSofiaV1.Versao,
        envelope.IdExecucaoSofia.ToString(),
        envelope.IdSolicitacao.ToString(),
        ContratoSofiaV1.Agente,
        StatusDoContrato(envelope.Status),
        envelope.Sintese,
        [.. envelope.Achados.Select(Achado)],
        [
            .. envelope.Documentos.Select(documento => $"Documento {documento.IdDocumento} recebido: {documento.Caracteres} caracteres, SHA-256 {documento.Hash}."),
            .. envelope.Observacoes ?? [],
            .. envelope.Limitacoes,
        ],
        envelope.Erro is { } erro ? new ErroSofia(erro.Codigo, erro.Mensagem) : null,
        envelope.IdExecucao.ToString(),
        envelope.RevisaoHumana,
        envelope.Veredito,
        envelope.Resumo,
        envelope.NaoVerificados);

    /// <summary>O contrato só admite três status: rejeição e falha técnica viram erro (E01).</summary>
    public static string StatusDoContrato(string statusInterno) => statusInterno switch
    {
        "concluida" => ContratoSofiaV1.StatusConcluido,
        "parcial" => ContratoSofiaV1.StatusParcial,
        _ => ContratoSofiaV1.StatusErro,
    };

    private static AchadoSofia Achado(AchadoDoEnvelope achado) => new(
        achado.IdAchado,
        achado.Descricao,
        [.. achado.Registros.Select(registro => registro.IdDocumento).Distinct(StringComparer.Ordinal)],
        achado.Evidencias,
        achado.Severidade,
        achado.Categoria,
        achado.Origem,
        achado.Fundamento,
        achado.Classe,
        achado.Registros,
        achado.Orientacao,
        achado.Fonte,
        achado.TrechosDoManual);
}
