using System.Text.Json;
using System.Text.Json.Nodes;
using A2A;
using A2A.AspNetCore;
using Api.Agente.Infraestrutura.Integracao;

namespace Api.Agente.Endpoints;

/// <summary>Cartão do agente e endpoint JSON-RPC do perfil A2A v0.3.x (REQ-A2A-001, REQ-A2A-006).</summary>
internal static class A2AEndpoints
{
    // Limite padrão do corpo no Kestrel: o documento fica em memória, sem arquivo temporário (SQ-04).
    private const int CorpoEmMemoria = 30_000_000;

    private const int ErroDeSintaxe = -32700;
    private const int OperacaoNaoSuportada = -32004;

    public static IEndpointRouteBuilder MapearA2A(this IEndpointRouteBuilder rotas)
    {
        var gerenciador = rotas.ServiceProvider.GetRequiredService<ITaskManager>();
        rotas.MapA2A(gerenciador, AgenteA2A.Rota).AddEndpointFilter(ConferirEnvelope);
        rotas.MapWellKnownAgentCard(gerenciador, AgenteA2A.Rota);
        return rotas;
    }

    /// <summary>
    /// O SDK responde a JSON malformado com erro interno e expõe streaming mesmo com o cartão dizendo que não há,
    /// sem como devolver um erro JSON-RPC depois de abrir o fluxo. Por isso o envelope é conferido antes dele.
    /// </summary>
    private static async ValueTask<object?> ConferirEnvelope(EndpointFilterInvocationContext contexto, EndpointFilterDelegate proximo)
    {
        var requisicao = contexto.HttpContext.Request;
        requisicao.EnableBuffering(CorpoEmMemoria);
        try
        {
            using var envelope = await JsonDocument.ParseAsync(requisicao.Body, cancellationToken: contexto.HttpContext.RequestAborted);
            if (envelope.RootElement.ValueKind == JsonValueKind.Object
                && envelope.RootElement.TryGetProperty("method", out var metodo)
                && metodo.ValueKind == JsonValueKind.String
                && A2AMethods.IsStreamingMethod(metodo.GetString()!))
            {
                envelope.RootElement.TryGetProperty("id", out var id);
                return Erro(id.ValueKind == JsonValueKind.Undefined ? null : JsonValue.Create(id.Clone()), OperacaoNaoSuportada, "Este agente não oferece streaming.");
            }
        }
        catch (JsonException)
        {
            return Erro(null, ErroDeSintaxe, "O corpo não é um JSON válido.");
        }

        requisicao.Body.Position = 0;
        return await proximo(contexto);
    }

    // JsonObject mantém "id": null, que o JSON-RPC exige quando a requisição não pôde ser lida.
    private static IResult Erro(JsonNode? id, int codigo, string mensagem) => Results.Json(new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id,
        ["error"] = new JsonObject { ["code"] = codigo, ["message"] = mensagem },
    });
}
