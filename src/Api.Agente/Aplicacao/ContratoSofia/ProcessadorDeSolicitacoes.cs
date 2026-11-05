using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Api.Agente.Aplicacao.ContratoSofia;

/// <summary>Desfecho do processamento: um resultado do contrato ou o motivo de não haver como correlacionar um.</summary>
internal sealed record DesfechoDaSolicitacao(ResultadoAnaliseSofia? Resultado, string? MotivoDaRejeicao);

/// <summary>
/// Serviço de aplicação comum aos canais do agente (REQ-A2A-002). O conteúdo recebido é dado não
/// confiável (P10). As etapas da análise chegam nas próximas mudanças; até lá, o status é parcial (P05, P08).
/// </summary>
internal sealed class ProcessadorDeSolicitacoes(long tamanhoMaximoDosDocumentosEmBytes)
{
    private const string ValidacoesNaoRealizadas =
        "Validações não realizadas nesta versão: identificação do leiaute, validação estrutural, regras de negócio, consulta ao manual e análise por IA.";

    public DesfechoDaSolicitacao Processar(string json)
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

        return new DesfechoDaSolicitacao(Analisar(solicitacao), null);
    }

    private ResultadoAnaliseSofia Analisar(SolicitacaoAnaliseSofia solicitacao)
    {
        if (solicitacao.VersaoContrato != ContratoSofiaV1.Versao)
        {
            return Erro(solicitacao, "versao_contrato_nao_suportada", $"Este agente aceita somente a versão {ContratoSofiaV1.Versao} do contrato.");
        }

        var camposInvalidos = ValidadorDaSolicitacao.CamposInvalidos(solicitacao);
        if (camposInvalidos.Count > 0)
        {
            return Erro(solicitacao, "solicitacao_invalida", $"Campos ausentes ou inválidos: {string.Join(", ", camposInvalidos)}.");
        }

        var documentos = solicitacao.Documentos!;
        if (documentos.Sum(documento => (long)Encoding.UTF8.GetByteCount(documento.Texto!)) > tamanhoMaximoDosDocumentosEmBytes)
        {
            return Erro(solicitacao, "documentos_acima_do_limite", $"O texto dos documentos ultrapassa o limite de {tamanhoMaximoDosDocumentosEmBytes} bytes; nada foi analisado.");
        }

        var analise = string.Create(
            CultureInfo.InvariantCulture,
            $"{documentos.Count} documento(s) recebido(s). Esta versão ainda não executa as validações da ECF: nenhum achado foi produzido e nenhuma conformidade é declarada.");
        return Resultado(solicitacao, ContratoSofiaV1.StatusParcial, analise, [.. documentos.Select(Rastrear), ValidacoesNaoRealizadas], erro: null);
    }

    private static string Rastrear(DocumentoSofia documento)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(documento.Texto!)));
        return string.Create(CultureInfo.InvariantCulture, $"Documento {documento.IdDocumento} recebido: {documento.Texto!.Length} caracteres, SHA-256 {hash}.");
    }

    private static ResultadoAnaliseSofia Erro(SolicitacaoAnaliseSofia solicitacao, string codigo, string mensagem) =>
        Resultado(solicitacao, ContratoSofiaV1.StatusErro, $"A solicitação não pôde ser analisada. {mensagem}", [], new ErroSofia(codigo, mensagem));

    private static ResultadoAnaliseSofia Resultado(
        SolicitacaoAnaliseSofia solicitacao, string status, string analise, IReadOnlyList<string> observacoes, ErroSofia? erro) =>
        new(ContratoSofiaV1.Versao, solicitacao.IdExecucao!, solicitacao.IdSolicitacao!, ContratoSofiaV1.Agente, status, analise, [], observacoes, erro);
}
