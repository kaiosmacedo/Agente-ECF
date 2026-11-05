using System.Text.RegularExpressions;

namespace Api.Agente.Aplicacao.Identidade;

/// <summary>Resultado da resolução da organização de uma requisição.</summary>
internal enum SituacaoDaOrganizacao
{
    Resolvida,
    FormatoInvalido,
    NaoAutorizada,
    Obrigatoria,
    SemOrganizacao,
}

internal sealed record ResultadoDaOrganizacao(SituacaoDaOrganizacao Situacao, string? Organizacao = null);

/// <summary>
/// Decide em nome de qual organização a requisição atua (REQ-AUTH-003): a informada, se autorizada,
/// ou a única autorizada. O valor informado é entrada não confiável e nunca é repetido em mensagens.
/// </summary>
internal static partial class ResolvedorDeOrganizacao
{
    public static ResultadoDaOrganizacao Resolver(IdentidadeChamador identidade, string? informada, bool obrigatoria)
    {
        if (!string.IsNullOrEmpty(informada))
        {
            if (!FormatoValido(informada))
            {
                return new(SituacaoDaOrganizacao.FormatoInvalido);
            }

            return identidade.PodeAcessar(informada)
                ? new(SituacaoDaOrganizacao.Resolvida, informada)
                : new(SituacaoDaOrganizacao.NaoAutorizada);
        }

        return identidade.Organizacoes.Count switch
        {
            1 => new(SituacaoDaOrganizacao.Resolvida, identidade.Organizacoes.Single()),
            0 when obrigatoria => new(SituacaoDaOrganizacao.SemOrganizacao),
            > 1 when obrigatoria => new(SituacaoDaOrganizacao.Obrigatoria),
            _ => new(SituacaoDaOrganizacao.Resolvida),
        };
    }

    /// <summary>Identificador opaco de 1 a 64 caracteres: letras, dígitos, hífen, sublinhado e ponto.</summary>
    public static bool FormatoValido(string organizacao) => Formato().IsMatch(organizacao);

    [GeneratedRegex(@"\A[A-Za-z0-9._-]{1,64}\z")]
    private static partial Regex Formato();
}
