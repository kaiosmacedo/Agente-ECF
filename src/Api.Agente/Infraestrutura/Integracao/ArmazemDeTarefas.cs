using A2A;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Agente.Infraestrutura.Integracao;

/// <summary>
/// Tarefas em memória, limitadas em quantidade e tempo, só com status e resultado: o histórico, que traria
/// documentos e o nome do usuário, nunca é guardado (SQ-04). Em instâncias que não duram, <c>GetTask</c> não é
/// garantido: a resposta de <c>SendMessage</c> já é final.
/// </summary>
internal sealed class ArmazemDeTarefas : ITaskStore, IDisposable
{
    private static readonly TimeSpan Retencao = TimeSpan.FromHours(1);

    private readonly MemoryCache tarefas = new(new MemoryCacheOptions { SizeLimit = 1000 });

    public Task<AgentTask?> GetTaskAsync(string taskId, CancellationToken cancellationToken = default) =>
        Task.FromResult(tarefas.Get<AgentTask>(taskId));

    public Task SaveTaskAsync(string taskId, AgentTask task, CancellationToken cancellationToken = default)
    {
        var guardada = new AgentTask { Id = task.Id, ContextId = task.ContextId, Status = task.Status, Artifacts = task.Artifacts };
        tarefas.Set(taskId, guardada, new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = Retencao });
        return Task.CompletedTask;
    }

    public Task DeleteTaskAsync(string taskId, CancellationToken cancellationToken = default)
    {
        tarefas.Remove(taskId);
        return Task.CompletedTask;
    }

    /// <summary>Sem identidade verificada (SQ-12), nenhum chamador pode ver as tarefas dos outros: a lista é sempre vazia.</summary>
    public Task<ListTasksResponse> ListTasksAsync(ListTasksRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ListTasksResponse { Tasks = [], NextPageToken = string.Empty, PageSize = 0, TotalSize = 0 });

    public void Dispose() => tarefas.Dispose();
}
