using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Aplicacao.ContratoSofia;

/// <summary>Desfecho do processamento: um resultado do contrato ou o motivo de não haver como correlacionar um.</summary>
internal sealed record DesfechoDaSolicitacao(ResultadoAnaliseSofia? Resultado, string? MotivoDaRejeicao);

/// <summary>O que o processador precisa saber do ambiente, sem depender da infraestrutura.</summary>
internal sealed record AmbienteDaAnalise(long TamanhoMaximoDosDocumentosEmBytes, string OrganizacaoPadrao, string VersaoDoServico);

/// <summary>
/// Serviço de aplicação comum aos canais do agente (REQ-A2A-002): cria e registra a execução e projeta o
/// resultado. O conteúdo recebido é dado não confiável (P10). As etapas da análise chegam nas próximas
/// mudanças; até lá, ficam como não executadas, e o status é parcial (P05, P08).
/// </summary>
internal sealed class ProcessadorDeSolicitacoes(IRepositorioDeExecucoes repositorio, AmbienteDaAnalise ambiente, TimeProvider relogio)
{
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
        var execucao = Execucao.Criar(
            Guid.Parse(solicitacao.IdExecucao!),
            Guid.Parse(solicitacao.IdSolicitacao!),
            IdentidadeDaRequisicao.Declarada(solicitacao.Usuario?.Nome, ambiente.OrganizacaoPadrao),
            relogio.GetUtcNow());
        execucao.ConcluirEtapa(Etapa.Recepcao);

        var erro = ValidarEntrada(solicitacao);
        string sintese;
        if (erro is not null)
        {
            execucao.FalharEtapa(Etapa.ValidacaoDaEntrada);
            sintese = $"A solicitação não pôde ser analisada. {erro.Mensagem}";
        }
        else
        {
            var documentos = solicitacao.Documentos!;
            foreach (var documento in documentos)
            {
                execucao.RegistrarDocumento(new DocumentoRecebido(documento.IdDocumento!, Hash(documento.Texto!), documento.Texto!.Length));
            }

            execucao.ConcluirEtapa(Etapa.ValidacaoDaEntrada);
            sintese = string.Create(
                CultureInfo.InvariantCulture,
                $"{documentos.Count} documento(s) recebido(s). Esta versão ainda não executa as validações da ECF: nenhum achado foi produzido e nenhuma conformidade é declarada.");
        }

        execucao.Concluir(relogio.GetUtcNow());
        return MontadorDoEnvelope.Montar(execucao, correlacao, ambiente.VersaoDoServico, sintese, erro);
    }

    private ErroDoEnvelope? ValidarEntrada(SolicitacaoAnaliseSofia solicitacao)
    {
        if (solicitacao.VersaoContrato != ContratoSofiaV1.Versao)
        {
            return new ErroDoEnvelope("versao_contrato_nao_suportada", $"Este agente aceita somente a versão {ContratoSofiaV1.Versao} do contrato.");
        }

        var camposInvalidos = ValidadorDaSolicitacao.CamposInvalidos(solicitacao);
        if (camposInvalidos.Count > 0)
        {
            return new ErroDoEnvelope("solicitacao_invalida", $"Campos ausentes ou inválidos: {string.Join(", ", camposInvalidos)}.");
        }

        var tamanho = solicitacao.Documentos!.Sum(documento => (long)Encoding.UTF8.GetByteCount(documento.Texto!));
        return tamanho > ambiente.TamanhoMaximoDosDocumentosEmBytes
            ? new ErroDoEnvelope("documentos_acima_do_limite", $"O texto dos documentos ultrapassa o limite de {ambiente.TamanhoMaximoDosDocumentosEmBytes} bytes; nada foi analisado.")
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
            new ErroSofia("falha_tecnica", Mensagem));
    }

    private static string Hash(string texto) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
}
