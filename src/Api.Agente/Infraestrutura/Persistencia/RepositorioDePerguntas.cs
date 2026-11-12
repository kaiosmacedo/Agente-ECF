using System.Text.Json;
using Api.Agente.Aplicacao.Consumo;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Aplicacao.Perguntas;
using Api.Agente.Dominio.Consumo;
using Dapper;
using Npgsql;

namespace Api.Agente.Infraestrutura.Persistencia;

/// <summary>Perguntas no PostgreSQL com Dapper, sem o texto da pergunta nem o da resposta (DP-25; mudança 17, D9).</summary>
internal sealed class RepositorioDePerguntas(NpgsqlDataSource banco) : IRepositorioDePerguntas
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task RegistrarAsync(RegistroDaPergunta registro, CancellationToken cancelamento)
    {
        const string Sql = """
            INSERT INTO perguntas (id, id_solicitacao, canal, status, codigo_erro, pergunta_sha256, pergunta_caracteres, versao_manual,
                trechos, citados, redacao, modelo, criada_em, concluida_em)
            VALUES (@Id, @IdSolicitacao, @Canal, @Status, @CodigoDoErro, @PerguntaSha256, @PerguntaCaracteres, @VersaoDoManual,
                CAST(@Trechos AS jsonb), CAST(@Citados AS jsonb), @Redacao, @Modelo, @CriadaEm, @ConcluidaEm)
            ON CONFLICT (id_solicitacao) DO NOTHING
            """;
        var parametros = new
        {
            registro.Id,
            registro.IdSolicitacao,
            registro.Canal,
            registro.Status,
            registro.CodigoDoErro,
            registro.PerguntaSha256,
            registro.PerguntaCaracteres,
            registro.VersaoDoManual,
            Redacao = registro.Redacao switch
            {
                DesfechoDaRedacao.Aceita => "aceita",
                DesfechoDaRedacao.Reserva => "reserva",
                _ => "nao-chamada",
            },
            registro.Modelo,
            CriadaEm = registro.CriadaEm.UtcDateTime,
            ConcluidaEm = registro.ConcluidaEm.UtcDateTime,
            Trechos = JsonSerializer.Serialize(registro.Trechos, Json),
            Citados = JsonSerializer.Serialize(registro.Citados, Json),
        };
        await ExecutarAsync(conexao => conexao.ExecuteAsync(new CommandDefinition(Sql, parametros, cancellationToken: cancelamento)), cancelamento);
    }

    public Task<long> ContarRespondidasDesdeAsync(DateTimeOffset desde, CancellationToken cancelamento)
    {
        const string Sql = "SELECT count(*) FROM perguntas WHERE status IN ('concluido', 'parcial') AND criada_em >= @Desde";
        return ExecutarAsync(
            conexao => conexao.ExecuteScalarAsync<long>(new CommandDefinition(Sql, new { Desde = desde.UtcDateTime }, cancellationToken: cancelamento)),
            cancelamento);
    }

    private async Task<T> ExecutarAsync<T>(Func<NpgsqlConnection, Task<T>> acao, CancellationToken cancelamento)
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

/// <summary>Consumo no PostgreSQL com Dapper. Só metadados: sem texto, prompt ou credencial (REQ-CONSUMO-005).</summary>
internal sealed class RepositorioDeConsumo(NpgsqlDataSource banco) : IRepositorioDeConsumo
{
    public async Task RegistrarAsync(RegistroDeConsumo registro, CancellationToken cancelamento)
    {
        const string Sql = """
            INSERT INTO consumo_ia (id, id_operacao, correlacao, provedor, modelo, tipo, status, criada_em, duracao_ms, tentativa,
                tokens_entrada, tokens_entrada_origem, tokens_saida, tokens_saida_origem, custo_usd, custo_origem, tabela_precos_versao, finalidade)
            VALUES (@Id, @IdOperacao, @Correlacao, @Provedor, @Modelo, @Tipo, @Status, @CriadaEm, @DuracaoMs, @Tentativa,
                @TokensEntrada, @TokensEntradaOrigem, @TokensSaida, @TokensSaidaOrigem, @CustoUsd, @CustoOrigem, @TabelaPrecosVersao, @Finalidade)
            """;
        var parametros = new
        {
            registro.Id,
            registro.IdOperacao,
            registro.Correlacao,
            registro.Provedor,
            registro.Modelo,
            Tipo = registro.Tipo == TipoDeChamada.Geracao ? "geracao" : "retentativa",
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
            Finalidade = registro.Finalidade == FinalidadeDaChamada.Pergunta ? "pergunta" : "analise",
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

    private static string Nome(OrigemDoValor origem) => origem switch
    {
        OrigemDoValor.Informado => "informado",
        OrigemDoValor.Estimado => "estimado",
        _ => "desconhecido",
    };
}
