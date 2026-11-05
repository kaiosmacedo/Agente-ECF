using System.Diagnostics.Metrics;
using System.Net;
using System.Text.Json;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Consumo;
using Api.RAG.Infraestrutura.Configuracao;
using Api.RAG.Infraestrutura.Consumo;
using Api.RAG.Tests.Infra;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.RAG.Tests;

public sealed class ConsumoTests
{
    private static readonly IReadOnlyList<TabelaDePrecos> Tabelas =
    [
        new("2026-10", new DateOnly(2026, 10, 1), "fonte", [new PrecoDoModelo("modelo-a", 0.04m, null)]),
        new("2026-11", new DateOnly(2026, 11, 1), "fonte", [new PrecoDoModelo("modelo-a", 0.02m, null), new PrecoDoModelo("modelo-com-saida", 1m, 2m)]),
    ];

    private static readonly DateTimeOffset Novembro = new(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Falha nas primeiras chamadas e depois responde, sem chamar nenhum provedor real.</summary>
    private sealed class ProvedorFalso(int falhas = 0, int? tokens = 100) : IProvedorDeEmbeddings
    {
        private static readonly float[] Vetor = [0.1f];

        private int chamadas;

        public string Provedor => "falso";

        public string Modelo => "modelo-a";

        public int Chamadas => chamadas;

        public Task<RespostaDeEmbeddings> GerarAsync(IReadOnlyList<string> textos, CancellationToken cancelamento)
        {
            if (Interlocked.Increment(ref chamadas) <= falhas)
            {
                throw new FalhaDoProvedor("Provedor indisponível no teste.");
            }

            return Task.FromResult(new RespostaDeEmbeddings([.. textos.Select(_ => Vetor)], tokens));
        }
    }

    private static (ClienteDeEmbeddingsComConsumo Cliente, RepositorioDeConsumoEmMemoria Repositorio, RegistradorDeConsumo Registrador) Montar(
        IProvedorDeEmbeddings provedor, int tentativas = 3, ILogger<RegistradorDeConsumo>? logger = null)
    {
        var repositorio = new RepositorioDeConsumoEmMemoria();
        var registrador = new RegistradorDeConsumo(repositorio, logger ?? NullLogger<RegistradorDeConsumo>.Instance);
        var cliente = new ClienteDeEmbeddingsComConsumo(
            provedor, registrador, new CalculadoraDeCusto(Tabelas), new PoliticaDeTentativas(tentativas, TimeSpan.Zero), TimeProvider.System);
        return (cliente, repositorio, registrador);
    }

    private static readonly ContextoDaChamada Contexto = new(Guid.NewGuid(), "corr-consumo");

    [Fact(DisplayName = "REQ-CONSUMO-001 Falha seguida de sucesso gera dois registros")]
    public async Task RetentativaRegistrada()
    {
        var (cliente, repositorio, registrador) = Montar(new ProvedorFalso(falhas: 1));
        using var _ = registrador;

        var resposta = await cliente.GerarAsync(["texto"], Contexto, TestContext.Current.CancellationToken);

        var registros = repositorio.Registros.ToArray();
        Assert.Single(resposta.Vetores);
        Assert.Equal(2, registros.Length);
        Assert.Equal((TipoDeChamada.Embedding, StatusDaChamada.Falha, 1), (registros[0].Tipo, registros[0].Status, registros[0].Tentativa));
        Assert.Equal((TipoDeChamada.Retentativa, StatusDaChamada.Sucesso, 2), (registros[1].Tipo, registros[1].Status, registros[1].Tentativa));
        Assert.All(registros, r => Assert.Equal((Contexto.IdOperacao, "corr-consumo", "falso", "modelo-a"), (r.IdOperacao, r.Correlacao, r.Provedor, r.Modelo)));
    }

    [Fact(DisplayName = "REQ-CONSUMO-001 Falha em todas as tentativas registra cada uma e termina em falha")]
    public async Task TodasAsTentativasFalham()
    {
        var provedor = new ProvedorFalso(falhas: 9);
        var (cliente, repositorio, registrador) = Montar(provedor, tentativas: 3);
        using var _ = registrador;

        await Assert.ThrowsAsync<FalhaDoProvedor>(() => cliente.GerarAsync(["texto"], Contexto, TestContext.Current.CancellationToken));

        Assert.Equal(3, provedor.Chamadas);
        Assert.Equal(3, repositorio.Registros.Count);
        Assert.All(repositorio.Registros, r => Assert.Equal(StatusDaChamada.Falha, r.Status));
    }

    [Fact(DisplayName = "REQ-CONSUMO-002 Sem contagem do provedor, os tokens ficam desconhecidos, nunca zero")]
    public async Task TokensDesconhecidos()
    {
        var (cliente, repositorio, registrador) = Montar(new ProvedorFalso(tokens: null));
        using var _ = registrador;

        await cliente.GerarAsync(["texto"], Contexto, TestContext.Current.CancellationToken);

        var registro = Assert.Single(repositorio.Registros);
        Assert.Null(registro.TokensEntrada);
        Assert.Equal(OrigemDoValor.Desconhecido, registro.TokensEntradaOrigem);
        Assert.Null(registro.CustoUsd);
        Assert.Equal(OrigemDoValor.Desconhecido, registro.CustoOrigem);
    }

    [Fact(DisplayName = "REQ-CONSUMO-002 Em embeddings, a saída não se aplica e não é zero")]
    public async Task SaidaNaoSeAplica()
    {
        var (cliente, repositorio, registrador) = Montar(new ProvedorFalso());
        using var _ = registrador;

        await cliente.GerarAsync(["texto"], Contexto, TestContext.Current.CancellationToken);

        var registro = Assert.Single(repositorio.Registros);
        Assert.Null(registro.TokensSaida);
        Assert.Equal(OrigemDoValor.NaoAplicavel, registro.TokensSaidaOrigem);
    }

    [Fact(DisplayName = "REQ-CONSUMO-003 Modelo sem preço fica com custo desconhecido e sem referência de tabela")]
    public void ModeloSemPreco()
    {
        var custo = new CalculadoraDeCusto(Tabelas).Calcular("modelo-sem-preco", 1000, null, Novembro);

        Assert.Equal(new CustoCalculado(null, OrigemDoValor.Desconhecido, null), custo);
    }

    [Fact(DisplayName = "REQ-CONSUMO-003 Estimativa usa a tabela vigente na data e cita a versão")]
    public void EstimativaComTabelaVigente()
    {
        var calculadora = new CalculadoraDeCusto(Tabelas);

        Assert.Equal(new CustoCalculado(0.00002m, OrigemDoValor.Estimado, "2026-11"), calculadora.Calcular("modelo-a", 1000, null, Novembro));
        Assert.Equal(new CustoCalculado(0.00004m, OrigemDoValor.Estimado, "2026-10"), calculadora.Calcular("modelo-a", 1000, null, new DateTimeOffset(2026, 10, 15, 0, 0, 0, TimeSpan.Zero)));
    }

    [Fact(DisplayName = "REQ-CONSUMO-003 Antes de qualquer vigência, o custo é desconhecido")]
    public void AntesDaVigencia() =>
        Assert.Equal(OrigemDoValor.Desconhecido, new CalculadoraDeCusto(Tabelas).Calcular("modelo-a", 1000, null, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)).Origem);

