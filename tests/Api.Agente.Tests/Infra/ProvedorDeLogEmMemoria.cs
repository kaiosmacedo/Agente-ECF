using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Api.Agente.Tests.Infra;

/// <summary>Captura os logs com seus escopos para verificar REQ-OBSERV-002 e REQ-OBSERV-003.</summary>
internal sealed class ProvedorDeLogEmMemoria : ILoggerProvider, ISupportExternalScope
{
    private IExternalScopeProvider escopos = new LoggerExternalScopeProvider();

    public ConcurrentQueue<RegistroDeLog> Registros { get; } = new();

    public ILogger CreateLogger(string categoryName) => new Registrador(this, categoryName);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => escopos = scopeProvider;

    public void Dispose()
    {
    }

    private sealed class Registrador(ProvedorDeLogEmMemoria provedor, string categoria) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => provedor.escopos.Push(state);

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var valores = new Dictionary<string, string?>();
            provedor.escopos.ForEachScope(
                (escopo, acumulado) =>
                {
                    if (escopo is IEnumerable<KeyValuePair<string, object>> pares)
                    {
                        foreach (var (chave, valor) in pares)
                        {
                            acumulado[chave] = valor?.ToString();
                        }
                    }
                    else
                    {
                        acumulado[$"escopo{acumulado.Count}"] = escopo?.ToString();
                    }
                },
                valores);

            provedor.Registros.Enqueue(new RegistroDeLog(categoria, logLevel, formatter(state, exception), exception?.ToString(), valores));
        }
    }
}

internal sealed record RegistroDeLog(
    string Categoria, LogLevel Nivel, string Mensagem, string? Excecao, IReadOnlyDictionary<string, string?> Escopos)
{
    public string TextoCompleto => string.Join(' ', [Mensagem, Excecao, .. Escopos.Select(e => $"{e.Key}={e.Value}")]);
}
