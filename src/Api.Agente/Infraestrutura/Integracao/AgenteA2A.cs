using System.Text.Json;
using A2A;
using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Infraestrutura.Observabilidade;
using TaskStatus = A2A.TaskStatus;

namespace Api.Agente.Infraestrutura.Integracao;

/// <summary>Adaptador A2A v1.0: mensagem → serviço de aplicação → Artifact (REQ-A2A-002, REQ-A2A-005).</summary>
internal sealed partial class AgenteA2A(ProcessadorDeSolicitacoes processador, IHttpContextAccessor http, ILogger<AgenteA2A> logger) : IAgentHandler
{
    public const string Rota = "/a2a";
    public const string CaminhoDoCartao = "/.well-known/agent-card.json";

    private const string NomeDoArtefato = "analise-sofia-v1";

    /// <summary>Emite a tarefa já terminada: a primeira resposta é final, mesmo com <c>returnImmediately</c> (REQ-A2A-006).</summary>
    public async Task ExecuteAsync(RequestContext context, AgentEventQueue eventQueue, CancellationToken cancellationToken)
    {
        var texto = context.UserText;
        var correlacao = http.HttpContext?.Response.Headers[CorrelacaoMiddleware.Cabecalho].ToString();
        var desfecho = texto is null
            ? new DesfechoDaSolicitacao(null, "mensagem_sem_texto")
            : await processador.ProcessarAsync(texto, correlacao, cancellationToken);

        var tarefa = new AgentTask { Id = context.TaskId, ContextId = context.ContextId };
        if (desfecho.Resultado is { } resultado)
        {
            tarefa.Artifacts = [Artefato(resultado)];
            tarefa.Status = new TaskStatus { State = TaskState.Completed, Timestamp = DateTimeOffset.UtcNow };
            LogTarefaConcluida(logger, resultado.IdExecucao, resultado.IdSolicitacao, resultado.Status);
        }
        else
        {
            tarefa.Status = new TaskStatus
            {
                State = TaskState.Failed,
                Timestamp = DateTimeOffset.UtcNow,
                Message = new Message
                {
                    Role = Role.Agent,
                    MessageId = Guid.NewGuid().ToString(),
                    Parts = [Part.FromText($"Solicitação rejeitada: {desfecho.MotivoDaRejeicao}.")],
                },
            };
            LogTarefaRejeitada(logger, desfecho.MotivoDaRejeicao!);
        }

        await eventQueue.EnqueueTaskAsync(tarefa, cancellationToken);
        eventQueue.Complete();
    }

    /// <summary>
    /// Cartão 1.1.0 (REQ-A2A-007, REQ-A2A-008; mudança 07, D8): diz à Sofia que documentos o agente analisa e o que
    /// devolve, sem prometer validação que ainda não existe. O identificador da skill é o padrão da turma.
    /// </summary>
    public static AgentCard Cartao(string url) => new()
    {
        Name = ContratoSofiaV1.Agente.Nome,
        Description = "Valida arquivos da Escrituração Contábil Fiscal (ECF) do SPED enviados pela Sofia INFIS e devolve o veredito, o que corrigir e o que não foi verificado.",
        Version = "1.1.0",
        SupportedInterfaces = [new AgentInterface { Url = url, ProtocolBinding = "JSONRPC", ProtocolVersion = "1.0" }],
        Capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false },
        DefaultInputModes = ["text/plain", "application/json"],
        DefaultOutputModes = ["text/plain", "application/json"],
        Skills =
        [
            new AgentSkill
            {
                Id = "analisar_documentos",
                Name = "Validação de arquivos da ECF",
                Description = "Recebe o texto de um arquivo da ECF (Escrituração Contábil Fiscal, do SPED) dos leiautes 7 a 12, anos-calendário 2020 a 2025. "
                    + "Uma ECF é um texto cuja primeira linha começa com |0000|LECF|. Envie uma ECF por pedido, com o arquivo inteiro. "
                    + "O agente identifica o leiaute pelo campo COD_VER e devolve o veredito, os achados com registro, linha e o que corrigir, e o que não foi verificado. "
                    + "Não analisa outros documentos, como NF-e, ECD ou EFD.",
                Tags = ["infis", "sofia", "ecf", "sped", "escrituracao-contabil-fiscal"],
                Examples =
                [
                    "Valide esta ECF e diga o que precisa ser corrigido.",
                    "Esta ECF do ano-calendário 2025 está correta?",
                    "Quais erros desta ECF impedem a entrega?",
                ],
                InputModes = ["text/plain"],
                OutputModes = ["application/json"],
            },
        ],
    };

    private static Artifact Artefato(ResultadoAnaliseSofia resultado) => new()
    {
        ArtifactId = Guid.NewGuid().ToString(),
        Name = NomeDoArtefato,
        Parts = [Part.FromText(JsonSerializer.Serialize(resultado, ContratoSofiaV1.Json))],
    };

    [LoggerMessage(EventId = 200, Level = LogLevel.Information, Message = "Tarefa A2A concluída: execução {IdExecucao}, solicitação {IdSolicitacao}, status {Status}")]
    private static partial void LogTarefaConcluida(ILogger logger, string idExecucao, string idSolicitacao, string status);

    [LoggerMessage(EventId = 201, Level = LogLevel.Warning, Message = "Tarefa A2A rejeitada: {Motivo}")]
    private static partial void LogTarefaRejeitada(ILogger logger, string motivo);
}
