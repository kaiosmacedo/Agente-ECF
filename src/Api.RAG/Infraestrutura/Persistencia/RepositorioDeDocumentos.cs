using System.Globalization;
using Api.RAG.Aplicacao.Conhecimento;
using Api.RAG.Aplicacao.Consumo;
using Dapper;
using Npgsql;

namespace Api.RAG.Infraestrutura.Persistencia;

/// <summary>Documentos e trechos no PostgreSQL com PgVector. Os trechos são gravados em lotes, por causa da distância até o banco.</summary>
internal sealed class RepositorioDeDocumentos(NpgsqlDataSource banco) : IRepositorioDeDocumentos
{
    private const int TamanhoDoLote = 500;

    private const string SelecaoDoDocumento = """
        SELECT d.id AS Id, d.emissor AS Emissor, d.titulo AS Titulo, d.tipo AS Tipo, d.versao AS Versao, d.ano_calendario AS AnoCalendario,
               d.versao_da_publicacao AS VersaoDaPublicacao, d.vigencia AS Vigencia, d.origem AS Origem, d.hash AS Hash,
               d.tamanho_bytes AS TamanhoBytes, d.paginas AS Paginas, d.estado AS Estado, d.ingerido_em AS IngeridoEm,
               (SELECT count(*)::int FROM chunks c WHERE c.documento_id = d.id) AS Chunks, d.modelo AS Modelo, d.dimensao AS Dimensao
        FROM documentos d
        """;

    public Task<DocumentoDaBase?> BuscarPorHashAsync(string hash, CancellationToken cancelamento) =>
        Protegido(async conexao => await conexao.QuerySingleOrDefaultAsync<DocumentoDaBase>(
            new CommandDefinition($"{SelecaoDoDocumento} WHERE d.hash = @Hash", new { Hash = hash }, cancellationToken: cancelamento)), cancelamento);

    public Task<DocumentoDaBase?> BuscarAsync(Guid id, CancellationToken cancelamento) =>
        Protegido(async conexao => await conexao.QuerySingleOrDefaultAsync<DocumentoDaBase>(
            new CommandDefinition($"{SelecaoDoDocumento} WHERE d.id = @Id", new { Id = id }, cancellationToken: cancelamento)), cancelamento);

    public Task<IReadOnlyList<DocumentoDaBase>> ListarAsync(string? tipo, string? versao, CancellationToken cancelamento) =>
        Protegido<IReadOnlyList<DocumentoDaBase>>(
            async conexao => [.. await conexao.QueryAsync<DocumentoDaBase>(new CommandDefinition(
                $"{SelecaoDoDocumento} WHERE (CAST(@Tipo AS text) IS NULL OR d.tipo = @Tipo) AND (CAST(@Versao AS text) IS NULL OR d.versao = @Versao) ORDER BY d.tipo, d.versao DESC, d.ingerido_em DESC",
                new { Tipo = tipo, Versao = versao },
                cancellationToken: cancelamento))],
            cancelamento);

    public Task<Guid> IniciarAsync(MetadadosDoDocumento metadados, string hash, long tamanhoBytes, DateTimeOffset quando, CancellationToken cancelamento) =>
        Protegido(
            async conexao =>
            {
                await using var transacao = await conexao.BeginTransactionAsync(cancelamento);
                await conexao.ExecuteAsync(new CommandDefinition(
                    "DELETE FROM documentos WHERE hash = @Hash AND estado IN ('falhou', 'processando')", new { Hash = hash }, transacao, cancellationToken: cancelamento));
                var id = Guid.NewGuid();
                await conexao.ExecuteAsync(new CommandDefinition(
                    """
                    INSERT INTO documentos (id, emissor, titulo, tipo, versao, ano_calendario, versao_da_publicacao, vigencia, origem, hash, tamanho_bytes, estado, ingerido_em)
                    VALUES (@Id, @Emissor, @Titulo, @Tipo, @Versao, @AnoCalendario, @VersaoDaPublicacao, @Vigencia, @Origem, @Hash, @TamanhoBytes, 'processando', @Quando)
                    """,
                    new
                    {
                        Id = id,
                        metadados.Emissor,
                        metadados.Titulo,
                        metadados.Tipo,
                        metadados.Versao,
                        metadados.AnoCalendario,
                        metadados.VersaoDaPublicacao,
                        metadados.Vigencia,
                        metadados.Origem,
                        Hash = hash,
                        TamanhoBytes = tamanhoBytes,
                        Quando = quando.UtcDateTime,
                    },
                    transacao,
                    cancellationToken: cancelamento));
                await transacao.CommitAsync(cancelamento);
                return id;
            },
            cancelamento);

