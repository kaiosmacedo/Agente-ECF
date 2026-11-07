using Api.RAG.Aplicacao.Conhecimento;
using Api.RAG.Aplicacao.Consumo;
using Api.RAG.Dominio.Consumo;
using Api.RAG.Tests.Infra;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.RAG.Tests;

/// <summary>Regras do serviço de busca com dublês: nenhum teste chama a OpenAI nem o banco (mudança 06).</summary>
public sealed class BuscaTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 7, 15, 0, 0, TimeSpan.Zero);

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }

    private sealed class ProvedorContado : IProvedorDeEmbeddings
    {
        public int Chamadas { get; private set; }

        public bool Indisponivel { get; init; }

        public string Provedor => "falso";

        public string Modelo => "text-embedding-3-small";

        public Task<RespostaDeEmbeddings> GerarAsync(IReadOnlyList<string> textos, CancellationToken cancelamento)
        {
            Chamadas++;
            return Indisponivel
                ? throw new FalhaDoProvedor("Provedor indisponível no teste.")
                : Task.FromResult(new RespostaDeEmbeddings([.. textos.Select(_ => new float[3])], 7));
        }
    }

    /// <summary>Só o que a busca usa; o resto não é chamado.</summary>
    private sealed class DocumentosFalsos(bool haVigente) : IRepositorioDeDocumentos
    {
        public string? CodigoRecebido { get; private set; }

        public Task<bool> HaVigenteAsync(string tipo, string versao, CancellationToken cancelamento) => Task.FromResult(haVigente);

        public Task<IReadOnlyList<TrechoEncontrado>> BuscarSemelhantesAsync(float[] vetor, string tipo, string versao, int quantidade, CancellationToken cancelamento, string? codigo = null)
        {
            CodigoRecebido = codigo;
            IReadOnlyList<TrechoEncontrado> trechos = [new(Guid.NewGuid(), Guid.NewGuid(), "Manual", versao, 1, null, "texto", 0.9, false)];
            return Task.FromResult(trechos);
        }

        public Task<DocumentoDaBase?> BuscarPorHashAsync(string hash, CancellationToken cancelamento) => throw new NotSupportedException();

        public Task<DocumentoDaBase?> BuscarAsync(Guid id, CancellationToken cancelamento) => throw new NotSupportedException();

        public Task<IReadOnlyList<DocumentoDaBase>> ListarAsync(string? tipo, string? versao, CancellationToken cancelamento) => throw new NotSupportedException();

        public Task<Guid> IniciarAsync(MetadadosDoDocumento metadados, string hash, long tamanhoBytes, DateTimeOffset quando, CancellationToken cancelamento) => throw new NotSupportedException();

        public Task ConcluirAsync(Guid id, MetadadosDoDocumento metadados, int paginas, IReadOnlyList<TrechoComVetor> trechos, string modelo, int dimensao, CancellationToken cancelamento) => throw new NotSupportedException();

        public Task MarcarFalhaAsync(Guid id, CancellationToken cancelamento) => throw new NotSupportedException();

        public Task<bool> DesativarAsync(Guid id, CancellationToken cancelamento) => throw new NotSupportedException();

        public Task ApagarParaReprocessarAsync(Guid id, CancellationToken cancelamento) => throw new NotSupportedException();
    }

    private static (ServicoDeBusca Servico, ProvedorContado Provedor, RepositorioDeConsumoEmMemoria Consumo, DocumentosFalsos Documentos) Montar(
        bool habilitada = true, int limite = 100, bool haVigente = true, bool provedorIndisponivel = false)
    {
        var relogio = new RelogioFixo(Agora);
        var provedor = new ProvedorContado { Indisponivel = provedorIndisponivel };
        var consumo = new RepositorioDeConsumoEmMemoria();
        var documentos = new DocumentosFalsos(haVigente);
        var registrador = new RegistradorDeConsumo(consumo, NullLogger<RegistradorDeConsumo>.Instance);
        var cliente = new ClienteDeEmbeddingsComConsumo(provedor, registrador, new CalculadoraDeCusto([]), new PoliticaDeTentativas(2, TimeSpan.Zero), relogio);
        var servico = new ServicoDeBusca(documentos, consumo, cliente, new ParametrosDaBusca(habilitada, limite, 5, 10), relogio);
        return (servico, provedor, consumo, documentos);
    }

    private static PedidoDeBusca Pedido(string? consulta = "Quais campos tem o Registro 0000?", string? tipo = "manual-ecf", string? versao = "leiaute-12", int? quantidade = 3, string? codigo = null, string? idExecucao = null) =>
        new(consulta, tipo, versao, quantidade, codigo, idExecucao);

    private static RegistroDeConsumo Consulta(DateTimeOffset quando) => new(
        Guid.NewGuid(), null, null, "falso", "text-embedding-3-small", TipoDeChamada.Embedding, StatusDaChamada.Sucesso, quando, 1, 1,
        5, OrigemDoValor.Informado, null, OrigemDoValor.NaoAplicavel, null, OrigemDoValor.Desconhecido, null, FinalidadeDaChamada.Consulta);

    private static readonly Dictionary<string, PedidoDeBusca> Invalidos = new(StringComparer.Ordinal)
    {
        ["quantidade 0"] = Pedido(quantidade: 0),
        ["quantidade 11"] = Pedido(quantidade: 11),
        ["consulta vazia"] = Pedido(consulta: "   "),
        ["consulta longa"] = Pedido(consulta: new string('a', 501)),
        ["tipo ausente"] = Pedido(tipo: null),
        ["tipo fora do padrão"] = Pedido(tipo: "ECF"),
        ["versão ausente"] = Pedido(versao: null),
        ["código com aspas"] = Pedido(codigo: "REGRA'; DROP"),
        ["execução que não é UUID"] = Pedido(idExecucao: "x"),
    };

    public static TheoryData<string> ArgumentosInvalidos => [.. Invalidos.Keys];

    [Theory(DisplayName = "REQ-RECUP-003 e 005 Argumento inválido gera erro tipado, sem nenhum embedding")]
    [MemberData(nameof(ArgumentosInvalidos))]
    public async Task ArgumentoInvalido(string caso)
    {
        var (servico, provedor, _, _) = Montar();

        var recusa = await Assert.ThrowsAsync<BuscaRecusada>(() => servico.BuscarAsync(Invalidos[caso], "corr", TestContext.Current.CancellationToken));

        Assert.True(recusa.Erro == ErroDaBusca.ArgumentoInvalido, caso);
        Assert.Equal(0, provedor.Chamadas);
    }

    [Fact(DisplayName = "REQ-RECUP-006 Busca desativada gera o erro busca_desativada, sem nenhum embedding")]
    public async Task BuscaDesativada()
    {
        var (servico, provedor, _, _) = Montar(habilitada: false);

        var recusa = await Assert.ThrowsAsync<BuscaRecusada>(() => servico.BuscarAsync(Pedido(), null, TestContext.Current.CancellationToken));

        Assert.Equal(ErroDaBusca.BuscaDesativada, recusa.Erro);
        Assert.Equal(0, provedor.Chamadas);
    }

    [Fact(DisplayName = "REQ-RECUP-006 Com 3 consultas hoje e limite de 3, a quarta gera limite_de_uso_atingido, sem embedding")]
    public async Task LimiteDiario()
    {
        var (servico, provedor, consumo, _) = Montar(limite: 3);
        for (var i = 0; i < 3; i++)
        {
            consumo.Registros.Enqueue(Consulta(Agora.AddHours(-1)));
        }

        var recusa = await Assert.ThrowsAsync<BuscaRecusada>(() => servico.BuscarAsync(Pedido(), null, TestContext.Current.CancellationToken));

        Assert.Equal(ErroDaBusca.LimiteDeUsoAtingido, recusa.Erro);
        Assert.Equal(0, provedor.Chamadas);
    }

    [Fact(DisplayName = "REQ-RECUP-006 Consultas de ontem não contam no limite do dia (UTC)")]
    public async Task LimiteSoDoDia()
    {
        var (servico, provedor, consumo, _) = Montar(limite: 3);
        for (var i = 0; i < 3; i++)
        {
            consumo.Registros.Enqueue(Consulta(Agora.AddDays(-1)));
        }

        var resultado = await servico.BuscarAsync(Pedido(), null, TestContext.Current.CancellationToken);

        Assert.Single(resultado.Trechos);
        Assert.Equal(1, provedor.Chamadas);
    }

    [Fact(DisplayName = "REQ-RECUP-002 Versão sem documento vigente volta vazia com versao_indisponivel, sem embedding")]
    public async Task VersaoIndisponivel()
    {
        var (servico, provedor, _, _) = Montar(haVigente: false);

        var resultado = await servico.BuscarAsync(Pedido(versao: "leiaute-99"), null, TestContext.Current.CancellationToken);

        Assert.Equal((ResultadoDaBusca.VersaoIndisponivel, 0), (resultado.Motivo, resultado.Trechos.Count));
        Assert.Equal(0, provedor.Chamadas);
    }

    [Fact(DisplayName = "REQ-RECUP-004 A consulta registra o consumo com a finalidade, a correlação e a execução")]
    public async Task ConsumoDaConsulta()
    {
        var (servico, _, consumo, documentos) = Montar();
        var execucao = Guid.NewGuid();

        await servico.BuscarAsync(Pedido(codigo: "K155", idExecucao: execucao.ToString()), "corr-busca", TestContext.Current.CancellationToken);

        var registro = Assert.Single(consumo.Registros);
        Assert.Equal((TipoDeChamada.Embedding, FinalidadeDaChamada.Consulta, "corr-busca", execucao), (registro.Tipo, registro.Finalidade, registro.Correlacao, registro.IdOperacao!.Value));
        Assert.Equal("K155", documentos.CodigoRecebido);
    }

    [Fact(DisplayName = "REQ-RECUP-004 Provedor indisponível vira falha_do_provedor, com cada tentativa no consumo")]
    public async Task ProvedorIndisponivel()
    {
        var (servico, provedor, consumo, _) = Montar(provedorIndisponivel: true);

        var recusa = await Assert.ThrowsAsync<BuscaRecusada>(() => servico.BuscarAsync(Pedido(), null, TestContext.Current.CancellationToken));

        Assert.Equal(ErroDaBusca.FalhaDoProvedor, recusa.Erro);
        Assert.Equal(2, provedor.Chamadas);
        Assert.Equal(2, consumo.Registros.Count(r => r.Finalidade == FinalidadeDaChamada.Consulta));
    }
}
