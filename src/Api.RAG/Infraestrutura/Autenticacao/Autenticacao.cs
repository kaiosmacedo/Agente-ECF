using Api.RAG.Infraestrutura.Configuracao;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Api.RAG.Infraestrutura.Autenticacao;

/// <summary>Autenticação por JWT do provedor OIDC e autorização por perfil na Api.RAG (ADR 0003; D1 a D3, D6, D7).</summary>
internal static class Autenticacao
{
    public const string ClaimPapeis = "roles";
    public const string ClaimTipo = "idtyp";

    public static IServiceCollection AdicionarAutenticacao(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesAutenticacao>(configuracao, OpcoesAutenticacao.Secao);

        servicos.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        servicos.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<OpcoesAutenticacao>>((jwt, opcoes) =>
            {
                var autenticacao = opcoes.Value;
                jwt.Authority = autenticacao.Autoridade;
                jwt.Audience = autenticacao.Audiencia;
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters.NameClaimType = "oid";
                jwt.TokenValidationParameters.RoleClaimType = ClaimPapeis;
                if (autenticacao.EmissoresValidos.Length > 0)
                {
                    jwt.TokenValidationParameters.ValidIssuers = autenticacao.EmissoresValidos;
                }

                jwt.Events = new JwtBearerEvents
                {
                    OnChallenge = contexto =>
                    {
                        RegistroDeAcesso.FalhaDeAutenticacao(Logger(contexto.HttpContext), MotivoDaFalha(contexto), contexto.Request.Path.Value ?? "/");
                        return Task.CompletedTask;
                    },
                    OnForbidden = contexto =>
                    {
                        RegistroDeAcesso.AcessoNegado(Logger(contexto.HttpContext), contexto.Request.Path.Value ?? "/");
                        return Task.CompletedTask;
                    },
                };
            });

        servicos.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(PoliticasDoRag.AdministradorRag, p => p.RequireRole(PapeisDoRag.Administrador))
            .AddPolicy(PoliticasDoRag.ServicoAgente, p => p.RequireRole(PapeisDoRag.ServicoAgente).RequireClaim(ClaimTipo, "app"));

        return servicos;
    }

    private static ILogger Logger(HttpContext contexto) =>
        contexto.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Autenticacao).FullName!);

    // Só o tipo da falha: a mensagem da biblioteca e o token nunca vão para o log (REQ-AUTH-006).
    private static string MotivoDaFalha(JwtBearerChallengeContext contexto) =>
        contexto.AuthenticateFailure?.GetType().Name
        ?? (contexto.Request.Headers.Authorization.Count == 0 ? "CredencialAusente" : "CredencialInvalida");
}
