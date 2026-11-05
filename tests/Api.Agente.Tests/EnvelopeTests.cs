using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using A2A;
using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Dominio.Execucoes;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Integracao;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Agente.Tests;

public sealed class EnvelopeTests
{
    private static EnvelopeDoResultado Montar(Action<Execucao>? ajuste = null, string sintese = "Síntese.")
    {
        var execucao = Execucao.Criar(Guid.NewGuid(), Guid.NewGuid(), new IdentidadeDeclarada("pseudonimo", "org-a"), DateTimeOffset.UtcNow);
        execucao.ConcluirEtapa(Etapa.Recepcao);
        execucao.ConcluirEtapa(Etapa.ValidacaoDaEntrada);
        ajuste?.Invoke(execucao);
        execucao.Concluir(DateTimeOffset.UtcNow);
        return MontadorDoEnvelope.Montar(execucao, "corr-1", "1.0.0", sintese, null);
    }

    [Fact(DisplayName = "REQ-RESULT-001 Envelope traz versão, identificadores, status, etapas e tempos preenchidos pelo sistema")]
    public void EnvelopeCompleto()
    {
        var envelope = Montar();

        Assert.Equal("1.0", envelope.VersaoContrato);
        Assert.NotEqual(Guid.Empty, envelope.IdExecucao);
        Assert.Equal("corr-1", envelope.IdCorrelacao);
        Assert.Equal("parcial", envelope.Status);
        Assert.Equal(9, envelope.Etapas.Count);
        Assert.NotNull(envelope.ConcluidaEm);
        Assert.False(envelope.Identidade.Verificada);
    }

