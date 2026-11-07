#!/usr/bin/env bash
# Constrói e testa a imagem de cada serviço (REQ-ENTREGA-001 a 003, REQ-OBSERV-002, REQ-A2A-001, REQ-A2A-005, REQ-EXEC-005).
# Uso no CI: VERSAO=<sha> bash .github/scripts/testar-conteineres.sh
set -euo pipefail

VERSAO="${VERSAO:-local}"

# Banco descartável de cada serviço, na imagem do compose. Sem senha: só existe durante este teste, em rede própria.
REDE=agente-ecf-ci
BANCO=agente-ecf-ci-banco
CONEXAO="Host=$BANCO;Database=postgres;Username=postgres"

remover_banco() {
  docker rm -f "$BANCO" > /dev/null 2>&1 || true
  docker network rm "$REDE" > /dev/null 2>&1 || true
}

subir_banco() {
  local inicio=$SECONDS
  remover_banco
  docker network create "$REDE" > /dev/null
  docker run -d --name "$BANCO" --network "$REDE" -e POSTGRES_HOST_AUTH_METHOD=trust pgvector/pgvector:pg16 > /dev/null
  until docker exec "$BANCO" pg_isready -q -h 127.0.0.1 -U postgres; do
    if (( SECONDS - inicio >= 60 )); then
      echo "::error::O PostgreSQL de teste não respondeu em 60 s."
      exit 1
    fi
    sleep 1
  done
}

trap remover_banco EXIT

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

# Cartão e SendMessage com dados sintéticos: Task concluída, com o resultado do contrato no Artifact.
testar_a2a() {
  local id="$1" corpo resposta execucao
  curl -fsS "http://localhost:8080/.well-known/agent-card.json" | jq -e '.supportedInterfaces[0].protocolVersion == "1.0" and .skills[0].id == "analisar_documentos"' > /dev/null \
    || falhar "$id" "Cartão do agente inválido."
  corpo=$(jq -n --arg texto '{"versaoContrato":"1.0","idExecucao":"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e","idSolicitacao":"e1b3a4cb-d729-4bd4-b796-b9c94be07b46","usuario":{"nome":"Usuario Teste"},"pergunta":"Analise o documento","documentos":[{"idDocumento":"doc-001","nome":"teste.txt","texto":"Documento sintetico."}]}' \
    '{jsonrpc:"2.0",id:"ci",method:"SendMessage",params:{message:{role:"ROLE_USER",messageId:"5f92c2ba-6870-4862-87ba-1ae8669a77f4",parts:[{text:$texto}]}}}')
  resposta=$(curl -fsS -X POST "http://localhost:8080/a2a" -H "Content-Type: application/json" -H "A2A-Version: 1.0" -d "$corpo")
  echo "$resposta" | jq -e '.result.task.status.state == "TASK_STATE_COMPLETED" and (.result.task.artifacts[0].parts[0].text | fromjson | .status == "parcial" and .veredito == "parcial" and .idSolicitacao == "e1b3a4cb-d729-4bd4-b796-b9c94be07b46")' > /dev/null \
    || falhar "$id" "SendMessage não devolveu o resultado do contrato."
  execucao=$(echo "$resposta" | jq -r '.result.task.artifacts[0].parts[0].text | fromjson | .idExecucaoAgente')
  curl -fsS "http://localhost:8080/analises/$execucao" | jq -e --arg id "$execucao" '.idExecucao == $id and .status == "parcial" and .veredito == "parcial" and (.etapas | length) == 9' > /dev/null \
    || falhar "$id" "GET /analises/{id} não devolveu o envelope registrado."
  curl -fsS -X POST "http://localhost:8080/a2a" -H "Content-Type: application/json" -H "A2A-Version: 1.0" -d "$corpo" \
    | jq -e --arg id "$execucao" '.result.task.artifacts[0].parts[0].text | fromjson | .idExecucaoAgente == $id' > /dev/null \
    || falhar "$id" "A retentativa criou outra execução."
  curl -fsS "http://localhost:8080/openapi/v1.json" | jq -e '.paths["/analises/{id}"]' > /dev/null \
    || falhar "$id" "O documento OpenAPI não descreve GET /analises/{id}."
}

