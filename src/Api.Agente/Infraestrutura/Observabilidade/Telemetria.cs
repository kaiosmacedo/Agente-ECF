using System.ComponentModel.DataAnnotations;
using Api.Agente.Infraestrutura.Configuracao;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Agente.Infraestrutura.Observabilidade;

/// <summary>Opções da seção <c>Telemetria</c>. Sem endereço, nada é exportado (REQ-OBSERV-006).</summary>
internal sealed class OpcoesTelemetria : IValidatableObject
{
    public const string Secao = "Telemetria";

    /// <summary>Endereço base do destino OTLP/HTTP, como <c>http://coletor:4318</c>.</summary>
    public string? Endpoint { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Endpoint) && !EnderecoValido(Endpoint))
        {
            yield return new ValidationResult("Endereço inválido.", [nameof(Endpoint)]);
        }
    }

    public static bool EnderecoValido(string? endereco) =>
        Uri.TryCreate(endereco, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

/// <summary>Traces e métricas com OpenTelemetry (REQ-OBSERV-004 a 007; SQ-07).</summary>
internal static class Telemetria
{
    // Fonte e medidor do SDK A2A, com o nome do pacote.
    private const string SdkA2A = "A2A";
    private const string SdkA2AAspNetCore = "A2A.AspNetCore";

    public static IServiceCollection AdicionarTelemetria(this IServiceCollection servicos, IConfiguration configuracao, IdentidadeServico identidade)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesTelemetria>(configuracao, OpcoesTelemetria.Secao);

        // Endereço inválido não registra exportador: a validação das opções impede a inicialização.
        var endereco = configuracao[$"{OpcoesTelemetria.Secao}:{nameof(OpcoesTelemetria.Endpoint)}"];
        var exportar = OpcoesTelemetria.EnderecoValido(endereco);

        servicos.AddOpenTelemetry()
            .ConfigureResource(recurso => recurso.AddService(identidade.Nome, serviceVersion: identidade.Versao))
            .WithTracing(traces =>
            {
                traces
                    .AddAspNetCoreInstrumentation(opcoes => opcoes.Filter = contexto => !contexto.Request.Path.StartsWithSegments("/health"))
                    .AddHttpClientInstrumentation()
                    .AddSource(SdkA2A, SdkA2AAspNetCore);
                if (exportar)
                {
                    traces.AddOtlpExporter(opcoes => Destino(opcoes, endereco!, "v1/traces"));
                }
            })
            .WithMetrics(metricas =>
            {
                metricas
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(SdkA2A);
                if (exportar)
                {
                    metricas.AddOtlpExporter(opcoes => Destino(opcoes, endereco!, "v1/metrics"));
                }
            });

        return servicos;
    }

    private static void Destino(OtlpExporterOptions opcoes, string endereco, string caminho)
    {
        opcoes.Protocol = OtlpExportProtocol.HttpProtobuf;
        opcoes.Endpoint = new Uri(new Uri(endereco.TrimEnd('/') + "/"), caminho);
    }
}
