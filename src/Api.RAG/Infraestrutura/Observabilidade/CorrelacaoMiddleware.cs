using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

namespace Api.RAG.Infraestrutura.Observabilidade;

/// <summary>
/// Associa um identificador de correlação a cada requisição (REQ-OBSERV-001) e o inclui no
/// escopo de todos os logs da requisição, com o nome e a versão do serviço (REQ-OBSERV-002).
/// </summary>
internal sealed partial class CorrelacaoMiddleware(
    RequestDelegate proximo, ILogger<CorrelacaoMiddleware> logger, IdentidadeServico identidade)
{
    public const string Cabecalho = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext contexto)
    {
        var correlacao = ObterOuGerar(contexto.Request.Headers[Cabecalho]);
        contexto.Response.Headers[Cabecalho] = correlacao;

        using var escopo = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlacao,
            ["Servico"] = identidade.Nome,
            ["Versao"] = identidade.Versao,
        });

        var inicio = Stopwatch.GetTimestamp();
        try
        {
            await proximo(contexto);
        }
        finally
        {
            // Só método, caminho, status e duração: nunca corpo, query string ou cabeçalhos (REQ-OBSERV-003).
            if (logger.IsEnabled(LogLevel.Information))
            {
                var metodo = contexto.Request.Method;
                var caminho = contexto.Request.Path.Value ?? "/";
                var duracaoMs = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
                RequisicaoConcluida(logger, metodo, caminho, contexto.Response.StatusCode, duracaoMs);
            }
        }
    }

    /// <summary>Reutiliza o valor recebido somente se for válido; o valor inválido é descartado sem registro.</summary>
    internal static string ObterOuGerar(StringValues recebido) =>
        recebido.Count == 1 && recebido[0] is { } valor && FormatoValido().IsMatch(valor)
            ? valor
            : Guid.NewGuid().ToString("N");

    [GeneratedRegex(@"\A[A-Za-z0-9._-]{1,128}\z")]
    private static partial Regex FormatoValido();

    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "Requisição concluída: {Metodo} {Caminho} {Status} em {DuracaoMs} ms")]
    private static partial void RequisicaoConcluida(ILogger logger, string metodo, string caminho, int status, double duracaoMs);
}
