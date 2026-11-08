"""Gera uma ECF sintética do leiaute 12, estruturalmente válida pelo catálogo (mudança 08, D12; SQ-10).

    python -I gerar-ecf-sintetica.py <catálogo.json> <saída.txt>

Tem só os 13 registros obrigatórios na importação e na transmissão, com os campos obrigatórios preenchidos com o primeiro
valor válido ou um valor do tipo do campo. Não tem dado real: o CNPJ e o nome são os do exemplo de preenchimento do manual.
As regras de conteúdo (mudança 09) não foram consideradas, então o programa da ECF pode apontar erros de negócio nela.
"""
import json
import sys

REGISTRO_0000 = "|0000|LECF|0012|11111111000191|EMPRESA SINTETICA DE TESTE|0|0|||01012025|31122025|N||0||"
SEM_DADOS = {"C001", "E001"}


def valor(campo):
    v = campo["verificacoes"]
    if campo["numero"] == 1:
        return None  # o REG é o próprio código
    if v["valores"] and v["valores"].get("lista"):
        return v["valores"]["lista"][0]
    if v["valores"] and v["valores"].get("intervalo"):
        return v["valores"]["intervalo"][0]
    if v["data"]:
        return "01012025"
    if v["obrigatorio"] is not True:
        return ""
    if campo["tipo"] in ("N", "NS"):
        return "1"
    return "A"


def linha(registro, sobrescrever=None):
    campos = [registro["codigo"]] + [valor(c) for c in registro["campos"][1:]]
    for indice, texto in (sobrescrever or {}).items():
        campos[indice] = texto
    return "|" + "|".join(campos) + "|"


def main():
    catalogo = json.load(open(sys.argv[1], encoding="utf-8"))
    registros = {r["codigo"]: r for r in catalogo["registros"]}
    obrigatorios = [r["codigo"] for r in catalogo["registros"] if r["entrada"] == "O" and r["saida"] == "O"]
    linhas = []
    for codigo in obrigatorios:
        if codigo == "0000":
            linhas.append(REGISTRO_0000)
        elif codigo.endswith("001") and len(codigo) == 4:
            linhas.append(linha(registros[codigo], {1: "1" if codigo in SEM_DADOS else "0"}))
        elif codigo in ("0990", "C990", "9900", "9990", "9999"):
            linhas.append(None)  # preenchidos no fim, com as contagens
        else:
            linhas.append(linha(registros[codigo]))
    presentes = obrigatorios
    contagem_do_bloco = lambda bloco: sum(1 for c in presentes if c[0] == bloco)
    saida = []
    for codigo, texto in zip(obrigatorios, linhas):
        if codigo in ("0990", "C990"):
            saida.append(f"|{codigo}|{contagem_do_bloco(codigo[0])}|")
        elif codigo == "9900":
            saida += [f"|9900|{c}|{len(presentes) if c == '9900' else 1}|||" for c in presentes]
        elif codigo == "9990":
            saida.append(None)
        elif codigo == "9999":
            saida.append(None)
        else:
            saida.append(texto)
    bloco9 = 1 + len(presentes) + 1 + 1  # 9001, as linhas do 9900, o 9990 e o 9999
    total = len([s for s in saida if s is not None]) + 2
    saida = [s if s is not None else "" for s in saida]
    saida[saida.index("")] = f"|9990|{bloco9}|"
    saida[saida.index("")] = f"|9999|{total}|"
    open(sys.argv[2], "w", encoding="latin-1", newline="").write("\r\n".join(saida) + "\r\n")
    print(f"{len(saida)} linhas, registros: {', '.join(presentes)}.")


if __name__ == "__main__":
    main()
