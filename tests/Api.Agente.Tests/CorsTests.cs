using System.Net;
using System.Text.Json;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Cors;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Hosting;

namespace Api.Agente.Tests;

public sealed class CorsTests
{
    private const string OrigemAutorizada = "http://localhost:4200";

    private static FabricaApi ComOrigemAutorizada() =>
        new FabricaApi().Com(builder => builder.UseSetting("Cors:AllowedOrigins:0", OrigemAutorizada));

    private static HttpRequestMessage Preflight(string caminho, string origem)
    {
        var requisicao = new HttpRequestMessage(HttpMethod.Options, new Uri(caminho, UriKind.Relative));
        requisicao.Headers.Add("Origin", origem);
        requisicao.Headers.Add("Access-Control-Request-Method", "POST");
        requisicao.Headers.Add("Access-Control-Request-Headers", "content-type,a2a-version");
        return requisicao;
    }

    [Theory(DisplayName = "REQ-CORS-001 Preflight de origem autorizada recebe a permissão")]
    [InlineData("/a2a")]
    [InlineData("/.well-known/agent-card.json")]
    public async Task PreflightAutorizado(string caminho)
    {
        using var fabrica = ComOrigemAutorizada();
        using var requisicao = Preflight(caminho, OrigemAutorizada);

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal(OrigemAutorizada, Assert.Single(resposta.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Contains("POST", string.Join(',', resposta.Headers.GetValues("Access-Control-Allow-Methods")), StringComparison.Ordinal);
        Assert.Contains("content-type", string.Join(',', resposta.Headers.GetValues("Access-Control-Allow-Headers")), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("a2a-version", string.Join(',', resposta.Headers.GetValues("Access-Control-Allow-Headers")), StringComparison.OrdinalIgnoreCase);
        Assert.False(resposta.Headers.Contains("Access-Control-Allow-Credentials"));
    }

    [Fact(DisplayName = "REQ-CORS-001 Com várias origens, a resposta varia por Origin")]
    public async Task VariasOrigens()
    {
        using var fabrica = ComOrigemAutorizada().Com(builder => builder.UseSetting("Cors:AllowedOrigins:1", "https://sofia.exemplo.teste"));
        using var requisicao = Preflight("/a2a", "https://sofia.exemplo.teste");

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal("https://sofia.exemplo.teste", Assert.Single(resposta.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Contains("Origin", resposta.Headers.Vary);
    }

    [Fact(DisplayName = "REQ-CORS-001 Preflight de origem não autorizada não recebe permissão")]
    public async Task PreflightNaoAutorizado()
    {
        using var fabrica = ComOrigemAutorizada();
        using var requisicao = Preflight("/a2a", "https://site-nao-autorizado.example");

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.False(resposta.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact(DisplayName = "REQ-CORS-001 Chamada real de origem autorizada recebe a permissão na resposta")]
    public async Task ChamadaDeOrigemAutorizada()
    {
        using var fabrica = ComOrigemAutorizada();
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri("/.well-known/agent-card.json", UriKind.Relative));
        requisicao.Headers.Add("Origin", OrigemAutorizada);

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(OrigemAutorizada, Assert.Single(resposta.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Fact(DisplayName = "REQ-CORS-002 Servidor sem Origin é atendido mesmo sem origens configuradas")]
    public async Task ServidorSemOrigin()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));

        Assert.Equal("TASK_STATE_COMPLETED", resposta.RootElement.GetProperty("result").GetProperty("task").GetProperty("status").GetProperty("state").GetString());
    }

    [Fact(DisplayName = "REQ-CORS-002 Sem origens configuradas, nenhum navegador recebe permissão")]
    public async Task SemOrigensConfiguradas()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = Preflight("/a2a", OrigemAutorizada);

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.False(resposta.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact(DisplayName = "REQ-CORS-002 Verificação de saúde fica fora do escopo da política")]
    public async Task SaudeForaDoEscopo()
    {
        using var fabrica = ComOrigemAutorizada();
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri("/health/live", UriKind.Relative));
        requisicao.Headers.Add("Origin", OrigemAutorizada);

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.False(resposta.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Theory(DisplayName = "REQ-CORS-003 Origem inválida impede a inicialização, cita a posição e não revela o valor")]
    [InlineData("*")]
    [InlineData("https://*.exemplo.teste")]
    [InlineData("https://exemplo.teste/")]
    [InlineData("https://exemplo.teste/caminho")]
    [InlineData("ftp://exemplo.teste")]
    [InlineData("exemplo.teste")]
    public async Task OrigemInvalida(string origem)
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesCors>(configuracao, OpcoesCors.Secao),
            [new("Cors:AllowedOrigins:0", origem)]);

        Assert.Contains("Cors:AllowedOrigins:0", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(origem, erro.Message, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "REQ-CORS-003 Origem explícita, com ou sem porta, é aceita")]
    [InlineData("http://localhost:4200")]
    [InlineData("https://sofia.exemplo.teste")]
    public async Task OrigemValida(string origem)
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseSetting("Cors:AllowedOrigins:0", origem));

        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/health/live", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }
}
