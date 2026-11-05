using Api.RAG.Aplicacao.Consumo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.RAG.Tests.Infra;

/// <summary>Serviço em memória com captura de logs e ajustes por teste.</summary>
internal sealed class FabricaApi(string? conexaoReal = null) : WebApplicationFactory<Program>
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
        builder.UseSetting("ConnectionStrings:Conhecimento", conexaoReal ?? "Host=banco-de-teste-nao-usado");
        if (conexaoReal is null)
        {
            builder.ConfigureTestServices(servicos => servicos.AddSingleton<IRepositorioDeConsumo>(new RepositorioDeConsumoEmMemoria()));
        }

        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        foreach (var ajuste in ajustes)
        {
            ajuste(builder);
        }
    }
}
