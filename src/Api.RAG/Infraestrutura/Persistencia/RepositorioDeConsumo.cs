using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Consumo;
using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Api.RAG.Infraestrutura.Persistencia;

/// <summary>Consumo no PostgreSQL com Dapper. Só metadados: sem texto, prompt ou credencial (REQ-CONSUMO-005).</summary>
internal sealed class RepositorioDeConsumo(NpgsqlDataSource banco) : IRepositorioDeConsumo
{
    public async Task RegistrarAsync(RegistroDeConsumo registro, CancellationToken cancelamento)
    {
        const string Sql = """
            INSERT INTO consumo_ia (id, id_operacao, correlacao, provedor, modelo, tipo, status, criada_em, duracao_ms, tentativa,
                tokens_entrada, tokens_entrada_origem, tokens_saida, tokens_saida_origem, custo_usd, custo_origem, tabela_precos_versao)
            VALUES (@Id, @IdOperacao, @Correlacao, @Provedor, @Modelo, @Tipo, @Status, @CriadaEm, @DuracaoMs, @Tentativa,
                @TokensEntrada, @TokensEntradaOrigem, @TokensSaida, @TokensSaidaOrigem, @CustoUsd, @CustoOrigem, @TabelaPrecosVersao)
            """;

        var parametros = new
        {
            registro.Id,
            registro.IdOperacao,
            registro.Correlacao,
            registro.Provedor,
            registro.Modelo,
            Tipo = Nome(registro.Tipo),
            Status = registro.Status == StatusDaChamada.Sucesso ? "sucesso" : "falha",
            CriadaEm = registro.CriadaEm.UtcDateTime,
            registro.DuracaoMs,
            registro.Tentativa,
            registro.TokensEntrada,
            TokensEntradaOrigem = Nome(registro.TokensEntradaOrigem),
            registro.TokensSaida,
            TokensSaidaOrigem = Nome(registro.TokensSaidaOrigem),
            registro.CustoUsd,
            CustoOrigem = Nome(registro.CustoOrigem),
            registro.TabelaPrecosVersao,
        };

        try
        {
            await using var conexao = await banco.OpenConnectionAsync(cancelamento);
            await conexao.ExecuteAsync(new CommandDefinition(Sql, parametros, cancellationToken: cancelamento));
        }
        catch (Exception erro) when (erro is NpgsqlException or TimeoutException)
        {
            throw new FalhaDoRepositorio(erro);
        }
    }

    public async Task<IReadOnlyList<TotalDoModelo>> AgregarAsync(FiltroDeConsumo filtro, CancellationToken cancelamento)
    {
        const string Sql = """
            SELECT modelo AS Modelo,
                   count(*) AS Chamadas,
                   COALESCE(sum(tokens_entrada), 0)::bigint AS TokensEntrada,
                   COALESCE(sum(tokens_saida), 0)::bigint AS TokensSaida,
                   COALESCE(sum(custo_usd), 0)::numeric AS CustoUsd,
                   count(*) FILTER (WHERE tokens_entrada IS NULL AND tokens_entrada_origem = 'desconhecido') AS ChamadasSemTokens,
                   count(*) FILTER (WHERE custo_usd IS NULL) AS ChamadasSemCusto
            FROM consumo_ia
            WHERE (CAST(@De AS timestamptz) IS NULL OR criada_em >= @De)
              AND (CAST(@Ate AS timestamptz) IS NULL OR criada_em <= @Ate)
              AND (CAST(@Modelo AS text) IS NULL OR modelo = @Modelo)
              AND (CAST(@IdOperacao AS uuid) IS NULL OR id_operacao = @IdOperacao)
            GROUP BY modelo
            ORDER BY modelo
            """;

        var parametros = new { De = filtro.De?.UtcDateTime, Ate = filtro.Ate?.UtcDateTime, filtro.Modelo, filtro.IdOperacao };
        try
        {
            await using var conexao = await banco.OpenConnectionAsync(cancelamento);
            return [.. await conexao.QueryAsync<TotalDoModelo>(new CommandDefinition(Sql, parametros, cancellationToken: cancelamento))];
        }
        catch (Exception erro) when (erro is NpgsqlException or TimeoutException)
        {
            throw new FalhaDoRepositorio(erro);
        }
    }

    public async Task<bool> DisponivelAsync(CancellationToken cancelamento)
    {
        try
        {
            await using var conexao = await banco.OpenConnectionAsync(cancelamento);
            await conexao.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1", cancellationToken: cancelamento));
            return true;
        }
        catch (Exception erro) when (erro is NpgsqlException or TimeoutException)
        {
            return false;
        }
    }

    private static string Nome(TipoDeChamada tipo) => tipo switch
    {
        TipoDeChamada.Geracao => "geracao",
        TipoDeChamada.Embedding => "embedding",
        TipoDeChamada.Retentativa => "retentativa",
        TipoDeChamada.Auxiliar => "auxiliar",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
    };

    private static string Nome(OrigemDoValor origem) => origem switch
    {
        OrigemDoValor.Informado => "informado",
        OrigemDoValor.Estimado => "estimado",
        OrigemDoValor.Desconhecido => "desconhecido",
        OrigemDoValor.NaoAplicavel => "nao-aplicavel",
        _ => throw new ArgumentOutOfRangeException(nameof(origem)),
    };
}

/// <summary>Prontidão inclui o banco, sem detalhes na resposta (REQ-SAUDE-002, 003).</summary>
internal sealed class VerificacaoDoBanco(IRepositorioDeConsumo repositorio) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        await repositorio.DisponivelAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
}
