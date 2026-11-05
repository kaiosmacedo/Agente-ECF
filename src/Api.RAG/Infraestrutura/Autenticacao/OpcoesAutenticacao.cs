using System.ComponentModel.DataAnnotations;

namespace Api.RAG.Infraestrutura.Autenticacao;

/// <summary>Opções da seção <c>Autenticacao</c>: provedor OAuth 2.0/OIDC que emite os tokens (ADR 0003).</summary>
internal sealed class OpcoesAutenticacao
{
    public const string Secao = "Autenticacao";

    /// <summary>Autoridade OIDC, por exemplo <c>https://login.microsoftonline.com/&lt;tenant&gt;/v2.0</c>.</summary>
    [Required]
    [Url]
    public string? Autoridade { get; init; }

    /// <summary>Audiência esperada na claim <c>aud</c>; em tokens v2 do Entra, o ID do aplicativo (client ID) do registro da Api.RAG.</summary>
    [Required]
    public string? Audiencia { get; init; }

    /// <summary>Emissores aceitos além do anunciado pela autoridade; vazio usa só o da autoridade.</summary>
    public string[] EmissoresValidos { get; init; } = [];
}
