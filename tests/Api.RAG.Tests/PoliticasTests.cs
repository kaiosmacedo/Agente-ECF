using System.Net;
using Api.RAG.Infraestrutura.Autenticacao;
using Api.RAG.Tests.Infra;

namespace Api.RAG.Tests;

public sealed class PoliticasTests
{
    [Fact(DisplayName = "REQ-AUTH-005 Chamada MCP sem credencial de serviço é rejeitada sem executar a ferramenta")]
    public async Task McpSemCredencial()
    {
        var (status, execucoes) = await Chamar(RotasDeTeste.FerramentaMcp, token: null);

        Assert.Equal(HttpStatusCode.Unauthorized, status);
        Assert.Equal(0, execucoes);
    }

    [Fact(DisplayName = "REQ-AUTH-005 Aplicação sem o papel de serviço recebe 403 na chamada MCP")]
    public async Task McpSemPapelDeServico()
    {
        var (status, execucoes) = await Chamar(RotasDeTeste.FerramentaMcp, GeradorDeTokens.Criar(papeis: [PapeisDoRag.Administrador]));

        Assert.Equal(HttpStatusCode.Forbidden, status);
        Assert.Equal(0, execucoes);
    }

    [Fact(DisplayName = "REQ-AUTH-005 Pessoa com o papel de serviço recebe 403: o MCP exige identidade de aplicação")]
    public async Task McpComPessoa()
    {
        var (status, execucoes) = await Chamar(RotasDeTeste.FerramentaMcp, GeradorDeTokens.Criar(id: "kaio-macedo", tipo: null, papeis: [PapeisDoRag.ServicoAgente]));

        Assert.Equal(HttpStatusCode.Forbidden, status);
        Assert.Equal(0, execucoes);
    }

    [Fact(DisplayName = "REQ-AUTH-005 Api.Agente com o papel de serviço executa a chamada MCP")]
    public async Task McpComPapelDeServico()
    {
        var (status, execucoes) = await Chamar(RotasDeTeste.FerramentaMcp, GeradorDeTokens.Criar(papeis: [PapeisDoRag.ServicoAgente]));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(1, execucoes);
    }

    [Fact(DisplayName = "REQ-AUTH-002 Sofia só com o perfil cliente agente recebe 403 na ingestão, sem ingerir")]
    public async Task ClienteAgenteNaIngestao()
    {
        await using var rotas = await RotasDeTeste.Iniciar();
        using var requisicao = GeradorDeTokens.Requisicao(HttpMethod.Post, RotasDeTeste.Ingestao, GeradorDeTokens.Criar(id: "sofia-infis", papeis: ["Agente.ClienteAgente"]));

        using var resposta = await rotas.CriarCliente().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        Assert.Equal(0, rotas.Execucoes);
        Assert.Contains(rotas.Logs.Registros, r => r.Mensagem == $"Acesso negado por perfil em {RotasDeTeste.Ingestao}");
    }

    [Fact(DisplayName = "REQ-AUTH-002 Administrador da base executa a ingestão")]
    public async Task AdministradorNaIngestao()
    {
        var (status, execucoes) = await Chamar(RotasDeTeste.Ingestao, GeradorDeTokens.Criar(id: "kaio-macedo", tipo: null, papeis: [PapeisDoRag.Administrador]));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(1, execucoes);
    }

    private static async Task<(HttpStatusCode Status, int Execucoes)> Chamar(string rota, string? token)
    {
        await using var rotas = await RotasDeTeste.Iniciar();
        using var requisicao = GeradorDeTokens.Requisicao(HttpMethod.Post, rota, token);

        using var resposta = await rotas.CriarCliente().SendAsync(requisicao, TestContext.Current.CancellationToken);
        return (resposta.StatusCode, rotas.Execucoes);
    }
}
