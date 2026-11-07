using System.Net;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Consumo;
using Api.RAG.Infraestrutura.Configuracao;
using Api.RAG.Infraestrutura.Persistencia;
using Api.RAG.Tests.Infra;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Api.RAG.Tests;

/// <summary>PostgreSQL real em contêiner, na mesma imagem do compose (SQ-09).</summary>
public sealed class BancoDeTeste : IAsyncLifetime
{
    private readonly PostgreSqlContainer conteiner = new PostgreSqlBuilder("pgvector/pgvector:pg16").Build();
    private readonly List<FabricaApi> fabricas = [];

    internal NpgsqlDataSource Banco { get; private set; } = null!;

    public string Conexao => conteiner.GetConnectionString();

    /// <summary>Serviço com o banco real e o provedor falso; sem provedor, fica o real, que não tem a chave nos testes.</summary>
    internal FabricaApi Criar(Aplicacao.Consumo.IProvedorDeEmbeddings? provedor, params (string Chave, string Valor)[] ajustes)
    {
        var fabrica = new FabricaApi(Conexao).Com(builder =>
        {
            foreach (var (chave, valor) in ajustes)
            {
                builder.UseSetting(chave, valor);
            }

            if (provedor is not null)
            {
                builder.ConfigureTestServices(servicos => servicos.AddSingleton(provedor));
            }
        });
        fabricas.Add(fabrica);
        return fabrica;
    }

    public async ValueTask InitializeAsync()
    {
        await conteiner.StartAsync(TestContext.Current.CancellationToken);
        Banco = NpgsqlDataSource.Create(Conexao);
        await new Migrador(Banco).AplicarAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var fabrica in fabricas)
        {
            await fabrica.DisposeAsync();
        }

        await Banco.DisposeAsync();
        await conteiner.DisposeAsync();
    }
}

public sealed class ConsumoPersistenciaTests(BancoDeTeste banco) : IClassFixture<BancoDeTeste>
{
    private static RegistroDeConsumo Registro(Guid operacao, string modelo, int? tokens, decimal? custo, StatusDaChamada status = StatusDaChamada.Sucesso, DateTimeOffset? quando = null) => new(
        Guid.NewGuid(),
        operacao,
        "corr-1",
        "falso",
        modelo,
        TipoDeChamada.Embedding,
        status,
        quando ?? DateTimeOffset.UtcNow,
        12.5,
        1,
        tokens,
        tokens is null ? OrigemDoValor.Desconhecido : OrigemDoValor.Informado,
        null,
        OrigemDoValor.NaoAplicavel,
        custo,
        custo is null ? OrigemDoValor.Desconhecido : OrigemDoValor.Estimado,
        custo is null ? null : "2026-11");

    [Fact(DisplayName = "REQ-CONFIG-002 Migração cria a tabela e rodar de novo não muda nada")]
    public async Task MigracaoIdempotente()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        await using var conexao = await banco.Banco.OpenConnectionAsync(cancelamento);
        var antes = await conexao.ExecuteScalarAsync<int>("SELECT count(*) FROM migracoes");

        await new Migrador(banco.Banco).AplicarAsync(cancelamento);

