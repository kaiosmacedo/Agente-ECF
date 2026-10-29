using Api.RAG.Infraestrutura.Configuracao;

namespace Api.RAG.Infraestrutura.Observabilidade;

/// <summary>Logs estruturados em JSON na saída padrão, com escopos (REQ-OBSERV-002).</summary>
internal static class Observabilidade
{
    public static WebApplicationBuilder AdicionarObservabilidade(this WebApplicationBuilder builder, IdentidadeServico identidade)
    {
        builder.Services.AddSingleton(identidade);

        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(opcoes =>
        {
            opcoes.IncludeScopes = true;
            opcoes.UseUtcTimestamp = true;
            opcoes.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

        // Valor inválido não altera o nível: a validação das opções impede a inicialização.
        var nivel = builder.Configuration[$"{OpcoesObservabilidade.Secao}:{nameof(OpcoesObservabilidade.NivelMinimo)}"];
        if (Enum.TryParse<LogLevel>(nivel, ignoreCase: false, out var nivelMinimo) && Enum.IsDefined(nivelMinimo))
        {
            builder.Logging.SetMinimumLevel(nivelMinimo);
        }

        return builder;
    }
}
