using A2A;
using A2A.V0_3Compat;
using Api.Agente.Infraestrutura.Integracao;

namespace Api.Agente.Endpoints;

/// <summary>
/// Cartão do agente e endpoint JSON-RPC do A2A, na v1.0 e na v0.3.0 da Sofia central do treinamento, no mesmo endereço
/// (REQ-A2A-001, REQ-A2A-006, REQ-A2A-009). Sem o cabeçalho <c>A2A-Version: 1.0</c>, o cartão sai no formato 0.3.0.
/// </summary>
internal static class A2AEndpoints
{
    public static IEndpointRouteBuilder MapearA2A(this IEndpointRouteBuilder rotas)
    {
        var manipulador = rotas.ServiceProvider.GetRequiredService<IA2ARequestHandler>();
        var cartao = rotas.ServiceProvider.GetRequiredService<AgentCard>();
        rotas.MapA2AWithV03Compat(manipulador, AgenteA2A.Rota);
        rotas.MapAgentCardGetWithV03Compat(manipulador, () => Task.FromResult(cartao), string.Empty);
        return rotas;
    }
}
