namespace Api.RAG.Infraestrutura.Autenticacao;

/// <summary>Papéis de aplicação do provedor (claim <c>roles</c>) aceitos pela Api.RAG (ADR 0003).</summary>
internal static class PapeisDoRag
{
    public const string Administrador = "Rag.Administrador";
    public const string ServicoAgente = "Rag.ServicoAgente";
}

/// <summary>Políticas de autorização declaradas pelos endpoints da Api.RAG (REQ-AUTH-002, REQ-AUTH-005).</summary>
internal static class PoliticasDoRag
{
    /// <summary>Administração da base de conhecimento (mudança 05).</summary>
    public const string AdministradorRag = nameof(AdministradorRag);

    /// <summary>Chamadas MCP do serviço do agente (mudança 06): papel de serviço em identidade de aplicação.</summary>
    public const string ServicoAgente = nameof(ServicoAgente);
}
