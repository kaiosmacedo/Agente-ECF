using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Api.Agente.Infraestrutura.Autenticacao;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Agente.Tests;

public sealed class PerfisTests
{
    [Fact(DisplayName = "REQ-AUTH-007 Credencial só com perfil desconhecido recebe 403")]
    public async Task PerfilDesconhecido()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = GeradorDeTokens.Requisicao("/identidade", GeradorDeTokens.Criar(papeis: ["perfil-desconhecido"]));

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact(DisplayName = "REQ-AUTH-007 Perfil reconhecido aparece na consulta da identidade")]
    public async Task PerfilReconhecido()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = GeradorDeTokens.Requisicao("/identidade", GeradorDeTokens.Criar(papeis: [PapeisDoAgente.ClienteAgente, "perfil-desconhecido"]));

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var perfil = Assert.Single(json.RootElement.GetProperty("perfis").EnumerateArray());
        Assert.Equal("cliente-agente", perfil.GetString());
    }

    [Fact(DisplayName = "REQ-AUTH-002 Cliente agente não satisfaz a política de administrador")]
    public async Task ClienteAgenteSemPerfilDeAdministrador()
    {
        using var fabrica = new FabricaApi();
        var autorizacao = fabrica.Services.GetRequiredService<IAuthorizationService>();

        var cliente = await autorizacao.AuthorizeAsync(Principal(PapeisDoAgente.ClienteAgente), PoliticasDoAgente.AdministradorAgente);
        var administrador = await autorizacao.AuthorizeAsync(Principal(PapeisDoAgente.Administrador), PoliticasDoAgente.AdministradorAgente);

        Assert.False(cliente.Succeeded);
        Assert.True(administrador.Succeeded);
    }

    private static ClaimsPrincipal Principal(params string[] papeis) => new(new ClaimsIdentity(
        [new Claim(LeitorDeIdentidade.ClaimId, "identidade-teste"), .. papeis.Select(p => new Claim(LeitorDeIdentidade.ClaimPapeis, p))],
        authenticationType: "Bearer",
        nameType: LeitorDeIdentidade.ClaimId,
        roleType: LeitorDeIdentidade.ClaimPapeis));
}
