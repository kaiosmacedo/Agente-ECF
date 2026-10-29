using System.Globalization;

namespace Api.RAG.Infraestrutura.Hospedagem;

/// <summary>Endereço de escuta definido pela plataforma: porta de PORT ou 8080 (REQ-ENTREGA-002).</summary>
internal static class EnderecoDeEscuta
{
    public const int PortaPadrao = 8080;

    public static string Resolver(string? porta)
    {
        if (string.IsNullOrWhiteSpace(porta))
        {
            return $"http://0.0.0.0:{PortaPadrao}";
        }

        // A mensagem não repete o valor recebido (REQ-CONFIG-002).
        if (!int.TryParse(porta, NumberStyles.None, CultureInfo.InvariantCulture, out var numero) || numero is < 1 or > 65535)
        {
            throw new InvalidOperationException("PORT deve ser um número de porta entre 1 e 65535.");
        }

        return $"http://0.0.0.0:{numero}";
    }
}
