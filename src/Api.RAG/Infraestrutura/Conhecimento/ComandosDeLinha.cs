using System.Globalization;
using System.Text.Json;

namespace Api.RAG.Infraestrutura.Conhecimento;

/// <summary>Lê os argumentos de <c>--ingerir</c> e <c>--avaliar</c> e devolve o código de saída.</summary>
internal static class ComandosDeLinha
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> ExecutarAsync(string[] args, IServiceProvider servicos, TextWriter saida)
    {
        var comandos = servicos.GetRequiredService<ComandosDaBase>();
        var ingerir = Valor(args, "--ingerir");
        if (ingerir is not null)
        {
            return await comandos.IngerirAsync(ingerir, Valor(args, "--versao"), args.Contains("--reprocessar"), saida, CancellationToken.None);
        }

        var avaliar = Valor(args, "--avaliar");
        if (avaliar is null)
        {
            await saida.WriteLineAsync("Informe o caminho do arquivo.");
            return 1;
        }

        var resultado = await comandos.AvaliarAsync(avaliar, CancellationToken.None, args.Contains("--com-codigo"));
        await saida.WriteLineAsync(string.Create(
            CultureInfo.InvariantCulture,
            $"Perguntas: {resultado.Perguntas}. Acertos nos 5 primeiros: {resultado.Acertos} ({resultado.AcertoNosCinco:P1}). Posição recíproca média: {resultado.PosicaoReciprocaMedia:0.000}. Busca por código: {(resultado.ComCodigo ? "sim" : "não")}."));
        if (resultado.AcertoPorQuantidade is { } porQuantidade)
        {
            await saida.WriteLineAsync(string.Create(
                CultureInfo.InvariantCulture,
                $"Acerto por quantidade: {string.Join("; ", porQuantidade.Select(par => $"{par.Key} trechos {par.Value:P1}"))}. Menor relevância dos acertos: {resultado.MenorRelevanciaDosAcertos:0.0000}. Maior relevância sem resposta ({resultado.PerguntasSemResposta} perguntas): {resultado.MaiorRelevanciaSemResposta:0.0000}."));
        }

        await saida.WriteLineAsync(JsonSerializer.Serialize(resultado, Json));
        return 0;
    }

    private static string? Valor(string[] args, string nome)
    {
        var indice = Array.IndexOf(args, nome);
        return indice >= 0 && indice + 1 < args.Length ? args[indice + 1] : null;
    }
}
