#!/usr/bin/env bash
# Regenera o catálogo estrutural do leiaute 12 e o documento de revisão a partir dos originais oficiais (mudança 08, D1).
# Baixa a versão em Word e o PDF do manual, confere os hashes de docs/fontes-oficiais.md e do manifesto, extrai o texto
# do PDF com pdftotext (poppler) e roda o gerador. Depois, gera a ECF sintética dos testes a partir do catálogo.
#
#   bash .github/scripts/catalogo/gerar-catalogo.sh
#
# Requer python3 e pdftotext. Correções vão em gerar-catalogo.py, nunca no JSON gerado.
set -euo pipefail

RAIZ=$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)
WORD_URL="https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/manual_ecf_leiaute_12_20_05_2026_ac_2025_sit_esp_2026.docx/@@display-file/file"
WORD_SHA256=942877bdd3d443444f35758e531b1ab30ce965a62ad148707ef2d68061b9f657
PDF_URL="https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/manual_ecf_leiaute_12_20_05_2026_ac_2025_sit_esp_2026.pdf/@@display-file/file"
PDF_SHA256=f583475c109bf3f2444b8953924503fb0e6fd7767e69c96958703c6ed80286eb
# O primeiro Python que de fato roda: no Windows, python3 pode ser só o atalho da loja.
PYTHON=$(for candidato in python3 python; do "$candidato" -c "" > /dev/null 2>&1 && { echo "$candidato"; break; }; done)
[ -n "$PYTHON" ] || { echo "ERRO: instale o Python 3." >&2; exit 1; }
temporario=$(mktemp -d)
trap 'rm -rf "$temporario" 2> /dev/null || true' EXIT

parar() {
  echo "ERRO: $1" >&2
  exit 1
}

baixar() {
  curl -fsSL --max-time 300 -A agente-ecf-catalogo -o "$temporario/$1" "$2" || parar "não foi possível baixar $1."
  [ "$(sha256sum "$temporario/$1" | cut -d' ' -f1)" = "$3" ] \
    || parar "$1 mudou no portal: o hash é diferente do registrado. Atualize docs/fontes-oficiais.md antes de gerar o catálogo."
}

command -v pdftotext > /dev/null || parar "instale o pdftotext (pacote poppler-utils)."
echo "1/3 Baixando a versão em Word e o PDF do manual e conferindo os hashes..."
baixar manual.docx "$WORD_URL" "$WORD_SHA256"
baixar manual.pdf "$PDF_URL" "$PDF_SHA256"
pdftotext -layout -enc UTF-8 "$temporario/manual.pdf" "$temporario/manual.txt"

echo "2/3 Gerando o catálogo e o documento de revisão..."
"$PYTHON" -I "$RAIZ/.github/scripts/catalogo/gerar-catalogo.py" \
  --word "$temporario/manual.docx" --texto-pdf "$temporario/manual.txt" \
  --json "$RAIZ/src/Api.Agente/Dominio/Leiaute/ecf-leiaute-12.json" --md "$RAIZ/docs/catalogo/ecf-leiaute-12.md"

echo "3/3 Gerando a ECF sintética estruturalmente válida dos testes..."
"$PYTHON" -I "$RAIZ/.github/scripts/catalogo/gerar-ecf-sintetica.py" \
  "$RAIZ/src/Api.Agente/Dominio/Leiaute/ecf-leiaute-12.json" "$RAIZ/tests/Api.Agente.Tests/Dados/ecf/ecf-leiaute-12-estrutural.txt"
echo "Pronto. Revise docs/catalogo/ecf-leiaute-12.md e rode os testes."
