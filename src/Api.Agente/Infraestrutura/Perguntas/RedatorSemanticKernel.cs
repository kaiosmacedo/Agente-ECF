using System.Net;
using Api.Agente.Aplicacao.Perguntas;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;

namespace Api.Agente.Infraestrutura.Perguntas;

/// <summary>
/// Redator com o Semantic Kernel e o conector da OpenAI (RA-06; mudança 17, D5): mensagem de sistema com o prompt
/// <c>pergunta-v2</c>, pergunta e trechos delimitados, saída estruturada estrita e temperatura da configuração. Uma
/// chamada é uma tentativa; timeout, falha de conexão, 429 e erro 5xx são transitórios.
/// </summary>
internal sealed class RedatorSemanticKernel(IChatCompletionService chat, IOptions<OpcoesDoLlm> opcoes) : IRedatorDeRespostas
{
    public const string NomeDoClienteHttp = "openai-llm";

    public string Provedor => "openai";

    public string Modelo => opcoes.Value.Modelo;

    public async Task<SaidaDoRedator> RedigirAsync(MaterialDaRedacao material, CancellationToken cancelamento)
    {
        var historico = new ChatHistory(PromptDaPergunta.Instrucoes);
        historico.AddUserMessage(PromptDaPergunta.Mensagem(material));
        var parametros = new OpenAIPromptExecutionSettings
        {
            Temperature = opcoes.Value.Temperatura,
            MaxTokens = opcoes.Value.MaximoDeTokensDeSaida,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "resposta_pergunta", BinaryData.FromString(PromptDaPergunta.SchemaDaSaida), jsonSchemaIsStrict: true),
        };

        using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancelamento);
        limite.CancelAfter(TimeSpan.FromSeconds(opcoes.Value.TimeoutPorChamadaEmSegundos));
        try
        {
            var resposta = await chat.GetChatMessageContentAsync(historico, parametros, cancellationToken: limite.Token);
            var uso = resposta.Metadata is { } metadados && metadados.TryGetValue("Usage", out var valor) ? valor as ChatTokenUsage : null;
            return string.IsNullOrWhiteSpace(resposta.Content)
                ? throw new FalhaDoRedator("O modelo não devolveu texto.", transitoria: false, uso?.InputTokenCount, uso?.OutputTokenCount)
                : new SaidaDoRedator(resposta.Content, uso?.InputTokenCount, uso?.OutputTokenCount);
        }
        catch (OperationCanceledException erro) when (!cancelamento.IsCancellationRequested)
        {
            throw new FalhaDoRedator("O modelo não respondeu no tempo.", transitoria: true, causa: erro);
        }
        catch (HttpOperationException erro)
        {
            // Sem a mensagem do provedor: ela pode ecoar parte do pedido (SQ-04).
            var transitoria = erro.StatusCode is null or HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError;
            throw new FalhaDoRedator($"O modelo respondeu com erro ({(int?)erro.StatusCode}).", transitoria, causa: erro);
        }
        catch (HttpRequestException erro)
        {
            throw new FalhaDoRedator("Falha de conexão com o modelo.", transitoria: true, causa: erro);
        }
    }
}
