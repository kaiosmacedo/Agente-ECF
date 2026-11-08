using System.Text;
using System.Text.RegularExpressions;
using Api.Agente.Aplicacao.Analise;
using Api.Agente.Dominio.Execucoes;
using Api.Agente.Dominio.Leiaute;
using Api.Agente.Infraestrutura.Recepcao;
using Api.Agente.Tests.Infra;

namespace Api.Agente.Tests;

/// <summary>
/// Decisão E10 (REQ-ESTRUT-016): onde o manual diverge do programa da ECF, o agente não recusa o que o programa aceita. Os
/// arquivos de <c>Dados/ecf/programa</c> são os mesmos que passaram pelo programa 12.2.8; a planilha diz o que ele fez.
/// </summary>
public sealed partial class DivergenciasDoProgramaTests
{
    private static readonly CatalogoDoLeiaute Catalogo = CatalogosDosLeiautes.Ler()[12];

    private static string Pasta(params string[] resto) => Repositorio.Caminho(["tests", "Api.Agente.Tests", "Dados", "ecf", "programa", .. resto]);

    public static TheoryData<string> Arquivos => [.. Planilha().Keys];

    /// <summary>Regras e veredito esperados de cada linha da tabela; o <c>\|</c> escapado não separa colunas.</summary>
    private static Dictionary<string, (IReadOnlyList<string> Regras, string Veredito)> Planilha() =>
        File.ReadAllLines(Pasta("resultados.md"))
            .Where(linha => linha.StartsWith("| `", StringComparison.Ordinal))
            .Select(linha => Colunas().Split(linha))
            .ToDictionary(
                celulas => celulas[1].Trim().Trim('`')[..^".txt".Length],
                celulas => ((IReadOnlyList<string>)[.. Citacao().Matches(celulas[5]).Select(item => item.Groups[1].Value)], celulas[6].Trim().Trim('`')),
                StringComparer.Ordinal);

    [Fact(DisplayName = "REQ-ESTRUT-016 A planilha tem uma linha para cada arquivo testado no programa")]
    public void PlanilhaCompleta()
    {
        var arquivos = Directory.GetFiles(Pasta(), "*.txt").Select(Path.GetFileNameWithoutExtension);

        Assert.Equal(arquivos.Order(StringComparer.Ordinal), Planilha().Keys.Order(StringComparer.Ordinal));
    }

    [Theory(DisplayName = "REQ-ESTRUT-016 Pelo serviço, cada arquivo testado no programa dá as regras e o veredito da planilha")]
    [MemberData(nameof(Arquivos))]
    public async Task ResultadoDoAgente(string arquivo)
    {
        var texto = Encoding.Latin1.GetString(File.ReadAllBytes(Pasta(arquivo + ".txt")));
        var servico = new ServicoDeAnalise(new AmbienteDaAnalise(4_194_304, "org-teste", "teste"), TimeProvider.System, catalogos: CatalogosDosLeiautes.Ler());

        var envelope = await servico.AnalisarAsync(
            new PedidoDeAnalise(Guid.NewGuid(), Guid.NewGuid(), CanalDaAnalise.A2A, null, [new DocumentoDoPedido("doc-001", texto, "hash")]),
            "corr",
            TestContext.Current.CancellationToken);

        var (regras, veredito) = Planilha()[arquivo];
        Assert.Equal(regras.Order(StringComparer.Ordinal), envelope.Achados.Select(achado => achado.Fundamento.Split(' ')[0]).Distinct().Order(StringComparer.Ordinal));
        Assert.Equal(veredito, envelope.Veredito);
    }

    private static ResultadoDaValidacaoEstrutural Validar(string texto) =>
        new ValidadorEstrutural(Catalogo, new ParametrosDaValidacao()).Validar(texto, "doc-001");

    private static string ComLinha(string antesDoRegistro, params string[] novas)
    {
        var linhas = EcfSintetica.Estrutural().Split("\r\n").ToList();
        linhas.InsertRange(linhas.FindIndex(linha => linha.StartsWith(antesDoRegistro, StringComparison.Ordinal)), novas);
        return string.Join("\r\n", linhas);
    }

    private static IEnumerable<Achado> Da(ResultadoDaValidacaoEstrutural resultado, string regra) =>
        resultado.Achados.Where(achado => achado.Fundamento.StartsWith(regra + " ", StringComparison.Ordinal));

