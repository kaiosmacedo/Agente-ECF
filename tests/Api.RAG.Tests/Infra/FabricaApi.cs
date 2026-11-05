using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace Api.RAG.Tests.Infra;

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
        foreach (var (chave, valor) in GeradorDeTokens.Configuracao)
        {
            builder.UseSetting(chave, valor);
        }

        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        builder.ConfigureTestServices(GeradorDeTokens.ValidarLocalmente);

        foreach (var ajuste in ajustes)
        {
            ajuste(builder);
        }
    }
}
