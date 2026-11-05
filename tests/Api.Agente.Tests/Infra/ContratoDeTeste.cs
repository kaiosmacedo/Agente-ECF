using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Api.Agente.Tests.Infra;

/// <summary>Fixtures sintéticas do contrato SOFIA v1.0 (skill sofia-a2a-contrato) e chamadas JSON-RPC cruas.</summary>
internal static class ContratoDeTeste
{
    public const string IdExecucao = "8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e";
    public const string IdSolicitacao = "e1b3a4cb-d729-4bd4-b796-b9c94be07b46";

    public static string Fixture(string nome) => File.ReadAllText(Repositorio.Caminho("tests", "Api.Agente.Tests", "Fixtures", "Sofia", nome), Encoding.UTF8);

    /// <summary>A solicitação de exemplo do contrato, com um ajuste opcional.</summary>
    public static string Solicitacao(Action<JsonObject>? ajuste = null)
    {
        var solicitacao = JsonNode.Parse(Fixture("solicitacao-exemplo.json"))!.AsObject();
        ajuste?.Invoke(solicitacao);
        return solicitacao.ToJsonString();
    }

    public static JsonObject MessageSend(string textoDaMensagem, JsonObject? configuracao = null)
    {
        var parametros = new JsonObject
        {
            ["message"] = new JsonObject
            {
                ["kind"] = "message",
                ["role"] = "user",
                ["messageId"] = Guid.NewGuid().ToString(),
                ["parts"] = new JsonArray(new JsonObject { ["kind"] = "text", ["text"] = textoDaMensagem }),
            },
        };
        if (configuracao is not null)
        {
            parametros["configuration"] = configuracao;
        }

        return parametros;
    }

    public static async Task<JsonDocument> ChamarJsonRpc(HttpClient cliente, string metodo, JsonObject parametros, string? correlacao = null)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, new Uri("/a2a", UriKind.Relative))
        {
            Content = JsonContent.Create(new JsonObject { ["jsonrpc"] = "2.0", ["id"] = "rpc-001", ["method"] = metodo, ["params"] = parametros }),
        };
        if (correlacao is not null)
        {
            requisicao.Headers.Add("X-Correlation-Id", correlacao);
        }

        using var resposta = await cliente.SendAsync(requisicao, TestContext.Current.CancellationToken);
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>O resultado do contrato que veio no Artifact de uma resposta <c>message/send</c> ou <c>tasks/get</c>.</summary>
    public static JsonDocument ResultadoDoArtefato(JsonElement tarefa) =>
        JsonDocument.Parse(tarefa.GetProperty("artifacts")[0].GetProperty("parts")[0].GetProperty("text").GetString()!);
}
