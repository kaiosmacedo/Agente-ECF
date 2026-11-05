using System.Text.Json;
using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Aplicacao.Execucoes;
using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Api.Agente.Infraestrutura.Persistencia;

/// <summary>Execuções no PostgreSQL com Dapper. Só o envelope é guardado: nunca texto, nome ou pergunta (GD-06, SQ-04).</summary>
internal sealed class RepositorioDeExecucoes(NpgsqlDataSource banco) : IRepositorioDeExecucoes
{
    public Task<EnvelopeDoResultado?> BuscarAsync(Guid idExecucao, CancellationToken cancelamento) =>
        Ler("SELECT conteudo::text FROM execucoes WHERE id = @Id", new { Id = idExecucao }, cancelamento);

    public Task<EnvelopeDoResultado?> BuscarPorSolicitacaoAsync(Guid idSolicitacao, CancellationToken cancelamento) =>
        Ler("SELECT conteudo::text FROM execucoes WHERE id_solicitacao = @Id", new { Id = idSolicitacao }, cancelamento);

    public async Task<EnvelopeDoResultado> RegistrarAsync(EnvelopeDoResultado envelope, CancellationToken cancelamento)
    {
        const string Sql = """
            INSERT INTO execucoes (id, id_execucao_sofia, id_solicitacao, organizacao, usuario_pseudonimo, status, veredito, criada_em, concluida_em, conteudo)
            VALUES (@IdExecucao, @IdExecucaoSofia, @IdSolicitacao, @Organizacao, @UsuarioPseudonimo, @Status, @Veredito, @CriadaEm, @ConcluidaEm, CAST(@Conteudo AS jsonb))
            ON CONFLICT (id_solicitacao) DO NOTHING
            """;

        var parametros = new
        {
            envelope.IdExecucao,
            envelope.IdExecucaoSofia,
            envelope.IdSolicitacao,
            envelope.Identidade.Organizacao,
            envelope.Identidade.UsuarioPseudonimo,
            envelope.Status,
            envelope.Veredito,
            CriadaEm = envelope.CriadaEm.UtcDateTime,
            ConcluidaEm = envelope.ConcluidaEm?.UtcDateTime,
            Conteudo = JsonSerializer.Serialize(envelope, ContratoSofiaV1.Json),
        };

        try
        {
            await using var conexao = await banco.OpenConnectionAsync(cancelamento);
            var gravadas = await conexao.ExecuteAsync(new CommandDefinition(Sql, parametros, cancellationToken: cancelamento));
            if (gravadas == 1)
            {
                return envelope;
            }
        }
        catch (Exception erro) when (erro is NpgsqlException or TimeoutException)
        {
            throw new FalhaDoRepositorio(erro);
        }

        // Outra chamada com o mesmo idSolicitacao gravou primeiro: vale o registro dela.
        return await BuscarPorSolicitacaoAsync(envelope.IdSolicitacao, cancelamento)
            ?? throw new FalhaDoRepositorio(new InvalidOperationException("Registro concorrente não encontrado."));
    }

    public async Task<bool> DisponivelAsync(CancellationToken cancelamento)
    {
        try
        {
            await using var conexao = await banco.OpenConnectionAsync(cancelamento);
            return await conexao.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1", cancellationToken: cancelamento)) == 1;
        }
        catch (Exception erro) when (erro is NpgsqlException or TimeoutException)
        {
            return false;
        }
    }

    private async Task<EnvelopeDoResultado?> Ler(string sql, object parametros, CancellationToken cancelamento)
    {
        try
        {
            await using var conexao = await banco.OpenConnectionAsync(cancelamento);
            var conteudo = await conexao.QuerySingleOrDefaultAsync<string>(new CommandDefinition(sql, parametros, cancellationToken: cancelamento));
            return conteudo is null ? null : JsonSerializer.Deserialize<EnvelopeDoResultado>(conteudo, ContratoSofiaV1.Json);
        }
        catch (Exception erro) when (erro is NpgsqlException or TimeoutException)
        {
            throw new FalhaDoRepositorio(erro);
        }
    }
}

/// <summary>Inclui o banco na prontidão, sem detalhes na resposta (REQ-SAUDE-002, REQ-SAUDE-003).</summary>
internal sealed class VerificacaoDoBanco(IRepositorioDeExecucoes repositorio) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        await repositorio.DisponivelAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
}
