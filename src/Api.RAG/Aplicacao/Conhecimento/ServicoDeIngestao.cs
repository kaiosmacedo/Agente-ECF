using System.Security.Cryptography;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Conhecimento;
using Api.RAG.Dominio.Consumo;

namespace Api.RAG.Aplicacao.Conhecimento;

/// <summary>Limites e parâmetros da ingestão, vindos da configuração.</summary>
internal sealed record ParametrosDaIngestao(long TamanhoMaximoEmBytes, int TamanhoDoLote, int Dimensao, ParametrosDeSegmentacao Segmentacao);

/// <summary>Por que a ingestão foi recusada antes de processar o conteúdo.</summary>
internal enum MotivoDaRecusa
{
    MetadadoAusente,
    FormatoNaoAceito,
    TamanhoExcedido,
}

/// <summary>Entrada recusada, com o campo quando houver (REQ-INGEST-001, 002).</summary>
internal sealed class IngestaoRecusada(MotivoDaRecusa motivo, string mensagem, string? campo = null) : Exception(mensagem)
{
    public MotivoDaRecusa Motivo { get; } = motivo;

    public string? Campo { get; } = campo;
}

/// <summary>A ingestão não terminou. Nenhum trecho desse documento fica vigente (REQ-INGEST-006).</summary>
internal sealed class FalhaDaIngestao(string mensagem, Exception causa) : Exception(mensagem, causa);

/// <summary>Resultado: documento novo, ou o que já existia com o mesmo conteúdo (REQ-INGEST-003).</summary>
internal sealed record ResultadoDaIngestao(DocumentoDaBase Documento, bool JaExistia);

/// <summary>
/// Ingestão de um documento oficial (design da mudança 05, D4). Os vetores são gerados antes de gravar os trechos,
/// em uma única transação, então uma falha nunca deixa chunks parciais como vigentes. O texto é só dado (P10).
/// </summary>
internal sealed class ServicoDeIngestao(
    IExtratorDeTexto extrator,
    IClienteDeEmbeddings embeddings,
    IRepositorioDeDocumentos repositorio,
    ParametrosDaIngestao parametros,
    TimeProvider relogio)
{
    private static readonly byte[] AssinaturaDoPdf = "%PDF"u8.ToArray();

    public async Task<ResultadoDaIngestao> IngerirAsync(
        MetadadosDoDocumento metadados, string nomeDoArquivo, byte[] conteudo, string? correlacao, CancellationToken cancelamento)
    {
        Validar(metadados, nomeDoArquivo, conteudo);

        var hash = Hash(conteudo);
        var existente = await repositorio.BuscarPorHashAsync(hash, cancelamento);
        if (existente is { Estado: not ("falhou" or "processando") })
        {
            return new ResultadoDaIngestao(existente, JaExistia: true);
        }

        var id = await repositorio.IniciarAsync(metadados, hash, conteudo.LongLength, relogio.GetUtcNow(), cancelamento);
        try
        {
            var paginas = extrator.Extrair(conteudo);
            var trechos = Segmentador.Segmentar(paginas, parametros.Segmentacao);
            var comVetores = await GerarVetoresAsync(trechos, new ContextoDaChamada(id, correlacao, FinalidadeDaChamada.Ingestao), cancelamento);
            await repositorio.ConcluirAsync(id, metadados, paginas.Count, comVetores, embeddings.Modelo, parametros.Dimensao, cancelamento);
        }
        catch (Exception erro) when (erro is FalhaDoProvedor or ArquivoIlegivel or InvalidOperationException)
        {
            await repositorio.MarcarFalhaAsync(id, CancellationToken.None);
            throw new FalhaDaIngestao("A ingestão não terminou; nenhum trecho ficou vigente.", erro);
        }

        var documento = await repositorio.BuscarAsync(id, cancelamento) ?? throw new InvalidOperationException("Documento ingerido não encontrado.");
        return new ResultadoDaIngestao(documento, JaExistia: false);
    }

    private void Validar(MetadadosDoDocumento metadados, string nomeDoArquivo, byte[] conteudo)
    {
        foreach (var (campo, valor) in new[] { ("emissor", metadados.Emissor), ("titulo", metadados.Titulo), ("tipo", metadados.Tipo), ("versao", metadados.Versao) })
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new IngestaoRecusada(MotivoDaRecusa.MetadadoAusente, $"O campo {campo} é obrigatório.", campo);
            }
        }

        if (conteudo.LongLength > parametros.TamanhoMaximoEmBytes)
        {
            throw new IngestaoRecusada(MotivoDaRecusa.TamanhoExcedido, $"O arquivo passa do limite de {parametros.TamanhoMaximoEmBytes} bytes.");
        }

        if (!nomeDoArquivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || !conteudo.AsSpan().StartsWith(AssinaturaDoPdf))
        {
            throw new IngestaoRecusada(MotivoDaRecusa.FormatoNaoAceito, "Só PDF é aceito.");
        }
    }

    private async Task<IReadOnlyList<TrechoComVetor>> GerarVetoresAsync(IReadOnlyList<Trecho> trechos, ContextoDaChamada contexto, CancellationToken cancelamento)
    {
        var resultado = new List<TrechoComVetor>(trechos.Count);
        foreach (var lote in trechos.Chunk(parametros.TamanhoDoLote))
        {
            var resposta = await embeddings.GerarAsync([.. lote.Select(trecho => trecho.Texto)], contexto, cancelamento);
            if (resposta.Vetores.Count != lote.Length || resposta.Vetores.Any(vetor => vetor.Length != parametros.Dimensao))
            {
                throw new InvalidOperationException("O provedor devolveu vetores em quantidade ou dimensão inesperadas.");
            }

            resultado.AddRange(lote.Select((trecho, indice) => new TrechoComVetor(trecho, Hash(System.Text.Encoding.UTF8.GetBytes(trecho.Texto)), resposta.Vetores[indice])));
        }

        return resultado;
    }

    private static string Hash(byte[] conteudo) => Convert.ToHexStringLower(SHA256.HashData(conteudo));
}
