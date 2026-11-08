using System.Globalization;
using System.Text.RegularExpressions;
using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Aplicacao.Conhecimento;

/// <summary>O manual da versão identificada, com os identificadores da base de conhecimento (mudança 06, D2).</summary>
internal sealed record ManualDaVersao(string TipoDocumento, string Versao)
{
    /// <summary>Tipo dos manuais da ECF na base (`docs/fontes/manifesto-rag.json`).</summary>
    public const string TipoDosManuaisDaEcf = "manual-ecf";

    public static ManualDaVersao DaEcf(int leiaute) => new(TipoDosManuaisDaEcf, string.Create(CultureInfo.InvariantCulture, $"leiaute-{leiaute}"));
}

/// <summary>
/// Etapa 5 do fluxo (P03): uma consulta por regra violada nos achados determinísticos, montada pelo sistema, sem LLM
/// (REQ-CONSULTA-005). Desligamento e limite da base viram limitação; falha da chamada vira falha técnica (D7).
/// </summary>
internal sealed partial class EtapaDeConsultaAoManual(IConsultaAoManual consulta, int maximoDeConsultas)
{
    public const string LimitacaoDesativada = "consulta ao manual desativada";
    public const string LimitacaoLimiteDiario = "limite diário de consultas ao manual atingido";
    public const string LimitacaoFalha = "manual não consultado";
    public const string LimitacaoVersaoAusente = "manual da versão identificada não está na base";
    private const int ConsultasSimultaneas = 4;
    private const int TamanhoMaximoDaConsulta = 500;

    public async Task ExecutarAsync(Execucao execucao, ManualDaVersao manual, string? correlacao, CancellationToken cancelamento)
    {
        var grupos = execucao.Achados
            .Where(achado => achado.Deterministico)
            .GroupBy(achado => achado.ChaveDaRegra, StringComparer.Ordinal)
            .Select(grupo => (Chave: grupo.Key, Primeiro: grupo.First()))
            .ToList();
        if (grupos.Count == 0)
        {
            execucao.MarcarNaoAplicavel(Etapa.ConsultaAoManual, FluxoDeEtapas.JustificativaSemAchadoAFundamentar);
            return;
        }

        var consultadas = grupos.Take(maximoDeConsultas).ToList();
        if (grupos.Count > maximoDeConsultas)
        {
            execucao.AdicionarLimitacao(string.Create(
                CultureInfo.InvariantCulture,
                $"consultas ao manual limitadas a {maximoDeConsultas} por execução; regras sem trecho do manual: {string.Join(", ", grupos.Skip(maximoDeConsultas).Select(grupo => grupo.Chave))}"));
        }

        using var vagas = new SemaphoreSlim(ConsultasSimultaneas);
        var respostas = await Task.WhenAll(consultadas.Select(async grupo =>
        {
            await vagas.WaitAsync(cancelamento);
            try
            {
                return (grupo.Chave, Resposta: await consulta.BuscarAsync(Pedido(grupo.Primeiro, manual, execucao.Id, correlacao), cancelamento));
            }
            finally
            {
                vagas.Release();
            }
        }));

        foreach (var (chave, resposta) in respostas.Where(item => item.Resposta.Desfecho == DesfechoDaConsulta.Trechos))
        {
            execucao.RegistrarTrechos(chave, [.. resposta.Trechos.Select(t => new TrechoConsultado(t.IdTrecho, t.IdDocumento, t.Versao, t.Pagina, t.Relevancia))]);
        }

        var desfechos = respostas.Select(item => item.Resposta.Desfecho).ToHashSet();
        if (desfechos.Contains(DesfechoDaConsulta.FalhaTecnica))
        {
            execucao.FalharEtapa(Etapa.ConsultaAoManual);
            execucao.AdicionarAchado(Achado.Criar(
                "ach-falha-consulta-manual",
                CategoriaDoAchado.FalhaTecnica,
                OrigemDoAchado.Sistema,
                "REQ-CONSULTA-003",
                "A base de conhecimento não respondeu dentro do tempo ou falhou: o manual não foi consultado.",
                SeveridadeDoAchado.Media));
            execucao.AdicionarLimitacao(LimitacaoFalha);
        }
        else if (desfechos.Contains(DesfechoDaConsulta.BuscaDesativada))
        {
            NaoRealizada(execucao, LimitacaoDesativada, "a busca no manual está desligada na base de conhecimento");
        }
        else if (desfechos.Contains(DesfechoDaConsulta.LimiteDeUsoAtingido))
        {
            NaoRealizada(execucao, LimitacaoLimiteDiario, "o limite diário de consultas ao manual foi atingido");
        }
        else if (desfechos.Contains(DesfechoDaConsulta.VersaoIndisponivel))
        {
            NaoRealizada(execucao, LimitacaoVersaoAusente, "o manual da versão identificada não está na base de conhecimento");
        }
        else
        {
            execucao.ConcluirEtapa(Etapa.ConsultaAoManual);
        }
    }

    /// <summary>A etapa existe e foi tentada: o motivo vai para a etapa, e a limitação, para o resultado (REQ-CONSULTA-007, REQ-RESULT-006).</summary>
    private static void NaoRealizada(Execucao execucao, string limitacao, string motivo)
    {
        execucao.DeixarNaoExecutada(Etapa.ConsultaAoManual, motivo);
        execucao.AdicionarLimitacao(limitacao);
    }

    private static PedidoDeTrechos Pedido(Achado achado, ManualDaVersao manual, Guid idExecucao, string? correlacao)
    {
        var registro = achado.Registros.Count > 0 ? achado.Registros[0] : null;
        var local = registro is null ? string.Empty : $"Registro {registro.Registro}{(registro.Campo is { } campo ? $", campo {campo}" : string.Empty)}: ";
        var texto = $"{local}{achado.Descricao}";
        var codigo = achado.Regra?.Codigo is { } doManual && Codigo().IsMatch(doManual)
            ? doManual
            : registro is not null && Codigo().IsMatch(registro.Registro) ? registro.Registro : null;
        return new PedidoDeTrechos(texto.Length > TamanhoMaximoDaConsulta ? texto[..TamanhoMaximoDaConsulta] : texto, manual.TipoDocumento, manual.Versao, codigo, idExecucao, correlacao);
    }

    // O mesmo formato que a ferramenta aceita: só letras maiúsculas, dígitos e sublinhado.
    [GeneratedRegex(@"\A[A-Z0-9_]{1,60}\z")]
    private static partial Regex Codigo();
}
