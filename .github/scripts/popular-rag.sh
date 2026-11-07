#!/usr/bin/env bash
# Popula a base de conhecimento com os manuais oficiais, sem exibir a chave nem a conexão (mudança 05, D5 e D7; SQ-03).
# Roda depois de o banco existir e de o CD ter aplicado as migrações.
#
#   bash .github/scripts/popular-rag.sh leiaute-12    # um leiaute, para avaliar os parâmetros antes da carga completa
#   bash .github/scripts/popular-rag.sh avaliar       # mede a recuperação com as perguntas de docs/avaliacao
#   bash .github/scripts/popular-rag.sh todos         # os seis manuais; o que já estiver na base não é refeito
#
# Para comparar parâmetros (D6): TAMANHO_DO_TRECHO, SOBREPOSICAO_DO_TRECHO e REPROCESSAR=1, que refaz o que já está na base.
# Na avaliação, COM_CODIGO=1 usa a busca por código da mudança 06 (D4).
# A chave da OpenAI vem do arquivo do projeto SofiaInfis, como Kaio Macedo autorizou. Para usar outra, informe ARQUIVO_DA_CHAVE.
# Ela vai só para o processo em contêiner, por variável de ambiente: nunca para o repositório, os logs, o Vercel ou o GitHub.
set -euo pipefail
umask 077

TIME=agent-ecf
PROJETO=agente-ecf-rag
REPOSITORIO=kaiosmacedo/Agente-ECF
RELEASE=fontes-ecf-2026-11
ARQUIVO_DA_CHAVE="${ARQUIVO_DA_CHAVE:-$HOME/source/repos/SofiaInfis/SofiaInfis.Api/appsettings.json}"

RAIZ=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
MANIFESTO="${MANIFESTO:-$RAIZ/docs/fontes/manifesto-rag.json}"
GH=$(command -v gh || echo "$HOME/.local/gh/bin/gh.exe")
temporario=$(mktemp -d)
trap 'rm -f "$temporario/.env.conexao"; rm -rf "$temporario" 2> /dev/null || true' EXIT

parar() {
  echo "ERRO: $1" >&2
  exit 1
}

vercel() { npx --yes vercel@65.7.0 "$@" --scope "$TIME" --cwd "$temporario"; }

ler() { sed -n "s/^$1=//p" "$temporario/.env.conexao" | head -n 1 | tr -d '\r' | sed -e 's/^"//' -e 's/"$//'; }

caminho_para_docker() { if command -v cygpath > /dev/null; then cygpath -w "$1"; else echo "$1"; fi; }

MODO="${1:-}"
case "$MODO" in
  avaliar) argumentos=(--avaliar /fontes/perguntas.json) ;;
  todos) argumentos=(--ingerir /fontes/manifesto-rag.json) ;;
  leiaute-*) argumentos=(--ingerir /fontes/manifesto-rag.json --versao "$MODO") ;;
  *) parar "use leiaute-12, avaliar ou todos." ;;
esac

[ -f "$ARQUIVO_DA_CHAVE" ] || parar "o arquivo da chave não existe. Informe ARQUIVO_DA_CHAVE."
"$GH" auth status > /dev/null 2>&1 || parar "entre no GitHub com: gh auth login"
vercel whoami > /dev/null 2>&1 || parar "entre no Vercel com: npx vercel@65.7.0 login"

echo "1/4 Lendo a conexão do banco e a chave, sem exibi-las..."
vercel link --project "$PROJETO" --yes > /dev/null 2>&1 || parar "não foi possível acessar o projeto $PROJETO."
vercel env pull .env.conexao --environment production --yes > /dev/null 2>&1 || parar "não foi possível ler as variáveis de $PROJETO."
host=$(ler PGHOST_UNPOOLED)
banco=$(ler PGDATABASE)
usuario=$(ler PGUSER)
senha=$(ler PGPASSWORD)
rm -f "$temporario/.env.conexao"
if [ -z "$host" ] || [ -z "$banco" ] || [ -z "$usuario" ] || [ -z "$senha" ]; then
  parar "as variáveis do Neon não puderam ser lidas em $PROJETO. Crie o banco com: cadastrar-banco-neon.sh rag"