    [Fact(DisplayName = "REQ-ESTRUT-016 O S990 está no catálogo, com o programa como fonte, sem página do manual e sem ser exigido")]
    public void S990NoCatalogo()
    {
        var registro = Catalogo.Registro("S990")!;

        Assert.True(registro.SoNoPrograma);
        Assert.Equal((0, false), (registro.Pagina, registro.ObrigatorioNaEntradaENaSaida));
        Assert.Equal("S700", Catalogo.Registros[Catalogo.Registros.ToList().FindIndex(item => item.Codigo == "S990") - 1].Codigo);
        Assert.Empty(Catalogo.Problemas());
        Assert.Contains((Catalogo with { Fonte = Catalogo.Fonte with { Programa = null } }).Problemas(), problema => problema.StartsWith("fonte ausente: Registro S990", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 Achado num registro que só o programa tem cita o programa como fonte")]
    public void FonteNoPrograma()
    {
        var resultado = Validar(ComLinha("|9001|", "|S001|1|", "|S990|A|"));

        var achado = Assert.Single(Da(resultado, "EST-CAMPO-002"));
        Assert.Contains("Programa da ECF 12.2.8, descritor 12003 v1", achado.Fonte!.Localizacao, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "REQ-ESTRUT-016 O tamanho que vale é o do programa, e acima dele o agente continua recusando")]
    [InlineData("|0930|F|12345678909|203||f@e.com|123456789012345|", false)]
    [InlineData("|0930|F|12345678909|203||f@e.com|1234567890123456|", true)]
    public void TamanhoDoPrograma(string linha, bool recusa)
    {
        var resultado = Validar(ComLinha("|0990|", linha));

        Assert.Equal(recusa, Da(resultado, "EST-CAMPO-003").Any(achado => achado.Classe == ClasseDoAchado.Impeditiva));
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 O sinal aceito pelo programa não abre para letras: o W200 continua numérico")]
    public void TipoNsContinuaNumerico()
    {
        var w200 = EcfSintetica.LinhaValida(Catalogo, "W200", (17, "12A"));

        var resultado = Validar(ComLinha("|9001|", w200));

        Assert.Contains(Da(resultado, "EST-CAMPO-002"), achado => achado.Classe == ClasseDoAchado.Impeditiva);
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 Campo obrigatório só no manual, vazio, gera observação que cita o manual e o programa")]
    public void ObservacaoNaObrigatoriedade()
    {
        var resultado = Validar(ComLinha("|0990|", "|0930|F|12345678909|203||f@e.com||"));

        var achado = Assert.Single(Da(resultado, "EST-CAMPO-001"));
        Assert.Equal(ClasseDoAchado.Observacao, achado.Classe);
        Assert.Contains("o programa da ECF o aceita vazio", achado.Descricao, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 Campo obrigatório também no programa continua recusando o arquivo")]
    public void ObrigatorioNosDoisContinuaImpeditivo()
    {
        var resultado = Validar(ComLinha("|0990|", "|0930|F|12345678909|||f@e.com||"));

        Assert.Contains(Da(resultado, "EST-CAMPO-001"), achado => achado.Classe == ClasseDoAchado.Impeditiva && achado.Fundamento.Contains("IDENT_QUALIF", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 A falta do C001 é observação, e a do 0030 e do 9999 continuam recusando")]
    public void RegistrosAusentes()
    {
        var linhas = EcfSintetica.Estrutural().Split("\r\n").Where(linha => !linha.StartsWith("|C001|", StringComparison.Ordinal) && !linha.StartsWith("|0030|", StringComparison.Ordinal) && !linha.StartsWith("|9999|", StringComparison.Ordinal));

        var resultado = Validar(string.Join("\r\n", linhas));

        var classes = Da(resultado, "EST-OCOR-001").ToDictionary(achado => achado.Fundamento.Split(' ')[1], achado => achado.Classe);
        Assert.Equal(ClasseDoAchado.Observacao, classes["C001"]);
        Assert.Equal(ClasseDoAchado.Impeditiva, classes["0030"]);
        Assert.Equal(ClasseDoAchado.Impeditiva, classes["9999"]);
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 Dois ativos iguais em países diferentes não são duplicidade: vale a chave do programa no Y590")]
    public void ChaveDoPrograma()
    {
        Assert.Contains("PAIS", Catalogo.Registro("Y590")!.Chave);
        Assert.Empty(Catalogo.Registro("X370")!.Chave);
    }

    // Separa colunas da tabela em Markdown, sem contar o \| escapado.
    [GeneratedRegex(@"(?<!\\)\|")]
    private static partial Regex Colunas();

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex Citacao();
}
