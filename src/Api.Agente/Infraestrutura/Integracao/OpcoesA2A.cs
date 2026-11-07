using System.ComponentModel.DataAnnotations;

namespace Api.Agente.Infraestrutura.Integracao;

/// <summary>Opções da seção <c>A2A</c>.</summary>
internal sealed class OpcoesA2A
{
    public const string Secao = "A2A";

    /// <summary>Endereço do endpoint A2A anunciado no cartão, como a Sofia o enxerga.</summary>
    [Required]
    [Url]
    public string UrlPublica { get; init; } = "http://localhost:8080/a2a";
}
