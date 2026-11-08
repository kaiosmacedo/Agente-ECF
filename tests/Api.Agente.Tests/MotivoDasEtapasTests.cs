using Api.Agente.Aplicacao.Analise;
using Api.Agente.Aplicacao.Conhecimento;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Infraestrutura.Recepcao;
using Api.Agente.Tests.Infra;

namespace Api.Agente.Tests;

/// <summary>
/// O motivo de cada validação obrigatória não realizada (REQ-RESULT-006), pelo serviço, como em produção: catálogo do
/// leiaute 12 e busca no manual desligada. A verificação da mudança 08 em produção achou a etapa 5 desligada descrita como
/// "ainda não implementada"; esse motivo só vale para as etapas que esta versão não tem.
/// </summary>
public sealed class MotivoDasEtapasTests
{
    private static readonly string[] EtapasQueAindaNaoExistem = ["Ferramentas externas", "Análise com LLM", "Validação da saída"];

    private static ServicoDeAnalise Servico() => new(
        new AmbienteDaAnalise(4_194_304, "org-teste", "teste"),
        TimeProvider.System,
        catalogos: CatalogosDosLeiautes.Ler(),
        consultaAoManual: new EtapaDeConsultaAoManual(new ConsultaDesativada(), 10));

    private static string Texto(string caso) => caso switch
    {
        "leiaute-12-valida" => EcfSintetica.Estrutural(),
        "leiaute-12-com-erro" => EcfSintetica.ComCampoDoRegistro0000Em(EcfSintetica.Estrutural(), 6, "9"),
        "leiaute-7" => EcfSintetica.Texto(7),
        "cortada" => EcfSintetica.Estrutural() + "[DOCUMENTO TRUNCADO]",
        "versao-nao-suportada" => EcfSintetica.ComCampoDoRegistro0000Em(EcfSintetica.Estrutural(), 3, "0013"),
        _ => throw new ArgumentOutOfRangeException(nameof(caso)),
    };

    private static async Task<EnvelopeDoResultado> Analisar(string caso) =>
        await Servico().AnalisarAsync(
            new PedidoDeAnalise(Guid.NewGuid(), Guid.NewGuid(), CanalDaAnalise.A2A, null, [new DocumentoDoPedido("doc-001", Texto(caso), "hash")]),
            "corr",
            TestContext.Current.CancellationToken);

    private static string Motivo(EnvelopeDoResultado envelope, string etapa) =>
        Assert.Single(envelope.Limitacoes, limitacao => limitacao.StartsWith($"Etapa não realizada: {etapa}. Motivo: ", StringComparison.Ordinal));

    [Theory(DisplayName = "REQ-RESULT-006 \"Ainda não implementada\" só aparece nas etapas que esta versão não tem")]
    [InlineData("leiaute-12-valida")]
    [InlineData("leiaute-12-com-erro")]
    [InlineData("leiaute-7")]
    [InlineData("cortada")]
    [InlineData("versao-nao-suportada")]
    public async Task SoAsEtapasQueNaoExistem(string caso)
    {
        var envelope = await Analisar(caso);

        var naoImplementadas = envelope.Limitacoes
            .Where(limitacao => limitacao.EndsWith($"Motivo: {MontadorDoEnvelope.MotivoNaoImplementada}.", StringComparison.Ordinal))
            .Select(limitacao => limitacao["Etapa não realizada: ".Length..limitacao.IndexOf(". Motivo:", StringComparison.Ordinal)]);
        Assert.Equal(EtapasQueAindaNaoExistem, naoImplementadas);
    }

    [Fact(DisplayName = "REQ-RESULT-006 e REQ-CONSULTA-007 Com a busca desligada, a etapa 5 diz que a busca está desligada, e a limitação continua")]
    public async Task BuscaDesligada()
    {
        var envelope = await Analisar("leiaute-12-com-erro");

        Assert.Equal("Etapa não realizada: Consulta ao manual. Motivo: a busca no manual está desligada na base de conhecimento.", Motivo(envelope, "Consulta ao manual"));
        Assert.Contains(EtapaDeConsultaAoManual.LimitacaoDesativada, envelope.Limitacoes);
    }

    [Theory(DisplayName = "REQ-RESULT-006 Sem a etapa 4, ela traz o motivo, e a etapa 5 diz que depende dela")]
    [InlineData("leiaute-7", "o leiaute 7 ainda não tem catálogo de regras")]
    [InlineData("cortada", "o documento chegou cortado, e a estrutura só se valida com o arquivo inteiro")]
    [InlineData("versao-nao-suportada", "versão do leiaute não suportada (COD_VER 0013)")]
    public async Task SemValidacaoEstrutural(string caso, string motivo)
    {
        var envelope = await Analisar(caso);

        Assert.Equal($"Etapa não realizada: Validação estrutural. Motivo: {motivo}.", Motivo(envelope, "Validação estrutural"));
        Assert.Equal($"Etapa não realizada: Consulta ao manual. Motivo: {ServicoDeAnalise.MotivoSemValidacaoEstrutural}.", Motivo(envelope, "Consulta ao manual"));
    }

    [Fact(DisplayName = "REQ-RESULT-006 Sem achado, a etapa 5 não se aplica e não aparece entre as não realizadas")]
    public async Task SemAchado()
    {
        var envelope = await Analisar("leiaute-12-valida");

        Assert.DoesNotContain(envelope.Limitacoes, limitacao => limitacao.StartsWith("Etapa não realizada: Consulta ao manual.", StringComparison.Ordinal));
        Assert.Equal("nao-aplicavel", envelope.Etapas.Single(etapa => etapa.Numero == 5).Estado);
    }
}
