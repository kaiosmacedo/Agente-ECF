using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Api.Agente.Aplicacao.Analise;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Aplicacao.ContratoSofia;

/// <summary>Desfecho do processamento: um resultado do contrato ou o motivo de não haver como correlacionar um.</summary>
internal sealed record DesfechoDaSolicitacao(ResultadoAnaliseSofia? Resultado, string? MotivoDaRejeicao);

/// <summary>
/// Entrada do canal A2A (REQ-A2A-002): lê o contrato SOFIA, valida os identificadores e o contrato, entrega o pedido
/// normalizado ao serviço de análise, registra a execução e projeta o resultado. O conteúdo é dado não confiável (P10).
/// </summary>
internal sealed class ProcessadorDeSolicitacoes(IRepositorioDeExecucoes repositorio, AmbienteDaAnalise ambiente, TimeProvider relogio)
{
    private readonly ServicoDeAnalise servico = new(ambiente, relogio);

    public async Task<DesfechoDaSolicitacao> ProcessarAsync(string json, string? correlacao, CancellationToken cancelamento)
    {
        SolicitacaoAnaliseSofia? solicitacao;
        try
        {
            solicitacao = JsonSerializer.Deserialize<SolicitacaoAnaliseSofia>(json, ContratoSofiaV1.Json);
        }
        catch (JsonException)
        {
            return new DesfechoDaSolicitacao(null, "json_invalido");
        }

        if (solicitacao is null || !ValidadorDaSolicitacao.IdentificadoresValidos(solicitacao))
        {
            return new DesfechoDaSolicitacao(null, "identificadores_invalidos");
        }

        try
        {
            // Retentativa da mesma chamada devolve o que já foi registrado (D7).
            var envelope = await repositorio.BuscarPorSolicitacaoAsync(Guid.Parse(solicitacao.IdSolicitacao!), cancelamento)
                ?? await repositorio.RegistrarAsync(Analisar(solicitacao, correlacao), cancelamento);
            return new DesfechoDaSolicitacao(ProjecaoParaSofia.Projetar(envelope), null);
        }
        catch (FalhaDoRepositorio)
        {
            return new DesfechoDaSolicitacao(FalhaTecnica(solicitacao), null);
        }
    }

    private EnvelopeDoResultado Analisar(SolicitacaoAnaliseSofia solicitacao, string? correlacao)
    {
        var erro = ValidarContrato(solicitacao);
        IReadOnlyList<DocumentoDoPedido> documentos = erro is null
            ? [.. solicitacao.Documentos!.Select(documento => new DocumentoDoPedido(documento.IdDocumento!, documento.Texto!, Hash(documento.Texto!)))]
            : [];
        return servico.Analisar(
            new PedidoDeAnalise(
                Guid.Parse(solicitacao.IdExecucao!), Guid.Parse(solicitacao.IdSolicitacao!), CanalDaAnalise.A2A, solicitacao.Usuario?.Nome, documentos, erro),
            correlacao);
    }

    private static ErroDoEnvelope? ValidarContrato(SolicitacaoAnaliseSofia solicitacao)
    {
        if (solicitacao.VersaoContrato != ContratoSofiaV1.Versao)
        {
            return new ErroDoEnvelope("versao_contrato_nao_suportada", $"Este agente aceita somente a versão {ContratoSofiaV1.Versao} do contrato.");
        }

        var camposInvalidos = ValidadorDaSolicitacao.CamposInvalidos(solicitacao);
        return camposInvalidos.Count > 0
            ? new ErroDoEnvelope("solicitacao_invalida", $"Campos ausentes ou inválidos: {string.Join(", ", camposInvalidos)}.")
            : null;
    }

    private static ResultadoAnaliseSofia FalhaTecnica(SolicitacaoAnaliseSofia solicitacao)
    {
        const string Mensagem = "O registro da execução não respondeu; tente novamente.";
        return new ResultadoAnaliseSofia(
            ContratoSofiaV1.Versao,
            solicitacao.IdExecucao!,
            solicitacao.IdSolicitacao!,
            ContratoSofiaV1.Agente,
            ContratoSofiaV1.StatusErro,
            $"A solicitação não pôde ser analisada. {Mensagem}",
            [],
            [],
            new ErroSofia("falha_tecnica", Mensagem),
            Veredito: MontadorDoEnvelope.NomeDoVeredito(VereditoDaExecucao.NaoAvaliado));
    }

    private static string Hash(string texto) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
}
