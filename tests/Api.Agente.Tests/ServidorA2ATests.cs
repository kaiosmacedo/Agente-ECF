using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using A2A;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Hosting;

namespace Api.Agente.Tests;

public sealed class ServidorA2ATests
{
    private static async Task<JsonDocument> LerCartao(FabricaApi fabrica)
    {
        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/.well-known/agent-card.json", UriKind.Relative), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "REQ-A2A-001 Cartão declara o perfil da turma e somente a skill especificada")]
    public async Task CartaoComAsCapacidadesEspecificadas()
    {
        using var fabrica = new FabricaApi();

        using var cartao = await LerCartao(fabrica);
        var raiz = cartao.RootElement;

        Assert.Equal("0.3.0", raiz.GetProperty("protocolVersion").GetString());
        Assert.Equal("JSONRPC", raiz.GetProperty("preferredTransport").GetString());
        Assert.False(raiz.GetProperty("capabilities").GetProperty("streaming").GetBoolean());
        Assert.False(raiz.GetProperty("capabilities").GetProperty("pushNotifications").GetBoolean());
        var skill = Assert.Single(raiz.GetProperty("skills").EnumerateArray());
        Assert.Equal("analisar_documentos", skill.GetProperty("id").GetString());
        foreach (var campo in new[] { "name", "description", "version" })
        {
            Assert.False(string.IsNullOrWhiteSpace(raiz.GetProperty(campo).GetString()), campo);
        }

        foreach (var campo in new[] { "name", "description" })
        {
            Assert.False(string.IsNullOrWhiteSpace(skill.GetProperty(campo).GetString()), campo);
        }

        Assert.NotEmpty(skill.GetProperty("tags").EnumerateArray());
        Assert.DoesNotContain(raiz.EnumerateObject(), campo => campo.Value.ValueKind == JsonValueKind.Null);
    }

    [Fact(DisplayName = "REQ-A2A-001 Cartão publicado é igual à cópia versionada, exceto o endereço (GD-01)")]
    public async Task CartaoIgualAoVersionado()
    {
        using var fabrica = new FabricaApi();
        using var servido = await LerCartao(fabrica);

        var publicado = JsonNode.Parse(servido.RootElement.GetRawText())!.AsObject();
        var versionado = JsonNode.Parse(File.ReadAllText(Repositorio.Caminho("docs", "contratos", "agent-card.json")))!.AsObject();
        publicado.Remove("url");
        versionado.Remove("url");

        Assert.True(JsonNode.DeepEquals(versionado, publicado), publicado.ToJsonString());
    }

    [Fact(DisplayName = "REQ-A2A-001 Endereço público configurado vai para o cartão")]
    public async Task EnderecoPublicoConfigurado()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseSetting("A2A:UrlPublica", "https://agente.exemplo.teste/a2a"));

        using var cartao = await LerCartao(fabrica);

        Assert.Equal("https://agente.exemplo.teste/a2a", cartao.RootElement.GetProperty("url").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-001 Em produção, o cartão anuncia o endereço público")]
    public async Task EnderecoEmProducao()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseEnvironment("Production"));

        using var cartao = await LerCartao(fabrica);

