using Api.Agente.Endpoints;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Cors;
using Api.Agente.Infraestrutura.Hospedagem;
using Api.Agente.Infraestrutura.Integracao;
using Api.Agente.Infraestrutura.Observabilidade;
using Api.Agente.Infraestrutura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(EnderecoDeEscuta.Resolver(builder.Configuration["PORT"]));
builder.Services.AdicionarOpcoesValidadas<OpcoesObservabilidade>(builder.Configuration, OpcoesObservabilidade.Secao);
builder.AdicionarObservabilidade(IdentidadeServico.De("Api.Agente", typeof(Program).Assembly));
builder.Services.AddHealthChecks();
builder.Services.AdicionarCorsSofia(builder.Configuration);
builder.Services.AdicionarServidorA2A(builder.Configuration);
builder.Services.AdicionarPersistencia(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

// O CD aplica as migrações antes do deploy; o serviço nunca as aplica ao iniciar.
if (args.Contains("--migrar"))
{
    await app.Services.GetRequiredService<Migrador>().AplicarAsync(CancellationToken.None);
    return;
}

app.UseMiddleware<CorrelacaoMiddleware>();
app.UsarCorsSofia();
app.MapearSaude();
app.MapearA2A();
app.MapearAnalises();
app.MapOpenApi();

await app.RunAsync();

/// <summary>Ponto de entrada, exposto para os testes HTTP em memória.</summary>
public partial class Program;
