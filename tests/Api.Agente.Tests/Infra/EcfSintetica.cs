using System.Text;
using Api.Agente.Dominio.Leiaute;

namespace Api.Agente.Tests.Infra;

/// <summary>ECF sintéticas de <c>Dados/ecf</c> e as variações dos testes (SQ-10). Nenhuma vem de arquivo real.</summary>
internal static class EcfSintetica
{
    public static byte[] Bytes(int leiaute = 12) =>
        File.ReadAllBytes(Repositorio.Caminho("tests", "Api.Agente.Tests", "Dados", "ecf", $"ecf-leiaute-{leiaute:00}.txt"));

    public static string Texto(int leiaute = 12) => Encoding.Latin1.GetString(Bytes(leiaute));

    /// <summary>A ECF do leiaute 12 estruturalmente válida pelo catálogo, gerada por <c>gerar-ecf-sintetica.py</c> (mudança 08).</summary>
    public static string Estrutural() =>
        Encoding.Latin1.GetString(File.ReadAllBytes(Repositorio.Caminho("tests", "Api.Agente.Tests", "Dados", "ecf", "ecf-leiaute-12-estrutural.txt")));

    /// <summary>Uma linha do registro com os campos obrigatórios válidos pelo catálogo, como faz o gerador da ECF sintética.</summary>
    public static string LinhaValida(CatalogoDoLeiaute catalogo, string codigo, (int Indice, string Valor)? trocar = null)
    {
        var registro = catalogo.Registro(codigo)!;
        var campos = registro.Campos.Select(campo => campo.Numero == 1 ? codigo : ValorValido(campo)).ToArray();
        if (trocar is { } troca)
        {
            campos[troca.Indice] = troca.Valor;
        }

        return "|" + string.Join('|', campos) + "|";
    }

    /// <summary>A ECF do leiaute 12 com um campo do Registro 0000 trocado; o campo 1 é o REG (manual, p. 63).</summary>
    public static string ComCampoDoRegistro0000(int campo, string valor, int leiaute = 12) => ComCampoDoRegistro0000Em(Texto(leiaute), campo, valor);

    /// <summary>O texto dado com um campo do Registro 0000, na primeira linha, trocado.</summary>
    public static string ComCampoDoRegistro0000Em(string texto, int campo, string valor)
    {
        var fim = texto.IndexOf('\r', StringComparison.Ordinal);
        var campos = texto[..fim].Split('|');
        campos[campo] = valor;
        return string.Join('|', campos) + texto[fim..];
    }

    private static string ValorValido(CampoDoCatalogo campo)
    {
        var v = campo.Verificacoes;
        if (v.Valores?.Lista is [var primeiro, ..])
        {
            return primeiro;
        }

        if (v.Valores?.Intervalo is [var minimo, ..])
        {
            return minimo;
        }

        if (v.Data)
        {
            return "01012025";
        }

        return v.Obrigatorio == true ? (campo.Tipo is "N" or "NS" ? "1" : "A") : string.Empty;
    }
}
