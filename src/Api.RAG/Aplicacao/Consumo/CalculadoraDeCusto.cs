using Api.RAG.Dominio.Consumo;

namespace Api.RAG.Aplicacao.Consumo;

/// <summary>Preço de um modelo em USD por milhão de tokens. Sem preço de saída, a saída não é cobrada ou não se aplica.</summary>
internal sealed record PrecoDoModelo(string Modelo, decimal EntradaPorMilhao, decimal? SaidaPorMilhao);

/// <summary>Uma versão da tabela de preços, com vigência e fonte (REQ-CONSUMO-003; D3).</summary>
internal sealed record TabelaDePrecos(string Versao, DateOnly VigenteDesde, string Fonte, IReadOnlyList<PrecoDoModelo> Modelos);

/// <summary>Custo estimado, ou desconhecido se faltar preço ou token (REQ-CONSUMO-002, 003).</summary>
internal sealed record CustoCalculado(decimal? CustoUsd, OrigemDoValor Origem, string? VersaoDaTabela);

/// <summary>Estima o custo pela tabela vigente na data da chamada, e cita a versão usada (D3).</summary>
internal sealed class CalculadoraDeCusto(IReadOnlyList<TabelaDePrecos> tabelas)
{
    private const decimal UmMilhao = 1_000_000m;

    private static readonly CustoCalculado Desconhecido = new(null, OrigemDoValor.Desconhecido, null);

    public CustoCalculado Calcular(string modelo, int? tokensEntrada, int? tokensSaida, DateTimeOffset quando)
    {
        var data = DateOnly.FromDateTime(quando.UtcDateTime);
        var tabela = tabelas.Where(t => t.VigenteDesde <= data).OrderByDescending(t => t.VigenteDesde).FirstOrDefault();
        var preco = tabela?.Modelos.FirstOrDefault(m => string.Equals(m.Modelo, modelo, StringComparison.Ordinal));
        if (tabela is null || preco is null || tokensEntrada is null || (preco.SaidaPorMilhao is not null && tokensSaida is null))
        {
            return Desconhecido;
        }

        var custo = (tokensEntrada.Value * preco.EntradaPorMilhao / UmMilhao) + ((tokensSaida ?? 0) * (preco.SaidaPorMilhao ?? 0m) / UmMilhao);
        return new CustoCalculado(custo, OrigemDoValor.Estimado, tabela.Versao);
    }
}
