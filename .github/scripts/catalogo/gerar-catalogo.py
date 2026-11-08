"""Gera o catálogo estrutural do Leiaute 12 da ECF a partir da versão em Word do manual (mudança 08, D1 e D2).

    python -I gerar-catalogo.py --word manual.docx --texto-pdf manual.txt --json catalogo.json --md catalogo.md

O Word dá as tabelas (a extração do PDF desalinha colunas); o texto do PDF, extraído com `pdftotext -layout`, dá as
páginas. Só a biblioteca padrão. Use `gerar-catalogo.sh`, que baixa os originais e confere os hashes.
Correções vão neste gerador, nunca no JSON à mão.
"""
import argparse
import json
import re
import sys
import zipfile
import xml.etree.ElementTree as ET

W = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"
VERSAO_DO_CATALOGO = "ecf-l12-1.2.0"
TIPOS = ("C", "N", "NS")
# Campo preenchido posição a posição, como "no formato XXXX onde X é igual a" (p. 73 e 74): a lista vale para cada caractere.
POR_POSICAO = re.compile(r"formato\s+(X{2,})\s")
# Tabelas externas ou dinâmicas citadas na coluna de valores válidos: ficam sem verificação (E04).
EXTERNA = re.compile(r"tabela|Código da Natureza Jurídica|CNAE Fiscal|Sigla da UF|Código do Município|Códigos existentes no registro", re.I)
REGRA = re.compile(r"REGRA_[A-Z0-9_]+")


def texto_do_paragrafo(p):
    partes = []
    for el in p.iter():
        if el.tag == W + "t" and el.text:
            partes.append(el.text)
        elif el.tag == W + "tab":
            partes.append("\t")
        elif el.tag == W + "br":
            partes.append("\n")
    return "".join(partes)


def estilo(p):
    propriedades = p.find(W + "pPr")
    marca = propriedades.find(W + "pStyle") if propriedades is not None else None
    return marca.get(W + "val") if marca is not None else None


def blocos_do_word(caminho):
    corpo = ET.fromstring(zipfile.ZipFile(caminho).read("word/document.xml")).find(W + "body")
    blocos = []
    for el in corpo:
        if el.tag == W + "p":
            blocos.append({"k": "p", "s": estilo(el), "t": texto_do_paragrafo(el)})
        elif el.tag == W + "tbl":
            linhas = [[" ".join(texto_do_paragrafo(p) for p in tc.iter(W + "p")).strip() for tc in tr.findall(W + "tc")] for tr in el.findall(W + "tr")]
            blocos.append({"k": "tbl", "rows": linhas})
    return blocos


def limpar(texto):
    return re.sub(r"\s+", " ", texto or "").strip()


def paginas_do_pdf(caminho):
    return open(caminho, encoding="utf-8", errors="replace").read().split("\f")


def pagina_do_registro(paginas, codigo):
    """Página do PDF com a linha "Nível Hierárquico" do cabeçalho do registro, depois de "REGISTRO CODIGO:"."""
    for i in range(60, len(paginas)):
        achado = re.search(r"REGISTRO\s+%s\s*:" % codigo, paginas[i])
        if not achado:
            continue
        for j in range(i, min(i + 3, len(paginas))):
            trecho = paginas[j][achado.start():] if j == i else paginas[j]
            if re.search(r"N[íi]vel\s+Hier", trecho):
                return j + 1
        raise SystemExit(f"O cabeçalho do Registro {codigo} não foi achado no texto do PDF: confira se ele foi extraído em UTF-8.")
    raise SystemExit(f"Página do Registro {codigo} não encontrada no texto do PDF.")


def ocorrencia(valor):
    """1:1, 0:N, [1;N] e afins viram (mínimo, máximo); máximo None é sem limite."""
    numeros = re.findall(r"\d+|N", (valor or "").replace(" ", ""))
    if len(numeros) != 2:
        return None
    minimo, maximo = numeros
    return int(minimo), None if maximo == "N" else int(maximo)


