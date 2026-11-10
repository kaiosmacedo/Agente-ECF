using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Api.Agente.Tests.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Agente.Tests;

public sealed partial class DocumentacaoApiTests
{
    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    private static async Task<string> Ler(FabricaApi fabrica, string caminho)
    {
        using var resposta = await fabrica.CreateClient().GetAsync(new Uri(caminho, UriKind.Relative), Cancelamento);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return await resposta.Content.ReadAsStringAsync(Cancelamento);
    }

    [Fact(DisplayName = "REQ-DOCAPI-001 /swagger leva à página da interface do Swagger")]
    public async Task SwaggerLevaAPagina()
    {
        using var fabrica = new FabricaApi();
        using var semSeguir = fabrica.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var redirecionamento = await semSeguir.GetAsync(new Uri("/swagger", UriKind.Relative), Cancelamento);
        using var pagina = await fabrica.CreateClient().GetAsync(new Uri("/swagger/index.html", UriKind.Relative), Cancelamento);
        var html = await pagina.Content.ReadAsStringAsync(Cancelamento);

        Assert.Equal(HttpStatusCode.MovedPermanently, redirecionamento.StatusCode);
        Assert.Equal("swagger/index.html", redirecionamento.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, pagina.StatusCode);
        Assert.Equal("text/html", pagina.Content.Headers.ContentType?.MediaType);
        Assert.Contains("swagger-ui", html, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-DOCAPI-001 A página carrega o documento de /openapi/v1.json")]
    public async Task PaginaApontaParaODocumento()
    {
        using var fabrica = new FabricaApi();

        var configuracao = Configuracao(await Ler(fabrica, "/swagger/index.js"));

        var url = Assert.Single(configuracao.GetProperty("urls").EnumerateArray());
        Assert.Equal("/openapi/v1.json", url.GetProperty("url").GetString());
    }

    [Fact(DisplayName = "REQ-DOCAPI-002 O documento declara o servidor relativo, sem http://")]
    public async Task ServidorRelativo()
    {
        using var fabrica = new FabricaApi();
        using var documento = JsonDocument.Parse(await Ler(fabrica, "/openapi/v1.json"));

        var servidor = Assert.Single(documento.RootElement.GetProperty("servers").EnumerateArray());
        Assert.Equal("/", servidor.GetProperty("url").GetString());
        Assert.DoesNotContain("http://", documento.RootElement.GetProperty("servers").GetRawText(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-DOCAPI-003 Scripts e estilos vêm do próprio serviço, e o validador externo fica desligado")]
    public async Task SemRecursoExterno()
    {
        using var fabrica = new FabricaApi();
        var html = await Ler(fabrica, "/swagger/index.html");

        var recursos = Recurso().Matches(html).Select(achado => achado.Groups["caminho"].Value).ToList();
        foreach (var recurso in recursos)
        {
            Assert.DoesNotContain("//", recurso, StringComparison.Ordinal);
            await Ler(fabrica, "/swagger/" + recurso.TrimStart('.', '/'));
        }

        Assert.Contains(recursos, recurso => recurso.EndsWith(".js", StringComparison.Ordinal));
        Assert.Contains(recursos, recurso => recurso.EndsWith(".css", StringComparison.Ordinal));
        Assert.Equal("none", Configuracao(await Ler(fabrica, "/swagger/index.js")).GetProperty("validatorUrl").GetString());
    }

    [Fact(DisplayName = "REQ-DOCAPI-003 Conexão e chave configuradas não aparecem no documento")]
    public async Task DocumentoSemSegredo()
    {
        const string conexao = "Host=banco-sintetico;Password=senha-sintetica-docapi";
        const string chave = "sk-chave-sintetica-docapi";
        using var fabrica = new FabricaApi().Com(builder => builder
            .UseSetting("ConnectionStrings:Execucoes", conexao)
            .UseSetting("OpenAI:ApiKey", chave));

        var documento = await Ler(fabrica, "/openapi/v1.json");

        Assert.DoesNotContain("senha-sintetica-docapi", documento, StringComparison.Ordinal);
        Assert.DoesNotContain("banco-sintetico", documento, StringComparison.Ordinal);
        Assert.DoesNotContain(chave, documento, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-DOCAPI-004 O documento traz as quatro rotas públicas, com os métodos")]
    public async Task RotasPublicas()
    {
        using var fabrica = new FabricaApi();
        using var documento = JsonDocument.Parse(await Ler(fabrica, "/openapi/v1.json"));
        var rotas = documento.RootElement.GetProperty("paths");

        foreach (var (caminho, metodo) in new[] { ("/analises", "post"), ("/analises/{id}", "get"), ("/a2a", "post"), ("/.well-known/agent-card.json", "get") })
        {
            Assert.True(rotas.TryGetProperty(caminho, out var rota), $"Rota {caminho} ausente do documento.");
            Assert.True(rota.TryGetProperty(metodo, out _), $"Método {metodo} ausente em {caminho}.");
        }
    }

    // A configuração da interface fica num objeto literal em index.js: var configObject = {...};
    private static JsonElement Configuracao(string indexJs)
    {
        var achado = ObjetoDeConfiguracao().Match(indexJs);
        Assert.True(achado.Success, "Configuração da interface não encontrada em index.js.");
        return JsonDocument.Parse(achado.Groups["json"].Value).RootElement.Clone();
    }

    [GeneratedRegex(@"var configObject = (?<json>\{.*?\});", RegexOptions.Singleline)]
    private static partial Regex ObjetoDeConfiguracao();

    [GeneratedRegex(@"<(?:script|link)[^>]*?(?:src|href)=""(?<caminho>[^""]+)""")]
    private static partial Regex Recurso();
}
