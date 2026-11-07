using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Api.Agente.Aplicacao.Conhecimento;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Observabilidade;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Api.Agente.Infraestrutura.Conhecimento;

/// <summary>Opções da seção <c>Conhecimento</c> (mudança 06, D6 e D10). Nada aqui é segredo.</summary>
internal sealed class OpcoesDoConhecimento : IValidatableObject
{
    public const string Secao = "Conhecimento";

    /// <summary>Endereço da ferramenta MCP do serviço de conhecimento. Em produção, vem de <c>appsettings.Production.json</c>.</summary>
    [Required]
    public string UrlDoServidorMcp { get; init; } = "http://localhost:8081/mcp";

    [Range(1, 60)]
    public int TimeoutEmSegundos { get; init; } = 10;

    [Range(1, 50)]
    public int MaximoDeConsultasPorExecucao { get; init; } = 10;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.TryCreate(UrlDoServidorMcp, UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https"))
        {
            yield return new ValidationResult("O endereço precisa ser absoluto, em http ou https.", [nameof(UrlDoServidorMcp)]);
        }
    }
}

/// <summary>
/// Cliente da ferramenta <c>buscar_no_manual</c> 1.0 pelo transporte HTTP sem estado do MCP (ADR 0005). Repete só
/// timeout, falha de conexão e erro HTTP do servidor, até 2 vezes, com 0,5 e 1 s de espera (D6). Os erros tipados da
/// ferramenta não são repetidos. Nunca lança por falha da chamada: devolve falha técnica (REQ-CONSULTA-003).
/// </summary>
internal sealed partial class ClienteMcpDoManual(
    IHttpClientFactory fabrica, IOptions<OpcoesDoConhecimento> opcoes, TimeProvider relogio, ILogger<ClienteMcpDoManual> logger) : IConsultaAoManual
{
    public const string NomeDoClienteHttp = "conhecimento-mcp";
    public const string Ferramenta = "buscar_no_manual";
    public const string VersaoDoContrato = "1.0";

    private static readonly TimeSpan[] Esperas = [TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1)];
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private sealed record Resposta(string? VersaoContrato, string? Motivo, IReadOnlyList<TrechoDoManual>? Trechos);

    private sealed record RespostaDeErro(string? VersaoContrato, CorpoDoErro? Erro);

    private sealed record CorpoDoErro(string? Codigo, string? Mensagem);

    public async Task<RespostaDaConsulta> BuscarAsync(PedidoDeTrechos pedido, CancellationToken cancelamento)
    {
        for (var tentativa = 0; ; tentativa++)
        {
            try
            {
                return await ChamarAsync(pedido, cancelamento);
            }
            catch (Exception erro) when (Transitorio(erro, cancelamento) && tentativa < Esperas.Length)
            {
                ConsultaRepetida(logger, tentativa + 1, erro.GetType().Name);
                await Task.Delay(Esperas[tentativa], relogio, cancelamento);
            }
            catch (Exception erro) when (!cancelamento.IsCancellationRequested && erro is not OutOfMemoryException)
            {
                // Sem a mensagem: ela pode trazer o endereço ou trechos da resposta (SQ-04).
                ConsultaFalhou(logger, erro.GetType().Name);
                return RespostaDaConsulta.Sem(DesfechoDaConsulta.FalhaTecnica);
            }
        }
    }

    private async Task<RespostaDaConsulta> ChamarAsync(PedidoDeTrechos pedido, CancellationToken cancelamento)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancelamento);
        limite.CancelAfter(TimeSpan.FromSeconds(opcoes.Value.TimeoutEmSegundos));
        var transporte = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(opcoes.Value.UrlDoServidorMcp),
                TransportMode = HttpTransportMode.StreamableHttp,
                AdditionalHeaders = pedido.Correlacao is { } correlacao ? new Dictionary<string, string> { [CorrelacaoMiddleware.Cabecalho] = correlacao } : null,
            },
            fabrica.CreateClient(NomeDoClienteHttp));
        await using var cliente = await McpClient.CreateAsync(transporte, cancellationToken: limite.Token);
        var argumentos = new Dictionary<string, object?>
        {
            ["consulta"] = pedido.Consulta,
            ["tipoDocumento"] = pedido.TipoDocumento,
            ["versao"] = pedido.Versao,
            ["idExecucao"] = pedido.IdExecucao.ToString(),
        };
        if (pedido.Codigo is { } codigo)
        {
            argumentos["codigo"] = codigo;
        }

        var resultado = await cliente.CallToolAsync(Ferramenta, argumentos, cancellationToken: limite.Token);
        return Interpretar(resultado);
    }

    private RespostaDaConsulta Interpretar(CallToolResult resultado)
    {
        var texto = resultado.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text;
        if (texto is null)
        {
            return Contrato("sem texto");
        }

        if (resultado.IsError == true)
        {
            var erro = JsonSerializer.Deserialize<RespostaDeErro>(texto, Json)?.Erro?.Codigo;
            ErroDaFerramenta(logger, erro ?? "-");
            return RespostaDaConsulta.Sem(erro switch
            {
                "busca_desativada" => DesfechoDaConsulta.BuscaDesativada,
                "limite_de_uso_atingido" => DesfechoDaConsulta.LimiteDeUsoAtingido,
                _ => DesfechoDaConsulta.FalhaTecnica,
            });
        }

        var resposta = JsonSerializer.Deserialize<Resposta>(texto, Json);
        if (resposta is not { VersaoContrato: VersaoDoContrato, Trechos: { } trechos })
        {
            return Contrato("versão ou formato");
        }

        return resposta.Motivo == "versao_indisponivel"
            ? RespostaDaConsulta.Sem(DesfechoDaConsulta.VersaoIndisponivel)
            : new RespostaDaConsulta(DesfechoDaConsulta.Trechos, trechos);
    }

    private RespostaDaConsulta Contrato(string motivo)
    {
        ContratoInesperado(logger, motivo);
        return RespostaDaConsulta.Sem(DesfechoDaConsulta.FalhaTecnica);
    }

    private static bool Transitorio(Exception erro, CancellationToken cancelamento) =>
        !cancelamento.IsCancellationRequested
        && erro is OperationCanceledException or TimeoutException or HttpRequestException { StatusCode: null or >= System.Net.HttpStatusCode.InternalServerError };

    [LoggerMessage(EventId = 30, Level = LogLevel.Warning, Message = "Consulta ao manual repetida, tentativa {Tentativa}: {Erro}")]
    private static partial void ConsultaRepetida(ILogger logger, int tentativa, string erro);

    [LoggerMessage(EventId = 31, Level = LogLevel.Error, Message = "Consulta ao manual falhou: {Erro}")]
    private static partial void ConsultaFalhou(ILogger logger, string erro);

    [LoggerMessage(EventId = 32, Level = LogLevel.Warning, Message = "A ferramenta do manual recusou a consulta: {Codigo}")]
    private static partial void ErroDaFerramenta(ILogger logger, string codigo);

    [LoggerMessage(EventId = 33, Level = LogLevel.Error, Message = "Resposta da ferramenta do manual fora do contrato: {Motivo}")]
    private static partial void ContratoInesperado(ILogger logger, string motivo);
}

/// <summary>Registro da consulta ao manual.</summary>
internal static class RegistroDoConhecimento
{
    public static IServiceCollection AdicionarConsultaAoManual(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesDoConhecimento>(configuracao, OpcoesDoConhecimento.Secao);
        servicos.TryAddSingleton(TimeProvider.System);
        servicos.AddHttpClient(ClienteMcpDoManual.NomeDoClienteHttp);
        servicos.AddSingleton<IConsultaAoManual, ClienteMcpDoManual>();
        servicos.AddSingleton(provedor => new EtapaDeConsultaAoManual(
            provedor.GetRequiredService<IConsultaAoManual>(),
            provedor.GetRequiredService<IOptions<OpcoesDoConhecimento>>().Value.MaximoDeConsultasPorExecucao));
        return servicos;
    }
}
