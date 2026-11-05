using System.Net;
using System.Text.Json;
using Api.Agente.Infraestrutura.Autenticacao;
using Api.Agente.Tests.Infra;

namespace Api.Agente.Tests;

public sealed class IdentidadeTests
{
    [Fact(DisplayName = "REQ-AUTH-008 Sofia confere identidade, perfis e organização resolvida, sem o token na resposta")]
    public async Task SofiaConfereAConfiguracao()
    {
        using var fabrica = new FabricaApi();
        var token = GeradorDeTokens.Criar(papeis: [PapeisDoAgente.ClienteAgente]);
        using var requisicao = GeradorDeTokens.Requisicao("/identidade", token, organizacao: "org-a");

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(corpo);
        var raiz = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("sofia-infis", raiz.GetProperty("id").GetString());
        Assert.Equal("aplicacao", raiz.GetProperty("tipo").GetString());
        Assert.Equal(["cliente-agente"], raiz.GetProperty("perfis").EnumerateArray().Select(p => p.GetString()));
        Assert.Equal(["org-a", "org-b"], raiz.GetProperty("organizacoes").EnumerateArray().Select(o => o.GetString()));
        Assert.Equal("org-a", raiz.GetProperty("organizacaoResolvida").GetString());
        Assert.DoesNotContain(token, corpo, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-AUTH-008 Sem cabeçalho e com várias organizações, a consulta não resolve nenhuma")]
    public async Task SemCabecalhoComVariasOrganizacoes()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = GeradorDeTokens.Requisicao("/identidade", GeradorDeTokens.Criar(papeis: [PapeisDoAgente.ClienteAgente]));

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("organizacaoResolvida").ValueKind);
    }
}
