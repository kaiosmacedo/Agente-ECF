using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using A2A;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Hosting;

namespace Api.Agente.Tests;

public sealed class ServidorA2ATests
{
    /// <summary>O cartão v1.0, pedido como um cliente v1.0 pede; sem o cabeçalho, ele sai no formato 0.3.0 (REQ-A2A-009).</summary>
    private static async Task<JsonDocument> LerCartao(FabricaApi fabrica)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri("/.well-known/agent-card.json", UriKind.Relative));
        requisicao.Headers.Add("A2A-Version", "1.0");
        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static string EnderecoDoCartao(JsonDocument cartao) =>
        cartao.RootElement.GetProperty("supportedInterfaces")[0].GetProperty("url").GetString()!;

    [Fact(DisplayName = "REQ-A2A-001 Cartão declara o protocolo 1.0 e somente a skill especificada")]
    public async Task CartaoComAsCapacidadesEspecificadas()
    {
        using var fabrica = new FabricaApi();

        using var cartao = await LerCartao(fabrica);
        var raiz = cartao.RootElement;
        var interfaceA2A = Assert.Single(raiz.GetProperty("supportedInterfaces").EnumerateArray());

        Assert.Equal("1.0", interfaceA2A.GetProperty("protocolVersion").GetString());
        Assert.Equal("JSONRPC", interfaceA2A.GetProperty("protocolBinding").GetString());
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
        publicado["supportedInterfaces"]![0]!["url"] = "omitido";
        versionado["supportedInterfaces"]![0]!["url"] = "omitido";

        Assert.True(JsonNode.DeepEquals(versionado, publicado), publicado.ToJsonString());
    }

    [Fact(DisplayName = "REQ-A2A-001 Endereço público configurado vai para o cartão")]
    public async Task EnderecoPublicoConfigurado()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseSetting("A2A:UrlPublica", "https://agente.exemplo.teste/a2a"));

        using var cartao = await LerCartao(fabrica);

        Assert.Equal("https://agente.exemplo.teste/a2a", EnderecoDoCartao(cartao));
    }

    [Fact(DisplayName = "REQ-A2A-001 Em produção, o cartão anuncia o endereço público")]
    public async Task EnderecoEmProducao()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseEnvironment("Production"));

        using var cartao = await LerCartao(fabrica);

        Assert.Equal("https://agente-ecf-agente.vercel.app/a2a", EnderecoDoCartao(cartao));
    }

    [Fact(DisplayName = "REQ-A2A-003 Cliente A2A real descobre o agente e recebe o resultado, sem credencial")]
    public async Task FluxoPontaAPontaComClienteDoSdk()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        var cancelamento = TestContext.Current.CancellationToken;

        var cartao = await new A2ACardResolver(http.BaseAddress!, http).GetAgentCardAsync(cancelamento);
        var cliente = new A2AClient(new Uri(cartao.SupportedInterfaces[0].Url), http);
        var resposta = await cliente.SendMessageAsync(
            new SendMessageRequest
            {
                Message = new Message
                {
                    Role = Role.User,
                    MessageId = Guid.NewGuid().ToString(),
                    Parts = [Part.FromText(ContratoDeTeste.Solicitacao())],
                },
            },
            cancelamento);

        var tarefa = Assert.IsType<AgentTask>(resposta.Task);
        Assert.Equal(TaskState.Completed, tarefa.Status.State);
        var artefato = Assert.Single(tarefa.Artifacts!);
        Assert.Equal("analise-sofia-v1", artefato.Name);
        using var resultado = JsonDocument.Parse(Assert.Single(artefato.Parts).Text!);
        Assert.Equal(ContratoDeTeste.IdExecucao, resultado.RootElement.GetProperty("idExecucao").GetString());
        Assert.Equal(ContratoDeTeste.IdSolicitacao, resultado.RootElement.GetProperty("idSolicitacao").GetString());
        Assert.Equal("parcial", resultado.RootElement.GetProperty("status").GetString());
        Assert.True(Guid.TryParse(resultado.RootElement.GetProperty("idExecucaoAgente").GetString(), out _));
        Assert.True(resultado.RootElement.GetProperty("revisaoHumana").GetBoolean());
    }

    [Fact(DisplayName = "REQ-A2A-005 SendMessage no envelope da v1.0 devolve Task concluída com Artifact")]
    public async Task EnvelopeDaV1()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));
        var tarefa = resposta.RootElement.GetProperty("result").GetProperty("task");

        Assert.Equal("2.0", resposta.RootElement.GetProperty("jsonrpc").GetString());
        Assert.Equal("rpc-001", resposta.RootElement.GetProperty("id").GetString());
        Assert.False(string.IsNullOrWhiteSpace(tarefa.GetProperty("id").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(tarefa.GetProperty("contextId").GetString()));
        Assert.Equal("TASK_STATE_COMPLETED", tarefa.GetProperty("status").GetProperty("state").GetString());
        Assert.False(string.IsNullOrWhiteSpace(tarefa.GetProperty("artifacts")[0].GetProperty("parts")[0].GetProperty("text").GetString()));
    }

    [Fact(DisplayName = "REQ-A2A-005 Resultado com status erro continua em Task concluída")]
    public async Task ErroDeNegocioEmTaskConcluida()
    {
        using var fabrica = new FabricaApi();
        var solicitacao = ContratoDeTeste.Solicitacao(s => s.Remove("pergunta"));

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "SendMessage", ContratoDeTeste.SendMessage(solicitacao));
        var tarefa = resposta.RootElement.GetProperty("result").GetProperty("task");
        using var resultado = ContratoDeTeste.ResultadoDoArtefato(tarefa);

        Assert.Equal("TASK_STATE_COMPLETED", tarefa.GetProperty("status").GetProperty("state").GetString());
        Assert.Equal("erro", resultado.RootElement.GetProperty("status").GetString());
        Assert.Equal(ContratoDeTeste.IdSolicitacao, resultado.RootElement.GetProperty("idSolicitacao").GetString());
    }

    [Theory(DisplayName = "REQ-A2A-005 Mensagem sem correlação possível termina em Task com falha, sem Artifact")]
    [InlineData("isto não é JSON")]
    [InlineData("""{"idExecucao":"teste","idSolicitacao":"teste-1"}""")]
    public async Task MensagemSemCorrelacao(string texto)
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "SendMessage", ContratoDeTeste.SendMessage(texto));
        var tarefa = resposta.RootElement.GetProperty("result").GetProperty("task");

        Assert.Equal("TASK_STATE_FAILED", tarefa.GetProperty("status").GetProperty("state").GetString());
        Assert.False(tarefa.TryGetProperty("artifacts", out var artefatos) && artefatos.ValueKind == JsonValueKind.Array && artefatos.GetArrayLength() > 0);
    }

    [Fact(DisplayName = "REQ-A2A-005 Mensagem sem parte de texto termina em Task com falha")]
    public async Task MensagemSemTexto()
    {
        using var fabrica = new FabricaApi();
        var parametros = ContratoDeTeste.SendMessage(string.Empty);
        parametros["message"]!["parts"] = new JsonArray(new JsonObject { ["data"] = new JsonObject() });

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "SendMessage", parametros);

        Assert.Equal("TASK_STATE_FAILED", resposta.RootElement.GetProperty("result").GetProperty("task").GetProperty("status").GetProperty("state").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-006 Resposta de SendMessage é final mesmo com returnImmediately")]
    public async Task RespostaFinalComRetornoImediato()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(
            fabrica.CreateClient(), "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao(), new JsonObject { ["returnImmediately"] = true }));
        var tarefa = resposta.RootElement.GetProperty("result").GetProperty("task");

        Assert.Equal("TASK_STATE_COMPLETED", tarefa.GetProperty("status").GetProperty("state").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-006 GetTask devolve a tarefa concluída com o mesmo resultado")]
    public async Task ConsultaDaTarefa()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));
        var id = enviada.RootElement.GetProperty("result").GetProperty("task").GetProperty("id").GetString();

        using var consultada = await ContratoDeTeste.ChamarJsonRpc(http, "GetTask", new JsonObject { ["id"] = id });
        using var resultado = ContratoDeTeste.ResultadoDoArtefato(consultada.RootElement.GetProperty("result"));

        Assert.Equal(ContratoDeTeste.IdSolicitacao, resultado.RootElement.GetProperty("idSolicitacao").GetString());
    }

    [Fact(DisplayName = "REQ-A2A-006 CancelTask em tarefa terminada responde -32002")]
    public async Task CancelarTarefaTerminada()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));
        var id = enviada.RootElement.GetProperty("result").GetProperty("task").GetProperty("id").GetString();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(http, "CancelTask", new JsonObject { ["id"] = id });

        Assert.Equal(-32002, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Theory(DisplayName = "REQ-A2A-006 Tarefa inexistente responde -32001")]
    [InlineData("GetTask")]
    [InlineData("CancelTask")]
    public async Task TarefaInexistente(string metodo)
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), metodo, new JsonObject { ["id"] = "nao-existe" });

        Assert.Equal(-32001, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Theory(DisplayName = "REQ-A2A-006 Método desconhecido ou da v0.3 responde -32601")]
    [InlineData("metodo/inexistente")]
    [InlineData("message/send")]
    public async Task MetodoDesconhecido(string metodo)
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), metodo, ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));

        Assert.Equal(-32601, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact(DisplayName = "REQ-A2A-006 Corpo que não é JSON responde -32700")]
    public async Task CorpoQueNaoEhJson()
    {
        using var fabrica = new FabricaApi();
        using var conteudo = new StringContent("isto não é JSON", Encoding.UTF8, "application/json");

        using var resposta = await fabrica.CreateClient().PostAsync(new Uri("/a2a", UriKind.Relative), conteudo, TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(-32700, json.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact(DisplayName = "REQ-A2A-006 Versão de protocolo não suportada responde -32009")]
    public async Task VersaoDeProtocoloNaoSuportada()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, new Uri("/a2a", UriKind.Relative))
        {
            Content = new StringContent(
                new JsonObject { ["jsonrpc"] = "2.0", ["id"] = "rpc-001", ["method"] = "SendMessage", ["params"] = ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()) }.ToJsonString(),
                Encoding.UTF8,
                "application/json"),
        };
        requisicao.Headers.Add("A2A-Version", "9.9");

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(-32009, json.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact(DisplayName = "REQ-A2A-006 Streaming não é oferecido e responde -32004")]
    public async Task StreamingNaoSuportado()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "SendStreamingMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));

        Assert.Equal("rpc-001", resposta.RootElement.GetProperty("id").GetString());
        Assert.Equal(-32004, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact(DisplayName = "REQ-A2A-006 Assinar uma tarefa terminada responde -32004")]
    public async Task AssinaturaNaoSuportada()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));
        var id = enviada.RootElement.GetProperty("result").GetProperty("task").GetProperty("id").GetString();

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(http, "SubscribeToTask", new JsonObject { ["id"] = id });

        Assert.Equal(-32004, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact(DisplayName = "REQ-A2A-006 Notificações não são oferecidas e respondem -32003")]
    public async Task NotificacoesNaoSuportadas()
    {
        using var fabrica = new FabricaApi();
        var parametros = new JsonObject { ["taskId"] = "qualquer", ["url"] = "https://exemplo.teste/retorno" };

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(fabrica.CreateClient(), "CreateTaskPushNotificationConfig", parametros);

        Assert.Equal(-32003, resposta.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact(DisplayName = "REQ-A2A-006 ListTasks não revela tarefas de outros chamadores")]
    public async Task ListaDeTarefasVazia()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));

        using var resposta = await ContratoDeTeste.ChamarJsonRpc(http, "ListTasks", new JsonObject());

        Assert.Empty(resposta.RootElement.GetProperty("result").GetProperty("tasks").EnumerateArray());
    }

    [Fact(DisplayName = "REQ-SOFIA-005 Nome, pergunta e documento ficam fora dos logs, da resposta e de GetTask")]
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

        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "SendMessage", ContratoDeTeste.SendMessage(solicitacao));
        var id = enviada.RootElement.GetProperty("result").GetProperty("task").GetProperty("id").GetString();
        using var consultada = await ContratoDeTeste.ChamarJsonRpc(http, "GetTask", new JsonObject { ["id"] = id });

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
            fabrica.CreateClient(), "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()), correlacao: "sofia-123");

        Assert.Contains(fabrica.Logs.Registros, registro =>
            registro.Mensagem.Contains(ContratoDeTeste.IdSolicitacao, StringComparison.Ordinal)
            && registro.Mensagem.Contains(ContratoDeTeste.IdExecucao, StringComparison.Ordinal)
            && registro.Escopos.TryGetValue("CorrelationId", out var correlacao) && correlacao == "sofia-123");
    }
}
