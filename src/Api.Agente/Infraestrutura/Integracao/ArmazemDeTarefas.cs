using A2A;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Agente.Infraestrutura.Integracao;

/// <summary>
/// Tarefas em memória, limitadas em quantidade e tempo. Guarda só status e resultado, sem documentos (SQ-04).
/// Em instâncias que não duram, <c>tasks/get</c> não é garantido: a resposta de <c>message/send</c> já é final.
/// </summary>
internal sealed class ArmazemDeTarefas : ITaskStore, IDisposable
{
    private static readonly TimeSpan Retencao = TimeSpan.FromHours(1);

    private readonly MemoryCache tarefas = new(new MemoryCacheOptions { SizeLimit = 1000 });

    public Task<AgentTask?> GetTaskAsync(string taskId, CancellationToken cancellationToken = default) =>
        Task.FromResult(tarefas.Get<AgentTask>(taskId));

    public Task SetTaskAsync(AgentTask task, CancellationToken cancellationToken = default)
    {
        tarefas.Set(task.Id, task, new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = Retencao });
        return Task.CompletedTask;
    }

    public async Task<AgentTaskStatus> UpdateStatusAsync(string taskId, TaskState status, AgentMessage? message = null, CancellationToken cancellationToken = default)
    {
        var tarefa = await GetTaskAsync(taskId, cancellationToken)
            ?? throw new A2AException("Tarefa não encontrada.", A2AErrorCode.TaskNotFound);
        tarefa.Status = new AgentTaskStatus { State = status, Message = message, Timestamp = DateTimeOffset.UtcNow };
        await SetTaskAsync(tarefa, cancellationToken);
        return tarefa.Status;
    }

    public Task<TaskPushNotificationConfig?> GetPushNotificationAsync(string taskId, string notificationConfigId, CancellationToken cancellationToken = default) =>
        throw NotificacoesNaoSuportadas();

    public Task SetPushNotificationConfigAsync(TaskPushNotificationConfig pushNotificationConfig, CancellationToken cancellationToken = default) =>
        throw NotificacoesNaoSuportadas();

    public Task<IEnumerable<TaskPushNotificationConfig>> GetPushNotificationsAsync(string taskId, CancellationToken cancellationToken = default) =>
        throw NotificacoesNaoSuportadas();

    public void Dispose() => tarefas.Dispose();

    private static A2AException NotificacoesNaoSuportadas() =>
        new("Este agente não oferece notificações.", A2AErrorCode.PushNotificationNotSupported);
}
