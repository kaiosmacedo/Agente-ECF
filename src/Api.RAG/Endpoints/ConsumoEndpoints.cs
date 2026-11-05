using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Consumo;
using Microsoft.AspNetCore.Mvc;

namespace Api.RAG.Endpoints;

/// <summary>Consulta agregada de consumo (REQ-CONSUMO-005). A restrição por perfil entra na retomada da autenticação (SQ-12).</summary>
internal static class ConsumoEndpoints
{
    public static IEndpointRouteBuilder MapearConsumo(this IEndpointRouteBuilder rotas)
    {
        rotas.MapGet("/consumo", ConsultarAsync).WithName("ConsultarConsumo");
        return rotas;
    }

    private static async Task<IResult> ConsultarAsync(
        [FromQuery] DateTimeOffset? de,
        [FromQuery] DateTimeOffset? ate,
        [FromQuery] string? modelo,
        [FromQuery] Guid? idOperacao,
        IRepositorioDeConsumo repositorio,
        CancellationToken cancelamento)
    {
        try
        {
            var totais = await repositorio.AgregarAsync(new FiltroDeConsumo(de, ate, modelo, idOperacao), cancelamento);
            return Results.Ok(new RespostaDoConsumo(de, ate, "USD", totais));
        }
        catch (FalhaDoRepositorio)
        {
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    private sealed record RespostaDoConsumo(DateTimeOffset? De, DateTimeOffset? Ate, string Moeda, IReadOnlyList<TotalDoModelo> Modelos);
}
