using A2A;
using A2A.AspNetCore;
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
        servicos.AddSingleton<ITaskStore, ArmazemDeTarefas>();

        // O SDK lê as capacidades deste cartão; o endereço real vem das opções validadas, abaixo.
        servicos.AddA2AAgent<AgenteA2A>(AgenteA2A.Cartao(new OpcoesA2A().UrlPublica));
        servicos.AddSingleton(provedor => AgenteA2A.Cartao(provedor.GetRequiredService<IOptions<OpcoesA2A>>().Value.UrlPublica));
        return servicos;
    }
}
