#!/usr/bin/env bash
# Cria o banco das execuções no Neon, pela integração do Vercel, e cadastra as duas conexões sem exibir os valores
# (mudança 03, tarefas 8.1 e 8.2; SQ-03). Quem roda é o dono das contas: a criação pede o aceite dos termos do Neon.
#
#   bash .github/scripts/cadastrar-banco-neon.sh
#
# Pode rodar de novo, por exemplo depois de trocar a senha do banco: o banco é mantido, e as conexões, sobrescritas.
set -euo pipefail
umask 077

TIME=agent-ecf
PROJETO=agente-ecf-agente
BANCO=agente-ecf-execucoes
REPOSITORIO=kaiosmacedo/Agente-ECF

GH=$(command -v gh || echo "$HOME/.local/gh/bin/gh.exe")
temporario=$(mktemp -d)
# No Windows, um processo da CLI pode segurar a pasta por alguns segundos; o arquivo com os valores sai antes, abaixo.
trap 'rm -f "$temporario/.env.conexao"; rm -rf "$temporario" 2> /dev/null || true' EXIT

vercel() { npx --yes vercel@65.7.0 "$@" --scope "$TIME" --cwd "$temporario"; }

parar() {
  echo "ERRO: $1" >&2
  exit 1
}

ler() { sed -n "s/^$1=//p" "$temporario/.env.conexao" | head -n 1 | tr -d '\r' | sed -e 's/^"//' -e 's/"$//'; }

conexao() {
  printf 'Host=%s;Database=%s;Username=%s;Password=%s;SSL Mode=VerifyFull;Channel Binding=Require;Maximum Pool Size=5' \
    "$1" "$banco_neon" "$usuario" "$senha"
}

# Quem roda entra na própria conta, se ainda não tiver entrado neste terminal.
if ! "$GH" auth status > /dev/null 2>&1; then
  echo "Entre no GitHub: o navegador abre e pede o código que aparece aqui."
  "$GH" auth login --web --hostname github.com --git-protocol https --skip-ssh-key || parar "não foi possível entrar no GitHub."
fi
if ! vercel whoami > /dev/null 2>&1; then
  echo "Entre no Vercel: siga o link que aparece aqui."
  npx --yes vercel@65.7.0 login || parar "não foi possível entrar no Vercel."
fi

echo "1/4 Ligando esta execução ao projeto $PROJETO..."
vercel link --project "$PROJETO" --yes > /dev/null 2>&1 || parar "não foi possível acessar o projeto $PROJETO."

if vercel integration list --all 2>&1 | grep -q "$BANCO"; then
  echo "2/4 O banco $BANCO já existe; mantido."
else
  echo "2/4 Criando o banco $BANCO: plano Free, região iad1, sem Auth e sem Realtime."
  echo "    Leia os termos do Neon quando forem pedidos. Para aceitar, responda y: Enter sozinho recusa."
  vercel integration add neon --name "$BANCO" --billing-plan free_v3 \
    --meta region=iad1 --meta auth=false --meta realtime=false \
    --environment production --no-env-pull \
    || parar "o banco não foi criado. Nada foi cadastrado."
fi

echo "3/4 Lendo as variáveis que a integração criou, sem exibi-las..."
for tentativa in 1 2 3 4 5 6; do
  vercel env pull .env.conexao --environment production --yes > /dev/null 2>&1 || true
  [ -s "$temporario/.env.conexao" ] && [ -n "$(ler PGPASSWORD)" ] && break
  [ "$tentativa" = 6 ] || sleep 5
done

host_pooling=$(ler PGHOST)
host_direto=$(ler PGHOST_UNPOOLED)
banco_neon=$(ler PGDATABASE)
usuario=$(ler PGUSER)
senha=$(ler PGPASSWORD)
rm -f "$temporario/.env.conexao"

if [ -z "$host_pooling" ] || [ -z "$host_direto" ] || [ -z "$banco_neon" ] || [ -z "$usuario" ] || [ -z "$senha" ]; then
  parar "as variáveis do Neon não puderam ser lidas em $PROJETO. Confira se o banco está ligado ao ambiente Production; se estiverem como Sensitive, siga o cadastro manual de docs/ci-cd.md. Nada foi cadastrado."
fi
case "$host_pooling" in
  *-pooler*) ;;
  *) parar "PGHOST não é o endereço com pooling. Nada foi cadastrado." ;;
esac
case "$host_direto" in
  *-pooler*) parar "PGHOST_UNPOOLED não é o endereço direto. Nada foi cadastrado." ;;
esac
case "$senha$usuario$banco_neon" in
  *\;*) parar "um dos valores contém ponto e vírgula, e o formato da conexão não o aceita. Nada foi cadastrado." ;;
esac

echo "4/4 Cadastrando as conexões..."
conexao "$host_pooling" | vercel env add ConnectionStrings__Execucoes production --sensitive --force > /dev/null 2>&1 \
  || parar "ConnectionStrings__Execucoes não foi cadastrada no Vercel."
conexao "$host_direto" | "$GH" secret set CONEXAO_MIGRACAO_EXECUCOES --env producao --repo "$REPOSITORIO" > /dev/null 2>&1 \
  || parar "ConnectionStrings__Execucoes foi cadastrada, mas CONEXAO_MIGRACAO_EXECUCOES não. Rode de novo."

echo
echo "Pronto. Nenhum valor foi exibido, e os arquivos temporários são apagados agora."
echo "  Vercel, $PROJETO, Production: ConnectionStrings__Execucoes (com pooling)"
echo "  GitHub, Environment producao: CONEXAO_MIGRACAO_EXECUCOES (direto)"
