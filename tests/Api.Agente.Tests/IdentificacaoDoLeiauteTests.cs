using Api.Agente.Dominio.Execucoes;
using Api.Agente.Dominio.Leiaute;
using Api.Agente.Tests.Infra;

namespace Api.Agente.Tests;

/// <summary>Reconhecimento da ECF e identificação do leiaute, com as ECF sintéticas de <c>Dados/ecf</c> (mudança 07, D2, D5 e D7).</summary>
public sealed class IdentificacaoDoLeiauteTests
{
    private static IdentificacaoDoLeiaute Identificar(string texto) => IdentificadorDeLeiaute.Identificar(ReconhecedorDeEcf.PrimeiraLinha(texto));

    [Theory(DisplayName = "REQ-LEIAUTE-001 O COD_VER de cada manual identifica o leiaute de 7 a 12")]
    [InlineData(7, "0007", 2020)]
    [InlineData(8, "0008", 2021)]
    [InlineData(9, "0009", 2022)]
    [InlineData(10, "0010", 2023)]
    [InlineData(11, "0011", 2024)]
    [InlineData(12, "0012", 2025)]
    public void LeiautesSuportados(int leiaute, string codVer, int ano)
    {
        var identificacao = Identificar(EcfSintetica.Texto(leiaute));

        Assert.Equal(
            (SituacaoDaIdentificacao.Identificado, "ECF", leiaute, codVer, ano),
            (identificacao.Situacao, identificacao.Tipo, identificacao.Leiaute!.Value, identificacao.CodVer, identificacao.AnoDoPeriodo!.Value));
    }

    [Fact(DisplayName = "REQ-LEIAUTE-002 COD_VER de 4 caracteres fora dos leiautes 7 a 12 é versão não suportada, com o valor registrado")]
    public void VersaoNaoSuportada()
    {
        var identificacao = Identificar(EcfSintetica.ComCampoDoRegistro0000(3, "0013"));

        Assert.Equal((SituacaoDaIdentificacao.VersaoNaoSuportada, "0013"), (identificacao.Situacao, identificacao.CodVer));
    }

    [Theory(DisplayName = "REQ-LEIAUTE-003 COD_VER vazio ou fora do formato torna a identificação ambígua")]
    [InlineData("")]
    [InlineData("012")]
    [InlineData("00012")]
    public void CodVerInvalido(string codVer)
    {
        var identificacao = Identificar(EcfSintetica.ComCampoDoRegistro0000(3, codVer));

        Assert.Equal(SituacaoDaIdentificacao.Ambigua, identificacao.Situacao);
        Assert.Contains("COD_VER", identificacao.Motivo, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "REQ-LEIAUTE-004 Situação especial no ano seguinte ainda é do leiaute 12")]
    public void SituacaoEspecialNoAnoSeguinte()
    {
        var texto = EcfSintetica.ComCampoDoRegistro0000(7, "1");
        texto = texto.Replace("|01012025|31122025|", "|01012026|31032026|", StringComparison.Ordinal);

        var identificacao = Identificar(texto);

        Assert.Equal((SituacaoDaIdentificacao.Identificado, 12), (identificacao.Situacao, identificacao.Leiaute!.Value));
    }

    [Fact(DisplayName = "REQ-LEIAUTE-004 COD_VER de outro ano torna a identificação ambígua, com o COD_VER e o ano no motivo")]
    public void VersaoDeOutroAno()
    {
        var texto = EcfSintetica.Texto().Replace("|01012025|31122025|", "|01012023|31122023|", StringComparison.Ordinal);

        var identificacao = Identificar(texto);

        Assert.Equal(SituacaoDaIdentificacao.Ambigua, identificacao.Situacao);
        Assert.Contains("0012", identificacao.Motivo, StringComparison.Ordinal);
        Assert.Contains("2023", identificacao.Motivo, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "REQ-LEIAUTE-004 DT_FIN fora do formato ddmmaaaa torna a identificação ambígua")]
    [InlineData("31022025")]
    [InlineData("2025-12-31")]
    [InlineData("")]
    public void DataFinalInvalida(string dtFin)
    {
        var identificacao = Identificar(EcfSintetica.ComCampoDoRegistro0000(11, dtFin));

        Assert.Equal(SituacaoDaIdentificacao.Ambigua, identificacao.Situacao);
        Assert.Contains("DT_FIN", identificacao.Motivo, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "REQ-RECEPCAO-005 A ECF é reconhecida pela primeira linha, e nada vem antes do Registro 0000")]
    [InlineData("|0000|LECF|0012|", true)]
    [InlineData("﻿|0000|LECF|0012|", true)]
    [InlineData("<NFe>texto</NFe>", false)]
    [InlineData("\r\n|0000|LECF|0012|", false)]
    [InlineData(" |0000|LECF|0012|", false)]
    [InlineData("|0000|LECD|0012|", false)]
    public void Reconhecimento(string texto, bool ehEcf) => Assert.Equal(ehEcf, ReconhecedorDeEcf.EhEcf(texto));

    [Theory(DisplayName = "REQ-RECEPCAO-007 O marcador de corte da Sofia é reconhecido no fim do texto")]
    [InlineData("|9999|6|\r\n[DOCUMENTO TRUNCADO]", true)]
    [InlineData("|9999|6|\r\n[DOCUMENTO TRUNCADO]\r\n  ", true)]
    [InlineData("[DOCUMENTO TRUNCADO]\r\n|9999|6|", false)]
    [InlineData("|9999|6|", false)]
    public void MarcadorDeCorte(string texto, bool cortado) => Assert.Equal(cortado, ReconhecedorDeEcf.CortadoPelaSofia(texto));

    [Fact(DisplayName = "REQ-LEIAUTE-003 Falha da etapa por causa do documento deixa a análise parcial; a de causa técnica, falha técnica")]
    public void CausaDaFalhaNoStatus()
    {
        var doDocumento = Execucao.Criar(Guid.NewGuid(), Guid.NewGuid(), new IdentidadeDeclarada("p", "org"), DateTimeOffset.UtcNow);
        var tecnica = Execucao.Criar(Guid.NewGuid(), Guid.NewGuid(), new IdentidadeDeclarada("p", "org"), DateTimeOffset.UtcNow);
        foreach (var execucao in new[] { doDocumento, tecnica })
        {
            execucao.ConcluirEtapa(Etapa.Recepcao);
            execucao.ConcluirEtapa(Etapa.ValidacaoDaEntrada);
        }

        doDocumento.FalharEtapa(Etapa.IdentificacaoDoLeiaute, CausaDaFalha.Documento);
        tecnica.FalharEtapa(Etapa.IdentificacaoDoLeiaute);

        Assert.Equal(StatusDaExecucao.Parcial, doDocumento.Status);
        Assert.Equal(StatusDaExecucao.FalhaTecnica, tecnica.Status);
        Assert.True(doDocumento.ExigeRevisaoHumana);
        Assert.Equal(EstadoDaEtapa.NaoExecutada, doDocumento.Etapas.Single(etapa => etapa.Etapa == Etapa.ValidacaoEstrutural).Estado);
    }
}
