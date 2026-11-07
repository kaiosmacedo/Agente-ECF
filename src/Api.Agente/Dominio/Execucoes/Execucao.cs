namespace Api.Agente.Dominio.Execucoes;

/// <summary>Status da execução, calculado só pelo sistema (P05, GD-03). <c>Concluida</c> não significa "conforme".</summary>
internal enum StatusDaExecucao
{
    EntradaRejeitada,
    Concluida,
    Parcial,
    FalhaTecnica,
}

/// <summary>Como a ECF está, calculado só pelo sistema, separado do status (REQ-RESULT-010; mudança 15, D1).</summary>
internal enum VereditoDaExecucao
{
    Conforme,
    ConformeComRessalvas,
    Recusado,
    Parcial,
    NaoAvaliado,
}

/// <summary>Hash e tamanho de um documento recebido; o texto nunca é guardado (GD-06, P19).</summary>
internal sealed record DocumentoRecebido(string IdDocumento, string Hash, int Caracteres);

/// <summary>Referência de um trecho do manual recebido na etapa 5, sem o texto (REQ-CONSULTA-002, 006).</summary>
internal sealed record TrechoConsultado(Guid IdTrecho, Guid IdDocumento, string Versao, int Pagina, double Relevancia);

/// <summary>Quem pediu a análise, como veio declarado (SQ-12). O nome não é guardado, só o pseudônimo.</summary>
internal sealed record IdentidadeDeclarada(string UsuarioPseudonimo, string Organizacao);

/// <summary>Uma análise, com as nove etapas, achados e limitações (REQ-EXEC-001 a 004, REQ-RESULT-005).</summary>
internal sealed class Execucao
{
    private readonly Dictionary<Etapa, EtapaDaExecucao> etapas;
    private readonly List<Achado> achados = [];
    private readonly List<string> limitacoes = [];
    private readonly List<DocumentoRecebido> documentos = [];
    private readonly Dictionary<string, IReadOnlyList<TrechoConsultado>> trechos = new(StringComparer.Ordinal);

    private Execucao(Guid id, Guid idExecucaoSofia, Guid idSolicitacao, IdentidadeDeclarada identidade, DateTimeOffset criadaEm)
    {
        Id = id;
        IdExecucaoSofia = idExecucaoSofia;
        IdSolicitacao = idSolicitacao;
        Identidade = identidade;
        CriadaEm = criadaEm;
        etapas = Enum.GetValues<Etapa>().ToDictionary(etapa => etapa, etapa => new EtapaDaExecucao(etapa, EstadoDaEtapa.NaoExecutada));
    }

    public Guid Id { get; }

    public Guid IdExecucaoSofia { get; }

    public Guid IdSolicitacao { get; }

    public IdentidadeDeclarada Identidade { get; }

    public DateTimeOffset CriadaEm { get; }

    public DateTimeOffset? ConcluidaEm { get; private set; }

    public IReadOnlyList<EtapaDaExecucao> Etapas => [.. etapas.Values.OrderBy(etapa => etapa.Etapa)];

    public IReadOnlyList<Achado> Achados => achados;

    public IReadOnlyList<string> Limitacoes => limitacoes;

    public IReadOnlyList<DocumentoRecebido> Documentos => documentos;

    /// <summary>Trechos recebidos por regra consultada, na chave de <see cref="Achado.ChaveDaRegra"/>.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<TrechoConsultado>> TrechosConsultados => trechos;

    /// <summary>Regra da tabela D2 do plano da mudança 03, nesta ordem.</summary>
    public StatusDaExecucao Status
    {
        get
        {
            if (etapas[Etapa.ValidacaoDaEntrada].Estado == EstadoDaEtapa.ComFalha)
            {
                return StatusDaExecucao.EntradaRejeitada;
            }

            if (HaFalhaTecnica)
            {
                return StatusDaExecucao.FalhaTecnica;
            }

            return HaObrigatoriaNaoConcluida || limitacoes.Count > 0 ? StatusDaExecucao.Parcial : StatusDaExecucao.Concluida;
        }
    }

