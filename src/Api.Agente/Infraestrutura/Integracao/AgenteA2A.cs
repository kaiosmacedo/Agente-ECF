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

    public static AgentCard Cartao(string url) => new()
    {
        Name = ContratoSofiaV1.Agente.Nome,
        Description = "Analisa arquivos da Escrituração Contábil Fiscal (ECF) enviados pela Sofia INFIS.",
        Version = "1.0.0",
        SupportedInterfaces = [new AgentInterface { Url = url, ProtocolBinding = "JSONRPC", ProtocolVersion = "1.0" }],
        Capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false },
        DefaultInputModes = ["text/plain", "application/json"],
        DefaultOutputModes = ["text/plain", "application/json"],
        Skills =
        [
            new AgentSkill
            {
                Id = "analisar_documentos",
                Name = "Análise de arquivos da ECF",
                Description = "Analisa o texto de arquivos da ECF enviados pela Sofia INFIS.",
                Tags = ["infis", "sofia", "ecf"],
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
