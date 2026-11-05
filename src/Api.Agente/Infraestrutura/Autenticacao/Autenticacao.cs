using Api.Agente.Infraestrutura.Configuracao;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Api.Agente.Infraestrutura.Autenticacao;

/// <summary>Autenticação por JWT do provedor OIDC e autorização por perfil (ADR 0003; D1 a D3, D7).</summary>
internal static class Autenticacao
{
    public static IServiceCollection AdicionarAutenticacao(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesAutenticacao>(configuracao, OpcoesAutenticacao.Secao);
        servicos.AdicionarOpcoesValidadas<OpcoesAutorizacoes>(configuracao, OpcoesAutorizacoes.Secao);
        servicos.AddSingleton<LeitorDeIdentidade>();

        servicos.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        servicos.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<OpcoesAutenticacao>>((jwt, opcoes) =>
            {
                var autenticacao = opcoes.Value;
                jwt.Authority = autenticacao.Autoridade;
                jwt.Audience = autenticacao.Audiencia;
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters.NameClaimType = LeitorDeIdentidade.ClaimId;
                jwt.TokenValidationParameters.RoleClaimType = LeitorDeIdentidade.ClaimPapeis;
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
            .AddPolicy(PoliticasDoAgente.AdministradorAgente, p => p.RequireRole(PapeisDoAgente.Administrador))
            .AddPolicy(PoliticasDoAgente.OperadorOrquestrador, p => p.RequireRole(PapeisDoAgente.OperadorOrquestrador))
            .AddPolicy(PoliticasDoAgente.ClienteAgente, p => p.RequireRole(PapeisDoAgente.ClienteAgente))
            .AddPolicy(PoliticasDoAgente.UsuarioAnalise, p => p.RequireRole(PapeisDoAgente.UsuarioAnalise))
            .AddPolicy(PoliticasDoAgente.QualquerPerfil, p => p.RequireRole(PapeisDoAgente.PerfilPorPapel.Keys));

        return servicos;
    }

    private static ILogger Logger(HttpContext contexto) =>
        contexto.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Autenticacao).FullName!);

    // Só o tipo da falha: a mensagem da biblioteca e o token nunca vão para o log (REQ-AUTH-006).
    private static string MotivoDaFalha(JwtBearerChallengeContext contexto) =>
        contexto.AuthenticateFailure?.GetType().Name
        ?? (contexto.Request.Headers.Authorization.Count == 0 ? "CredencialAusente" : "CredencialInvalida");
}
