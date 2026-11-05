using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Api.Agente.Infraestrutura.Integracao;

/// <summary>Opções da seção <c>A2A</c>.</summary>
internal sealed class OpcoesA2A
{
    public const string Secao = "A2A";

    /// <summary>Endereço do endpoint A2A anunciado no cartão, como a Sofia o enxerga.</summary>
    [Required]
    [Url]
    public string UrlPublica { get; init; } = "http://localhost:8080/a2a";

    /// <summary>Limite do texto somado dos documentos. O padrão vale até DP-05 ser resolvida na mudança 07.</summary>
    [Required]
    [RegularExpression(@"^[1-9]\d{0,14}$")]
    public string TamanhoMaximoDosDocumentosEmBytes { get; init; } = "10485760";

    public long TamanhoMaximo => long.Parse(TamanhoMaximoDosDocumentosEmBytes, CultureInfo.InvariantCulture);
}
