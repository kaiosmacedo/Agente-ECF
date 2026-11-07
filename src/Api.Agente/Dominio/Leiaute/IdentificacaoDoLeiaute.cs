using System.Globalization;

namespace Api.Agente.Dominio.Leiaute;

/// <summary>Como terminou a identificação do leiaute (REQ-LEIAUTE-001 a 004).</summary>
internal enum SituacaoDaIdentificacao
{
    Identificado,
    VersaoNaoSuportada,
    Ambigua,
}

/// <summary>O que a etapa 3 registra: tipo, leiaute, <c>COD_VER</c> e ano de <c>DT_FIN</c>, ou o motivo de não identificar.</summary>
internal sealed record IdentificacaoDoLeiaute(SituacaoDaIdentificacao Situacao, string Tipo, int? Leiaute, string? CodVer, int? AnoDoPeriodo, string? Motivo)
{
    public const string TipoEcf = "ECF";
}

/// <summary>
/// Reconhece a ECF pela primeira linha: o Registro 0000 com <c>REG</c> igual a <c>0000</c> e <c>NOME_ESC</c> igual a
/// <c>LECF</c>, delimitados por <c>|</c>, na primeira coluna (REQ-RECEPCAO-005; Manual da ECF, leiaute 12, p. 32 e 63).
/// </summary>
internal static class ReconhecedorDeEcf
{
    public const string Abertura = "|0000|LECF|";

    /// <summary>O que o código atual da Sofia acrescenta ao cortar um arquivo em 20.000 caracteres (REQ-RECEPCAO-007).</summary>
    public const string MarcadorDeCorteDaSofia = "[DOCUMENTO TRUNCADO]";

    // A marca de codificação UTF-8 não é conteúdo: é a única coisa aceita antes do Registro 0000.
    private const char MarcaDeCodificacao = '﻿';

    public static bool EhEcf(string texto) => SemMarcaDeCodificacao(texto).StartsWith(Abertura, StringComparison.Ordinal);

    public static bool CortadoPelaSofia(string texto) => texto.AsSpan().TrimEnd().EndsWith(MarcadorDeCorteDaSofia, StringComparison.Ordinal);

    public static string PrimeiraLinha(string texto)
    {
        var semMarca = SemMarcaDeCodificacao(texto);
        var fim = semMarca.IndexOfAny('\r', '\n');
        return (fim < 0 ? semMarca : semMarca[..fim]).ToString();
    }

    private static ReadOnlySpan<char> SemMarcaDeCodificacao(string texto) =>
        texto.Length > 0 && texto[0] == MarcaDeCodificacao ? texto.AsSpan(1) : texto.AsSpan();
}

/// <summary>
/// Identifica o leiaute pelo <c>COD_VER</c> do Registro 0000 e confere com o ano de <c>DT_FIN</c> e o
/// <c>SIT_ESPECIAL</c> (REQ-LEIAUTE-001 a 004). Os exemplos de preenchimento dos manuais não servem de fonte.
/// </summary>
internal static class IdentificadorDeLeiaute
{
    // Posição dos campos no Registro 0000, a mesma nos seis manuais: campo 3 COD_VER, 7 SIT_ESPECIAL e 11 DT_FIN.
    private const int CampoCodVer = 3;
    private const int CampoSitEspecial = 7;
    private const int CampoDtFin = 11;

    /// <summary>
    /// Observação do campo <c>COD_VER</c> em cada manual: leiaute 7, p. 55; 8, p. 56; 9, p. 55; 10, p. 60 e 65; 11, p. 59;
    /// 12, p. 63. No leiaute 10, o texto diz <c>00010</c> e o exemplo usa <c>0010</c>: vale <c>0010</c>, que cabe no campo
    /// de 4 caracteres (mudança 07, D5, aprovado por Kaio Macedo em 2026-11-07).
    /// </summary>
    private static readonly Dictionary<string, (int Leiaute, int AnoCalendario)> Versoes = new(StringComparer.Ordinal)
    {
        ["0007"] = (7, 2020),
        ["0008"] = (8, 2021),
        ["0009"] = (9, 2022),
        ["0010"] = (10, 2023),
        ["0011"] = (11, 2024),
        ["0012"] = (12, 2025),
    };

    /// <summary>Situações especiais do ano seguinte que o mesmo leiaute cobre (leiaute 12, p. 10): códigos 1 a 6.</summary>
    private static readonly string[] SituacoesEspeciaisDoAnoSeguinte = ["1", "2", "3", "4", "5", "6"];

    public static IdentificacaoDoLeiaute Identificar(string primeiraLinha)
    {
        var campos = primeiraLinha.Split('|');
        var codVer = campos.Length > CampoCodVer ? campos[CampoCodVer] : null;
        if (codVer is not { Length: 4 })
        {
            return Ambigua(null, null, "COD_VER vazio ou fora do formato de 4 caracteres (Registro 0000, campo 3).");
        }

        if (!Versoes.TryGetValue(codVer, out var versao))
        {
            return new IdentificacaoDoLeiaute(SituacaoDaIdentificacao.VersaoNaoSuportada, IdentificacaoDoLeiaute.TipoEcf, null, codVer, null, "versão do leiaute não suportada");
        }

        var dtFin = campos.Length > CampoDtFin ? campos[CampoDtFin] : string.Empty;
        if (!DateOnly.TryParseExact(dtFin, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fim))
        {
            return Ambigua(codVer, null, "DT_FIN fora do formato ddmmaaaa (Registro 0000, campo 11).");
        }

        var sitEspecial = campos.Length > CampoSitEspecial ? campos[CampoSitEspecial] : string.Empty;
        var coerente = fim.Year == versao.AnoCalendario
            || (fim.Year == versao.AnoCalendario + 1 && SituacoesEspeciaisDoAnoSeguinte.Contains(sitEspecial, StringComparer.Ordinal));
        return coerente
            ? new IdentificacaoDoLeiaute(SituacaoDaIdentificacao.Identificado, IdentificacaoDoLeiaute.TipoEcf, versao.Leiaute, codVer, fim.Year, null)
            : Ambigua(codVer, fim.Year, string.Create(
                CultureInfo.InvariantCulture,
                $"COD_VER {codVer} não corresponde a DT_FIN em {fim.Year}, segundo o manual do leiaute {versao.Leiaute}: ele vale para o ano-calendário {versao.AnoCalendario} e para as situações especiais 1 a 6 de {versao.AnoCalendario + 1}."));
    }

    private static IdentificacaoDoLeiaute Ambigua(string? codVer, int? ano, string motivo) =>
        new(SituacaoDaIdentificacao.Ambigua, IdentificacaoDoLeiaute.TipoEcf, null, codVer, ano, motivo);
}
