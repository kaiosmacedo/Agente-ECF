using System.Globalization;
using Api.Agente.Dominio.Execucoes;

namespace Api.Agente.Dominio.Leiaute;

/// <summary>Identificadores das regras estruturais da mudança 08 (D2). As de prefixo <c>REGRA_</c> são do próprio manual.</summary>
internal static class RegrasEstruturais
{
    public const string Delimitador = "EST-ARQ-001";
    public const string RegistroExistente = "EST-ARQ-002";
    public const string QuantidadeDeCampos = "EST-ARQ-003";
    public const string Obrigatorio = "EST-CAMPO-001";
    public const string Tipo = "EST-CAMPO-002";
    public const string Tamanho = "EST-CAMPO-003";
    public const string Decimais = "EST-CAMPO-004";
    public const string Data = "EST-CAMPO-005";
    public const string Valores = "EST-CAMPO-006";
    public const string Hierarquia = "EST-HIER-001";
    public const string RegistroObrigatorio = "EST-OCOR-001";
    public const string OcorrenciaMaxima = "EST-OCOR-002";
    public const string OcorrenciaUnitaria = "REGRA_OCORRENCIA_UNITARIA_ARQ";
    public const string OrdemDosBlocos = "EST-ORDEM-001";
    public const string ContagemDoBloco = "EST-TOT-001";
    public const string ContagemDoRegistro = "EST-TOT-002";
    public const string ContagemDoArquivo = "EST-TOT-003";
    public const string Duplicidade = "REGRA_DUPLICIDADE_DESPREZADA";
}

/// <summary>Limites da lista de achados (mudança 08, D4; REQ-ESTRUT-013).</summary>
internal sealed record ParametrosDaValidacao(int MaximoDeAchadosPorRegra = 20, int MaximoDeAchados = 200);

/// <summary>Uma validação que a etapa 4 não fez, com o motivo (REQ-ESTRUT-002, REQ-ESTRUT-011).</summary>
internal sealed record ValidacaoNaoRealizada(string Item, string Motivo);

/// <summary>Resultado da etapa 4: achados listados, os não listados por regra, indícios, o que não foi verificado e falhas.</summary>
internal sealed record ResultadoDaValidacaoEstrutural(
    IReadOnlyList<Achado> Achados,
    IReadOnlyDictionary<string, int> NaoListadosPorRegra,
    IReadOnlyList<string> Indicios,
    IReadOnlyList<ValidacaoNaoRealizada> NaoRealizadas,
    IReadOnlyList<string> RegrasComFalha);

/// <summary>
/// Validação estrutural determinística da ECF pelo catálogo do leiaute (mudança 08; REQ-ESTRUT-001 a 016). Lê o texto em
/// uma passada e devolve os mesmos achados, na mesma ordem, para o mesmo texto e a mesma versão do catálogo (REQ-ESTRUT-004).
/// O conteúdo dos campos é dado não confiável (P10): ele só é comparado, nunca interpretado.
/// </summary>
internal sealed class ValidadorEstrutural(CatalogoDoLeiaute catalogo, ParametrosDaValidacao parametros, Action<string>? antesDaRegra = null)
{
    public ResultadoDaValidacaoEstrutural Validar(string texto, string idDocumento) =>
        new Passada(catalogo, parametros, idDocumento, antesDaRegra).Executar(texto);

    /// <summary>O estado de uma validação: só o que hierarquia, ocorrência e contagens precisam (D6).</summary>
    private sealed class Passada(CatalogoDoLeiaute catalogo, ParametrosDaValidacao parametros, string idDocumento, Action<string>? antesDaRegra)
    {
        private const int NiveisMaximos = 16;
        private readonly List<(int Linha, string Regra, Func<Achado> Criar)> pendentes = [];
        private readonly Dictionary<string, int> porRegra = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> contagem = new(StringComparer.Ordinal);

        // Linhas com código fora do leiaute: já acusadas por EST-ARQ-002, mas existem no arquivo e contam no 9900.
        private readonly Dictionary<string, int> desconhecidos = new(StringComparer.Ordinal);
        private readonly Dictionary<char, int> contagemPorBloco = [];
        private readonly HashSet<char> blocosComDados = [];
        private readonly Dictionary<(long Pai, string Codigo), int> ocorrencias = [];
        private readonly HashSet<(long Pai, string Codigo, string Chave)> chaves = [];
        private readonly List<(int Linha, string Codigo, string Valor)> declaradosNoBloco = [];
        private readonly List<(int Linha, string Registro, string Valor)> declaradosNo9900 = [];
        private readonly List<string> indicios = [];
        private readonly HashSet<string> regrasComFalha = new(StringComparer.Ordinal);
        private readonly string?[] abertos = new string?[NiveisMaximos];
        private readonly long[] instancias = new long[NiveisMaximos];
        private long proximaInstancia;
        private int maiorBloco = -1;
        private string? blocoAtual;
        private int registrosLidos;
        private int linhasFisicas;
        private (int Linha, string Valor)? declaradoNo9999;

