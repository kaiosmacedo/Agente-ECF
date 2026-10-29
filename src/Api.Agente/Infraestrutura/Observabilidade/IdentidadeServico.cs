using System.Reflection;

namespace Api.Agente.Infraestrutura.Observabilidade;

/// <summary>Nome e versão do serviço gravados em cada log (REQ-OBSERV-002, P19).</summary>
internal sealed record IdentidadeServico(string Nome, string Versao)
{
    public static IdentidadeServico De(string nome, Assembly assembly) =>
        new(nome, assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "desconhecida");
}
