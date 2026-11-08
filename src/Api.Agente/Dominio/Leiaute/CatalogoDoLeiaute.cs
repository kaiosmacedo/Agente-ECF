namespace Api.Agente.Dominio.Leiaute;

/// <summary>
/// Catálogo estrutural de um leiaute, gerado da versão em Word do manual por <c>.github/scripts/catalogo/gerar-catalogo.py</c>
/// (mudança 08, D1). Cada registro e cada campo trazem a página do manual (REQ-ESTRUT-001; P01).
/// </summary>
internal sealed record CatalogoDoLeiaute(
    string VersaoDoCatalogo, FonteDoCatalogo Fonte, IReadOnlyList<string> OrdemDosBlocos, IReadOnlyList<string> Divergencias, IReadOnlyList<RegistroDoCatalogo> Registros)
{
    private Dictionary<string, RegistroDoCatalogo>? porCodigo;

    public RegistroDoCatalogo? Registro(string codigo) =>
        (porCodigo ??= Registros.ToDictionary(registro => registro.Codigo, StringComparer.Ordinal)).GetValueOrDefault(codigo);

    /// <summary>
    /// O que impede usar o catálogo: registro ou campo sem página ("fonte ausente", REQ-ESTRUT-001), tipo desconhecido e
    /// registro filho sem pai no catálogo. Vazio quando o catálogo pode ser usado.
    /// </summary>
    public IEnumerable<string> Problemas()
    {
        var codigos = Registros.Select(registro => registro.Codigo).ToHashSet(StringComparer.Ordinal);
        foreach (var registro in Registros)
        {
            if (registro.Pagina <= 0)
            {
                yield return $"fonte ausente: Registro {registro.Codigo}";
            }

            if (registro.Nivel > 0 && (registro.Pai is null || !codigos.Contains(registro.Pai)))
            {
                yield return $"pai ausente: Registro {registro.Codigo}";
            }

            foreach (var campo in registro.Campos)
            {
                if (campo.Pagina <= 0)
                {
                    yield return $"fonte ausente: Registro {registro.Codigo}, campo {campo.Nome}";
                }

                if (campo.Tipo is not ("C" or "N" or "NS" or "D"))
                {
                    yield return $"tipo desconhecido: Registro {registro.Codigo}, campo {campo.Nome}";
                }
            }
        }
    }
}

/// <summary>O manual de onde o catálogo foi gerado.</summary>
internal sealed record FonteDoCatalogo(string Documento, string Versao, string Publicacao);

/// <summary>
/// Um registro do leiaute. <see cref="OcorrenciaVerificada"/> e <see cref="HierarquiaVerificada"/> ficam falsos onde o
/// próprio manual diverge entre o cabeçalho do registro e a Tabela de Registros (REQ-ESTRUT-011).
/// </summary>
internal sealed record RegistroDoCatalogo(
    string Codigo,
    string Nome,
    string Bloco,
    int Nivel,
    int? NivelNaTabela,
    string Ocorrencia,
    string OcorrenciaNaTabela,
    int? OcorrenciaMaxima,
    bool OcorrenciaVerificada,
    string Entrada,
    string Saida,
    IReadOnlyList<string> Chave,
    IReadOnlyList<string> Regras,
    int Pagina,
    IReadOnlyList<CampoDoCatalogo> Campos,
    bool HierarquiaVerificada,
    string? Pai)
{
    /// <summary>Obrigatório na importação e na geração para transmissão (Tabela de Registros, p. 43 a 58).</summary>
    public bool ObrigatorioNaEntradaENaSaida => Entrada == "O" && Saida == "O";

    public bool TemRegra(string codigo) => Regras.Contains(codigo, StringComparer.Ordinal);
}

/// <summary>Um campo da tabela do registro, como o manual o define, e as verificações que o catálogo aplica.</summary>
internal sealed record CampoDoCatalogo(
    int Numero,
    string Nome,
    string Tipo,
    string Tamanho,
    string Decimal,
    string ValoresValidos,
    string Obrigatorio,
    int Pagina,
    VerificacoesDoCampo Verificacoes);

/// <summary>
/// Verificações do campo (mudança 08, D2). Nulo é sem verificação: tamanho sem limite, decimais não indicados, valores que
/// dependem de tabela externa ou obrigatoriedade condicional.
/// </summary>
internal sealed record VerificacoesDoCampo(bool Tipo, int? TamanhoMaximo, int? DecimaisMaximos, bool Data, ValoresDoCampo? Valores, bool? Obrigatorio);

/// <summary>
/// Valores válidos da coluna do manual: uma lista de valores literais ou um intervalo numérico (p. 37). <see cref="PorPosicao"/>
/// é a lista de um campo preenchido posição a posição, como "no formato XXXX" (p. 73 e 74): vale para cada caractere, e o
/// campo tem exatamente o tamanho da coluna.
/// </summary>
internal sealed record ValoresDoCampo(IReadOnlyList<string>? Lista, IReadOnlyList<string>? Intervalo, IReadOnlyList<string>? PorPosicao = null);