        public ResultadoDaValidacaoEstrutural Executar(string texto)
        {
            foreach (var (linha, conteudo) in Linhas(texto))
            {
                if (contagem.ContainsKey("9999"))
                {
                    // A assinatura fica depois do 9999 (p. 17): não é validada, só registrada como indício.
                    indicios.Add("Há conteúdo depois do Registro 9999, como a assinatura digital (manual, p. 17): indício de arquivo gerado pelo programa da ECF. Esse conteúdo não foi validado.");
                    break;
                }

                ValidarLinha(linha, conteudo);
            }

            Proteger(RegrasEstruturais.RegistroObrigatorio, VerificarObrigatorios);
            Proteger(RegrasEstruturais.ContagemDoBloco, VerificarContagensDosBlocos);
            Proteger(RegrasEstruturais.ContagemDoRegistro, VerificarContagensDo9900);
            Proteger(RegrasEstruturais.ContagemDoArquivo, VerificarContagemDoArquivo);
            RegistrarIndicios();
            return Montar();
        }

        /// <summary>Registros lógicos: o Y800 pode ocupar várias linhas até o campo <c>Y800FIM</c> (D2; p. 610 e 611).</summary>
        private IEnumerable<(int Linha, string Conteudo)> Linhas(string texto)
        {
            var fisicas = texto.Split('\n');
            var total = fisicas.Length > 0 && fisicas[^1].Length == 0 ? fisicas.Length - 1 : fisicas.Length;
            linhasFisicas = total;
            for (var i = 0; i < total; i++)
            {
                var inicio = i;
                var conteudo = fisicas[i].TrimEnd('\r');
                if (conteudo.StartsWith("|Y800|", StringComparison.Ordinal))
                {
                    while (!conteudo.EndsWith("|Y800FIM|", StringComparison.Ordinal) && i + 1 < total)
                    {
                        i++;
                        conteudo = string.Concat(conteudo, "\n", fisicas[i].TrimEnd('\r'));
                    }
                }

                yield return (inicio + 1, conteudo);
            }
        }

        private void ValidarLinha(int linha, string conteudo)
        {
            if (!Proteger(RegrasEstruturais.Delimitador, () => VerificarDelimitador(linha, conteudo)))
            {
                return;
            }

            var campos = conteudo[1..^1].Split('|');
            var codigo = campos[0];
            var registro = catalogo.Registro(codigo);
            if (registro is null)
            {
                Achar(RegrasEstruturais.RegistroExistente, null, Recortar(codigo), linha, null, codigo,
                    $"A linha {linha} tem o código de registro \"{Recortar(codigo)}\", que não existe no leiaute.",
                    "Use um dos registros da Tabela de Registros do manual, ou retire a linha.", SeveridadeDoAchado.Critica);
                desconhecidos[codigo] = desconhecidos.GetValueOrDefault(codigo) + 1;
                return;
            }

            registrosLidos++;
            contagem[codigo] = contagem.GetValueOrDefault(codigo) + 1;
            var bloco = codigo[0];
            contagemPorBloco[bloco] = contagemPorBloco.GetValueOrDefault(bloco) + 1;
            Proteger(RegrasEstruturais.OrdemDosBlocos, () => VerificarOrdem(linha, registro));
            Proteger(RegrasEstruturais.Hierarquia, () => VerificarHierarquia(linha, registro));
            Proteger(RegrasEstruturais.QuantidadeDeCampos, () => VerificarQuantidadeDeCampos(linha, registro, campos));
            for (var i = 0; i < registro.Campos.Count; i++)
            {
                var campo = registro.Campos[i];
                var valor = i < campos.Length ? campos[i] : null;
                Proteger(RegrasEstruturais.Tipo, () => VerificarCampo(linha, registro, campo, valor));
            }

            Proteger(RegrasEstruturais.Duplicidade, () => VerificarDuplicidade(linha, registro, campos));
            GuardarContagens(linha, codigo, campos);
        }

