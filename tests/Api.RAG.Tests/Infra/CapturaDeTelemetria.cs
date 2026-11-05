using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Api.RAG.Tests.Infra;

/// <summary>
/// Captura traces e métricas em memória. O ouvinte de atividades vale para o processo, então cada teste
/// filtra pelo próprio <see cref="NovoContexto"/>. O trace e a métrica fecham depois de a resposta chegar
/// ao cliente, por isso as leituras esperam a condição, em vez de confiar na primeira tentativa.
/// </summary>
internal sealed class CapturaDeTelemetria
{
    private static readonly TimeSpan Espera = TimeSpan.FromSeconds(5);

    private readonly ConcurrentQueue<Activity> traces = new();
    private readonly List<MetricSnapshot> metricas = [];

    /// <summary>Um <c>traceparent</c> W3C novo e o identificador do trace dele.</summary>
    public static (string Cabecalho, string IdDoTrace) NovoContexto()
    {
        var idDoTrace = ActivityTraceId.CreateRandom().ToHexString();
        return ($"00-{idDoTrace}-{ActivitySpanId.CreateRandom().ToHexString()}-01", idDoTrace);
    }

    public void Instalar(IWebHostBuilder builder) => builder.ConfigureTestServices(servicos =>
    {
        servicos.ConfigureOpenTelemetryTracerProvider(provedor => provedor.AddProcessor(new SimpleActivityExportProcessor(new ExportadorParaFila(traces))));
        servicos.ConfigureOpenTelemetryMeterProvider(provedor => provedor.AddInMemoryExporter(metricas));
    });

    /// <summary>Espera até haver um trace do contexto, ou até a condição valer.</summary>
    public async Task<IReadOnlyList<Activity>> EsperarTraces(string idDoTrace, Func<IReadOnlyList<Activity>, bool>? ate = null)
    {
        var condicao = ate ?? (encontrados => encontrados.Count > 0);
        var inicio = Stopwatch.GetTimestamp();
        while (true)
        {
            var encontrados = TracesDo(idDoTrace);
            if (condicao(encontrados) || Stopwatch.GetElapsedTime(inicio) > Espera)
            {
                return encontrados;
            }

            await Task.Delay(50, TestContext.Current.CancellationToken);
        }
    }

    /// <summary>Dá tempo para um trace fechar e devolve o que houver: para conferir a ausência dele.</summary>
    public async Task<IReadOnlyList<Activity>> TracesAposEspera(string idDoTrace)
    {
        await Task.Delay(500, TestContext.Current.CancellationToken);
        return TracesDo(idDoTrace);
    }

    /// <summary>Coleta as métricas até a condição valer ou o tempo acabar.</summary>
    public async Task<IReadOnlyList<MetricSnapshot>> EsperarMetricas(IServiceProvider servicos, Func<IReadOnlyList<MetricSnapshot>, bool> ate)
    {
        var inicio = Stopwatch.GetTimestamp();
        while (true)
        {
            servicos.GetRequiredService<MeterProvider>().ForceFlush();
            IReadOnlyList<MetricSnapshot> coletadas = [.. metricas];
            if (ate(coletadas) || Stopwatch.GetElapsedTime(inicio) > Espera)
            {
                return coletadas;
            }

            await Task.Delay(50, TestContext.Current.CancellationToken);
        }
    }

    private List<Activity> TracesDo(string idDoTrace) => [.. traces.Where(atividade => atividade.TraceId.ToHexString() == idDoTrace)];

    private sealed class ExportadorParaFila(ConcurrentQueue<Activity> fila) : BaseExporter<Activity>
    {
        public override ExportResult Export(in Batch<Activity> batch)
        {
            foreach (var atividade in batch)
            {
                fila.Enqueue(atividade);
            }

            return ExportResult.Success;
        }
    }
}