    [Fact(DisplayName = "REQ-CONSUMO-003 Modelo com preço de saída e sem tokens de saída fica com custo desconhecido")]
    public void SaidaDesconhecida() =>
        Assert.Equal(OrigemDoValor.Desconhecido, new CalculadoraDeCusto(Tabelas).Calcular("modelo-com-saida", 1000, null, Novembro).Origem);

    [Fact(DisplayName = "REQ-CONSUMO-003 Tabela de preços da configuração tem a versão e o preço do modelo de embeddings")]
    public async Task TabelaDaConfiguracao()
    {
        using var fabrica = new FabricaApi();
        using var http = fabrica.CreateClient();
        await http.GetAsync(new Uri("/health/live", UriKind.Relative), TestContext.Current.CancellationToken);

        var custo = fabrica.Services.GetRequiredService<CalculadoraDeCusto>().Calcular("text-embedding-3-small", 1_000_000, null, Novembro);

        Assert.Equal(new CustoCalculado(0.02m, OrigemDoValor.Estimado, "2026-11"), custo);
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Tabela de preços sem fonte impede a inicialização")]
    public async Task TabelaInvalida()
    {
        var erro = await HostIsolado.FalharAoIniciar(
            (servicos, configuracao) => servicos.AdicionarOpcoesValidadas<OpcoesDeConsumo>(configuracao, OpcoesDeConsumo.Secao),
            [new("Consumo:TabelasDePrecos:9:Versao", "2099-01"), new("Consumo:TabelasDePrecos:9:VigenteDesde", "2099-01-01")]);

        Assert.Contains("Consumo", erro.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-CONSUMO-004 Falha ao gravar gera log com a correlação e aumenta a métrica, sem interromper a chamada")]
    public async Task FalhaAoGravar()
    {
        var logs = new ProvedorDeLogEmMemoria();
        using var fabricaDeLogs = LoggerFactory.Create(builder => builder.AddProvider(logs));
        var (cliente, repositorio, registrador) = Montar(new ProvedorFalso(), logger: fabricaDeLogs.CreateLogger<RegistradorDeConsumo>());
        using var _ = registrador;
        repositorio.Indisponivel = true;

        long falhas = 0;
        using var ouvinte = new MeterListener();
        ouvinte.InstrumentPublished = (instrumento, o) =>
        {
            if (instrumento.Meter.Name == RegistradorDeConsumo.NomeDoMedidor && instrumento.Name == RegistradorDeConsumo.NomeDaMetricaDeFalhas)
            {
                o.EnableMeasurementEvents(instrumento);
            }
        };
        ouvinte.SetMeasurementEventCallback<long>((_, valor, _, _) => Interlocked.Add(ref falhas, valor));
        ouvinte.Start();

        var resposta = await cliente.GerarAsync(["texto"], Contexto, TestContext.Current.CancellationToken);

        Assert.Single(resposta.Vetores);
        Assert.Equal(1, Interlocked.Read(ref falhas));
        Assert.Contains(logs.Registros, r => r.Nivel == LogLevel.Error && r.TextoCompleto.Contains("corr-consumo", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "REQ-CONSUMO-005 Consulta agregada traz os totais por modelo, sem dados sensíveis")]
    public async Task ConsultaAgregada()
    {
        var repositorio = new RepositorioDeConsumoEmMemoria();
        repositorio.Registros.Enqueue(new RegistroDeConsumo(
            Guid.NewGuid(), Guid.NewGuid(), "corr-sigilosa", "falso", "modelo-a", TipoDeChamada.Embedding, StatusDaChamada.Sucesso, Novembro, 10, 1,
            1000, OrigemDoValor.Informado, null, OrigemDoValor.NaoAplicavel, 0.00002m, OrigemDoValor.Estimado, "2026-11"));
        repositorio.Registros.Enqueue(new RegistroDeConsumo(
            Guid.NewGuid(), Guid.NewGuid(), "corr-sigilosa", "falso", "modelo-a", TipoDeChamada.Embedding, StatusDaChamada.Falha, Novembro, 10, 1,
            null, OrigemDoValor.Desconhecido, null, OrigemDoValor.NaoAplicavel, null, OrigemDoValor.Desconhecido, null));
        using var fabrica = new FabricaApi().Com(builder => builder.ConfigureTestServices(servicos =>
            servicos.AddSingleton<IRepositorioDeConsumo>(repositorio)));

        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/consumo", UriKind.Relative), TestContext.Current.CancellationToken);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(corpo);

        var modelo = Assert.Single(json.RootElement.GetProperty("modelos").EnumerateArray());
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("USD", json.RootElement.GetProperty("moeda").GetString());
        Assert.Equal("modelo-a", modelo.GetProperty("modelo").GetString());
        Assert.Equal(2, modelo.GetProperty("chamadas").GetInt64());
        Assert.Equal(1000, modelo.GetProperty("tokensEntrada").GetInt64());
        Assert.Equal(1, modelo.GetProperty("chamadasSemTokens").GetInt64());
        Assert.Equal(1, modelo.GetProperty("chamadasSemCusto").GetInt64());
        Assert.DoesNotContain("corr-sigilosa", corpo, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-CONSUMO-005 Banco indisponível responde 503, sem detalhes")]
    public async Task ConsultaSemBanco()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.ConfigureTestServices(servicos =>
            servicos.AddSingleton<IRepositorioDeConsumo>(new RepositorioDeConsumoEmMemoria { Indisponivel = true })));

        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/consumo", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        Assert.Empty(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "GD-01 Rota de consumo consta do documento OpenAPI")]
    public async Task DocumentoOpenApi()
    {
        using var fabrica = new FabricaApi();

        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/openapi/v1.json", UriKind.Relative), TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.True(json.RootElement.GetProperty("paths").TryGetProperty("/consumo", out _));
    }
}
