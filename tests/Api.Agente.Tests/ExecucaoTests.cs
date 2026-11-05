using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Tests;

public sealed class ExecucaoTests
{
    private static Execucao Nova() => Execucao.Criar(
        Guid.NewGuid(), Guid.NewGuid(), new IdentidadeDeclarada("pseudonimo", "org-a"), DateTimeOffset.UtcNow);

    /// <summary>Todas as validações obrigatórias concluídas, com a etapa 6 não aplicável.</summary>
    private static Execucao Completa()
    {
        var execucao = Nova();
        foreach (var etapa in FluxoDeEtapas.Obrigatorias.Where(etapa => etapa != Etapa.FerramentasExternas))
        {
            execucao.ConcluirEtapa(etapa);
        }

        execucao.MarcarNaoAplicavel(Etapa.FerramentasExternas, "Nenhuma ferramenta externa é necessária nesta análise.");
        return execucao;
    }

    [Fact(DisplayName = "REQ-EXEC-001 Execução nasce com identificador do sistema e as nove etapas não executadas")]
    public void ExecucaoCriada()
    {
        var execucao = Nova();

        Assert.NotEqual(Guid.Empty, execucao.Id);
        Assert.Equal(9, execucao.Etapas.Count);
        Assert.All(execucao.Etapas, etapa => Assert.Equal(EstadoDaEtapa.NaoExecutada, etapa.Estado));
    }

    [Fact(DisplayName = "REQ-EXEC-002 Falha na etapa 3 deixa as etapas 4 a 8 como não executadas")]
    public void FalhaNaIdentificacao()
    {
        var execucao = Completa();

        execucao.FalharEtapa(Etapa.IdentificacaoDoLeiaute);

        Assert.Equal(EstadoDaEtapa.ComFalha, execucao.Etapas.Single(etapa => etapa.Etapa == Etapa.IdentificacaoDoLeiaute).Estado);
        Assert.All(
            execucao.Etapas.Where(etapa => etapa.Etapa is >= Etapa.ValidacaoEstrutural and <= Etapa.ValidacaoDaSaida),
            etapa => Assert.Equal(EstadoDaEtapa.NaoExecutada, etapa.Estado));
    }

    [Fact(DisplayName = "REQ-EXEC-003 Etapa 6 não aplicável registra a justificativa prevista")]
    public void EtapaNaoAplicavel()
    {
        var execucao = Nova();

        execucao.MarcarNaoAplicavel(Etapa.FerramentasExternas, "Nenhuma ferramenta externa é necessária nesta análise.");

        var etapa = execucao.Etapas.Single(item => item.Etapa == Etapa.FerramentasExternas);
        Assert.Equal(EstadoDaEtapa.NaoAplicavel, etapa.Estado);
        Assert.Equal("Nenhuma ferramenta externa é necessária nesta análise.", etapa.Justificativa);
    }

    [Theory(DisplayName = "REQ-EXEC-003 Etapa não aplicável sem justificativa prevista é recusada")]
    [InlineData(nameof(Etapa.FerramentasExternas), "Porque sim.")]
    [InlineData(nameof(Etapa.ValidacaoEstrutural), "Nenhuma ferramenta externa é necessária nesta análise.")]
    public void JustificativaNaoPrevista(string etapa, string justificativa) =>
        Assert.Throws<ExcecaoDeDominio>(() => Nova().MarcarNaoAplicavel(Enum.Parse<Etapa>(etapa), justificativa));

    [Fact(DisplayName = "REQ-EXEC-004 Etapa 2 rejeitada dá status de entrada rejeitada")]
    public void EntradaRejeitada()
    {
        var execucao = Nova();

        execucao.FalharEtapa(Etapa.ValidacaoDaEntrada);

        Assert.Equal(StatusDaExecucao.EntradaRejeitada, execucao.Status);
    }

    [Fact(DisplayName = "REQ-EXEC-004 Etapa com falha técnica dá status de falha técnica")]
    public void FalhaTecnicaEmEtapa()
    {
        var execucao = Completa();

        execucao.FalharEtapa(Etapa.ConsultaAoManual);

        Assert.Equal(StatusDaExecucao.FalhaTecnica, execucao.Status);
    }

    [Fact(DisplayName = "REQ-EXEC-004 Achado de falha técnica dá status de falha técnica")]
    public void FalhaTecnicaEmAchado()
    {
        var execucao = Completa();

        execucao.AdicionarAchado(Achado.Criar("ach-1", CategoriaDoAchado.FalhaTecnica, OrigemDoAchado.Sistema, "SQ-06", "Provedor indisponível.", SeveridadeDoAchado.Alta));

        Assert.Equal(StatusDaExecucao.FalhaTecnica, execucao.Status);
    }

    [Fact(DisplayName = "REQ-EXEC-004 Validação obrigatória não executada nunca dá status concluída")]
    public void ValidacaoNaoConcluida()
    {
        var execucao = Nova();
        execucao.ConcluirEtapa(Etapa.Recepcao);
        execucao.ConcluirEtapa(Etapa.ValidacaoDaEntrada);

        Assert.Equal(StatusDaExecucao.Parcial, execucao.Status);
    }

