#!/usr/bin/env bash
# Guarda os manuais originais em um Release privado do repositório (mudança 05, E04), porque o portal troca os arquivos
# sem mudar o endereço. Baixa dos endereços oficiais e confere cada hash antes de publicar.
#
#   bash .github/scripts/publicar-fontes.sh
set -euo pipefail
umask 077

REPOSITORIO=kaiosmacedo/Agente-ECF
RELEASE=fontes-ecf-2026-11
RAIZ=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
MANIFESTO="${MANIFESTO:-$RAIZ/docs/fontes/manifesto-rag.json}"
GH=$(command -v gh || echo "$HOME/.local/gh/bin/gh.exe")
temporario=$(mktemp -d)
trap 'rm -rf "$temporario" 2> /dev/null || true' EXIT

parar() {
  echo "ERRO: $1" >&2
  exit 1
}

"$GH" release view "$RELEASE" --repo "$REPOSITORIO" > /dev/null 2>&1 && parar "o Release $RELEASE já existe."

node -e '
  const m = JSON.parse(require("fs").readFileSync(process.argv[1], "utf8"));
  for (const d of m.documentos) console.log([d.arquivo, d.sha256, d.origem].join("\t"));
' "$MANIFESTO" | tr -d '\r' > "$temporario/lista.tsv"

while IFS=$'\t' read -r arquivo esperado origem; do
  curl -fsSL --max-time 300 -A agente-ecf-carga -o "$temporario/$arquivo" "$origem" || parar "não foi possível baixar $arquivo."
  [ "$(sha256sum "$temporario/$arquivo" | cut -d' ' -f1)" = "$esperado" ] \
    || parar "$arquivo mudou no portal: o hash é diferente do registrado. Atualize docs/fontes-oficiais.md e o manifesto."
done < "$temporario/lista.tsv"

"$GH" release create "$RELEASE" --repo "$REPOSITORIO" --title "Fontes oficiais da ECF, novembro de 2026" \
  --notes "Cópia dos originais da página oficial da Receita Federal (E04 da mudança 05). O portal troca os arquivos sem mudar o endereço. Versões e hashes em docs/fontes-oficiais.md." \
  "$temporario/"*.pdf > /dev/null 2>&1 || parar "não foi possível criar o Release."

echo "Pronto. Release privado $RELEASE criado, com os hashes conferidos."
