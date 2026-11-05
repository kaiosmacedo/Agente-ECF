namespace Api.Agente.Aplicacao.Execucoes;

/// <summary>Onde as execuções ficam guardadas. Falhas de acesso chegam como <see cref="FalhaDoRepositorio"/>.</summary>
internal interface IRepositorioDeExecucoes
{
    Task<EnvelopeDoResultado?> BuscarAsync(Guid idExecucao, CancellationToken cancelamento);

    Task<EnvelopeDoResultado?> BuscarPorSolicitacaoAsync(Guid idSolicitacao, CancellationToken cancelamento);

    /// <summary>Grava a execução. Se o <c>idSolicitacao</c> já existir, devolve a registrada antes (D7).</summary>
    Task<EnvelopeDoResultado> RegistrarAsync(EnvelopeDoResultado envelope, CancellationToken cancelamento);

    Task<bool> DisponivelAsync(CancellationToken cancelamento);
}

/// <summary>O armazenamento não respondeu. A mensagem é genérica: detalhes de conexão não saem da infraestrutura.</summary>
internal sealed class FalhaDoRepositorio(Exception causa) : Exception("O armazenamento de execuções não respondeu.", causa);
