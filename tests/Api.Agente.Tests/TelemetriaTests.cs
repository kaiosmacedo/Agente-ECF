using System.Net;
using System.Text.Json.Nodes;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Observabilidade;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Api.Agente.Tests;

public sealed class TelemetriaTests
{
    private static (FabricaApi Fabrica, CapturaDeTelemetria Captura) Iniciar()
    {
        var captura = new CapturaDeTelemetria();
        return (new FabricaApi().Com(captura.Instalar), captura);
    }

    private static async Task<HttpResponseMessage> Chamar(FabricaApi fabrica, string caminho, string traceparent)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri(caminho, UriKind.Relative));
        requisicao.Headers.Add("traceparent", traceparent);
        return await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "REQ-OBSERV-004 Requisição gera trace que continua o traceparent recebido")]
    public async Task TraceContinuaOContextoRecebido()
    {
        var (fabrica, captura) = Iniciar();
        using var _ = fabrica;
        var (traceparent, idDoTrace) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await Chamar(fabrica, "/.well-known/agent-card.json", traceparent);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotEmpty(await captura.EsperarTraces(idDoTrace));
    }

    [Fact(DisplayName = "REQ-OBSERV-004 Logs da requisição trazem o mesmo TraceId")]
    public async Task LogsComOMesmoTrace()
    {
        var (fabrica, _) = Iniciar();
        using var __ = fabrica;
        var (traceparent, idDoTrace) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await Chamar(fabrica, "/.well-known/agent-card.json", traceparent);

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

    [Fact(DisplayName = "REQ-OBSERV-004 SendMessage gera os traces do SDK A2A")]
    public async Task TracesDoSdkA2A()
    {
        var (fabrica, captura) = Iniciar();
        using var _ = fabrica;
        var (traceparent, idDoTrace) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await EnviarMensagem(fabrica, ContratoDeTeste.Solicitacao(), traceparent);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var traces = await captura.EsperarTraces(idDoTrace, encontrados =>
            encontrados.Any(atividade => atividade.Source.Name == "A2A") && encontrados.Any(atividade => atividade.Source.Name == "A2A.AspNetCore"));
        var fontes = traces.Select(atividade => atividade.Source.Name).ToList();
        Assert.Contains("A2A", fontes);
        Assert.Contains("A2A.AspNetCore", fontes);
    }

    [Fact(DisplayName = "REQ-OBSERV-005 Métrica registra a duração por rota e status")]
    public async Task MetricaPorRotaEStatus()
    {
        var (fabrica, captura) = Iniciar();
        using var _ = fabrica;
        var (traceparent, _) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await EnviarMensagem(fabrica, ContratoDeTeste.Solicitacao(), traceparent);

        static bool Registrada(IEnumerable<MetricSnapshot> metricas) => metricas
            .Where(metrica => metrica.Name == "http.server.request.duration")
            .SelectMany(metrica => metrica.MetricPoints)
            .Select(Atributos)
            .Any(atributos =>
                atributos.TryGetValue("http.route", out var rota) && rota == "/a2a"
                && atributos.TryGetValue("http.response.status_code", out var status) && status == "200");

        Assert.True(Registrada(await captura.EsperarMetricas(fabrica.Services, Registrada)));
    }

    [Fact(DisplayName = "REQ-OBSERV-006 Sem destino configurado, o serviço atende normalmente")]
    public async Task SemDestino()
    {
        var (fabrica, _) = Iniciar();
        using var __ = fabrica;
        var (traceparent, _) = CapturaDeTelemetria.NovoContexto();

        using var resposta = await Chamar(fabrica, "/.well-known/agent-card.json", traceparent);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
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

    [Fact(DisplayName = "REQ-OBSERV-007 Traces não trazem query string, credencial, nome nem documento")]
    public async Task TelemetriaSemDadosSensiveis()
    {
        var (fabrica, captura) = Iniciar();
        using var _ = fabrica;
        var (traceparent, idDoTrace) = CapturaDeTelemetria.NovoContexto();
        var solicitacao = ContratoDeTeste.Solicitacao(s => s["documentos"]![0]!["texto"] = "conteudo-sigiloso-123");

        using var resposta = await EnviarMensagem(fabrica, solicitacao, traceparent, "/a2a?chave=segredo-123");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var traces = await captura.EsperarTraces(idDoTrace);
        Assert.NotEmpty(traces);
        foreach (var sensivel in new[] { "segredo-123", "conteudo-sigiloso-123", "Maria Oliveira", "token-secreto-123" })
        {
            Assert.DoesNotContain(traces.SelectMany(atividade => atividade.Tags), tag => tag.Value?.Contains(sensivel, StringComparison.Ordinal) == true);
            Assert.DoesNotContain(traces, atividade => atividade.DisplayName.Contains(sensivel, StringComparison.Ordinal));
        }
    }

    private static async Task<HttpResponseMessage> EnviarMensagem(FabricaApi fabrica, string solicitacao, string traceparent, string caminho = "/a2a")
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, new Uri(caminho, UriKind.Relative))
        {
            Content = new StringContent(
                new JsonObject { ["jsonrpc"] = "2.0", ["id"] = "rpc-001", ["method"] = "SendMessage", ["params"] = ContratoDeTeste.SendMessage(solicitacao) }.ToJsonString(),
                System.Text.Encoding.UTF8,
                "application/json"),
        };
        requisicao.Headers.Add("A2A-Version", "1.0");
        requisicao.Headers.Add("traceparent", traceparent);
        requisicao.Headers.TryAddWithoutValidation("Authorization", "Bearer token-secreto-123");
        return await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
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
            using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/.well-known/agent-card.json", UriKind.Relative), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            // O span fecha depois da resposta: repete a exportação até o coletor receber, com limite.
            var tempo = System.Diagnostics.Stopwatch.StartNew();
            while (!recebidos.Contains("traces") && tempo.Elapsed < TimeSpan.FromSeconds(10))
            {
                fabrica.Services.GetRequiredService<TracerProvider>().ForceFlush(1_000);
                await Task.Delay(50, TestContext.Current.CancellationToken);
            }

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

            using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/.well-known/agent-card.json", UriKind.Relative), TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            fabrica.Services.GetRequiredService<TracerProvider>().ForceFlush(5_000);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Telemetria__Endpoint", null);
        }
    }
}
