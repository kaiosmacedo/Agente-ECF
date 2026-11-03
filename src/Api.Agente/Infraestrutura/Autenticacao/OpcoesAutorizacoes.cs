using System.ComponentModel.DataAnnotations;
using Api.Agente.Aplicacao.Identidade;

namespace Api.Agente.Infraestrutura.Autenticacao;

/// <summary>
/// Opções da seção <c>Autorizacoes</c>: em quais organizações cada identidade pode atuar (ADR 0003, decisão 7).
/// </summary>
internal sealed class OpcoesAutorizacoes : IValidatableObject
{
    public const string Secao = "Autorizacoes";

    public List<AutorizacaoDeIdentidade> Identidades { get; init; } = [];

    // Cita só a posição da entrada com problema, nunca o valor (REQ-CONFIG-002).
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        for (var i = 0; i < Identidades.Count; i++)
        {
            var identidade = Identidades[i];
            if (string.IsNullOrWhiteSpace(identidade.Id))
            {
                yield return new ValidationResult("Identificador obrigatório.", [$"{nameof(Identidades)}:{i}:{nameof(identidade.Id)}"]);
            }

            for (var j = 0; j < identidade.Organizacoes.Count; j++)
            {
                if (!ResolvedorDeOrganizacao.FormatoValido(identidade.Organizacoes[j]))
                {
                    yield return new ValidationResult("Organização com formato inválido.", [$"{nameof(Identidades)}:{i}:{nameof(identidade.Organizacoes)}:{j}"]);
                }
            }
        }
    }
}

internal sealed class AutorizacaoDeIdentidade
{
    /// <summary>Identificador da identidade no provedor (claim <c>oid</c>).</summary>
    public string? Id { get; init; }

    /// <summary>Nome legível, só para leitura da configuração (por exemplo, "Sofia INFIS").</summary>
    public string? Nome { get; init; }

    public List<string> Organizacoes { get; init; } = [];
}
