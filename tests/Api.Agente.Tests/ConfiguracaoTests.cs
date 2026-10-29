using System.ComponentModel.DataAnnotations;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Agente.Tests;

public sealed class ConfiguracaoTests
{
    [Fact(DisplayName = "REQ-CONFIG-002 Opção obrigatória ausente impede a inicialização e cita o nome")]
    public void OpcaoObrigatoriaAusente()
    {
        using var fabrica = ComOpcaoDeExemplo(valor: null);

        var mensagem = MensagemCompleta(Assert.ThrowsAny<Exception>(() => fabrica.CreateClient()));

        Assert.Contains("Exemplo:Endereco", mensagem, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Opção inválida impede a inicialização sem revelar o valor")]
    public void OpcaoInvalidaSemRevelarValor()
    {
        using var fabrica = ComOpcaoDeExemplo(valor: "segredo-nao-url");

        var mensagem = MensagemCompleta(Assert.ThrowsAny<Exception>(() => fabrica.CreateClient()));

        Assert.Contains("Exemplo:Endereco", mensagem, StringComparison.Ordinal);
        Assert.DoesNotContain("segredo-nao-url", mensagem, StringComparison.Ordinal);
        Assert.DoesNotContain(fabrica.Logs.Registros, r => r.TextoCompleto.Contains("segredo-nao-url", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Nível de log fora da lista impede a inicialização")]
    public void NivelDeLogInvalido()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseSetting("Observabilidade:NivelMinimo", "Verboso"));

        var mensagem = MensagemCompleta(Assert.ThrowsAny<Exception>(() => fabrica.CreateClient()));

        Assert.Contains("Observabilidade:NivelMinimo", mensagem, StringComparison.Ordinal);
        Assert.DoesNotContain("Verboso", mensagem, StringComparison.Ordinal);
    }

    private static FabricaApi ComOpcaoDeExemplo(string? valor) => new FabricaApi().Com(builder =>
    {
        if (valor is not null)
        {
            builder.UseSetting("Exemplo:Endereco", valor);
        }

        builder.ConfigureServices((contexto, servicos) =>
            servicos.AdicionarOpcoesValidadas<OpcoesDeExemplo>(contexto.Configuration, "Exemplo"));
    });

    private static string MensagemCompleta(Exception erro) => erro.ToString();

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
