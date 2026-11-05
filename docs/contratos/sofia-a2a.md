# Integração com a Sofia INFIS por A2A

Como a Sofia aciona o Agente-ECF. Requisitos em `openspec/changes/servidor-a2a/` e regras em RA-07, SQ-12 e SQ-13 da constitution. O contrato de negócio é compartilhado entre os agentes da turma e está em [`.claude/skills/sofia-a2a-contrato/references/contrato-sofia-v1.md`](../../.claude/skills/sofia-a2a-contrato/references/contrato-sofia-v1.md).

**Situação:** esqueleto da integração. Protocolo e contrato são reais, mas a análise ainda não existe: toda solicitação válida responde com status `parcial`, sem achados, e declara as validações não realizadas. Sem autenticação nesta versão (SQ-12): use somente dados sintéticos ou anonimizados.

## Identificação

| Item | Valor |
| --- | --- |
| Cartão | `GET /.well-known/agent-card.json` (cópia versionada em [`agent-card.json`](agent-card.json)) |
| Endpoint A2A | o campo `url` do cartão; em produção, `https://agente-ecf-agente.vercel.app/a2a` |
| Protocolo | A2A `0.3.0`, JSON-RPC 2.0 |
| `agente.idAgente` | `agente-ecf` |
| Skill | `analisar_documentos` |
| Streaming e notificações | Não |

## O que é de A2A e o que é do contrato

| Camada | Campos | Quem define |
| --- | --- | --- |
| JSON-RPC | `jsonrpc`, `id`, `method`, `params`, `result`, `error` | Protocolo |
| Mensagem A2A | `message.role`, `message.messageId`, `message.parts[].kind`, `message.parts[].text` | Protocolo |
| Tarefa A2A | `id`, `contextId`, `status.state`, `artifacts[].artifactId`, `artifacts[].name`, `artifacts[].parts[]` | Protocolo |
| `SolicitacaoAnaliseSofia` | JSON dentro de `message.parts[0].text` | Contrato SOFIA v1.0 |
| `ResultadoAnaliseSofia` | JSON dentro de `artifacts[0].parts[0].text` | Contrato SOFIA v1.0 |

## Exemplo

Requisição (`POST` no `url` do cartão, com `Content-Type: application/json`):

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "method": "message/send",
  "params": {
    "message": {
      "kind": "message",
      "role": "user",
      "messageId": "5f92c2ba-6870-4862-87ba-1ae8669a77f4",
      "parts": [
        {
          "kind": "text",
          "text": "{\"versaoContrato\":\"1.0\",\"idExecucao\":\"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e\",\"idSolicitacao\":\"e1b3a4cb-d729-4bd4-b796-b9c94be07b46\",\"usuario\":{\"nome\":\"Maria Oliveira\"},\"pergunta\":\"Analise o documento\",\"documentos\":[{\"idDocumento\":\"doc-001\",\"nome\":\"ECF-sintetica.txt\",\"texto\":\"|0000|LECF|0012|\"}]}"
        }
      ]
    }
  }
}
```

Resposta (o texto do Artifact foi formatado aqui para leitura; ele chega como string JSON):

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "result": {
    "kind": "task",
    "id": "5cd591b8-6cd1-4dd2-87d2-ede0a99ac066",
    "contextId": "4441e6c2-5d89-4715-9a9f-f9e4885d029e",
    "status": { "state": "completed", "timestamp": "2026-11-05T13:58:45.7168463+00:00" },
    "artifacts": [
      {
        "artifactId": "a147e2de-93ac-4c60-a764-efeae072d3df",
        "name": "analise-sofia-v1",
        "parts": [
          {
            "kind": "text",
            "text": {
              "versaoContrato": "1.0",
              "idExecucao": "8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e",
              "idSolicitacao": "e1b3a4cb-d729-4bd4-b796-b9c94be07b46",
              "agente": { "idAgente": "agente-ecf", "nome": "Agente ECF" },
              "status": "parcial",
              "analise": "1 documento(s) recebido(s). Esta versão ainda não executa as validações da ECF: nenhum achado foi produzido e nenhuma conformidade é declarada.",
              "achados": [],
              "observacoes": [
                "Documento doc-001 recebido: 16 caracteres, SHA-256 62c7300c09e9ba7fb61fd495bda362790382b6f7d22517ee2a6e0c2bf1f9ac18.",
                "Validações não realizadas nesta versão: identificação do leiaute, validação estrutural, regras de negócio, consulta ao manual e análise por IA."
              ],
              "erro": null
            }
          }
        ]
      }
    ]
  }
}
```