        private void VerificarDelimitador(int linha, string conteudo)
        {
            if (conteudo.Length >= 2 && conteudo[0] == '|' && conteudo[^1] == '|')
            {
                return;
            }

            Achar(RegrasEstruturais.Delimitador, null, "—", linha, null, conteudo,
                $"A linha {linha} não começa e termina com o caractere |, como todo registro.",
                "Cada linha deve começar com | e terminar com | depois do último campo (manual, p. 32).", SeveridadeDoAchado.Critica);
            throw new LinhaRecusada();
        }

        private void VerificarOrdem(int linha, RegistroDoCatalogo registro)
        {
            if (registro.Bloco == blocoAtual)
            {
                return;
            }

            blocoAtual = registro.Bloco;
            var indice = IndiceDoBloco(registro.Bloco);
            if (indice < maiorBloco)
            {
                Achar(RegrasEstruturais.OrdemDosBlocos, registro, registro.Codigo, linha, null, null,
                    $"O Bloco {registro.Bloco} aparece na linha {linha}, depois de um bloco que deve vir depois dele.",
                    $"Siga a ordem dos blocos do manual: {string.Join(", ", catalogo.OrdemDosBlocos)} (p. 42 e 58).", SeveridadeDoAchado.Critica);
            }

            maiorBloco = Math.Max(maiorBloco, indice);
        }

        private void VerificarHierarquia(int linha, RegistroDoCatalogo registro)
        {
            var nivel = Math.Min(registro.Nivel, NiveisMaximos - 1);
            if (nivel > 0 && registro.HierarquiaVerificada && registro.Pai is { } pai && abertos[nivel - 1] != pai)
            {
                Achar(RegrasEstruturais.Hierarquia, registro, registro.Codigo, linha, null, null,
                    $"O Registro {registro.Codigo}, na linha {linha}, precisa do Registro {pai} antes dele, e ele não está aberto.",
                    $"Informe o Registro {pai} antes do {registro.Codigo}, ou retire o {registro.Codigo} (manual, p. 59: todo filho precisa do pai).", SeveridadeDoAchado.Critica);
            }

            abertos[nivel] = registro.Codigo;
            instancias[nivel] = ++proximaInstancia;
            Array.Clear(abertos, nivel + 1, NiveisMaximos - nivel - 1);

            var chave = (nivel > 0 ? instancias[nivel - 1] : 0, registro.Codigo);
            var vezes = ocorrencias[chave] = ocorrencias.GetValueOrDefault(chave) + 1;
            Executar(RegrasEstruturais.OcorrenciaUnitaria, () =>
            {
                if (registro.TemRegra(RegrasEstruturais.OcorrenciaUnitaria) && contagem[registro.Codigo] == 2)
                {
                    Achar(RegrasEstruturais.OcorrenciaUnitaria, registro, registro.Codigo, linha, null, null,
                        $"O Registro {registro.Codigo} aparece de novo na linha {linha}; ele só pode ocorrer uma vez no arquivo.",
                        $"Deixe um único Registro {registro.Codigo} no arquivo.", SeveridadeDoAchado.Critica, new RegraDoManual(RegrasEstruturais.OcorrenciaUnitaria, TipoDaRegra.Erro));
                }
            });
            Executar(RegrasEstruturais.OcorrenciaMaxima, () =>
            {
                if (!registro.TemRegra(RegrasEstruturais.OcorrenciaUnitaria) && registro.OcorrenciaVerificada
                    && registro.OcorrenciaMaxima is { } maximo && vezes == maximo + 1)
                {
                    Achar(RegrasEstruturais.OcorrenciaMaxima, registro, registro.Codigo, linha, null, null,
                        string.Create(CultureInfo.InvariantCulture, $"O Registro {registro.Codigo} passa de {maximo} ocorrência(s) sob o mesmo registro pai, na linha {linha}."),
                        string.Create(CultureInfo.InvariantCulture, $"Informe no máximo {maximo} Registro(s) {registro.Codigo} para cada registro pai (ocorrência {registro.Ocorrencia})."), SeveridadeDoAchado.Critica);
                }
            });
        }

        private void VerificarQuantidadeDeCampos(int linha, RegistroDoCatalogo registro, string[] campos)
        {
            if (campos.Length > registro.Campos.Count)
            {
                Achar(RegrasEstruturais.QuantidadeDeCampos, registro, registro.Codigo, linha, null, null,
                    string.Create(CultureInfo.InvariantCulture, $"A linha {linha} tem {campos.Length} campos, e o Registro {registro.Codigo} tem {registro.Campos.Count}."),
                    string.Create(CultureInfo.InvariantCulture, $"Informe os {registro.Campos.Count} campos do Registro {registro.Codigo}, na ordem da tabela do registro (manual, p. 32)."), SeveridadeDoAchado.Critica);
            }
        }

