using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace Api.Agente.Tests.Infra;

/// <summary>Serviço em memória com captura de logs e ajustes por teste.</summary>
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
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        foreach (var ajuste in ajustes)
        {
            ajuste(builder);
        }
    }
}
