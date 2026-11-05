using System.Text.Json;
using A2A;
using Api.Agente.Aplicacao.ContratoSofia;
using Microsoft.Extensions.Options;

namespace Api.Agente.Infraestrutura.Integracao;

/// <summary>Adaptador A2A v0.3.x: mensagem → serviço de aplicação → Artifact (REQ-A2A-002, REQ-A2A-005).</summary>
internal sealed partial class AgenteA2A(
    ProcessadorDeSolicitacoes processador, ITaskStore tarefas, IOptions<OpcoesA2A> opcoes, ILogger<AgenteA2A> logger)
{
    public const string Rota = "/a2a";
    public const string CaminhoDoCartao = "/.well-known/agent-card.json";

    private const string NomeDoArtefato = "analise-sofia-v1";

    public async Task AoCriarTarefa(AgentTask tarefa, CancellationToken cancelamento)
    {
        var texto = tarefa.History?.LastOrDefault()?.Parts.OfType<TextPart>().FirstOrDefault()?.Text;

        // O histórico traz os documentos e o nome do usuário: não fica em memória nem volta na resposta (SQ-04).
        tarefa.History = null;

        var desfecho = texto is null ? new DesfechoDaSolicitacao(null, "mensagem_sem_texto") : processador.Processar(texto);

        if (desfecho.Resultado is { } resultado)
        {
            tarefa.Artifacts = [Artefato(resultado)];
            tarefa.Status = Status(TaskState.Completed);
            LogTarefaConcluida(logger, resultado.IdExecucao, resultado.IdSolicitacao, resultado.Status);
        }
        else
        {
            tarefa.Status = Status(TaskState.Failed, $"Solicitação rejeitada: {desfecho.MotivoDaRejeicao}.");
            LogTarefaRejeitada(logger, desfecho.MotivoDaRejeicao!);
        }

        await tarefas.SetTaskAsync(tarefa, cancelamento);
    }

    public Task<AgentCard> AoConsultarCartao(string urlDaRequisicao, CancellationToken cancelamento) =>
        Task.FromResult(new AgentCard
        {
            Name = ContratoSofiaV1.Agente.Nome,
            Description = "Analisa arquivos da Escrituração Contábil Fiscal (ECF) enviados pela Sofia INFIS.",
            Url = opcoes.Value.UrlPublica ?? urlDaRequisicao,
            Version = "1.0.0",
            ProtocolVersion = "0.3.0",
            PreferredTransport = new AgentTransport("JSONRPC"),
            Capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
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
        });

    private static Artifact Artefato(ResultadoAnaliseSofia resultado) => new()
    {
        ArtifactId = Guid.NewGuid().ToString(),
        Name = NomeDoArtefato,
        Parts = [new TextPart { Text = JsonSerializer.Serialize(resultado, ContratoSofiaV1.Json) }],
    };

    private static AgentTaskStatus Status(TaskState estado, string? mensagem = null) => new()
    {
        State = estado,
        Timestamp = DateTimeOffset.UtcNow,
        Message = mensagem is null ? null : new AgentMessage
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            Parts = [new TextPart { Text = mensagem }],
        },
    };

    [LoggerMessage(EventId = 200, Level = LogLevel.Information, Message = "Tarefa A2A concluída: execução {IdExecucao}, solicitação {IdSolicitacao}, status {Status}")]
    private static partial void LogTarefaConcluida(ILogger logger, string idExecucao, string idSolicitacao, string status);

    [LoggerMessage(EventId = 201, Level = LogLevel.Warning, Message = "Tarefa A2A rejeitada: {Motivo}")]
    private static partial void LogTarefaRejeitada(ILogger logger, string motivo);
}
