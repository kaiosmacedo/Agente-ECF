using System.Reflection;
using System.Xml.Linq;
using Api.RAG.Tests.Infra;
using NetArchTest.Rules;

namespace Api.RAG.Tests;

public sealed class ArquiteturaTests
{
    private const string Servico = "Api.RAG";
    private const string OutroServico = "Api.Agente";
    private static readonly Assembly Assembly = typeof(Program).Assembly;

    private static readonly string[] Infraestrutura =
    [
        "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "Microsoft.SemanticKernel", "OpenAI",
        "ModelContextProtocol", "Npgsql", "Pgvector", "MongoDB", "StackExchange.Redis",
    ];

    [Fact(DisplayName = "REQ-QUALIDADE-003 Dominio não depende de outras camadas nem de infraestrutura")]
    public void DominioIsolado()
    {
        var resultado = Types.InAssembly(Assembly)
            .That().ResideInNamespace($"{Servico}.Dominio")
            .ShouldNot().HaveDependencyOnAny([$"{Servico}.Endpoints", $"{Servico}.Aplicacao", $"{Servico}.Infraestrutura", .. Infraestrutura])
            .GetResult();

        Assert.True(resultado.IsSuccessful, Falhas(resultado));
    }

    [Fact(DisplayName = "REQ-QUALIDADE-003 Aplicacao não depende de Endpoints nem de Infraestrutura")]
    public void AplicacaoSemEndpointsNemInfraestrutura()
    {
        var resultado = Types.InAssembly(Assembly)
            .That().ResideInNamespace($"{Servico}.Aplicacao")
            .ShouldNot().HaveDependencyOnAny($"{Servico}.Endpoints", $"{Servico}.Infraestrutura")
            .GetResult();

        Assert.True(resultado.IsSuccessful, Falhas(resultado));
    }

    [Fact(DisplayName = "REQ-QUALIDADE-004 Serviço não referencia o outro serviço")]
    public void SemReferenciaAoOutroServico()
    {
        Assert.DoesNotContain(Assembly.GetReferencedAssemblies(), referencia => referencia.Name == OutroServico);

        var pasta = Repositorio.Caminho("src", Servico);
        var projeto = XDocument.Load(Path.Combine(pasta, $"{Servico}.csproj"));
        Assert.DoesNotContain(
            projeto.Descendants("ProjectReference"),
            referencia => ((string?)referencia.Attribute("Include") ?? string.Empty).Contains(OutroServico, StringComparison.OrdinalIgnoreCase));

        // Código, configuração e projeto não citam o outro serviço (dados, migrações ou credenciais).
        var citacoes = Directory.EnumerateFiles(pasta, "*", SearchOption.AllDirectories)
            .Where(arquivo => !EmPastaDeBuild(arquivo) && Path.GetExtension(arquivo) is ".cs" or ".json" or ".csproj")
            .Where(arquivo => File.ReadAllText(arquivo).Contains(OutroServico, StringComparison.OrdinalIgnoreCase));
        Assert.Empty(citacoes);
    }

    private static bool EmPastaDeBuild(string arquivo) =>
        arquivo.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) is var partes
        && (partes.Contains("bin") || partes.Contains("obj"));

    private static string Falhas(NetArchTest.Rules.TestResult resultado) =>
        $"Tipos infratores: {string.Join(", ", resultado.FailingTypeNames ?? [])}";
}