# A migração cria o banco; rodar de novo não muda nada, e a prontidão só fica verde com o banco no ar.
testar_migracao() {
  local imagem="$1" variavel="$2" saida aplicadas
  saida=$(docker run --rm --network "$REDE" -e "$variavel=$CONEXAO" "$imagem" --migrar 2>&1)
  aplicadas=$(docker exec "$BANCO" psql -U postgres -tAc "SELECT count(*) FROM migracoes")
  saida+=$(docker run --rm --network "$REDE" -e "$variavel=$CONEXAO" "$imagem" --migrar 2>&1)
  echo "$saida" | jq -e -s 'all(.[]; type == "object")' > /dev/null \
    || { echo "$saida"; echo "::error::A migração escreveu fora do JSON."; exit 1; }
  [ "$aplicadas" -gt 0 ] && [ "$(docker exec "$BANCO" psql -U postgres -tAc "SELECT count(*) FROM migracoes")" = "$aplicadas" ] \
    || { echo "::error::A migração não ficou registrada uma única vez."; exit 1; }
}

# Ferramenta MCP sem estado, sem initialize (ADR 0005), e desligada por padrão (REQ-RECUP-006). A resposta vem em SSE.
mcp() {
  curl -fsS -X POST "http://localhost:8080/mcp" -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -d "$1" \
    | sed -n 's/^data: //p'
}

testar_mcp() {
  local id="$1"
  mcp '{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}' | jq -e '[.result.tools[].name] == ["buscar_no_manual"]' > /dev/null \
    || falhar "$id" "O servidor MCP não anunciou buscar_no_manual."
  mcp '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"buscar_no_manual","arguments":{"consulta":"Registro 0000","tipoDocumento":"manual-ecf","versao":"leiaute-12"}}}' \
    | jq -e '.result.isError == true and (.result.content[0].text | fromjson | .erro.codigo) == "busca_desativada"' > /dev/null \
    || falhar "$id" "A busca desligada não respondeu busca_desativada."
}

for servico in Api.Agente Api.RAG; do
  imagem="agente-ecf-$(echo "$servico" | tr '[:upper:].' '[:lower:]-'):ci"
  echo "::group::$servico"

  docker build --build-arg VERSAO="$VERSAO" -t "$imagem" "src/$servico"

  # Os dois serviços exigem o banco: sem a conexão, eles não sobem.
  if [ "$servico" = "Api.Agente" ]; then variavel=ConnectionStrings__Execucoes; else variavel=ConnectionStrings__Conhecimento; fi
  if docker run --rm "$imagem" --migrar > /dev/null 2>&1; then
    echo "::error::$servico iniciou sem a conexão com o banco."
    exit 1
  fi
  subir_banco
  testar_migracao "$imagem" "$variavel"
  ambiente=(--network "$REDE" -e "$variavel=$CONEXAO")

  # Sem PORT: porta 8080, vivacidade em até 30 s e logs em JSON com correlação.
  id=$(docker run -d "${ambiente[@]}" -p 8080:8080 "$imagem")
  esperar_saude "http://localhost:8080/health/live" 30 || falhar "$id" "$servico não respondeu na porta 8080 em 30 s."
  curl -fsS -H "X-Correlation-Id: ci-conteiner" "http://localhost:8080/health/live" > /dev/null
  sleep 1
  docker logs "$id" 2>&1 | jq -e -s 'length > 0 and all(.[]; type == "object")' > /dev/null \
    || falhar "$id" "$servico gerou log que não é JSON."
  docker logs "$id" 2>&1 | jq -e -s 'any(.[]; any(.Scopes[]?; type == "object" and .CorrelationId? == "ci-conteiner"))' > /dev/null \
    || falhar "$id" "$servico não registrou o identificador de correlação no log."
  curl -fsS "http://localhost:8080/health/ready" > /dev/null || falhar "$id" "$servico não ficou pronto com o banco no ar."
  if [ "$servico" = "Api.Agente" ]; then
    testar_a2a "$id"
  else
    curl -fsS "http://localhost:8080/consumo" | jq -e '.moeda == "USD" and (.modelos | type == "array")' > /dev/null \
      || falhar "$id" "GET /consumo não respondeu."
    curl -fsS "http://localhost:8080/openapi/v1.json" | jq -e '.paths["/consumo"]' > /dev/null \
      || falhar "$id" "O documento OpenAPI não descreve GET /consumo."
    testar_mcp "$id"
  fi
  docker logs "$id" 2>&1 | jq -e -s 'all(.[]; type == "object")' > /dev/null \
    || falhar "$id" "$servico gerou log que não é JSON depois de usar o banco."
  docker rm -f "$id" > /dev/null

  # Com PORT=3000: a porta informada pela plataforma é respeitada.
  id=$(docker run -d "${ambiente[@]}" -e PORT=3000 -p 3000:3000 "$imagem")
  esperar_saude "http://localhost:3000/health/live" 30 || falhar "$id" "$servico não respondeu na porta 3000 com PORT=3000."
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