    public Task ConcluirAsync(Guid id, MetadadosDoDocumento metadados, int paginas, IReadOnlyList<TrechoComVetor> trechos, string modelo, int dimensao, CancellationToken cancelamento) =>
        Protegido(
            async conexao =>
            {
                await using var transacao = await conexao.BeginTransactionAsync(cancelamento);
                foreach (var lote in trechos.Chunk(TamanhoDoLote))
                {
                    await conexao.ExecuteAsync(new CommandDefinition(
                        """
                        INSERT INTO chunks (id, documento_id, ordem, pagina, secao, texto, hash, modelo, dimensao, embedding)
                        SELECT i, @Documento, o, p, s, t, h, @Modelo, @Dimensao, CAST(e AS vector)
                        FROM unnest(@Ids, @Ordens, @Paginas, @Secoes, @Textos, @Hashes, @Vetores) AS x(i, o, p, s, t, h, e)
                        """,
                        new
                        {
                            Documento = id,
                            Modelo = modelo,
                            Dimensao = dimensao,
                            Ids = lote.Select(_ => Guid.NewGuid()).ToArray(),
                            Ordens = lote.Select(t => t.Trecho.Ordem).ToArray(),
                            Paginas = lote.Select(t => t.Trecho.Pagina).ToArray(),
                            Secoes = lote.Select(t => t.Trecho.Secao).ToArray(),
                            Textos = lote.Select(t => t.Trecho.Texto).ToArray(),
                            Hashes = lote.Select(t => t.Hash).ToArray(),
                            Vetores = lote.Select(t => ComoTexto(t.Vetor)).ToArray(),
                        },
                        transacao,
                        cancellationToken: cancelamento));
                }

                await conexao.ExecuteAsync(new CommandDefinition(
                    "UPDATE documentos SET estado = 'substituido' WHERE tipo = @Tipo AND versao = @Versao AND estado = 'vigente' AND id <> @Id",
                    new { metadados.Tipo, metadados.Versao, Id = id },
                    transacao,
                    cancellationToken: cancelamento));
                await conexao.ExecuteAsync(new CommandDefinition(
                    "UPDATE documentos SET estado = 'vigente', paginas = @Paginas, modelo = @Modelo, dimensao = @Dimensao WHERE id = @Id",
                    new { Paginas = paginas, Modelo = modelo, Dimensao = dimensao, Id = id },
                    transacao,
                    cancellationToken: cancelamento));
                await transacao.CommitAsync(cancelamento);
                return true;
            },
            cancelamento);

    public Task MarcarFalhaAsync(Guid id, CancellationToken cancelamento) =>
        Protegido(
            async conexao => await conexao.ExecuteAsync(new CommandDefinition(
                "UPDATE documentos SET estado = 'falhou' WHERE id = @Id", new { Id = id }, cancellationToken: cancelamento)),
            cancelamento);

    public Task<bool> DesativarAsync(Guid id, CancellationToken cancelamento) =>
        Protegido(
            async conexao => await conexao.ExecuteAsync(new CommandDefinition(
                "UPDATE documentos SET estado = 'desativado' WHERE id = @Id", new { Id = id }, cancellationToken: cancelamento)) == 1,
            cancelamento);

    public Task ApagarParaReprocessarAsync(Guid id, CancellationToken cancelamento) =>
        Protegido(
            async conexao => await conexao.ExecuteAsync(new CommandDefinition(
                "DELETE FROM documentos WHERE id = @Id", new { Id = id }, cancellationToken: cancelamento)),
            cancelamento);

    public Task<IReadOnlyList<TrechoEncontrado>> BuscarSemelhantesAsync(float[] vetor, string tipo, string versao, int quantidade, CancellationToken cancelamento, string? codigo = null) =>
        Protegido<IReadOnlyList<TrechoEncontrado>>(
            async conexao => [.. await conexao.QueryAsync<TrechoEncontrado>(new CommandDefinition(
                // Busca exata, sem índice aproximado (ADR 0004). O código é parâmetro, nunca concatenado (SQ-05).
                """
                SELECT c.id AS IdTrecho, d.id AS IdDocumento, d.titulo AS Titulo, d.versao AS Versao, c.pagina AS Pagina,
                       c.secao AS Secao, c.texto AS Texto, 1 - (c.embedding <=> CAST(@Vetor AS vector)) AS Similaridade,
                       (CAST(@Codigo AS text) IS NOT NULL AND strpos(c.texto, CAST(@Codigo AS text)) > 0) AS ContemCodigo
                FROM chunks c
                JOIN documentos d ON d.id = c.documento_id
                WHERE d.estado = 'vigente' AND d.tipo = @Tipo AND d.versao = @Versao
                ORDER BY ContemCodigo DESC, c.embedding <=> CAST(@Vetor AS vector), c.id
                LIMIT @Quantidade
                """,
                new { Vetor = ComoTexto(vetor), Tipo = tipo, Versao = versao, Quantidade = quantidade, Codigo = codigo },
                cancellationToken: cancelamento))],
            cancelamento);

    public Task<bool> HaVigenteAsync(string tipo, string versao, CancellationToken cancelamento) =>
        Protegido(
            async conexao => await conexao.ExecuteScalarAsync<bool>(new CommandDefinition(
                "SELECT EXISTS (SELECT 1 FROM documentos WHERE estado = 'vigente' AND tipo = @Tipo AND versao = @Versao)",
                new { Tipo = tipo, Versao = versao },
                cancellationToken: cancelamento)),
            cancelamento);

    private static string ComoTexto(float[] vetor) =>
        string.Concat("[", string.Join(',', vetor.Select(valor => valor.ToString("R", CultureInfo.InvariantCulture))), "]");

    private async Task<T> Protegido<T>(Func<NpgsqlConnection, Task<T>> acao, CancellationToken cancelamento)
    {
        try
        {
            await using var conexao = await banco.OpenConnectionAsync(cancelamento);
            return await acao(conexao);
        }
        catch (Exception erro) when (erro is NpgsqlException or TimeoutException)
        {
            throw new FalhaDoRepositorio(erro);
        }
    }
}
