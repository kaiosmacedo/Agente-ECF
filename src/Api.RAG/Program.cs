using Api.RAG.Endpoints;
using Api.RAG.Infraestrutura.Configuracao;
using Api.RAG.Infraestrutura.Conhecimento;
using Api.RAG.Infraestrutura.Consumo;
using Api.RAG.Infraestrutura.Hospedagem;
using Api.RAG.Infraestrutura.Observabilidade;
using Api.RAG.Infraestrutura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(EnderecoDeEscuta.Resolver(builder.Configuration["PORT"]));
builder.Services.AdicionarOpcoesValidadas<OpcoesObservabilidade>(builder.Configuration, OpcoesObservabilidade.Secao);
var identidade = IdentidadeServico.De("Api.RAG", typeof(Program).Assembly);
builder.AdicionarObservabilidade(identidade);
builder.Services.AddHealthChecks();
builder.Services.AdicionarPersistencia(builder.Configuration);
builder.Services.AdicionarConsumo(builder.Configuration);
builder.Services.AdicionarBaseDeConhecimento(builder.Configuration);
builder.Services.AdicionarBusca(builder.Configuration);
builder.Services.AdicionarServidorMcp(identidade.Versao);
builder.Services.AddOpenApi();

var app = builder.Build();

// O CD aplica as migrações antes do deploy; o serviço nunca as aplica ao iniciar.
if (args.Contains("--migrar"))
{
    await app.Services.GetRequiredService<Migrador>().AplicarAsync(CancellationToken.None);
    return;
}

// Operações eventuais, fora do Vercel: ver docs/ci-cd.md.
if (args.Contains("--ingerir") || args.Contains("--avaliar"))
{
    Environment.ExitCode = await ComandosDeLinha.ExecutarAsync(args, app.Services, Console.Out);
    return;
}

app.UseMiddleware<CorrelacaoMiddleware>();
app.MapearSaude();
app.MapearConsumo();
app.MapearDocumentos();
app.MapearMcp();
app.MapOpenApi();

await app.RunAsync();

/// <summary>Ponto de entrada, exposto para os testes HTTP em memória.</summary>
public partial class Program;
