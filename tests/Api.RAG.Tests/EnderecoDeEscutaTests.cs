using Api.RAG.Infraestrutura.Hospedagem;

namespace Api.RAG.Tests;

public sealed class EnderecoDeEscutaTests
{
    [Fact(DisplayName = "REQ-ENTREGA-002 Sem PORT, escuta em todas as interfaces na porta 8080")]
    public void PortaPadrao() => Assert.Equal("http://0.0.0.0:8080", EnderecoDeEscuta.Resolver(null));

    [Fact(DisplayName = "REQ-ENTREGA-002 Com PORT=3000, escuta na porta informada")]
    public void PortaInformada() => Assert.Equal("http://0.0.0.0:3000", EnderecoDeEscuta.Resolver("3000"));

    [Theory(DisplayName = "REQ-ENTREGA-002 PORT inválida impede a inicialização sem revelar o valor")]
    [InlineData("porta-x")]
    [InlineData("70000")]
    public void PortaInvalida(string porta)
    {
        var erro = Assert.Throws<InvalidOperationException>(() => EnderecoDeEscuta.Resolver(porta));

        Assert.Contains("PORT", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(porta, erro.Message, StringComparison.Ordinal);
    }
}