    [Fact(DisplayName = "REQ-EXEC-004 Limitação declarada dá status parcial")]
    public void ComLimitacao()
    {
        var execucao = Completa();

        execucao.AdicionarLimitacao("Manual da versão indisponível.");

        Assert.Equal(StatusDaExecucao.Parcial, execucao.Status);
    }

    [Fact(DisplayName = "REQ-EXEC-004 Todas as validações obrigatórias concluídas dão status concluída, mesmo com erro encontrado")]
    public void Concluida()
    {
        var execucao = Completa();

        execucao.AdicionarAchado(Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra de teste", "Campo ausente.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "R1", 3, "CAMPO")]));

        Assert.Equal(StatusDaExecucao.Concluida, execucao.Status);
        Assert.False(execucao.ExigeRevisaoHumana);
    }

    [Fact(DisplayName = "REQ-RESULT-005 Validação obrigatória não concluída exige revisão humana")]
    public void RevisaoComValidacaoPendente() => Assert.True(Nova().ExigeRevisaoHumana);

    [Fact(DisplayName = "REQ-RESULT-005 Falha técnica exige revisão humana")]
    public void RevisaoComFalhaTecnica()
    {
        var execucao = Completa();

        execucao.AdicionarAchado(Achado.Criar("ach-1", CategoriaDoAchado.FalhaTecnica, OrigemDoAchado.Sistema, "SQ-06", "Provedor indisponível.", SeveridadeDoAchado.Alta));

        Assert.True(execucao.ExigeRevisaoHumana);
    }

    [Fact(DisplayName = "REQ-RESULT-005 Indício da IA exige revisão humana")]
    public void RevisaoComIndicio()
    {
        var execucao = Completa();

        execucao.AdicionarAchado(Achado.Criar(
            "ach-1", CategoriaDoAchado.IndicioDaIa, OrigemDoAchado.Llm, "Trecho do manual", "Possível divergência.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "R1")]));

        Assert.True(execucao.ExigeRevisaoHumana);
    }

    [Fact(DisplayName = "REQ-RESULT-005 Ausência de evidências exige revisão humana")]
    public void RevisaoComAusenciaDeEvidencias()
    {
        var execucao = Completa();

        execucao.AdicionarAchado(Achado.Criar("ach-1", CategoriaDoAchado.AusenciaDeEvidencias, OrigemDoAchado.Sistema, "P08", "Manual indisponível.", SeveridadeDoAchado.Alta));

        Assert.True(execucao.ExigeRevisaoHumana);
    }

    [Fact(DisplayName = "REQ-RESULT-005 Recomendação do modelo para dispensar a revisão é ignorada")]
    public void RecomendacaoDeDispensaIgnorada()
    {
        var execucao = Completa();

        execucao.AdicionarAchado(Achado.Criar(
            "ach-1", CategoriaDoAchado.AusenciaDeEvidencias, OrigemDoAchado.Llm, "Trecho do manual", "O modelo recomenda dispensar a revisão humana.", SeveridadeDoAchado.Media));

        Assert.True(execucao.ExigeRevisaoHumana);
        Assert.False(typeof(Execucao).GetProperty(nameof(Execucao.ExigeRevisaoHumana))!.CanWrite);
    }

    [Fact(DisplayName = "REQ-EXEC-004 Entrada rejeitada prevalece sobre falha técnica")]
    public void EntradaRejeitadaPrevalece()
    {
        var execucao = Nova();

        execucao.FalharEtapa(Etapa.ValidacaoDaEntrada);
        execucao.AdicionarAchado(Achado.Criar("ach-1", CategoriaDoAchado.FalhaTecnica, OrigemDoAchado.Sistema, "SQ-06", "Provedor indisponível.", SeveridadeDoAchado.Alta));

        Assert.Equal(StatusDaExecucao.EntradaRejeitada, execucao.Status);
    }

    [Fact(DisplayName = "REQ-RESULT-002 Indício com origem validador é rejeitado")]
    public void IndicioComOrigemValidador() =>
        Assert.Throws<ExcecaoDeDominio>(() => Achado.Criar(
            "ach-1", CategoriaDoAchado.IndicioDaIa, OrigemDoAchado.Validador, "Regra", "Descrição.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "R1")]));

    [Fact(DisplayName = "REQ-RESULT-002 Erro estrutural vindo do LLM é rejeitado")]
    public void ErroEstruturalDoLlm() =>
        Assert.Throws<ExcecaoDeDominio>(() => Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Llm, "Regra", "Descrição.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "R1")]));

    [Fact(DisplayName = "REQ-RESULT-003 Achado sem fundamento é rejeitado")]
    public void AchadoSemFundamento() =>
        Assert.Throws<ExcecaoDeDominio>(() => Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, " ", "Descrição.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "R1")]));

    [Fact(DisplayName = "REQ-RESULT-003 Erro estrutural sem registros é rejeitado")]
    public void AchadoSemRegistros() =>
        Assert.Throws<ExcecaoDeDominio>(() => Achado.Criar("ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra", "Descrição.", SeveridadeDoAchado.Alta));

    [Fact(DisplayName = "REQ-RESULT-004 Severidade fora da escala é rejeitada")]
    public void SeveridadeForaDaEscala() =>
        Assert.Throws<ExcecaoDeDominio>(() => Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra", "Descrição.", (SeveridadeDoAchado)99, [new RegistroEnvolvido("doc-1", "R1")]));
}
