using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Api.Agente.Infraestrutura.Integracao;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Primitives;

namespace Api.Agente.Tests;

/// <summary>
/// Integração com a Sofia central do treinamento: A2A 0.3.0 com a chave Bearer compartilhada (skill
/// <c>sofia-a2a-autenticacao</c>). As checagens de descoberta e de resposta repetem as do código da Sofia central
/// (<c>AgentDiscoveryService</c>, <c>JsonRpc03AgentClient</c> e <c>A2A03JsonCodec</c>, inspecionados em 2026-11-10).
/// </summary>
public sealed class AutenticacaoSofiaTests
{
    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    private static HttpClient SemChave(FabricaApi fabrica)
    {
        var cliente = fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = null;
        return cliente;
    }

    private static HttpClient ComCabecalho(FabricaApi fabrica, string valor)
    {
        var cliente = SemChave(fabrica);
        cliente.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", valor);
        return cliente;
    }

    /// <summary>O <c>message/send</c> como a Sofia central monta: TextPart com o contrato em string JSON, sem <c>configuration</c>.</summary>
    private static string MessageSend(string rpcId, string solicitacao) => new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = rpcId,
        ["method"] = "message/send",
        ["params"] = new JsonObject
        {
            ["message"] = new JsonObject
            {
                ["kind"] = "message",
                ["role"] = "user",
                ["messageId"] = Guid.NewGuid().ToString(),
                ["parts"] = new JsonArray(new JsonObject { ["kind"] = "text", ["text"] = solicitacao }),
            },
        },
    }.ToJsonString();

    private static string Rpc(string rpcId, string metodo, JsonNode parametros) =>
        new JsonObject { ["jsonrpc"] = "2.0", ["id"] = rpcId, ["method"] = metodo, ["params"] = parametros }.ToJsonString();

    private static async Task<(HttpStatusCode Status, string Corpo)> Postar(HttpClient cliente, string corpo)
    {
        using var resposta = await cliente.PostAsync(
            new Uri("/a2a", UriKind.Relative), new StringContent(corpo, Encoding.UTF8, "application/json"), Cancelamento);
        return (resposta.StatusCode, await resposta.Content.ReadAsStringAsync(Cancelamento));
    }

    [Theory(DisplayName = "REQ-A2A-010 Cartão sem chave, com chave errada ou com outra caixa responde 401 com WWW-Authenticate: Bearer")]
    [InlineData(null)]
    [InlineData("Bearer chave-errada")]
    [InlineData("Bearer CHAVE-SINTETICA-DE-TESTE-A2A")]
    [InlineData("Basic " + FabricaApi.ChaveDeTeste)]
    public async Task CartaoSemChaveValida(string? cabecalho)
    {
        using var fabrica = new FabricaApi();
        using var cliente = cabecalho is null ? SemChave(fabrica) : ComCabecalho(fabrica, cabecalho);

        foreach (var caminho in new[] { "/.well-known/agent-card.json", "/" })
        {
            using var resposta = await cliente.GetAsync(new Uri(caminho, UriKind.Relative), Cancelamento);
            var corpo = await resposta.Content.ReadAsStringAsync(Cancelamento);

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
            Assert.Equal("Bearer", resposta.Headers.WwwAuthenticate.ToString());
            Assert.DoesNotContain("analisar_documentos", corpo, StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "REQ-A2A-010 RPC sem chave responde 401 e não executa nada")]
    public async Task RpcSemChave()
    {
        using var fabrica = new FabricaApi();

        var (status, corpo) = await Postar(SemChave(fabrica), MessageSend("rpc-001", ContratoDeTeste.Solicitacao()));

        Assert.Equal(HttpStatusCode.Unauthorized, status);
        Assert.Contains("nao_autorizado", corpo, StringComparison.Ordinal);
        Assert.DoesNotContain(fabrica.Logs.Registros, registro => registro.Mensagem.StartsWith("Tarefa A2A", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "REQ-A2A-010 Sem a chave configurada, o cartão e o RPC ficam indisponíveis, nunca abertos")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SemChaveConfigurada(string configurada)
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseSetting("SOFIA_A2A_TOKEN", configurada));
        using var cliente = fabrica.CreateClient();

        using var cartao = await cliente.GetAsync(new Uri("/.well-known/agent-card.json", UriKind.Relative), Cancelamento);
        var (status, _) = await Postar(cliente, MessageSend("rpc-001", ContratoDeTeste.Solicitacao()));
        using var saude = await cliente.GetAsync(new Uri("/health/live", UriKind.Relative), Cancelamento);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, cartao.StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal(HttpStatusCode.OK, saude.StatusCode);
    }

    [Fact(DisplayName = "REQ-A2A-009 Cartão autenticado sem A2A-Version passa nas checagens de descoberta da Sofia central")]
    public async Task CartaoNoPerfilDaSofia()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseSetting("A2A:UrlPublica", "https://agente.exemplo.teste/a2a"));

        var corpo = await fabrica.CreateClient().GetStringAsync(new Uri("/.well-known/agent-card.json", UriKind.Relative), Cancelamento);
        using var documento = JsonDocument.Parse(corpo);
        var cartao = documento.RootElement;

        Assert.Equal("0.3.0", cartao.GetProperty("protocolVersion").GetString());
        Assert.Equal("JSONRPC", cartao.GetProperty("preferredTransport").GetString());
        Assert.Equal("Agente ECF", cartao.GetProperty("name").GetString());
        Assert.False(string.IsNullOrWhiteSpace(cartao.GetProperty("version").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(cartao.GetProperty("description").GetString()));
        Assert.Equal(JsonValueKind.Object, cartao.GetProperty("capabilities").ValueKind);
        Assert.False(cartao.GetProperty("capabilities").GetProperty("streaming").GetBoolean());
        Assert.False(cartao.GetProperty("capabilities").GetProperty("pushNotifications").GetBoolean());
        Assert.Contains(cartao.GetProperty("defaultInputModes").EnumerateArray(), modo => modo.GetString() is "text/plain");
        Assert.Contains(cartao.GetProperty("defaultOutputModes").EnumerateArray(), modo => modo.GetString() is "text/plain");
        Assert.Equal("https://agente.exemplo.teste/a2a", cartao.GetProperty("url").GetString());
        var esquema = cartao.GetProperty("securitySchemes").GetProperty(AgenteA2A.EsquemaDeSeguranca);
        Assert.Equal("http", esquema.GetProperty("type").GetString());
        Assert.Equal("bearer", esquema.GetProperty("scheme").GetString());
        var exigencia = Assert.Single(cartao.GetProperty("security").EnumerateArray());
        Assert.True(exigencia.TryGetProperty(AgenteA2A.EsquemaDeSeguranca, out _));
        Assert.Equal("analisar_documentos", Assert.Single(cartao.GetProperty("skills").EnumerateArray()).GetProperty("id").GetString());
        Assert.DoesNotContain(FabricaApi.ChaveDeTeste, corpo, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-A2A-009 Cartão 0.3.0 publicado é igual à cópia versionada, exceto o endereço (GD-01)")]
    public async Task CartaoV03IgualAoVersionado()
    {
        using var fabrica = new FabricaApi();

        var publicado = JsonNode.Parse(await fabrica.CreateClient().GetStringAsync(new Uri("/.well-known/agent-card.json", UriKind.Relative), Cancelamento))!.AsObject();
        var versionado = JsonNode.Parse(File.ReadAllText(Repositorio.Caminho("docs", "contratos", "agent-card-v0.3.json")))!.AsObject();
        foreach (var cartao in new[] { publicado, versionado })
        {
            cartao["url"] = "omitido";
            cartao["supportedInterfaces"]![0]!["url"] = "omitido";
        }

        Assert.True(JsonNode.DeepEquals(versionado, publicado), publicado.ToJsonString());
    }

    [Fact(DisplayName = "REQ-A2A-009 message/send da Sofia central volta Task concluída, um Artifact analise-sofia-v1 e os mesmos IDs")]
    public async Task MessageSendComoASofia()
    {
        using var fabrica = new FabricaApi();

        var (status, corpo) = await Postar(fabrica.CreateClient(), MessageSend("rpc-sofia-001", ContratoDeTeste.Solicitacao()));
        using var resposta = JsonDocument.Parse(corpo);
        var raiz = resposta.RootElement;

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal("2.0", raiz.GetProperty("jsonrpc").GetString());
        Assert.Equal("rpc-sofia-001", raiz.GetProperty("id").GetString());
        Assert.False(raiz.TryGetProperty("error", out _));
        var tarefa = raiz.GetProperty("result");
        Assert.Equal("task", tarefa.GetProperty("kind").GetString());
        Assert.False(string.IsNullOrWhiteSpace(tarefa.GetProperty("id").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(tarefa.GetProperty("contextId").GetString()));
        Assert.Equal("completed", tarefa.GetProperty("status").GetProperty("state").GetString());
        var artefato = Assert.Single(tarefa.GetProperty("artifacts").EnumerateArray(), item => item.GetProperty("name").GetString() == "analise-sofia-v1");
        Assert.True(artefato.TryGetProperty("artifactId", out _));
        var parte = Assert.Single(artefato.GetProperty("parts").EnumerateArray());
        Assert.Equal("text", parte.GetProperty("kind").GetString());
        using var resultado = JsonDocument.Parse(parte.GetProperty("text").GetString()!);
        Assert.Equal(ContratoDeTeste.IdExecucao, resultado.RootElement.GetProperty("idExecucao").GetString());
        Assert.Equal(ContratoDeTeste.IdSolicitacao, resultado.RootElement.GetProperty("idSolicitacao").GetString());
        Assert.Equal("agente-ecf", resultado.RootElement.GetProperty("agente").GetProperty("idAgente").GetString());
        Assert.Equal("Agente ECF", resultado.RootElement.GetProperty("agente").GetProperty("nome").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-009 tasks/get devolve a mesma tarefa e contexto, e tasks/cancel recusa a tarefa terminada com erro do protocolo")]
    public async Task ConsultaECancelamento()
    {
        using var fabrica = new FabricaApi();
        using var cliente = fabrica.CreateClient();
        var (_, corpo) = await Postar(cliente, MessageSend("rpc-001", ContratoDeTeste.Solicitacao()));
        using var enviada = JsonDocument.Parse(corpo);
        var id = enviada.RootElement.GetProperty("result").GetProperty("id").GetString()!;
        var contexto = enviada.RootElement.GetProperty("result").GetProperty("contextId").GetString()!;

        var (_, corpoGet) = await Postar(cliente, Rpc("rpc-002", "tasks/get", new JsonObject { ["id"] = id }));
        var (_, corpoCancel) = await Postar(cliente, Rpc("rpc-003", "tasks/cancel", new JsonObject { ["id"] = id }));
        using var consultada = JsonDocument.Parse(corpoGet);
        using var cancelada = JsonDocument.Parse(corpoCancel);

        Assert.Equal("rpc-002", consultada.RootElement.GetProperty("id").GetString());
        Assert.Equal(id, consultada.RootElement.GetProperty("result").GetProperty("id").GetString());
        Assert.Equal(contexto, consultada.RootElement.GetProperty("result").GetProperty("contextId").GetString());
        Assert.Equal("completed", consultada.RootElement.GetProperty("result").GetProperty("status").GetProperty("state").GetString());
        Assert.Equal(-32002, cancelada.RootElement.GetProperty("error").GetProperty("code").GetInt32());
        Assert.DoesNotContain("   at ", corpoCancel, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "REQ-A2A-009 Método inexistente e payload inválido voltam erro JSON-RPC, sem stack trace")]
    [InlineData("metodo/inexistente", "{}", -32601)]
    [InlineData("message/send", "{\"message\":\"nao-e-objeto\"}", -32602)]
    public async Task ErrosDoProtocolo(string metodo, string parametros, int codigo)
    {
        using var fabrica = new FabricaApi();

        var (_, corpo) = await Postar(fabrica.CreateClient(), Rpc("rpc-001", metodo, JsonNode.Parse(parametros)!));
        using var resposta = JsonDocument.Parse(corpo);

        Assert.Equal(codigo, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
        Assert.DoesNotContain("   at ", corpo, StringComparison.Ordinal);

        // Nos métodos da Sofia o id volta; no método desconhecido, o SDK responde com id nulo.
        if (metodo == "message/send")
        {
            Assert.Equal("rpc-001", resposta.RootElement.GetProperty("id").GetString());
        }
    }

    [Fact(DisplayName = "REQ-A2A-010 A chave não aparece nos logs, nas respostas nem no documento OpenAPI")]
    public async Task ChaveForaDeLogsERespostas()
    {
        using var fabrica = new FabricaApi();
        using var cliente = fabrica.CreateClient();

        var respostas = new List<string>
        {
            (await Postar(cliente, MessageSend("rpc-001", ContratoDeTeste.Solicitacao()))).Corpo,
            (await Postar(ComCabecalho(fabrica, "Bearer chave-errada"), MessageSend("rpc-002", ContratoDeTeste.Solicitacao()))).Corpo,
            await cliente.GetStringAsync(new Uri("/.well-known/agent-card.json", UriKind.Relative), Cancelamento),
            await cliente.GetStringAsync(new Uri("/openapi/v1.json", UriKind.Relative), Cancelamento),
        };

        Assert.DoesNotContain(respostas, corpo => corpo.Contains(FabricaApi.ChaveDeTeste, StringComparison.Ordinal));
        Assert.DoesNotContain(fabrica.Logs.Registros, registro => registro.TextoCompleto.Contains(FabricaApi.ChaveDeTeste, StringComparison.Ordinal));
        Assert.DoesNotContain(fabrica.Logs.Registros, registro => registro.TextoCompleto.Contains("chave-errada", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "REQ-A2A-010 O documento OpenAPI declara o Bearer nas rotas do A2A, e o REST e a saúde seguem sem chave")]
    public async Task BearerSoNasRotasDoA2A()
    {
        using var fabrica = new FabricaApi();
        using var cliente = SemChave(fabrica);
        using var documento = JsonDocument.Parse(await fabrica.CreateClient().GetStringAsync(new Uri("/openapi/v1.json", UriKind.Relative), Cancelamento));
        var rotas = documento.RootElement.GetProperty("paths");

        using var analise = await cliente.GetAsync(new Uri($"/analises/{Guid.NewGuid()}", UriKind.Relative), Cancelamento);
        using var saude = await cliente.GetAsync(new Uri("/health/live", UriKind.Relative), Cancelamento);

        Assert.True(rotas.GetProperty("/a2a").GetProperty("post").TryGetProperty("security", out _));
        Assert.True(rotas.GetProperty("/.well-known/agent-card.json").GetProperty("get").TryGetProperty("security", out _));
        Assert.False(rotas.GetProperty("/analises").GetProperty("post").TryGetProperty("security", out _));
        Assert.Equal("bearer", documento.RootElement.GetProperty("components").GetProperty("securitySchemes").GetProperty(AgenteA2A.EsquemaDeSeguranca).GetProperty("scheme").GetString());
        Assert.Equal(HttpStatusCode.NotFound, analise.StatusCode);
        Assert.Equal(HttpStatusCode.OK, saude.StatusCode);
    }

    [Theory(DisplayName = "REQ-A2A-010 Só um cabeçalho Bearer com a chave exata vale")]
    [InlineData("Bearer chave", true)]
    [InlineData("bearer chave", true)]
    [InlineData("Bearer Chave", false)]
    [InlineData("Bearer ", false)]
    [InlineData("Bearerchave", false)]
    [InlineData("Basic chave", false)]
    [InlineData("chave", false)]
    public void ValidacaoDoCabecalho(string cabecalho, bool valido) =>
        Assert.Equal(valido, AutenticacaoDaSofia.Valida(new StringValues(cabecalho), "chave"));

    [Fact(DisplayName = "REQ-A2A-010 Dois cabeçalhos Authorization não valem")]
    public void DoisCabecalhos() =>
        Assert.False(AutenticacaoDaSofia.Valida(new StringValues(["Bearer chave", "Bearer chave"]), "chave"));

    [Fact(DisplayName = "REQ-A2A-010 O preflight de CORS passa sem a chave")]
    public async Task PreflightSemChave()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:4200"));
        using var requisicao = new HttpRequestMessage(HttpMethod.Options, new Uri("/a2a", UriKind.Relative));
        requisicao.Headers.Add("Origin", "http://localhost:4200");
        requisicao.Headers.Add("Access-Control-Request-Method", "POST");
        requisicao.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");

        using var resposta = await SemChave(fabrica).SendAsync(requisicao, Cancelamento);

        Assert.True(resposta.IsSuccessStatusCode, resposta.StatusCode.ToString());
        Assert.Equal("http://localhost:4200", Assert.Single(resposta.Headers.GetValues("Access-Control-Allow-Origin")));
    }
}
