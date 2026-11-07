using System.Diagnostics.Metrics;
using Api.RAG.Dominio.Consumo;
using Microsoft.Extensions.Logging;

namespace Api.RAG.Aplicacao.Consumo;

/// <summary>Grava o consumo sem interromper a chamada, e a falha ao gravar nunca é silenciosa (REQ-CONSUMO-004; D4).</summary>
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

    [LoggerMessage(Level = LogLevel.Error, Message = "Consumo não registrado. Correlação: {Correlacao}. Operação: {IdOperacao}. Modelo: {Modelo}.")]
    private static partial void LogFalha(ILogger logger, string? correlacao, Guid? idOperacao, string modelo, Exception erro);
}
