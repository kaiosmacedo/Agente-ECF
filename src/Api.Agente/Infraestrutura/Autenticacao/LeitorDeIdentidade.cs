using System.Security.Claims;
using Api.Agente.Aplicacao.Identidade;
using Microsoft.Extensions.Options;

namespace Api.Agente.Infraestrutura.Autenticacao;

/// <summary>
/// Converte o usuário autenticado em <see cref="IdentidadeChamador"/> a partir das claims padrão do token
/// e das autorizações configuradas (D4). Nada vem do corpo, da query ou de outros cabeçalhos.
/// </summary>
internal sealed class LeitorDeIdentidade
{
    public const string ClaimId = "oid";
    public const string ClaimTipo = "idtyp";
    public const string ClaimPapeis = "roles";

    private readonly Dictionary<string, HashSet<string>> organizacoesPorIdentidade;

    public LeitorDeIdentidade(IOptions<OpcoesAutorizacoes> opcoes)
    {
        organizacoesPorIdentidade = opcoes.Value.Identidades
            .Where(i => !string.IsNullOrWhiteSpace(i.Id))
            .GroupBy(i => i.Id!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.SelectMany(i => i.Organizacoes).ToHashSet(StringComparer.Ordinal), StringComparer.Ordinal);
    }

    public IdentidadeChamador? Ler(ClaimsPrincipal usuario)
    {
        var id = usuario.FindFirst(ClaimId)?.Value;
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var tipo = string.Equals(usuario.FindFirst(ClaimTipo)?.Value, "app", StringComparison.OrdinalIgnoreCase)
            ? TipoIdentidade.Aplicacao
            : TipoIdentidade.Pessoa;
        var perfis = usuario.FindAll(ClaimPapeis)
            .Select(c => PapeisDoAgente.PerfilPorPapel.TryGetValue(c.Value, out var perfil) ? perfil : (Perfil?)null)
            .OfType<Perfil>()
            .ToHashSet();
        var organizacoes = organizacoesPorIdentidade.TryGetValue(id, out var autorizadas) ? autorizadas : [];

        return new IdentidadeChamador(id, tipo, perfis, organizacoes);
    }
}
