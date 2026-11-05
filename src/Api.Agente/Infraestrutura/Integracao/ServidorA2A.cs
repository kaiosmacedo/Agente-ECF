using System.Text.Json.Serialization;
using A2A;
using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Infraestrutura.Configuracao;
using Microsoft.Extensions.Options;

namespace Api.Agente.Infraestrutura.Integracao;

/// <summary>Registro do servidor A2A e do serviço de aplicação que ele usa.</summary>
internal static class ServidorA2A
{
    public static IServiceCollection AdicionarServidorA2A(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesA2A>(configuracao, OpcoesA2A.Secao);
        servicos.AddSingleton(provedor => new ProcessadorDeSolicitacoes(provedor.GetRequiredService<IOptions<OpcoesA2A>>().Value.TamanhoMaximo));
        // Campos opcionais sem valor ficam fora do cartão, como no exemplo do perfil da turma.
        servicos.ConfigureHttpJsonOptions(json => json.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
        servicos.AddSingleton<ITaskStore, ArmazemDeTarefas>();
        servicos.AddSingleton<AgenteA2A>();
        servicos.AddSingleton<ITaskManager>(provedor =>
        {
            var agente = provedor.GetRequiredService<AgenteA2A>();
            return new TaskManager(taskStore: provedor.GetRequiredService<ITaskStore>())
            {
                OnTaskCreated = agente.AoCriarTarefa,
                OnAgentCardQuery = agente.AoConsultarCartao,
            };
        });
        return servicos;
    }
}
