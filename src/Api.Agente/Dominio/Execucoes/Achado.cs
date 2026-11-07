namespace Api.Agente.Dominio.Execucoes;

/// <summary>As cinco categorias de resultado (P04, GD-04).</summary>
internal enum CategoriaDoAchado
{
    ErroEstrutural,
    InconsistenciaDeNegocio,
    IndicioDaIa,
    AusenciaDeEvidencias,
    FalhaTecnica,
}

/// <summary>Quem produziu o achado (GD-04).</summary>
internal enum OrigemDoAchado
{
    Validador,
    Ferramenta,
    Llm,
    Sistema,
}

/// <summary>Escala de severidade, a mesma do contrato SOFIA v1.0 (DP-07; design da mudança 03, D3).</summary>
internal enum SeveridadeDoAchado
{
    Baixa,
    Media,
    Alta,
    Critica,
}

/// <summary>Classe do achado, calculada pela severidade (REQ-RESULT-007; mudança 15, D2).</summary>
internal enum ClasseDoAchado
{
    Impeditiva,
    Grave,
    Observacao,
}

/// <summary>
/// Tipo da regra: Erro e Aviso são os do manual (leiaute 12, capítulo 3, página 38). Não declarado é para as
/// instâncias em que o manual não traz o tipo, que nunca são impeditivas (docs/classificacao-das-regras.md).
/// </summary>
internal enum TipoDaRegra
{
    Erro,
    Aviso,
    NaoDeclarado,
}

/// <summary>A regra do manual que produziu o achado. O nível só vem quando o manual o declara (REQ-RESULT-008).</summary>
internal sealed record RegraDoManual(string Codigo, TipoDaRegra Tipo, string? Nivel = null);

/// <summary>De onde vem a regra: documento, versão e localização (P06, P01).</summary>
internal sealed record FonteDoAchado(string Documento, string Versao, string Localizacao);

/// <summary>Registro envolvido em um achado, com linha, campo e valor quando disponíveis (P06, REQ-RESULT-011).</summary>
internal sealed record RegistroEnvolvido(string IdDocumento, string Registro, int? Linha = null, string? Campo = null, string? ValorEncontrado = null);

/// <summary>Regra de domínio violada ao criar um achado ou mudar uma execução.</summary>
internal sealed class ExcecaoDeDominio(string mensagem) : Exception(mensagem);

/// <summary>Um problema apontado na análise, com categoria, origem e fundamento (REQ-RESULT-002 a 004).</summary>
internal sealed record Achado
{
    private static readonly Dictionary<CategoriaDoAchado, OrigemDoAchado[]> OrigensPermitidas = new()
    {
        [CategoriaDoAchado.ErroEstrutural] = [OrigemDoAchado.Validador],
        [CategoriaDoAchado.InconsistenciaDeNegocio] = [OrigemDoAchado.Validador, OrigemDoAchado.Ferramenta],
        [CategoriaDoAchado.IndicioDaIa] = [OrigemDoAchado.Llm],
        [CategoriaDoAchado.AusenciaDeEvidencias] = [OrigemDoAchado.Sistema, OrigemDoAchado.Llm],
        [CategoriaDoAchado.FalhaTecnica] = [OrigemDoAchado.Sistema],
    };

    // Falha técnica e ausência de evidências podem não ter registro a citar: justamente não houve como verificar.
    private static readonly CategoriaDoAchado[] ExigemRegistros =
        [CategoriaDoAchado.ErroEstrutural, CategoriaDoAchado.InconsistenciaDeNegocio, CategoriaDoAchado.IndicioDaIa];

    private const int TamanhoMaximoDoValor = 200;
    private const string IndicacaoDeCorte = " [cortado]";

    private static readonly string[] NiveisDoManual = ["1.1", "1.2", "2", "3"];

    private Achado()
    {
    }

    public required string IdAchado { get; init; }

    public required CategoriaDoAchado Categoria { get; init; }

    public required OrigemDoAchado Origem { get; init; }

    public required string Fundamento { get; init; }

    public required string Descricao { get; init; }

    public required IReadOnlyList<RegistroEnvolvido> Registros { get; init; }

    public required IReadOnlyList<string> Evidencias { get; init; }

    public required SeveridadeDoAchado Severidade { get; init; }

    public RegraDoManual? Regra { get; init; }

    public FonteDoAchado? Fonte { get; init; }

    public string? Orientacao { get; init; }