        Assert.Equal("https://agente-ecf-agente.vercel.app/a2a", cartao.RootElement.GetProperty("url").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-003 Cliente A2A real descobre o agente e recebe o resultado, sem credencial")]
    public async Task FluxoPontaAPontaComClienteDoSdk()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        var cancelamento = TestContext.Current.CancellationToken;

        var cartao = await new A2ACardResolver(http.BaseAddress!, http).GetAgentCardAsync(cancelamento);
        var cliente = new A2AClient(new Uri(cartao.Url), http);
        var resposta = await cliente.SendMessageAsync(
            new MessageSendParams
            {
                Message = new AgentMessage
                {
                    Role = MessageRole.User,
                    MessageId = Guid.NewGuid().ToString(),
                    Parts = [new TextPart { Text = ContratoDeTeste.Fixture("solicitacao-exemplo.json") }],
                },
            },
            cancelamento);

        var tarefa = Assert.IsType<AgentTask>(resposta);
        Assert.Equal(TaskState.Completed, tarefa.Status.State);
        var artefato = Assert.Single(tarefa.Artifacts!);
        Assert.Equal("analise-sofia-v1", artefato.Name);
        using var resultado = JsonDocument.Parse(Assert.IsType<TextPart>(Assert.Single(artefato.Parts)).Text);
        Assert.Equal(ContratoDeTeste.IdExecucao, resultado.RootElement.GetProperty("idExecucao").GetString());
        Assert.Equal(ContratoDeTeste.IdSolicitacao, resultado.RootElement.GetProperty("idSolicitacao").GetString());
        Assert.Equal("parcial", resultado.RootElement.GetProperty("status").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-005 message/send no envelope do exemplo da turma devolve Task completed com Artifact")]
    public async Task EnvelopeDoExemplo()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "message/send", ContratoDeTeste.MessageSend(ContratoDeTeste.Solicitacao()));
        var tarefa = resposta.RootElement.GetProperty("result");

        Assert.Equal("2.0", resposta.RootElement.GetProperty("jsonrpc").GetString());
        Assert.Equal("rpc-001", resposta.RootElement.GetProperty("id").GetString());
        Assert.Equal("task", tarefa.GetProperty("kind").GetString());
        Assert.False(string.IsNullOrWhiteSpace(tarefa.GetProperty("id").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(tarefa.GetProperty("contextId").GetString()));
        Assert.Equal("completed", tarefa.GetProperty("status").GetProperty("state").GetString());
        Assert.Equal("text", tarefa.GetProperty("artifacts")[0].GetProperty("parts")[0].GetProperty("kind").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-005 Resultado com status erro continua em Task completed")]
    public async Task ErroDeNegocioEmTaskCompleted()
    {
        using var fabrica = new FabricaApi();
        var solicitacao = ContratoDeTeste.Solicitacao(s => s.Remove("pergunta"));

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "message/send", ContratoDeTeste.MessageSend(solicitacao));
        var tarefa = resposta.RootElement.GetProperty("result");
        using var resultado = ContratoDeTeste.ResultadoDoArtefato(tarefa);

        Assert.Equal("completed", tarefa.GetProperty("status").GetProperty("state").GetString());
        Assert.Equal("erro", resultado.RootElement.GetProperty("status").GetString());
        Assert.Equal(ContratoDeTeste.IdSolicitacao, resultado.RootElement.GetProperty("idSolicitacao").GetString());
    }

    [Theory(DisplayName = "REQ-A2A-005 Mensagem sem correlação possível termina em Task failed, sem Artifact")]
    [InlineData("isto não é JSON")]
    [InlineData("""{"idExecucao":"teste","idSolicitacao":"teste-1"}""")]
    public async Task MensagemSemCorrelacao(string texto)
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "message/send", ContratoDeTeste.MessageSend(texto));
        var tarefa = resposta.RootElement.GetProperty("result");

        Assert.Equal("failed", tarefa.GetProperty("status").GetProperty("state").GetString());
        Assert.False(tarefa.TryGetProperty("artifacts", out var artefatos) && artefatos.ValueKind == JsonValueKind.Array && artefatos.GetArrayLength() > 0);
    }

    [Fact(DisplayName = "REQ-A2A-005 Mensagem sem TextPart termina em Task failed")]
    public async Task MensagemSemTexto()
    {
        using var fabrica = new FabricaApi();
        var parametros = ContratoDeTeste.MessageSend(string.Empty);
        parametros["message"]!["parts"] = new JsonArray(new JsonObject { ["kind"] = "data", ["data"] = new JsonObject() });

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "message/send", parametros);

