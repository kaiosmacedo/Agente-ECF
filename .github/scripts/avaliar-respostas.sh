#!/usr/bin/env bash
# Avalia as respostas às perguntas sobre a ECF (mudança 17, D11; REQ-PERGUNTA-010), sem exibir a chave nem a conexão (SQ-03).
#
#   bash .github/scripts/avaliar-respostas.sh
#
# Sobe, em contêineres e numa rede própria:
#   - a Api.RAG com a busca ligada, sobre a base de produção (como popular-rag.sh), lida pelo Vercel;
#   - um PostgreSQL descartável para as execuções e as perguntas da Api.Agente;
#   - a Api.Agente com as perguntas e o LLM ligados.
# Depois manda cada pergunta de docs/avaliacao/perguntas-respostas.json ao POST /perguntas e uma pela Sofia (A2A), e grava o
# resultado em RESULTADO (padrão: um arquivo temporário cujo caminho é impresso no fim).
# A chave da OpenAI vem do arquivo do projeto SofiaInfis, como Kaio Macedo decidiu em 2026-11-12 (E11). Para usar outra,
# informe ARQUIVO_DA_CHAVE. Ela vai só para os processos em contêiner, por variável de ambiente: nunca para o repositório,
# os logs, o Vercel ou o GitHub.
set -euo pipefail
umask 077

TIME=agent-ecf
PROJETO=agente-ecf-rag
ARQUIVO_DA_CHAVE="${ARQUIVO_DA_CHAVE:-$HOME/source/repos/SofiaInfis/SofiaInfis.Api/appsettings.json}"
RAIZ=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
PERGUNTAS="$RAIZ/docs/avaliacao/perguntas-respostas.json"
PORTA="${PORTA:-18090}"
REDE=agente-ecf-avaliacao
CHAVE_A2A=chave-sintetica-de-avaliacao
temporario=$(mktemp -d)
RESULTADO="${RESULTADO:-$temporario/resultado.json}"

limpar() {
  docker rm -f agente-ecf-aval-agente agente-ecf-aval-rag agente-ecf-aval-banco > /dev/null 2>&1 || true
  docker network rm "$REDE" > /dev/null 2>&1 || true
  rm -f "$temporario/.env.conexao"
}
trap limpar EXIT

parar() {
  echo "ERRO: $1" >&2
  exit 1
}

vercel() { npx --yes vercel@65.7.0 "$@" --scope "$TIME" --cwd "$temporario"; }

ler() { sed -n "s/^$1=//p" "$temporario/.env.conexao" | head -n 1 | tr -d '\r' | sed -e 's/^"//' -e 's/"$//'; }

[ -f "$ARQUIVO_DA_CHAVE" ] || parar "o arquivo da chave não existe. Informe ARQUIVO_DA_CHAVE."
vercel whoami > /dev/null 2>&1 || parar "entre no Vercel com: npx vercel@65.7.0 login"

echo "1/5 Lendo a conexão da base e a chave, sem exibi-las..."
vercel link --project "$PROJETO" --yes > /dev/null 2>&1 || parar "não foi possível acessar o projeto $PROJETO."
vercel env pull .env.conexao --environment production --yes > /dev/null 2>&1 || parar "não foi possível ler as variáveis de $PROJETO."
host=$(ler PGHOST_UNPOOLED)
banco=$(ler PGDATABASE)
usuario=$(ler PGUSER)
senha=$(ler PGPASSWORD)
rm -f "$temporario/.env.conexao"
[ -n "$host" ] && [ -n "$banco" ] && [ -n "$usuario" ] && [ -n "$senha" ] || parar "as variáveis do Neon não puderam ser lidas em $PROJETO."
export ConnectionStrings__Conhecimento="Host=$host;Database=$banco;Username=$usuario;Password=$senha;SSL Mode=VerifyFull;Channel Binding=Require;Maximum Pool Size=5"
unset host banco usuario senha

