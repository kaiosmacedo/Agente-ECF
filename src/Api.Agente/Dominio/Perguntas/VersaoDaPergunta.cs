using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Api.Agente.Dominio.Perguntas;

/// <summary>Como a versão do manual foi escolhida para a pergunta (REQ-PERGUNTA-002; mudança 17, D3).</summary>
internal enum OrigemDaVersao
{
    Padrao,
    Citada,
    MaisRecenteDasCitadas,
    Informada,
    ForaDaBase,
}

/// <summary>Versão a consultar: <see cref="Versao"/> é nula quando a pergunta cita uma versão que a base não tem.</summary>
internal sealed record VersaoDaPergunta(string? Versao, OrigemDaVersao Origem, string? Citacao);

/// <summary>
/// Resolve a versão do manual pelo texto da pergunta: leiaute ou ano-calendário citados, sem diferenciar maiúsculas nem
/// acentos. Uma consulta por pergunta, sem misturar versões (P11).
/// </summary>
internal static partial class ResolvedorDeVersao
{
    public const string VersaoPadrao = "leiaute-12";
    public const int PrimeiroLeiaute = 7;
    public const int UltimoLeiaute = 12;

    /// <summary>
    /// Vigência de cada manual na base: o ano-calendário que ele cobre, como em <c>docs/fontes/manifesto-rag.json</c>
    /// (mudança 05). Um teste confere esta tabela contra o manifesto.
    /// </summary>
    public static IReadOnlyDictionary<int, int> LeiautePorAnoCalendario { get; } = new Dictionary<int, int>
    {
        [2020] = 7,
        [2021] = 8,
        [2022] = 9,
        [2023] = 10,
        [2024] = 11,
        [2025] = 12,
    };

    public static string NomeDaVersao(int leiaute) => string.Create(CultureInfo.InvariantCulture, $"leiaute-{leiaute}");

    /// <summary>Versão informada à parte, como o campo <c>versaoDoManual</c> do REST, no formato <c>leiaute-N</c>.</summary>
    public static VersaoDaPergunta? Informada(string? versao)
    {
        if (string.IsNullOrWhiteSpace(versao))
        {
            return null;
        }

        var achado = VersaoInformada().Match(versao.Trim());
        if (!achado.Success)
        {
            return new VersaoDaPergunta(null, OrigemDaVersao.ForaDaBase, versao.Trim());
        }

        var leiaute = int.Parse(achado.Groups[1].Value, CultureInfo.InvariantCulture);
        return leiaute is >= PrimeiroLeiaute and <= UltimoLeiaute
            ? new VersaoDaPergunta(NomeDaVersao(leiaute), OrigemDaVersao.Informada, versao.Trim())
            : new VersaoDaPergunta(null, OrigemDaVersao.ForaDaBase, versao.Trim());
    }

    public static VersaoDaPergunta Resolver(string pergunta)
    {
        var texto = SemAcentos(pergunta);
        var citados = new List<(int Leiaute, string Citacao)>();
        foreach (Match achado in Leiaute().Matches(texto))
        {
            citados.Add((int.Parse(achado.Groups[1].Value, CultureInfo.InvariantCulture), achado.Value));
        }

        foreach (Match achado in AnoCalendario().Matches(texto))
        {
            var ano = int.Parse(achado.Groups[1].Value, CultureInfo.InvariantCulture);
            citados.Add((LeiautePorAnoCalendario.TryGetValue(ano, out var leiaute) ? leiaute : 0, achado.Value));
        }

        if (citados.Count == 0)
        {
            return new VersaoDaPergunta(VersaoPadrao, OrigemDaVersao.Padrao, null);
        }

        var fora = citados.FirstOrDefault(citado => citado.Leiaute is < PrimeiroLeiaute or > UltimoLeiaute);
        if (fora.Citacao is not null)
        {
            return new VersaoDaPergunta(null, OrigemDaVersao.ForaDaBase, fora.Citacao);
        }

        var distintos = citados.Select(citado => citado.Leiaute).Distinct().ToList();
        var maisRecente = citados.MaxBy(citado => citado.Leiaute);
        return new VersaoDaPergunta(
            NomeDaVersao(maisRecente.Leiaute),
            distintos.Count > 1 ? OrigemDaVersao.MaisRecenteDasCitadas : OrigemDaVersao.Citada,
            maisRecente.Citacao);
    }

    private static string SemAcentos(string texto)
    {
        var decomposto = texto.Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(decomposto.Length);
        foreach (var caractere in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(caractere);
            }
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"\b(?:leiaute|layout)\s*[-–]?\s*(\d{1,2})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Leiaute();

    [GeneratedRegex(@"\b(?:ano[\s-]*calendario|AC)\s*(?:de\s+)?(\d{4})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AnoCalendario();

    [GeneratedRegex(@"^leiaute-(\d{1,2})$", RegexOptions.CultureInvariant)]
    private static partial Regex VersaoInformada();
}

/// <summary>
/// Código literal que a busca procura primeiro (mudança 06, REQ-RECUP-005): <c>REGRA_...</c>, um registro com letra e três
/// dígitos, como <c>M300</c>, ou "registro" seguido de quatro dígitos, como <c>0000</c>. Um ano sozinho nunca é código.
/// </summary>
internal static partial class ExtratorDeCodigo
{
    public static string? Extrair(string pergunta)
    {
        var regra = Regra().Match(pergunta);
        if (regra.Success)
        {
            return regra.Value;
        }

        var registro = Registro().Match(pergunta);
        if (registro.Success)
        {
            return registro.Value;
        }

        var numerico = RegistroNumerico().Match(pergunta);
        return numerico.Success ? numerico.Groups[1].Value : null;
    }

    [GeneratedRegex(@"\bREGRA_[A-Z0-9_]+\b", RegexOptions.CultureInvariant)]
    private static partial Regex Regra();

    [GeneratedRegex(@"\b[A-Z][0-9]{3}\b", RegexOptions.CultureInvariant)]
    private static partial Regex Registro();

    [GeneratedRegex(@"\bregistro\s+([0-9]{4})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RegistroNumerico();
}
