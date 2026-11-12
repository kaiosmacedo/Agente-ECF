using System.Net;
using System.Text;
using System.Text.Json;
using Api.Agente.Aplicacao.Perguntas;
using Api.Agente.Dominio.Consumo;
using Api.Agente.Infraestrutura.Perguntas;
using Api.Agente.Infraestrutura.Persistencia;
using Api.Agente.Tests.Infra;
using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Api.Agente.Tests;

/// <summary>Registro das perguntas e do consumo no PostgreSQL em contêiner (mudança 17, D9 e D10).</summary>
public sealed class PerguntasNoBancoTests(BancoDeTeste banco) : IClassFixture<BancoDeTeste>
{
    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    private static RegistroDaPergunta Registro(Guid? idSolicitacao, string status = "concluido", DateTimeOffset? quando = null) => new(
        Guid.NewGuid(), idSolicitacao, "a2a", status, null, new string('a', 64), 31, "leiaute-12",
        [new TrechoRegistrado(Guid.NewGuid(), 63, 0.71)], [], DesfechoDaRedacao.Aceita, "gpt-4.1-mini", quando ?? DateTimeOffset.UtcNow, quando ?? DateTimeOffset.UtcNow);

    [Fact(DisplayName = "REQ-PERGUNTA-009 A tabela de perguntas não tem coluna para o texto da pergunta nem o da resposta")]
    public async Task SemColunaDeTexto()
    {
        await using var conexao = await banco.Banco.OpenConnectionAsync(Cancelamento);
        var colunas = (await conexao.QueryAsync<string>("SELECT column_name FROM information_schema.columns WHERE table_name = 'perguntas'")).ToList();

        Assert.Contains("pergunta_sha256", colunas);
        Assert.DoesNotContain(colunas, coluna => coluna is "pergunta" or "resposta" or "texto");
    }

