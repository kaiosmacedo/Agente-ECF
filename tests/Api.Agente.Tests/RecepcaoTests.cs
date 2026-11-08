using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Api.Agente.Aplicacao.Analise;
using Api.Agente.Aplicacao.ContratoSofia;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Hosting;

namespace Api.Agente.Tests;

/// <summary>Etapas 2 e 3 pelo serviço comum aos canais e pelo <c>POST /analises</c> (mudança 07; REQ-RECEPCAO-001 a 008).</summary>
public sealed class RecepcaoTests
{
    private static ServicoDeAnalise Servico(long tamanho = 4_194_304, int linha = int.MaxValue, int linhas = int.MaxValue) =>
        new(new AmbienteDaAnalise(tamanho, "org-teste", "teste", linha, linhas), TimeProvider.System);

    private static DocumentoDoPedido Documento(string id, string texto) =>
        new(id, texto, Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(texto))));

    private static Task<EnvelopeDoResultado> Analisar(ServicoDeAnalise servico, params DocumentoDoPedido[] documentos) =>
        servico.AnalisarAsync(new PedidoDeAnalise(Guid.NewGuid(), Guid.NewGuid(), CanalDaAnalise.A2A, "Usuario Teste", documentos), "corr-recepcao", TestContext.Current.CancellationToken);

    private static string Estado(EnvelopeDoResultado envelope, int etapa) => envelope.Etapas.Single(e => e.Numero == etapa).Estado;

    [Fact(DisplayName = "REQ-RECEPCAO-006 Com uma ECF e uma NF-e, a ECF é analisada e a NF-e fica nas observações")]
    public async Task EcfENfe()
    {
        var envelope = await Analisar(Servico(), Documento("doc-001", EcfSintetica.Texto()), Documento("doc-002", "<NFe>nota sintética</NFe>"));

        Assert.Equal(("parcial", "concluida", "concluida"), (envelope.Status, Estado(envelope, 2), Estado(envelope, 3)));
        Assert.Contains("Documento doc-002 não analisado por este agente: não é uma ECF.", envelope.Observacoes!);
        Assert.Equal(("identificado", 12, "0012", 2025), (envelope.Leiaute!.Situacao, envelope.Leiaute.Leiaute!.Value, envelope.Leiaute.CodVer, envelope.Leiaute.AnoDoPeriodo!.Value));
        Assert.Empty(envelope.Achados);
    }

    [Fact(DisplayName = "REQ-RECEPCAO-006 Sem nenhuma ECF, a entrada é rejeitada com nenhuma_ecf")]
    public async Task NenhumaEcf()
    {
        var envelope = await Analisar(Servico(), Documento("doc-001", "<NFe>nota sintética</NFe>"));

        Assert.Equal(("entrada-rejeitada", "nenhuma_ecf"), (envelope.Status, envelope.Erro!.Codigo));
        Assert.Contains("formato não aceito", envelope.Erro.Mensagem, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("com-falha", Estado(envelope, 2));
        Assert.All(Enumerable.Range(3, 6), etapa => Assert.Equal("nao-executada", Estado(envelope, etapa)));
    }

    [Fact(DisplayName = "REQ-RECEPCAO-006 Com duas ECF, a entrada é rejeitada com mais_de_uma_ecf")]
    public async Task DuasEcf()
    {
        var envelope = await Analisar(Servico(), Documento("doc-001", EcfSintetica.Texto()), Documento("doc-002", EcfSintetica.Texto(11)));

        Assert.Equal(("entrada-rejeitada", "mais_de_uma_ecf"), (envelope.Status, envelope.Erro!.Codigo));
    }

    [Fact(DisplayName = "REQ-RECEPCAO-002 Acima do tamanho máximo, a entrada é rejeitada e as etapas 3 a 9 não rodam")]
    public async Task TamanhoAcimaDoLimite()
    {
        var envelope = await Analisar(Servico(tamanho: 100), Documento("doc-001", EcfSintetica.Texto()));

        Assert.Equal(("entrada-rejeitada", "documentos_acima_do_limite"), (envelope.Status, envelope.Erro!.Codigo));
        Assert.All(Enumerable.Range(3, 6), etapa => Assert.Equal("nao-executada", Estado(envelope, etapa)));
    }

    [Theory(DisplayName = "REQ-RECEPCAO-004 Linha maior que o limite ou linhas demais rejeitam a entrada")]
    [InlineData(50, int.MaxValue, "linha_acima_do_limite")]
    [InlineData(int.MaxValue, 3, "linhas_acima_do_limite")]
    public async Task LimitesDeLinha(int linha, int linhas, string codigo)
    {
        var envelope = await Analisar(Servico(linha: linha, linhas: linhas), Documento("doc-001", EcfSintetica.Texto()));

        Assert.Equal(("entrada-rejeitada", codigo), (envelope.Status, envelope.Erro!.Codigo));
    }

    [Fact(DisplayName = "REQ-RECEPCAO-007 ECF cortada pela Sofia: leiaute identificado, etapas 4 e 5 não executadas e a limitação declarada")]
    public async Task EcfCortada()
    {
        var envelope = await Analisar(Servico(), Documento("doc-001", EcfSintetica.Texto() + "[DOCUMENTO TRUNCADO]"));

        Assert.Equal(("identificado", 12), (envelope.Leiaute!.Situacao, envelope.Leiaute.Leiaute!.Value));
        Assert.Equal(("nao-executada", "nao-executada"), (Estado(envelope, 4), Estado(envelope, 5)));
        Assert.Contains("documento cortado antes de chegar ao agente", envelope.Limitacoes);
        Assert.Empty(envelope.Achados);
        Assert.Equal("parcial", envelope.Status);
    }

    [Fact(DisplayName = "REQ-LEIAUTE-002 Versão não suportada: etapa 3 concluída, valor registrado e etapas 4 e 5 não executadas")]
    public async Task VersaoNaoSuportada()
    {
        var envelope = await Analisar(Servico(), Documento("doc-001", EcfSintetica.ComCampoDoRegistro0000(3, "0013")));

        Assert.Equal(("concluida", "nao-suportado", "0013"), (Estado(envelope, 3), envelope.Leiaute!.Situacao, envelope.Leiaute.CodVer));
        Assert.Equal(("nao-executada", "nao-executada"), (Estado(envelope, 4), Estado(envelope, 5)));
        Assert.Contains(envelope.Limitacoes, limitacao => limitacao.StartsWith("versão do leiaute não suportada", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "REQ-LEIAUTE-003 COD_VER vazio: etapa 3 com falha do documento e status parcial, não falha técnica")]
    public async Task CodVerVazio()
    {
        var envelope = await Analisar(Servico(), Documento("doc-001", EcfSintetica.ComCampoDoRegistro0000(3, string.Empty)));

        var etapa = envelope.Etapas.Single(e => e.Numero == 3);
        Assert.Equal(("com-falha", "documento"), (etapa.Estado, etapa.Causa));
        Assert.Equal(("parcial", "parcial"), (envelope.Status, envelope.Veredito));
        Assert.True(envelope.RevisaoHumana);
    }

    [Fact(DisplayName = "REQ-LEIAUTE-003 Exceção no identificador: etapa 3 com falha técnica e status falha-tecnica, sem derrubar o pedido")]
    public async Task FalhaInternaDoIdentificador()
    {
        var servico = new ServicoDeAnalise(
            new AmbienteDaAnalise(4_194_304, "org-teste", "teste"), TimeProvider.System, _ => throw new InvalidOperationException("defeito simulado"));

        var envelope = await Analisar(servico, Documento("doc-001", EcfSintetica.Texto()));

        var etapa = envelope.Etapas.Single(e => e.Numero == 3);
        Assert.Equal(("com-falha", "tecnica"), (etapa.Estado, etapa.Causa));
        Assert.Equal(("falha-tecnica", "nao-avaliado"), (envelope.Status, envelope.Veredito));
        Assert.Contains(envelope.Achados, achado => achado.Categoria == "falha-tecnica");
        Assert.DoesNotContain("defeito simulado", JsonSerializer.Serialize(envelope, ContratoSofiaV1.Json), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-LEIAUTE-004 COD_VER de outro ano: identificação ambígua, com o motivo na limitação")]
    public async Task VersaoDeOutroAno()
    {
        var texto = EcfSintetica.Texto().Replace("|01012025|31122025|", "|01012023|31122023|", StringComparison.Ordinal);

        var envelope = await Analisar(Servico(), Documento("doc-001", texto));

        Assert.Equal("ambiguo", envelope.Leiaute!.Situacao);
        Assert.Contains(envelope.Limitacoes, limitacao => limitacao.Contains("0012", StringComparison.Ordinal) && limitacao.Contains("2023", StringComparison.Ordinal));
    }

    private static MultipartFormDataContent Formulario(byte[] arquivo, string nome = "ecf.txt", string parte = "arquivo")
    {
        var conteudo = new ByteArrayContent(arquivo);
        conteudo.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        return new MultipartFormDataContent { { conteudo, parte, nome } };
    }

    private static async Task<(HttpStatusCode Status, JsonDocument Corpo, Uri? Local)> Enviar(FabricaApi fabrica, HttpContent conteudo)
    {
        using var resposta = await fabrica.CreateClient().PostAsync(new Uri("/analises", UriKind.Relative), conteudo, TestContext.Current.CancellationToken);
        return (resposta.StatusCode, JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)), resposta.Headers.Location);
    }

    [Fact(DisplayName = "REQ-RECEPCAO-001 POST /analises cria a execução, com o hash SHA-256 dos bytes enviados")]
    public async Task EnvioValido()
    {
        using var fabrica = new FabricaApi();
        var bytes = EcfSintetica.Bytes();

        var (status, corpo, local) = await Enviar(fabrica, Formulario(bytes));

        Assert.Equal(HttpStatusCode.Created, status);
        var raiz = corpo.RootElement;
        Assert.Equal($"/analises/{raiz.GetProperty("idExecucao").GetString()}", local!.OriginalString);
        Assert.Equal(("parcial", "rest", 12), (raiz.GetProperty("status").GetString(), raiz.GetProperty("canal").GetString(), raiz.GetProperty("leiaute").GetProperty("leiaute").GetInt32()));
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(bytes)), raiz.GetProperty("documentos")[0].GetProperty("hash").GetString());
    }

    [Fact(DisplayName = "REQ-RECEPCAO-005 POST /analises lê o arquivo como ISO 8859-1: um caractere por byte")]
    public async Task LeituraEmIso88591()
    {
        using var fabrica = new FabricaApi();
        var bytes = Encoding.UTF8.GetBytes(EcfSintetica.ComCampoDoRegistro0000(5, "EMPRESA AÇÚCAR"));

        var (_, corpo, _) = await Enviar(fabrica, Formulario(bytes));

        Assert.Equal(bytes.Length, corpo.RootElement.GetProperty("documentos")[0].GetProperty("caracteres").GetInt32());
    }

    [Fact(DisplayName = "REQ-RECEPCAO-002 POST /analises acima do limite registra a entrada rejeitada, sem ler o resto")]
    public async Task EnvioAcimaDoLimite()
    {
        using var fabrica = new FabricaApi().Com(builder => builder.UseSetting("Recepcao:TamanhoMaximoDosDocumentosEmBytes", "100"));

        var (status, corpo, _) = await Enviar(fabrica, Formulario(EcfSintetica.Bytes()));

        Assert.Equal(HttpStatusCode.Created, status);
        Assert.Equal(("entrada-rejeitada", "documentos_acima_do_limite"), (corpo.RootElement.GetProperty("status").GetString(), corpo.RootElement.GetProperty("erro").GetProperty("codigo").GetString()));
        Assert.Empty(corpo.RootElement.GetProperty("documentos").EnumerateArray());
    }

    [Fact(DisplayName = "REQ-RECEPCAO-004 O nome do arquivo com caminho não é usado: nada é gravado e o nome não aparece")]
    public async Task NomeComCaminho()
    {
        using var fabrica = new FabricaApi();

        var (status, corpo, _) = await Enviar(fabrica, Formulario(EcfSintetica.Bytes(), "../../etc/passwd"));

        Assert.Equal(HttpStatusCode.Created, status);
        Assert.DoesNotContain("passwd", corpo.RootElement.GetRawText(), StringComparison.Ordinal);
        Assert.DoesNotContain(fabrica.Logs.Registros, registro => registro.TextoCompleto.Contains("passwd", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "REQ-RECEPCAO-001 Sem multipart (415) ou sem a parte \"arquivo\" (400), nenhuma execução é criada")]
    [InlineData("texto", HttpStatusCode.UnsupportedMediaType)]
    [InlineData("outra-parte", HttpStatusCode.BadRequest)]
    public async Task EnvioMalFormado(string caso, HttpStatusCode esperado)
    {
        using var fabrica = new FabricaApi();
        HttpContent conteudo = caso == "texto" ? new StringContent(EcfSintetica.Texto()) : Formulario(EcfSintetica.Bytes(), parte: "outra");

        using var resposta = await fabrica.CreateClient().PostAsync(new Uri("/analises", UriKind.Relative), conteudo, TestContext.Current.CancellationToken);

        Assert.Equal(esperado, resposta.StatusCode);
    }

    [Fact(DisplayName = "SQ-04 Os logs do envio não trazem o texto do arquivo nem o nome dele")]
    public async Task LogsSemConteudo()
    {
        using var fabrica = new FabricaApi();

        await Enviar(fabrica, Formulario(Encoding.Latin1.GetBytes(EcfSintetica.ComCampoDoRegistro0000(5, "EMPRESA SIGILOSA 456")), "balanco-sigiloso.txt"));

        Assert.DoesNotContain(fabrica.Logs.Registros, registro =>
            registro.TextoCompleto.Contains("SIGILOSA 456", StringComparison.Ordinal) || registro.TextoCompleto.Contains("balanco-sigiloso", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "REQ-A2A-007 e 008 O cartão 1.2.0 descreve a especialidade ECF, com exemplos e as tags ecf e sped")]
    public async Task CartaoDaEcf()
    {
        using var fabrica = new FabricaApi();
        using var resposta = await fabrica.CreateClient().GetAsync(new Uri("/.well-known/agent-card.json", UriKind.Relative), TestContext.Current.CancellationToken);
        using var cartao = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var skill = Assert.Single(cartao.RootElement.GetProperty("skills").EnumerateArray());
        var descricao = skill.GetProperty("description").GetString()!;
        Assert.Equal("1.2.0", cartao.RootElement.GetProperty("version").GetString());
        Assert.Contains("valida a estrutura", descricao, StringComparison.Ordinal);
        Assert.Contains("ECF", descricao, StringComparison.Ordinal);
        Assert.Contains("leiautes 7 a 12", descricao, StringComparison.Ordinal);
        Assert.Contains("|0000|LECF|", descricao, StringComparison.Ordinal);
        Assert.Contains("Não analisa outros documentos", descricao, StringComparison.Ordinal);
        Assert.NotEmpty(skill.GetProperty("examples").EnumerateArray());
        var tags = skill.GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()).ToList();
        Assert.Contains("ecf", tags);
        Assert.Contains("sped", tags);
    }

    [Fact(DisplayName = "REQ-RECEPCAO-008 Pelo A2A, a projeção para a Sofia traz o leiaute e o documento não analisado nas observações")]
    public async Task ObservacoesParaASofia()
    {
        var processador = new ProcessadorDeSolicitacoes(new RepositorioEmMemoria(), new AmbienteDaAnalise(4_194_304, "org-teste", "teste"), TimeProvider.System);

        var desfecho = await processador.ProcessarAsync(ContratoDeTeste.Solicitacao(), "corr", TestContext.Current.CancellationToken);

        var resultado = desfecho.Resultado!;
        Assert.Equal("parcial", resultado.Status);
        Assert.Contains(resultado.Observacoes, observacao => observacao.StartsWith("Leiaute identificado no documento doc-001: 12", StringComparison.Ordinal));
        Assert.Contains("Documento doc-002 não analisado por este agente: não é uma ECF.", resultado.Observacoes);
    }
}
