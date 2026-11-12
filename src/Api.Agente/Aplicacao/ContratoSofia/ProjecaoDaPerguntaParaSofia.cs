using Api.Agente.Aplicacao.Perguntas;

namespace Api.Agente.Aplicacao.ContratoSofia;

/// <summary>
/// Projeta a resposta de uma pergunta no contrato SOFIA v1.0, sem campo novo (RA-07; mudança 17, D7): a resposta vai em
/// <c>analise</c>, as fontes e as observações em <c>observacoes</c>, e <c>achados</c> vai vazio.
/// </summary>
internal static class ProjecaoDaPerguntaParaSofia
{
    public static ResultadoAnaliseSofia Projetar(SolicitacaoAnaliseSofia solicitacao, RespostaDaPergunta resposta) => new(
        ContratoSofiaV1.Versao,
        solicitacao.IdExecucao!,
        solicitacao.IdSolicitacao!,
        ContratoSofiaV1.Agente,
        resposta.Status,
        resposta.Resposta,
        [],
        resposta.Observacoes,
        resposta.Erro is { } erro ? new ErroSofia(erro.Codigo, erro.Mensagem) : null,
        resposta.IdPergunta.ToString());
}