def valores_validos(bruto):
    """Lista ou intervalo da coluna "Valores Válidos" (manual, p. 37); None se não for autocontida."""
    valor = limpar(bruto)
    if valor in ("", "-") or EXTERNA.search(valor):
        return None
    dentro = re.fullmatch(r"\[(.*)\]", valor)
    if not dentro:
        return None
    intervalo = re.fullmatch(r"\s*(-?\d+(?:,\d+)?)\s+a\s+(-?\d+(?:,\d+)?)\s*", dentro.group(1))
    if intervalo:
        return {"intervalo": [intervalo.group(1), intervalo.group(2)]}
    # Só listas de valores literais, separados por ponto e vírgula ou vírgula; o resto fica sem verificação.
    itens = [item.strip().strip("“”\"'").strip() for item in re.split(r"[;,]", dentro.group(1))]
    return {"lista": itens} if itens and all(re.fullmatch(r"[A-Za-z0-9._/-]+", item) for item in itens) else None


def campo(linha, pagina):
    numero, nome, descricao, tipo, tamanho, decimal, valores, obrigatorio = [limpar(celula) for celula in linha]
    tipo_verificado = tipo in TIPOS
    if re.fullmatch(r"\d+", tamanho):
        tamanho_maximo = int(tamanho)
    elif tamanho == "-" and tipo == "C":
        tamanho_maximo = 255  # "C" sem tamanho tem máximo de 255 (p. 33 e 59)
    else:
        tamanho_maximo = None  # "N" sem tamanho não tem limite (p. 59); textos fora do padrão ficam sem verificação
    if tipo in ("N", "NS"):
        decimais = int(decimal) if re.fullmatch(r"\d+", decimal) else 0 if decimal == "-" else None
    else:
        decimais = None
    marca = obrigatorio.lower()
    verificacao_de_valores = valores_validos(valores)
    posicoes = POR_POSICAO.search(descricao)
    if verificacao_de_valores and "lista" in verificacao_de_valores and posicoes and len(posicoes.group(1)) == tamanho_maximo:
        verificacao_de_valores = {"porPosicao": verificacao_de_valores["lista"]}
    # Valor da lista mais longo que a coluna Tamanho, como o 10 de FORMA_TRIB (p. 71 e 72): vale a lista, e a divergência fica no catálogo.
    lista = (verificacao_de_valores or {}).get("lista") or []
    if tamanho_maximo is not None and lista and max(map(len, lista)) > tamanho_maximo:
        tamanho_maximo = max(map(len, lista))
    return {
        "numero": int(numero),
        "nome": nome,
        "tipo": tipo,
        "tamanho": tamanho,
        "decimal": decimal,
        "valoresValidos": valores,
        "obrigatorio": obrigatorio,
        "pagina": pagina,
        "verificacoes": {
            "tipo": tipo_verificado,
            "tamanhoMaximo": tamanho_maximo,
            "decimaisMaximos": decimais,
            # Datas no padrão ddmmaaaa (p. 34), nos campos DT_ de tamanho 8.
            "data": nome.startswith("DT_") and tamanho.lstrip("0") == "8",
            "valores": verificacao_de_valores,
            "obrigatorio": True if marca in ("sim", "s") else None if marca.startswith("oc") else False,
        },
    }


