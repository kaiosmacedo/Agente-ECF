using System.Text.Json.Serialization;
using Api.Agente.Aplicacao.Conhecimento;

namespace Api.Agente.Aplicacao.Perguntas;

/// <summary>
/// Resultado de uma pergunta, contrato <c>resposta-pergunta</c> 1.0 (mudança 17, D7). É o corpo do <c>POST /perguntas</c>
/// e a origem da projeção no contrato SOFIA v1.0. Tudo aqui é preenchido pelo sistema (P13).
/// </summary>
internal sealed record RespostaDaPergunta(
    string VersaoContrato,
    Guid IdPergunta,
    string Status,
    string Resposta,
    string? VersaoDoManual,
    IReadOnlyList<FonteDaResposta> Fontes,
    IReadOnlyList<string> Observacoes,
    ErroDaPergunta? Erro,
    VersoesDaPergunta Versoes,
    DateTimeOffset CriadaEm,
    long DuracaoMs)
{
    public const string Contrato = "1.0";
}

/// <summary>Um trecho do manual usado na resposta: o texto não vem aqui, só a referência recuperável (P06).</summary>
internal sealed record FonteDaResposta(string Documento, string Versao, int Pagina, Guid IdTrecho);

internal sealed record ErroDaPergunta(string Codigo, string Mensagem);

internal sealed record VersoesDaPergunta(
    string Contrato,
    string Servico,
    string Prompt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Modelo);

/// <summary>Os três status do contrato SOFIA v1.0, usados também no REST (REQ-PERGUNTA-008).</summary>
internal static class StatusDaPergunta
{
    public const string Concluido = "concluido";
    public const string Parcial = "parcial";
    public const string Erro = "erro";
}

/// <summary>Como terminou a redação (mudança 17, D9): aceita, reserva do sistema ou sem chamada ao modelo.</summary>
internal enum DesfechoDaRedacao
{
    Aceita,
    Reserva,
    NaoChamada,
}

/// <summary>Pedido normalizado, igual nos dois canais (REQ-PERGUNTA-008).</summary>
internal sealed record PedidoDePergunta(
    string Pergunta,
    string Canal,
    Guid? IdSolicitacao,
    string? VersaoInformada,
    string? Correlacao);

/// <summary>O que fica registrado de uma pergunta, sem o texto dela nem o da resposta (DP-25; REQ-PERGUNTA-009).</summary>
internal sealed record RegistroDaPergunta(
    Guid Id,
    Guid? IdSolicitacao,
    string Canal,
    string Status,
    string? CodigoDoErro,
    string PerguntaSha256,
    int PerguntaCaracteres,
    string? VersaoDoManual,
    IReadOnlyList<TrechoRegistrado> Trechos,
    IReadOnlyList<Guid> Citados,
    DesfechoDaRedacao Redacao,
    string? Modelo,
    DateTimeOffset CriadaEm,
    DateTimeOffset ConcluidaEm);

internal sealed record TrechoRegistrado(Guid IdTrecho, int Pagina, double Relevancia);

/// <summary>Onde as perguntas ficam registradas. Falhas de acesso chegam como <c>FalhaDoRepositorio</c>.</summary>
internal interface IRepositorioDePerguntas
{
    /// <summary>Grava o registro. Uma retentativa com o mesmo <c>idSolicitacao</c> não grava de novo (D9).</summary>
    Task RegistrarAsync(RegistroDaPergunta registro, CancellationToken cancelamento);

    /// <summary>Quantas perguntas foram respondidas, com status <c>concluido</c> ou <c>parcial</c>, desde o instante dado.</summary>
    Task<long> ContarRespondidasDesdeAsync(DateTimeOffset desde, CancellationToken cancelamento);
}

/// <summary>Material de uma tentativa de redação: a pergunta e os trechos, ambos dados não confiáveis (P10).</summary>
internal sealed record MaterialDaRedacao(string Pergunta, string Versao, IReadOnlyList<TrechoDoManual> Trechos);

/// <summary>Saída bruta de uma tentativa, com os tokens informados pelo provedor, quando informados.</summary>
internal sealed record SaidaDoRedator(string Texto, int? TokensEntrada, int? TokensSaida);

/// <summary>
/// Redator da resposta (mudança 17, D5). A implementação usa o Semantic Kernel na infraestrutura (RA-06). Uma chamada é
/// uma tentativa; as retentativas ficam com o serviço, que registra o consumo de cada uma.
/// </summary>
internal interface IRedatorDeRespostas
{
    string Provedor { get; }

    string Modelo { get; }

    Task<SaidaDoRedator> RedigirAsync(MaterialDaRedacao material, CancellationToken cancelamento);
}

/// <summary>Falha de uma tentativa de redação; <see cref="Transitoria"/> diz se vale repetir.</summary>
internal sealed class FalhaDoRedator(string mensagem, bool transitoria, int? tokensEntrada = null, int? tokensSaida = null, Exception? causa = null)
    : Exception(mensagem, causa)
{
    public bool Transitoria { get; } = transitoria;

    public int? TokensEntrada { get; } = tokensEntrada;

    public int? TokensSaida { get; } = tokensSaida;
}

/// <summary>O que o serviço precisa saber da configuração, sem depender da infraestrutura.</summary>
internal sealed record ConfiguracaoDasPerguntas(
    bool Habilitada,
    bool RedacaoHabilitada,
    int LimitePorDia,
    double? RelevanciaMinima,
    int Retentativas,
    string VersaoDoServico,
    int QuantidadeDeTrechos = 5);