        private void VerificarCampo(int linha, RegistroDoCatalogo registro, CampoDoCatalogo campo, string? valor)
        {
            var v = campo.Verificacoes;
            if (string.IsNullOrEmpty(valor))
            {
                Executar(RegrasEstruturais.Obrigatorio, () =>
                {
                    if (v.Obrigatorio == true && v.ObrigatorioDivergente)
                    {
                        // O manual obriga, e o programa da ECF aceita vazio: não recusa o arquivo (REQ-ESTRUT-016, decisão E10).
                        AcharNoCampo(RegrasEstruturais.Obrigatorio, registro, campo, linha, valor,
                            $"O campo {campo.Nome} do Registro {registro.Codigo} está vazio na linha {linha}: o manual o dá como obrigatório, e o programa da ECF o aceita vazio.",
                            $"Confira se {campo.Nome} deve ser preenchido neste caso.",
                            SeveridadeDoAchado.Media);
                    }
                    else if (v.Obrigatorio == true)
                    {
                        AcharNoCampo(RegrasEstruturais.Obrigatorio, registro, campo, linha, valor,
                            $"O campo {campo.Nome} do Registro {registro.Codigo} é obrigatório e está vazio na linha {linha}.",
                            $"Preencha o campo {campo.Nome}.");
                    }
                });
                return;
            }

            if (v.Tipo && !TipoValido(campo.TipoEfetivo, valor))
            {
                AcharNoCampo(RegrasEstruturais.Tipo, registro, campo, linha, valor,
                    $"O campo {campo.Nome} do Registro {registro.Codigo} tem caracteres fora do tipo {campo.TipoEfetivo} na linha {linha}.",
                    campo.TipoEfetivo switch
                    {
                        "N" => $"Preencha {campo.Nome} só com algarismos e vírgula decimal, sem separador de milhar (manual, p. 33 e 34).",
                        "NS" => $"Preencha {campo.Nome} com sinal opcional (+ ou -) e algarismos (manual, p. 33).",
                        _ => $"Tire de {campo.Nome} os caracteres de controle (manual, p. 33).",
                    });
                return;
            }

            Executar(RegrasEstruturais.Tamanho, () =>
            {
                var tamanho = campo.TipoEfetivo == "C" ? valor.Length : valor.Count(char.IsAsciiDigit);
                if (v.Valores?.PorPosicao is not null && v.TamanhoMaximo is { } exato && tamanho != exato)
                {
                    AcharNoCampo(RegrasEstruturais.Tamanho, registro, campo, linha, valor,
                        string.Create(CultureInfo.InvariantCulture, $"O campo {campo.Nome} do Registro {registro.Codigo} tem {tamanho} caracteres na linha {linha}, e precisa ter exatamente {exato}, um por posição."),
                        string.Create(CultureInfo.InvariantCulture, $"Preencha {campo.Nome} com {exato} caracteres, um para cada posição (manual, p. {campo.Pagina})."));
                }
                else if (v.TamanhoMaximo is { } maximo && tamanho > maximo)
                {
                    AcharNoCampo(RegrasEstruturais.Tamanho, registro, campo, linha, valor,
                        string.Create(CultureInfo.InvariantCulture, $"O campo {campo.Nome} do Registro {registro.Codigo} tem {tamanho} caracteres na linha {linha}, acima do máximo de {maximo}."),
                        string.Create(CultureInfo.InvariantCulture, $"Use no máximo {maximo} caracteres em {campo.Nome}."));
                }
            });
            Executar(RegrasEstruturais.Decimais, () =>
            {
                var virgula = valor.IndexOf(',', StringComparison.Ordinal);
                if (v.DecimaisMaximos is { } decimais && virgula >= 0 && valor.Length - virgula - 1 > decimais)
                {
                    AcharNoCampo(RegrasEstruturais.Decimais, registro, campo, linha, valor,
                        string.Create(CultureInfo.InvariantCulture, $"O campo {campo.Nome} do Registro {registro.Codigo} tem mais casas decimais que as {decimais} permitidas, na linha {linha}."),
                        decimais == 0
                            ? $"Informe {campo.Nome} sem casas decimais (manual, p. 59)."
                            : string.Create(CultureInfo.InvariantCulture, $"Informe {campo.Nome} com no máximo {decimais} casas decimais, separadas por vírgula (manual, p. 34)."));
                }
            });
            Executar(RegrasEstruturais.Data, () =>
            {
                if (v.Data && !DateOnly.TryParseExact(valor, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    AcharNoCampo(RegrasEstruturais.Data, registro, campo, linha, valor,
                        $"O campo {campo.Nome} do Registro {registro.Codigo} não é uma data válida no formato ddmmaaaa, na linha {linha}.",
                        $"Informe {campo.Nome} como ddmmaaaa, com uma data que existe no calendário (manual, p. 34).");
                }
            });
            Executar(RegrasEstruturais.Valores, () =>
            {
                if (v.Valores is { } valores && !ValorValido(valores, valor))
                {
                    AcharNoCampo(RegrasEstruturais.Valores, registro, campo, linha, valor,
                        $"O campo {campo.Nome} do Registro {registro.Codigo} tem um valor fora dos valores válidos, na linha {linha}.",
                        valores.PorPosicao is null
                            ? $"Preencha {campo.Nome} com um dos valores válidos: {campo.ValoresValidos}."
                            : $"Preencha cada posição de {campo.Nome} com um dos valores válidos: {campo.ValoresValidos}.");
                }
            });
        }

        private void VerificarDuplicidade(int linha, RegistroDoCatalogo registro, string[] campos)
        {
            if (!registro.TemRegra(RegrasEstruturais.Duplicidade) || registro.Chave.Count == 0)
            {
                return;
            }

            var indices = registro.Chave.Select(nome => registro.Campos.ToList().FindIndex(campo => campo.Nome == nome)).ToList();
            if (indices.Any(indice => indice < 0))
            {
                return;
            }

            var nivel = Math.Min(registro.Nivel, NiveisMaximos - 1);
            var valor = string.Join('|', indices.Select(indice => indice < campos.Length ? campos[indice] : string.Empty));
            if (!chaves.Add((nivel > 0 ? instancias[nivel - 1] : 0, registro.Codigo, valor)))
            {
                Achar(RegrasEstruturais.Duplicidade, registro, registro.Codigo, linha, null, valor,
                    $"O Registro {registro.Codigo} repete, na linha {linha}, a chave {string.Join(" + ", registro.Chave)} de um registro anterior sob o mesmo pai.",
                    "Retire a linha repetida: o programa da ECF despreza o registro duplicado na importação.", SeveridadeDoAchado.Baixa,
                    new RegraDoManual(RegrasEstruturais.Duplicidade, TipoDaRegra.Aviso));
            }
        }

        private void GuardarContagens(int linha, string codigo, string[] campos)
        {
            if (codigo.EndsWith("001", StringComparison.Ordinal) && campos.Length > 1 && campos[1] == "0")
            {
                blocosComDados.Add(codigo[0]);
            }
            else if (codigo.EndsWith("990", StringComparison.Ordinal) && campos.Length > 1)
            {
                declaradosNoBloco.Add((linha, codigo, campos[1]));
            }
            else if (codigo == "9900" && campos.Length > 2)
            {
                declaradosNo9900.Add((linha, campos[1], campos[2]));
            }
            else if (codigo == "9999" && campos.Length > 1)
            {
                declaradoNo9999 = (linha, campos[1]);
            }
        }

        private void VerificarObrigatorios()
        {
            foreach (var registro in catalogo.Registros.Where(registro => registro.ObrigatorioNaEntradaENaSaida && !contagem.ContainsKey(registro.Codigo)))
            {
                if (registro.ObrigatorioDivergente)
                {
                    // O programa da ECF preenche esses registros sozinho (p. 41): a falta não recusa o arquivo (REQ-ESTRUT-016, decisão E10).
                    Achar(RegrasEstruturais.RegistroObrigatorio, registro, registro.Codigo, null, null, null,
                        $"O arquivo não tem o Registro {registro.Codigo} ({registro.Nome}): o manual o dá como obrigatório, e o programa da ECF o preenche sozinho na importação.",
                        $"O programa não exige correção; para seguir o manual, inclua o Registro {registro.Codigo}.", SeveridadeDoAchado.Media);
                    continue;
                }

                Achar(RegrasEstruturais.RegistroObrigatorio, registro, registro.Codigo, null, null, null,
                    $"O arquivo não tem o Registro {registro.Codigo} ({registro.Nome}), obrigatório na importação e na transmissão.",
                    $"Inclua o Registro {registro.Codigo}. Se o arquivo terminou antes da hora, envie o arquivo inteiro.", SeveridadeDoAchado.Critica);
            }
        }

        /// <summary>A contagem de cada bloco inclui a abertura e o encerramento (p. 42: abertura, dados e encerramento; p. 108).</summary>
        private void VerificarContagensDosBlocos()
        {
            foreach (var (linha, codigo, valor) in declaradosNoBloco)
            {
                var real = contagemPorBloco.GetValueOrDefault(codigo[0]);
                if (int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var declarado) && declarado != real)
                {
                    AcharContagem(codigo == "9990" ? RegrasEstruturais.ContagemDoArquivo : RegrasEstruturais.ContagemDoBloco, codigo, linha, valor,
                        string.Create(CultureInfo.InvariantCulture, $"O Registro {codigo} declara {declarado} registros no Bloco {codigo[0]}, e o bloco tem {real}, contando a abertura e o encerramento."),
                        string.Create(CultureInfo.InvariantCulture, $"Informe {real} no campo 2 do Registro {codigo}."),
                        BlocoImportado(codigo[0]) ? SeveridadeDoAchado.Critica : SeveridadeDoAchado.Media);
                }
            }
        }

