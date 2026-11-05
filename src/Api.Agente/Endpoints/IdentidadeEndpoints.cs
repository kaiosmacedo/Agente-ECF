using Api.Agente.Aplicacao.Identidade;
using Api.Agente.Infraestrutura.Autenticacao;

namespace Api.Agente.Endpoints;

/// <summary>Consulta da identidade autenticada (REQ-AUTH-008), para clientes conferirem credencial e organização.</summary>
internal static class IdentidadeEndpoints
{
    public const string CabecalhoOrganizacao = "X-Organizacao-Id";

    private static readonly Dictionary<Perfil, string> NomeDoPerfil = new()
    {
        [Perfil.AdministradorAgente] = "administrador-agente",
        [Perfil.OperadorOrquestrador] = "operador-orquestrador",
        [Perfil.ClienteAgente] = "cliente-agente",
        [Perfil.UsuarioAnalise] = "usuario-analise",
    };

    public static IEndpointRouteBuilder MapearIdentidade(this IEndpointRouteBuilder rotas)
    {
        rotas.MapGet("/identidade", ConsultarIdentidade).RequireAuthorization(PoliticasDoAgente.QualquerPerfil);
        return rotas;
    }

    private static IResult ConsultarIdentidade(HttpContext contexto, LeitorDeIdentidade leitor, ILoggerFactory logs)
    {
        if (leitor.Ler(contexto.User) is not { } identidade)
        {
            return Results.Forbid();
        }

        // A organização não é obrigatória aqui: a consulta só informa a resolvida (D5).
        var resultado = ResolvedorDeOrganizacao.Resolver(identidade, contexto.Request.Headers[CabecalhoOrganizacao].ToString(), obrigatoria: false);
        if (resultado.Situacao != SituacaoDaOrganizacao.Resolvida)
        {
            var logger = logs.CreateLogger(typeof(IdentidadeEndpoints).FullName!);
            RegistroDeAcesso.OrganizacaoRecusada(logger, resultado.Situacao.ToString(), contexto.Request.Path.Value ?? "/");
            return resultado.Situacao == SituacaoDaOrganizacao.FormatoInvalido
                ? Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: $"Cabeçalho {CabecalhoOrganizacao} inválido.")
                : Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Organização não autorizada para esta identidade.");
        }

        return Results.Ok(new
        {
            id = identidade.Id,
            tipo = identidade.Tipo == TipoIdentidade.Aplicacao ? "aplicacao" : "pessoa",
            perfis = identidade.Perfis.Select(p => NomeDoPerfil[p]).Order(StringComparer.Ordinal),
            organizacoes = identidade.Organizacoes.Order(StringComparer.Ordinal),
            organizacaoResolvida = resultado.Organizacao,
        });
    }
}
