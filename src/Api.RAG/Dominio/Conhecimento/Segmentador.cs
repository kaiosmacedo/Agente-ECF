using System.Text;
using System.Text.RegularExpressions;

namespace Api.RAG.Dominio.Conhecimento;

/// <summary>Uma página do documento, já sem cabeçalho e rodapé repetidos.</summary>
internal sealed record PaginaDoDocumento(int Numero, string Texto);

/// <summary>Um trecho do documento, com a localização para citar (P06, REQ-INGEST-004).</summary>
internal sealed record Trecho(int Ordem, int Pagina, string? Secao, string Texto);

/// <summary>Tamanho e sobreposição dos trechos, em caracteres. Os valores finais saem da avaliação (P11, GD-08).</summary>
internal sealed record ParametrosDeSegmentacao(int Tamanho, int Sobreposicao);

/// <summary>
/// Divide o documento em trechos sem atravessar páginas, para a página citada estar sempre certa, e começa um
/// trecho novo em cada título, para a seção também estar certa (design da mudança 05, D3).
/// </summary>
internal static partial class Segmentador
{
    public static IReadOnlyList<Trecho> Segmentar(IEnumerable<PaginaDoDocumento> paginas, ParametrosDeSegmentacao parametros)
    {
        var trechos = new List<Trecho>();
        string? secao = null;
        foreach (var pagina in paginas)
        {
            var atual = new StringBuilder();
            var secaoDoTrecho = secao;
            foreach (var linhaBruta in pagina.Texto.Split('\n'))
            {
                var linha = linhaBruta.Trim();
                if (linha.Length == 0)
                {
                    continue;
                }

                if (Titulo().IsMatch(linha) && linha.Length <= 160)
                {
                    Fechar(trechos, pagina.Numero, secaoDoTrecho, atual, sobreposicao: 0);
                    secao = linha;
                    secaoDoTrecho = linha;
                }

                foreach (var parte in Partes(linha, parametros.Tamanho))
                {
                    if (atual.Length > 0 && atual.Length + parte.Length + 1 > parametros.Tamanho)
                    {
                        Fechar(trechos, pagina.Numero, secaoDoTrecho, atual, parametros.Sobreposicao);
                    }

                    if (atual.Length > 0)
                    {
                        atual.Append('\n');
                    }

                    atual.Append(parte);
                }
            }

            Fechar(trechos, pagina.Numero, secaoDoTrecho, atual, sobreposicao: 0);
        }

        return trechos;
    }

    /// <summary>Guarda o trecho e deixa no início do próximo o fim deste, começando em uma palavra inteira.</summary>
    private static void Fechar(List<Trecho> trechos, int pagina, string? secao, StringBuilder atual, int sobreposicao)
    {
        if (atual.Length == 0)
        {
            return;
        }

        var texto = atual.ToString();
        trechos.Add(new Trecho(trechos.Count, pagina, secao, texto));
        atual.Clear();
        if (sobreposicao > 0 && texto.Length > sobreposicao)
        {
            var fim = texto[^sobreposicao..];
            var espaco = fim.IndexOfAny([' ', '\n']);
            atual.Append(espaco >= 0 ? fim[(espaco + 1)..] : fim);
        }
    }

    /// <summary>Linha maior que o trecho é dividida em palavras inteiras, quando possível.</summary>
    private static IEnumerable<string> Partes(string linha, int tamanho)
    {
        var restante = linha;
        while (restante.Length > tamanho)
        {
            var corte = restante.LastIndexOf(' ', tamanho - 1);
            corte = corte > 0 ? corte : tamanho;
            yield return restante[..corte].TrimEnd();
            restante = restante[corte..].TrimStart();
        }

        if (restante.Length > 0)
        {
            yield return restante;
        }
    }

    [GeneratedRegex(@"^(Capítulo \d+|Seção \d+(\.\d+)*\.?|Registro [0-9A-Z]\d{3}[A-Z]?:|Bloco [0-9A-Z]:|\d+\.\d+\.\s)")]
    private static partial Regex Titulo();
}
