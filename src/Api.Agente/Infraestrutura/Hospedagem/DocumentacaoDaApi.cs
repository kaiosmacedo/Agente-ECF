using Microsoft.OpenApi;

namespace Api.Agente.Infraestrutura.Hospedagem;

/// <summary>
/// Documento OpenAPI e interface do Swagger em /swagger, em todos os ambientes (mudança documentacao-api).
/// </summary>
internal static class DocumentacaoDaApi
{
    public const string CaminhoDoDocumento = "/openapi/v1.json";

    public const string PrefixoDaInterface = "swagger";

    /// <summary>
    /// O Vercel termina o https e repassa em http, então o servidor declarado vira relativo: as chamadas de teste da
    /// página usam o esquema e o host em que ela foi aberta (REQ-DOCAPI-002; D2).
    /// </summary>
    public static IServiceCollection AdicionarDocumentacao(this IServiceCollection servicos) =>
        servicos.AddOpenApi(opcoes => opcoes.AddDocumentTransformer((documento, _, _) =>
        {
            documento.Servers = [new OpenApiServer { Url = "/" }];
            return Task.CompletedTask;
        }));

    /// <summary>
    /// Os arquivos da interface vêm do próprio serviço, e o validador on-line fica desligado, para que a página não
    /// faça requisição a outro domínio (REQ-DOCAPI-001, 003; D1).
    /// </summary>
    public static WebApplication UsarDocumentacao(this WebApplication app)
    {
        app.MapOpenApi();
        app.UseSwaggerUI(opcoes =>
        {
            opcoes.SwaggerEndpoint(CaminhoDoDocumento, "Api.Agente v1");
            opcoes.RoutePrefix = PrefixoDaInterface;
            opcoes.DocumentTitle = "Api.Agente | Documentação";
            opcoes.ConfigObject.ValidatorUrl = "none";
        });
        return app;
    }
}
