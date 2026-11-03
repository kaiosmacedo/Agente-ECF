using System.Net;
using System.Security.Cryptography;
using Api.Agente.Infraestrutura.Autenticacao;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Tests.Infra;
using Microsoft.IdentityModel.Tokens;

namespace Api.Agente.Tests;

public sealed class AutenticacaoTests
{
    private static readonly string[] PapelCliente = [PapeisDoAgente.ClienteAgente];

    [Fact(DisplayName = "REQ-AUTH-001 Endpoint funcional sem credencial responde 401")]
    public async Task SemCredencial()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = GeradorDeTokens.Requisicao("/identidade", token: null);

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Theory(DisplayName = "REQ-AUTH-001 Token expirado, com assinatura, emissor ou audiência inválidos responde 401")]
    [InlineData("expirado")]
    [InlineData("assinatura")]
    [InlineData("emissor")]
    [InlineData("audiencia")]
    public async Task TokenInvalido(string caso)
    {
        using var outraChave = RSA.Create(2048);
        var token = caso switch
        {
            "expirado" => GeradorDeTokens.Criar(papeis: PapelCliente, expiraEm: DateTime.UtcNow.AddMinutes(-10)),
            "assinatura" => GeradorDeTokens.Criar(papeis: PapelCliente, chave: new RsaSecurityKey(outraChave)),
            "emissor" => GeradorDeTokens.Criar(papeis: PapelCliente, emissor: "https://outro-emissor.teste/v2.0"),
            _ => GeradorDeTokens.Criar(papeis: PapelCliente, audiencia: "api://outra-api"),
        };
        using var fabrica = new FabricaApi();
        using var requisicao = GeradorDeTokens.Requisicao("/identidade", token);

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Theory(DisplayName = "REQ-AUTH-001 Verificações de saúde continuam anônimas")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task SaudeAnonima(string caminho)
    {
        using var fabrica = new FabricaApi();

        using var resposta = await fabrica.CreateClient().GetAsync(new Uri(caminho, UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact(DisplayName = "REQ-AUTH-001 Autoridade ausente impede a inicialização e cita a opção")]
    public async Task AutoridadeAusente()
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesAutenticacao>(configuracao, OpcoesAutenticacao.Secao),
            [new("Autenticacao:Audiencia", GeradorDeTokens.Audiencia)]);

        Assert.Contains("Autenticacao:Autoridade", erro.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-AUTH-006 Falha de autenticação é registrada com correlação e sem o token")]
    public async Task FalhaRegistradaSemToken()
    {
        using var fabrica = new FabricaApi();
        using var requisicao = GeradorDeTokens.Requisicao("/identidade", "token-invalido-789");
        requisicao.Headers.Add("X-Correlation-Id", "teste-auth-1");

        using var resposta = await fabrica.CreateClient().SendAsync(requisicao, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Contains(fabrica.Logs.Registros, r =>
            r.Categoria == typeof(Autenticacao).FullName
            && r.Mensagem.Contains("Falha de autenticação", StringComparison.Ordinal)
            && r.Escopos.TryGetValue("CorrelationId", out var correlacao) && correlacao == "teste-auth-1");
        Assert.DoesNotContain(fabrica.Logs.Registros, r => r.TextoCompleto.Contains("token-invalido-789", StringComparison.Ordinal));
    }
}