    [Fact(DisplayName = "REQ-RESULT-001 Envelope faz o round-trip JSON sem perda")]
    public void RoundTrip()
    {
        var envelope = Montar(execucao => execucao.AdicionarAchado(Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra", "Descrição.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "R1", 3, "CAMPO")], ["Linha 3"])));

        var json = JsonSerializer.Serialize(envelope, ContratoSofiaV1.Json);
        var lido = JsonSerializer.Deserialize<EnvelopeDoResultado>(json, ContratoSofiaV1.Json);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(JsonSerializer.Serialize(lido, ContratoSofiaV1.Json))));
    }

    [Fact(DisplayName = "REQ-RESULT-006 Validações não realizadas e limitações constam do resultado, com o motivo")]
    public void LimitacoesComMotivo()
    {
        var envelope = Montar(execucao => execucao.AdicionarLimitacao("Manual da versão indisponível: a consulta não encontrou a versão aplicável."));

        Assert.Contains(envelope.Limitacoes, limitacao => limitacao.StartsWith("Etapa não realizada: Validação estrutural. Motivo:", StringComparison.Ordinal));
        Assert.Contains("Manual da versão indisponível: a consulta não encontrou a versão aplicável.", envelope.Limitacoes);
        Assert.Equal("parcial", envelope.Status);
    }

    [Fact(DisplayName = "REQ-RESULT-006 Depois de uma falha, o motivo das etapas seguintes é a falha")]
    public void MotivoDeFalhaAnterior()
    {
        var envelope = Montar(execucao => execucao.FalharEtapa(Etapa.IdentificacaoDoLeiaute));

        Assert.Contains("Etapa não realizada: Validação estrutural. Motivo: uma etapa anterior falhou.", envelope.Limitacoes);
    }

    [Fact(DisplayName = "REQ-EXEC-003 Justificativa da etapa não aplicável consta do resultado")]
    public void JustificativaNoResultado()
    {
        const string Justificativa = "Nenhuma ferramenta externa é necessária nesta análise.";

        var envelope = Montar(execucao => execucao.MarcarNaoAplicavel(Etapa.FerramentasExternas, Justificativa));

        Assert.Contains(envelope.Etapas, etapa => etapa.Justificativa == Justificativa);
    }

    [Fact(DisplayName = "REQ-RESULT-006 Cenário do manual da versão indisponível")]
    public void ManualIndisponivel()
    {
        var envelope = Montar(execucao => execucao.AdicionarLimitacao("Consulta ao manual: versão aplicável não encontrada."));

        Assert.Contains("Consulta ao manual: versão aplicável não encontrada.", envelope.Limitacoes);
        Assert.Contains("Consulta ao manual: versão aplicável não encontrada.", ProjecaoParaSofia.Projetar(envelope).Observacoes);
    }

    [Fact(DisplayName = "GD-01 Exemplo do documento é um envelope válido, e o documento cita todos os campos")]
    public void DocumentoDoContrato()
    {
        var documento = File.ReadAllText(Repositorio.Caminho("docs", "contratos", "envelope-resultado.md"));
        var inicio = documento.IndexOf("```json\n", StringComparison.Ordinal) + "```json\n".Length;
        var exemplo = documento[inicio..documento.IndexOf("```", inicio, StringComparison.Ordinal)];

        var lido = JsonSerializer.Deserialize<EnvelopeDoResultado>(exemplo, ContratoSofiaV1.Json);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(exemplo), JsonSerializer.SerializeToNode(lido, ContratoSofiaV1.Json)));

        var completo = JsonSerializer.SerializeToNode(
            Montar(execucao => execucao.AdicionarAchado(Achado.Criar(
                "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra", "Descrição.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "R1", 3, "CAMPO")], ["Linha 3"]))),
            ContratoSofiaV1.Json)!.AsObject();
        var campos = completo.Select(campo => campo.Key)
            .Concat(completo["achados"]![0]!.AsObject().Select(campo => campo.Key))
            .Concat(completo["identidade"]!.AsObject().Select(campo => campo.Key))
            .Concat(completo["versoes"]!.AsObject().Select(campo => campo.Key));
        Assert.All(campos, campo => Assert.Contains($"`{campo}`", documento, StringComparison.Ordinal));
    }

    [Theory(DisplayName = "RA-07 Status interno é mapeado para os três do contrato SOFIA")]
    [InlineData("concluida", "concluido")]
    [InlineData("parcial", "parcial")]
    [InlineData("entrada-rejeitada", "erro")]
    [InlineData("falha-tecnica", "erro")]
    public void MapeamentoDeStatus(string interno, string contrato) =>
        Assert.Equal(contrato, ProjecaoParaSofia.StatusDoContrato(interno));

    [Fact(DisplayName = "RA-07 Projeção para a Sofia leva os campos opcionais e mantém os da v1.0")]
    public void ProjecaoParaOContrato()
    {
        var envelope = Montar(execucao => execucao.AdicionarAchado(Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra X", "Campo ausente.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "R1"), new RegistroEnvolvido("doc-1", "R2")], ["Linha 3"])));

        var resultado = ProjecaoParaSofia.Projetar(envelope);

        var achado = Assert.Single(resultado.Achados);
        Assert.Equal(envelope.IdSolicitacao.ToString(), resultado.IdSolicitacao);
        Assert.Equal(envelope.IdExecucao.ToString(), resultado.IdExecucaoAgente);
        Assert.True(resultado.RevisaoHumana);
        Assert.Equal(["doc-1"], achado.IdDocumentos);
        Assert.Equal("alta", achado.Severidade);
        Assert.Equal("erro-estrutural", achado.Categoria);
        Assert.Equal("validador", achado.Origem);
        Assert.Equal("Regra X", achado.Regra);
    }
}

public sealed class IdentidadeTests
{
    [Fact(DisplayName = "D5 Do nome, só o SHA-256 é guardado, como pseudônimo")]
    public void Pseudonimo()
    {
        var identidade = IdentidadeDaRequisicao.Declarada("Maria Oliveira", "org-a");

        Assert.Equal(64, identidade.UsuarioPseudonimo.Length);
        Assert.DoesNotContain("Maria", identidade.UsuarioPseudonimo, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(identidade.UsuarioPseudonimo, IdentidadeDaRequisicao.Declarada("Maria Oliveira", "org-b").UsuarioPseudonimo);
        Assert.Equal("org-a", identidade.Organizacao);
    }

    [Theory(DisplayName = "D5 Sem nome informado, nada é inventado")]
    [InlineData(null)]
    [InlineData(" ")]
    public void SemNome(string? nome) =>
        Assert.Equal(IdentidadeDaRequisicao.UsuarioNaoInformado, IdentidadeDaRequisicao.Declarada(nome, "org-a").UsuarioPseudonimo);

    [Theory(DisplayName = "REQ-EXEC-001 Organização padrão é aplicada e marcada como não verificada")]
    [InlineData(null, "infis")]
    [InlineData("org-a", "org-a")]
    public async Task OrganizacaoPadrao(string? configurada, string esperada)
    {
        using var fabrica = new FabricaApi().Com(builder =>
        {
            if (configurada is not null)
            {
                builder.UseSetting("Identidade:OrganizacaoPadrao", configurada);
            }
        });
        using var http = fabrica.CreateClient();
        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));
        using var resultado = ContratoDeTeste.ResultadoDoArtefato(enviada.RootElement.GetProperty("result").GetProperty("task"));

