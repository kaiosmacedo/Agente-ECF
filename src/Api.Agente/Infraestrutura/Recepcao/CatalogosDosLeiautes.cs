using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Api.Agente.Dominio.Leiaute;

namespace Api.Agente.Infraestrutura.Recepcao;

/// <summary>Lê os catálogos estruturais embutidos no assembly (mudança 08, D1). Hoje, só o do leiaute 12.</summary>
internal static class CatalogosDosLeiautes
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static IReadOnlyDictionary<int, CatalogoDoLeiaute> Ler() => new Dictionary<int, CatalogoDoLeiaute>
    {
        [12] = Ler("ecf-leiaute-12.json"),
    };

    private static CatalogoDoLeiaute Ler(string recurso)
    {
        using var fluxo = typeof(CatalogosDosLeiautes).Assembly.GetManifestResourceStream(recurso)
            ?? throw new InvalidOperationException($"O catálogo {recurso} não está embutido no assembly.");
        var catalogo = JsonSerializer.Deserialize<CatalogoDoLeiaute>(fluxo, Json) ?? throw new InvalidOperationException($"O catálogo {recurso} está vazio.");
        var problemas = catalogo.Problemas().Take(5).ToList();
        return problemas.Count == 0
            ? catalogo
            : throw new InvalidOperationException($"O catálogo {recurso} não pode ser usado: {string.Join("; ", problemas)}.");
    }
}

/// <summary>Opções da seção <c>ValidacaoEstrutural</c>: os limites da lista de achados (mudança 08, D4).</summary>
internal sealed class OpcoesDaValidacaoEstrutural
{
    public const string Secao = "ValidacaoEstrutural";

    [Range(1, 1000)]
    public int MaximoDeAchadosPorRegra { get; init; } = 20;

    [Range(1, 10000)]
    public int MaximoDeAchados { get; init; } = 200;
}
