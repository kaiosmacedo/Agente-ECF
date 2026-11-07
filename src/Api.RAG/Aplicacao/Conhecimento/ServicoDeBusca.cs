using System.Text.RegularExpressions;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Consumo;

namespace Api.RAG.Aplicacao.Conhecimento;

/// <summary>Os erros tipados da busca, que viram os códigos do contrato da ferramenta (mudança 06, D2).</summary>
internal enum ErroDaBusca
{
    ArgumentoInvalido,
    BuscaDesativada,
    LimiteDeUsoAtingido,
    FalhaDoProvedor,
    FalhaTecnica,
}

/// <summary>A busca não foi feita. A mensagem não traz o texto da consulta nem detalhes internos.</summary>
internal sealed class BuscaRecusada(ErroDaBusca erro, string mensagem, Exception? causa = null) : Exception(mensagem, causa)
{
    public ErroDaBusca Erro { get; } = erro;
}

/// <summary>Argumentos como chegaram, todos opcionais: a validação é deste serviço (REQ-RECUP-003).</summary>
internal sealed record PedidoDeBusca(string? Consulta, string? TipoDocumento, string? Versao, int? Quantidade, string? Codigo, string? IdExecucao);

/// <summary>Resultado da busca. Sem documento vigente, vem vazio com o motivo (REQ-RECUP-002).</summary>
internal sealed record ResultadoDaBusca(string? Motivo, IReadOnlyList<TrechoEncontrado> Trechos)
{
    public const string VersaoIndisponivel = "versao_indisponivel";
}

/// <summary>Valores da busca, vindos da configuração validada (mudança 06, D5).</summary>
internal sealed record ParametrosDaBusca(bool Habilitada, int LimiteDeConsultasPorDia, int QuantidadePadrao, int QuantidadeMaxima);

/// <summary>
/// Busca nos manuais vigentes de um tipo e de uma versão (REQ-RECUP-001 a 006). Nenhum embedding é gerado antes de
/// validar os argumentos, conferir o desligamento, a versão e o limite do dia. O texto da consulta nunca vai para log.
/// </summary>
internal sealed partial class ServicoDeBusca(
    IRepositorioDeDocumentos documentos,
    IRepositorioDeConsumo consumo,
    IClienteDeEmbeddings embeddings,
    ParametrosDaBusca parametros,
    TimeProvider relogio)
{
    public const int TamanhoMaximoDaConsulta = 500;

    public async Task<ResultadoDaBusca> BuscarAsync(PedidoDeBusca pedido, string? correlacao, CancellationToken cancelamento)
    {
        var (consulta, tipo, versao, quantidade, codigo, idExecucao) = Validar(pedido);
        if (!parametros.Habilitada)
        {
            throw new BuscaRecusada(ErroDaBusca.BuscaDesativada, "A busca está desativada neste ambiente.");
        }

        try
        {
            if (!await documentos.HaVigenteAsync(tipo, versao, cancelamento))
            {
                return new ResultadoDaBusca(ResultadoDaBusca.VersaoIndisponivel, []);
            }

            var inicioDoDia = new DateTimeOffset(relogio.GetUtcNow().UtcDateTime.Date, TimeSpan.Zero);
            if (await consumo.ContarConsultasDesdeAsync(inicioDoDia, cancelamento) >= parametros.LimiteDeConsultasPorDia)
            {
                throw new BuscaRecusada(ErroDaBusca.LimiteDeUsoAtingido, "O limite diário de consultas foi atingido.");
            }

            var resposta = await embeddings.GerarAsync([consulta], new ContextoDaChamada(idExecucao, correlacao, FinalidadeDaChamada.Consulta), cancelamento);
            var trechos = await documentos.BuscarSemelhantesAsync(resposta.Vetores[0], tipo, versao, quantidade, cancelamento, codigo);
            return new ResultadoDaBusca(null, trechos);
        }
        catch (FalhaDoProvedor erro)
        {
            throw new BuscaRecusada(ErroDaBusca.FalhaDoProvedor, "O provedor de embeddings não respondeu.", erro);
        }
        catch (FalhaDoRepositorio erro)
        {
            throw new BuscaRecusada(ErroDaBusca.FalhaTecnica, "A base de conhecimento não respondeu.", erro);
        }
    }

    private (string Consulta, string Tipo, string Versao, int Quantidade, string? Codigo, Guid? IdExecucao) Validar(PedidoDeBusca pedido)
    {
        var consulta = pedido.Consulta?.Trim();
        if (string.IsNullOrEmpty(consulta) || consulta.Length > TamanhoMaximoDaConsulta)
        {
            throw Invalido($"consulta deve ter de 1 a {TamanhoMaximoDaConsulta} caracteres.");
        }

        if (pedido.TipoDocumento is not { } tipo || !Identificador().IsMatch(tipo))
        {
            throw Invalido("tipoDocumento é obrigatório, com letras minúsculas, dígitos e hífen, como manual-ecf.");
        }

        if (pedido.Versao is not { } versao || !Identificador().IsMatch(versao))
        {
            throw Invalido("versao é obrigatória, com letras minúsculas, dígitos, ponto e hífen, como leiaute-12.");
        }

        var quantidade = pedido.Quantidade ?? parametros.QuantidadePadrao;
        if (quantidade < 1 || quantidade > parametros.QuantidadeMaxima)
        {
            throw Invalido($"quantidade deve estar entre 1 e {parametros.QuantidadeMaxima}.");
        }

        if (pedido.Codigo is { } codigo && !Codigo().IsMatch(codigo))
        {
            throw Invalido("codigo deve ter só letras maiúsculas, dígitos e sublinhado, até 60 caracteres.");
        }

        Guid? idExecucao = null;
        if (pedido.IdExecucao is { } texto)
        {
            idExecucao = Guid.TryParse(texto, out var valor) ? valor : throw Invalido("idExecucao deve ser um UUID.");
        }

        return (consulta, tipo, versao, quantidade, pedido.Codigo, idExecucao);
    }

    private static BuscaRecusada Invalido(string mensagem) => new(ErroDaBusca.ArgumentoInvalido, mensagem);

    [GeneratedRegex(@"\A[a-z0-9][a-z0-9.\-]{0,39}\z")]
    private static partial Regex Identificador();

    [GeneratedRegex(@"\A[A-Z0-9_]{1,60}\z")]
    private static partial Regex Codigo();
}
