"""Gera as ECF sintéticas do gabarito no programa da ECF (mudança 08, D10; tarefa 6.1).

    python -I gerar-gabarito.py <ecf-estrutural.txt> <pasta-de-saída>

Cada arquivo planta um único erro de estrutura na ECF estruturalmente válida; as contagens de encerramento (x990, 9900,
9990 e 9999) são recalculadas, exceto nos casos em que a contagem é o próprio erro. A planilha `erros-esperados.md` diz,
para cada arquivo, as regras do agente que devem acusar o erro: a do erro plantado e, quando houver, as que acusam a
consequência dele, como o registro que deixa de ser lido. O teste do gabarito confere a planilha pelo serviço de
análise, da recepção à validação estrutural. Só dados sintéticos (SQ-10).
"""
import os
import re
import sys

J001 = "|J001|0|"
J051 = "|J051||A|"  # COD_CTA_REF preenchido; filho do J050, que não existe no arquivo
J990 = "|J990|0|"  # a quantidade é recalculada


def recontar(linhas, sem_9999=False):
    """Refaz os encerramentos de bloco, o 9900, o 9990 e o 9999 (manual, p. 108, 616, 618 e 619).

    Um item pode trazer várias linhas, separadas por CRLF, e cada uma conta. Com `sem_9999`, o arquivo fica sem o
    Registro 9999, e nenhuma contagem o inclui, para que a falta dele seja o único erro.
    """
    linhas = [parte for item in linhas for parte in item.split("\r\n")]
    corpo = [l for l in linhas if not (l.startswith("|9900|") or l.startswith("|9990|") or l.startswith("|9999|"))]
    for i, linha in enumerate(corpo):
        codigo = linha.split("|")[1] if linha.startswith("|") else ""
        if codigo.endswith("990") and codigo[0] != "9":
            corpo[i] = f"|{codigo}|{sum(1 for l in corpo if l.startswith('|' + codigo[0]))}|"
    codigos = []
    for linha in corpo:
        codigo = linha.split("|")[1] if linha.startswith("|") else ""
        if codigo and codigo not in codigos:
            codigos.append(codigo)
    encerramento = ["9900", "9990"] + ([] if sem_9999 else ["9999"])
    codigos += encerramento
    contagem = {c: sum(1 for l in corpo if l.startswith(f"|{c}|")) for c in codigos}
    contagem["9900"], contagem["9990"] = len(codigos), 1
    if not sem_9999:
        contagem["9999"] = 1
    nove = [f"|9900|{c}|{contagem[c]}|||" for c in codigos]
    bloco9 = sum(1 for l in corpo if l.startswith("|9")) + len(nove) + len(encerramento) - 1
    total = len(corpo) + len(nove) + len(encerramento) - 1
    return corpo + nove + [f"|9990|{bloco9}|"] + ([] if sem_9999 else [f"|9999|{total}|"])


def campo_do_0000(linhas, campo, valor):
    campos = linhas[0].split("|")
    campos[campo] = valor
    return ["|".join(campos)] + linhas[1:]


def trocar(linhas, inicio, nova):
    return [nova if l.startswith(inicio) else l for l in linhas]


def celula(texto):
    """O | é o separador das tabelas em Markdown."""
    return texto.replace("|", "\\|")


def preenchido_antes(caminho):
    """A última coluna da planilha vem do programa da ECF (D10): gerar de novo não pode apagá-la."""
    if not os.path.exists(caminho):
        return {}
    colunas = {}
    for linha in open(caminho, encoding="utf-8"):
        if linha.startswith("| `"):
            celulas = re.split(r"(?<!\\)\|", linha.strip())
            colunas[celulas[1].strip().strip("`")] = celulas[-2].strip()
    return colunas


