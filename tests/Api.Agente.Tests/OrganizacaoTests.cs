using System.Net;
using System.Text.Json;
using Api.Agente.Aplicacao.Identidade;
using Api.Agente.Infraestrutura.Autenticacao;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Tests.Infra;

namespace Api.Agente.Tests;

public sealed class OrganizacaoTests
{
    private static readonly IdentidadeChamador Sofia = Identidade("org-a", "org-b");
    private static readonly string[] PapelCliente = [PapeisDoAgente.ClienteAgente];

    [Fact(DisplayName = "REQ-AUTH-003 Agente autorizado para várias organizações atua pela informada")]
    public void VariasAutorizadasComOrganizacaoInformada() =>
        Assert.Equal(new ResultadoDaOrganizacao(SituacaoDaOrganizacao.Resolvida, "org-b"), ResolvedorDeOrganizacao.Resolver(Sofia, "org-b", obrigatoria: true));

    [Fact(DisplayName = "REQ-AUTH-003 Organização não autorizada é recusada")]
    public void OrganizacaoNaoAutorizada() =>
        Assert.Equal(SituacaoDaOrganizacao.NaoAutorizada, ResolvedorDeOrganizacao.Resolver(Sofia, "org-c", obrigatoria: true).Situacao);

    [Fact(DisplayName = "REQ-AUTH-003 Organização não informada com várias autorizadas é obrigatória")]
    public void OrganizacaoObrigatoria() =>
        Assert.Equal(SituacaoDaOrganizacao.Obrigatoria, ResolvedorDeOrganizacao.Resolver(Sofia, null, obrigatoria: true).Situacao);

    [Fact(DisplayName = "REQ-AUTH-003 Única organização autorizada vale sem cabeçalho")]
    public void UnicaOrganizacao() =>
        Assert.Equal(new ResultadoDaOrganizacao(SituacaoDaOrganizacao.Resolvida, "org-a"), ResolvedorDeOrganizacao.Resolver(Identidade("org-a"), null, obrigatoria: true));

    [Theory(DisplayName = "REQ-AUTH-003 Cabeçalho de organização com formato inválido é recusado")]
    [InlineData("org a")]
    [InlineData("org-a\n")]
    [InlineData("org/../a")]
    public void FormatoInvalido(string informada) =>
        Assert.Equal(SituacaoDaOrganizacao.FormatoInvalido, ResolvedorDeOrganizacao.Resolver(Sofia, informada, obrigatoria: true).Situacao);

    [Fact(DisplayName = "REQ-AUTH-003 Identidade sem organizações não atua em nenhuma")]
    public void SemOrganizacao() =>
        Assert.Equal(SituacaoDaOrganizacao.SemOrganizacao, ResolvedorDeOrganizacao.Resolver(Identidade(), null, obrigatoria: true).Situacao);

    [Fact(DisplayName = "REQ-AUTH-004 Identidade autorizada só para org-b não acessa recurso de org-a")]
    public void IsolamentoPorOrganizacao()
    {
        var identidade = Identidade("org-b");

        Assert.False(identidade.PodeAcessar("org-a"));
        Assert.True(identidade.PodeAcessar("org-b"));
    }

    [Theory(DisplayName = "REQ-AUTH-003 Consulta da identidade aplica as regras da organização")]
    [InlineData("org-c", HttpStatusCode.Forbidden)]
    [InlineData("org a", HttpStatusCode.BadRequest)]
    [InlineData("org-b", HttpStatusCode.OK)]
    public async Task ConsultaComOrganizacao(string organizacao, HttpStatusCode esperado)
    {
        using var fabrica = new FabricaApi();
        using var requisicao = GeradorDeTokens.Requisicao("/identidade", GeradorDeTokens.Criar(papeis: PapelCliente), organizacao);

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal(esperado, resposta.StatusCode);
    }

    [Fact(DisplayName = "REQ-AUTH-003 Identidade informada pelo cliente não substitui a da credencial")]
    public async Task IdentidadeInformadaIgnorada()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = GeradorDeTokens.Requisicao("/identidade?usuario=outro-agente", GeradorDeTokens.Criar(papeis: PapelCliente));

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal("sofia-infis", json.RootElement.GetProperty("id").GetString());
    }

    [Fact(DisplayName = "REQ-AUTH-003 Autorização sem identificador impede a inicialização e cita a posição")]
    public async Task AutorizacaoSemIdentificador()
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesAutorizacoes>(configuracao, OpcoesAutorizacoes.Secao),
            [new("Autorizacoes:Identidades:0:Nome", "Sem identificador"), new("Autorizacoes:Identidades:0:Organizacoes:0", "org a")]);

        Assert.Contains("Autorizacoes:Identidades:0:Id", erro.Message, StringComparison.Ordinal);
        Assert.Contains("Autorizacoes:Identidades:0:Organizacoes:0", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("org a", erro.ToString(), StringComparison.Ordinal);
    }

    private static IdentidadeChamador Identidade(params string[] organizacoes) =>
        new("identidade-teste", TipoIdentidade.Aplicacao, new HashSet<Perfil> { Perfil.ClienteAgente }, organizacoes.ToHashSet());
}
