using Api.Agente.Endpoints;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Hospedagem;
using Api.Agente.Infraestrutura.Observabilidade;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(EnderecoDeEscuta.Resolver(builder.Configuration["PORT"]));
builder.Services.AdicionarOpcoesValidadas<OpcoesObservabilidade>(builder.Configuration, OpcoesObservabilidade.Secao);
builder.AdicionarObservabilidade(IdentidadeServico.De("Api.Agente", typeof(Program).Assembly));
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelacaoMiddleware>();
app.MapearSaude();

app.Run();

/// <summary>Ponto de entrada, exposto para os testes HTTP em memória.</summary>
public partial class Program;
