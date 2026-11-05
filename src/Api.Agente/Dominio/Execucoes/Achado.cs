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

/// <summary>Registro envolvido em um achado, com linha e campo quando disponíveis (P06).</summary>
internal sealed record RegistroEnvolvido(string IdDocumento, string Registro, int? Linha = null, string? Campo = null);

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

    /// <summary>Cria o achado ou recusa a combinação inválida, sem exceção silenciosa.</summary>
    public static Achado Criar(
        string idAchado,
        CategoriaDoAchado categoria,
        OrigemDoAchado origem,
        string fundamento,
        string descricao,
        SeveridadeDoAchado severidade,
        IReadOnlyList<RegistroEnvolvido>? registros = null,
        IReadOnlyList<string>? evidencias = null)
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

        return new Achado
        {
            IdAchado = idAchado,
            Categoria = categoria,
            Origem = origem,
            Fundamento = fundamento,
            Descricao = descricao,
            Registros = registros ?? [],
            Evidencias = evidencias ?? [],
            Severidade = severidade,
        };
    }
}
