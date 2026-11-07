using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Api.RAG.Aplicacao.Conhecimento;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Consumo;

namespace Api.RAG.Infraestrutura.Conhecimento;

/// <summary>Uma entrada do manifesto: o arquivo, o hash e os metadados de um documento oficial (design da mudança 05, D5).</summary>
internal sealed record DocumentoDoManifesto(
    string Arquivo,
    string Sha256,
    string Emissor,
    string Titulo,
    string Tipo,
    string Versao,
    int? AnoCalendario,
    string? VersaoDaPublicacao,
    string? Vigencia,
    string? Origem);

internal sealed record Manifesto(IReadOnlyList<DocumentoDoManifesto> Documentos);

/// <summary>Uma pergunta da avaliação, com as páginas onde está a resposta (D6; GD-08).</summary>
/// <summary>Uma pergunta da avaliação, com as páginas da resposta; sem páginas, o manual não a responde (mudança 06, D4).</summary>
internal sealed record PerguntaDeAvaliacao(string Id, string Pergunta, string Tipo, string Versao, IReadOnlyList<int> Paginas, string? Codigo = null);

internal sealed record ConjuntoDeAvaliacao(IReadOnlyList<PerguntaDeAvaliacao> Perguntas);

/// <summary>Resultado da avaliação de recuperação.</summary>
internal sealed record ResultadoDaAvaliacao(
    int Perguntas,
    int Acertos,
    double AcertoNosCinco,
    double PosicaoReciprocaMedia,
    IReadOnlyList<string> Erradas,
    IReadOnlyDictionary<string, IReadOnlyList<int>> PaginasDasErradas,
    bool ComCodigo = false,
    IReadOnlyDictionary<int, double>? AcertoPorQuantidade = null,
    double? MenorRelevanciaDosAcertos = null,
    double? MaiorRelevanciaSemResposta = null,
    int PerguntasSemResposta = 0);

