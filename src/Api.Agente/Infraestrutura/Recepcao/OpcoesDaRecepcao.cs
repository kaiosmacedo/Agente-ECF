using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Api.Agente.Aplicacao.Analise;
using Api.Agente.Aplicacao.Conhecimento;
using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Dominio.Leiaute;
using Api.Agente.Infraestrutura.Configuracao;
using Api.Agente.Infraestrutura.Integracao;
using Api.Agente.Infraestrutura.Observabilidade;
using Microsoft.Extensions.Options;

namespace Api.Agente.Infraestrutura.Recepcao;

/// <summary>
/// Opções da seção <c>Recepcao</c>, as mesmas para A2A e REST (mudança 07, D4). Os valores ficam em texto, para que um
/// erro de conversão não exponha o valor recebido (REQ-CONFIG-002).
/// </summary>
internal sealed class OpcoesDaRecepcao
{
    public const string Secao = "Recepcao";

    /// <summary>4 MiB: abaixo do limite do corpo no Vercel, de 4,4 MB a 4,6 MB, já contando o envelope JSON.</summary>
    [Required]
    [RegularExpression(@"^[1-9]\d{0,14}$")]
    public string TamanhoMaximoDosDocumentosEmBytes { get; init; } = "4194304";

    /// <summary>Vazio: igual ao tamanho máximo, porque o manual não limita o campo ARQ_RTF do Y800 (leiaute 12, p. 611).</summary>
    [RegularExpression(@"^[1-9]\d{0,9}$")]
    public string? TamanhoMaximoDaLinhaEmCaracteres { get; init; }

    [Required]
    [RegularExpression(@"^[1-9]\d{0,9}$")]
    public string QuantidadeMaximaDeLinhas { get; init; } = "1000000";

    public long TamanhoMaximo => long.Parse(TamanhoMaximoDosDocumentosEmBytes, CultureInfo.InvariantCulture);

    public int TamanhoMaximoDaLinha => TamanhoMaximoDaLinhaEmCaracteres is { } valor
        ? int.Parse(valor, CultureInfo.InvariantCulture)
        : (int)Math.Min(TamanhoMaximo, int.MaxValue);

    public int MaximoDeLinhas => int.Parse(QuantidadeMaximaDeLinhas, CultureInfo.InvariantCulture);
}

/// <summary>Registro do serviço de análise comum aos canais (REQ-RECEPCAO-008).</summary>
internal static class RegistroDaRecepcao
{
    public static IServiceCollection AdicionarRecepcao(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesDaRecepcao>(configuracao, OpcoesDaRecepcao.Secao);
        servicos.AdicionarOpcoesValidadas<OpcoesDaIdentidade>(configuracao, OpcoesDaIdentidade.Secao);
        servicos.AddSingleton(provedor =>
        {
            var recepcao = provedor.GetRequiredService<IOptions<OpcoesDaRecepcao>>().Value;
            var identidade = provedor.GetRequiredService<IOptions<OpcoesDaIdentidade>>().Value;
            return new AmbienteDaAnalise(
                recepcao.TamanhoMaximo,
                identidade.OrganizacaoPadrao,
                provedor.GetRequiredService<IdentidadeServico>().Versao,
                recepcao.TamanhoMaximoDaLinha,
                recepcao.MaximoDeLinhas);
        });
        servicos.AdicionarOpcoesValidadas<OpcoesDaValidacaoEstrutural>(configuracao, OpcoesDaValidacaoEstrutural.Secao);
        servicos.AddSingleton(_ => CatalogosDosLeiautes.Ler());
        servicos.AddSingleton(provedor =>
        {
            var limites = provedor.GetRequiredService<IOptions<OpcoesDaValidacaoEstrutural>>().Value;
            return new ServicoDeAnalise(
                provedor.GetRequiredService<AmbienteDaAnalise>(),
                provedor.GetRequiredService<TimeProvider>(),
                catalogos: provedor.GetRequiredService<IReadOnlyDictionary<int, CatalogoDoLeiaute>>(),
                consultaAoManual: provedor.GetRequiredService<EtapaDeConsultaAoManual>(),
                validacao: new ParametrosDaValidacao(limites.MaximoDeAchadosPorRegra, limites.MaximoDeAchados));
        });
        servicos.AddSingleton(provedor => new ProcessadorDeSolicitacoes(
            provedor.GetRequiredService<IRepositorioDeExecucoes>(), provedor.GetRequiredService<ServicoDeAnalise>()));
        return servicos;
    }
}
