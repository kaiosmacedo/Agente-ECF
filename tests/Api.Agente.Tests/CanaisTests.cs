using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Api.Agente.Tests.Infra;

namespace Api.Agente.Tests;

/// <summary>Os dois canais com o PostgreSQL real em contêiner: o mesmo texto dá o mesmo resultado (REQ-RECEPCAO-008; mudança 07, tarefa 4.2).</summary>
public sealed class CanaisTests(BancoDeTeste banco) : IClassFixture<BancoDeTeste>
{
    private static string Resumo(JsonElement envelope) => JsonSerializer.Serialize(new
    {
        status = envelope.GetProperty("status").GetString(),
        veredito = envelope.GetProperty("veredito").GetString(),
        etapas = envelope.GetProperty("etapas").EnumerateArray().Select(etapa => $"{etapa.GetProperty("numero").GetInt32()}:{etapa.GetProperty("estado").GetString()}").ToList(),
        achados = envelope.GetProperty("achados").GetArrayLength(),
        leiaute = envelope.GetProperty("leiaute").GetRawText(),
        limitacoes = envelope.GetProperty("limitacoes").EnumerateArray().Select(limitacao => limitacao.GetString()).ToList(),
    });

    [Fact(DisplayName = "REQ-RECEPCAO-008 A mesma ECF pelo POST /analises e por A2A tem o mesmo status, veredito, etapas, achados e leiaute")]
    public async Task MesmoResultado()
    {
        using var fabrica = new FabricaApi(banco.Conexao);
        var cliente = fabrica.CreateClient();
        var cancelamento = TestContext.Current.CancellationToken;

        var arquivo = new ByteArrayContent(EcfSintetica.Bytes());
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        using var rest = await cliente.PostAsync(new Uri("/analises", UriKind.Relative), new MultipartFormDataContent { { arquivo, "arquivo", "ecf.txt" } }, cancelamento);
        using var pelaApi = JsonDocument.Parse(await rest.Content.ReadAsStringAsync(cancelamento));

        var solicitacao = ContratoDeTeste.Solicitacao(s =>
        {
            s["idSolicitacao"] = Guid.NewGuid().ToString();
            s["documentos"] = new JsonArray(new JsonObject { ["idDocumento"] = "doc-001", ["nome"] = "ecf.txt", ["texto"] = EcfSintetica.Texto() });
        });
        using var enviada = await ContratoDeTeste.ChamarJsonRpc(cliente, "SendMessage", ContratoDeTeste.SendMessage(solicitacao));
        using var resultado = ContratoDeTeste.ResultadoDoArtefato(enviada.RootElement.GetProperty("result").GetProperty("task"));
        using var consulta = await cliente.GetAsync(new Uri($"/analises/{resultado.RootElement.GetProperty("idExecucaoAgente").GetString()}", UriKind.Relative), cancelamento);
        using var peloA2A = JsonDocument.Parse(await consulta.Content.ReadAsStringAsync(cancelamento));

        Assert.Equal(Resumo(pelaApi.RootElement), Resumo(peloA2A.RootElement));
        Assert.Equal(("rest", "a2a"), (pelaApi.RootElement.GetProperty("canal").GetString(), peloA2A.RootElement.GetProperty("canal").GetString()));
    }
}