    /// <summary>
    /// Regra da tabela D1 do plano da mudança 15, nesta ordem. Um erro impeditivo já prova que a ECF não está correta,
    /// mesmo com validações pendentes; o contrário não vale: "conforme" exige o status concluída (P05).
    /// </summary>
    public VereditoDaExecucao Veredito
    {
        get
        {
            if (achados.Any(achado => achado.Classe == ClasseDoAchado.Impeditiva))
            {
                return VereditoDaExecucao.Recusado;
            }

            return Status switch
            {
                StatusDaExecucao.EntradaRejeitada or StatusDaExecucao.FalhaTecnica => VereditoDaExecucao.NaoAvaliado,
                StatusDaExecucao.Parcial => VereditoDaExecucao.Parcial,
                _ => achados.Count > 0 ? VereditoDaExecucao.ConformeComRessalvas : VereditoDaExecucao.Conforme,
            };
        }
    }

    /// <summary>Calculada pelo sistema; nenhuma recomendação do modelo a dispensa (P09).</summary>
    public bool ExigeRevisaoHumana =>
        HaFalhaTecnica
        || HaObrigatoriaNaoConcluida
        || achados.Any(achado => achado.Categoria is CategoriaDoAchado.IndicioDaIa or CategoriaDoAchado.AusenciaDeEvidencias
            || achado.Classe == ClasseDoAchado.Grave);

    private bool HaFalhaTecnica =>
        etapas.Values.Any(etapa => etapa.Estado == EstadoDaEtapa.ComFalha)
        || achados.Any(achado => achado.Categoria == CategoriaDoAchado.FalhaTecnica);

    private bool HaObrigatoriaNaoConcluida =>
        FluxoDeEtapas.Obrigatorias.Any(etapa => etapas[etapa].Estado is not (EstadoDaEtapa.Concluida or EstadoDaEtapa.NaoAplicavel));

    public static Execucao Criar(Guid idExecucaoSofia, Guid idSolicitacao, IdentidadeDeclarada identidade, DateTimeOffset criadaEm) =>
        new(Guid.NewGuid(), idExecucaoSofia, idSolicitacao, identidade, criadaEm);

    public void ConcluirEtapa(Etapa etapa) => etapas[etapa] = new EtapaDaExecucao(etapa, EstadoDaEtapa.Concluida);

    /// <summary>Registra a falha e deixa as etapas dependentes como não executadas (REQ-EXEC-002).</summary>
    public void FalharEtapa(Etapa etapa)
    {
        etapas[etapa] = new EtapaDaExecucao(etapa, EstadoDaEtapa.ComFalha);
        foreach (var dependente in FluxoDeEtapas.DependentesDe(etapa))
        {
            etapas[dependente] = new EtapaDaExecucao(dependente, EstadoDaEtapa.NaoExecutada);
        }
    }

    /// <summary>Só com justificativa prevista na especificação (REQ-EXEC-003).</summary>
    public void MarcarNaoAplicavel(Etapa etapa, string justificativa)
    {
        if (!FluxoDeEtapas.JustificativaPrevista(etapa, justificativa))
        {
            throw new ExcecaoDeDominio($"A etapa {etapa} não admite essa justificativa para não se aplicar.");
        }

        etapas[etapa] = new EtapaDaExecucao(etapa, EstadoDaEtapa.NaoAplicavel, justificativa);
    }

    public void AdicionarAchado(Achado achado) => achados.Add(achado);

    public void AdicionarLimitacao(string limitacao) => limitacoes.Add(limitacao);

    public void RegistrarDocumento(DocumentoRecebido documento) => documentos.Add(documento);

    public void RegistrarTrechos(string chaveDaRegra, IReadOnlyList<TrechoConsultado> recebidos) => trechos[chaveDaRegra] = recebidos;

    public void Concluir(DateTimeOffset concluidaEm)
    {
        ConcluirEtapa(Etapa.RetornoDoResultado);
        ConcluidaEm = concluidaEm;
    }
}