/// <summary>
/// Operações eventuais, fora do Vercel, como a migração: o PDF passa do limite do corpo, e a função não dura o
/// suficiente para milhares de trechos (D5). Mostram só o resumo, nunca a chave ou a conexão.
/// </summary>
internal sealed class ComandosDaBase(
    ServicoDeIngestao ingestao,
    IClienteDeEmbeddings embeddings,
    IRepositorioDeDocumentos repositorio,
    IRepositorioDeConsumo consumo)
{
    private const int QuantidadeAvaliada = 5;

    private static readonly int[] QuantidadesComparadas = [3, 5, 8];

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Ingere os documentos do manifesto, recusando arquivo com hash diferente do registrado.</summary>
    public async Task<int> IngerirAsync(string caminhoDoManifesto, string? somenteVersao, bool reprocessar, TextWriter saida, CancellationToken cancelamento)
    {
        var manifesto = JsonSerializer.Deserialize<Manifesto>(await File.ReadAllTextAsync(caminhoDoManifesto, cancelamento), Json)
            ?? throw new InvalidOperationException("Manifesto vazio.");
        var pasta = Path.GetDirectoryName(Path.GetFullPath(caminhoDoManifesto))!;
        var documentos = manifesto.Documentos.Where(d => somenteVersao is null || string.Equals(d.Versao, somenteVersao, StringComparison.Ordinal)).ToArray();
        if (documentos.Length == 0)
        {
            await saida.WriteLineAsync($"Nenhum documento no manifesto para a versão {somenteVersao}.");
            return 1;
        }

        // Confere todos antes de gastar qualquer chamada.
        foreach (var documento in documentos)
        {
            var calculado = Convert.ToHexStringLower(SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(pasta, documento.Arquivo), cancelamento)));
            if (!string.Equals(calculado, documento.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                await saida.WriteLineAsync($"ERRO: o hash de {documento.Arquivo} é diferente do registrado. Nada foi ingerido.");
                return 1;
            }
        }

        foreach (var documento in documentos)
        {
            var conteudo = await File.ReadAllBytesAsync(Path.Combine(pasta, documento.Arquivo), cancelamento);
            if (reprocessar && await repositorio.BuscarPorHashAsync(documento.Sha256.ToLowerInvariant(), cancelamento) is { } anterior)
            {
                await repositorio.ApagarParaReprocessarAsync(anterior.Id, cancelamento);
            }

            var metadados = new MetadadosDoDocumento(
                documento.Emissor, documento.Titulo, documento.Tipo, documento.Versao, documento.AnoCalendario, documento.VersaoDaPublicacao, documento.Vigencia, documento.Origem);
            try
            {
                var resultado = await ingestao.IngerirAsync(metadados, documento.Arquivo, conteudo, correlacao: null, cancelamento);
                var situacao = resultado.JaExistia ? "já existia" : "ingerido";
                await saida.WriteLineAsync(string.Create(
                    CultureInfo.InvariantCulture,
                    $"{documento.Versao}: {situacao}, {resultado.Documento.Paginas} páginas, {resultado.Documento.Chunks} trechos, estado {resultado.Documento.Estado}."));
            }
            catch (FalhaDaIngestao erro)
            {
                await saida.WriteLineAsync($"ERRO em {documento.Versao}: {erro.Message} {erro.InnerException?.Message}");
                return 1;
            }
        }

        await MostrarConsumoAsync(saida, cancelamento);
        return 0;
    }

    /// <summary>
    /// Mede se a página certa aparece entre os primeiros trechos (mudança 05, D6) e, para a mudança 06 (D4), o acerto com
    /// 3, 5 e 8 trechos, com ou sem a busca por código, e as relevâncias que separam as perguntas sem resposta no manual.
    /// </summary>
    public async Task<ResultadoDaAvaliacao> AvaliarAsync(string caminhoDasPerguntas, CancellationToken cancelamento, bool comCodigo = false)
    {
        var conjunto = JsonSerializer.Deserialize<ConjuntoDeAvaliacao>(await File.ReadAllTextAsync(caminhoDasPerguntas, cancelamento), Json)
            ?? throw new InvalidOperationException("Conjunto de avaliação vazio.");
        var operacao = new ContextoDaChamada(Guid.NewGuid(), Correlacao: "avaliacao", FinalidadeDaChamada.Avaliacao);
        var acertos = 0;
        var somaRecíproca = 0d;
        var erradas = new List<string>();
        var paginasDasErradas = new Dictionary<string, IReadOnlyList<int>>(StringComparer.Ordinal);
        var acertosPorQuantidade = QuantidadesComparadas.ToDictionary(quantidade => quantidade, _ => 0);
        var relevanciasDosAcertos = new List<double>();
        var relevanciasSemResposta = new List<double>();
        var respondidas = conjunto.Perguntas.Where(pergunta => pergunta.Paginas.Count > 0).ToList();
        foreach (var pergunta in conjunto.Perguntas)
        {
            var resposta = await embeddings.GerarAsync([pergunta.Pergunta], operacao, cancelamento);
            var trechos = await repositorio.BuscarSemelhantesAsync(
                resposta.Vetores[0], pergunta.Tipo, pergunta.Versao, QuantidadesComparadas.Max(), cancelamento, comCodigo ? pergunta.Codigo : null);
            if (pergunta.Paginas.Count == 0)
            {
                relevanciasSemResposta.Add(trechos.Count > 0 ? trechos[0].Similaridade : 0);
                continue;
            }

            var posicao = trechos.Select((trecho, indice) => (trecho, indice)).FirstOrDefault(x => pergunta.Paginas.Contains(x.trecho.Pagina), (null!, -1)).Item2;
            foreach (var quantidade in QuantidadesComparadas.Where(quantidade => posicao >= 0 && posicao < quantidade))
            {
                acertosPorQuantidade[quantidade]++;
            }

            if (posicao >= 0 && posicao < QuantidadeAvaliada)
            {
                acertos++;
                somaRecíproca += 1d / (posicao + 1);
                relevanciasDosAcertos.Add(trechos[posicao].Similaridade);
            }
            else
            {
                erradas.Add(pergunta.Id);
                paginasDasErradas[pergunta.Id] = [.. trechos.Take(QuantidadeAvaliada).Select(trecho => trecho.Pagina)];
            }
        }

        var total = respondidas.Count;
        return new ResultadoDaAvaliacao(
            total,
            acertos,
            total == 0 ? 0 : (double)acertos / total,
            total == 0 ? 0 : somaRecíproca / total,
            erradas,
            paginasDasErradas,
            comCodigo,
            acertosPorQuantidade.ToDictionary(par => par.Key, par => total == 0 ? 0 : (double)par.Value / total),
            relevanciasDosAcertos.Count > 0 ? relevanciasDosAcertos.Min() : null,
            relevanciasSemResposta.Count > 0 ? relevanciasSemResposta.Max() : null,
            relevanciasSemResposta.Count);
    }

    private async Task MostrarConsumoAsync(TextWriter saida, CancellationToken cancelamento)
    {
        foreach (var total in await consumo.AgregarAsync(new FiltroDeConsumo(null, null, null, null), cancelamento))
        {
            await saida.WriteLineAsync(string.Create(
                CultureInfo.InvariantCulture,
                $"Consumo acumulado de {total.Modelo}: {total.Chamadas} chamadas, {total.TokensEntrada} tokens, US$ {total.CustoUsd:0.######}."));
        }
    }
}