OpenAI__ApiKey=$(node -e '
  const texto = require("fs").readFileSync(process.argv[1], "utf8").replace(/^﻿/, "").replace(/^\s*\/\/.*$/gm, "");
  process.stdout.write((JSON.parse(texto).OpenAI || {}).ApiKey || "");
' "$ARQUIVO_DA_CHAVE") || parar "o arquivo da chave não pôde ser lido."
[ -n "$OpenAI__ApiKey" ] || parar "o arquivo não tem OpenAI:ApiKey."
export OpenAI__ApiKey

echo "2/5 Construindo as imagens..."
docker build -q -t agente-ecf-rag-avaliacao "$RAIZ/src/Api.RAG" > /dev/null || parar "a imagem da Api.RAG não foi construída."
docker build -q -t agente-ecf-agente-avaliacao "$RAIZ/src/Api.Agente" > /dev/null || parar "a imagem da Api.Agente não foi construída."

echo "3/5 Subindo o banco, a Api.RAG e a Api.Agente..."
limpar
docker network create "$REDE" > /dev/null
docker run -d --name agente-ecf-aval-banco --network "$REDE" \
  -e POSTGRES_DB=execucoes -e POSTGRES_USER=agente_ecf -e POSTGRES_HOST_AUTH_METHOD=trust pgvector/pgvector:pg16 > /dev/null
for _ in $(seq 1 30); do
  docker exec agente-ecf-aval-banco pg_isready -q -h 127.0.0.1 -U agente_ecf -d execucoes 2> /dev/null && break
  sleep 1
done
CONEXAO_EXECUCOES="Host=agente-ecf-aval-banco;Database=execucoes;Username=agente_ecf"
docker run --rm --network "$REDE" -e ConnectionStrings__Execucoes="$CONEXAO_EXECUCOES" agente-ecf-agente-avaliacao --migrar > /dev/null \
  || parar "as migrações da Api.Agente falharam."

docker run -d --name agente-ecf-aval-rag --network "$REDE" \
  -e OpenAI__ApiKey -e ConnectionStrings__Conhecimento -e Busca__Habilitada=true -e Observabilidade__NivelMinimo=Warning \
  agente-ecf-rag-avaliacao > /dev/null
docker run -d --name agente-ecf-aval-agente --network "$REDE" -p "127.0.0.1:$PORTA:8080" \
  -e OpenAI__ApiKey -e ConnectionStrings__Execucoes="$CONEXAO_EXECUCOES" \
  -e Conhecimento__UrlDoServidorMcp=http://agente-ecf-aval-rag:8080/mcp \
  -e Perguntas__Habilitada=true -e Llm__Habilitada=true -e SOFIA_A2A_TOKEN="$CHAVE_A2A" \
  -e Observabilidade__NivelMinimo=Warning \
  agente-ecf-agente-avaliacao > /dev/null
unset OpenAI__ApiKey ConnectionStrings__Conhecimento
for _ in $(seq 1 60); do
  curl -fs "http://127.0.0.1:$PORTA/health/ready" > /dev/null 2>&1 && break
  sleep 1
done
curl -fs "http://127.0.0.1:$PORTA/health/ready" > /dev/null || parar "a Api.Agente não ficou pronta."

echo "4/5 Mandando as perguntas..."
python -I - "$PERGUNTAS" "http://127.0.0.1:$PORTA" "$CHAVE_A2A" "$RESULTADO" <<'PY'
import json, sys, time, urllib.request, uuid

perguntas_arquivo, base, chave, saida = sys.argv[1:5]
sys.stdout.reconfigure(encoding="utf-8")
conjunto = json.load(open(perguntas_arquivo, encoding="utf-8"))


def postar(caminho, corpo, cabecalhos=None):
    dados = json.dumps(corpo, ensure_ascii=False).encode("utf-8")
    pedido = urllib.request.Request(base + caminho, data=dados, method="POST",
                                    headers={"Content-Type": "application/json", **(cabecalhos or {})})
    inicio = time.monotonic()
    with urllib.request.urlopen(pedido, timeout=120) as resposta:
        return json.loads(resposta.read().decode("utf-8")), round((time.monotonic() - inicio) * 1000)


resultados = []
for item in conjunto["perguntas"]:
    resposta, ms = postar("/perguntas", {"pergunta": item["pergunta"]})
    paginas = sorted({fonte["pagina"] for fonte in resposta.get("fontes", [])})
    resultados.append({**item, "status": resposta["status"], "erro": (resposta.get("erro") or {}).get("codigo"),
                       "paginasCitadas": paginas, "resposta": resposta["resposta"], "idPergunta": resposta["idPergunta"],
                       "versaoDoManual": resposta.get("versaoDoManual"), "duracaoMs": ms})
    print(f"  {item['id']:4} {resposta['status']:9} páginas {paginas}")

# Uma pergunta pela Sofia: pseudodocumento sofia-question-context, A2A 0.3.0, como a Sofia central manda.
pergunta = conjunto["perguntas"][0]["pergunta"]
solicitacao = {"versaoContrato": "1.0", "idExecucao": str(uuid.uuid4()), "idSolicitacao": str(uuid.uuid4()),
               "usuario": {"nome": "Usuária Sintética"}, "pergunta": pergunta,
               "documentos": [{"idDocumento": "sofia-question-context", "nome": "contexto-pergunta.txt", "tipo": "text/plain", "texto": pergunta}]}
rpc = {"jsonrpc": "2.0", "id": "avaliacao-001", "method": "message/send", "params": {"message": {
    "kind": "message", "role": "user", "messageId": str(uuid.uuid4()),
    "parts": [{"kind": "text", "text": json.dumps(solicitacao, ensure_ascii=False)}]}}}
a2a, _ = postar("/a2a", rpc, {"Authorization": f"Bearer {chave}"})
tarefa = a2a["result"]
artefato = json.loads(tarefa["artifacts"][0]["parts"][0]["text"])
sofia = {"estado": tarefa["status"]["state"], "artefato": tarefa["artifacts"][0]["name"], "status": artefato["status"],
         "analise": artefato["analise"], "observacoes": artefato["observacoes"],
         "idSolicitacaoConfere": artefato["idSolicitacao"] == solicitacao["idSolicitacao"]}
print(f"  Sofia {sofia['estado']} {sofia['status']}")
json.dump({"resultados": resultados, "sofia": sofia}, open(saida, "w", encoding="utf-8"), ensure_ascii=False, indent=2)
PY

echo "5/5 Lendo a relevância dos trechos no banco descartável..."
docker exec agente-ecf-aval-banco psql -U agente_ecf -d execucoes -At -c \
  "SELECT id, coalesce((SELECT max((t->>'relevancia')::float) FROM jsonb_array_elements(trechos) t), -1), redacao FROM perguntas" \
  > "$temporario/relevancias.txt"
docker exec agente-ecf-aval-banco psql -U agente_ecf -d execucoes -At -c \
  "SELECT id_operacao, count(*), coalesce(sum(tokens_entrada), 0), coalesce(sum(tokens_saida), 0), coalesce(sum(custo_usd), 0) FROM consumo_ia GROUP BY id_operacao" \
  > "$temporario/consumo.txt"
python -I - "$RESULTADO" "$temporario/relevancias.txt" "$temporario/consumo.txt" <<'PY'
import json, sys
caminho, relevancias, consumo = sys.argv[1:4]
dados = json.load(open(caminho, encoding="utf-8"))
por_id = {}
for linha in open(relevancias, encoding="utf-8"):
    partes = linha.strip().split("|")
    if len(partes) == 3:
        por_id[partes[0]] = (float(partes[1]), partes[2])
gastos = {}
for linha in open(consumo, encoding="utf-8"):
    partes = linha.strip().split("|")
    if len(partes) == 5:
        gastos[partes[0]] = {"chamadas": int(partes[1]), "tokensEntrada": int(partes[2]), "tokensSaida": int(partes[3]), "custoUsd": float(partes[4])}
for item in dados["resultados"]:
    relevancia, redacao = por_id.get(item["idPergunta"], (None, None))
    item["melhorRelevancia"] = relevancia
    item["redacao"] = redacao
    item["consumo"] = gastos.get(item["idPergunta"])
json.dump(dados, open(caminho, "w", encoding="utf-8"), ensure_ascii=False, indent=2)
PY
echo
echo "Resultado em $RESULTADO. Os contêineres e a rede são apagados agora; a chave e a conexão não foram exibidas."
