using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Api.Agente.Aplicacao.Conhecimento;
using Api.Agente.Aplicacao.Consumo;
using Api.Agente.Aplicacao.Perguntas;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Observabilidade;
using Api.Agente.Infraestrutura.Persistencia;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Api.Agente.Infraestrutura.Perguntas;

/// <summary>Opções da seção <c>Perguntas</c> (mudança 17, D4 e D9). Nada aqui é segredo.</summary>
internal sealed class OpcoesDasPerguntas
{
    public const string Secao = "Perguntas";

    /// <summary>Desligada em produção até a chave e a avaliação (REQ-PERGUNTA-010).</summary>
    public bool Habilitada { get; init; }

    [Range(1, 100_000)]
    public int LimitePorDia { get; init; } = 500;

    /// <summary>Vazia até a avaliação definir o limiar (DP-03; E08).</summary>
    [Range(0.0, 1.0)]
    public double? RelevanciaMinima { get; init; }
}

/// <summary>Opções da seção <c>Llm</c>, as mesmas que a mudança 10 vai usar (D5). A chave fica em <c>OpenAI:ApiKey</c>.</summary>
internal sealed class OpcoesDoLlm
{
    public const string Secao = "Llm";

    public bool Habilitada { get; init; }

    [Required]
    public string Modelo { get; init; } = "gpt-4.1-mini";

    [Range(0.0, 2.0)]
    public double Temperatura { get; init; }

    [Range(1, 120)]
    public int TimeoutPorChamadaEmSegundos { get; init; } = 30;

    [Range(0, 5)]
    public int Retentativas { get; init; } = 2;

    [Range(100, 4000)]
    public int MaximoDeTokensDeSaida { get; init; } = 1200;
}

/// <summary>Tabelas de preços da seção <c>Consumo</c>, copiadas do serviço de conhecimento (D10; DP-13).</summary>
internal sealed class OpcoesDeConsumo : IValidatableObject
{
    public const string Secao = "Consumo";

    public List<OpcoesDaTabelaDePrecos> TabelasDePrecos { get; init; } = [];

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

/// <summary>Registro das perguntas, do redator e do consumo (mudança 17).</summary>
internal static class RegistroDasPerguntas
{
    public const string ChaveDaOpenAI = "OpenAI:ApiKey";

    public static IServiceCollection AdicionarPerguntas(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesDasPerguntas>(configuracao, OpcoesDasPerguntas.Secao);
        servicos.AdicionarOpcoesValidadas<OpcoesDoLlm>(configuracao, OpcoesDoLlm.Secao);
        servicos.AdicionarOpcoesValidadas<OpcoesDeConsumo>(configuracao, OpcoesDeConsumo.Secao);
        servicos.AddSingleton(provedor => new CalculadoraDeCusto(provedor.GetRequiredService<IOptions<OpcoesDeConsumo>>().Value.Tabelas()));
        servicos.AddSingleton<IRepositorioDeConsumo, RepositorioDeConsumo>();
        servicos.AddSingleton<RegistradorDeConsumo>();
        servicos.AddSingleton<IRepositorioDePerguntas, RepositorioDePerguntas>();

        // Com o LLM ligado e sem a chave, o serviço não sobe (RA-09; D5). O valor nunca vai para log nem mensagem (SQ-03).
        var llmHabilitado = configuracao.GetValue<bool>($"{OpcoesDoLlm.Secao}:Habilitada");
        if (llmHabilitado)
        {
            var chave = configuracao[ChaveDaOpenAI];
            if (string.IsNullOrWhiteSpace(chave))
            {
                throw new OptionsValidationException(
                    OpcoesDoLlm.Secao, typeof(OpcoesDoLlm), ["Llm:Habilitada=true exige OpenAI:ApiKey, uma variável sensível."]);
            }

            servicos.AddHttpClient(RedatorSemanticKernel.NomeDoClienteHttp);
            servicos.AddSingleton<IChatCompletionService>(provedor => new OpenAIChatCompletionService(
                provedor.GetRequiredService<IOptions<OpcoesDoLlm>>().Value.Modelo,
                chave,
                httpClient: provedor.GetRequiredService<IHttpClientFactory>().CreateClient(RedatorSemanticKernel.NomeDoClienteHttp)));
            servicos.AddSingleton<IRedatorDeRespostas, RedatorSemanticKernel>();
        }

        servicos.AddSingleton(provedor =>
        {
            var perguntas = provedor.GetRequiredService<IOptions<OpcoesDasPerguntas>>().Value;
            var llm = provedor.GetRequiredService<IOptions<OpcoesDoLlm>>().Value;
            var redator = provedor.GetService<IRedatorDeRespostas>();
            return new ServicoDePerguntas(
                new ConfiguracaoDasPerguntas(
                    perguntas.Habilitada,
                    llm.Habilitada && redator is not null,
                    perguntas.LimitePorDia,
                    perguntas.RelevanciaMinima,
                    llm.Retentativas,
                    provedor.GetRequiredService<IdentidadeServico>().Versao),
                provedor.GetRequiredService<IConsultaAoManual>(),
                provedor.GetRequiredService<IRepositorioDePerguntas>(),
                provedor.GetRequiredService<TimeProvider>(),
                redator,
                provedor.GetRequiredService<RegistradorDeConsumo>(),
                provedor.GetRequiredService<CalculadoraDeCusto>());
        });
        return servicos;
    }
}
