#!/usr/bin/env bash
# Confere a integração A2A 0.3.0 com a chave Bearer da Sofia central, como a Sofia chama (mudança 13, tarefa 8.6).
# Roda Kaio Macedo, que tem a chave: o assistente não usa a chave real (SQ-03). A chave não aparece na tela,
# na linha de comando nem em arquivo fora da pasta temporária, apagada no fim.
#
#   bash .github/scripts/verificar-sofia-a2a.sh                       # produção
#   BASE=http://localhost:8080 bash .github/scripts/verificar-sofia-a2a.sh
#
# Sem SOFIA_A2A_TOKEN no ambiente, o script pede a chave sem mostrá-la.
set -euo pipefail
umask 077

BASE="${BASE:-https://agente-ecf-agente.vercel.app}"
RAIZ=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
ECF="$RAIZ/tests/Api.Agente.Tests/Dados/ecf/ecf-leiaute-12-estrutural.txt"
temporario=$(mktemp -d)
trap 'rm -rf "$temporario"' EXIT

if [ -z "${SOFIA_A2A_TOKEN:-}" ]; then
  read -rsp "Chave SOFIA_A2A_TOKEN (não aparece na tela): " SOFIA_A2A_TOKEN
  echo
fi
printf 'Authorization: Bearer %s' "$SOFIA_A2A_TOKEN" > "$temporario/cabecalho"
unset SOFIA_A2A_TOKEN

falhas=0
conferir() { if [ "$2" = "$3" ]; then echo "ok    $1"; else echo "FALHA $1: esperado $3, veio $2"; falhas=$((falhas + 1)); fi; }

conferir "cartão sem chave responde 401" "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/.well-known/agent-card.json")" 401
conferir "RPC sem chave responde 401" "$(curl -s -o /dev/null -w '%{http_code}' -X POST -H 'Content-Type: application/json' -d '{}' "$BASE/a2a")" 401
conferir "cartão com chave responde 200" "$(curl -s -o "$temporario/cartao.json" -w '%{http_code}' -H @"$temporario/cabecalho" "$BASE/.well-known/agent-card.json")" 200

python -I - "$ECF" "$temporario/pedido.json" <<'PY'
import json, sys, uuid
ecf = open(sys.argv[1], encoding="latin-1").read()
solicitacao = {
    "versaoContrato": "1.0", "idExecucao": str(uuid.uuid4()), "idSolicitacao": str(uuid.uuid4()),
    "usuario": {"nome": "Usuária Sintética"}, "pergunta": "Valide esta ECF sintética.",
    "documentos": [{"idDocumento": "doc-001", "nome": "ecf-sintetica.txt", "tipo": "text/plain", "texto": ecf}],
}
pedido = {"jsonrpc": "2.0", "id": "verificacao-001", "method": "message/send", "params": {"message": {
    "kind": "message", "role": "user", "messageId": str(uuid.uuid4()),
    "parts": [{"kind": "text", "text": json.dumps(solicitacao, ensure_ascii=False)}]}}}
json.dump({"pedido": pedido, "idSolicitacao": solicitacao["idSolicitacao"]}, open(sys.argv[2], "w", encoding="utf-8"), ensure_ascii=False)
PY
python -I -c "import json,sys; json.dump(json.load(open(sys.argv[1],encoding='utf-8'))['pedido'], open(sys.argv[2],'w',encoding='utf-8'), ensure_ascii=False)" "$temporario/pedido.json" "$temporario/corpo.json"
conferir "message/send com chave responde 200" "$(curl -s -o "$temporario/resposta.json" -w '%{http_code}' -X POST -H 'Content-Type: application/json' -H @"$temporario/cabecalho" --data-binary @"$temporario/corpo.json" "$BASE/a2a")" 200

python -I - "$temporario/cartao.json" "$temporario/resposta.json" "$temporario/pedido.json" <<'PY'
import json, sys
sys.stdout.reconfigure(encoding="utf-8")
falhas = 0
def conferir(nome, ok):
    global falhas
    print(("ok    " if ok else "FALHA ") + nome)
    falhas += 0 if ok else 1
try:
    cartao = json.load(open(sys.argv[1], encoding="utf-8"))
    conferir("cartão 0.3.0 com JSONRPC", cartao.get("protocolVersion") == "0.3.0" and cartao.get("preferredTransport") == "JSONRPC")
    conferir("cartão com nome Agente ECF", cartao.get("name") == "Agente ECF")
    conferir("cartão com esquema http bearer", any(e.get("type") == "http" and e.get("scheme") == "bearer" for e in cartao.get("securitySchemes", {}).values()))
    conferir("cartão com url https do RPC", str(cartao.get("url", "")).startswith("https://"))
except Exception as erro:
    conferir(f"cartão legível ({type(erro).__name__})", False)
try:
    resposta = json.load(open(sys.argv[2], encoding="utf-8"))
    tarefa = resposta.get("result", {})
    artefatos = [a for a in tarefa.get("artifacts", []) if a.get("name") == "analise-sofia-v1"]
    conferir("id JSON-RPC preservado", resposta.get("id") == "verificacao-001")
    conferir("Task concluída", tarefa.get("kind") == "task" and tarefa.get("status", {}).get("state") == "completed")
    conferir("um Artifact analise-sofia-v1 com uma parte de texto", len(artefatos) == 1 and [p.get("kind") for p in artefatos[0].get("parts", [])] == ["text"])
    resultado = json.loads(artefatos[0]["parts"][0]["text"]) if artefatos else {}
    esperado = json.load(open(sys.argv[3], encoding="utf-8"))["idSolicitacao"]
    conferir("mesmo idSolicitacao e agente agente-ecf", resultado.get("idSolicitacao") == esperado and resultado.get("agente", {}).get("idAgente") == "agente-ecf")
    print(f"      status do contrato: {resultado.get('status')}; veredito: {resultado.get('veredito')}")
except Exception as erro:
    conferir(f"resposta legível ({type(erro).__name__})", False)
sys.exit(1 if falhas else 0)
PY
[ $? -eq 0 ] && [ "$falhas" -eq 0 ] && echo "Integração A2A 0.3.0 com a chave: tudo certo." || { echo "Há falhas acima."; exit 1; }