        /// <summary>
        /// Contagem errada num bloco que a importação lê impede a importação (p. 38; REQ-ESTRUT-015, decisão E06). O Bloco 0 sempre é lido, e os
        /// demais só com dados (IND_DAD igual a 0). Os blocos C e E não são importados (p. 41), o Bloco 9 encerra o arquivo, e o programa
        /// não confere a contagem do S990, que o manual nem cita (decisão E10).
        /// </summary>
        private bool BlocoImportado(char bloco) => bloco == '0' || (blocosComDados.Contains(bloco) && bloco is not ('C' or 'E' or 'S' or '9'));

        /// <summary>O 9900 traz cada registro do arquivo, inclusive ele mesmo (p. 616).</summary>
        private void VerificarContagensDo9900()
        {
            if (declaradosNo9900.Count == 0)
            {
                return;
            }

            foreach (var (linha, registro, valor) in declaradosNo9900)
            {
                var real = contagem.GetValueOrDefault(registro) + desconhecidos.GetValueOrDefault(registro);
                if (int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var declarado) && declarado != real)
                {
                    AcharContagem(RegrasEstruturais.ContagemDoRegistro, "9900", linha, valor,
                        string.Create(CultureInfo.InvariantCulture, $"O Registro 9900 declara {declarado} ocorrência(s) do Registro {Recortar(registro)}, e o arquivo tem {real}."),
                        string.Create(CultureInfo.InvariantCulture, $"Informe {real} em QTD_REG_BLC para o Registro {Recortar(registro)}."));
                }
            }

