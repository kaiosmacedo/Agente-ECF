using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Api.Agente.Tests.Infra;

/// <summary>Fixtures sintéticas do contrato SOFIA v1.0 (skill sofia-a2a-contrato) e chamadas JSON-RPC cruas do A2A v1.0.</summary>
internal static class ContratoDeTeste
{
    public const string IdExecucao = "8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e";
    public const string IdSolicitacao = "e1b3a4cb-d729-4bd4-b796-b9c94be07b46";

    public static string Fixture(string nome) => File.ReadAllText(Repositorio.Caminho("tests", "Api.Agente.Tests", "Fixtures", "Sofia", nome), Encoding.UTF8);

    /// <summary>
    /// A solicitação de exemplo do contrato, com um ajuste opcional. O primeiro documento vira uma ECF sintética do
    /// leiaute 12 (mudança 07); o segundo continua um documento que não é ECF.
    /// </summary>
    public static string Solicitacao(Action<JsonObject>? ajuste = null)
    {
        var solicitacao = JsonNode.Parse(Fixture("solicitacao-exemplo.json"))!.AsObject();
        var ecf = solicitacao["documentos"]![0]!;
        ecf["nome"] = "ecf-2025.txt";
        ecf["tipo"] = "text/plain";
        ecf["texto"] = EcfSintetica.Texto();
        ajuste?.Invoke(solicitacao);
        return solicitacao.ToJsonString();
    }

    public static JsonObject SendMessage(string textoDaMensagem, JsonObject? configuracao = null)
    {
        var parametros = new JsonObject
        {
            ["message"] = new JsonObject
            {
                ["role"] = "ROLE_USER",
                ["messageId"] = Guid.NewGuid().ToString(),
                ["parts"] = new JsonArray(new JsonObject { ["text"] = textoDaMensagem }),
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
        requisicao.Headers.Add("A2A-Version", "1.0");
        if (correlacao is not null)
        {
            requisicao.Headers.Add("X-Correlation-Id", correlacao);
        }

        using var resposta = await cliente.SendAsync(requisicao, TestContext.Current.CancellationToken);
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>O resultado do contrato no Artifact de uma tarefa.</summary>
    public static JsonDocument ResultadoDoArtefato(JsonElement tarefa) =>
        JsonDocument.Parse(tarefa.GetProperty("artifacts")[0].GetProperty("parts")[0].GetProperty("text").GetString()!);
}
