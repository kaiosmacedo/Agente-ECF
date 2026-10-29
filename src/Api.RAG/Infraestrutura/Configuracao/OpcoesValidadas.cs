using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Api.RAG.Infraestrutura.Configuracao;

/// <summary>
/// Registro de opções validadas na inicialização (REQ-CONFIG-002). As propriedades ligadas à
/// configuração são strings, para que erros de conversão nunca exponham o valor recebido.
/// </summary>
internal static class OpcoesValidadas
{
    public static OptionsBuilder<TOpcoes> AdicionarOpcoesValidadas<TOpcoes>(
        this IServiceCollection servicos, IConfiguration configuracao, string secao)
        where TOpcoes : class
    {
        servicos.AddSingleton<IValidateOptions<TOpcoes>>(new ValidadorDeOpcoes<TOpcoes>(secao));
        return servicos.AddOptions<TOpcoes>().Bind(configuracao.GetSection(secao)).ValidateOnStart();
    }
}

/// <summary>Valida as anotações das opções e cita apenas os nomes, nunca os valores.</summary>
internal sealed class ValidadorDeOpcoes<TOpcoes>(string secao) : IValidateOptions<TOpcoes>
    where TOpcoes : class
{
    public ValidateOptionsResult Validate(string? name, TOpcoes options)
    {
        var resultados = new List<ValidationResult>();
        if (Validator.TryValidateObject(options, new ValidationContext(options), resultados, validateAllProperties: true))
        {
            return ValidateOptionsResult.Success;
        }

        var nomes = resultados.SelectMany(r => r.MemberNames).Distinct().Select(membro => $"{secao}:{membro}");
        return ValidateOptionsResult.Fail($"Configuração ausente ou inválida: {string.Join(", ", nomes)}.");
    }
}
