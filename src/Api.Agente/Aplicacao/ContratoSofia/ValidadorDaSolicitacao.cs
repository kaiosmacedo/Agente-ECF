namespace Api.Agente.Aplicacao.ContratoSofia;

/// <summary>Regras de obrigatoriedade do contrato SOFIA v1.0 (REQ-SOFIA-001). Devolve nomes de campos, nunca valores.</summary>
internal static class ValidadorDaSolicitacao
{
    /// <summary>Sem esses identificadores, a Sofia não consegue correlacionar um resultado.</summary>
    public static bool IdentificadoresValidos(SolicitacaoAnaliseSofia solicitacao) =>
        EhUuid(solicitacao.IdExecucao) && EhUuid(solicitacao.IdSolicitacao);

    public static IReadOnlyList<string> CamposInvalidos(SolicitacaoAnaliseSofia solicitacao)
    {
        var campos = new List<string>();
        if (string.IsNullOrWhiteSpace(solicitacao.Usuario?.Nome))
        {
            campos.Add("usuario.nome");
        }

        if (string.IsNullOrWhiteSpace(solicitacao.Pergunta))
        {
            campos.Add("pergunta");
        }

        if (solicitacao.Documentos is not { Count: > 0 } documentos)
        {
            campos.Add("documentos");
            return campos;
        }

        var identificadores = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < documentos.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(documentos[i].IdDocumento) || !identificadores.Add(documentos[i].IdDocumento!))
            {
                campos.Add($"documentos[{i}].idDocumento");
            }

            if (string.IsNullOrWhiteSpace(documentos[i].Nome))
            {
                campos.Add($"documentos[{i}].nome");
            }

            if (string.IsNullOrWhiteSpace(documentos[i].Texto))
            {
                campos.Add($"documentos[{i}].texto");
            }
        }

        return campos;
    }

    private static bool EhUuid(string? valor) => Guid.TryParseExact(valor, "D", out _);
}
