using System.Collections.Concurrent;
using Api.Agente.Aplicacao.Execucoes;

namespace Api.Agente.Tests.Infra;

/// <summary>
/// Repositório em memória para os testes de contrato e HTTP, que não tratam do banco. A persistência real
/// é coberta com PostgreSQL em contêiner (SQ-09), em <c>PersistenciaTests</c>.
/// </summary>
internal sealed class RepositorioEmMemoria : IRepositorioDeExecucoes
{
    private readonly ConcurrentDictionary<Guid, EnvelopeDoResultado> execucoes = new();

    /// <summary>Quando verdadeiro, simula o banco fora do ar.</summary>
    public bool Indisponivel { get; set; }

    public Task<EnvelopeDoResultado?> BuscarAsync(Guid idExecucao, CancellationToken cancelamento)
    {
        LancarSeIndisponivel();
        return Task.FromResult(execucoes.Values.FirstOrDefault(envelope => envelope.IdExecucao == idExecucao));
    }

    public Task<EnvelopeDoResultado?> BuscarPorSolicitacaoAsync(Guid idSolicitacao, CancellationToken cancelamento)
    {
        LancarSeIndisponivel();
        return Task.FromResult(execucoes.GetValueOrDefault(idSolicitacao));
    }

    public Task<EnvelopeDoResultado> RegistrarAsync(EnvelopeDoResultado envelope, CancellationToken cancelamento)
    {
        LancarSeIndisponivel();
        return Task.FromResult(execucoes.GetOrAdd(envelope.IdSolicitacao, envelope));
    }

    public Task<bool> DisponivelAsync(CancellationToken cancelamento) => Task.FromResult(!Indisponivel);

    private void LancarSeIndisponivel()
    {
        if (Indisponivel)
        {
            throw new FalhaDoRepositorio(new TimeoutException("Banco indisponível no teste."));
        }
    }
}