    [Fact(DisplayName = "REQ-PERGUNTA-009 Retentativa com o mesmo idSolicitacao não grava duas vezes nem conta duas vezes")]
    public async Task Retentativa()
    {
        var repositorio = new RepositorioDePerguntas(banco.Banco);
        var idSolicitacao = Guid.NewGuid();
        var antes = DateTimeOffset.UtcNow.AddSeconds(-1);
        var contagemAntes = await repositorio.ContarRespondidasDesdeAsync(antes, Cancelamento);

        await repositorio.RegistrarAsync(Registro(idSolicitacao), Cancelamento);
        await repositorio.RegistrarAsync(Registro(idSolicitacao), Cancelamento);
        await repositorio.RegistrarAsync(Registro(null, "erro"), Cancelamento);

        Assert.Equal(contagemAntes + 1, await repositorio.ContarRespondidasDesdeAsync(antes, Cancelamento));
        await using var conexao = await banco.Banco.OpenConnectionAsync(Cancelamento);
        Assert.Equal(1, await conexao.ExecuteScalarAsync<int>("SELECT count(*) FROM perguntas WHERE id_solicitacao = @idSolicitacao", new { idSolicitacao }));
        var trechos = await conexao.ExecuteScalarAsync<string>("SELECT trechos::text FROM perguntas WHERE id_solicitacao = @idSolicitacao", new { idSolicitacao });
        Assert.Contains("\"pagina\": 63", trechos, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-PERGUNTA-009 A contagem do limite só considera o dia em UTC")]
    public async Task ContagemDoDia()
    {
        var repositorio = new RepositorioDePerguntas(banco.Banco);
        var hoje = new DateTimeOffset(DateTimeOffset.UtcNow.UtcDateTime.Date, TimeSpan.Zero);
        var contagemAntes = await repositorio.ContarRespondidasDesdeAsync(hoje, Cancelamento);

        await repositorio.RegistrarAsync(Registro(Guid.NewGuid(), quando: hoje.AddMinutes(-5)), Cancelamento);

        Assert.Equal(contagemAntes, await repositorio.ContarRespondidasDesdeAsync(hoje, Cancelamento));
    }

    [Fact(DisplayName = "D10 Consumo da pergunta gravado com tokens, custo e finalidade")]
    public async Task Consumo()
    {
        var id = Guid.NewGuid();
        await new RepositorioDeConsumo(banco.Banco).RegistrarAsync(
            new RegistroDeConsumo(
                id, Guid.NewGuid(), "corr-1", "openai", "gpt-4.1-mini", TipoDeChamada.Geracao, StatusDaChamada.Sucesso, DateTimeOffset.UtcNow, 812.5, 1,
                900, OrigemDoValor.Informado, 60, OrigemDoValor.Informado, 0.000456m, OrigemDoValor.Estimado, "2026-11-12", FinalidadeDaChamada.Pergunta),
            Cancelamento);

        await using var conexao = await banco.Banco.OpenConnectionAsync(Cancelamento);
        var linha = await conexao.QuerySingleAsync<(string Finalidade, int TokensEntrada, decimal CustoUsd)>(
            "SELECT finalidade, tokens_entrada, custo_usd FROM consumo_ia WHERE id = @id", new { id });
        Assert.Equal(("pergunta", 900, 0.000456m), linha);
    }
}

/// <summary>O redator com o Semantic Kernel diante de respostas HTTP simuladas da OpenAI (mudança 17, D5).</summary>
public sealed class RedatorSemanticKernelTests
{
    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    private sealed class ProvedorSimulado(HttpStatusCode status, string corpo) : HttpMessageHandler
    {
        public List<string> Corpos { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Corpos.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            return new HttpResponseMessage(status) { Content = new StringContent(corpo, Encoding.UTF8, "application/json") };
        }
    }

    private static (RedatorSemanticKernel Redator, ProvedorSimulado Provedor) Redator(HttpStatusCode status, string corpo)
    {
        var provedor = new ProvedorSimulado(status, corpo);
        var chat = new OpenAIChatCompletionService("gpt-4.1-mini", "chave-sintetica-de-teste", httpClient: new HttpClient(provedor));
        return (new RedatorSemanticKernel(chat, Options.Create(new OpcoesDoLlm())), provedor);
    }

    private static MaterialDaRedacao Material() => new("O que é o Registro 0000?", "leiaute-12", [TrechosSinteticos.Trecho(63, 0.71)]);

    [Fact(DisplayName = "D5 Saída estruturada estrita, temperatura 0, prompt pergunta-v1 e tokens informados")]
    public async Task Sucesso()
    {
        const string Completo = """
            {"id":"chatcmpl-1","object":"chat.completion","created":1794480000,"model":"gpt-4.1-mini",
             "choices":[{"index":0,"message":{"role":"assistant","content":"{\"respondida\":false,\"resposta\":\"\",\"citacoes\":[]}"},"finish_reason":"stop"}],
             "usage":{"prompt_tokens":812,"completion_tokens":19,"total_tokens":831}}
            """;
        var (redator, provedor) = Redator(HttpStatusCode.OK, Completo);

        var saida = await redator.RedigirAsync(Material(), Cancelamento);

        Assert.Equal((812, 19), (saida.TokensEntrada, saida.TokensSaida));
        Assert.NotNull(ValidadorDaRedacao.Validar(saida.Texto, Material().Trechos));
        using var pedido = JsonDocument.Parse(Assert.Single(provedor.Corpos));
        var raiz = pedido.RootElement;
        Assert.Equal("gpt-4.1-mini", raiz.GetProperty("model").GetString());
        Assert.Equal(0, raiz.GetProperty("temperature").GetDouble());
        Assert.Equal("json_schema", raiz.GetProperty("response_format").GetProperty("type").GetString());
        Assert.True(raiz.GetProperty("response_format").GetProperty("json_schema").GetProperty("strict").GetBoolean());
        Assert.Equal("system", raiz.GetProperty("messages")[0].GetProperty("role").GetString());
        Assert.Contains("Use somente o que está escrito nos trechos", raiz.GetProperty("messages")[0].GetProperty("content").GetString(), StringComparison.Ordinal);
        Assert.Contains("<pergunta>", raiz.GetProperty("messages")[1].GetProperty("content").GetString(), StringComparison.Ordinal);
    }

    [Theory(DisplayName = "D5 429 e 5xx são transitórios, 400 não; cada chamada é uma tentativa")]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public async Task Falha(HttpStatusCode status, bool transitoria)
    {
        var (redator, provedor) = Redator(status, """{"error":{"message":"falha sintética","type":"server_error"}}""");

        var falha = await Assert.ThrowsAsync<FalhaDoRedator>(() => redator.RedigirAsync(Material(), Cancelamento));

        Assert.Equal(transitoria, falha.Transitoria);
        Assert.DoesNotContain("falha sintética", falha.Message, StringComparison.Ordinal);
        Assert.Single(provedor.Corpos);
    }
}
