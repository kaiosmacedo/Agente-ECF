using System.ComponentModel.DataAnnotations;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Endpoints;
using Api.RAG.Infraestrutura.Configuracao;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Api.RAG.Infraestrutura.Persistencia;

/// <summary>Conexão com o banco da Api.RAG (seção <c>ConnectionStrings</c>; segredo, nunca versionado).</summary>
internal sealed class OpcoesDoBanco : IValidatableObject
{
    public const string Secao = "ConnectionStrings";

    [Required]
    public string? Conhecimento { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var valida = true;
        try
        {
            _ = new NpgsqlConnectionStringBuilder(Conhecimento);
        }
        catch (ArgumentException)
        {
            valida = false;
        }

        // A mensagem nunca traz o valor: ele contém a senha (SQ-03).
        if (!valida)
        {
            yield return new ValidationResult("Use o formato Host=...;Database=...;Username=...;Password=...", [nameof(Conhecimento)]);
        }
    }
}

/// <summary>Registro do banco, do repositório, do migrador e da verificação de prontidão.</summary>
internal static class Persistencia
{
    public static IServiceCollection AdicionarPersistencia(this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.AdicionarOpcoesValidadas<OpcoesDoBanco>(configuracao, OpcoesDoBanco.Secao);
        servicos.AddSingleton(provedor =>
        {
            // Sem GSS: não é usado, e a tentativa escreve uma linha fora do JSON nos logs, porque a imagem não traz o Kerberos.
            var conexao = new NpgsqlConnectionStringBuilder(provedor.GetRequiredService<IOptions<OpcoesDoBanco>>().Value.Conhecimento)
            {
                GssEncryptionMode = GssEncryptionMode.Disable,
            };
            return NpgsqlDataSource.Create(conexao.ConnectionString);
        });
        servicos.AddSingleton<IRepositorioDeConsumo, RepositorioDeConsumo>();
        servicos.AddSingleton<Migrador>();
        servicos.AddHealthChecks().AddCheck<VerificacaoDoBanco>("banco", tags: [SaudeEndpoints.TagProntidao]);
        return servicos;
    }
}