        using var resposta = await http.GetAsync(new Uri($"/analises/{resultado.RootElement.GetProperty("idExecucaoAgente").GetString()}", UriKind.Relative), TestContext.Current.CancellationToken);
        using var envelope = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var identidade = envelope.RootElement.GetProperty("identidade");
        Assert.Equal(esperada, identidade.GetProperty("organizacao").GetString());
        Assert.False(identidade.GetProperty("verificada").GetBoolean());
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Organização padrão inválida impede a inicialização sem revelar o valor")]
    public async Task OrganizacaoInvalida()
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesDaIdentidade>(configuracao, OpcoesDaIdentidade.Secao),
            [new("Identidade:OrganizacaoPadrao", "org com espaço")]);

        Assert.Contains("Identidade:OrganizacaoPadrao", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("org com espaço", erro.Message, StringComparison.Ordinal);
    }
}

public sealed class AnalisesEndpointsTests
{
    /// <summary>Envia a solicitação de exemplo com o cliente real do SDK A2A, como a Sofia fará.</summary>
    private static async Task<(AgentTask Tarefa, JsonDocument Resultado)> EnviarComClienteDoSdk(HttpClient http)
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var cartao = await new A2ACardResolver(http.BaseAddress!, http).GetAgentCardAsync(cancelamento);
        var resposta = await new A2AClient(new Uri(cartao.SupportedInterfaces[0].Url), http).SendMessageAsync(
            new SendMessageRequest
            {
                Message = new Message
                {
                    Role = Role.User,
                    MessageId = Guid.NewGuid().ToString(),
                    Parts = [Part.FromText(ContratoDeTeste.Fixture("solicitacao-exemplo.json"))],
                },
            },
            cancelamento);
        var tarefa = Assert.IsType<AgentTask>(resposta.Task);
        return (tarefa, JsonDocument.Parse(Assert.Single(Assert.Single(tarefa.Artifacts!).Parts).Text!));
    }

    [Fact(DisplayName = "REQ-A2A-002 Pelo cliente A2A real, a solicitação vira execução parcial, com os campos opcionais")]
    public async Task PontaAPontaComClienteDoSdk()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();

        var (tarefa, resultado) = await EnviarComClienteDoSdk(http);

        using (resultado)
        {
            Assert.Equal(TaskState.Completed, tarefa.Status.State);
            Assert.Equal("parcial", resultado.RootElement.GetProperty("status").GetString());
            Assert.True(resultado.RootElement.GetProperty("revisaoHumana").GetBoolean());
            using var consulta = await http.GetAsync(new Uri($"/analises/{resultado.RootElement.GetProperty("idExecucaoAgente").GetString()}", UriKind.Relative), TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        }
    }

    [Fact(DisplayName = "REQ-A2A-002 Pelo cliente A2A real, falha do banco termina em resultado de falha técnica")]
    public async Task FalhaDoBancoComClienteDoSdk()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.ConfigureTestServices(servicos =>
            servicos.AddSingleton<IRepositorioDeExecucoes>(new RepositorioEmMemoria { Indisponivel = true })));
        using var http = fabrica.CreateClient();

        var (tarefa, resultado) = await EnviarComClienteDoSdk(http);

        using (resultado)
        {
            Assert.Equal(TaskState.Completed, tarefa.Status.State);
            Assert.Equal("erro", resultado.RootElement.GetProperty("status").GetString());
            Assert.Equal("falha_tecnica", resultado.RootElement.GetProperty("erro").GetProperty("codigo").GetString());
            Assert.Equal(ContratoDeTeste.IdSolicitacao, resultado.RootElement.GetProperty("idSolicitacao").GetString());
            Assert.Empty(resultado.RootElement.GetProperty("achados").EnumerateArray());
        }
    }

    private static async Task<(string IdExecucao, string IdSolicitacao)> Registrar(HttpClient http, string? correlacao = null)
    {
        using var resposta = await ContratoDeTeste.ChamarJsonRpc(http, "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()), correlacao);
        using var resultado = ContratoDeTeste.ResultadoDoArtefato(resposta.RootElement.GetProperty("result").GetProperty("task"));
        return (resultado.RootElement.GetProperty("idExecucaoAgente").GetString()!, resultado.RootElement.GetProperty("idSolicitacao").GetString()!);
    }

    [Fact(DisplayName = "REQ-EXEC-005 GET /analises/{id} devolve o envelope da execução, sem credencial")]
    public async Task ConsultaDaExecucao()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        var (idExecucao, idSolicitacao) = await Registrar(http, "corr-consulta");

        using var resposta = await http.GetAsync(new Uri($"/analises/{idExecucao}", UriKind.Relative), TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var raiz = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(idExecucao, raiz.GetProperty("idExecucao").GetString());
        Assert.Equal(idSolicitacao, raiz.GetProperty("idSolicitacao").GetString());
        Assert.Equal("corr-consulta", raiz.GetProperty("idCorrelacao").GetString());
        Assert.Equal("parcial", raiz.GetProperty("status").GetString());
        Assert.True(raiz.GetProperty("revisaoHumana").GetBoolean());
        Assert.Equal(9, raiz.GetProperty("etapas").GetArrayLength());
    }

    [Fact(DisplayName = "REQ-EXEC-005 Execução inexistente responde 404")]
    public async Task ExecucaoInexistente()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await fabrica.CreateClient().GetAsync(new Uri($"/analises/{Guid.NewGuid()}", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact(DisplayName = "GD-01 Rota de consulta consta do documento OpenAPI")]
    public async Task DocumentoOpenApi()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/openapi/v1.json", UriKind.Relative), TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.True(json.RootElement.GetProperty("paths").TryGetProperty("/analises/{id}", out _));
    }

    [Fact(DisplayName = "REQ-EXEC-006 Execução registra hash, versões e etapas, sem o texto e sem o nome")]
    public async Task ExecucaoAuditavel()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        var (idExecucao, _) = await Registrar(http);

        using var resposta = await http.GetAsync(new Uri($"/analises/{idExecucao}", UriKind.Relative), TestContext.Current.CancellationToken);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(corpo);
        var raiz = json.RootElement;

        Assert.Equal(2, raiz.GetProperty("documentos").GetArrayLength());
        Assert.Equal(64, raiz.GetProperty("documentos")[0].GetProperty("hash").GetString()!.Length);
        Assert.Equal("1.0", raiz.GetProperty("versoes").GetProperty("contrato").GetString());
        Assert.False(string.IsNullOrWhiteSpace(raiz.GetProperty("versoes").GetProperty("servico").GetString()));
        Assert.DoesNotContain("Maria Oliveira", corpo, StringComparison.Ordinal);
        Assert.DoesNotContain("texto extraído", corpo, StringComparison.Ordinal);
        Assert.DoesNotContain("divergências fiscais", corpo, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "D7 Retentativa com o mesmo idSolicitacao devolve a execução já registrada")]
    public async Task RetentativaDevolveORegistrado()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();

        var primeira = await Registrar(http);
        var segunda = await Registrar(http);

        Assert.Equal(primeira.IdExecucao, segunda.IdExecucao);
    }

    [Fact(DisplayName = "REQ-SAUDE-002 Banco indisponível deixa a prontidão em 503, e a vivacidade em 200")]
    public async Task ProntidaoSemBanco()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.ConfigureTestServices(servicos =>
            servicos.AddSingleton<IRepositorioDeExecucoes>(new RepositorioEmMemoria { Indisponivel = true })));
        using var http = fabrica.CreateClient();

        using var prontidao = await http.GetAsync(new Uri("/health/ready", UriKind.Relative), TestContext.Current.CancellationToken);
        using var vivacidade = await http.GetAsync(new Uri("/health/live", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, prontidao.StatusCode);
        Assert.Equal(HttpStatusCode.OK, vivacidade.StatusCode);
    }
}
