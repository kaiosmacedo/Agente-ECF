using System.ComponentModel;
using System.Text.Json;
using Api.RAG.Aplicacao.Conhecimento;
using Api.RAG.Infraestrutura.Observabilidade;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Api.RAG.Endpoints;

/// <summary>Contrato 1.0 da saída de <c>buscar_no_manual</c> (mudança 06, D2; <c>docs/contratos/mcp-buscar-no-manual.md</c>).</summary>
internal sealed record RespostaDaBuscaNoManual(string VersaoContrato, string? Motivo, IReadOnlyList<TrechoDaResposta> Trechos);

/// <summary>Um trecho com a referência recuperável (P06).</summary>
internal sealed record TrechoDaResposta(
    Guid IdTrecho, Guid IdDocumento, string Titulo, string Versao, int Pagina, string? Secao, double Relevancia, bool ContemCodigo, string Texto);

/// <summary>Erro tipado da ferramenta, devolvido como resultado com <c>isError</c>, sem exceção nem log de erro.</summary>
internal sealed record ErroDaFerramenta(string VersaoContrato, CorpoDoErro Erro);

internal sealed record CorpoDoErro(string Codigo, string Mensagem);

/// <summary>
/// Ferramentas MCP da base de conhecimento (REQ-RECUP-001 a 006). Sem estado entre chamadas (ADR 0005): cada uma traz
/// tudo o que precisa. Sem autenticação na primeira versão (SQ-12); o limite de uso fica no serviço de busca.
/// </summary>
[McpServerToolType]
internal sealed partial class FerramentasDoManual(ServicoDeBusca busca, IHttpContextAccessor http, ILogger<FerramentasDoManual> logger)
{
    public const string VersaoContrato = "1.0";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [McpServerTool(Name = "buscar_no_manual", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Busca trechos dos manuais oficiais vigentes de um tipo e de uma versão, com documento, versão, página e relevância. Contrato 1.0.")]
    public async Task<CallToolResult> BuscarNoManualAsync(
        [Description("Texto da consulta, de 1 a 500 caracteres. Obrigatório.")] string? consulta = null,
        [Description("Tipo do documento na base, como manual-ecf. Obrigatório.")] string? tipoDocumento = null,
        [Description("Versão do documento na base, como leiaute-12. Obrigatória.")] string? versao = null,
        [Description("Quantidade de trechos, de 1 a 10; padrão 5.")] int? quantidade = null,
        [Description("Código literal a procurar primeiro, como REGRA_X ou K155: letras maiúsculas, dígitos e sublinhado.")] string? codigo = null,
        [Description("UUID da execução que pediu a busca, para o registro de consumo.")] string? idExecucao = null,
        CancellationToken cancellationToken = default)
    {
        var correlacao = http.HttpContext?.Response.Headers[CorrelacaoMiddleware.Cabecalho].ToString();
        try
        {
            var resultado = await busca.BuscarAsync(
                new PedidoDeBusca(consulta, tipoDocumento, versao, quantidade, codigo, idExecucao), correlacao, cancellationToken);
            BuscaFeita(logger, resultado.Trechos.Count, resultado.Motivo ?? "-");
            return Resultado(new RespostaDaBuscaNoManual(VersaoContrato, resultado.Motivo, [.. resultado.Trechos.Select(Converter)]), erro: false);
        }
        catch (BuscaRecusada recusa)
        {
            var codigoDoErro = Codigo(recusa.Erro);
            BuscaRecusadaLog(logger, codigoDoErro);
            return Resultado(new ErroDaFerramenta(VersaoContrato, new CorpoDoErro(codigoDoErro, recusa.Message)), erro: true);
        }
    }

    private static CallToolResult Resultado<T>(T corpo, bool erro) => new()
    {
        IsError = erro,
        Content = [new TextContentBlock { Text = JsonSerializer.Serialize(corpo, Json) }],
    };

    public static string Codigo(ErroDaBusca erro) => erro switch
    {
        ErroDaBusca.ArgumentoInvalido => "argumento_invalido",
        ErroDaBusca.BuscaDesativada => "busca_desativada",
        ErroDaBusca.LimiteDeUsoAtingido => "limite_de_uso_atingido",
        ErroDaBusca.FalhaDoProvedor => "falha_do_provedor",
        _ => "falha_tecnica",
    };

    private static TrechoDaResposta Converter(TrechoEncontrado trecho) => new(
        trecho.IdTrecho,
        trecho.IdDocumento,
        trecho.Titulo,
        trecho.Versao,
        trecho.Pagina,
        trecho.Secao,
        Math.Round(trecho.Similaridade, 4, MidpointRounding.AwayFromZero),
        trecho.ContemCodigo,
        trecho.Texto);

    // Só contagens e códigos: nunca o texto da consulta nem dos trechos (SQ-04).
    [LoggerMessage(EventId = 20, Level = LogLevel.Information, Message = "Busca no manual: {Trechos} trechos, motivo {Motivo}")]
    private static partial void BuscaFeita(ILogger logger, int trechos, string motivo);

    [LoggerMessage(EventId = 21, Level = LogLevel.Warning, Message = "Busca no manual recusada: {Codigo}")]
    private static partial void BuscaRecusadaLog(ILogger logger, string codigo);
}

/// <summary>Servidor MCP sem estado em <c>/mcp</c> (ADR 0005).</summary>
internal static class McpEndpoints
{
    public const string Caminho = "/mcp";

    public static IServiceCollection AdicionarServidorMcp(this IServiceCollection servicos, string versaoDoServico)
    {
        servicos.AddHttpContextAccessor();
        servicos
            .AddMcpServer(opcoes => opcoes.ServerInfo = new() { Name = "agente-ecf-conhecimento", Version = versaoDoServico })
            .WithHttpTransport(opcoes => opcoes.SessionMode = HttpServerSessionMode.Stateless)
            .WithTools<FerramentasDoManual>();
        return servicos;
    }

    public static IEndpointRouteBuilder MapearMcp(this IEndpointRouteBuilder rotas)
    {
        rotas.MapMcp(Caminho);
        return rotas;
    }
}