        Assert.True(antes > 0);
        Assert.Equal(antes, await conexao.ExecuteScalarAsync<int>("SELECT count(*) FROM migracoes"));
        Assert.Equal(1, await conexao.ExecuteScalarAsync<int>("SELECT count(*) FROM information_schema.tables WHERE table_name = 'consumo_ia'"));
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Migrações simultâneas não conflitam")]
    public async Task MigracoesSimultaneas() =>
        await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => new Migrador(banco.Banco).AplicarAsync(TestContext.Current.CancellationToken)));

    [Fact(DisplayName = "REQ-CONFIG-002 Falta da conexão impede a inicialização, citando a opção")]
    public async Task ConexaoAusente()
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesDoBanco>(configuracao, OpcoesDoBanco.Secao),
            []);

        Assert.Contains("ConnectionStrings:Conhecimento", erro.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Conexão em formato de URI impede a inicialização e não revela a senha")]
    public async Task ConexaoEmUri()
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesDoBanco>(configuracao, OpcoesDoBanco.Secao),
            [new("ConnectionStrings:Conhecimento", "postgresql://usuario:senha-secreta@banco.exemplo/conhecimento")]);

        Assert.Contains("ConnectionStrings:Conhecimento", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("senha-secreta", erro.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-CONSUMO-001 Registro fica no banco, com todos os campos e desconhecido como nulo")]
    public async Task GravacaoNoBanco()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var repositorio = new RepositorioDeConsumo(banco.Banco);
        var registro = Registro(Guid.NewGuid(), "modelo-gravacao", null, null, StatusDaChamada.Falha);

        await repositorio.RegistrarAsync(registro, cancelamento);

        await using var conexao = await banco.Banco.OpenConnectionAsync(cancelamento);
        var linha = await conexao.QuerySingleAsync<(string Status, string Tipo, int? Tokens, string Origem, string Saida, decimal? Custo, string? Tabela)>(
            "SELECT status, tipo, tokens_entrada, tokens_entrada_origem, tokens_saida_origem, custo_usd, tabela_precos_versao FROM consumo_ia WHERE id = @Id", new { registro.Id });
        Assert.Equal(("falha", "embedding", (int?)null, "desconhecido", "nao-aplicavel", (decimal?)null, (string?)null), linha);
    }

    [Fact(DisplayName = "REQ-CONSUMO-005 Agregação soma por modelo, conta desconhecidos à parte e respeita os filtros")]
    public async Task Agregacao()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var repositorio = new RepositorioDeConsumo(banco.Banco);
        var operacao = Guid.NewGuid();
        var modelo = $"modelo-{Guid.NewGuid():N}";
        await repositorio.RegistrarAsync(Registro(operacao, modelo, 1000, 0.00002m), cancelamento);
        await repositorio.RegistrarAsync(Registro(operacao, modelo, 500, 0.00001m), cancelamento);
        await repositorio.RegistrarAsync(Registro(operacao, modelo, null, null), cancelamento);
        await repositorio.RegistrarAsync(Registro(Guid.NewGuid(), modelo, 9, 0.1m, quando: new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)), cancelamento);

        var porOperacao = await repositorio.AgregarAsync(new FiltroDeConsumo(null, null, modelo, operacao), cancelamento);
        var total = Assert.Single(porOperacao);
        Assert.Equal((3L, 1500L, 0L, 1L, 1L), (total.Chamadas, total.TokensEntrada, total.TokensSaida, total.ChamadasSemTokens, total.ChamadasSemCusto));
        Assert.Equal(0.00003m, total.CustoUsd);

        var porPeriodo = await repositorio.AgregarAsync(new FiltroDeConsumo(new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero), modelo, null), cancelamento);
        Assert.Equal(9L, Assert.Single(porPeriodo).TokensEntrada);

        Assert.Empty(await repositorio.AgregarAsync(new FiltroDeConsumo(null, null, "modelo-inexistente", null), cancelamento));
    }

    [Fact(DisplayName = "REQ-SAUDE-002 Com o banco, a prontidão fica em 200 e o consumo responde")]
    public async Task ServicoComBanco()
    {
        using var fabrica = new FabricaApi(banco.Conexao);
        using var http = fabrica.CreateClient();

        using var prontidao = await http.GetAsync(new Uri("/health/ready", UriKind.Relative), TestContext.Current.CancellationToken);
        using var consumo = await http.GetAsync(new Uri("/consumo", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, prontidao.StatusCode);
        Assert.Equal(HttpStatusCode.OK, consumo.StatusCode);
    }

    [Fact(DisplayName = "REQ-SAUDE-002 Sem banco, a prontidão fica em 503, sem detalhes, e nada vaza nos logs")]
    public async Task ServicoSemBanco()
    {
        using var fabrica = new FabricaApi("Host=127.0.0.1;Port=1;Username=usuario-secreto;Password=senha-secreta;Database=x;Timeout=1");
        using var http = fabrica.CreateClient();

        using var prontidao = await http.GetAsync(new Uri("/health/ready", UriKind.Relative), TestContext.Current.CancellationToken);
        var corpo = await prontidao.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, prontidao.StatusCode);
        foreach (var detalhe in new[] { "senha-secreta", "usuario-secreto", "127.0.0.1", "Npgsql" })
        {
            Assert.DoesNotContain(detalhe, corpo, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotContain(fabrica.Logs.Registros, registro => registro.TextoCompleto.Contains("senha-secreta", StringComparison.Ordinal));
    }
}
