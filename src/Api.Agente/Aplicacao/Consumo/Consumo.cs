using System.Diagnostics.Metrics;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Dominio.Consumo;
using Microsoft.Extensions.Logging;

namespace Api.Agente.Aplicacao.Consumo;

/// <summary>Onde o consumo fica guardado, no banco da própria Api.Agente (P15; mudança 17, D10). Falhas chegam como <see cref="FalhaDoRepositorio"/>.</summary>
internal interface IRepositorioDeConsumo
{
    Task RegistrarAsync(RegistroDeConsumo registro, CancellationToken cancelamento);
}

internal sealed record PrecoDoModelo(string Modelo, decimal EntradaPorMilhao, decimal? SaidaPorMilhao);

internal sealed record TabelaDePrecos(string Versao, DateOnly VigenteDesde, string Fonte, IReadOnlyList<PrecoDoModelo> Modelos);

internal sealed record CustoCalculado(decimal? CustoUsd, OrigemDoValor Origem, string? VersaoDaTabela);

/// <summary>
/// Custo estimado pela tabela vigente na data da chamada, copiado do serviço de conhecimento (RA-10; DP-13). Sem tabela, preço ou tokens,
/// o custo é desconhecido, nunca zero.
/// </summary>
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

/// <summary>Grava o consumo sem derrubar a resposta: a falha do banco vira log de erro e métrica (mudança 17, D10).</summary>
internal sealed partial class RegistradorDeConsumo : IDisposable
{
    public const string NomeDoMedidor = "Agente.ECF.Consumo";
    public const string NomeDaMetricaDeFalhas = "agente_ecf.consumo.falhas_registro";

    private readonly IRepositorioDeConsumo repositorio;
    private readonly ILogger<RegistradorDeConsumo> logger;
    private readonly Meter medidor = new(NomeDoMedidor);
    private readonly Counter<long> falhas;

    public RegistradorDeConsumo(IRepositorioDeConsumo repositorio, ILogger<RegistradorDeConsumo> logger)
    {
        this.repositorio = repositorio;
        this.logger = logger;
        falhas = medidor.CreateCounter<long>(NomeDaMetricaDeFalhas, description: "Registros de consumo que não puderam ser gravados.");
    }

    public async Task RegistrarAsync(RegistroDeConsumo registro, CancellationToken cancelamento)
    {
        try
        {
            await repositorio.RegistrarAsync(registro, cancelamento);
        }
        catch (FalhaDoRepositorio erro)
        {
            falhas.Add(1);
            LogFalha(logger, registro.Correlacao, registro.IdOperacao, registro.Modelo, erro);
        }
    }

    public void Dispose() => medidor.Dispose();

    [LoggerMessage(EventId = 50, Level = LogLevel.Error, Message = "Consumo não registrado. Correlação: {Correlacao}. Operação: {IdOperacao}. Modelo: {Modelo}.")]
    private static partial void LogFalha(ILogger logger, string? correlacao, Guid? idOperacao, string modelo, Exception erro);
}
