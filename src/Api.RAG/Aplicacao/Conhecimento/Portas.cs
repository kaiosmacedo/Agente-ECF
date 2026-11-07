using Api.RAG.Dominio.Conhecimento;

namespace Api.RAG.Aplicacao.Conhecimento;

/// <summary>O que identifica um documento oficial (GD-07, REQ-INGEST-001).</summary>
internal sealed record MetadadosDoDocumento(
    string? Emissor,
    string? Titulo,
    string? Tipo,
    string? Versao,
    int? AnoCalendario = null,
    string? VersaoDaPublicacao = null,
    string? Vigencia = null,
    string? Origem = null);

/// <summary>Um documento da base, sem o conteúdo (REQ-BASE-001, 002).</summary>
internal sealed record DocumentoDaBase(
    Guid Id,
    string Emissor,
    string Titulo,
    string Tipo,
    string Versao,
    int? AnoCalendario,
    string? VersaoDaPublicacao,
    string? Vigencia,
    string? Origem,
    string Hash,
    long TamanhoBytes,
    int? Paginas,
    string Estado,
    DateTime IngeridoEm,
    int Chunks,
    string? Modelo,
    int? Dimensao);

/// <summary>Um trecho com o vetor, pronto para gravar.</summary>
internal sealed record TrechoComVetor(Trecho Trecho, string Hash, float[] Vetor);

/// <summary>Um trecho encontrado na busca, com a referência para citar (P06) e se ele contém o código pedido (REQ-RECUP-005).</summary>
internal sealed record TrechoEncontrado(
    Guid IdTrecho,
    Guid IdDocumento,
    string Titulo,
    string Versao,
    int Pagina,
    string? Secao,
    string Texto,
    double Similaridade,
    bool ContemCodigo);

/// <summary>Lê o texto do arquivo, página por página.</summary>
internal interface IExtratorDeTexto
{
    IReadOnlyList<PaginaDoDocumento> Extrair(byte[] conteudo);
}

/// <summary>O arquivo não pôde ser lido como o formato aceito.</summary>
internal sealed class ArquivoIlegivel(Exception causa) : Exception("O arquivo não pôde ser lido.", causa);

/// <summary>Onde a base fica guardada. Falhas viram <c>FalhaDoRepositorio</c>.</summary>
internal interface IRepositorioDeDocumentos
{
    Task<DocumentoDaBase?> BuscarPorHashAsync(string hash, CancellationToken cancelamento);

    Task<DocumentoDaBase?> BuscarAsync(Guid id, CancellationToken cancelamento);

    Task<IReadOnlyList<DocumentoDaBase>> ListarAsync(string? tipo, string? versao, CancellationToken cancelamento);

    /// <summary>Grava o documento como processando, trocando uma tentativa anterior que tenha falhado com o mesmo hash.</summary>
    Task<Guid> IniciarAsync(MetadadosDoDocumento metadados, string hash, long tamanhoBytes, DateTimeOffset quando, CancellationToken cancelamento);

    /// <summary>Em uma transação: grava os trechos, torna o documento vigente e substitui a versão anterior (D4).</summary>
    Task ConcluirAsync(Guid id, MetadadosDoDocumento metadados, int paginas, IReadOnlyList<TrechoComVetor> trechos, string modelo, int dimensao, CancellationToken cancelamento);

    Task MarcarFalhaAsync(Guid id, CancellationToken cancelamento);

    /// <summary>Tira o documento de "vigente", mantendo-o para auditoria. Falso se ele não existir.</summary>
    Task<bool> DesativarAsync(Guid id, CancellationToken cancelamento);

    /// <summary>Apaga o documento e os trechos, para testar outros parâmetros de segmentação (D6). O consumo fica.</summary>
    Task ApagarParaReprocessarAsync(Guid id, CancellationToken cancelamento);

    /// <summary>
    /// Busca só nos vigentes do tipo e da versão pedidos (P11). Com código, os trechos que o contêm vêm primeiro,
    /// completados pelos mais parecidos, sem repetir (mudança 06, D3).
    /// </summary>
    Task<IReadOnlyList<TrechoEncontrado>> BuscarSemelhantesAsync(float[] vetor, string tipo, string versao, int quantidade, CancellationToken cancelamento, string? codigo = null);

    /// <summary>Se há documento vigente do tipo e da versão, para responder "versão indisponível" sem gerar embedding.</summary>
    Task<bool> HaVigenteAsync(string tipo, string versao, CancellationToken cancelamento);
}
