using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Api.Agente.Tests.Infra;

/// <summary>Serviço em memória com autenticação de teste, captura de logs e ajustes por teste.</summary>
internal sealed class FabricaApi : WebApplicationFactory<Program>
{
    private readonly List<Action<IWebHostBuilder>> ajustes = [];

    public ProvedorDeLogEmMemoria Logs { get; } = new();

    public FabricaApi Com(Action<IWebHostBuilder> ajuste)
    {
        ajustes.Add(ajuste);
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Autenticacao:Autoridade", GeradorDeTokens.Emissor);
        builder.UseSetting("Autenticacao:Audiencia", GeradorDeTokens.Audiencia);
        builder.UseSetting("Autorizacoes:Identidades:0:Id", "sofia-infis");
        builder.UseSetting("Autorizacoes:Identidades:0:Nome", "Sofia INFIS");
        builder.UseSetting("Autorizacoes:Identidades:0:Organizacoes:0", "org-a");
        builder.UseSetting("Autorizacoes:Identidades:0:Organizacoes:1", "org-b");
        builder.UseSetting("Autorizacoes:Identidades:1:Id", "usuario-unico");
        builder.UseSetting("Autorizacoes:Identidades:1:Organizacoes:0", "org-a");

        builder.ConfigureLogging(logging => logging.AddProvider(Logs));

        // Valida os tokens de teste localmente, sem buscar metadados do provedor (D8).
        builder.ConfigureTestServices(servicos => servicos.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, jwt =>
        {
            jwt.Authority = null;
            jwt.Configuration = new OpenIdConnectConfiguration { Issuer = GeradorDeTokens.Emissor };
            jwt.TokenValidationParameters.ValidIssuer = GeradorDeTokens.Emissor;
            jwt.TokenValidationParameters.IssuerSigningKey = GeradorDeTokens.Chave;
        }));

        foreach (var ajuste in ajustes)
        {
            ajuste(builder);
        }
    }
}
