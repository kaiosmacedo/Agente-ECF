using System.Net;
using System.Text.Json;
using Api.Agente.Endpoints;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Agente.Tests;

public sealed class SaudeTests
{
    [Fact(DisplayName = "REQ-SAUDE-001 Vivacidade responde 200 e Healthy sem dependências externas")]
    public async Task VivacidadeRespondeHealthy()
    {
        using var fabrica = new FabricaApi();
        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/health/live", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Healthy", await LerEstado(resposta));
    }

    [Fact(DisplayName = "REQ-SAUDE-002 Prontidão responde 200 sem dependências obrigatórias registradas")]
    public async Task ProntidaoSemDependencias()
    {
        using var fabrica = new FabricaApi();
        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/health/ready", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Healthy", await LerEstado(resposta));
    }

    [Fact(DisplayName = "REQ-SAUDE-002 REQ-SAUDE-003 Dependência indisponível responde 503 sem a mensagem da exceção")]
    public async Task ProntidaoComDependenciaIndisponivel()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.ConfigureTestServices(servicos =>
            servicos.AddHealthChecks().AddCheck(
                "dependencia-obrigatoria",
                () => throw new InvalidOperationException("senha inválida para host db-interno"),
                tags: [SaudeEndpoints.TagProntidao])));

        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/health/ready", UriKind.Relative), TestContext.Current.CancellationToken);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        Assert.Equal("Unhealthy", JsonDocument.Parse(corpo).RootElement.GetProperty("status").GetString());
        Assert.DoesNotContain("senha inválida", corpo, StringComparison.Ordinal);
        Assert.DoesNotContain("db-interno", corpo, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-SAUDE-003 Resposta anônima contém somente o estado geral")]
    public async Task RespostaContemSomenteEstado()
    {
        using var fabrica = new FabricaApi();
        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/health/live", UriKind.Relative), TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var propriedade = Assert.Single(json.RootElement.EnumerateObject());
        Assert.Equal("status", propriedade.Name);
    }

    private static async Task<string?> LerEstado(HttpResponseMessage resposta)
    {
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return json.RootElement.GetProperty("status").GetString();
    }
}
