namespace Api.Agente.Aplicacao.Identidade;

/// <summary>Perfis do Agente-ECF (REQ-AUTH-007).</summary>
internal enum Perfil
{
    AdministradorAgente,
    OperadorOrquestrador,
    ClienteAgente,
    UsuarioAnalise,
}

/// <summary>Tipo da identidade autenticada: pessoa ou aplicação (agente ou serviço).</summary>
internal enum TipoIdentidade
{
    Pessoa,
    Aplicacao,
}

/// <summary>
/// Quem chama o agente, obtido somente da credencial (REQ-AUTH-003), com as organizações em que pode atuar.
/// </summary>
internal sealed record IdentidadeChamador(
    string Id, TipoIdentidade Tipo, IReadOnlySet<Perfil> Perfis, IReadOnlySet<string> Organizacoes)
{
    /// <summary>Indica se a identidade pode acessar recursos da organização (REQ-AUTH-004).</summary>
    public bool PodeAcessar(string organizacao) => Organizacoes.Contains(organizacao);
}
