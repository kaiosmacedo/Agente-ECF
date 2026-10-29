using Api.RAG.Infraestrutura.Observabilidade;
using Api.RAG.Tests.Infra;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Api.RAG.Tests;

public sealed class CorrelacaoTests
{
    [Fact(DisplayName = "REQ-OBSERV-001 Identificador recebido e válido é devolvido")]
    public async Task IdentificadorValidoDevolvido()
    {
        using var fabrica = new FabricaApi();

        var devolvido = await EnviarComCorrelacao(fabrica, "sofia-123.abc");

        Assert.Equal("sofia-123.abc", devolvido);
    }

    [Fact(DisplayName = "REQ-OBSERV-001 Identificador ausente é gerado e devolvido")]
    public async Task IdentificadorAusenteGerado()
    {
        using var fabrica = new FabricaApi();

        var devolvido = await EnviarComCorrelacao(fabrica, valor: null);

        Assert.Matches("^[0-9a-f]{32}$", devolvido);
    }

    [Fact(DisplayName = "REQ-OBSERV-001 Identificador com 129 caracteres é substituído e não registrado")]
    public async Task IdentificadorLongoSubstituido()
    {
        using var fabrica = new FabricaApi();
        var longo = new string('a', 129);

        var devolvido = await EnviarComCorrelacao(fabrica, longo);

        Assert.NotEqual(longo, devolvido);
        Assert.Matches("^[0-9a-f]{32}$", devolvido);
        Assert.DoesNotContain(fabrica.Logs.Registros, r => r.TextoCompleto.Contains(longo, StringComparison.Ordinal));
    }

    [Theory(DisplayName = "REQ-OBSERV-001 Identificador com quebra de linha é substituído e não registrado")]
    [InlineData("zzz-sofia\ninjetado")]
    [InlineData("zzz-sofia\n")]
    public async Task IdentificadorComQuebraDeLinhaSubstituido(string recebido)
    {
        var logs = new ProvedorDeLogEmMemoria();
        using var fabricaDeLogs = LoggerFactory.Create(builder => builder.AddProvider(logs));
        var middleware = new CorrelacaoMiddleware(
            _ => Task.CompletedTask,
            fabricaDeLogs.CreateLogger<CorrelacaoMiddleware>(),
            new IdentidadeServico("Api.RAG", "teste"));
        var contexto = new DefaultHttpContext();
        contexto.Request.Headers[CorrelacaoMiddleware.Cabecalho] = recebido;

        await middleware.InvokeAsync(contexto);

        var devolvido = contexto.Response.Headers[CorrelacaoMiddleware.Cabecalho].ToString();
        Assert.Matches("^[0-9a-f]{32}$", devolvido);
        Assert.NotEmpty(logs.Registros);
        Assert.DoesNotContain(logs.Registros, r => r.TextoCompleto.Contains("zzz-sofia", StringComparison.Ordinal));
    }

    private static async Task<string> EnviarComCorrelacao(FabricaApi fabrica, string? valor)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri("/health/live", UriKind.Relative));
        if (valor is not null)
        {
            requisicao.Headers.Add(CorrelacaoMiddleware.Cabecalho, valor);
        }

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
        return Assert.Single(resposta.Headers.GetValues(CorrelacaoMiddleware.Cabecalho));
    }
}
