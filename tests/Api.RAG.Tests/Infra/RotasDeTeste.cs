using Api.RAG.Infraestrutura.Autenticacao;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Api.RAG.Tests.Infra;

/// <summary>
/// Servidor em memória com o mesmo registro de autenticação da Api.RAG e rotas que existem só nos testes,
/// protegidas pelas políticas que as mudanças 05 (ingestão) e 06 (MCP) vão usar (D6).
/// </summary>
internal sealed class RotasDeTeste : IAsyncDisposable
{
    public const string Ingestao = "/teste/ingestao";
    public const string FerramentaMcp = "/teste/mcp";

    private readonly WebApplication app;
    private int execucoes;

    private RotasDeTeste(WebApplication app, ProvedorDeLogEmMemoria logs)
    {
        this.app = app;
        Logs = logs;
    }

    public ProvedorDeLogEmMemoria Logs { get; }

    /// <summary>Quantas vezes alguma operação protegida chegou a executar.</summary>
    public int Execucoes => Volatile.Read(ref execucoes);

    public static async Task<RotasDeTeste> Iniciar()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(GeradorDeTokens.Configuracao);
        var logs = new ProvedorDeLogEmMemoria();
        builder.Logging.ClearProviders().AddProvider(logs);
        builder.Services.AdicionarAutenticacao(builder.Configuration);
        GeradorDeTokens.ValidarLocalmente(builder.Services);

        var rotas = new RotasDeTeste(builder.Build(), logs);
        rotas.app.UseAuthentication();
        rotas.app.UseAuthorization();
        rotas.app.MapPost(Ingestao, rotas.Executar).RequireAuthorization(PoliticasDoRag.AdministradorRag);
        rotas.app.MapPost(FerramentaMcp, rotas.Executar).RequireAuthorization(PoliticasDoRag.ServicoAgente);
        await rotas.app.StartAsync(TestContext.Current.CancellationToken);
        return rotas;
    }

    public HttpClient CriarCliente() => app.GetTestClient();

    public ValueTask DisposeAsync() => app.DisposeAsync();

    private IResult Executar()
    {
        Interlocked.Increment(ref execucoes);
        return Results.Ok();
    }
}
