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
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra de teste", "Campo ausente.", SeveridadeDoAchado.Media, [new RegistroEnvolvido("doc-1", "R1", 3, "CAMPO")]));

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

    private static Achado DeRegra(TipoDaRegra tipo, SeveridadeDoAchado severidade, string? valor = null) => Achado.Criar(
        "ach-1",
        CategoriaDoAchado.ErroEstrutural,
        OrigemDoAchado.Validador,
        "REGRA_DE_TESTE",
        "Campo inválido.",
        severidade,
        [new RegistroEnvolvido("doc-1", "0000", 1, "DT_FIN", valor)],
        regra: new RegraDoManual("REGRA_DE_TESTE", tipo, "1.1"),
        fonte: new FonteDoAchado("Manual sintético", "12", "página 1"),
        orientacao: "Corrija o campo.");

    [Theory(DisplayName = "REQ-RESULT-007 Classe é calculada pela severidade")]
    [InlineData(nameof(SeveridadeDoAchado.Critica), nameof(ClasseDoAchado.Impeditiva))]
    [InlineData(nameof(SeveridadeDoAchado.Alta), nameof(ClasseDoAchado.Grave))]
    [InlineData(nameof(SeveridadeDoAchado.Media), nameof(ClasseDoAchado.Observacao))]
    [InlineData(nameof(SeveridadeDoAchado.Baixa), nameof(ClasseDoAchado.Observacao))]
    public void ClassePelaSeveridade(string severidade, string classe)
    {
        var achado = Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra", "Descrição.", Enum.Parse<SeveridadeDoAchado>(severidade), [new RegistroEnvolvido("doc-1", "R1")]);

        Assert.Equal(Enum.Parse<ClasseDoAchado>(classe), achado.Classe);
    }

    [Fact(DisplayName = "REQ-RESULT-008 Regra do tipo Erro com severidade menor é rejeitada")]
    public void ErroSemCritica() => Assert.Throws<ExcecaoDeDominio>(() => DeRegra(TipoDaRegra.Erro, SeveridadeDoAchado.Media));

    [Fact(DisplayName = "REQ-RESULT-008 Regra do tipo Aviso não pode ser impeditiva")]
    public void AvisoCritico() => Assert.Throws<ExcecaoDeDominio>(() => DeRegra(TipoDaRegra.Aviso, SeveridadeDoAchado.Critica));

    [Fact(DisplayName = "REQ-RESULT-008 Regra sem tipo declarado não pode ser impeditiva, mas pode ser grave")]
    public void RegraSemTipo()
    {
        Assert.Throws<ExcecaoDeDominio>(() => DeRegra(TipoDaRegra.NaoDeclarado, SeveridadeDoAchado.Critica));
        Assert.Equal(ClasseDoAchado.Grave, DeRegra(TipoDaRegra.NaoDeclarado, SeveridadeDoAchado.Alta).Classe);
    }

    [Fact(DisplayName = "REQ-RESULT-008 Nível só vem quando o manual o declara")]
    public void NivelNaoDeclarado()
    {
        var achado = Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra", "Descrição.", SeveridadeDoAchado.Critica, [new RegistroEnvolvido("doc-1", "R1")],
            regra: new RegraDoManual("REGRA", TipoDaRegra.Erro), fonte: new FonteDoAchado("Manual", "12", "p. 1"), orientacao: "Corrija.");

        Assert.Null(achado.Regra!.Nivel);
    }

    [Fact(DisplayName = "REQ-RESULT-009 Sem regra do tipo Erro, só a violação de leiaute pode ser impeditiva")]
    public void ImpeditivaSemRegra()
    {
        Assert.Throws<ExcecaoDeDominio>(() => Achado.Criar(
            "ach-1", CategoriaDoAchado.InconsistenciaDeNegocio, OrigemDoAchado.Validador, "Fonte", "Descrição.", SeveridadeDoAchado.Critica, [new RegistroEnvolvido("doc-1", "R1")]));
        Assert.Equal(ClasseDoAchado.Impeditiva, Achado.Criar(
            "ach-2", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Leiaute", "Linha fora da estrutura.", SeveridadeDoAchado.Critica, [new RegistroEnvolvido("doc-1", "R1")]).Classe);
    }

    [Fact(DisplayName = "REQ-RESULT-005 Achado grave exige revisão humana")]
    public void RevisaoComGrave()
    {
        var execucao = Completa();

        execucao.AdicionarAchado(Achado.Criar(
            "ach-1", CategoriaDoAchado.InconsistenciaDeNegocio, OrigemDoAchado.Validador, "Regra", "Compensação acima do limite.", SeveridadeDoAchado.Alta, [new RegistroEnvolvido("doc-1", "M300")]));

        Assert.True(execucao.ExigeRevisaoHumana);
        Assert.Equal(VereditoDaExecucao.ConformeComRessalvas, execucao.Veredito);
    }

    [Fact(DisplayName = "REQ-RESULT-008 Regra com nível fora do manual é rejeitada")]
    public void NivelInvalido() =>
        Assert.Throws<ExcecaoDeDominio>(() => Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra", "Descrição.", SeveridadeDoAchado.Critica, [new RegistroEnvolvido("doc-1", "R1")],
            regra: new RegraDoManual("REGRA", TipoDaRegra.Erro, "9"), fonte: new FonteDoAchado("Manual", "12", "p. 1"), orientacao: "Corrija."));

    [Theory(DisplayName = "REQ-RESULT-009 Só erro estrutural e inconsistência de negócio podem ser impeditivos")]
    [InlineData(nameof(CategoriaDoAchado.IndicioDaIa), nameof(OrigemDoAchado.Llm))]
    [InlineData(nameof(CategoriaDoAchado.AusenciaDeEvidencias), nameof(OrigemDoAchado.Sistema))]
    [InlineData(nameof(CategoriaDoAchado.FalhaTecnica), nameof(OrigemDoAchado.Sistema))]
    public void SoValidadorImpede(string categoria, string origem) =>
        Assert.Throws<ExcecaoDeDominio>(() => Achado.Criar(
            "ach-1", Enum.Parse<CategoriaDoAchado>(categoria), Enum.Parse<OrigemDoAchado>(origem), "Fonte", "Descrição.", SeveridadeDoAchado.Critica, [new RegistroEnvolvido("doc-1", "R1")]));

    [Fact(DisplayName = "REQ-RESULT-010 Regra impeditiva violada recusa, mesmo com validações pendentes")]
    public void RecusadoComPendencias()
    {
        var execucao = Nova();

        execucao.AdicionarAchado(DeRegra(TipoDaRegra.Erro, SeveridadeDoAchado.Critica));

        Assert.Equal(StatusDaExecucao.Parcial, execucao.Status);
        Assert.Equal(VereditoDaExecucao.Recusado, execucao.Veredito);
    }

    [Fact(DisplayName = "REQ-RESULT-010 Sem achados e com validação pendente, o veredito é parcial")]
    public void ParcialSemAchados() => Assert.Equal(VereditoDaExecucao.Parcial, Nova().Veredito);

    [Fact(DisplayName = "REQ-RESULT-010 Entrada rejeitada não é avaliada")]
    public void EntradaRejeitadaNaoAvaliada()
    {
        var execucao = Nova();

        execucao.FalharEtapa(Etapa.ValidacaoDaEntrada);

        Assert.Equal(VereditoDaExecucao.NaoAvaliado, execucao.Veredito);
    }

    [Fact(DisplayName = "REQ-RESULT-010 Falha técnica não é avaliada")]
    public void FalhaTecnicaNaoAvaliada()
    {
        var execucao = Completa();

        execucao.FalharEtapa(Etapa.ConsultaAoManual);

        Assert.Equal(VereditoDaExecucao.NaoAvaliado, execucao.Veredito);
    }

    [Fact(DisplayName = "REQ-RESULT-010 Validações concluídas com aviso dão conforme com ressalvas")]
    public void ConformeComRessalvas()
    {
        var execucao = Completa();

        execucao.AdicionarAchado(DeRegra(TipoDaRegra.Aviso, SeveridadeDoAchado.Media));

        Assert.Equal(VereditoDaExecucao.ConformeComRessalvas, execucao.Veredito);
    }

    [Fact(DisplayName = "REQ-RESULT-010 Validações concluídas sem achados dão conforme")]
    public void Conforme() => Assert.Equal(VereditoDaExecucao.Conforme, Completa().Veredito);

    [Fact(DisplayName = "P05 Limitação declarada impede o veredito conforme")]
    public void LimitacaoImpedeConforme()
    {
        var execucao = Completa();

        execucao.AdicionarLimitacao("Manual da versão indisponível.");

        Assert.Equal(VereditoDaExecucao.Parcial, execucao.Veredito);
    }

    [Fact(DisplayName = "REQ-RESULT-011 Achado de regra sem fonte ou orientação é rejeitado")]
    public void RegraSemDetalhe() =>
        Assert.Throws<ExcecaoDeDominio>(() => Achado.Criar(
            "ach-1", CategoriaDoAchado.ErroEstrutural, OrigemDoAchado.Validador, "Regra", "Descrição.", SeveridadeDoAchado.Critica, [new RegistroEnvolvido("doc-1", "R1")],
            regra: new RegraDoManual("REGRA", TipoDaRegra.Erro, "1.1")));

    [Fact(DisplayName = "REQ-RESULT-011 Valor encontrado longo é cortado em 200 caracteres, com o corte indicado")]
    public void ValorLongo()
    {
        var achado = DeRegra(TipoDaRegra.Erro, SeveridadeDoAchado.Critica, new string('x', 250));

        var valor = Assert.Single(achado.Registros).ValorEncontrado!;
        Assert.StartsWith(new string('x', 200) + " ", valor, StringComparison.Ordinal);
        Assert.EndsWith("[cortado]", valor, StringComparison.Ordinal);
        Assert.DoesNotContain(new string('x', 201), valor, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-RESULT-011 Valor curto fica como está")]
    public void ValorCurto() =>
        Assert.Equal("20251231", Assert.Single(DeRegra(TipoDaRegra.Erro, SeveridadeDoAchado.Critica, "20251231").Registros).ValorEncontrado);

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
