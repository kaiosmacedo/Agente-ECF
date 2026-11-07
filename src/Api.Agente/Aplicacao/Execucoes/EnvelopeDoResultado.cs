using System.Globalization;
using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Aplicacao.Execucoes;

/// <summary>
/// Envelope versionado do resultado, preenchido só pelo sistema (REQ-RESULT-001, GD-02). É o que fica guardado
/// e o que <c>GET /analises/{id}</c> devolve; <c>ResultadoAnaliseSofia</c> é uma projeção dele.
/// </summary>
internal sealed record EnvelopeDoResultado(
    string VersaoContrato,
    Guid IdExecucao,
    Guid IdExecucaoSofia,
    Guid IdSolicitacao,
    string? IdCorrelacao,
    string Status,
    string Veredito,
    bool RevisaoHumana,
    ResumoDosAchados Resumo,
    string Sintese,
    IReadOnlyList<EtapaDoEnvelope> Etapas,
    IReadOnlyList<AchadoDoEnvelope> Achados,
    IReadOnlyList<string> Limitacoes,
    IReadOnlyList<ItemNaoVerificado> NaoVerificados,
    IReadOnlyList<DocumentoRecebido> Documentos,
    IdentidadeDoEnvelope Identidade,
    VersoesDoEnvelope Versoes,
    DateTimeOffset CriadaEm,
    DateTimeOffset? ConcluidaEm,
    double DuracaoMs,
    ErroDoEnvelope? Erro);

internal sealed record EtapaDoEnvelope(int Numero, string Nome, string Estado, string? Justificativa);

internal sealed record AchadoDoEnvelope(
    string IdAchado,
    string Categoria,
    string Origem,
    string Fundamento,
    string Descricao,
    IReadOnlyList<RegistroEnvolvido> Registros,
    IReadOnlyList<string> Evidencias,
    string Severidade,
    string Classe,
    RegraDoEnvelope? Regra,
    FonteDoAchado? Fonte,
    string? Orientacao);

/// <summary>Quantos achados há em cada classe (REQ-RESULT-011).</summary>
internal sealed record ResumoDosAchados(int Impeditivas, int Graves, int Observacoes);

/// <summary>A regra do manual, com o tipo pelo nome usado no manual. O nível só vem se o manual o declarar (REQ-RESULT-008).</summary>
internal sealed record RegraDoEnvelope(string Codigo, string Tipo, string? Nivel);

/// <summary>O que o arquivo não prova, com o motivo. Não muda o status nem o veredito (REQ-RESULT-013).</summary>
internal sealed record ItemNaoVerificado(string Item, string Motivo);

/// <summary>Identidade como veio declarada: nesta versão, nunca é verificada (SQ-12).</summary>
internal sealed record IdentidadeDoEnvelope(string UsuarioPseudonimo, string Organizacao, bool Verificada);

/// <summary>Versões usadas na execução, para explicar o resultado depois (P19, REQ-EXEC-006).</summary>
internal sealed record VersoesDoEnvelope(string Contrato, string Servico, string Validadores);

internal sealed record ErroDoEnvelope(string Codigo, string Mensagem);

/// <summary>Monta o envelope a partir da execução, com os nomes públicos de status, categorias e severidades.</summary>
internal static class MontadorDoEnvelope
{
    public const string VersaoDoContrato = "1.0";

    /// <summary>O que nenhum arquivo prova sozinho (docs/classificacao-das-regras.md; decisão 4 da pesquisa).</summary>
    private static readonly ItemNaoVerificado[] NaoVerificadosSempre =
    [
        new("Confronto do IRPJ e da CSLL com DCTF, DCOMP e pagamentos", "Depende de dados da Receita Federal ou de relatórios que não vêm no arquivo."),
        new("Certificado e procuração, ECD e ECF anterior na base, hash na base, cadastro do CNPJ, versão do programa e recepção", "São conferidos na transmissão ou na base da Receita Federal."),
        new("Pagamento efetivo, PGFN, parcelamento, intimação, aviso de malha, autos de infração e Sintonia", "Estão em sistemas da Receita Federal, com acesso autenticado."),
        new("e-Financeira, EFD-Reinf e eSocial de terceiros, DIRF de fontes pagadoras e Siscomex", "São dados prestados por terceiros."),
        new("Documentos que comprovam prejuízo fiscal e base negativa, e guarda de documentos", "Estão fora da ECF."),
        new("REGRA_DV_RECIBO, REGRA_VALIDA_HASH_ARQUIVO, regras das tabelas dinâmicas fora do manual e fórmulas com funções sem definição", "Não há algoritmo ou fonte acessível, e adotar um violaria P01."),
    ];

