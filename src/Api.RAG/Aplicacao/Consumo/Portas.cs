using Api.RAG.Dominio.Consumo;

namespace Api.RAG.Aplicacao.Consumo;

/// <summary>Filtros da consulta agregada de consumo (REQ-CONSUMO-005).</summary>
internal sealed record FiltroDeConsumo(DateTimeOffset? De, DateTimeOffset? Ate, string? Modelo, Guid? IdOperacao);

/// <summary>Onde o consumo fica guardado. Falhas viram <see cref="FalhaDoRepositorio"/>.</summary>
internal interface IRepositorioDeConsumo
{
    Task RegistrarAsync(RegistroDeConsumo registro, CancellationToken cancelamento);

    Task<IReadOnlyList<TotalDoModelo>> AgregarAsync(FiltroDeConsumo filtro, CancellationToken cancelamento);

    Task<bool> DisponivelAsync(CancellationToken cancelamento);

    /// <summary>Consultas de busca feitas desde o instante dado, contando só a primeira tentativa (mudança 06, D5).</summary>
    Task<long> ContarConsultasDesdeAsync(DateTimeOffset desde, CancellationToken cancelamento);
}

/// <summary>O banco não respondeu. A mensagem não traz detalhes da conexão (SQ-03).</summary>
internal sealed class FalhaDoRepositorio(Exception causa) : Exception("O banco não respondeu.", causa);

/// <summary>A que operação uma chamada pertence: a execução ou a ingestão que a originou.</summary>
internal sealed record ContextoDaChamada(Guid? IdOperacao, string? Correlacao, FinalidadeDaChamada? Finalidade = null);

/// <summary>Resposta de uma chamada de embeddings. Sem contagem do provedor, os tokens ficam nulos (REQ-CONSUMO-002).</summary>
internal sealed record RespostaDeEmbeddings(IReadOnlyList<float[]> Vetores, int? TokensEntrada);

/// <summary>Uma única tentativa no provedor, sem retentativas, para cada uma poder ser registrada (D4).</summary>
internal interface IProvedorDeEmbeddings
{
    string Provedor { get; }

    string Modelo { get; }

    Task<RespostaDeEmbeddings> GerarAsync(IReadOnlyList<string> textos, CancellationToken cancelamento);
}

/// <summary>O provedor falhou em todas as tentativas. A mensagem não traz credenciais nem conteúdo (SQ-03).</summary>
internal sealed class FalhaDoProvedor(string mensagem, Exception? causa = null) : Exception(mensagem, causa);

/// <summary>Quem precisa de embeddings usa este ponto, que repete as falhas e registra cada tentativa (D4).</summary>
internal interface IClienteDeEmbeddings
{
    string Modelo { get; }

    Task<RespostaDeEmbeddings> GerarAsync(IReadOnlyList<string> textos, ContextoDaChamada contexto, CancellationToken cancelamento);
}
