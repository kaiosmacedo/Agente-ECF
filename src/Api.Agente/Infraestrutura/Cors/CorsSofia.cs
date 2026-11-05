using System.ComponentModel.DataAnnotations;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Integracao;
using Api.Agente.Infraestrutura.Observabilidade;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace Api.Agente.Infraestrutura.Cors;

/// <summary>Opções da seção <c>Cors</c>. Sem cliente de navegador, a lista fica vazia (REQ-CORS-002).</summary>
internal sealed class OpcoesCors : IValidatableObject
{
    public const string Secao = "Cors";

    public string[] AllowedOrigins { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        for (var i = 0; i < AllowedOrigins.Length; i++)
        {
            if (!OrigemExplicita(AllowedOrigins[i]))
            {
                yield return new ValidationResult("Origem inválida.", [$"{nameof(AllowedOrigins)}:{i}"]);
            }
        }
    }

    // Origem = esquema + host + porta, sem caminho, barra final ou curinga (REQ-CORS-003).
    private static bool OrigemExplicita(string origem) =>
        !origem.Contains('*', StringComparison.Ordinal)
        && Uri.TryCreate(origem, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && string.IsNullOrEmpty(uri.UserInfo)
        && string.Equals(uri.GetLeftPart(UriPartial.Authority), origem, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Política <c>SofiaBrowser</c>: vale para navegadores; chamadas entre servidores não dependem dela (SQ-13).</summary>
internal static class CorsSofia
{
    public const string Politica = "SofiaBrowser";

    public static IServiceCollection AdicionarCorsSofia(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesCors>(configuracao, OpcoesCors.Secao);
        servicos.AddCors();
        servicos.AddOptions<CorsOptions>().Configure<IOptions<OpcoesCors>>((cors, opcoes) => cors.AddPolicy(Politica, politica => politica
            .WithOrigins(opcoes.Value.AllowedOrigins)
            .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Options)
            .WithHeaders("Content-Type", "Authorization", CorrelacaoMiddleware.Cabecalho)
            .WithExposedHeaders(CorrelacaoMiddleware.Cabecalho)));
        return servicos;
    }

    /// <summary>Só o cartão e o endpoint A2A: <c>RequireCors</c> por endpoint não responde a preflight sozinho.</summary>
    public static IApplicationBuilder UsarCorsSofia(this IApplicationBuilder app) =>
        app.UseWhen(
            contexto => contexto.Request.Path.StartsWithSegments(AgenteA2A.Rota, StringComparison.Ordinal)
                || contexto.Request.Path.Equals(AgenteA2A.CaminhoDoCartao, StringComparison.Ordinal),
            ramo => ramo.UseCors(Politica));
}