    public ClasseDoAchado Classe => Severidade switch
    {
        SeveridadeDoAchado.Critica => ClasseDoAchado.Impeditiva,
        SeveridadeDoAchado.Alta => ClasseDoAchado.Grave,
        _ => ClasseDoAchado.Observacao,
    };

    /// <summary>Cria o achado ou recusa a combinação inválida, sem exceção silenciosa.</summary>
    public static Achado Criar(
        string idAchado,
        CategoriaDoAchado categoria,
        OrigemDoAchado origem,
        string fundamento,
        string descricao,
        SeveridadeDoAchado severidade,
        IReadOnlyList<RegistroEnvolvido>? registros = null,
        IReadOnlyList<string>? evidencias = null,
        RegraDoManual? regra = null,
        FonteDoAchado? fonte = null,
        string? orientacao = null)
    {
        if (!OrigensPermitidas[categoria].Contains(origem))
        {
            throw new ExcecaoDeDominio($"A categoria {categoria} não admite a origem {origem}.");
        }

        if (string.IsNullOrWhiteSpace(fundamento))
        {
            throw new ExcecaoDeDominio("O achado precisa citar a regra, o requisito ou a fonte que o fundamenta.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ExcecaoDeDominio("O achado precisa de uma descrição.");
        }

        if (ExigemRegistros.Contains(categoria) && registros is not { Count: > 0 })
        {
            throw new ExcecaoDeDominio($"A categoria {categoria} precisa citar os registros envolvidos.");
        }

        if (!Enum.IsDefined(severidade))
        {
            throw new ExcecaoDeDominio("Severidade fora da escala.");
        }

        // Só validador impede: o LLM não recusa uma ECF sozinho (REQ-RESULT-009; P02).
        if (severidade == SeveridadeDoAchado.Critica && categoria is not (CategoriaDoAchado.ErroEstrutural or CategoriaDoAchado.InconsistenciaDeNegocio))
        {
            throw new ExcecaoDeDominio($"A categoria {categoria} não pode ser impeditiva.");
        }

        // Sem regra do manual, só a violação de leiaute impede, porque não deixa determinar a estrutura.
        if (severidade == SeveridadeDoAchado.Critica && regra is null && categoria != CategoriaDoAchado.ErroEstrutural)
        {
            throw new ExcecaoDeDominio("Sem regra do tipo Erro, só a violação de leiaute pode ser impeditiva.");
        }

        ValidarRegra(regra, severidade, fonte, orientacao);

        return new Achado
        {
            IdAchado = idAchado,
            Categoria = categoria,
            Origem = origem,
            Fundamento = fundamento,
            Descricao = descricao,
            Registros = [.. (registros ?? []).Select(registro => registro with { ValorEncontrado = Cortar(registro.ValorEncontrado) })],
            Evidencias = evidencias ?? [],
            Severidade = severidade,
            Regra = regra,
            Fonte = fonte,
            Orientacao = orientacao,
        };
    }

    /// <summary>Tipo e nível como no manual; regra do manual sempre traz fonte e orientação (REQ-RESULT-008, 011).</summary>
    private static void ValidarRegra(RegraDoManual? regra, SeveridadeDoAchado severidade, FonteDoAchado? fonte, string? orientacao)
    {
        if (regra is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(regra.Codigo) || (regra.Nivel is not null && !NiveisDoManual.Contains(regra.Nivel)))
        {
            throw new ExcecaoDeDominio("A regra precisa do código, e o nível, quando houver, precisa ser um do manual.");
        }

        if (regra.Tipo == TipoDaRegra.Erro && severidade != SeveridadeDoAchado.Critica)
        {
            throw new ExcecaoDeDominio("Regra do tipo Erro impede a conclusão, então a severidade é crítica.");
        }

        if (regra.Tipo != TipoDaRegra.Erro && severidade == SeveridadeDoAchado.Critica)
        {
            throw new ExcecaoDeDominio("Só regra do tipo Erro pode ser crítica.");
        }

        if (fonte is null || string.IsNullOrWhiteSpace(orientacao))
        {
            throw new ExcecaoDeDominio("Achado de regra do manual precisa da fonte e da orientação de correção.");
        }
    }

    /// <summary>Só o valor do campo, com tamanho limitado; o corte fica indicado (mudança 15, D4).</summary>
    private static string? Cortar(string? valor) =>
        valor is { Length: > TamanhoMaximoDoValor } ? string.Concat(valor.AsSpan(0, TamanhoMaximoDoValor), IndicacaoDeCorte) : valor;
}