        Assert.Equal("failed", resposta.RootElement.GetProperty("result").GetProperty("status").GetProperty("state").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-006 Resposta de message/send é final mesmo sem bloqueio pedido")]
    public async Task RespostaFinalSemBloqueio()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(
            fabrica.CreateClient(), "message/send", ContratoDeTeste.MessageSend(ContratoDeTeste.Solicitacao(), new JsonObject { ["acceptedOutputModes"] = new JsonArray("text"), ["blocking"] = false }));

        Assert.Equal("completed", resposta.RootElement.GetProperty("result").GetProperty("status").GetProperty("state").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-006 tasks/get devolve a tarefa concluída com o mesmo resultado")]
    public async Task ConsultaDaTarefa()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "message/send", ContratoDeTeste.MessageSend(ContratoDeTeste.Solicitacao()));
        var id = enviada.RootElement.GetProperty("result").GetProperty("id").GetString();

        using var consultada = await ContratoDeTeste.ChamarJsonRpc(http, "tasks/get", new JsonObject { ["id"] = id });
        using var resultado = ContratoDeTeste.ResultadoDoArtefato(consultada.RootElement.GetProperty("result"));

        Assert.Equal(ContratoDeTeste.IdSolicitacao, resultado.RootElement.GetProperty("idSolicitacao").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-006 tasks/cancel em tarefa terminada responde -32002")]
    public async Task CancelarTarefaTerminada()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "message/send", ContratoDeTeste.MessageSend(ContratoDeTeste.Solicitacao()));
        var id = enviada.RootElement.GetProperty("result").GetProperty("id").GetString();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(http, "tasks/cancel", new JsonObject { ["id"] = id });

        Assert.Equal(-32002, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Theory(DisplayName = "REQ-A2A-006 Tarefa inexistente responde -32001")]
    [InlineData("tasks/get")]
    [InlineData("tasks/cancel")]
    public async Task TarefaInexistente(string metodo)
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), metodo, new JsonObject { ["id"] = "nao-existe" });

        Assert.Equal(-32001, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact(DisplayName = "REQ-A2A-006 Método desconhecido responde -32601")]
    public async Task MetodoDesconhecido()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "metodo/inexistente", new JsonObject());

        Assert.Equal(-32601, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Theory(DisplayName = "REQ-A2A-006 Streaming não é oferecido e responde -32004")]
    [InlineData("message/stream")]
    [InlineData("tasks/resubscribe")]
    public async Task StreamingNaoSuportado(string metodo)
    {
        using var fabrica = new FabricaApi();
        var parametros = metodo == "message/stream" ? ContratoDeTeste.MessageSend(ContratoDeTeste.Solicitacao()) : new JsonObject { ["id"] = "qualquer" };

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), metodo, parametros);

        Assert.Equal("rpc-001", resposta.RootElement.GetProperty("id").GetString());
        Assert.Equal(-32004, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact(DisplayName = "REQ-A2A-006 Corpo que não é JSON responde -32700")]
    public async Task CorpoQueNaoEhJson()
    {
        using var fabrica = new FabricaApi();
        using var conteudo = new StringContent("isto não é JSON", System.Text.Encoding.UTF8, "application/json");

        using var resposta = await fabrica.CreateClient().PostAsync(new Uri("/a2a", UriKind.Relative), conteudo, TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(-32700, json.RootElement.GetProperty("error").GetProperty("code").GetInt32());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("id").ValueKind);
    }

    [Fact(DisplayName = "REQ-A2A-006 Notificações não são oferecidas e respondem -32003")]
    public async Task NotificacoesNaoSuportadas()
    {
        using var fabrica = new FabricaApi();
        var parametros = new JsonObject
        {
            ["taskId"] = "qualquer",
            ["pushNotificationConfig"] = new JsonObject { ["url"] = "https://exemplo.teste/retorno" },
        };

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "tasks/pushNotificationConfig/set", parametros);

        Assert.Equal(-32003, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact(DisplayName = "REQ-SOFIA-005 Nome, pergunta e documento ficam fora dos logs, da resposta e de tasks/get")]
    public async Task DadosSensiveisForaDosLogsEDaMemoria()
    {
        const string nome = "Maria Oliveira";
        const string pergunta = "pergunta-sigilosa-456";
        const string texto = "conteudo-sigiloso-123";
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        var solicitacao = ContratoDeTeste.Solicitacao(s =>
        {
            s["pergunta"] = pergunta;
            s["documentos"]![0]!["texto"] = texto;
        });

        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "message/send", ContratoDeTeste.MessageSend(solicitacao));
        var id = enviada.RootElement.GetProperty("result").GetProperty("id").GetString();
        using var consultada = await ContratoDeTeste.ChamarJsonRpc(http, "tasks/get", new JsonObject { ["id"] = id });

        foreach (var sensivel in new[] { nome, pergunta, texto })
        {
            Assert.DoesNotContain(sensivel, enviada.RootElement.GetRawText(), StringComparison.Ordinal);
            Assert.DoesNotContain(sensivel, consultada.RootElement.GetRawText(), StringComparison.Ordinal);
            Assert.DoesNotContain(fabrica.Logs.Registros, registro => registro.TextoCompleto.Contains(sensivel, StringComparison.Ordinal));
        }
    }

    [Fact(DisplayName = "REQ-A2A-005 Correlação recebida e identificadores do contrato chegam aos logs")]
    public async Task CorrelacaoNosLogs()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(
            fabrica.CreateClient(), "message/send", ContratoDeTeste.MessageSend(ContratoDeTeste.Solicitacao()), correlacao: "sofia-123");

        Assert.Contains(fabrica.Logs.Registros, registro =>
            registro.Mensagem.Contains(ContratoDeTeste.IdSolicitacao, StringComparison.Ordinal)
            && registro.Mensagem.Contains(ContratoDeTeste.IdExecucao, StringComparison.Ordinal)
            && registro.Escopos.TryGetValue("CorrelationId", out var correlacao) && correlacao == "sofia-123");
    }
}
