using System.ComponentModel.DataAnnotations;
using Api.RAG.Aplicacao.Conhecimento;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Infraestrutura.Configuracao;
using Microsoft.Extensions.Options;

namespace Api.RAG.Infraestrutura.Conhecimento;

/// <summary>
/// Opções da seção <c>Busca</c> (mudança 06, D5). A busca é pública na primeira versão e gera custo no provedor,
/// então fica desligada por padrão e tem limite diário (SQ-12).
/// </summary>
internal sealed class OpcoesDaBusca
{
    public const string Secao = "Busca";

    public bool Habilitada { get; init; }

    [Range(1, 1_000_000)]
    public int LimiteDeConsultasPorDia { get; init; } = 2000;

    [Range(1, 10)]
    public int QuantidadePadrao { get; init; } = 5;

    [Range(1, 10)]
    public int QuantidadeMaxima { get; init; } = 10;
}

/// <summary>Com a busca ligada, a chave da OpenAI é obrigatória na inicialização (D10; RA-09).</summary>
internal sealed class ValidacaoDaChaveDaBusca(IOptions<OpcoesDaOpenAi> openAi) : IValidateOptions<OpcoesDaBusca>
{
    public ValidateOptionsResult Validate(string? name, OpcoesDaBusca options)
    {
        if (options.QuantidadePadrao > options.QuantidadeMaxima)
        {
            return ValidateOptionsResult.Fail("Busca:QuantidadePadrao não pode passar de Busca:QuantidadeMaxima.");
        }

        return options.Habilitada && string.IsNullOrWhiteSpace(openAi.Value.ApiKey)
            ? ValidateOptionsResult.Fail("Busca:Habilitada exige OpenAI:ApiKey configurada.")
            : ValidateOptionsResult.Success;
    }
}

/// <summary>Registro do serviço de busca.</summary>
internal static class RegistroDaBusca
{
    public static IServiceCollection AdicionarBusca(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesDaBusca>(configuracao, OpcoesDaBusca.Secao);
        servicos.AddSingleton<IValidateOptions<OpcoesDaBusca>, ValidacaoDaChaveDaBusca>();
        servicos.AddSingleton(provedor =>
        {
            var opcoes = provedor.GetRequiredService<IOptions<OpcoesDaBusca>>().Value;
            return new ServicoDeBusca(
                provedor.GetRequiredService<IRepositorioDeDocumentos>(),
                provedor.GetRequiredService<IRepositorioDeConsumo>(),
                provedor.GetRequiredService<IClienteDeEmbeddings>(),
                new ParametrosDaBusca(opcoes.Habilitada, opcoes.LimiteDeConsultasPorDia, opcoes.QuantidadePadrao, opcoes.QuantidadeMaxima),
                provedor.GetRequiredService<TimeProvider>());
        });
        return servicos;
    }
}
