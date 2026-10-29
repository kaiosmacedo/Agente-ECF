using Api.RAG.Infraestrutura.Observabilidade;
using Api.RAG.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Api.RAG.Tests;

public sealed class LogsTests
{
    [Fact(DisplayName = "REQ-OBSERV-002 Logs da requisição têm correlação, nome e versão do serviço")]
    public async Task LogsDaRequisicaoComCamposObrigatorios()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri("/health/live", UriKind.Relative));
        requisicao.Headers.Add(CorrelacaoMiddleware.Cabecalho, "teste-log-1");

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        var daRequisicao = fabrica.Logs.Registros
            .Where(r => r.Escopos.TryGetValue("RequestPath", out var caminho) && caminho == "/health/live")
            .ToList();
        Assert.Contains(daRequisicao, r => r.Categoria == typeof(CorrelacaoMiddleware).FullName);
        Assert.All(daRequisicao, registro =>
        {
            Assert.Equal("teste-log-1", registro.Escopos["CorrelationId"]);
            Assert.Equal("Api.RAG", registro.Escopos["Servico"]);
            Assert.False(string.IsNullOrWhiteSpace(registro.Escopos["Versao"]));
        });
    }

    [Fact(DisplayName = "REQ-OBSERV-002 Saída de logs configurada em JSON com escopos")]
    public void SaidaEmJsonComEscopos()
    {
        using var fabrica = new FabricaApi();

        var console = fabrica.Services.GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>().CurrentValue;
        var json = fabrica.Services.GetRequiredService<IOptionsMonitor<JsonConsoleFormatterOptions>>().CurrentValue;

        Assert.Equal(ConsoleFormatterNames.Json, console.FormatterName);
        Assert.True(json.IncludeScopes);
    }

    [Fact(DisplayName = "REQ-OBSERV-003 Cabeçalho Authorization não aparece nos logs")]
    public async Task AuthorizationNaoRegistrado()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri("/health/live", UriKind.Relative));
        requisicao.Headers.TryAddWithoutValidation("Authorization", "Bearer token-secreto-123");
        requisicao.Headers.TryAddWithoutValidation("Cookie", "sessao=cookie-secreto-456");

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.NotEmpty(fabrica.Logs.Registros);
        Assert.DoesNotContain(fabrica.Logs.Registros, r => r.TextoCompleto.Contains("token-secreto-123", StringComparison.Ordinal));
        Assert.DoesNotContain(fabrica.Logs.Registros, r => r.TextoCompleto.Contains("cookie-secreto-456", StringComparison.Ordinal));
    }
}
