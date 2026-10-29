using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Api.Agente.Tests.Infra;

namespace Api.Agente.Tests;

public sealed partial class SegredosEmConfiguracaoTests
{
    [Fact(DisplayName = "REQ-CONFIG-001 Arquivos de configuração versionados não têm segredos preenchidos")]
    public void ArquivosVersionadosSemSegredos()
    {
        var arquivos = Directory.GetFiles(Repositorio.Caminho("src", "Api.Agente"), "appsettings*.json");

        Assert.NotEmpty(arquivos);
        foreach (var arquivo in arquivos)
        {
            Assert.Empty(ChavesDeSegredoPreenchidas(File.ReadAllText(arquivo)));
        }
    }

    [Fact(DisplayName = "REQ-CONFIG-001 Detector aponta chave de segredo preenchida e ignora valor vazio")]
    public void DetectorApontaSegredo()
    {
        const string json = """
            {
              "Banco": { "Senha": "abc", "Usuario": "leitor" },
              "OpenAI": { "ApiKey": "" },
              "ConnectionStrings": { "Consumo": "Host=x" }
            }
            """;

        Assert.Equal(["Banco:Senha", "ConnectionStrings:Consumo"], ChavesDeSegredoPreenchidas(json));
    }

    private static List<string> ChavesDeSegredoPreenchidas(string json)
    {
        var achados = new List<string>();
        var opcoes = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
        Percorrer(JsonNode.Parse(json, documentOptions: opcoes), string.Empty, achados);
        return achados;
    }

    private static void Percorrer(JsonNode? no, string caminho, List<string> achados)
    {
        if (no is JsonObject objeto)
        {
            foreach (var (chave, valor) in objeto)
            {
                Percorrer(valor, caminho.Length == 0 ? chave : $"{caminho}:{chave}", achados);
            }
        }
        else if (no is JsonValue valor && valor.TryGetValue<string>(out var texto) && texto.Length > 0 && NomeDeSegredo().IsMatch(caminho))
        {
            achados.Add(caminho);
        }
    }

    [GeneratedRegex("senha|password|chave|key|token|secret|segredo|connectionstring", RegexOptions.IgnoreCase)]
    private static partial Regex NomeDeSegredo();
}
