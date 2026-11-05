using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Infraestrutura.Configuracao;
using Microsoft.Extensions.Options;

namespace Api.RAG.Infraestrutura.Consumo;

/// <summary>Opções da seção <c>Consumo</c>: as tabelas de preços, versionadas, e a política de tentativas (D3, D4).</summary>
internal sealed class OpcoesDeConsumo : IValidatableObject
{
    public const string Secao = "Consumo";

    public List<OpcoesDaTabelaDePrecos> TabelasDePrecos { get; init; } = [];

    [Range(1, 10)]
    public int MaximoDeTentativas { get; init; } = 3;

    [Range(0, 60)]
    public int SegundosEntreTentativas { get; init; } = 2;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var tabela in TabelasDePrecos)
        {
            if (string.IsNullOrWhiteSpace(tabela.Versao) || string.IsNullOrWhiteSpace(tabela.Fonte)
                || !DateOnly.TryParse(tabela.VigenteDesde, CultureInfo.InvariantCulture, out _) || tabela.Modelos.Count == 0)
            {
                yield return new ValidationResult("Cada tabela precisa de versão, vigência, fonte e modelos.", [nameof(TabelasDePrecos)]);
            }
        }
    }

    public IReadOnlyList<TabelaDePrecos> Tabelas() =>
    [
        .. TabelasDePrecos.Select(tabela => new TabelaDePrecos(
            tabela.Versao,
            DateOnly.Parse(tabela.VigenteDesde, CultureInfo.InvariantCulture),
            tabela.Fonte,
            [.. tabela.Modelos.Select(modelo => new PrecoDoModelo(modelo.Modelo, modelo.EntradaPorMilhao, modelo.SaidaPorMilhao))])),
    ];
}

internal sealed class OpcoesDaTabelaDePrecos
{
    public string Versao { get; init; } = string.Empty;

    public string VigenteDesde { get; init; } = string.Empty;

    public string Fonte { get; init; } = string.Empty;

    public List<OpcoesDoPrecoDoModelo> Modelos { get; init; } = [];
}

internal sealed class OpcoesDoPrecoDoModelo
{
    public string Modelo { get; init; } = string.Empty;

    public decimal EntradaPorMilhao { get; init; }

    public decimal? SaidaPorMilhao { get; init; }
}

/// <summary>Registro da medição de consumo. O provedor real de embeddings entra na mudança de ingestão.</summary>
internal static class RegistroDoConsumo
{
    public static IServiceCollection AdicionarConsumo(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesDeConsumo>(configuracao, OpcoesDeConsumo.Secao);
        servicos.AddSingleton(TimeProvider.System);
        servicos.AddSingleton(provedor => new CalculadoraDeCusto(provedor.GetRequiredService<IOptions<OpcoesDeConsumo>>().Value.Tabelas()));
        servicos.AddSingleton(provedor =>
        {
            var opcoes = provedor.GetRequiredService<IOptions<OpcoesDeConsumo>>().Value;
            return new PoliticaDeTentativas(opcoes.MaximoDeTentativas, TimeSpan.FromSeconds(opcoes.SegundosEntreTentativas));
        });
        servicos.AddSingleton<RegistradorDeConsumo>();
        return servicos;
    }
}
