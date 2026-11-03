using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Api.Agente.Tests.Infra;

/// <summary>Emite JWTs de teste assinados por chave criada na execução, sem depender do provedor real (D8).</summary>
internal static class GeradorDeTokens
{
    public const string Emissor = "https://emissor.teste/v2.0";
    public const string Audiencia = "api://agente-ecf-agente-teste";

    public static RsaSecurityKey Chave { get; } = new(RSA.Create(2048)) { KeyId = "chave-de-teste" };

    public static string Criar(
        string id = "sofia-infis",
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

    public static HttpRequestMessage Requisicao(string caminho, string? token, string? organizacao = null)
    {
        var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri(caminho, UriKind.Relative));
        if (token is not null)
        {
            requisicao.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
        }

        if (organizacao is not null)
        {
            requisicao.Headers.TryAddWithoutValidation("X-Organizacao-Id", organizacao);
        }

        return requisicao;
    }
}
