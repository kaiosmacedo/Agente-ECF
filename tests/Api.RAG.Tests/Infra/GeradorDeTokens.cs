using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Api.RAG.Tests.Infra;

/// <summary>Emite JWTs de teste assinados por chave criada na execução, sem depender do provedor real (D8).</summary>
internal static class GeradorDeTokens
{
    public const string Emissor = "https://emissor.teste/v2.0";
    public const string Audiencia = "api://agente-ecf-rag-teste";

    public static RsaSecurityKey Chave { get; } = new(RSA.Create(2048)) { KeyId = "chave-de-teste" };

    public static IEnumerable<KeyValuePair<string, string?>> Configuracao { get; } =
    [
        new("Autenticacao:Autoridade", Emissor),
        new("Autenticacao:Audiencia", Audiencia),
    ];

    public static string Criar(
        string id = "api-agente",
        string? tipo = "app",
        string[]? papeis = null,
        DateTime? expiraEm = null,
        string emissor = Emissor,
        string audiencia = Audiencia,
        SecurityKey? chave = null)
    {
        var claims = new Dictionary<string, object> { ["oid"] = id, ["roles"] = papeis ?? [] };
        if (tipo is not null)
        {
            claims["idtyp"] = tipo;
        }

        var agora = DateTime.UtcNow;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = emissor,
            Audience = audiencia,
            Claims = claims,
            NotBefore = agora.AddHours(-1),
            IssuedAt = agora.AddHours(-1),
            Expires = expiraEm ?? agora.AddMinutes(30),
            SigningCredentials = new SigningCredentials(chave ?? Chave, SecurityAlgorithms.RsaSha256),
        });
    }

    public static HttpRequestMessage Requisicao(HttpMethod metodo, string caminho, string? token)
    {
        var requisicao = new HttpRequestMessage(metodo, new Uri(caminho, UriKind.Relative));
        if (token is not null)
        {
            requisicao.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
        }

        return requisicao;
    }

    /// <summary>Valida os tokens de teste localmente, sem buscar metadados do provedor (D8).</summary>
    public static void ValidarLocalmente(IServiceCollection servicos) =>
        servicos.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, jwt =>
        {
            jwt.Authority = null;
            jwt.Configuration = new OpenIdConnectConfiguration { Issuer = Emissor };
            jwt.TokenValidationParameters.ValidIssuer = Emissor;
            jwt.TokenValidationParameters.IssuerSigningKey = Chave;
        });
}
