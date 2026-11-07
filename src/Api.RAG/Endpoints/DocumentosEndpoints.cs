using Api.RAG.Aplicacao.Conhecimento;
using Api.RAG.Infraestrutura.Conhecimento;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Api.RAG.Endpoints;

/// <summary>
/// Ingestão e administração da base (REQ-INGEST-001 a 003, REQ-BASE-001 a 003). A restrição a administradores entra na
/// retomada da autenticação (SQ-12). O envio aciona provedor pago, então só existe se for ligado por configuração.
/// </summary>
internal static class DocumentosEndpoints
{
    public static IEndpointRouteBuilder MapearDocumentos(this IEndpointRouteBuilder rotas)
    {
        rotas.MapGet("/documentos", ListarAsync).WithName("ListarDocumentos");
        rotas.MapGet("/documentos/{id:guid}", ConsultarAsync).WithName("ConsultarDocumento");
        rotas.MapPost("/documentos/{id:guid}/desativar", DesativarAsync).WithName("DesativarDocumento");

        if (rotas.ServiceProvider.GetRequiredService<IOptions<OpcoesDaIngestao>>().Value.EndpointHabilitado)
        {
            rotas.MapPost("/documentos", IngerirAsync).WithName("IngerirDocumento").DisableAntiforgery();
        }

        return rotas;
    }

    private static async Task<IResult> ListarAsync([FromQuery] string? tipo, [FromQuery] string? versao, IRepositorioDeDocumentos repositorio, CancellationToken cancelamento) =>
        Results.Ok(await repositorio.ListarAsync(tipo, versao, cancelamento));

    private static async Task<IResult> ConsultarAsync(Guid id, IRepositorioDeDocumentos repositorio, CancellationToken cancelamento) =>
        await repositorio.BuscarAsync(id, cancelamento) is { } documento ? Results.Ok(documento) : Results.NotFound();

    private static async Task<IResult> DesativarAsync(Guid id, IRepositorioDeDocumentos repositorio, CancellationToken cancelamento) =>
        await repositorio.DesativarAsync(id, cancelamento) ? Results.NoContent() : Results.NotFound();

    private static async Task<IResult> IngerirAsync(
        IFormFile arquivo,
        [FromForm] string? emissor,
        [FromForm] string? titulo,
        [FromForm] string? tipo,
        [FromForm] string? versao,
        [FromForm] int? anoCalendario,
        [FromForm] string? versaoDaPublicacao,
        [FromForm] string? vigencia,
        [FromForm] string? origem,
        ServicoDeIngestao ingestao,
        HttpContext contexto,
        CancellationToken cancelamento)
    {
        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria, cancelamento);
        var metadados = new MetadadosDoDocumento(emissor, titulo, tipo, versao, anoCalendario, versaoDaPublicacao, vigencia, origem);
        var correlacao = contexto.Response.Headers["X-Correlation-Id"].ToString();

        try
        {
            var resultado = await ingestao.IngerirAsync(metadados, arquivo.FileName, memoria.ToArray(), correlacao, cancelamento);
            return resultado.JaExistia ? Results.Ok(resultado) : Results.Created($"/documentos/{resultado.Documento.Id}", resultado);
        }
        catch (IngestaoRecusada recusa)
        {
            var codigo = recusa.Motivo == MotivoDaRecusa.TamanhoExcedido ? StatusCodes.Status413PayloadTooLarge : StatusCodes.Status400BadRequest;
            return Results.Json(new { erro = recusa.Message, campo = recusa.Campo }, statusCode: codigo);
        }
        catch (FalhaDaIngestao falha)
        {
            return Results.Json(new { erro = falha.Message }, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
