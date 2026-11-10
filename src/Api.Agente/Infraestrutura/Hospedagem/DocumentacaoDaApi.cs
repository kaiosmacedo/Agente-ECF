using Api.Agente.Infraestrutura.Integracao;
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
            DeclararBearerDaSofia(documento);
            return Task.CompletedTask;
        }));

    /// <summary>
    /// As rotas do A2A pedem o Bearer da Sofia, e a página do Swagger ganha o botão de autorizar; o documento só declara o
    /// esquema, nunca o valor da chave (REQ-A2A-010; REQ-DOCAPI-003).
    /// </summary>
    private static void DeclararBearerDaSofia(OpenApiDocument documento)
    {
        documento.Components ??= new OpenApiComponents();
        documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        documento.Components.SecuritySchemes[AgenteA2A.EsquemaDeSeguranca] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            Description = $"Chave compartilhada da Sofia, configurada no servidor em {AutenticacaoDaSofia.VariavelDaChave}.",
        };

        foreach (var (caminho, item) in documento.Paths ?? [])
        {
            if (!AutenticacaoDaSofia.Protegido(new PathString(caminho)) || item.Operations is null)
            {
                continue;
            }

            foreach (var operacao in item.Operations.Values)
            {
                operacao.Security ??= [];
                operacao.Security.Add(new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(AgenteA2A.EsquemaDeSeguranca, documento)] = [] });
            }
        }
    }

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
