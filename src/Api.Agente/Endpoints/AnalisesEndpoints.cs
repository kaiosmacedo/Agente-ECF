using System.Security.Cryptography;
using System.Text;
using Api.Agente.Aplicacao.Analise;
using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Infraestrutura.Observabilidade;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Api.Agente.Endpoints;

/// <summary>
/// Envio e consulta de análises pela API REST (REQ-RECEPCAO-001, REQ-EXEC-005), sem credencial nem restrição por
/// organização (SQ-12). O envio usa o mesmo serviço de análise do A2A (REQ-RECEPCAO-008).
/// </summary>
internal static class AnalisesEndpoints
{
    public const string ParteDoArquivo = "arquivo";

    // Folga para o envelope do multipart além do arquivo (mudança 07, D3).
    private const long FolgaDoMultipart = 64 * 1024;

    public static IEndpointRouteBuilder MapearAnalises(this IEndpointRouteBuilder rotas)
    {
        var limite = rotas.ServiceProvider.GetRequiredService<AmbienteDaAnalise>().TamanhoMaximoDosDocumentosEmBytes;
        rotas.MapPost("/analises", EnviarAsync)
            .WithName("EnviarAnalise")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(limite + FolgaDoMultipart))
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<EnvelopeDoResultado>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
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

    /// <summary>
    /// Lê o arquivo em fluxo, sem gravar nada em disco e sem usar o nome do arquivo (REQ-RECEPCAO-004), até o limite
    /// mais um byte (REQ-RECEPCAO-002). O texto é lido como ISO 8859-1, a codificação do manual (leiaute 12, p. 32).
    /// </summary>
    private static async Task<IResult> EnviarAsync(
        HttpContext contexto, ServicoDeAnalise servico, AmbienteDaAnalise ambiente, IRepositorioDeExecucoes repositorio, CancellationToken cancelamento)
    {
        var limite = ambiente.TamanhoMaximoDosDocumentosEmBytes;
        if (!MediaTypeHeaderValue.TryParse(contexto.Request.ContentType, out var tipo)
            || !tipo.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase)
            || HeaderUtilities.RemoveQuotes(tipo.Boundary).Value is not { Length: > 0 } fronteira)
        {
            return Recusa("Envie o arquivo em multipart/form-data, na parte \"arquivo\".");
        }

        var leitor = new MultipartReader(fronteira, contexto.Request.Body);
        while (await leitor.ReadNextSectionAsync(cancelamento) is { } secao)
        {
            if (!ContentDispositionHeaderValue.TryParse(secao.ContentDisposition, out var disposicao)
                || !disposicao.IsFileDisposition()
                || !string.Equals(disposicao.Name.Value, ParteDoArquivo, StringComparison.Ordinal))
            {
                continue;
            }

            var (bytes, acimaDoLimite) = await LerAteOLimiteAsync(secao.Body, limite, cancelamento);
            var pedido = acimaDoLimite
                ? new PedidoDeAnalise(Guid.NewGuid(), Guid.NewGuid(), CanalDaAnalise.Rest, null, [], ServicoDeAnalise.ErroDeTamanho(limite))
                : new PedidoDeAnalise(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    CanalDaAnalise.Rest,
                    null,
                    [new DocumentoDoPedido(ParteDoArquivo, Encoding.Latin1.GetString(bytes), Convert.ToHexStringLower(SHA256.HashData(bytes)))]);
            var envelope = servico.Analisar(pedido, contexto.Response.Headers[CorrelacaoMiddleware.Cabecalho].ToString());
            try
            {
                var registrado = await repositorio.RegistrarAsync(envelope, cancelamento);
                contexto.Response.Headers.Location = $"/analises/{registrado.IdExecucao}";
                return Results.Json(registrado, ContratoSofiaV1.Json, statusCode: StatusCodes.Status201Created);
            }
            catch (FalhaDoRepositorio)
            {
                return Results.Problem(
                    "O registro da execução não respondeu; tente novamente.", statusCode: StatusCodes.Status503ServiceUnavailable, title: "falha_tecnica");
            }
        }

        return Recusa("Falta a parte \"arquivo\" com o arquivo da ECF.");
    }

    private static async Task<(byte[] Bytes, bool AcimaDoLimite)> LerAteOLimiteAsync(Stream corpo, long limite, CancellationToken cancelamento)
    {
        using var copia = new MemoryStream();
        var buffer = new byte[81920];
        int lidos;
        while ((lidos = await corpo.ReadAsync(buffer, cancelamento)) > 0)
        {
            if (copia.Length + lidos > limite)
            {
                return ([], true);
            }

            copia.Write(buffer, 0, lidos);
        }

        return (copia.ToArray(), false);
    }

    private static IResult Recusa(string mensagem) =>
        Results.Problem(mensagem, statusCode: StatusCodes.Status400BadRequest, title: "solicitacao_invalida");
}
