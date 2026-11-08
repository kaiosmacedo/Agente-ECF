using Api.Agente.Aplicacao.Conhecimento;
using Api.Agente.Aplicacao.Execucoes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Agente.Tests.Infra;

/// <summary>A consulta ao manual desligada, como em produção enquanto a chave da OpenAI está em aberto.</summary>
internal sealed class ConsultaDesativada : IConsultaAoManual
{
    public Task<RespostaDaConsulta> BuscarAsync(PedidoDeTrechos pedido, CancellationToken cancelamento) =>
        Task.FromResult(RespostaDaConsulta.Sem(DesfechoDaConsulta.BuscaDesativada));
}

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
        builder.UseSetting("ConnectionStrings:Execucoes", conexaoReal ?? "Host=banco-de-teste-nao-usado");
        if (conexaoReal is null)
        {
            builder.ConfigureTestServices(servicos => servicos.AddSingleton<IRepositorioDeExecucoes>(new RepositorioEmMemoria()));
        }

        // Como em produção hoje: a busca no manual responde desativada, sem rede (mudança 06, E04).
        builder.ConfigureTestServices(servicos => servicos.AddSingleton<IConsultaAoManual>(new ConsultaDesativada()));
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        foreach (var ajuste in ajustes)
        {
            ajuste(builder);
        }
    }
}
