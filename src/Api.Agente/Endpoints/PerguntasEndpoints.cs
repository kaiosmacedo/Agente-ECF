using System.Text.Json;
using System.Text.Json.Nodes;
using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Aplicacao.Perguntas;
using Api.Agente.Infraestrutura.Observabilidade;

namespace Api.Agente.Endpoints;

/// <summary>Corpo do <c>POST /perguntas</c> (mudança 17, D2). A versão, quando vem, prevalece sobre a citada na pergunta.</summary>
internal sealed record PedidoDePerguntaRest(string? Pergunta, string? VersaoDoManual);

/// <summary>
/// Perguntas sobre a ECF pela API REST (REQ-PERGUNTA-008; REQ-DOCAPI-004), abertas como os demais endpoints REST
/// (SQ-12). Usa o mesmo serviço do A2A, então a resposta, as fontes e o status são os mesmos nos dois canais.
/// </summary>
internal static class PerguntasEndpoints
{
    public const string ExemploDePergunta = "O que é o Registro 0000 da ECF?";

    public static IEndpointRouteBuilder MapearPerguntas(this IEndpointRouteBuilder rotas)
    {
        rotas.MapPost("/perguntas", ResponderAsync)
            .WithName("ResponderPergunta")
            .WithSummary("Responde uma pergunta sobre a ECF só com os trechos dos manuais oficiais, citando a fonte.")
            .Accepts<PedidoDePerguntaRest>("application/json")
            // Exemplo só com a pergunta: com o "string" padrão em versaoDoManual, o Swagger pediria uma versão que não existe.
            .AddOpenApiOperationTransformer((operacao, _, _) =>
            {
                if (operacao.RequestBody?.Content?.TryGetValue("application/json", out var corpo) == true)
                {
                    corpo.Example = new JsonObject { ["pergunta"] = ExemploDePergunta };
                }

                return Task.CompletedTask;
            })
            .Produces<RespostaDaPergunta>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
        return rotas;
    }

    private static async Task<IResult> ResponderAsync(HttpContext contexto, ServicoDePerguntas servico, CancellationToken cancelamento)
    {
        if (!servico.Habilitada)
        {
            return Results.Problem("As perguntas sobre a ECF ainda não estão ligadas neste ambiente.", statusCode: StatusCodes.Status404NotFound, title: "perguntas_desligadas");
        }

        PedidoDePerguntaRest? pedido;
        try
        {
            pedido = await contexto.Request.ReadFromJsonAsync<PedidoDePerguntaRest>(ContratoSofiaV1.Json, cancelamento);
        }
        catch (Exception erro) when (erro is JsonException or InvalidOperationException or BadHttpRequestException)
        {
            pedido = null;
        }

        if (pedido?.Pergunta is null)
        {
            return Results.Problem(
                "Envie um JSON com o campo pergunta, como {\"pergunta\": \"O que é o Registro 0000?\"}.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "solicitacao_invalida");
        }

        var resposta = await servico.ResponderAsync(
            new PedidoDePergunta(pedido.Pergunta, "rest", null, pedido.VersaoDoManual, contexto.Response.Headers[CorrelacaoMiddleware.Cabecalho].ToString()),
            cancelamento);
        return Results.Json(resposta, ContratoSofiaV1.Json);
    }
}