def gerar(word, texto_pdf):
    blocos = blocos_do_word(word)
    paginas = paginas_do_pdf(texto_pdf)
    tabela = next(b for b in blocos if b["k"] == "tbl" and b["rows"] and any("Obrigatoriedade de Entrada" in c for c in b["rows"][0]))
    # Na Tabela de Registros, o encerramento do Bloco 9 está como 9099; no Capítulo 4, 9990 (p. 58 e 618).
    da_tabela = {("9990" if l[0].strip() == "9099" else l[0].strip()): [limpar(c) for c in l] for l in tabela["rows"][1:]}
    inicio = next(i for i, b in enumerate(blocos) if b["k"] == "p" and limpar(b["t"]).startswith("4.5. Leiaute dos Registros"))
    titulos = [i for i, b in enumerate(blocos) if i > inicio and b["k"] == "p" and b["s"] == "Ttulo4" and re.match(r"\s*Registro\s+[0-9A-Z]{4}\s*:", b["t"])]
    registros = []
    divergencias = []
    for k, i in enumerate(titulos):
        fim = titulos[k + 1] if k + 1 < len(titulos) else len(blocos)
        codigo = re.match(r"\s*Registro\s+([0-9A-Z]{4})", blocos[i]["t"]).group(1)
        trecho = blocos[i:fim]
        cabecalho = next(" ".join(" ".join(l) for l in b["rows"]) for b in trecho if b["k"] == "tbl" and any("Nível Hier" in c for l in b["rows"] for c in l))
        campos_tbl = next(b for b in trecho if b["k"] == "tbl" and len(b["rows"][0]) >= 7 and any("Tamanho" in c for c in b["rows"][0]))
        pagina = pagina_do_registro(paginas, codigo)
        nivel = int(re.search(r"Nível Hierárquico\s*[–-]\s*(\d+)", cabecalho).group(1))
        oc_cabecalho = re.sub(r"\s+", "", re.search(r"Ocorrência\s*[–-]\s*([0-9N:\s]+[0-9N])", cabecalho).group(1))
        chave = re.search(r"Campo\(s\) chave:\s*([^\n]*?)(?:$|\s{2,})", cabecalho)
        chave_campos = [c for c in re.split(r"[+;,\s\[\]]+", chave.group(1) if chave else "") if re.fullmatch(r"[A-Z][A-Z0-9_]*", c)]
        linha_tabela = da_tabela[codigo]
        oc_tabela = linha_tabela[5]
        concordam = ocorrencia(oc_cabecalho) == ocorrencia(oc_tabela) and ocorrencia(oc_cabecalho) is not None
        registros.append({
            "codigo": codigo,
            "nome": re.sub(r"^Registro\s+\S+:\s*", "", limpar(blocos[i]["t"])),
            "bloco": codigo[0],
            "nivel": nivel,
            "nivelNaTabela": int(linha_tabela[1]) if linha_tabela[1].isdigit() else None,
            "ocorrencia": oc_cabecalho,
            "ocorrenciaNaTabela": oc_tabela,
            "ocorrenciaMaxima": ocorrencia(oc_cabecalho)[1] if concordam else None,
            "ocorrenciaVerificada": concordam,
            "entrada": linha_tabela[3].split(" ")[0],
            "saida": linha_tabela[4].split(" ")[0],
            "chave": chave_campos,
            # Regras do registro: as do cabeçalho e as descritas na prosa do registro, como em L030 e X366.
            "regras": sorted(set(REGRA.findall(re.split(r"N[íi]vel\s+Hier", cabecalho)[0]))
                              | {m.group(1) for b in trecho if b["k"] == "p" for m in [re.match(r"\s*(REGRA_[A-Z0-9_]+)\s*:", b["t"])] if m}),
            "pagina": pagina,
            "campos": [campo(l, pagina) for l in campos_tbl["rows"][1:] if limpar(l[0])],
        })
        # O REG é conferido pela existência do registro; a coluna de valores dele só registra divergências do manual.
        reg = registros[-1]["campos"][0]
        lista = (reg["verificacoes"]["valores"] or {}).get("lista")
        if lista and lista != [codigo]:
            divergencias.append(f"Registro {codigo}, p. {pagina}: o campo REG traz {reg['valoresValidos']} nos valores válidos, e não [{codigo}].")
        reg["verificacoes"]["valores"] = None
        for c in registros[-1]["campos"]:
            if re.fullmatch(r"\d+", c["tamanho"]) and c["verificacoes"]["tamanhoMaximo"] > int(c["tamanho"]):
                divergencias.append(f"Registro {codigo}, p. {pagina}: o campo {c['nome']} tem tamanho {c['tamanho']} na coluna, e os valores válidos {c['valoresValidos']} vão até {c['verificacoes']['tamanhoMaximo']} caracteres; vale a lista, que o programa da ECF aceita.")

    # Pai: o registro anterior de nível imediatamente superior, na ordem sequencial (p. 58 e 59).
    for indice, registro in enumerate(registros):
        divergente = registro["nivelNaTabela"] is not None and registro["nivelNaTabela"] != registro["nivel"]
        registro["hierarquiaVerificada"] = not divergente
        if registro["nivel"] == 0:
            registro["pai"] = None
            continue
        anteriores = [r for r in registros[:indice] if r["nivel"] == registro["nivel"] - 1]
        registro["pai"] = anteriores[-1]["codigo"] if anteriores else None

    blocos_em_ordem = []
    for registro in registros:
        if registro["bloco"] not in blocos_em_ordem:
            blocos_em_ordem.append(registro["bloco"])
    return {
        "versaoDoCatalogo": VERSAO_DO_CATALOGO,
        "fonte": {"documento": "Manual de Orientação do Leiaute 12 da ECF", "versao": "leiaute-12", "publicacao": "23/07/2026"},
        "ordemDosBlocos": blocos_em_ordem,
        "divergencias": divergencias,
        "registros": registros,
    }


