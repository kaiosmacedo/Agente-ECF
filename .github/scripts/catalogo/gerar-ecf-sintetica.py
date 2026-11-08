"""Gera uma ECF sintética do leiaute 12, estruturalmente válida e aceita na importação do programa da ECF (mudança 08, D10 e D12; SQ-10).

    python -I gerar-ecf-sintetica.py <catálogo.json> <saída.txt>

Tem só os 13 registros obrigatórios na importação e na transmissão. O 0010, o 0020 e o 0030 seguem os exemplos de
preenchimento do manual, com os ajustes que o programa da ECF 12.2.8 exigiu na importação (design, gabarito no PGE).
Não tem dado real: o CNPJ, o nome e o endereço são os dos exemplos do manual.
As regras de conteúdo (mudança 09) não foram consideradas: o programa importa esta ECF e aponta erros de negócio na validação.
"""
import json
import sys

FIXOS = {
    "0000": "|0000|LECF|0012|11111111000191|EMPRESA SINTETICA DE TESTE|0|0|||01012025|31122025|N||0||",
    # Exemplo da p. 79, sem o IND_REC_RECEITA: o programa da ECF não aceita o campo com FORMA_TRIB igual a 1.
    "0010": "|0010||N|1|T|01|RRRR|||||||",
    # Exemplo da p. 94, com os indicadores em N, porque a ECF sintética não tem esses dados, e sem o CEBAS, que depende deles.
    "0020": "|0020|1|0|" + "N|" * 28 + "|",
    # Exemplo da p. 102.
    "0030": "|0030|2046|6204000|RUA TESTE|1234|BLOCO Z SALA 301|BAIRRO ECF|DF|5300108|71000000|6133333333|testeecf@ecf.gov.br|",
}
SEM_DADOS = {"C001", "E001"}


def main():
    catalogo = json.load(open(sys.argv[1], encoding="utf-8"))
    obrigatorios = [r["codigo"] for r in catalogo["registros"] if r["entrada"] == "O" and r["saida"] == "O"]
    linhas = []
    for codigo in obrigatorios:
        if codigo in FIXOS:
            linhas.append(FIXOS[codigo])
        elif codigo.endswith("001") and len(codigo) == 4:
            linhas.append(f"|{codigo}|{1 if codigo in SEM_DADOS else 0}|")
        elif codigo in ("0990", "C990", "9900", "9990", "9999"):
            linhas.append(None)  # preenchidos no fim, com as contagens
        else:
            raise SystemExit(f"O Registro {codigo} é obrigatório, e este gerador não sabe preenchê-lo.")
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
