using System.ComponentModel;
using System.Net;
using System.Text.Json;
using Api.Agente.Aplicacao.Conhecimento;
using Api.Agente.Infraestrutura.Conhecimento;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Api.Agente.Tests;

/// <summary>
/// Cliente MCP da consulta ao manual contra um servidor MCP falso, em memória e sem estado, que segue o contrato 1.0 de
/// <c>buscar_no_manual</c> (D6, D7; REQ-CONSULTA-003, 007). Nenhum teste usa rede.
/// </summary>
public sealed class ClienteMcpTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>O que o servidor falso responde e o que ele recebeu.</summary>
    internal sealed class Roteiro
    {
        public string Modo { get; set; } = "trechos";

        public int Chamadas;

        public string? CorrelacaoRecebida { get; set; }

        public Dictionary<string, string?> Argumentos { get; } = [];
    }

    [McpServerToolType]
    internal sealed class FerramentaFalsa(Roteiro roteiro, Microsoft.AspNetCore.Http.IHttpContextAccessor http)
    {
        [McpServerTool(Name = "buscar_no_manual")]
        [Description("Dublê do contrato 1.0.")]
        public async Task<CallToolResult> Buscar(string? consulta = null, string? tipoDocumento = null, string? versao = null, int? quantidade = null, string? codigo = null, string? idExecucao = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref roteiro.Chamadas);
            roteiro.CorrelacaoRecebida = http.HttpContext?.Request.Headers["X-Correlation-Id"].ToString();
            roteiro.Argumentos["tipoDocumento"] = tipoDocumento;
            roteiro.Argumentos["versao"] = versao;
            roteiro.Argumentos["codigo"] = codigo;
            roteiro.Argumentos["idExecucao"] = idExecucao;
            if (roteiro.Modo == "demorar")
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }

            return roteiro.Modo switch
            {
                "trechos" => Texto(false, new
                {
                    versaoContrato = "1.0",
                    motivo = (string?)null,
                    trechos = new[] { new { idTrecho = Guid.NewGuid(), idDocumento = Guid.NewGuid(), titulo = "Manual", versao, pagina = 63, secao = (string?)null, relevancia = 0.91, contemCodigo = true, texto = "Registro 0000" } },
                }),
                "versao_indisponivel" => Texto(false, new { versaoContrato = "1.0", motivo = "versao_indisponivel", trechos = Array.Empty<object>() }),
                "versao_2" => Texto(false, new { versaoContrato = "2.0", motivo = (string?)null, trechos = Array.Empty<object>() }),
                _ => Texto(true, new { versaoContrato = "1.0", erro = new { codigo = roteiro.Modo, mensagem = "Recusada no teste." } }),
            };
        }

        private static CallToolResult Texto(bool erro, object corpo) => new()
        {
            IsError = erro,
            Content = [new TextContentBlock { Text = JsonSerializer.Serialize(corpo, Json) }],
        };
    }

    private sealed class FabricaDeHttp(Func<HttpMessageHandler> criar) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(criar(), disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
    }

    /// <summary>Responde sempre o mesmo status e conta as requisições, sem servidor MCP.</summary>
    private sealed class RespostaFixa(HttpStatusCode status) : HttpMessageHandler
    {
        public int Requisicoes;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Requisicoes);
            return Task.FromResult(new HttpResponseMessage(status));
        }
    }

    private static async Task<(WebApplication Servidor, Roteiro Roteiro)> ServidorFalso(string modo)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        var roteiro = new Roteiro { Modo = modo };
        builder.Services.AddSingleton(roteiro);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMcpServer().WithHttpTransport(opcoes => opcoes.SessionMode = HttpServerSessionMode.Stateless).WithTools<FerramentaFalsa>();
        var servidor = builder.Build();
        servidor.MapMcp("/mcp");
        await servidor.StartAsync(TestContext.Current.CancellationToken);
        return (servidor, roteiro);
    }

    private static ClienteMcpDoManual Cliente(Func<HttpMessageHandler> handler, int timeout = 10) => new(
        new FabricaDeHttp(handler),
        Options.Create(new OpcoesDoConhecimento { UrlDoServidorMcp = "http://conhecimento.teste/mcp", TimeoutEmSegundos = timeout }),
        TimeProvider.System,
        NullLogger<ClienteMcpDoManual>.Instance);

    private static readonly Guid Execucao = Guid.NewGuid();

    private static PedidoDeTrechos Pedido() => new("Registro 0000, campo COD_VER: valor fora da lista.", "manual-ecf", "leiaute-12", "0000", Execucao, "corr-cliente");

    private static async Task<(RespostaDaConsulta Resposta, Roteiro Roteiro)> Consultar(string modo, int timeout = 10)
    {
        var (servidor, roteiro) = await ServidorFalso(modo);
        await using (servidor)
        {
            var resposta = await Cliente(() => servidor.GetTestServer().CreateHandler(), timeout).BuscarAsync(Pedido(), TestContext.Current.CancellationToken);
            return (resposta, roteiro);
        }
    }

    [Fact(DisplayName = "REQ-CONSULTA-002 Os trechos do contrato 1.0 chegam com documento, versão, página e relevância")]
    public async Task Trechos()
    {
        var (resposta, roteiro) = await Consultar("trechos");

        Assert.Equal(DesfechoDaConsulta.Trechos, resposta.Desfecho);
        var trecho = Assert.Single(resposta.Trechos);
        Assert.Equal(("leiaute-12", 63, 0.91), (trecho.Versao, trecho.Pagina, trecho.Relevancia));
        Assert.Equal(("manual-ecf", "leiaute-12", "0000", Execucao.ToString()), (roteiro.Argumentos["tipoDocumento"], roteiro.Argumentos["versao"], roteiro.Argumentos["codigo"], roteiro.Argumentos["idExecucao"]));
    }

    [Fact(DisplayName = "SQ-07 A correlação vai no cabeçalho X-Correlation-Id da chamada MCP")]
    public async Task Correlacao()
    {
        var (_, roteiro) = await Consultar("trechos");

        Assert.Equal("corr-cliente", roteiro.CorrelacaoRecebida);
    }

    [Theory(DisplayName = "REQ-CONSULTA-007 Os erros tipados da ferramenta viram o desfecho certo, sem repetir a chamada")]
    [InlineData("busca_desativada", "BuscaDesativada")]
    [InlineData("limite_de_uso_atingido", "LimiteDeUsoAtingido")]
    [InlineData("versao_indisponivel", "VersaoIndisponivel")]
    [InlineData("argumento_invalido", "FalhaTecnica")]
    [InlineData("falha_do_provedor", "FalhaTecnica")]
    [InlineData("versao_2", "FalhaTecnica")]
    public async Task ErrosTipados(string modo, string desfecho)
    {
        var (resposta, roteiro) = await Consultar(modo);

        Assert.Equal(Enum.Parse<DesfechoDaConsulta>(desfecho), resposta.Desfecho);
        Assert.Equal(1, roteiro.Chamadas);
    }

    [Fact(DisplayName = "REQ-CONSULTA-003 Sem resposta dentro do timeout, a chamada é repetida 2 vezes e termina em falha técnica")]
    public async Task SemRespostaNoTempo()
    {
        var (resposta, roteiro) = await Consultar("demorar", timeout: 1);

        Assert.Equal(DesfechoDaConsulta.FalhaTecnica, resposta.Desfecho);
        Assert.Equal(3, roteiro.Chamadas);
    }

    [Fact(DisplayName = "REQ-CONSULTA-003 Servidor fora do ar (HTTP 503) é repetido 2 vezes e termina em falha técnica")]
    public async Task ServidorIndisponivel()
    {
        var handler = new RespostaFixa(HttpStatusCode.ServiceUnavailable);

        var resposta = await Cliente(() => handler).BuscarAsync(Pedido(), TestContext.Current.CancellationToken);

        Assert.Equal(DesfechoDaConsulta.FalhaTecnica, resposta.Desfecho);
        Assert.Equal(3, handler.Requisicoes);
    }

    [Fact(DisplayName = "REQ-CONSULTA-003 Erro do cliente (HTTP 400) não é repetido")]
    public async Task ErroDoCliente()
    {
        var handler = new RespostaFixa(HttpStatusCode.BadRequest);

        var resposta = await Cliente(() => handler).BuscarAsync(Pedido(), TestContext.Current.CancellationToken);

        // Uma tentativa só: com 4xx, o SDK tenta uma vez a revisão anterior do protocolo, e o cliente não repete.
        Assert.Equal(DesfechoDaConsulta.FalhaTecnica, resposta.Desfecho);
        Assert.InRange(handler.Requisicoes, 1, 2);
    }
}
