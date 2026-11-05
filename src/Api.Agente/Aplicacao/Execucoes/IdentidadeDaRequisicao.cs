using System.Security.Cryptography;
using System.Text;
using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Aplicacao.Execucoes;

/// <summary>
/// Único ponto que define quem pediu a análise na primeira versão, sem autenticação (SQ-12; design da
/// mudança 03, D5). Usuário e organização são declarados, não verificados. Do nome, só o SHA-256 é
/// guardado, como pseudônimo (SQ-04); a organização vem da configuração, porque o contrato não a traz.
/// </summary>
internal static class IdentidadeDaRequisicao
{
    public const string UsuarioNaoInformado = "nao-informado";

    public static IdentidadeDeclarada Declarada(string? nomeDoUsuario, string organizacaoPadrao) => new(
        string.IsNullOrWhiteSpace(nomeDoUsuario)
            ? UsuarioNaoInformado
            : Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(nomeDoUsuario))),
        organizacaoPadrao);
}