## Desfechos

| Situação | `status.state` | `status` do contrato | `erro.codigo` |
| --- | --- | --- | --- |
| Solicitação válida | `completed` | `parcial` | — |
| Campo obrigatório ausente ou inválido, `idDocumento` repetido | `completed` | `erro` | `solicitacao_invalida` |
| `versaoContrato` diferente de `1.0` | `completed` | `erro` | `versao_contrato_nao_suportada` |
| Texto dos documentos acima do limite | `completed` | `erro` | `documentos_acima_do_limite` |
| Mensagem sem `TextPart`, texto que não é JSON, `idExecucao` ou `idSolicitacao` que não são UUID | `failed` | sem resultado | motivo em `status.message` |

`concluido` fica reservado para quando todas as validações obrigatórias existirem (P05).

| Erro JSON-RPC | Quando |
| --- | --- |
| `-32700` | O corpo não é JSON. |
| `-32601` | Método desconhecido. |
| `-32602` | Parâmetros inválidos. O SDK exige `acceptedOutputModes` quando a mensagem traz `configuration`. |
| `-32001` | Tarefa não encontrada. |
| `-32002` | `tasks/cancel` em tarefa já terminada. |
| `-32003` | Configuração de notificações. |
| `-32004` | `message/stream` e `tasks/resubscribe`. |

A resposta de `message/send` já é final. No Vercel, as instâncias não duram, então `tasks/get` não é garantido.

## Configuração

Nenhum destes valores é segredo.

| Opção (variável de ambiente) | Padrão | Uso |
| --- | --- | --- |
| `A2A:UrlPublica` (`A2A__UrlPublica`) | Em produção, a URL do Vercel; fora dela, o endereço da requisição | `url` anunciada no cartão |
| `A2A:TamanhoMaximoDosDocumentosEmBytes` | `10485760` (provisório até DP-05, na mudança 07) | Limite do texto somado dos documentos |
| `Cors:AllowedOrigins:<n>` (`Cors__AllowedOrigins__<n>`) | Vazio | Origens de navegador, só se houver frontend |

**CORS:** a chamada da Sofia é entre servidores e não depende de CORS. Preencha `Cors:AllowedOrigins` só se um frontend no navegador chamar o agente, com a origem desse frontend (esquema, host e porta, sem barra final). CORS não substitui autenticação.

**Autenticação:** adiada pela SQ-12. Na retomada, a credencial fica no transporte (`Authorization`), nunca no JSON do contrato. Veja a seção "Autenticação adiada" de [`docs/roteiro.md`](../roteiro.md).

**Privacidade:** `usuario.nome`, `pergunta` e o texto dos documentos não vão para logs, para a resposta nem para o armazenamento das tarefas. Os logs trazem `idExecucao`, `idSolicitacao`, status e o `X-Correlation-Id`.

## Como executar

Testes:

```bash
dotnet test --solution Agente-ECF.slnx -c Release
```

Na rede dos agentes, com Docker:

```bash
docker network create agentes-network
```

```bash
docker compose up --build agente-ecf
```

```bash
curl http://localhost:8085/.well-known/agent-card.json
```

De outro contêiner na mesma rede, o endereço é o nome do serviço, não `localhost`:

```bash
docker run --rm --network agentes-network curlimages/curl:8.10.1 http://agente-ecf:8080/.well-known/agent-card.json
```

Se a Sofia rodar fora do Docker, informe o endereço que ela enxerga:

```bash
A2A_URL_PUBLICA=http://localhost:8085/a2a docker compose up --build agente-ecf
```

## Pendências com o professor

- **Protocolo do cliente:** o projeto `SofiaInfis` referencia `Microsoft.Agents.AI.A2A` 1.27.0-preview, que traz o SDK `A2A` 1.0.0-preview3 (protocolo v1.0), enquanto a skill fixa v0.3.x. Se o cliente não falar v0.3, a chamada falha com método desconhecido. O pacote `A2A.V0_3Compat` é a saída no cliente.
- **Rede:** a skill usa `agentes-network`; o compose de `SofiaInfis` usa `sofia_network`, e a API da Sofia roda fora dele.
- **Autenticação:** emissor, audience, scopes ou papéis, obtenção do token e se o cartão fica público.
- **Organização:** o contrato v1.0 não tem esse campo, e o Agente-ECF precisa isolar organizações clientes.
