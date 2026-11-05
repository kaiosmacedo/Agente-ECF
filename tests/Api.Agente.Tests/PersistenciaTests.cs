using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Dominio.Execucoes;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Persistencia;
using Api.Agente.Tests.Infra;
using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Api.Agente.Tests;

/// <summary>PostgreSQL real em contêiner, na mesma imagem do compose (SQ-09).</summary>
public sealed class BancoDeTeste : IAsyncLifetime
{
    private readonly PostgreSqlContainer conteiner = new PostgreSqlBuilder("pgvector/pgvector:pg16").Build();

    internal NpgsqlDataSource Banco { get; private set; } = null!;

    public string Conexao => conteiner.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await conteiner.StartAsync(TestContext.Current.CancellationToken);
        Banco = NpgsqlDataSource.Create(Conexao);
        await new Migrador(Banco).AplicarAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await Banco.DisposeAsync();
        await conteiner.DisposeAsync();
    }
}

public sealed class PersistenciaTests(BancoDeTeste banco) : IClassFixture<BancoDeTeste>
{
    private static EnvelopeDoResultado Envelope(Guid? idSolicitacao = null)
    {
        var execucao = Execucao.Criar(Guid.NewGuid(), idSolicitacao ?? Guid.NewGuid(), new IdentidadeDeclarada("pseudonimo-teste", "org-a"), DateTimeOffset.UtcNow);
        execucao.ConcluirEtapa(Etapa.Recepcao);
        execucao.AdicionarAchado(Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra de teste", "Campo ausente.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "R1", 3, "CAMPO")], ["Linha 3"]));
        execucao.RegistrarDocumento(new DocumentoRecebido("doc-1", new string('a', 64), 10));
        execucao.Concluir(DateTimeOffset.UtcNow);
        return MontadorDoEnvelope.Montar(execucao, "corr-1", "1.0.0", "Síntese.", null);
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Migração cria a tabela e rodar de novo não muda nada")]
    public async Task MigracaoIdempotente()
    {
        await new Migrador(banco.Banco).AplicarAsync(TestContext.Current.CancellationToken);

        await using var conexao = await banco.Banco.OpenConnectionAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, await conexao.ExecuteScalarAsync<int>("SELECT count(*) FROM migracoes WHERE nome LIKE '%0001_execucoes.sql'"));
        Assert.Equal(1, await conexao.ExecuteScalarAsync<int>("SELECT count(*) FROM migracoes WHERE nome LIKE '%0002_veredito.sql'"));
        Assert.Equal(1, await conexao.ExecuteScalarAsync<int>("SELECT count(*) FROM information_schema.tables WHERE table_name = 'execucoes'"));
    }

    [Fact(DisplayName = "REQ-RESULT-010 Migração preenche o veredito das execuções antigas, e rodar de novo não muda nada")]
    public async Task MigracaoDoVeredito()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        await using (var admin = await banco.Banco.OpenConnectionAsync(cancelamento))
        {
            await admin.ExecuteAsync(new CommandDefinition("CREATE DATABASE migracao_antiga", cancellationToken: cancelamento));
        }

        var conexao = new NpgsqlConnectionStringBuilder(banco.Conexao) { Database = "migracao_antiga" }.ConnectionString;
        await using var antigo = NpgsqlDataSource.Create(conexao);
        var idParcial = Guid.NewGuid();
        var idRejeitada = Guid.NewGuid();
        await using (var c = await antigo.OpenConnectionAsync(cancelamento))
        {
            await c.ExecuteAsync(new CommandDefinition(await File.ReadAllTextAsync(Repositorio.Caminho("src", "Api.Agente", "Infraestrutura", "Persistencia", "Migracoes", "0001_execucoes.sql"), cancelamento), cancellationToken: cancelamento));
            await c.ExecuteAsync(new CommandDefinition("CREATE TABLE migracoes (nome text PRIMARY KEY, aplicada_em timestamptz NOT NULL DEFAULT now())", cancellationToken: cancelamento));
            await c.ExecuteAsync(new CommandDefinition("INSERT INTO migracoes (nome) VALUES ('Api.Agente.Infraestrutura.Persistencia.Migracoes.0001_execucoes.sql')", cancellationToken: cancelamento));
            const string Inserir = "INSERT INTO execucoes (id, id_execucao_sofia, id_solicitacao, organizacao, usuario_pseudonimo, status, criada_em, conteudo) VALUES (@Id, @Id, @Id, 'org', 'p', @Status, now(), '{}')";
            await c.ExecuteAsync(new CommandDefinition(Inserir, new { Id = idParcial, Status = "parcial" }, cancellationToken: cancelamento));
            await c.ExecuteAsync(new CommandDefinition(Inserir, new { Id = idRejeitada, Status = "entrada-rejeitada" }, cancellationToken: cancelamento));
        }

        await new Migrador(antigo).AplicarAsync(cancelamento);
        await new Migrador(antigo).AplicarAsync(cancelamento);

        await using var leitura = await antigo.OpenConnectionAsync(cancelamento);
        Assert.Equal("parcial", await leitura.ExecuteScalarAsync<string>("SELECT veredito FROM execucoes WHERE id = @Id", new { Id = idParcial }));
        Assert.Equal("nao-avaliado", await leitura.ExecuteScalarAsync<string>("SELECT conteudo->>'veredito' FROM execucoes WHERE id = @Id", new { Id = idRejeitada }));
        Assert.Equal(0, await leitura.ExecuteScalarAsync<int>("SELECT (conteudo->'resumo'->>'impeditivas')::int FROM execucoes WHERE id = @Id", new { Id = idParcial }));
        Assert.Equal(2, await leitura.ExecuteScalarAsync<int>("SELECT count(*) FROM migracoes"));
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Migrações simultâneas não conflitam")]
    public async Task MigracoesSimultaneas()
    {
        var migracoes = Enumerable.Range(0, 4).Select(_ => new Migrador(banco.Banco).AplicarAsync(TestContext.Current.CancellationToken));

        await Task.WhenAll(migracoes);
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Falta da conexão impede a inicialização e não revela valores")]
    public async Task ConexaoAusente()
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesDoBanco>(configuracao, OpcoesDoBanco.Secao),
            []);

        Assert.Contains("ConnectionStrings:Execucoes", erro.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Formato documentado da conexão é aceito, com TLS verificado")]
    public void FormatoDocumentado()
    {
        var formato = File.ReadLines(Repositorio.Caminho("docs", "ci-cd.md")).Single(linha => linha.StartsWith("Host=<host>", StringComparison.Ordinal));

        var conexao = new NpgsqlConnectionStringBuilder(formato);

        Assert.Equal(SslMode.VerifyFull, conexao.SslMode);
        Assert.Equal(ChannelBinding.Require, conexao.ChannelBinding);

        var script = File.ReadAllText(Repositorio.Caminho(".github", "scripts", "cadastrar-banco-neon.sh"));
        var noScript = formato.Replace("<host>", "%s", StringComparison.Ordinal).Replace("<banco>", "%s", StringComparison.Ordinal)
            .Replace("<usuario>", "%s", StringComparison.Ordinal).Replace("<senha>", "%s", StringComparison.Ordinal);
        Assert.Contains(noScript, script, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Conexão em formato de URI impede a inicialização e não revela a senha")]
    public async Task ConexaoEmUri()
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesDoBanco>(configuracao, OpcoesDoBanco.Secao),
            [new("ConnectionStrings:Execucoes", "postgresql://usuario:senha-secreta@banco.exemplo/execucoes")]);

        Assert.Contains("ConnectionStrings:Execucoes", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("senha-secreta", erro.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-EXEC-001 Execução gravada é lida por identificador e por idSolicitacao")]
    public async Task GravacaoELeitura()
    {
        var repositorio = new RepositorioDeExecucoes(banco.Banco);
        var envelope = Envelope();

        var gravado = await repositorio.RegistrarAsync(envelope, TestContext.Current.CancellationToken);
        var porId = await repositorio.BuscarAsync(envelope.IdExecucao, TestContext.Current.CancellationToken);
        var porSolicitacao = await repositorio.BuscarPorSolicitacaoAsync(envelope.IdSolicitacao, TestContext.Current.CancellationToken);

        Assert.Equal(envelope.IdExecucao, gravado.IdExecucao);
        Assert.Equal(envelope.IdExecucao, porId!.IdExecucao);
        Assert.Equal(envelope.IdExecucao, porSolicitacao!.IdExecucao);
        Assert.Equal(9, porId.Etapas.Count);
        Assert.Equal("doc-1", Assert.Single(porId.Documentos).IdDocumento);
        await using (var coluna = await banco.Banco.OpenConnectionAsync(TestContext.Current.CancellationToken))
        {
            Assert.Equal(envelope.Veredito, await coluna.ExecuteScalarAsync<string>("SELECT veredito FROM execucoes WHERE id = @Id", new { Id = envelope.IdExecucao }));
        }

        Assert.True(JsonNode.DeepEquals(
            JsonSerializer.SerializeToNode(envelope, ContratoSofiaV1.Json),
            JsonSerializer.SerializeToNode(porId, ContratoSofiaV1.Json)));
        Assert.Null(await repositorio.BuscarAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "D7 Chamadas simultâneas com o mesmo idSolicitacao geram uma só execução")]
    public async Task ChamadasSimultaneas()
    {
        var repositorio = new RepositorioDeExecucoes(banco.Banco);
        var idSolicitacao = Guid.NewGuid();

        var resultados = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => repositorio.RegistrarAsync(Envelope(idSolicitacao), TestContext.Current.CancellationToken)));

        Assert.Single(resultados.Select(resultado => resultado.IdExecucao).Distinct());
        await using var conexao = await banco.Banco.OpenConnectionAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, await conexao.ExecuteScalarAsync<int>("SELECT count(*) FROM execucoes WHERE id_solicitacao = @Id", new { Id = idSolicitacao }));
    }

    [Fact(DisplayName = "REQ-SAUDE-002 Repositório informa quando o banco responde e quando não responde")]
    public async Task Disponibilidade()
    {
        await using var inacessivel = NpgsqlDataSource.Create("Host=127.0.0.1;Port=1;Username=x;Password=x;Database=x;Timeout=1");

        Assert.True(await new RepositorioDeExecucoes(banco.Banco).DisponivelAsync(TestContext.Current.CancellationToken));
        Assert.False(await new RepositorioDeExecucoes(inacessivel).DisponivelAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "SQ-06 Banco inacessível vira FalhaDoRepositorio, sem detalhes da conexão")]
    public async Task FalhaSemDetalhes()
    {
        await using var inacessivel = NpgsqlDataSource.Create("Host=127.0.0.1;Port=1;Username=usuario-secreto;Password=senha-secreta;Database=x;Timeout=1");

        var erro = await Assert.ThrowsAsync<FalhaDoRepositorio>(() => new RepositorioDeExecucoes(inacessivel).BuscarAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.DoesNotContain("senha-secreta", erro.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-SAUDE-002 Sem banco, a prontidão fica em 503, sem detalhes, e a análise termina em falha técnica")]
    public async Task ServicoSemBanco()
    {
        const string Conexao = "Host=127.0.0.1;Port=1;Username=usuario-secreto;Password=senha-secreta;Database=x;Timeout=1";
        using var fabrica = new FabricaApi(Conexao);
        using var http = fabrica.CreateClient();

        using var prontidao = await http.GetAsync(new Uri("/health/ready", UriKind.Relative), TestContext.Current.CancellationToken);
        var corpoDaProntidao = await prontidao.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "SendMessage", ContratoDeTeste.SendMessage(ContratoDeTeste.Solicitacao()));
        using var resultado = ContratoDeTeste.ResultadoDoArtefato(enviada.RootElement.GetProperty("result").GetProperty("task"));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, prontidao.StatusCode);
        Assert.Equal("falha_tecnica", resultado.RootElement.GetProperty("erro").GetProperty("codigo").GetString());
        var respostas = corpoDaProntidao + enviada.RootElement.GetRawText();
        foreach (var detalhe in new[] { "senha-secreta", "usuario-secreto", "127.0.0.1", "Npgsql" })
        {
            Assert.DoesNotContain(detalhe, respostas, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(fabrica.Logs.Registros, registro => registro.TextoCompleto.Contains("senha-secreta", StringComparison.Ordinal));
        }
    }

    [Fact(DisplayName = "SQ-04 Ponta a ponta, texto, nome e pergunta não chegam ao banco")]
    public async Task PontaAPontaSemDadosSensiveis()
    {
        using var fabrica = new FabricaApi(banco.Conexao);
        using var http = fabrica.CreateClient();
        var idSolicitacao = Guid.NewGuid().ToString();
        var solicitacao = ContratoDeTeste.Solicitacao(s =>
        {
            s["idSolicitacao"] = idSolicitacao;
            s["usuario"]!["nome"] = "nome-sigiloso-123";
            s["pergunta"] = "pergunta-sigilosa-456";
            s["documentos"]![0]!["texto"] = "conteudo-sigiloso-789";
        });

        using var enviada = await ContratoDeTeste.ChamarJsonRpc(http, "SendMessage", ContratoDeTeste.SendMessage(solicitacao));
        using var resultado = ContratoDeTeste.ResultadoDoArtefato(enviada.RootElement.GetProperty("result").GetProperty("task"));
        var idExecucao = resultado.RootElement.GetProperty("idExecucaoAgente").GetString();
        using var consulta = await http.GetAsync(new Uri($"/analises/{idExecucao}", UriKind.Relative), TestContext.Current.CancellationToken);
        using var prontidao = await http.GetAsync(new Uri("/health/ready", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, prontidao.StatusCode);
        using var envelope = JsonDocument.Parse(await consulta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(idSolicitacao, envelope.RootElement.GetProperty("idSolicitacao").GetString());

        await using var conexao = await banco.Banco.OpenConnectionAsync(TestContext.Current.CancellationToken);
        var linha = await conexao.QuerySingleAsync<string>(
            "SELECT row_to_json(execucoes)::text FROM execucoes WHERE id_solicitacao = @Id", new { Id = Guid.Parse(idSolicitacao) });
        foreach (var sensivel in new[] { "nome-sigiloso-123", "pergunta-sigilosa-456", "conteudo-sigiloso-789" })
        {
            Assert.DoesNotContain(sensivel, linha, StringComparison.Ordinal);
        }
    }
}
