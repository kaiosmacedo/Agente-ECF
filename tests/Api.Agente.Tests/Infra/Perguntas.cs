using System.Collections.Concurrent;
using Api.Agente.Aplicacao.Conhecimento;
using Api.Agente.Aplicacao.Consumo;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Aplicacao.Perguntas;
using Api.Agente.Dominio.Consumo;

namespace Api.Agente.Tests.Infra;

/// <summary>Registro de perguntas em memória, com contagem ajustável para o teste do limite.</summary>
internal sealed class PerguntasEmMemoria : IRepositorioDePerguntas
{
    public ConcurrentQueue<RegistroDaPergunta> Registros { get; } = new();

    public long RespondidasAntes { get; set; }

    public bool Indisponivel { get; set; }

    public Task RegistrarAsync(RegistroDaPergunta registro, CancellationToken cancelamento)
    {
        Falhar();
        if (registro.IdSolicitacao is null || Registros.All(existente => existente.IdSolicitacao != registro.IdSolicitacao))
        {
            Registros.Enqueue(registro);
        }

        return Task.CompletedTask;
    }

    public Task<long> ContarRespondidasDesdeAsync(DateTimeOffset desde, CancellationToken cancelamento)
    {
        Falhar();
        return Task.FromResult(RespondidasAntes + Registros.Count(registro => registro.Status is "concluido" or "parcial" && registro.CriadaEm >= desde));
    }

    private void Falhar()
    {
        if (Indisponivel)
        {
            throw new FalhaDoRepositorio(new TimeoutException());
        }
    }
}

internal sealed class ConsumoEmMemoria : IRepositorioDeConsumo
{
    public ConcurrentQueue<RegistroDeConsumo> Registros { get; } = new();

    public Task RegistrarAsync(RegistroDeConsumo registro, CancellationToken cancelamento)
    {
        Registros.Enqueue(registro);
        return Task.CompletedTask;
    }
}

/// <summary>Busca falsa: devolve a resposta configurada e guarda os pedidos.</summary>
internal sealed class BuscaFalsa(RespostaDaConsulta resposta) : IConsultaAoManual
{
    public ConcurrentQueue<PedidoDeTrechos> Pedidos { get; } = new();

    public Task<RespostaDaConsulta> BuscarAsync(PedidoDeTrechos pedido, CancellationToken cancelamento)
    {
        Pedidos.Enqueue(pedido);
        return Task.FromResult(resposta);
    }
}

/// <summary>Redator falso: cada chamada executa o próximo comportamento da fila; sem fila, repete o último.</summary>
internal sealed class RedatorFalso(params Func<MaterialDaRedacao, SaidaDoRedator>[] comportamentos) : IRedatorDeRespostas
{
    private int chamadas;

    public int Chamadas => chamadas;

    public string Provedor => "openai";

    public string Modelo => "gpt-4.1-mini";

    public ConcurrentQueue<MaterialDaRedacao> Materiais { get; } = new();

    public Task<SaidaDoRedator> RedigirAsync(MaterialDaRedacao material, CancellationToken cancelamento)
    {
        Materiais.Enqueue(material);
        var indice = Math.Min(Interlocked.Increment(ref chamadas) - 1, comportamentos.Length - 1);
        return Task.FromResult(comportamentos[indice](material));
    }

    /// <summary>Resposta válida que cita o primeiro trecho recebido.</summary>
    public static SaidaDoRedator CitaOPrimeiro(MaterialDaRedacao material) =>
        new($$"""{"respondida":true,"resposta":"O Registro 0000 abre o arquivo digital e identifica a pessoa jurídica.","citacoes":["trecho:{{material.Trechos[0].IdTrecho}}"]}""", 900, 60);

    public static SaidaDoRedator Texto(string json) => new(json, 900, 60);

    public static SaidaDoRedator Indisponivel(MaterialDaRedacao material) =>
        throw new FalhaDoRedator("O modelo respondeu com erro (503).", transitoria: true);
}

/// <summary>Trechos sintéticos do manual do leiaute 12, com o texto do nome do Registro 0000 (catálogo, p. 63).</summary>
internal static class TrechosSinteticos
{
    public const string Titulo = "Manual de Orientação do Leiaute 12 da ECF";

    public static TrechoDoManual Trecho(int pagina, double relevancia, string texto = "Registro 0000: Abertura do Arquivo Digital e Identificação da Pessoa Jurídica.") =>
        new(Guid.NewGuid(), Guid.Parse("4d1c2b5e-0000-4000-8000-000000000012"), Titulo, "leiaute-12", pagina, null, relevancia, false, texto);

    public static RespostaDaConsulta Cinco() => new(
        DesfechoDaConsulta.Trechos,
        [Trecho(63, 0.71), Trecho(64, 0.66), Trecho(70, 0.52), Trecho(17, 0.41), Trecho(38, 0.40)]);
}

/// <summary>Monta o serviço de perguntas ligado, com dublês, para os testes de unidade e HTTP.</summary>
internal sealed class PerguntasDeTeste(RespostaDaConsulta? busca = null, IRedatorDeRespostas? redator = null, bool habilitada = true, double? relevanciaMinima = null)
{
    public BuscaFalsa Busca { get; } = new(busca ?? TrechosSinteticos.Cinco());

    public PerguntasEmMemoria Repositorio { get; } = new();

    public ConsumoEmMemoria Consumo { get; } = new();

    public IRedatorDeRespostas? Redator { get; } = redator;

    public ServicoDePerguntas Servico() => new(
        new ConfiguracaoDasPerguntas(habilitada, Redator is not null, 500, relevanciaMinima, 2, "1.0.0-teste"),
        Busca,
        Repositorio,
        TimeProvider.System,
        Redator,
        new RegistradorDeConsumo(Consumo, Microsoft.Extensions.Logging.Abstractions.NullLogger<RegistradorDeConsumo>.Instance),
        new CalculadoraDeCusto([new TabelaDePrecos("teste", new DateOnly(2026, 1, 1), "https://developers.openai.com/api/docs/pricing", [new PrecoDoModelo("gpt-4.1-mini", 0.40m, 1.60m)])]));
}
