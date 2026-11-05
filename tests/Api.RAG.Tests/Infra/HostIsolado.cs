using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.RAG.Tests.Infra;

/// <summary>
/// Host isolado com o mesmo registro de opções do serviço: o erro de validação na inicialização chega
/// intacto, sem a corrida de descarte do servidor de testes (REQ-CONFIG-002).
/// </summary>
internal static class HostIsolado
{
    public static async Task<OptionsValidationException> FalharAoIniciar(
        Action<IServiceCollection, IConfiguration> registrar,
        IEnumerable<KeyValuePair<string, string?>> configuracao,
        ProvedorDeLogEmMemoria? logs = null)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        if (logs is not null)
        {
            builder.Logging.AddProvider(logs);
        }

        builder.Configuration.AddInMemoryCollection(configuracao);
        registrar(builder.Services, builder.Configuration);
        using var host = builder.Build();
        return await Assert.ThrowsAsync<OptionsValidationException>(() => host.StartAsync(TestContext.Current.CancellationToken));
    }
}
