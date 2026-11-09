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
        Assert.Contains("o programa da ECF não o exige", achado.Descricao, StringComparison.Ordinal);
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

    [Fact(DisplayName = "REQ-ESTRUT-013 Um achado impeditivo nunca some da lista: 25 observações da mesma regra antes dele não mudam o veredito")]
    public async Task ImpeditivoNaoSomeDaLista()
    {
        var observacoes = Enumerable.Repeat("|0930|F|12345678909|203||f@e.com||", 25);
        var texto = ComLinha("|0990|", [.. observacoes, "|0930|F|12345678909|||f@e.com|123|"]);
        var servico = new ServicoDeAnalise(new AmbienteDaAnalise(4_194_304, "org-teste", "teste"), TimeProvider.System, catalogos: CatalogosDosLeiautes.Ler());

        var envelope = await servico.AnalisarAsync(
            new PedidoDeAnalise(Guid.NewGuid(), Guid.NewGuid(), CanalDaAnalise.A2A, null, [new DocumentoDoPedido("doc-001", texto, "hash")]),
            "corr",
            TestContext.Current.CancellationToken);

        Assert.Equal("recusado", envelope.Veredito);
        Assert.Contains(envelope.Limitacoes, limitacao => limitacao.Contains("EST-CAMPO-001", StringComparison.Ordinal) && limitacao.Contains("não listados", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "REQ-ESTRUT-013 Mesmo no limite total, o impeditivo entra primeiro, e a lista sai na ordem das linhas")]
    public void LimiteTotalPreservaImpeditivo()
    {
        var texto = ComLinha("|0990|", [.. Enumerable.Repeat("|0930|F|12345678909|203||f@e.com||", 10), "|0930|F|12345678909|||f@e.com|123|"]);

        var resultado = new ValidadorEstrutural(Catalogo, new ParametrosDaValidacao(MaximoDeAchados: 3)).Validar(texto, "doc-001");

        Assert.Equal(3, resultado.Achados.Count);
        Assert.Contains(resultado.Achados, achado => achado.Classe == ClasseDoAchado.Impeditiva);
        var linhas = resultado.Achados.Select(achado => achado.Registros[0].Linha ?? int.MaxValue).ToList();
        Assert.Equal(linhas.Order(), linhas);
    }

    [Theory(DisplayName = "D2 Y800 e W300 sem o campo de fim adiante ficam numa linha só, sem engolir o resto do arquivo")]
    [InlineData("|Y800|001|D||{\\rtf1")]
    [InlineData("|W300|US|N|N|N|N|N|N|N|N|N|N|TEXTO")]
    public void SemCampoDeFim(string linha)
    {
        var resultado = Validar(ComLinha("|9001|", linha));

        Assert.Contains(Regras(resultado), regra => regra == "EST-ARQ-001");
        Assert.DoesNotContain(Da(resultado, "EST-OCOR-001"), achado => achado.Fundamento.Contains("9999", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "D2 O fim de linha pode ser CR LF, LF ou CR: a ECF válida não gera achado em nenhum deles")]
    [InlineData("\r\n")]
    [InlineData("\n")]
    [InlineData("\r")]
    public void FimDeLinha(string fim)
    {
        var resultado = Validar(EcfSintetica.Estrutural().Replace("\r\n", fim, StringComparison.Ordinal));

        Assert.Empty(resultado.Achados);
    }

    [Theory(DisplayName = "REQ-ESTRUT-008 No tipo C, só os caracteres 0 a 31 são recusados (p. 33)")]
    [InlineData("EMPRESA \u0096 SINTETICA", false)]
    [InlineData("EMPRESA \u007F SINTETICA", false)]
    [InlineData("EMPRESA \u001F SINTETICA", true)]
    public void CaracteresDoTipoC(string nome, bool recusa)
    {
        var resultado = Validar(EcfSintetica.ComCampoDoRegistro0000Em(EcfSintetica.Estrutural(), 5, nome));

        Assert.Equal(recusa, Da(resultado, "EST-CAMPO-002").Any());
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 No W200, o valor em reais vazio não recusa o arquivo: o sistema o calcula do valor em moeda estrangeira (p. 445)")]
    public void W200CalculadoPeloSistema()
    {
        var campos = Catalogo.Registro("W200")!.Campos;

        Assert.True(campos.Single(campo => campo.Nome == "VL_REC_NAO_REL").Verificacoes.ObrigatorioDivergente);
        Assert.False(campos.Single(campo => campo.Nome == "NUM_EMP").Verificacoes.ObrigatorioDivergente);
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 Lista que só o manual tem: valor fora dela gera observação; lista conferida pelo programa continua recusando")]
    public void ListaSoNoManual()
    {
        var soNoManual = Catalogo.Registro("0020")!.Campos.Single(campo => campo.Nome == "IND_ADM_FUN_CLU");
        var noPrograma = Catalogo.Registro("0000")!.Campos.Single(campo => campo.Nome == "IND_SIT_INI_PER");

        Assert.True(soNoManual.Verificacoes.ValoresDivergentes);
        Assert.False(noPrograma.Verificacoes.ValoresDivergentes);
        Assert.Equal(ClasseDoAchado.Impeditiva, Assert.Single(Validar(EcfSintetica.ComCampoDoRegistro0000Em(EcfSintetica.Estrutural(), 6, "9")).Achados).Classe);
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 O tipo que vira texto não herda o limite do manual, e o M990 não é indício de arquivo gerado")]
    public void AjustesDoCatalogo()
    {
        Assert.Equal(255, Catalogo.Registro("9100")!.Campos.Single(campo => campo.Nome == "NOM_REGRA").Verificacoes.TamanhoMaximo);
        Assert.Equal(255, Catalogo.Registro("W100")!.Campos.Single(campo => campo.Nome == "TIN_SUBSTITUTA").Verificacoes.TamanhoMaximo);
        Assert.True(Catalogo.Registro("M990")!.LidoNaImportacao);
        Assert.False(Catalogo.Registro("Y800")!.Campos.Single(campo => campo.Nome == "HASH").Verificacoes.Obrigatorio);
        Assert.Empty(Validar(ComLinha("|9001|", "|M001|1|", "|M990|2|")).Indicios);
    }

    [Fact(DisplayName = "REQ-ESTRUT-016 A fonte diz quando o limite veio do programa, e o S990 cita o programa como documento")]
    public void FonteDoAjuste()
    {
        var fone = Assert.Single(Da(Validar(ComLinha("|0990|", "|0930|F|12345678909|203||f@e.com|1234567890123456|")), "EST-CAMPO-003"));
        var s990 = Assert.Single(Da(Validar(ComLinha("|9001|", "|S001|1|", "|S990|A|")), "EST-CAMPO-002"));

        Assert.Contains("ajustado pelo Programa da ECF 12.2.8", fone.Fonte!.Localizacao, StringComparison.Ordinal);
        Assert.Equal("Programa da ECF 12.2.8, descritor 12003 v1", s990.Fonte!.Documento);
    }

    [Fact(DisplayName = "REQ-ESTRUT-010 O achado de contagem cita o campo, o valor e a quantidade encontrada")]
    public void ContagemCitaCampo()
    {
        var linhas = EcfSintetica.Estrutural().Split("\r\n").Select(linha => linha.StartsWith("|9999|", StringComparison.Ordinal) ? "|9999|41|" : linha);

        var achado = Assert.Single(Da(Validar(string.Join("\r\n", linhas)), "EST-TOT-003"));

        var registro = Assert.Single(achado.Registros);
        Assert.Equal(("QTD_LIN", "41"), (registro.Campo, registro.ValorEncontrado));
        Assert.Contains("25 registros", achado.Descricao, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "REQ-ESTRUT-006 A falha de uma regra não desliga as outras")]
    [InlineData("EST-CAMPO-002", "|0000|", "EST-CAMPO-006")]
    [InlineData("EST-HIER-001", "|0010|", "REGRA_OCORRENCIA_UNITARIA_ARQ")]
    public void FalhaNaoDesligaOutras(string comFalha, string antesDoRegistro, string continua)
    {
        var texto = comFalha == "EST-CAMPO-002"
            ? EcfSintetica.ComCampoDoRegistro0000Em(EcfSintetica.Estrutural(), 6, "9")
            : ComLinha(antesDoRegistro, "|0001|0|");

        var resultado = new ValidadorEstrutural(Catalogo, new ParametrosDaValidacao(), regra =>
        {
            if (regra == comFalha)
            {
                throw new InvalidOperationException("defeito simulado");
            }
        }).Validar(texto, "doc-001");

        Assert.Equal([comFalha], resultado.RegrasComFalha);
        Assert.NotEmpty(Da(resultado, continua));
    }

    [Fact(DisplayName = "REQ-ESTRUT-006 Pelo serviço, regra com falha vira falha técnica com a limitação, sem erro atribuído ao documento")]
    public async Task FalhaDeRegraPeloServico()
    {
        var registro = Catalogo.Registro("0000")!;
        var quebrado = registro with
        {
            Campos = [.. registro.Campos.Select(campo => campo.Nome == "PAT_REMAN_CIS"
                ? campo with { Verificacoes = campo.Verificacoes with { Valores = new ValoresDoCampo(null, ["x", "y"]) } }
                : campo)],
        };
        var catalogo = new CatalogoDoLeiaute(Catalogo.VersaoDoCatalogo, Catalogo.Fonte, Catalogo.OrdemDosBlocos, Catalogo.Divergencias, [quebrado, .. Catalogo.Registros.Skip(1)]);
        var servico = new ServicoDeAnalise(new AmbienteDaAnalise(4_194_304, "org-teste", "teste"), TimeProvider.System, catalogos: new Dictionary<int, CatalogoDoLeiaute> { [12] = catalogo });

        var envelope = await servico.AnalisarAsync(
            new PedidoDeAnalise(Guid.NewGuid(), Guid.NewGuid(), CanalDaAnalise.A2A, null, [new DocumentoDoPedido("doc-001", EcfSintetica.ComCampoDoRegistro0000Em(EcfSintetica.Estrutural(), 8, "5"), "hash")]),
            "corr",
            TestContext.Current.CancellationToken);

        Assert.Equal("falha-tecnica", envelope.Status);
        Assert.Contains(envelope.Limitacoes, limitacao => limitacao.Contains("EST-CAMPO-006", StringComparison.Ordinal) && limitacao.Contains("falha técnica", StringComparison.Ordinal));
        Assert.Contains(envelope.Achados, achado => achado.Categoria == "falha-tecnica" && achado.Fundamento == "EST-CAMPO-006");
        Assert.DoesNotContain(envelope.Achados, achado => achado.Categoria == "erro-estrutural");
    }

    [Fact(DisplayName = "REQ-ESTRUT-011 A ocorrência divergente do C155 consta como não verificada, com as duas páginas")]
    public void OcorrenciaDivergenteComPaginas()
    {
        var pagina = Catalogo.Registro("C155")!.Pagina;

        var item = Assert.Single(Validar(EcfSintetica.Estrutural()).NaoRealizadas, naoRealizada => naoRealizada.Item.StartsWith("Ocorrência mínima e máxima", StringComparison.Ordinal));

        Assert.Contains($"C155 (p. {pagina})", item.Item, StringComparison.Ordinal);
        Assert.Contains("p. 43 a 58", item.Motivo, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-ESTRUT-005 Nenhuma validação XSD é aplicada ao texto: o código não usa esquemas XML")]
    public void SemXsd()
    {
        var fontes = Directory.GetFiles(Repositorio.Caminho("src", "Api.Agente"), "*.cs", SearchOption.AllDirectories);

        Assert.DoesNotContain(fontes, fonte => File.ReadAllText(fonte).Contains("XmlSchema", StringComparison.Ordinal));
        Assert.Empty(Directory.GetFiles(Repositorio.Caminho("src", "Api.Agente"), "*.xsd", SearchOption.AllDirectories));
    }

    [Theory(DisplayName = "REQ-ESTRUT-012 O 9100 e o Y750 são indícios de arquivo gerado, sem gerar achado por isso")]
    [InlineData("9100", "|9100|REGRA_TESTE|M|0000|DT_INI|||||||||||||")]
    [InlineData("Y750", "|Y750|1|A|1|")]
    public void IndiciosDeArquivoGerado(string registro, string linha)
    {
        var resultado = Validar(ComLinha(registro == "9100" ? "|9900|" : "|9001|", linha));

        Assert.Contains(resultado.Indicios, indicio => indicio.Contains($"Registro {registro}", StringComparison.Ordinal));
    }

    private static IEnumerable<string> Regras(ResultadoDaValidacaoEstrutural resultado) =>
        resultado.Achados.Select(achado => achado.Fundamento.Split(' ')[0]);

    // Separa colunas da tabela em Markdown, sem contar o \| escapado.
    [GeneratedRegex(@"(?<!\\)\|")]
    private static partial Regex Colunas();

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex Citacao();
}
