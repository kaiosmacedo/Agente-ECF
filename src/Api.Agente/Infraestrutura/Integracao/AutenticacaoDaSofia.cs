using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Primitives;

namespace Api.Agente.Infraestrutura.Integracao;

/// <summary>
/// Chave Bearer compartilhada da Sofia central no cartão e no RPC do A2A (REQ-A2A-003, REQ-A2A-010; skill
/// <c>sofia-a2a-autenticacao</c>). Sem a chave configurada, a integração fica indisponível, nunca aberta. O valor da chave
/// não aparece em log, resposta nem cartão (SQ-03).
/// </summary>
internal sealed partial class AutenticacaoDaSofia(RequestDelegate proximo, IConfiguration configuracao, ILogger<AutenticacaoDaSofia> logger)
{
    /// <summary>Nome da variável, o mesmo que a Sofia central usa como referência da credencial de saída.</summary>
    public const string VariavelDaChave = "SOFIA_A2A_TOKEN";

    private const string Esquema = "Bearer ";

    /// <summary>O cartão (também servido em <c>/</c> pelo pacote de compatibilidade) e o RPC.</summary>
    public static bool Protegido(PathString caminho) =>
        caminho == "/" || caminho.StartsWithSegments(AgenteA2A.Rota) || caminho.StartsWithSegments(AgenteA2A.CaminhoDoCartao);

    public async Task InvokeAsync(HttpContext contexto)
    {
        // O preflight de CORS não traz credencial; a política SofiaBrowser responde antes (SQ-13).
        if (HttpMethods.IsOptions(contexto.Request.Method))
        {
            await proximo(contexto);
            return;
        }

        var esperada = configuracao[VariavelDaChave];
        if (string.IsNullOrWhiteSpace(esperada))
        {
            LogIndisponivel(logger, contexto.Request.Path);
            await Responder(contexto, StatusCodes.Status503ServiceUnavailable, "integracao_indisponivel", "A integração A2A está indisponível: a credencial do serviço não está configurada.");
            return;
        }

        if (!Valida(contexto.Request.Headers.Authorization, esperada))
        {
            LogRecusada(logger, contexto.Request.Path);
            contexto.Response.Headers.WWWAuthenticate = "Bearer";
            await Responder(contexto, StatusCodes.Status401Unauthorized, "nao_autorizado", "Credencial ausente ou inválida.");
            return;
        }

        await proximo(contexto);
    }

    /// <summary>
    /// Um único cabeçalho <c>Bearer</c> com a chave exata, sem normalizar maiúsculas. Os dois lados passam por SHA-256 antes
    /// da comparação em tempo constante, que assim não revela nem o tamanho da chave.
    /// </summary>
    public static bool Valida(StringValues cabecalhos, string esperada)
    {
        if (cabecalhos.Count != 1 || cabecalhos[0] is not { } valor || !valor.StartsWith(Esquema, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var recebida = valor[Esquema.Length..];
        return recebida.Length > 0 && CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(recebida)),
            SHA256.HashData(Encoding.UTF8.GetBytes(esperada)));
    }

    private static Task Responder(HttpContext contexto, int status, string codigo, string mensagem)
    {
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = "application/json; charset=utf-8";
        return contexto.Response.WriteAsync(JsonSerializer.Serialize(new { erro = new { codigo, mensagem } }));
    }

    [LoggerMessage(EventId = 210, Level = LogLevel.Warning, Message = "Chamada A2A recusada em {Caminho}: credencial ausente ou inválida")]
    private static partial void LogRecusada(ILogger logger, string caminho);

    [LoggerMessage(EventId = 211, Level = LogLevel.Error, Message = "Integração A2A indisponível em {Caminho}: a variável SOFIA_A2A_TOKEN não está configurada")]
    private static partial void LogIndisponivel(ILogger logger, string caminho);
}
