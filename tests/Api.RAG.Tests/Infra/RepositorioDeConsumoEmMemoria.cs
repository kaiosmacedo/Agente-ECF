using System.Collections.Concurrent;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Consumo;

namespace Api.RAG.Tests.Infra;

/// <summary>Repositório em memória para os testes que não tratam do banco. O banco real é coberto em <c>ConsumoPersistenciaTests</c>.</summary>
internal sealed class RepositorioDeConsumoEmMemoria : IRepositorioDeConsumo
{
    public ConcurrentQueue<RegistroDeConsumo> Registros { get; } = new();

    /// <summary>Quando verdadeiro, simula o banco fora do ar.</summary>
    public bool Indisponivel { get; set; }

    public Task RegistrarAsync(RegistroDeConsumo registro, CancellationToken cancelamento)
    {
        LancarSeIndisponivel();
        Registros.Enqueue(registro);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TotalDoModelo>> AgregarAsync(FiltroDeConsumo filtro, CancellationToken cancelamento)
    {
        LancarSeIndisponivel();
        IReadOnlyList<TotalDoModelo> totais =
        [
            .. Registros.GroupBy(r => r.Modelo).Select(g => new TotalDoModelo(
                g.Key,
                g.Count(),
                g.Sum(r => (long)(r.TokensEntrada ?? 0)),
                g.Sum(r => (long)(r.TokensSaida ?? 0)),
                g.Sum(r => r.CustoUsd ?? 0m),
                g.Count(r => r.TokensEntradaOrigem == OrigemDoValor.Desconhecido),
                g.Count(r => r.CustoUsd is null))),
        ];
        return Task.FromResult(totais);
    }

    public Task<bool> DisponivelAsync(CancellationToken cancelamento) => Task.FromResult(!Indisponivel);

    public Task<long> ContarConsultasDesdeAsync(DateTimeOffset desde, CancellationToken cancelamento)
    {
        LancarSeIndisponivel();
        return Task.FromResult((long)Registros.Count(r => r.Finalidade == FinalidadeDaChamada.Consulta && r.Tentativa == 1 && r.CriadaEm >= desde));
    }

    private void LancarSeIndisponivel()
    {
        if (Indisponivel)
        {
            throw new FalhaDoRepositorio(new TimeoutException("Banco indisponível no teste."));
        }
    }
}