    /// <summary>Nenhum validador existe ainda: entram nas mudanças 08 e 09.</summary>
    private const string VersaoDosValidadores = "nenhum";

    private static readonly Dictionary<Etapa, string> NomeDaEtapa = new()
    {
        [Etapa.Recepcao] = "Recepção",
        [Etapa.ValidacaoDaEntrada] = "Validação da entrada",
        [Etapa.IdentificacaoDoLeiaute] = "Identificação do leiaute",
        [Etapa.ValidacaoEstrutural] = "Validação estrutural",
        [Etapa.ConsultaAoManual] = "Consulta ao manual",
        [Etapa.FerramentasExternas] = "Ferramentas externas",
        [Etapa.AnaliseComLlm] = "Análise com LLM",
        [Etapa.ValidacaoDaSaida] = "Validação da saída",
        [Etapa.RetornoDoResultado] = "Retorno do resultado",
    };

    public static EnvelopeDoResultado Montar(
        Execucao execucao, string? correlacao, string versaoDoServico, string sintese, ErroDoEnvelope? erro) =>
        new(
            VersaoDoContrato,
            execucao.Id,
            execucao.IdExecucaoSofia,
            execucao.IdSolicitacao,
            correlacao,
            NomeDoStatus(execucao.Status),
            NomeDoVeredito(execucao.Veredito),
            execucao.ExigeRevisaoHumana,
            Resumo(execucao),
            $"{TextoDoVeredito(execucao)} {sintese}",
            [.. execucao.Etapas.Select(etapa => new EtapaDoEnvelope((int)etapa.Etapa, NomeDaEtapa[etapa.Etapa], NomeDoEstado(etapa.Estado), etapa.Justificativa))],
            [.. execucao.Achados.Select(Achado)],
            [.. ValidacoesNaoRealizadas(execucao), .. execucao.Limitacoes],
            execucao.Veredito == VereditoDaExecucao.NaoAvaliado ? [] : NaoVerificadosSempre,
            execucao.Documentos,
            new IdentidadeDoEnvelope(execucao.Identidade.UsuarioPseudonimo, execucao.Identidade.Organizacao, Verificada: false),
            new VersoesDoEnvelope(VersaoDoContrato, versaoDoServico, VersaoDosValidadores),
            execucao.CriadaEm,
            execucao.ConcluidaEm,
            execucao.ConcluidaEm is { } fim ? (fim - execucao.CriadaEm).TotalMilliseconds : 0,
            erro);

