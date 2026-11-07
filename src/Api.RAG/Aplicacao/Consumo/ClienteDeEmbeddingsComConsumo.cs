using System.Diagnostics;
using Api.RAG.Dominio.Consumo;

namespace Api.RAG.Aplicacao.Consumo;

/// <summary>Quantas vezes tentar e quanto esperar entre as tentativas.</summary>
internal sealed record PoliticaDeTentativas(int Maximo, TimeSpan Espera);

/// <summary>
/// Repete as falhas do provedor e registra cada tentativa: a primeira como embedding, as seguintes como
/// retentativa (REQ-CONSUMO-001; design da mudança 04, D4). Quem repete é este código, não o SDK.
/// </summary>
internal sealed class ClienteDeEmbeddingsComConsumo(
    IProvedorDeEmbeddings provedor,
    RegistradorDeConsumo registrador,
    CalculadoraDeCusto calculadora,
    PoliticaDeTentativas politica,
    TimeProvider relogio) : IClienteDeEmbeddings
{
    public string Modelo => provedor.Modelo;

    public async Task<RespostaDeEmbeddings> GerarAsync(IReadOnlyList<string> textos, ContextoDaChamada contexto, CancellationToken cancelamento)
    {
        for (var tentativa = 1; ; tentativa++)
        {
            var inicio = relogio.GetTimestamp();
            var criadaEm = relogio.GetUtcNow();
            try
            {
                var resposta = await provedor.GerarAsync(textos, cancelamento);
                await Registrar(contexto, tentativa, criadaEm, inicio, StatusDaChamada.Sucesso, resposta.TokensEntrada);
                return resposta;
            }
            catch (FalhaDoProvedor) when (!cancelamento.IsCancellationRequested)
            {
                await Registrar(contexto, tentativa, criadaEm, inicio, StatusDaChamada.Falha, tokensEntrada: null);
                if (tentativa >= politica.Maximo)
                {
                    throw;
                }

                await Task.Delay(politica.Espera, relogio, cancelamento);
            }
        }
    }

    private Task Registrar(ContextoDaChamada contexto, int tentativa, DateTimeOffset criadaEm, long inicio, StatusDaChamada status, int? tokensEntrada)
    {
        var custo = calculadora.Calcular(provedor.Modelo, tokensEntrada, tokensSaida: null, criadaEm);
        var registro = new RegistroDeConsumo(
            Guid.NewGuid(),
            contexto.IdOperacao,
            contexto.Correlacao,
            provedor.Provedor,
            provedor.Modelo,
            tentativa == 1 ? TipoDeChamada.Embedding : TipoDeChamada.Retentativa,
            status,
            criadaEm,
            relogio.GetElapsedTime(inicio).TotalMilliseconds,
            tentativa,
            tokensEntrada,
            tokensEntrada is null ? OrigemDoValor.Desconhecido : OrigemDoValor.Informado,
            TokensSaida: null,
            OrigemDoValor.NaoAplicavel,
            custo.CustoUsd,
            custo.Origem,
            custo.VersaoDaTabela);

        // O registro não pode ser cancelado junto com a chamada: a tentativa já aconteceu (P17).
        return registrador.RegistrarAsync(registro, CancellationToken.None);
    }
}
