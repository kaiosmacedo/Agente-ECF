using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Agente.Endpoints;

/// <summary>Verificações de vivacidade e prontidão (REQ-SAUDE-001 a 003).</summary>
internal static class SaudeEndpoints
{
    /// <summary>Tag das verificações de dependências obrigatórias consultadas pela prontidão.</summary>
    public const string TagProntidao = "ready";

    public static IEndpointRouteBuilder MapearSaude(this IEndpointRouteBuilder rotas)
    {
        rotas.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = EscreverEstado,
        });

        rotas.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registro => registro.Tags.Contains(TagProntidao),
            ResponseWriter = EscreverEstado,
        });

        return rotas;
    }

    // Somente o estado geral: sem descrições, exceções, duração ou nomes de dependências (REQ-SAUDE-003).
    private static Task EscreverEstado(HttpContext contexto, HealthReport relatorio)
    {
        contexto.Response.ContentType = "application/json; charset=utf-8";
        return contexto.Response.WriteAsync($$"""{"status":"{{relatorio.Status}}"}""");
    }
}