fi
export ConnectionStrings__Conhecimento="Host=$host;Database=$banco;Username=$usuario;Password=$senha;SSL Mode=VerifyFull;Channel Binding=Require;Maximum Pool Size=5"

# O caminho vai para o Node, que lê o JSON; comentários de linha são aceitos, como no arquivo de configuração do .NET.
OpenAI__ApiKey=$(node -e '
  const texto = require("fs").readFileSync(process.argv[1], "utf8").replace(/^﻿/, "").replace(/^\s*\/\/.*$/gm, "");
  process.stdout.write((JSON.parse(texto).OpenAI || {}).ApiKey || "");
' "$ARQUIVO_DA_CHAVE") || parar "o arquivo da chave não pôde ser lido."
[ -n "$OpenAI__ApiKey" ] || parar "o arquivo não tem OpenAI:ApiKey."
export OpenAI__ApiKey

mkdir -p "$temporario/fontes"
if [ "$MODO" = "avaliar" ]; then
  echo "2/4 Preparando as perguntas de avaliação..."
  cp "$RAIZ/docs/avaliacao/perguntas.json" "$temporario/fontes/perguntas.json"
elif "$GH" release view "$RELEASE" --repo "$REPOSITORIO" > /dev/null 2>&1; then
  echo "2/4 Baixando os manuais do Release $RELEASE..."
  "$GH" release download "$RELEASE" --repo "$REPOSITORIO" --pattern '*.pdf' --dir "$temporario/fontes" > /dev/null 2>&1 \
    || parar "não foi possível baixar os manuais do Release."
else
  # Sem o Release, baixa da Receita Federal e confere cada hash. Para guardar os originais: publicar-fontes.sh (E04).
  echo "2/4 Baixando os manuais dos endereços oficiais e conferindo os hashes..."
  node -e '
    const m = JSON.parse(require("fs").readFileSync(process.argv[1], "utf8"));
    for (const d of m.documentos) console.log([d.arquivo, d.sha256, d.origem].join("\t"));
  ' "$MANIFESTO" | tr -d '\r' > "$temporario/lista.tsv"
  while IFS=$'\t' read -r arquivo esperado origem; do
    curl -fsSL --max-time 300 -A agente-ecf-carga -o "$temporario/fontes/$arquivo" "$origem" || parar "não foi possível baixar $arquivo."
    [ "$(sha256sum "$temporario/fontes/$arquivo" | cut -d' ' -f1)" = "$esperado" ] \
      || parar "$arquivo mudou no portal: o hash é diferente do registrado. Atualize docs/fontes-oficiais.md e o manifesto."
  done < "$temporario/lista.tsv"
fi
[ "$MODO" = "avaliar" ] || cp "$MANIFESTO" "$temporario/fontes/manifesto-rag.json"

[ -z "${REPROCESSAR:-}" ] || argumentos+=(--reprocessar)
[ -z "${COM_CODIGO:-}" ] || argumentos+=(--com-codigo)
ajustes=()
[ -z "${TAMANHO_DO_TRECHO:-}" ] || ajustes+=(-e "Ingestao__TamanhoDoTrecho=$TAMANHO_DO_TRECHO")
[ -z "${SOBREPOSICAO_DO_TRECHO:-}" ] || ajustes+=(-e "Ingestao__SobreposicaoDoTrecho=$SOBREPOSICAO_DO_TRECHO")

echo "3/4 Construindo a imagem da Api.RAG..."
docker build -q -t agente-ecf-rag-carga "$RAIZ/src/Api.RAG" > /dev/null || parar "a imagem não foi construída."

echo "4/4 Rodando. A carga completa leva alguns minutos."
MSYS_NO_PATHCONV=1 docker run --rm \
  -e OpenAI__ApiKey -e ConnectionStrings__Conhecimento -e Observabilidade__NivelMinimo=Warning "${ajustes[@]}" \
  -v "$(caminho_para_docker "$temporario/fontes"):/fontes:ro" \
  agente-ecf-rag-carga "${argumentos[@]}" || parar "o comando terminou com falha. A chave e a conexão não foram exibidas."

echo
echo "Pronto. Os arquivos temporários são apagados agora."
