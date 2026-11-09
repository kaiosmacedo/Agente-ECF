"""Extrai do programa da ECF os ajustes do catálogo do Leiaute 12 aprovados na decisão E10 (mudança 08).

    python -I gerar-catalogo.py --word manual.docx --texto-pdf manual.txt --json manual.json --md manual.md
    python -I extrair-ajustes-do-programa.py --programa "<pasta do SpedECF>" --catalogo-do-manual manual.json --saida ajustes-do-programa-12.json

Compara o catálogo vindo só do manual com o descritor do leiaute e as tabelas de valores do programa, e grava onde o agente,
por seguir o manual, recusaria o que o programa aceita. O descritor e as tabelas são só lidos; nenhum código do programa é
executado. Rode de novo quando o programa ou o manual mudarem, e confira o resultado na tela do programa.
"""
import argparse
import datetime
import hashlib
import json
import os
import re
import xml.etree.ElementTree as ET
import zipfile

JAR = os.path.join("lib", "br.gov.serpro.sped.irpjpva", "irpjpva-infra.jar")
DESCRITOR = "descritor/escrituracao/estrutura12003/v1/descritor.xml"
VERSAO_DO_PROGRAMA = "12.2.8"
# Registros que o manual obriga e o programa preenche sozinho: testado em 2026-11-08, com arquivo sem eles (E10, D8).
REGISTROS_SEM_EXIGENCIA = ["C001", "C990", "E001"]
# A Tabela de Registros dá "N" na entrada, mas o programa lê o registro na importação, e a contagem dele recusa o arquivo (E06).
REGISTROS_LIDOS_NA_IMPORTACAO = ["M990"]
# O manual diz "deve ser informado vazio" na importação (p. 611), e a coluna diz "Sim"; o programa importa vazio (teste de 2026-11-08).
VAZIOS_NA_IMPORTACAO = ["Y800.HASH"]
# "Caso o campo em moeda estrangeira seja informado, este campo será calculado pelo sistema" (p. 445 a 449).
CALCULADOS_PELO_SISTEMA = [f"W200.{c}" for c in ("VL_REC_NAO_REL", "VL_REC_REL", "VL_REC_TOTAL", "VL_LUC_PREJ_ANTES_IR", "VL_IR_PAGO", "VL_IR_DEVIDO", "VL_CAP_SOC", "VL_LUC_ACUM", "VL_ATIV_TANG")]
# Tabelas do programa com a mesma lista do campo no manual. A do Y612 fica de fora: o programa filtra uma tabela geral.
TABELAS = {
    "0010.TIP_ENT": "SPEDECF_TIPO_ENTIDADE",
    "0010.FORMA_TRIB": "SPEDECF_FORMA_TRIBUTACAO",
    "0010.COD_QUALIF_PJ": "SPEDECF_QUALIF_PESSOA_JURIDICA",
    "0000.IND_SIT_INI_PER": "SPEDECF_INDICADOR_INICIO_PERIODO",
    "0000.SIT_ESPECIAL": "SPEDECF_SITUACAO",
    "L200.IND_AVAL_ESTOQ": "SPEDECF_METODO_AVALIACAO_ESTOQUE",
    "Y672.IND_AVAL_ESTOQ": "SPEDECF_METODO_AVALIACAO_ESTOQUE",
    "Y520.FORMA": "SPEDECF_FORMA_RECTO_PAGTO",
    "Y600.QUALIF": "SPEDECF_QUALIF_SOCIO_TITULAR",
    "X280.IND_ATIV": "SPEDECF_BENEFICIO_FISCAL",
    "X280.IND_PROJ": "SPEDECF_PROJETO",
    "X340.IND_CONTROLE": "SPEDECF_TIPO_EMPRESA",
    "X370.TIPO_TRANSACAO": "SPEDECF_TIPO_TRANSACAO",
    "X370.TIP_AJ_COMPENSATORIO": "SPEDECF_TIP_AJ_COMPENSATORIO",
    "X370.METODO": "SPEDECF_METODO_PRECO_TRANSFERENCIA",
    "X485.TIPO_BENEF": "SPEDECF_BENEFICIOS_SOCIAIS",
}
ANO_INICIO, ANO_FIM = datetime.date(2025, 1, 1), datetime.date(2025, 12, 31)


def ler_descritor(xml):
    registros = {}

    def andar(no, bloco):
        for filho in no:
            if filho.tag == "bloco":
                andar(filho, filho.get("id"))
            elif filho.tag == "registro":
                campos = []
                for c in filho.findall("campo"):
                    v = c.find("valores-validos")
                    campos.append({"id": c.get("id"), "tipo": c.get("tipo"), "tamanho": c.get("tamanho"), "obrigatorio": c.get("obrigatorio"),
                                   "valores": v.get("valores") if v is not None else None, "tabela": v.get("datasource") if v is not None else None})
                registros[filho.get("id")] = {"id": filho.get("id"), "nivel": int(filho.get("nivel")), "bloco": bloco, "chave": filho.get("chave"), "campos": campos}
                andar(filho, bloco)

    andar(ET.fromstring(xml), None)
    return registros


def data(texto):
    try:
        return datetime.date(int(texto[4:8]), int(texto[2:4]), int(texto[:2]))
    except ValueError:
        return None


