using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Api.RAG.Aplicacao.Conhecimento;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Tests.Infra;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Api.RAG.Tests;

/// <summary>
/// A ferramenta <c>buscar_no_manual</c> pelo transporte HTTP do MCP, com PostgreSQL real em contêiner e embeddings
/// falsos: nenhum teste chama a OpenAI (SQ-09). Cobre REQ-RECUP-001 a 006 e o ADR 0005.
/// </summary>
public sealed class McpTests(BancoDeTeste banco) : IClassFixture<BancoDeTeste>
{
    private const int Dimensao = 1536;
    private const string Tipo = "manual-teste";

    private static readonly (string, string)[] BuscaLigada = [("Busca:Habilitada", "true"), ("OpenAI:ApiKey", "chave-falsa-de-teste")];

    /// <summary>Vetores determinísticos pelo texto: o mesmo texto dá similaridade 1.</summary>
    private sealed class ProvedorFalso : IProvedorDeEmbeddings
    {
        public int Chamadas { get; private set; }

        public string Provedor => "falso";

        public string Modelo => "text-embedding-3-small";

        public Task<RespostaDeEmbeddings> GerarAsync(IReadOnlyList<string> textos, CancellationToken cancelamento)
        {
            Chamadas++;
            return Task.FromResult(new RespostaDeEmbeddings([.. textos.Select(Vetor)], textos.Sum(texto => texto.Length / 4)));
        }

        private static float[] Vetor(string texto)
        {
            var vetor = new float[Dimensao];
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
            for (var i = 0; i < Dimensao; i++)
            {
                vetor[i] = (hash[i % hash.Length] - 128) / 128f;
            }

            return vetor;
        }
    }

    private sealed record Resposta(string VersaoContrato, string? Motivo, IReadOnlyList<Trecho> Trechos);

    private sealed record Trecho(Guid IdTrecho, Guid IdDocumento, string Titulo, string Versao, int Pagina, string? Secao, double Relevancia, bool ContemCodigo, string Texto);

    private sealed record RespostaDeErro(string VersaoContrato, CorpoDoErro Erro);

    private sealed record CorpoDoErro(string Codigo, string Mensagem);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Alterna as requisições entre duas instâncias, como o Vercel pode fazer (ADR 0005).</summary>
    private sealed class Alternador(HttpMessageHandler a, HttpMessageHandler b) : HttpMessageHandler
    {
        private readonly HttpMessageInvoker[] instancias = [new(a), new(b)];
        private int proxima;

