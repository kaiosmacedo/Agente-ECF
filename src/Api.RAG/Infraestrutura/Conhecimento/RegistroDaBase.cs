using System.ClientModel;
using System.ClientModel.Primitives;
using System.ComponentModel.DataAnnotations;
using Api.RAG.Aplicacao.Conhecimento;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Conhecimento;
using Api.RAG.Infraestrutura.Configuracao;
using Api.RAG.Infraestrutura.Persistencia;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Api.RAG.Infraestrutura.Conhecimento;

/// <summary>Opções da seção <c>Ingestao</c> (design da mudança 05, D3 e D5).</summary>
internal sealed class OpcoesDaIngestao : IValidatableObject
{
    public const string Secao = "Ingestao";

    /// <summary>Endpoint público que aciona provedor pago fica desligado, salvo se ligado de propósito (SQ-12).</summary>
    public bool EndpointHabilitado { get; init; }

    [Range(1, 100_000_000)]
    public long TamanhoMaximoEmBytes { get; init; } = 10_485_760;

    [Range(1, 2048)]
    public int TamanhoDoLote { get; init; } = 100;

    [Range(100, 8000)]
    public int TamanhoDoTrecho { get; init; } = 1200;

    [Range(0, 4000)]
    public int SobreposicaoDoTrecho { get; init; } = 200;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SobreposicaoDoTrecho >= TamanhoDoTrecho)
        {
            yield return new ValidationResult("A sobreposição precisa ser menor que o trecho.", [nameof(SobreposicaoDoTrecho)]);
        }
    }
}

/// <summary>Opções da seção <c>OpenAI</c>. A chave é segredo e nunca fica no repositório (SQ-03).</summary>
internal sealed class OpcoesDaOpenAi
{
    public const string Secao = "OpenAI";

    public string? ApiKey { get; init; }

    [Required]
    public string ModeloDeEmbeddings { get; init; } = "text-embedding-3-small";

    [Range(1, 4096)]
    public int DimensaoDosEmbeddings { get; init; } = 1536;
}

/// <summary>Lê o PDF página por página e tira o cabeçalho e o rodapé que se repetem em todas elas (D3).</summary>
internal sealed class ExtratorDePdf : IExtratorDeTexto
{
    private static readonly string[] LinhasRepetidas = ["Anexo ao Ato Declarat", "Atualização:", "RFB/Subsecretaria de Fiscalização"];

    public IReadOnlyList<PaginaDoDocumento> Extrair(byte[] conteudo)
    {
        try
        {
            using var pdf = PdfDocument.Open(conteudo);
            return
            [
                .. pdf.GetPages().Select(pagina => new PaginaDoDocumento(pagina.Number, Limpar(ContentOrderTextExtractor.GetText(pagina)))),
            ];
        }
        catch (Exception erro) when (erro is not OutOfMemoryException)
        {
            throw new ArquivoIlegivel(erro);
        }
    }

    private static string Limpar(string texto) =>
        string.Join('\n', texto.Split('\n').Where(linha => !LinhasRepetidas.Any(repetida => linha.TrimStart().StartsWith(repetida, StringComparison.Ordinal))));
}

/// <summary>Uma tentativa na OpenAI, sem as retentativas do SDK, que passam pelo nosso registro de consumo (mudança 04, D4).</summary>
internal sealed class ProvedorOpenAiDeEmbeddings(IOptions<OpcoesDaOpenAi> opcoes) : IProvedorDeEmbeddings
{
    private readonly Lazy<EmbeddingClient?> cliente = new(() => Criar(opcoes.Value));

    public string Provedor => "openai";

    public string Modelo => opcoes.Value.ModeloDeEmbeddings;

    public async Task<RespostaDeEmbeddings> GerarAsync(IReadOnlyList<string> textos, CancellationToken cancelamento)
    {
        var client = cliente.Value ?? throw new FalhaDoProvedor("A chave da OpenAI não está configurada.");
        try
        {
            var resposta = await client.GenerateEmbeddingsAsync(
                textos, new EmbeddingGenerationOptions { Dimensions = opcoes.Value.DimensaoDosEmbeddings }, cancelamento);
            var valor = resposta.Value;
            return new RespostaDeEmbeddings([.. valor.Select(embedding => embedding.ToFloats().ToArray())], valor.Usage?.InputTokenCount);
        }
        catch (ClientResultException erro)
        {
            // Sem o corpo da resposta nem o texto enviado: só o código (SQ-03, SQ-04).
            throw new FalhaDoProvedor($"A OpenAI respondeu {erro.Status}.", new InvalidOperationException($"HTTP {erro.Status}"));
        }
        catch (Exception erro) when (erro is HttpRequestException or TaskCanceledException && !cancelamento.IsCancellationRequested)
        {
            throw new FalhaDoProvedor("A OpenAI não respondeu.", erro);
        }
    }

    private static EmbeddingClient? Criar(OpcoesDaOpenAi opcoes) =>
        string.IsNullOrWhiteSpace(opcoes.ApiKey)
            ? null
            : new EmbeddingClient(opcoes.ModeloDeEmbeddings, new ApiKeyCredential(opcoes.ApiKey), new OpenAIClientOptions { RetryPolicy = new ClientRetryPolicy(0) });
}

/// <summary>Registro da ingestão e da base de conhecimento.</summary>
internal static class RegistroDaBase
{
    public static IServiceCollection AdicionarBaseDeConhecimento(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesDaIngestao>(configuracao, OpcoesDaIngestao.Secao);
        servicos.AdicionarOpcoesValidadas<OpcoesDaOpenAi>(configuracao, OpcoesDaOpenAi.Secao);
        servicos.AddSingleton<IExtratorDeTexto, ExtratorDePdf>();
        servicos.AddSingleton<IProvedorDeEmbeddings, ProvedorOpenAiDeEmbeddings>();
        servicos.AddSingleton<IClienteDeEmbeddings, ClienteDeEmbeddingsComConsumo>();
        servicos.AddSingleton<IRepositorioDeDocumentos, RepositorioDeDocumentos>();
        servicos.AddSingleton(provedor =>
        {
            var ingestao = provedor.GetRequiredService<IOptions<OpcoesDaIngestao>>().Value;
            var openAi = provedor.GetRequiredService<IOptions<OpcoesDaOpenAi>>().Value;
            return new ParametrosDaIngestao(
                ingestao.TamanhoMaximoEmBytes,
                ingestao.TamanhoDoLote,
                openAi.DimensaoDosEmbeddings,
                new ParametrosDeSegmentacao(ingestao.TamanhoDoTrecho, ingestao.SobreposicaoDoTrecho));
        });
        servicos.AddSingleton<ServicoDeIngestao>();
        servicos.AddSingleton<ComandosDaBase>();
        return servicos;
    }
}