def codigos_vigentes(pasta, nome):
    """Códigos da tabela que valem em algum dia de 2025; a linha é CÓDIGO|DESCRIÇÃO|DT_INI|DT_FIM."""
    caminhos = [a for a in os.listdir(pasta) if "$" in a and a.split("$")[1] == nome]
    arquivo = max(caminhos, key=lambda a: int(a.rsplit("$", 1)[1]))
    codigos = []
    with open(os.path.join(pasta, arquivo), encoding="latin-1") as f:
        next(f)
        for linha in f:
            partes = linha.rstrip("\r\n").split("|")
            if len(partes) >= 4 and partes[0]:
                inicio, fim = data(partes[-2]), data(partes[-1])
                if (inicio is None or inicio <= ANO_FIM) and (fim is None or fim >= ANO_INICIO):
                    codigos.append(partes[0])
    return codigos


def extrair(pasta, catalogo):
    with zipfile.ZipFile(os.path.join(pasta, JAR)) as jar:
        xml = jar.read(DESCRITOR)
    desc = ler_descritor(xml)
    cat = {r["codigo"]: r for r in catalogo["registros"]}
    tipos, tamanhos, valores, obrigatoriedade, chaves, listas_so_no_manual = {}, {}, {}, [], {}, []
    for codigo, r in cat.items():
        d = desc[codigo]
        if len(d["campos"]) != len(r["campos"]):
            raise SystemExit(f"Registro {codigo}: {len(r['campos'])} campos no manual e {len(d['campos'])} no programa.")
        for c, o in zip(r["campos"], d["campos"]):
            rotulo = f"{codigo}.{c['nome']}"
            v = c["verificacoes"]
            tipo_do_programa = {"Q": "C"}.get(o["tipo"], o["tipo"])
            if c["tipo"] in ("N", "NS") and tipo_do_programa in ("NS", "C") and tipo_do_programa != c["tipo"]:
                tipos[rotulo] = tipo_do_programa
            tamanho = re.sub(r"\D", "", o["tamanho"] or "")
            if tipos.get(rotulo) == "C":
                # O limite de N e NS conta só os algarismos. Como texto, vale o limite do programa ou, sem ele no descritor,
                # o máximo de 255 do manual (p. 33), que o programa aplica: 0000.NOME com 300 caracteres é recusado.
                tamanhos[rotulo] = max(int(tamanho), v["tamanhoMaximo"] or 0) if tamanho else 255
            elif tamanho and v["tamanhoMaximo"] is not None and int(tamanho) > v["tamanhoMaximo"]:
                tamanhos[rotulo] = int(tamanho)
            if v["obrigatorio"] is True and (o["obrigatorio"] == "0" or rotulo in CALCULADOS_PELO_SISTEMA):
                obrigatoriedade.append(rotulo)
            lista = (v["valores"] or {}).get("lista")
            if lista and not o["valores"] and not o["tabela"] and c["numero"] != 1:
                listas_so_no_manual.append(rotulo)
            if lista and o["valores"]:
                a_mais = [p.split("=", 1)[0].strip() for p in o["valores"].split(";") if p.strip() and p.split("=", 1)[0].strip() not in lista]
                if a_mais and c["numero"] != 1:
                    valores[rotulo] = a_mais
            if lista and rotulo in TABELAS:
                a_mais = [x for x in codigos_vigentes(os.path.join(pasta, "recursos", "tabelas"), TABELAS[rotulo]) if x not in lista]
                if a_mais:
                    valores[rotulo] = a_mais
        # A chave do programa, com os nomes do catálogo: os campos são os mesmos, na mesma posição.
        nomes = {o["id"]: c["nome"] for c, o in zip(r["campos"], d["campos"])}
        chave = sorted(nomes[n] for n in re.split(r"[,;]", d["chave"] or "") if n in nomes and n != "REG")
        if chave != sorted(n for n in r["chave"] if n != "REG"):
            chaves[codigo] = chave
    so_no_programa = []
    ordem = list(desc)
    for codigo in ordem:
        if codigo not in cat:
            d = desc[codigo]
            anteriores = [c for c in ordem[: ordem.index(codigo)] if desc[c]["bloco"] == d["bloco"]]
            so_no_programa.append({
                "codigo": codigo, "nivel": d["nivel"], "bloco": d["bloco"], "depoisDe": anteriores[-1],
                "campos": [{"nome": c["id"], "tipo": c["tipo"], "tamanho": re.sub(r"\D", "", c["tamanho"] or "") or "-"} for c in d["campos"]],
            })
    return {
        "programa": {"versao": VERSAO_DO_PROGRAMA, "descritor": "12003 v1", "sha256DoDescritor": hashlib.sha256(xml).hexdigest()},
        "registrosSoNoPrograma": so_no_programa,
        "registrosSemExigencia": REGISTROS_SEM_EXIGENCIA,
        "registrosLidosNaImportacao": REGISTROS_LIDOS_NA_IMPORTACAO,
        "vaziosNaImportacao": VAZIOS_NA_IMPORTACAO,
        "listasSoNoManual": listas_so_no_manual,
        "tipos": tipos,
        "tamanhos": tamanhos,
        "valores": valores,
        "obrigatoriedade": obrigatoriedade,
        "chaves": chaves,
    }


def main():
    argumentos = argparse.ArgumentParser()
    argumentos.add_argument("--programa", required=True)
    argumentos.add_argument("--catalogo-do-manual", required=True)
    argumentos.add_argument("--saida", required=True)
    a = argumentos.parse_args()
    ajustes = extrair(a.programa, json.load(open(a.catalogo_do_manual, encoding="utf-8")))
    open(a.saida, "w", encoding="utf-8", newline="\n").write(json.dumps(ajustes, ensure_ascii=False, indent=1) + "\n")
    print({k: len(v) for k, v in ajustes.items() if k != "programa"})


if __name__ == "__main__":
    main()
