using System.ComponentModel.DataAnnotations;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Agente.Tests;

public sealed class ConfiguracaoTests
{
    [Fact(DisplayName = "REQ-CONFIG-002 Opção obrigatória ausente impede a inicialização e cita o nome")]
    public async Task OpcaoObrigatoriaAusente()
    {
        var erro = await IniciarHost(ConfigurarExemplo, ("Exemplo:Endereco", null), new ProvedorDeLogEmMemoria());

        Assert.Contains("Exemplo:Endereco", erro.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Opção inválida impede a inicialização sem revelar o valor")]
    public async Task OpcaoInvalidaSemRevelarValor()
    {
        var logs = new ProvedorDeLogEmMemoria();

        var erro = await IniciarHost(ConfigurarExemplo, ("Exemplo:Endereco", "segredo-nao-url"), logs);

        Assert.Contains("Exemplo:Endereco", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("segredo-nao-url", erro.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(logs.Registros, r => r.TextoCompleto.Contains("segredo-nao-url", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Nível de log fora da lista impede a inicialização sem revelar o valor")]
    public async Task NivelDeLogInvalido()
    {
        var erro = await IniciarHost(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesObservabilidade>(configuracao, OpcoesObservabilidade.Secao),
            ("Observabilidade:NivelMinimo", "Verboso"),
            new ProvedorDeLogEmMemoria());

        Assert.Contains("Observabilidade:NivelMinimo", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Verboso", erro.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-CONFIG-002 O serviço não sobe com opção obrigatória inválida")]
    public void ServicoNaoSobeComOpcaoInvalida()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseSetting("Observabilidade:NivelMinimo", "Verboso"));

        Assert.ThrowsAny<Exception>(() => fabrica.CreateClient());
    }

    // Host isolado com o mesmo registro de opções do serviço: o erro de validação chega intacto,
    // sem a corrida de descarte do servidor de testes quando a inicialização falha.
    private static async Task<OptionsValidationException> IniciarHost(
        Action<IServiceCollection, IConfiguration> registrar, (string Chave, string? Valor) opcao, ProvedorDeLogEmMemoria logs)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(logs);
        if (opcao.Valor is not null)
        {
            builder.Configuration.AddInMemoryCollection([new KeyValuePair<string, string?>(opcao.Chave, opcao.Valor)]);
        }

        registrar(builder.Services, builder.Configuration);
        using var host = builder.Build();
        return await Assert.ThrowsAsync<OptionsValidationException>(() => host.StartAsync(TestContext.Current.CancellationToken));
    }

    private static void ConfigurarExemplo(IServiceCollection servicos, IConfiguration configuracao) =>
        servicos.AdicionarOpcoesValidadas<OpcoesDeExemplo>(configuracao, "Exemplo");

    private sealed class OpcoesDeExemplo
    {
        [Required]
        [Url]
        public string? Endereco { get; init; }
    }
}

/// <summary>Altera variável de ambiente do processo; roda isolado dos demais testes.</summary>
[CollectionDefinition(nameof(VariaveisDeAmbiente), DisableParallelization = true)]
public sealed class VariaveisDeAmbiente;

[Collection(nameof(VariaveisDeAmbiente))]
public sealed class ConfiguracaoPorAmbienteTests
{
    [Fact(DisplayName = "REQ-CONFIG-001 Variável de ambiente prevalece sobre o arquivo de configuração")]
    public void VariavelDeAmbientePrevalece()
    {
        Environment.SetEnvironmentVariable("Observabilidade__NivelMinimo", "Warning");
        try
        {
            using var fabrica = new FabricaApi();
            var logger = fabrica.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Api.Agente.Teste");

            Assert.False(logger.IsEnabled(LogLevel.Information));
            Assert.True(logger.IsEnabled(LogLevel.Warning));
        }
        finally
        {
            Environment.SetEnvironmentVariable("Observabilidade__NivelMinimo", null);
        }
    }
}
