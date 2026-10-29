namespace Api.RAG.Tests.Infra;

/// <summary>Localiza arquivos do repositório a partir da pasta de saída dos testes.</summary>
internal static class Repositorio
{
    public static string Raiz { get; } = EncontrarRaiz();

    public static string Caminho(params string[] partes) => Path.Combine([Raiz, .. partes]);

    private static string EncontrarRaiz()
    {
        for (var pasta = new DirectoryInfo(AppContext.BaseDirectory); pasta is not null; pasta = pasta.Parent)
        {
            if (File.Exists(Path.Combine(pasta.FullName, "Agente-ECF.slnx")))
            {
                return pasta.FullName;
            }
        }

        throw new InvalidOperationException("Raiz do repositório (Agente-ECF.slnx) não encontrada.");
    }
}
