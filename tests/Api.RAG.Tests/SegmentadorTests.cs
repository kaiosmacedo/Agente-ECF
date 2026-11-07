using Api.RAG.Dominio.Conhecimento;

namespace Api.RAG.Tests;

public sealed class SegmentadorTests
{
    private static readonly ParametrosDeSegmentacao Parametros = new(100, 20);

    [Fact(DisplayName = "REQ-INGEST-004 Trecho não atravessa páginas, e a página citada é a certa")]
    public void TrechoPorPagina()
    {
        var trechos = Segmentador.Segmentar(
            [new PaginaDoDocumento(7, "Texto da página sete."), new PaginaDoDocumento(8, "Texto da página oito.")], Parametros);

        Assert.Equal(2, trechos.Count);
        Assert.Equal((0, 7, "Texto da página sete."), (trechos[0].Ordem, trechos[0].Pagina, trechos[0].Texto));
        Assert.Equal((1, 8, "Texto da página oito."), (trechos[1].Ordem, trechos[1].Pagina, trechos[1].Texto));
    }

    [Fact(DisplayName = "REQ-INGEST-004 Título abre um trecho novo, e a seção segue para as páginas seguintes")]
    public void SecaoPeloTitulo()
    {
        var trechos = Segmentador.Segmentar(
            [
                new PaginaDoDocumento(1, "Introdução.\nRegistro K155: Detalhes dos Saldos\nConteúdo do registro."),
                new PaginaDoDocumento(2, "Continuação do registro."),
            ],
            Parametros);

        Assert.Equal((1, (string?)null, "Introdução."), (trechos[0].Pagina, trechos[0].Secao, trechos[0].Texto));
        Assert.Equal("Registro K155: Detalhes dos Saldos", trechos[1].Secao);
        Assert.StartsWith("Registro K155", trechos[1].Texto, StringComparison.Ordinal);
        Assert.Equal((2, "Registro K155: Detalhes dos Saldos"), (trechos[2].Pagina, trechos[2].Secao));
    }

    [Fact(DisplayName = "P11 Trecho respeita o tamanho configurado e repete a sobreposição, começando em palavra inteira")]
    public void TamanhoESobreposicao()
    {
        var linhas = string.Join('\n', Enumerable.Range(1, 30).Select(numero => $"linha numero {numero:00} do texto"));

        var trechos = Segmentador.Segmentar([new PaginaDoDocumento(1, linhas)], Parametros);

        Assert.True(trechos.Count > 1);
        Assert.All(trechos, trecho => Assert.True(trecho.Texto.Length <= Parametros.Tamanho, trecho.Texto));
        var fimDoPrimeiro = trechos[0].Texto[^10..];
        Assert.Contains(fimDoPrimeiro, trechos[1].Texto, StringComparison.Ordinal);
        Assert.Equal(Enumerable.Range(0, trechos.Count), trechos.Select(trecho => trecho.Ordem));
    }

    [Fact(DisplayName = "P11 Linha maior que o trecho é dividida em palavras inteiras")]
    public void LinhaLonga()
    {
        var linha = string.Join(' ', Enumerable.Repeat("palavra", 60));

        var trechos = Segmentador.Segmentar([new PaginaDoDocumento(1, linha)], new ParametrosDeSegmentacao(100, 0));

        Assert.All(trechos, trecho => Assert.True(trecho.Texto.Length <= 100));
        Assert.Equal(60, trechos.Sum(trecho => trecho.Texto.Split([' ', '\n']).Length));
    }

    [Fact(DisplayName = "P11 Página vazia não gera trecho")]
    public void PaginaVazia() =>
        Assert.Empty(Segmentador.Segmentar([new PaginaDoDocumento(1, "  \n \n")], Parametros));
}
