using System.Text;

namespace Api.Agente.Tests.Infra;

/// <summary>ECF sintéticas de <c>Dados/ecf</c> e as variações dos testes (SQ-10). Nenhuma vem de arquivo real.</summary>
internal static class EcfSintetica
{
    public static byte[] Bytes(int leiaute = 12) =>
        File.ReadAllBytes(Repositorio.Caminho("tests", "Api.Agente.Tests", "Dados", "ecf", $"ecf-leiaute-{leiaute:00}.txt"));

    public static string Texto(int leiaute = 12) => Encoding.Latin1.GetString(Bytes(leiaute));

    /// <summary>A ECF do leiaute 12 com um campo do Registro 0000 trocado; o campo 1 é o REG (manual, p. 63).</summary>
    public static string ComCampoDoRegistro0000(int campo, string valor, int leiaute = 12)
    {
        var texto = Texto(leiaute);
        var fim = texto.IndexOf('\r', StringComparison.Ordinal);
        var campos = texto[..fim].Split('|');
        campos[campo] = valor;
        return string.Join('|', campos) + texto[fim..];
    }
}
