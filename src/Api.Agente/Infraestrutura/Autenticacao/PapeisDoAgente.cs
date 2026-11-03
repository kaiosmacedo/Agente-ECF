using Api.Agente.Aplicacao.Identidade;

namespace Api.Agente.Infraestrutura.Autenticacao;

/// <summary>Papéis de aplicação do provedor (claim <c>roles</c>) e o perfil de cada um (ADR 0003).</summary>
internal static class PapeisDoAgente
{
    public const string Administrador = "Agente.Administrador";
    public const string OperadorOrquestrador = "Agente.OperadorOrquestrador";
    public const string ClienteAgente = "Agente.ClienteAgente";
    public const string UsuarioAnalise = "Agente.UsuarioAnalise";

    public static readonly IReadOnlyDictionary<string, Perfil> PerfilPorPapel = new Dictionary<string, Perfil>
    {
        [Administrador] = Perfil.AdministradorAgente,
        [OperadorOrquestrador] = Perfil.OperadorOrquestrador,
        [ClienteAgente] = Perfil.ClienteAgente,
        [UsuarioAnalise] = Perfil.UsuarioAnalise,
    };
}

/// <summary>Políticas de autorização declaradas pelos endpoints (REQ-AUTH-002).</summary>
internal static class PoliticasDoAgente
{
    public const string AdministradorAgente = nameof(AdministradorAgente);
    public const string OperadorOrquestrador = nameof(OperadorOrquestrador);
    public const string ClienteAgente = nameof(ClienteAgente);
    public const string UsuarioAnalise = nameof(UsuarioAnalise);

    /// <summary>Qualquer perfil reconhecido; papéis desconhecidos não bastam (REQ-AUTH-007).</summary>
    public const string QualquerPerfil = nameof(QualquerPerfil);
}
