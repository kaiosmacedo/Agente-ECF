using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Aplicacao.Execucoes;

namespace Api.Agente.Endpoints;

/// <summary>Consulta do resultado de uma execução (REQ-EXEC-005). Sem credencial nem restrição por organização (SQ-12).</summary>
internal static class AnalisesEndpoints
{
    public static IEndpointRouteBuilder MapearAnalises(this IEndpointRouteBuilder rotas)
    {
        rotas.MapGet("/analises/{id:guid}", ConsultarAsync)
            .WithName("ConsultarAnalise")
            .Produces<EnvelopeDoResultado>()
            .Produces(StatusCodes.Status404NotFound);
        return rotas;
    }

    private static async Task<IResult> ConsultarAsync(Guid id, IRepositorioDeExecucoes repositorio, CancellationToken cancelamento)
    {
        var envelope = await repositorio.BuscarAsync(id, cancelamento);
        return envelope is null ? Results.NotFound() : Results.Json(envelope, ContratoSofiaV1.Json);
    }
}
