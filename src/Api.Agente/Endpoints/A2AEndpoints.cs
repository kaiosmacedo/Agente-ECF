using A2A;
using A2A.AspNetCore;
using Api.Agente.Infraestrutura.Integracao;

namespace Api.Agente.Endpoints;

/// <summary>Cartão do agente e endpoint JSON-RPC do protocolo A2A v1.0 (REQ-A2A-001, REQ-A2A-006).</summary>
internal static class A2AEndpoints
{
    public static IEndpointRouteBuilder MapearA2A(this IEndpointRouteBuilder rotas)
    {
        rotas.MapA2A(AgenteA2A.Rota);
        rotas.MapWellKnownAgentCard(rotas.ServiceProvider.GetRequiredService<AgentCard>());
        return rotas;
    }
}
