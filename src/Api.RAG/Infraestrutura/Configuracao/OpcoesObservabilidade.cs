using System.ComponentModel.DataAnnotations;

namespace Api.RAG.Infraestrutura.Configuracao;

/// <summary>Opções da seção <c>Observabilidade</c>.</summary>
internal sealed class OpcoesObservabilidade
{
    public const string Secao = "Observabilidade";

    /// <summary>Nível mínimo de log, com o nome exato de um <see cref="LogLevel"/>.</summary>
    [Required]
    [AllowedValues("Trace", "Debug", "Information", "Warning", "Error", "Critical", "None")]
    public string NivelMinimo { get; init; } = nameof(LogLevel.Information);
}
