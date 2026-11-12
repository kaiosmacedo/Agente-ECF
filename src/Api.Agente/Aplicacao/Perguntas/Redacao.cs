using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Api.Agente.Aplicacao.Conhecimento;

namespace Api.Agente.Aplicacao.Perguntas;

/// <summary>Prompt versionado <c>pergunta-v1</c> (P19) e a mensagem com a pergunta e os trechos delimitados (P10).</summary>
internal static class PromptDaPergunta
{
    public const string Versao = "pergunta-v1";

    private const string Recurso = "Api.Agente.Aplicacao.Perguntas.Prompts.pergunta-v1.md";

    /// <summary>Schema estrito da saída (P12): nada além de respondida, resposta e citações.</summary>
    public const string SchemaDaSaida = """
        {
          "type": "object",
          "properties": {
            "respondida": { "type": "boolean" },
            "resposta": { "type": "string" },
            "citacoes": { "type": "array", "items": { "type": "string" } }
          },
          "required": ["respondida", "resposta", "citacoes"],
          "additionalProperties": false
        }
        """;

    public static string Instrucoes { get; } = Ler();

    public static string Mensagem(MaterialDaRedacao material)
    {
        var texto = new StringBuilder();
        texto.Append("<pergunta>").Append(WebUtility.HtmlEncode(material.Pergunta)).AppendLine("</pergunta>");
        texto.Append("Trechos do manual, versão ").Append(WebUtility.HtmlEncode(material.Versao)).AppendLine(":");
        foreach (var trecho in material.Trechos)
        {
            texto.AppendLine(DelimitadorDeFontes.Delimitar(trecho));
        }

        return texto.ToString();
    }

    private static string Ler()
    {
        using var recurso = typeof(PromptDaPergunta).Assembly.GetManifestResourceStream(Recurso)
            ?? throw new InvalidOperationException($"O prompt {Versao} não está no assembly.");
        using var leitor = new StreamReader(recurso, Encoding.UTF8);
        return leitor.ReadToEnd();
    }
}

/// <summary>Resposta aceita: o texto e os trechos citados, já conferidos.</summary>
internal sealed record RedacaoAceita(bool Respondida, string Resposta, IReadOnlyList<TrechoDoManual> Citados);

/// <summary>
/// Validação antes do aceite (P07, P12; mudança 17, D6): schema, tamanho, citações entre os trechos recebidos e frases que
/// declarariam conformidade de um documento. Qualquer falha devolve nulo, e o serviço usa a reserva.
/// </summary>
internal static partial class ValidadorDaRedacao
{
    public const int TamanhoMaximoDaResposta = 3000;

    private const string PrefixoDaCitacao = "trecho:";

    public static RedacaoAceita? Validar(string saida, IReadOnlyList<TrechoDoManual> recebidos)
    {
        JsonElement raiz;
        try
        {
            using var documento = JsonDocument.Parse(saida);
            raiz = documento.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }

        if (raiz.ValueKind != JsonValueKind.Object
            || !raiz.TryGetProperty("respondida", out var respondida) || respondida.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
            || !raiz.TryGetProperty("resposta", out var resposta) || resposta.ValueKind != JsonValueKind.String
            || !raiz.TryGetProperty("citacoes", out var citacoes) || citacoes.ValueKind != JsonValueKind.Array
            || raiz.EnumerateObject().Count() != 3)
        {
            return null;
        }

        if (!respondida.GetBoolean())
        {
            return new RedacaoAceita(false, string.Empty, []);
        }

        var texto = resposta.GetString()!.Trim();
        if (texto.Length is 0 or > TamanhoMaximoDaResposta || DeclaraConformidade(texto))
        {
            return null;
        }

        var porId = recebidos.ToDictionary(trecho => trecho.IdTrecho);
        var citados = new List<TrechoDoManual>();
        foreach (var citacao in citacoes.EnumerateArray())
        {
            if (citacao.ValueKind != JsonValueKind.String
                || citacao.GetString() is not { } valor
                || !valor.StartsWith(PrefixoDaCitacao, StringComparison.Ordinal)
                || !Guid.TryParse(valor[PrefixoDaCitacao.Length..], out var id)
                || !porId.TryGetValue(id, out var trecho))
            {
                return null;
            }

            if (!citados.Contains(trecho))
            {
                citados.Add(trecho);
            }
        }

        return citados.Count == 0 ? null : new RedacaoAceita(true, texto, citados);
    }