    public static string NomeDoStatus(StatusDaExecucao status) => status switch
    {
        StatusDaExecucao.EntradaRejeitada => "entrada-rejeitada",
        StatusDaExecucao.Concluida => "concluida",
        StatusDaExecucao.Parcial => "parcial",
        StatusDaExecucao.FalhaTecnica => "falha-tecnica",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    public static string NomeDoVeredito(VereditoDaExecucao veredito) => veredito switch
    {
        VereditoDaExecucao.Conforme => "conforme",
        VereditoDaExecucao.ConformeComRessalvas => "conforme-com-ressalvas",
        VereditoDaExecucao.Recusado => "recusado",
        VereditoDaExecucao.Parcial => "parcial",
        VereditoDaExecucao.NaoAvaliado => "nao-avaliado",
        _ => throw new ArgumentOutOfRangeException(nameof(veredito)),
    };

    private static ResumoDosAchados Resumo(Execucao execucao) => new(
        execucao.Achados.Count(achado => achado.Classe == ClasseDoAchado.Impeditiva),
        execucao.Achados.Count(achado => achado.Classe == ClasseDoAchado.Grave),
        execucao.Achados.Count(achado => achado.Classe == ClasseDoAchado.Observacao));

    /// <summary>O que o veredito significa. Vale para as validações realizadas e é apoio à conferência (seção 1.3).</summary>
    private static string TextoDoVeredito(Execucao execucao)
    {
        const string Apoio = "O resultado vale para as validações realizadas e é apoio à conferência, sem substituir o programa oficial da ECF.";
        var resumo = Resumo(execucao);
        return execucao.Veredito switch
        {
            VereditoDaExecucao.Conforme => $"De acordo e verificada. {Apoio}",
            VereditoDaExecucao.ConformeComRessalvas => string.Create(CultureInfo.InvariantCulture, $"De acordo, com ressalvas: nenhuma regra impeditiva violada, {resumo.Graves} ponto(s) grave(s) e {resumo.Observacoes} em observação. {Apoio}"),
            VereditoDaExecucao.Recusado => string.Create(CultureInfo.InvariantCulture, $"Recusada na avaliação: {resumo.Impeditivas} regra(s) impeditiva(s) violada(s). {Apoio}"),
            VereditoDaExecucao.Parcial => "Avaliação parcial: nem todas as validações foram realizadas.",
            _ => "Não foi possível avaliar.",
        };
    }

    /// <summary>Cada validação obrigatória não realizada, com o motivo (REQ-RESULT-006).</summary>
    private static IEnumerable<string> ValidacoesNaoRealizadas(Execucao execucao)
    {
        var etapas = execucao.Etapas.ToDictionary(etapa => etapa.Etapa);
        var falhou = execucao.Etapas.Any(etapa => etapa.Estado == EstadoDaEtapa.ComFalha);
        foreach (var obrigatoria in FluxoDeEtapas.Obrigatorias.Where(etapa => etapas[etapa].Estado == EstadoDaEtapa.NaoExecutada))
        {
            var motivo = falhou ? "uma etapa anterior falhou" : "ainda não implementada nesta versão";
            yield return $"Etapa não realizada: {NomeDaEtapa[obrigatoria]}. Motivo: {motivo}.";
        }
    }

    private static string NomeDoTipo(TipoDaRegra tipo) => tipo switch
    {
        TipoDaRegra.Erro => "Erro",
        TipoDaRegra.Aviso => "Aviso",
        TipoDaRegra.NaoDeclarado => "Não declarado",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
    };

    private static string NomeDoEstado(EstadoDaEtapa estado) => estado switch
    {
        EstadoDaEtapa.NaoExecutada => "nao-executada",
        EstadoDaEtapa.Concluida => "concluida",
        EstadoDaEtapa.NaoAplicavel => "nao-aplicavel",
        EstadoDaEtapa.ComFalha => "com-falha",
        _ => throw new ArgumentOutOfRangeException(nameof(estado)),
    };

    private static AchadoDoEnvelope Achado(Achado achado) => new(
        achado.IdAchado,
        achado.Categoria switch
        {
            CategoriaDoAchado.ErroEstrutural => "erro-estrutural",
            CategoriaDoAchado.InconsistenciaDeNegocio => "inconsistencia-de-negocio",
            CategoriaDoAchado.IndicioDaIa => "indicio-da-ia",
            CategoriaDoAchado.AusenciaDeEvidencias => "ausencia-de-evidencias",
            CategoriaDoAchado.FalhaTecnica => "falha-tecnica",
            _ => throw new ArgumentOutOfRangeException(nameof(achado)),
        },
        achado.Origem switch
        {
            OrigemDoAchado.Validador => "validador",
            OrigemDoAchado.Ferramenta => "ferramenta",
            OrigemDoAchado.Llm => "llm",
            OrigemDoAchado.Sistema => "sistema",
            _ => throw new ArgumentOutOfRangeException(nameof(achado)),
        },
        achado.Fundamento,
        achado.Descricao,
        achado.Registros,
        achado.Evidencias,
        achado.Severidade switch
        {
            SeveridadeDoAchado.Baixa => "baixa",
            SeveridadeDoAchado.Media => "media",
            SeveridadeDoAchado.Alta => "alta",
            SeveridadeDoAchado.Critica => "critica",
            _ => throw new ExcecaoDeDominio("Severidade fora da escala."),
        },
        achado.Classe switch
        {
            ClasseDoAchado.Impeditiva => "impeditiva",
            ClasseDoAchado.Grave => "grave",
            _ => "observacao",
        },
        achado.Regra is { } regra ? new RegraDoEnvelope(regra.Codigo, NomeDoTipo(regra.Tipo), regra.Nivel) : null,
        achado.Fonte,
        achado.Orientacao);
}
