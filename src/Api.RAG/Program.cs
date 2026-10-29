using Api.RAG.Endpoints;
using Api.RAG.Infraestrutura.Configuracao;
using Api.RAG.Infraestrutura.Hospedagem;
using Api.RAG.Infraestrutura.Observabilidade;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(EnderecoDeEscuta.Resolver(builder.Configuration["PORT"]));
builder.Services.AdicionarOpcoesValidadas<OpcoesObservabilidade>(builder.Configuration, OpcoesObservabilidade.Secao);
builder.AdicionarObservabilidade(IdentidadeServico.De("Api.RAG", typeof(Program).Assembly));
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelacaoMiddleware>();
app.MapearSaude();

app.Run();

/// <summary>Ponto de entrada, exposto para os testes HTTP em memória.</summary>
public partial class Program;
