using System.Net;
using Api.RAG.Infraestrutura.Configuracao;
using Api.RAG.Infraestrutura.Observabilidade;
using Api.RAG.Tests.Infra;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Api.RAG.Tests;

public sealed class TelemetriaTests
{
    // A Api.RAG ainda não tem endpoint funcional: uma rota inexistente também gera trace e métrica.
    private const string RotaSemEndpoint = "/rota-sem-endpoint";

    private static (FabricaApi Fabrica, CapturaDeTelemetria Captura) Iniciar()
    {
        var captura = new CapturaDeTelemetria();
        return (new FabricaApi().Com(captura.Instalar), captura);
    }

    private static async Task<HttpResponseMessage> Chamar(FabricaApi fabrica, string caminho, string traceparent)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri(caminho, UriKind.Relative));
        requisicao.Headers.Add("traceparent", traceparent);
        requisicao.Headers.TryAddWithoutValidation("Authorization", "Bearer token-secreto-123");
        return await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "REQ-OBSERV-004 Requisição gera trace que continua o traceparent recebido")]
    public async Task TraceContinuaOContextoRecebido()
    {
        var (fabrica, captura) = Iniciar();
        using var _ = fabrica;
        var (traceparent, idDoTrace) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await Chamar(fabrica, RotaSemEndpoint, traceparent);

        Assert.NotEmpty(await captura.EsperarTraces(idDoTrace));
    }

    [Fact(DisplayName = "REQ-OBSERV-004 Logs da requisição trazem o mesmo TraceId")]
    public async Task LogsComOMesmoTrace()
    {
        var (fabrica, _) = Iniciar();
        using var __ = fabrica;
        var (traceparent, idDoTrace) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await Chamar(fabrica, RotaSemEndpoint, traceparent);

        Assert.Contains(fabrica.Logs.Registros, registro =>
            registro.Categoria == typeof(CorrelacaoMiddleware).FullName
            && registro.Escopos.TryGetValue("TraceId", out var trace) && trace == idDoTrace);
    }

    [Theory(DisplayName = "REQ-OBSERV-004 Verificação de saúde não gera trace")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task SaudeSemTrace(string caminho)
    {
        var (fabrica, captura) = Iniciar();
        using var _ = fabrica;
        var (traceparent, idDoTrace) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await Chamar(fabrica, caminho, traceparent);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Empty(await captura.TracesAposEspera(idDoTrace));
    }

    [Fact(DisplayName = "REQ-OBSERV-005 Métrica registra a duração e o status")]
    public async Task MetricaPorStatus()
    {
        var (fabrica, captura) = Iniciar();
        using var _ = fabrica;
        var (traceparent, _) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await Chamar(fabrica, "/health/live", traceparent);

        static bool Registrada(IEnumerable<MetricSnapshot> metricas) => metricas
            .Where(metrica => metrica.Name == "http.server.request.duration")
            .SelectMany(metrica => metrica.MetricPoints)
            .Select(Atributos)
            .Any(atributos => atributos.TryGetValue("http.response.status_code", out var status) && status == "200");

        Assert.True(Registrada(await captura.EsperarMetricas(fabrica.Services, Registrada)));
    }

    [Fact(DisplayName = "REQ-OBSERV-006 Endereço inválido impede a inicialização, cita a opção e não revela o valor")]
    public async Task EnderecoInvalido()
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesTelemetria>(configuracao, OpcoesTelemetria.Secao),
            [new("Telemetria:Endpoint", "nao-e-url")]);

        Assert.Contains("Telemetria:Endpoint", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("nao-e-url", erro.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-OBSERV-007 Traces não trazem query string nem credencial")]
    public async Task TelemetriaSemDadosSensiveis()
    {
        var (fabrica, captura) = Iniciar();
        using var _ = fabrica;
        var (traceparent, idDoTrace) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await Chamar(fabrica, $"{RotaSemEndpoint}?chave=segredo-123", traceparent);

        var traces = await captura.EsperarTraces(idDoTrace);
        Assert.NotEmpty(traces);
        foreach (var sensivel in new[] { "segredo-123", "token-secreto-123" })
        {
            Assert.DoesNotContain(traces.SelectMany(atividade => atividade.Tags), tag => tag.Value?.Contains(sensivel, StringComparison.Ordinal) == true);
        }
    }

    private static Dictionary<string, string?> Atributos(MetricPoint ponto)
    {
        var atributos = new Dictionary<string, string?>();
        foreach (var atributo in ponto.Tags)
        {
            atributos[atributo.Key] = atributo.Value?.ToString();
        }

        return atributos;
    }
}

/// <summary>Altera variável de ambiente do processo; roda isolado dos demais testes.</summary>
[Collection(nameof(VariaveisDeAmbiente))]
public sealed class TelemetriaExportadaTests
{
    [Fact(DisplayName = "REQ-OBSERV-006 Com destino configurado, traces e métricas são exportados por OTLP")]
    public async Task ExportaParaODestino()
    {
        var recebidos = new System.Collections.Concurrent.ConcurrentBag<string>();
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var coletor = builder.Build();
        coletor.MapPost("/v1/{sinal}", (string sinal) =>
        {
            recebidos.Add(sinal);
            return Results.Ok();
        });
        await coletor.StartAsync(TestContext.Current.CancellationToken);

        Environment.SetEnvironmentVariable("Telemetria__Endpoint", coletor.Urls.Single());
        try
        {
            using var fabrica = new FabricaApi();
            using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/rota-sem-endpoint", UriKind.Relative), TestContext.Current.CancellationToken);

            Assert.True(fabrica.Services.GetRequiredService<TracerProvider>().ForceFlush(10_000));
            Assert.True(fabrica.Services.GetRequiredService<MeterProvider>().ForceFlush(10_000));
            Assert.Contains("traces", recebidos);
            Assert.Contains("metrics", recebidos);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Telemetria__Endpoint", null);
        }
    }

    [Fact(DisplayName = "REQ-OBSERV-006 Destino indisponível não afeta a requisição")]
    public async Task DestinoIndisponivel()
    {
        Environment.SetEnvironmentVariable("Telemetria__Endpoint", "http://127.0.0.1:1");
        try
        {
            using var fabrica = new FabricaApi();

            using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/health/live", UriKind.Relative), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            fabrica.Services.GetRequiredService<TracerProvider>().ForceFlush(5_000);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Telemetria__Endpoint", null);
        }
    }
}