        public int[] Atendidas { get; } = [0, 0];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var indice = Interlocked.Increment(ref proxima) % 2;
            Atendidas[indice]++;
            return instancias[indice].SendAsync(request, cancellationToken);
        }
    }

    private static byte[] Pdf(params string[] paginas)
    {
        var construtor = new PdfDocumentBuilder();
        var fonte = construtor.AddStandard14Font(Standard14Font.Helvetica);
        foreach (var texto in paginas)
        {
            var pagina = construtor.AddPage(UglyToad.PdfPig.Content.PageSize.A4);
            pagina.AddText(texto, 10, new UglyToad.PdfPig.Core.PdfPoint(25, 700), fonte);
        }

        return construtor.Build();
    }

    private static async Task<McpClient> Conectar(HttpClient http, CancellationToken cancelamento, string? correlacao = null)
    {
        var opcoes = new HttpClientTransportOptions
        {
            Endpoint = new Uri(http.BaseAddress!, "/mcp"),
            TransportMode = HttpTransportMode.StreamableHttp,
            AdditionalHeaders = correlacao is null ? null : new Dictionary<string, string> { ["X-Correlation-Id"] = correlacao },
        };
        return await McpClient.CreateAsync(new HttpClientTransport(opcoes, http), cancellationToken: cancelamento);
    }

    private static Dictionary<string, object?> Argumentos(string consulta, string versao, int? quantidade = 5, string? codigo = null, string? idExecucao = null)
    {
        var argumentos = new Dictionary<string, object?> { ["consulta"] = consulta, ["tipoDocumento"] = Tipo, ["versao"] = versao };
        if (quantidade is not null)
        {
            argumentos["quantidade"] = quantidade;
        }

        if (codigo is not null)
        {
            argumentos["codigo"] = codigo;
        }

        if (idExecucao is not null)
        {
            argumentos["idExecucao"] = idExecucao;
        }

        return argumentos;
    }

    private static string Texto(CallToolResult resultado) => Assert.Single(resultado.Content.OfType<TextContentBlock>()).Text;

    private static Resposta Sucesso(CallToolResult resultado)
    {
        Assert.NotEqual(true, resultado.IsError);
        return JsonSerializer.Deserialize<Resposta>(Texto(resultado), Json)!;
    }

    private static CorpoDoErro Falha(CallToolResult resultado)
    {
        Assert.True(resultado.IsError);
        return JsonSerializer.Deserialize<RespostaDeErro>(Texto(resultado), Json)!.Erro;
    }

    private static string VersaoNova() => $"v-{Guid.NewGuid():N}";

    private static async Task<string> IngerirAsync(IServiceProvider servicos, string versao, params string[] paginas)
    {
        var ingestao = servicos.GetRequiredService<ServicoDeIngestao>();
        await ingestao.IngerirAsync(new MetadadosDoDocumento("Emissor de teste", "Manual sintético", Tipo, versao), "m.pdf", Pdf(paginas), null, TestContext.Current.CancellationToken);
        return versao;
    }

    [Fact(DisplayName = "REQ-RECUP-001 A ferramenta devolve cada trecho com documento, versão, página e relevância, no contrato 1.0")]
    public async Task BuscaComResultado()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var fabrica = banco.Criar(new ProvedorFalso(), BuscaLigada);
        var versao = await IngerirAsync(fabrica.Services, VersaoNova(), $"Registro 0000 abertura {Guid.NewGuid()}", $"Registro 9999 encerramento {Guid.NewGuid()}");
        await using var cliente = await Conectar(fabrica.CreateClient(), cancelamento);

        var resposta = Sucesso(await cliente.CallToolAsync("buscar_no_manual", Argumentos("Registro 0000", versao), cancellationToken: cancelamento));

        Assert.Equal(("1.0", null, 2), (resposta.VersaoContrato, resposta.Motivo, resposta.Trechos.Count));
        Assert.All(resposta.Trechos, trecho =>
        {
            Assert.Equal(("Manual sintético", versao), (trecho.Titulo, trecho.Versao));
            Assert.InRange(trecho.Pagina, 1, 2);
            Assert.InRange(trecho.Relevancia, -1, 1);
            Assert.NotEqual(Guid.Empty, trecho.IdTrecho);
            Assert.NotEqual(Guid.Empty, trecho.IdDocumento);
        });
    }

    [Fact(DisplayName = "REQ-RECUP-001 O servidor anuncia buscar_no_manual com os argumentos do contrato")]
    public async Task ListaDeFerramentas()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        await using var cliente = await Conectar(banco.Criar(new ProvedorFalso(), BuscaLigada).CreateClient(), cancelamento);

        var ferramenta = Assert.Single(await cliente.ListToolsAsync(cancellationToken: cancelamento));

        Assert.Equal("buscar_no_manual", ferramenta.Name);
        var propriedades = ferramenta.JsonSchema.GetProperty("properties").EnumerateObject().Select(p => p.Name).ToHashSet();
        Assert.Superset(new HashSet<string> { "consulta", "tipoDocumento", "versao", "quantidade", "codigo", "idExecucao" }, propriedades);
    }

    [Fact(DisplayName = "REQ-RECUP-002 Com duas versões ingeridas, a busca da versão pedida não devolve trecho da outra")]
    public async Task DuasVersoes()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var fabrica = banco.Criar(new ProvedorFalso(), BuscaLigada);
        var v11 = await IngerirAsync(fabrica.Services, VersaoNova(), $"Regra da versao onze {Guid.NewGuid()}");
        var v12 = await IngerirAsync(fabrica.Services, VersaoNova(), $"Regra da versao doze {Guid.NewGuid()}");
        await using var cliente = await Conectar(fabrica.CreateClient(), cancelamento);

        var resposta = Sucesso(await cliente.CallToolAsync("buscar_no_manual", Argumentos("Regra da versao onze", v12), cancellationToken: cancelamento));

        Assert.All(resposta.Trechos, trecho => Assert.Equal(v12, trecho.Versao));
        Assert.DoesNotContain(resposta.Trechos, trecho => trecho.Versao == v11);
    }

    [Fact(DisplayName = "REQ-RECUP-002 Versão sem documento vigente volta vazia com versao_indisponivel")]
    public async Task VersaoIndisponivel()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var provedor = new ProvedorFalso();
        await using var cliente = await Conectar(banco.Criar(provedor, BuscaLigada).CreateClient(), cancelamento);

        var resposta = Sucesso(await cliente.CallToolAsync("buscar_no_manual", Argumentos("Qualquer", VersaoNova()), cancellationToken: cancelamento));

        Assert.Equal(("versao_indisponivel", 0), (resposta.Motivo, resposta.Trechos.Count));
        Assert.Equal(0, provedor.Chamadas);
    }

    [Theory(DisplayName = "REQ-RECUP-003 e 005 Argumento inválido chega como erro tipado argumento_invalido, sem embedding")]
    [InlineData(0, null)]
    [InlineData(5, "REGRA'; DROP")]
    public async Task ArgumentoInvalido(int quantidade, string? codigo)
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var provedor = new ProvedorFalso();
        await using var cliente = await Conectar(banco.Criar(provedor, BuscaLigada).CreateClient(), cancelamento);

        var erro = Falha(await cliente.CallToolAsync("buscar_no_manual", Argumentos("Registro 0000", "leiaute-12", quantidade, codigo), cancellationToken: cancelamento));

        Assert.Equal("argumento_invalido", erro.Codigo);
        Assert.Equal(0, provedor.Chamadas);
    }

    [Fact(DisplayName = "REQ-RECUP-003 Sem tipo e versão, o erro é tipado, e não do protocolo")]
    public async Task TipoEVersaoAusentes()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        await using var cliente = await Conectar(banco.Criar(new ProvedorFalso(), BuscaLigada).CreateClient(), cancelamento);

        var erro = Falha(await cliente.CallToolAsync("buscar_no_manual", new Dictionary<string, object?> { ["consulta"] = "x" }, cancellationToken: cancelamento));

        Assert.Equal("argumento_invalido", erro.Codigo);
    }

    [Fact(DisplayName = "REQ-RECUP-005 Com o código, o trecho que o contém vem primeiro, e os demais completam sem repetir")]
    public async Task BuscaPorCodigo()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var fabrica = banco.Criar(new ProvedorFalso(), BuscaLigada);
        var versao = await IngerirAsync(fabrica.Services, VersaoNova(), $"REGRA_TESTE_X verifica a natureza {Guid.NewGuid()}", $"Texto parecido com a consulta {Guid.NewGuid()}", $"Outro texto {Guid.NewGuid()}");
        await using var cliente = await Conectar(fabrica.CreateClient(), cancelamento);
        var parecido = Sucesso(await cliente.CallToolAsync("buscar_no_manual", Argumentos("x", versao, 10), cancellationToken: cancelamento))
            .Trechos.Single(trecho => trecho.Texto.Contains("Texto parecido", StringComparison.Ordinal)).Texto;

        var semCodigo = Sucesso(await cliente.CallToolAsync("buscar_no_manual", Argumentos(parecido, versao, 3), cancellationToken: cancelamento));
        var comCodigo = Sucesso(await cliente.CallToolAsync("buscar_no_manual", Argumentos(parecido, versao, 3, "REGRA_TESTE_X"), cancellationToken: cancelamento));

        Assert.Equal(parecido, semCodigo.Trechos[0].Texto);
        Assert.Contains("REGRA_TESTE_X", comCodigo.Trechos[0].Texto, StringComparison.Ordinal);
        Assert.True(comCodigo.Trechos[0].ContemCodigo);
        Assert.Equal(parecido, comCodigo.Trechos[1].Texto);
        Assert.Equal(3, comCodigo.Trechos.Select(trecho => trecho.IdTrecho).Distinct().Count());
    }

    [Fact(DisplayName = "REQ-RECUP-006 Com a busca desativada, a ferramenta responde busca_desativada, sem embedding")]
    public async Task BuscaDesativada()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var provedor = new ProvedorFalso();
        await using var cliente = await Conectar(banco.Criar(provedor).CreateClient(), cancelamento);

        var erro = Falha(await cliente.CallToolAsync("buscar_no_manual", Argumentos("Registro 0000", "leiaute-12"), cancellationToken: cancelamento));

        Assert.Equal("busca_desativada", erro.Codigo);
        Assert.Equal(0, provedor.Chamadas);
    }

    [Fact(DisplayName = "REQ-RECUP-004 O embedding da consulta fica no consumo, com a correlação, a execução e a finalidade")]
    public async Task ConsumoDaConsulta()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var fabrica = banco.Criar(new ProvedorFalso(), BuscaLigada);
        var versao = await IngerirAsync(fabrica.Services, VersaoNova(), $"Registro 0010 parametros {Guid.NewGuid()}");
        var execucao = Guid.NewGuid();
        var correlacao = $"corr-{Guid.NewGuid():N}";
        await using var cliente = await Conectar(fabrica.CreateClient(), cancelamento, correlacao);

        Sucesso(await cliente.CallToolAsync("buscar_no_manual", Argumentos("Registro 0010", versao, idExecucao: execucao.ToString()), cancellationToken: cancelamento));

        await using var conexao = await banco.Banco.OpenConnectionAsync(cancelamento);
        var registro = await conexao.QuerySingleAsync<(string Tipo, string Finalidade, string Correlacao)>(new CommandDefinition(
            "SELECT tipo, finalidade, correlacao FROM consumo_ia WHERE id_operacao = @Execucao", new { Execucao = execucao }, cancellationToken: cancelamento));
        Assert.Equal(("embedding", "consulta", correlacao), registro);
    }

    [Fact(DisplayName = "ADR 0005 Sem estado: as chamadas funcionam alternando entre duas instâncias")]
    public async Task DuasInstancias()
    {
        var cancelamento = TestContext.Current.CancellationToken;
        var a = banco.Criar(new ProvedorFalso(), BuscaLigada);
        var b = banco.Criar(new ProvedorFalso(), BuscaLigada);
        var versao = await IngerirAsync(a.Services, VersaoNova(), $"Registro K155 saldos {Guid.NewGuid()}");
        var alternador = new Alternador(a.Server.CreateHandler(), b.Server.CreateHandler());
        await using var cliente = await Conectar(new HttpClient(alternador) { BaseAddress = new Uri("http://localhost") }, cancelamento);

        await cliente.ListToolsAsync(cancellationToken: cancelamento);
        for (var i = 0; i < 4; i++)
        {
            Assert.Single(Sucesso(await cliente.CallToolAsync("buscar_no_manual", Argumentos("Registro K155", versao), cancellationToken: cancelamento)).Trechos);
        }

        Assert.All(alternador.Atendidas, atendidas => Assert.True(atendidas >= 2));
    }

    [Fact(DisplayName = "REQ-CONFIG-002 Busca ligada sem a chave da OpenAI impede a inicialização")]
    public void BuscaLigadaSemChave()
    {
        var fabrica = banco.Criar(new ProvedorFalso(), ("Busca:Habilitada", "true"));

        var erro = Assert.ThrowsAny<Exception>(() => fabrica.CreateClient());

        Assert.Contains("OpenAI:ApiKey", erro.ToString(), StringComparison.Ordinal);
    }
}