def main():
    base = open(sys.argv[1], encoding="latin-1", newline="").read().split("\r\n")
    base = [l for l in base if l]
    saida = sys.argv[2]
    os.makedirs(saida, exist_ok=True)
    # (arquivo, erro plantado, regras do agente: a do erro e as da consequência, linha do erro, linhas)
    casos = [
        ("00-valida", "Nenhum erro de estrutura.", [], None, recontar(base)),
        ("01-valor-fora-da-lista", "IND_SIT_INI_PER do 0000 igual a 9 (p. 63).", ["EST-CAMPO-006"], 1, recontar(campo_do_0000(base, 6, "9"))),
        ("02-data-invalida", "DT_INI do 0000 igual a 31022025, data que não existe (p. 34 e 63). A DT_FIN não serve: ela identifica o leiaute na etapa 3, que pararia antes da validação estrutural.",
         ["EST-CAMPO-005"], 1, recontar(campo_do_0000(base, 10, "31022025"))),
        ("03-registro-inexistente", "Linha |Z999|1| no Bloco 0 (p. 43 a 58).", ["EST-ARQ-002"], 7, recontar(base[:6] + ["|Z999|1|"] + base[6:])),
        ("04-campos-a-mais", "Registro 0001 com um campo a mais (p. 70).", ["EST-ARQ-003"], 2, recontar(trocar(base, "|0001|", "|0001|0|X|"))),
        ("05-sem-delimitador", "Registro C001 sem o | inicial (p. 32). Consequência: a linha não é lida como C001, que falta no arquivo.",
         ["EST-ARQ-001", "EST-OCOR-001"], 7, recontar(trocar(base, "|C001|", "C001|1|"))),
        ("06-sem-9999", "Arquivo sem o Registro 9999, com as contagens do Bloco 9 feitas sem ele (p. 58 e 619).", ["EST-OCOR-001"], None, recontar(base, sem_9999=True)),
        ("07-abertura-repetida", "Registro 0001 duas vezes, com as contagens refeitas (REGRA_OCORRENCIA_UNITARIA_ARQ, p. 70).",
         ["REGRA_OCORRENCIA_UNITARIA_ARQ"], 3, recontar(trocar(base, "|0001|", "|0001|0|\r\n|0001|0|"))),
        ("08-total-de-linhas", "9999.QTD_LIN maior que o total (p. 619).", ["EST-TOT-003"], None, (lambda l: l[:-1] + [f"|9999|{len(l) + 5}|"])(recontar(base))),
        ("09-contagem-do-bloco", "0990.QTD_LIN errado (p. 108).", ["EST-TOT-001"], 6, trocar(recontar(base), "|0990|", "|0990|9|")),
        ("10-contagem-do-9900", "9900 do 0030 com quantidade 2 (p. 616).", ["EST-TOT-002"], None, trocar(recontar(base), "|9900|0030|", "|9900|0030|2|||")),
        ("11-filho-sem-pai", "Bloco J com J001, J051 e J990, sem o J050, pai do J051 (p. 59 e 136).", ["EST-HIER-001"], 11,
         recontar(trocar(base, "|9001|", f"{J001}\r\n{J051}\r\n{J990}\r\n|9001|0|"))),
    ]
    tabela = ["# Gabarito da validação estrutural", "",
              "ECF sintéticas da mudança 08 (D10). Cada arquivo planta um único erro de estrutura na ECF válida; o programa da ECF (PGE) é rodado com cada uma, sem assinar nem transmitir, e o que ele aponta é comparado com as regras do agente. A coluna das regras traz a do erro plantado e, quando houver, as da consequência dele; o teste `Gabarito` confere essa coluna pelo serviço de análise. Gerado por `.github/scripts/catalogo/gerar-gabarito.py`; a última coluna vem do programa, e o gerador a preserva. Rodado em 2026-11-08 no PGE 12.2.8, descritor 12003.1. A comparação e as divergências estão no `design.md` da mudança.", "",
              "| Arquivo | Erro plantado | Regras do agente | Linha | O que o PGE apontou |", "| --- | --- | --- | --- | --- |"]
    planilha = os.path.join(saida, "erros-esperados.md")
    apontado = preenchido_antes(planilha)
    for nome, descricao, regras, linha, linhas in casos:
        texto = "\r\n".join(l for item in linhas for l in item.split("\r\n")) + "\r\n"
        open(os.path.join(saida, nome + ".txt"), "w", encoding="latin-1", newline="").write(texto)
        coluna = ", ".join(f"`{regra}`" for regra in regras) or "nenhuma"
        tabela.append(f"| `{nome}.txt` | {celula(descricao)} | {coluna} | {linha or '—'} | {apontado.get(nome + '.txt', '(a preencher)')} |")
    open(planilha, "w", encoding="utf-8", newline="\n").write("\n".join(tabela) + "\n")
    print(f"{len(casos)} arquivos em {saida}.")


if __name__ == "__main__":
    main()
