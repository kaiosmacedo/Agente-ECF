using System.ComponentModel.DataAnnotations;

namespace Api.Agente.Infraestrutura.Integracao;

/// <summary>Opções da seção <c>Identidade</c> (design da mudança 03, D5).</summary>
internal sealed class OpcoesDaIdentidade
{
    public const string Secao = "Identidade";

    /// <summary>Organização registrada nas execuções: o contrato SOFIA v1.0 não tem esse campo (E07).</summary>
    [Required]
    [RegularExpression(@"^[A-Za-z0-9._-]{1,64}$")]
    public string OrganizacaoPadrao { get; init; } = "infis";
}