            var declarados = declaradosNo9900.Select(item => item.Registro).ToHashSet(StringComparer.Ordinal);
            foreach (var codigo in contagem.Keys.Where(codigo => !declarados.Contains(codigo)).Order(StringComparer.Ordinal))
            {
                AcharContagem(RegrasEstruturais.ContagemDoRegistro, "9900", null, codigo,
                    $"O Registro {codigo} aparece no arquivo, mas não tem linha no Registro 9900.",
                    $"Inclua uma linha do Registro 9900 para o Registro {codigo}, com a quantidade de ocorrências.");
            }
        }

        /// <summary>QTD_LIN conta as linhas do arquivo, com o próprio 9999 (p. 41 e 619); vale a contagem física ou a lógica.</summary>
        private void VerificarContagemDoArquivo()
        {
            if (declaradoNo9999 is not { } ultimo)
            {
                return;
            }

            var (linha, valor) = ultimo;
            if (!int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var declarado)
                || declarado == registrosLidos || declarado == linhasFisicas)
            {
                return;
            }

            AcharContagem(RegrasEstruturais.ContagemDoArquivo, "9999", linha, valor,
                string.Create(CultureInfo.InvariantCulture, $"O Registro 9999 declara {declarado} linhas, e o arquivo tem {registrosLidos} registros até o 9999, inclusive."),
                string.Create(CultureInfo.InvariantCulture, $"Informe {registrosLidos} em QTD_LIN do Registro 9999."));
        }

        private void RegistrarIndicios()
        {
            foreach (var registro in catalogo.Registros.Where(registro => contagem.ContainsKey(registro.Codigo)))
            {
                if (registro.Entrada == "N")
                {
                    indicios.Add($"O Registro {registro.Codigo} não deve existir no arquivo de importação (Tabela de Registros, p. 43 a 58; p. {registro.Pagina}): indício de arquivo gerado pelo programa da ECF.");
                }
                else if (registro.Codigo is "9100" or "Y750")
                {
                    indicios.Add($"O Registro {registro.Codigo} é preenchido pelo próprio programa da ECF (p. {registro.Pagina}): indício de arquivo gerado pelo programa.");
                }
            }
        }

        private ResultadoDaValidacaoEstrutural Montar()
        {
            var ordenados = pendentes.OrderBy(item => item.Linha).ThenBy(item => item.Regra, StringComparer.Ordinal).ToList();
            var achados = new List<Achado>();
            var naoListados = new Dictionary<string, int>(StringComparer.Ordinal);
            var listadosPorRegra = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var (_, regra, criar) in ordenados)
            {
                var listados = listadosPorRegra.GetValueOrDefault(regra);
                if (listados >= parametros.MaximoDeAchadosPorRegra || achados.Count >= parametros.MaximoDeAchados)
                {
                    naoListados[regra] = naoListados.GetValueOrDefault(regra) + 1;
                    continue;
                }

                listadosPorRegra[regra] = listados + 1;
                achados.Add(criar());
            }

            var numerados = achados.Select((achado, indice) => achado with { IdAchado = string.Create(CultureInfo.InvariantCulture, $"est-{indice + 1:0000}") }).ToList();
            return new ResultadoDaValidacaoEstrutural(numerados, naoListados, indicios, NaoRealizadas(), [.. regrasComFalha.Order(StringComparer.Ordinal)]);
        }

        private List<ValidacaoNaoRealizada> NaoRealizadas()
        {
            var divergentes = catalogo.Registros.Where(registro => !registro.OcorrenciaVerificada).Select(registro => registro.Codigo).ToList();
            return
            [
                new("Regras REGRA_ de conteúdo do manual: valores, cruzamentos entre registros, cálculos, preenchimento e existência condicionados", "Ficam para a mudança 09, de regras de negócio."),
                new("Dígito verificador de CNPJ, CPF e recibo (REGRA_VALIDA_CNPJ, REGRA_VALIDA_CPF_CNPJ e outras)", "O algoritmo não está no manual; adotar um sem fonte violaria P01."),
                new("REGRA_LEIAUTE_VIGENTE e REGRA_SEM_LEIAUTE", "Dependem de tabelas que não estão no manual."),
                new("Valores de campos que dependem de tabela externa ou dinâmica", "As tabelas ficam fora do manual, como a planilha de tabelas dinâmicas (p. 620)."),
                new("Valores de campos que remetem a outro registro: M305, M310, M355, M360, M500 e M510", "A coluna de valores válidos cita o campo de outro registro, como [M010.COD_CTA_B] (p. 251). É cruzamento e fica para a mudança 09."),
                new("Tamanho exato dos campos", "O manual fala em tamanho exato no caso geral (p. 59), e o programa da ECF só o exige em alguns campos, sem lista publicada. Foram conferidos o máximo e o tamanho exato dos campos preenchidos por posição."),
                new(
                    string.Create(CultureInfo.InvariantCulture, $"Ocorrência mínima e máxima de {divergentes.Count} registros: {string.Join(", ", divergentes)}"),
                    "O cabeçalho de cada registro (página em docs/catalogo/ecf-leiaute-12.md) e a Tabela de Registros (p. 43 a 58) divergem (REQ-ESTRUT-011)."),
                new("Hierarquia dos registros X451 e Y650", "O nível do cabeçalho e o da Tabela de Registros divergem (REQ-ESTRUT-011)."),
                new("Ordem entre registros irmãos", "O manual só diz que a ordem é sequencial (p. 58); foram conferidas a ordem dos blocos e a hierarquia."),
                new("Fim de linha CR LF", "O texto que chega pela Sofia pode ter as quebras de linha trocadas."),
                new("Campos do tipo D (X280.VIG_INI e X280.VIG_FIM)", "O tipo não está definido no manual (p. 33)."),
                new("Duplicidade no Registro V100", "A chave do registro está vazia no cabeçalho (p. 424)."),
            ];
        }

        private void AcharNoCampo(
            string regra, RegistroDoCatalogo registro, CampoDoCatalogo campo, int linha, string? valor, string descricao, string orientacao, SeveridadeDoAchado severidade = SeveridadeDoAchado.Critica) =>
            Achar(regra, registro, registro.Codigo, linha, campo, valor, descricao, orientacao, severidade);

        private void AcharContagem(string regra, string codigo, int? linha, string valor, string descricao, string orientacao, SeveridadeDoAchado severidade = SeveridadeDoAchado.Media) =>
            Achar(regra, catalogo.Registro(codigo), codigo, linha, null, valor, descricao, orientacao, severidade);

        private void Achar(
            string regra, RegistroDoCatalogo? registro, string codigo, int? linha, CampoDoCatalogo? campo, string? valor,
            string descricao, string orientacao, SeveridadeDoAchado severidade, RegraDoManual? doManual = null)
        {
            var pagina = campo?.Pagina ?? registro?.Pagina;
            var localNoRegistro = $"Registro {codigo}{(campo is null ? string.Empty : $", campo {campo.Numero} {campo.Nome}")}";
            var local = pagina switch
            {
                null => "seção 2.2, p. 32",
                _ when registro is { SoNoPrograma: true } => $"{catalogo.Fonte.Programa}, {localNoRegistro}",
                _ => string.Create(CultureInfo.InvariantCulture, $"p. {pagina}, {localNoRegistro}"),
            };
            var fundamento = campo is null ? $"{regra} {codigo}" : $"{regra} {codigo}.{campo.Nome}";
            pendentes.Add((linha ?? int.MaxValue, regra, () => Achado.Criar(
                "est",
                CategoriaDoAchado.ErroEstrutural,
                OrigemDoAchado.Validador,
                fundamento,
                descricao,
                severidade,
                [new RegistroEnvolvido(idDocumento, codigo, linha, campo?.Nome, valor)],
                linha is null ? [] : [string.Create(CultureInfo.InvariantCulture, $"Linha {linha}")],
                doManual,
                new FonteDoAchado(catalogo.Fonte.Documento, catalogo.Fonte.Versao, local),
                orientacao)));
            porRegra[regra] = porRegra.GetValueOrDefault(regra) + 1;
        }

        /// <summary>Uma regra que lança exceção vira falha técnica e não atribui erro ao documento (REQ-ESTRUT-006; D8).</summary>
        private bool Proteger(string regra, Action verificar)
        {
            try
            {
                Executar(regra, verificar);
                return true;
            }
            catch (LinhaRecusada)
            {
                return false;
            }
        }

        private void Executar(string regra, Action verificar)
        {
            if (regrasComFalha.Contains(regra))
            {
                return;
            }

            try
            {
                antesDaRegra?.Invoke(regra);
                verificar();
            }
            catch (Exception erro) when (erro is not (LinhaRecusada or OutOfMemoryException))
            {
                regrasComFalha.Add(regra);
            }
        }

        private int IndiceDoBloco(string bloco)
        {
            for (var i = 0; i < catalogo.OrdemDosBlocos.Count; i++)
            {
                if (catalogo.OrdemDosBlocos[i] == bloco)
                {
                    return i;
                }
            }

            return int.MaxValue;
        }

        private static bool TipoValido(string tipo, string valor) => tipo switch
        {
            "N" => Numero(valor),
            "NS" => Numero(valor is ['+' or '-', .. var resto] ? resto : valor),
            _ => !valor.Any(char.IsControl),
        };

        private static bool Numero(string valor) =>
            valor.Length > 0 && valor.All(c => char.IsAsciiDigit(c) || c == ',') && valor.Count(c => c == ',') <= 1 && valor.Any(char.IsAsciiDigit);

        private static bool ValorValido(ValoresDoCampo valores, string valor)
        {
            if (valores.PorPosicao is { } porPosicao)
            {
                return valor.All(caractere => porPosicao.Contains(caractere.ToString(), StringComparer.Ordinal));
            }

            if (valores.Lista is { } lista)
            {
                return lista.Contains(valor, StringComparer.Ordinal);
            }

            var cultura = CultureInfo.GetCultureInfo("pt-BR");
            return valores.Intervalo is [var minimo, var maximo]
                && decimal.TryParse(valor, NumberStyles.AllowDecimalPoint, cultura, out var numero)
                && numero >= decimal.Parse(minimo, cultura)
                && numero <= decimal.Parse(maximo, cultura);
        }

        private static string Recortar(string codigo) => codigo.Length > 10 ? codigo[..10] : codigo;
    }

    /// <summary>A linha não tem a forma de um registro; as demais regras não se aplicam a ela.</summary>
    private sealed class LinhaRecusada : Exception
    {
    }
}
