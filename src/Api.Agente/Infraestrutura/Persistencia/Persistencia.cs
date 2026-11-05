using System.ComponentModel.DataAnnotations;
using Api.Agente.Aplicacao.Execucoes;
using Api.Agente.Endpoints;
using Api.Agente.Infraestrutura.Configuracao;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Api.Agente.Infraestrutura.Persistencia;

/// <summary>Conexão com o banco da Api.Agente (seção <c>ConnectionStrings</c>; segredo, nunca versionado).</summary>
internal sealed class OpcoesDoBanco : IValidatableObject
{
    public const string Secao = "ConnectionStrings";

    [Required]
    public string? Execucoes { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var valida = true;
        try
        {
            _ = new NpgsqlConnectionStringBuilder(Execucoes);
        }
        catch (ArgumentException)
        {
            valida = false;
        }

        // A mensagem nunca traz o valor: ele contém a senha (SQ-03).
        if (!valida)
        {
            yield return new ValidationResult("Use o formato Host=...;Database=...;Username=...;Password=...", [nameof(Execucoes)]);
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
            var conexao = new NpgsqlConnectionStringBuilder(provedor.GetRequiredService<IOptions<OpcoesDoBanco>>().Value.Execucoes)
            {
                GssEncryptionMode = GssEncryptionMode.Disable,
            };
            return NpgsqlDataSource.Create(conexao.ConnectionString);
        });
        servicos.AddSingleton<IRepositorioDeExecucoes, RepositorioDeExecucoes>();
        servicos.AddSingleton<Migrador>();
        servicos.AddHealthChecks().AddCheck<VerificacaoDoBanco>("banco", tags: [SaudeEndpoints.TagProntidao]);
        return servicos;
    }
}
