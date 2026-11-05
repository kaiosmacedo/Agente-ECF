using System.Reflection;
using Dapper;
using Npgsql;

namespace Api.RAG.Infraestrutura.Persistencia;

/// <summary>
/// Aplica os scripts SQL embutidos, em ordem e uma única vez. O CD o executa antes do deploy, com
/// <c>--migrar</c>, nunca na inicialização normal (design da mudança 03, D6).
/// </summary>
internal sealed class Migrador(NpgsqlDataSource banco)
{
    // Número fixo que identifica a trava de migração do serviço no banco.
    private const long TravaDeMigracao = 7_301_190_404;

    private const string PrefixoDosScripts = "Api.RAG.Infraestrutura.Persistencia.Migracoes.";

    public async Task AplicarAsync(CancellationToken cancelamento)
    {
        await using var conexao = await banco.OpenConnectionAsync(cancelamento);
        await conexao.ExecuteAsync(new CommandDefinition("SELECT pg_advisory_lock(@Trava)", new { Trava = TravaDeMigracao }, cancellationToken: cancelamento));
        try
        {
            await conexao.ExecuteAsync(new CommandDefinition(
                "CREATE TABLE IF NOT EXISTS migracoes (nome text PRIMARY KEY, aplicada_em timestamptz NOT NULL DEFAULT now())",
                cancellationToken: cancelamento));
            var aplicadas = (await conexao.QueryAsync<string>(new CommandDefinition("SELECT nome FROM migracoes", cancellationToken: cancelamento))).ToHashSet(StringComparer.Ordinal);

            foreach (var nome in Scripts().Where(nome => !aplicadas.Contains(nome)))
            {
                await using var transacao = await conexao.BeginTransactionAsync(cancelamento);
                await conexao.ExecuteAsync(new CommandDefinition(await Ler(nome, cancelamento), transaction: transacao, cancellationToken: cancelamento));
                await conexao.ExecuteAsync(new CommandDefinition("INSERT INTO migracoes (nome) VALUES (@Nome)", new { Nome = nome }, transacao, cancellationToken: cancelamento));
                await transacao.CommitAsync(cancelamento);
            }
        }
        finally
        {
            await conexao.ExecuteAsync(new CommandDefinition("SELECT pg_advisory_unlock(@Trava)", new { Trava = TravaDeMigracao }, cancellationToken: CancellationToken.None));
        }
    }

    private static IOrderedEnumerable<string> Scripts() => typeof(Migrador).Assembly.GetManifestResourceNames()
        .Where(nome => nome.StartsWith(PrefixoDosScripts, StringComparison.Ordinal) && nome.EndsWith(".sql", StringComparison.Ordinal))
        .Order(StringComparer.Ordinal);

    private static async Task<string> Ler(string nome, CancellationToken cancelamento)
    {
        await using var recurso = typeof(Migrador).Assembly.GetManifestResourceStream(nome)
            ?? throw new InvalidOperationException($"Script de migração não encontrado: {nome}.");
        using var leitor = new StreamReader(recurso);
        return await leitor.ReadToEndAsync(cancelamento);
    }
}