def markdown(catalogo):
    linhas = [
        "# Catálogo estrutural da ECF, leiaute 12",
        "",
        f"Gerado por `.github/scripts/catalogo/gerar-catalogo.py` a partir da versão em Word do manual, com as páginas do PDF. Versão `{catalogo['versaoDoCatalogo']}`. É o que Kaio Macedo revisa (mudança 08, E02). Correções vão no gerador, nunca no JSON.",
        "",
        f"**Ordem dos blocos:** {', '.join(catalogo['ordemDosBlocos'])}.",
        "",
        "**Divergências do próprio manual, sem achado (REQ-ESTRUT-011):** "
        + (" ".join(catalogo["divergencias"]) if catalogo["divergencias"] else "nenhuma além das de ocorrência e nível marcadas na tabela.")
        + " Ocorrência e nível divergentes entre o cabeçalho do registro e a Tabela de Registros (p. 43 a 58) ficam sem verificação.",
        "",
        "| Registro | Nome | Nível | Pai | Ocorrência (cabeçalho / tabela) | Entrada / saída | Página |",
        "| --- | --- | --- | --- | --- | --- | --- |",
    ]
    for r in catalogo["registros"]:
        marca = "" if r["ocorrenciaVerificada"] else " (divergente, não verificada)"
        hierarquia = "" if r["hierarquiaVerificada"] else " (nível divergente)"
        linhas.append(f"| {r['codigo']} | {r['nome']} | {r['nivel']}{hierarquia} | {r['pai'] or '—'} | {r['ocorrencia']} / {r['ocorrenciaNaTabela']}{marca} | {r['entrada']} / {r['saida']} | {r['pagina']} |")
    for r in catalogo["registros"]:
        linhas += ["", f"## {r['codigo']}: {r['nome']}", "", f"Página {r['pagina']}. Chave: {', '.join(r['chave']) or '—'}. Regras do registro: {', '.join(r['regras']) or '—'}.", "",
                   "| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |", "| --- | --- | --- | --- | --- | --- | --- | --- |"]
        for c in r["campos"]:
            v = c["verificacoes"]
            aplicadas = [n for n, ok in (("tipo", v["tipo"]), ("data", v["data"])) if ok]
            por_posicao = (v["valores"] or {}).get("porPosicao")
            if v["tamanhoMaximo"] is not None:
                aplicadas.append(f"exatamente {v['tamanhoMaximo']}, valores por posição" if por_posicao else f"até {v['tamanhoMaximo']}")
            if v["decimaisMaximos"] is not None:
                aplicadas.append(f"{v['decimaisMaximos']} decimais")
            if v["valores"] and not por_posicao:
                aplicadas.append("valores")
            if v["obrigatorio"]:
                aplicadas.append("obrigatório")
            celulas = [str(c["numero"]), c["nome"], c["tipo"], c["tamanho"], c["decimal"], c["valoresValidos"].replace("|", "\\|")[:80], c["obrigatorio"][:20], ", ".join(aplicadas)]
            linhas.append("| " + " | ".join(celulas) + " |")
    return "\n".join(linhas) + "\n"


def main():
    argumentos = argparse.ArgumentParser()
    argumentos.add_argument("--word", required=True)
    argumentos.add_argument("--texto-pdf", required=True)
    argumentos.add_argument("--json", required=True)
    argumentos.add_argument("--md", required=True)
    a = argumentos.parse_args()
    catalogo = gerar(a.word, a.texto_pdf)
    open(a.json, "w", encoding="utf-8", newline="\n").write(json.dumps(catalogo, ensure_ascii=False, indent=1) + "\n")
    open(a.md, "w", encoding="utf-8", newline="\n").write(markdown(catalogo))
    campos = sum(len(r["campos"]) for r in catalogo["registros"])
    print(f"{len(catalogo['registros'])} registros, {campos} campos, {catalogo['versaoDoCatalogo']}.")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
