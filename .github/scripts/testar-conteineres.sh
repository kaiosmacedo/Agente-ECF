#!/usr/bin/env bash
# Constrói e testa a imagem de cada serviço (REQ-ENTREGA-001 a 003, REQ-OBSERV-002, REQ-AUTH-001).
# Uso no CI: VERSAO=<sha> bash .github/scripts/testar-conteineres.sh
set -euo pipefail

VERSAO="${VERSAO:-local}"

# Opções de autenticação de teste: a saúde é anônima, então nenhum metadado do provedor é buscado.
AUTENTICACAO=(
  -e "Autenticacao__Autoridade=https://login.microsoftonline.com/00000000-0000-0000-0000-000000000000/v2.0"
  -e "Autenticacao__Audiencia=api://agente-ecf-ci"
)

esperar_saude() {
  local url="$1" limite="$2" inicio=$SECONDS
  until curl -fsS --max-time 2 "$url" > /dev/null 2>&1; do
    if (( SECONDS - inicio >= limite )); then
      return 1
    fi
    sleep 1
  done
  echo "Saudável em $(( SECONDS - inicio )) s: $url"
}

falhar() {
  local id="$1" mensagem="$2"
  docker logs "$id" || true
  docker rm -f "$id" > /dev/null || true
  echo "::error::$mensagem"
  exit 1
}

for servico in Api.Agente Api.RAG; do
  imagem="agente-ecf-$(echo "$servico" | tr '[:upper:].' '[:lower:]-'):ci"
  echo "::group::$servico"

  docker build --build-arg VERSAO="$VERSAO" -t "$imagem" "src/$servico"

  # Sem PORT: porta 8080, vivacidade em até 30 s e logs em JSON com correlação.
  id=$(docker run -d -p 8080:8080 "${AUTENTICACAO[@]}" "$imagem")
  esperar_saude "http://localhost:8080/health/live" 30 || falhar "$id" "$servico não respondeu na porta 8080 em 30 s."
  curl -fsS -H "X-Correlation-Id: ci-conteiner" "http://localhost:8080/health/live" > /dev/null
  sleep 1
  docker logs "$id" 2>&1 | jq -e -s 'length > 0 and all(.[]; type == "object")' > /dev/null \
    || falhar "$id" "$servico gerou log que não é JSON."
  docker logs "$id" 2>&1 | jq -e -s 'any(.[]; any(.Scopes[]?; type == "object" and .CorrelationId? == "ci-conteiner"))' > /dev/null \
    || falhar "$id" "$servico não registrou o identificador de correlação no log."
  docker rm -f "$id" > /dev/null

  # Com PORT=3000: a porta informada pela plataforma é respeitada.
  id=$(docker run -d -e PORT=3000 -p 3000:3000 "${AUTENTICACAO[@]}" "$imagem")
  esperar_saude "http://localhost:3000/health/live" 30 || falhar "$id" "$servico não respondeu na porta 3000 com PORT=3000."
  docker rm -f "$id" > /dev/null

  # Sem as opções de autenticação, o serviço não inicia e cita a opção ausente.
  id=$(docker run -d "$imagem")
  codigo=$(timeout 30 docker wait "$id" || echo "em-execucao")
  logs=$(docker logs "$id" 2>&1 || true)
  if [ "$codigo" = "em-execucao" ] || [ "$codigo" = "0" ] || ! grep -q "Autenticacao:Autoridade" <<< "$logs"; then
    falhar "$id" "$servico iniciou sem as opções de autenticação ou não citou a opção ausente."
  fi
  docker rm -f "$id" > /dev/null

  # Conteúdo da aplicação na imagem: sem segredos, testes, specs ou histórico do repositório.
  temporario=$(docker create "$imagem")
  proibidos=$(docker export "$temporario" | tar -t | grep '^app/' \
    | grep -E '(^|/)\.env|(^|/)\.git/|(^|/)tests/|(^|/)openspec/|appsettings\..*\.local\.json' || true)
  docker rm "$temporario" > /dev/null
  if [ -n "$proibidos" ]; then
    echo "$proibidos"
    echo "::error::$servico tem arquivos proibidos na imagem."
    exit 1
  fi

  echo "::endgroup::"
done
