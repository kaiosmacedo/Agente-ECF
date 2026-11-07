using System.Globalization;
using System.Net;

namespace Api.Agente.Aplicacao.Conhecimento;

/// <summary>
/// Delimita o texto de um trecho como conteúdo de fonte, identificado pela referência, antes de qualquer uso pelo modelo
/// (REQ-CONSULTA-004; P10). O texto é escapado, então nada dentro dele fecha a marcação nem vira instrução.
/// </summary>
internal static class DelimitadorDeFontes
{
    public static string Delimitar(TrechoDoManual trecho) => string.Create(
        CultureInfo.InvariantCulture,
        $"<fonte id=\"{trecho.IdTrecho}\" documento=\"{trecho.IdDocumento}\" versao=\"{WebUtility.HtmlEncode(trecho.Versao)}\" pagina=\"{trecho.Pagina}\">{WebUtility.HtmlEncode(trecho.Texto)}</fonte>");
}