    /// <summary>Frases que afirmariam que um documento está conforme (seção 1.3, v2.2.0; P05).</summary>
    public static bool DeclaraConformidade(string texto) => Conformidade().IsMatch(texto);

    [GeneratedRegex(
        @"\b(ecf|arquivo|escritura[çc][ãa]o|declara[çc][ãa]o)\b[^.;:]{0,40}?\b(est[áa]|é|ficou|encontra-se)\s+(conforme|corret[ao]|v[áa]lid[ao]|em\s+conformidade|sem\s+(erros|inconsist[êe]ncias))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Conformidade();
}

/// <summary>Textos montados pelo sistema, sem o modelo (mudança 17, D6).</summary>
internal static class TextosDaPergunta
{
    public const int TrechosNaReserva = 3;
    public const int CaracteresPorTrechoNaReserva = 600;

    public static string NomeDoManual(string versao) =>
        versao.StartsWith("leiaute-", StringComparison.Ordinal)
            ? $"Manual de Orientação do Leiaute {versao["leiaute-".Length..]} da ECF"
            : $"Manual da ECF, versão {versao}";

    public static string ForaDosManuais(string versao) =>
        $"Os trechos do {NomeDoManual(versao)} registrados na base não tratam deste assunto. O agente responde só com o que os manuais oficiais da ECF dizem, sem completar com outras fontes.";

    public const string VersaoForaDaBase =
        "A base de conhecimento tem os manuais oficiais da ECF dos leiautes 7 a 12 (anos-calendário 2020 a 2025). A versão citada na pergunta não está entre eles.";

    public const string ComparacaoNaoAtendida =
        "A pergunta cita mais de uma versão do manual. Comparar versões não é atendido: a resposta usa só a mais recente das citadas.";

    public const string Limitacao =
        "Resposta de apoio, só com trechos do manual oficial; não analisa arquivo nem substitui o profissional contábil ou fiscal.";

    public static string Fonte(TrechoDoManual trecho) => string.Create(
        CultureInfo.InvariantCulture, $"Fonte: {trecho.Titulo}, versão {trecho.Versao}, p. {trecho.Pagina}.");

    /// <summary>Reserva: os trechos mais relevantes, com documento, versão e página, até 3.000 caracteres.</summary>
    public static (string Texto, IReadOnlyList<TrechoDoManual> Usados) Reserva(IReadOnlyList<TrechoDoManual> trechos)
    {
        var usados = trechos.OrderByDescending(trecho => trecho.Relevancia).Take(TrechosNaReserva).ToList();
        var texto = new StringBuilder("Não foi possível redigir uma resposta. Estes são os trechos mais relevantes do manual para a pergunta:");
        var numero = 0;
        foreach (var trecho in usados)
        {
            numero++;
            var conteudo = Compactar(trecho.Texto);
            if (conteudo.Length > CaracteresPorTrechoNaReserva)
            {
                conteudo = string.Concat(conteudo.AsSpan(0, CaracteresPorTrechoNaReserva - 1).TrimEnd(), "…");
            }

            texto.Append(CultureInfo.InvariantCulture, $" ({numero}) {trecho.Titulo}, p. {trecho.Pagina}: \"{conteudo}\"");
        }

        var resultado = texto.ToString();
        return (resultado.Length <= ValidadorDaRedacao.TamanhoMaximoDaResposta ? resultado : resultado[..ValidadorDaRedacao.TamanhoMaximoDaResposta], usados);
    }

    private static string Compactar(string texto) => string.Join(' ', texto.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
