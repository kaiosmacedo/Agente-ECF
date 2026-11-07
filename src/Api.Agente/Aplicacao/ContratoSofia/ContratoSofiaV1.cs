using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Aplicacao.ContratoSofia;

/// <summary>Requisição do contrato SOFIA v1.0. Tudo é anulável porque vem de fora e passa pelo validador.</summary>
internal sealed record SolicitacaoAnaliseSofia(
    string? VersaoContrato,
    string? IdExecucao,
    string? IdSolicitacao,
    UsuarioSofia? Usuario,
    string? Pergunta,
    IReadOnlyList<DocumentoSofia>? Documentos,
    ContextoSofia? Contexto);

internal sealed record UsuarioSofia(string? Nome);

internal sealed record DocumentoSofia(string? IdDocumento, string? Nome, string? Tipo, string? Texto);

internal sealed record ContextoSofia(string? Idioma, string? Instrucoes);

/// <summary>Resposta do contrato SOFIA v1.0, preenchida somente pelo sistema (P13).</summary>
internal sealed record ResultadoAnaliseSofia(
    string VersaoContrato,
    string IdExecucao,
    string IdSolicitacao,
    AgenteSofia Agente,
    string Status,
    string Analise,
    IReadOnlyList<AchadoSofia> Achados,
    IReadOnlyList<string> Observacoes,
    ErroSofia? Erro,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? IdExecucaoAgente = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] bool? RevisaoHumana = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Veredito = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ResumoDosAchados? ResumoAchados = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<ItemNaoVerificado>? NaoVerificados = null);

internal sealed record AgenteSofia(string IdAgente, string Nome);

internal sealed record AchadoSofia(
    string IdAchado,
    string Descricao,
    IReadOnlyList<string> IdDocumentos,
    IReadOnlyList<string> Evidencias,
    string? Severidade,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Categoria = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Origem = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Regra = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Classe = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<RegistroEnvolvido>? Registros = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Orientacao = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] FonteDoAchado? Fonte = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<TrechoConsultado>? TrechosDoManual = null);

internal sealed record ErroSofia(string Codigo, string Mensagem);

/// <summary>Valores fixados pelo contrato SOFIA v1.0 (skill <c>sofia-a2a-contrato</c>).</summary>
internal static class ContratoSofiaV1
{
    public const string Versao = "1.0";

    public const string StatusConcluido = "concluido";
    public const string StatusParcial = "parcial";
    public const string StatusErro = "erro";

    public static AgenteSofia Agente { get; } = new("agente-ecf", "Agente ECF");

    /// <summary>Nomes em camelCase, iguais aos do contrato, sem escapar acentos no texto JSON.</summary>
    public static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
