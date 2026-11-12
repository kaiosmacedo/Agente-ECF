#!/usr/bin/env bash
# Constrói e testa a imagem de cada serviço (REQ-ENTREGA-001 a 003, REQ-OBSERV-002, REQ-A2A-001, REQ-A2A-005, REQ-EXEC-005).
# Uso no CI: VERSAO=<sha> bash .github/scripts/testar-conteineres.sh
set -euo pipefail

VERSAO="${VERSAO:-local}"
TEMPORARIO=$(mktemp -d)

# Banco descartável de cada serviço, na imagem do compose. Sem senha: só existe durante este teste, em rede própria.
REDE=agente-ecf-ci
BANCO=agente-ecf-ci-banco
RAG=agente-ecf-ci-rag
CONEXAO="Host=$BANCO;Database=postgres;Username=postgres"

remover_banco() {
  docker rm -f "$RAG" > /dev/null 2>&1 || true
  docker rm -f "$BANCO" > /dev/null 2>&1 || true
  docker network rm "$REDE" > /dev/null 2>&1 || true
  rm -rf "$TEMPORARIO" 2> /dev/null || true
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

# ECF sintéticas dos testes (SQ-10): uma estruturalmente válida e uma mínima, com erros de estrutura (mudança 08).
ECF_VALIDA=tests/Api.Agente.Tests/Dados/ecf/ecf-leiaute-12-estrutural.txt
ECF_COM_ERROS=tests/Api.Agente.Tests/Dados/ecf/ecf-leiaute-12.txt

# Chave sintética da Sofia só para este teste; a real fica no Vercel (mudança 13, D12; SQ-03).
CHAVE_A2A=chave-sintetica-de-ci

# SendMessage com uma ECF do arquivo dado, no contrato SOFIA v1.0.
corpo_a2a() {
  jq -n --arg ecf "$(cat "$1")" --arg solicitacao "$2" \
    '({versaoContrato:"1.0",idExecucao:"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e",idSolicitacao:$solicitacao,usuario:{nome:"Usuario Teste"},pergunta:"Valide esta ECF",documentos:[{idDocumento:"doc-001",nome:"ecf.txt",tipo:"text/plain",texto:$ecf}]} | tojson) as $texto
     | {jsonrpc:"2.0",id:"ci",method:"SendMessage",params:{message:{role:"ROLE_USER",messageId:"5f92c2ba-6870-4862-87ba-1ae8669a77f4",parts:[{text:$texto}]}}}'
}

# Cartão e SendMessage com dados sintéticos: Task concluída, com o resultado do contrato no Artifact.
testar_a2a() {
  local id="$1" corpo resposta execucao
  local chave=(-H "Authorization: Bearer $CHAVE_A2A")
  [ "$(curl -s -o /dev/null -w '%{http_code}' "http://localhost:8080/.well-known/agent-card.json")" = 401 ] \
    || falhar "$id" "O cartão respondeu sem a chave da Sofia."
  curl -fsS "${chave[@]}" -H "A2A-Version: 1.0" "http://localhost:8080/.well-known/agent-card.json" | jq -e '.supportedInterfaces[0].protocolVersion == "1.0" and .skills[0].id == "analisar_documentos"' > /dev/null \
    || falhar "$id" "Cartão do agente inválido."
  curl -fsS "${chave[@]}" "http://localhost:8080/.well-known/agent-card.json" | jq -e '.protocolVersion == "0.3.0" and .preferredTransport == "JSONRPC" and .securitySchemes.sofiaService.scheme == "bearer"' > /dev/null \
    || falhar "$id" "Cartão 0.3.0 da Sofia central inválido."
  corpo=$(corpo_a2a "$ECF_VALIDA" e1b3a4cb-d729-4bd4-b796-b9c94be07b46)
  resposta=$(curl -fsS -X POST "http://localhost:8080/a2a" "${chave[@]}" -H "Content-Type: application/json" -H "A2A-Version: 1.0" -d "$corpo")
  echo "$resposta" | jq -e '.result.task.status.state == "TASK_STATE_COMPLETED" and (.result.task.artifacts[0].parts[0].text | fromjson | .status == "parcial" and .veredito == "parcial" and .idSolicitacao == "e1b3a4cb-d729-4bd4-b796-b9c94be07b46")' > /dev/null \
    || falhar "$id" "SendMessage não devolveu o resultado do contrato."
  execucao=$(echo "$resposta" | jq -r '.result.task.artifacts[0].parts[0].text | fromjson | .idExecucaoAgente')
  curl -fsS "http://localhost:8080/analises/$execucao" | jq -e --arg id "$execucao" '.idExecucao == $id and .status == "parcial" and .veredito == "parcial" and (.etapas | length) == 9' > /dev/null \
    || falhar "$id" "GET /analises/{id} não devolveu o envelope registrado."
  curl -fsS -X POST "http://localhost:8080/a2a" "${chave[@]}" -H "Content-Type: application/json" -H "A2A-Version: 1.0" -d "$corpo" \
    | jq -e --arg id "$execucao" '.result.task.artifacts[0].parts[0].text | fromjson | .idExecucaoAgente == $id' > /dev/null \
    || falhar "$id" "A retentativa criou outra execução."
  # message/send 0.3.0, como a Sofia central envia: Task concluída e um Artifact analise-sofia-v1.
  corpo=$(echo "$corpo" | jq '.method = "message/send" | .params.message.kind = "message" | .params.message.role = "user" | .params.message.parts[0].kind = "text"')
  curl -fsS -X POST "http://localhost:8080/a2a" "${chave[@]}" -H "Content-Type: application/json" -d "$corpo" \
    | jq -e '.id == "ci" and .result.kind == "task" and .result.status.state == "completed" and ([.result.artifacts[] | select(.name == "analise-sofia-v1")] | length) == 1' > /dev/null \
    || falhar "$id" "message/send 0.3.0 não devolveu a Task concluída."
  curl -fsS "http://localhost:8080/openapi/v1.json" | jq -e '.paths["/analises/{id}"] and .paths["/analises"].post' > /dev/null \
    || falhar "$id" "O documento OpenAPI não descreve /analises."
  # REST com a mesma ECF sintética, em ISO 8859-1 e CR LF, como o manual pede (mudança 07, REQ-RECEPCAO-001 e 008).
  mkdir -p "$TEMPORARIO"
  cp "$ECF_VALIDA" "$TEMPORARIO/ecf.txt"
  (cd "$TEMPORARIO" && curl -fsS -X POST "http://localhost:8080/analises" -F "arquivo=@ecf.txt;type=text/plain") \
    | jq -e '.canal == "rest" and .status == "parcial" and .leiaute.leiaute == 12 and .leiaute.codVer == "0012"' > /dev/null \
    || falhar "$id" "POST /analises não identificou o leiaute da ECF."
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
  ambiente=(--network "$REDE" -e "$variavel=$CONEXAO" -e "SOFIA_A2A_TOKEN=$CHAVE_A2A")

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

# Ponta a ponta (mudança 08, tarefa 5.3): a ECF com erros de estrutura gera achados, a etapa 5 chama a ferramenta MCP
# da Api.RAG real, e a busca desligada vira limitação, sem falha técnica (REQ-CONSULTA-007).
echo "::group::Api.Agente com a Api.RAG"
subir_banco
# Um banco por serviço, como em produção (P15): os dois têm a tabela consumo_ia (mudança 17).
docker exec "$BANCO" psql -q -U postgres -c "CREATE DATABASE agente" -c "CREATE DATABASE rag" > /dev/null
CONEXAO_AGENTE="Host=$BANCO;Database=agente;Username=postgres"
CONEXAO_RAG="Host=$BANCO;Database=rag;Username=postgres"
docker run --rm --network "$REDE" -e "ConnectionStrings__Execucoes=$CONEXAO_AGENTE" agente-ecf-api-agente:ci --migrar > /dev/null
docker run --rm --network "$REDE" -e "ConnectionStrings__Conhecimento=$CONEXAO_RAG" agente-ecf-api-rag:ci --migrar > /dev/null
docker run -d --name "$RAG" --network "$REDE" -e "ConnectionStrings__Conhecimento=$CONEXAO_RAG" agente-ecf-api-rag:ci > /dev/null
id=$(docker run -d --network "$REDE" -e "ConnectionStrings__Execucoes=$CONEXAO_AGENTE" -e "SOFIA_A2A_TOKEN=$CHAVE_A2A" -e "Conhecimento__UrlDoServidorMcp=http://$RAG:8080/mcp" -e Perguntas__Habilitada=true -p 8080:8080 agente-ecf-api-agente:ci)
esperar_saude "http://localhost:8080/health/live" 30 || falhar "$id" "A Api.Agente não subiu com a Api.RAG."
resposta=$(curl -fsS -X POST "http://localhost:8080/a2a" -H "Authorization: Bearer $CHAVE_A2A" -H "Content-Type: application/json" -H "A2A-Version: 1.0" -d "$(corpo_a2a "$ECF_COM_ERROS" 0c6f1f6e-6a4b-4b8e-9d3a-2f7d1c9b5a10)")
execucao=$(echo "$resposta" | jq -r '.result.task.artifacts[0].parts[0].text | fromjson | .idExecucaoAgente')
curl -fsS "http://localhost:8080/analises/$execucao" \
  | jq -e '.veredito == "recusado" and .status == "parcial" and ([.etapas[] | select(.numero == 4) | .estado] == ["concluida"])
           and ([.etapas[] | select(.numero == 5) | .estado] == ["nao-executada"]) and (.limitacoes | index("consulta ao manual desativada")) != null
           and (.limitacoes | index("Etapa não realizada: Consulta ao manual. Motivo: a busca no manual está desligada na base de conhecimento.")) != null' > /dev/null \
  || falhar "$id" "A etapa 5 não consultou a Api.RAG por MCP ou não tratou a busca desligada."
docker logs "$RAG" 2>&1 | jq -e -s 'any(.[]; .Message? == "Busca no manual recusada: busca_desativada")' > /dev/null \
  || falhar "$id" "A Api.RAG não recebeu a chamada MCP da Api.Agente."
# Pergunta sem arquivo (mudança 17): com a busca desligada na Api.RAG, a resposta é consulta_indisponivel, sem falha.
curl -fsS -X POST "http://localhost:8080/perguntas" -H "Content-Type: application/json" -d '{"pergunta":"O que \u00e9 o Registro 0000 da ECF?"}' \
  | jq -e '.versaoContrato == "1.0" and .status == "erro" and .erro.codigo == "consulta_indisponivel" and .versaoDoManual == "leiaute-12"' > /dev/null \
  || falhar "$id" "A pergunta pelo REST não chegou à busca da Api.RAG."
docker rm -f "$id" "$RAG" > /dev/null
echo "::endgroup::"
