namespace Api.RAG.Infraestrutura.Autenticacao;

/// <summary>Registros de falhas de autenticação e autorização, sem credenciais (REQ-AUTH-006).</summary>
internal static partial class RegistroDeAcesso
{
    [LoggerMessage(EventId = 100, Level = LogLevel.Warning, Message = "Falha de autenticação: {Motivo} em {Rota}")]
    public static partial void FalhaDeAutenticacao(ILogger logger, string motivo, string rota);

    [LoggerMessage(EventId = 101, Level = LogLevel.Warning, Message = "Acesso negado por perfil em {Rota}")]
    public static partial void AcessoNegado(ILogger logger, string rota);
}
