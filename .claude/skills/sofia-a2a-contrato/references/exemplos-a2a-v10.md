# Exemplos de envelope A2A v1.0 (JSON-RPC)

Escritos no Agente-ECF a partir da [especificação v1.0](https://a2a-protocol.org/v1.0.0/specification/), porque o responsável padronizou o protocolo em v1.0 (2026-11-05). O arquivo `exemplos-a2a-v03.md` fica como referência da turma. O contrato de negócio não muda: só o envelope ao redor dele. Em integração real, o texto é o objeto completo de `contrato-sofia-v1.md`, serializado como string JSON.

## Chamada Sofia → agente (POST no `url` da interface JSON-RPC do cartão)

Cabeçalhos: `Content-Type: application/json` e `A2A-Version: 1.0`.

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "method": "SendMessage",
  "params": {
    "message": {
      "role": "ROLE_USER",
      "messageId": "5f92c2ba-6870-4862-87ba-1ae8669a77f4",
      "parts": [
        {
          "text": "{\"versaoContrato\": \"1.0\", \"idExecucao\": \"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e\", \"idSolicitacao\": \"e1b3a4cb-d729-4bd4-b796-b9c94be07b46\", \"usuario\": {\"nome\": \"Maria Oliveira\"}, \"pergunta\": \"Analise o documento\", \"documentos\": [{\"idDocumento\": \"doc-001\", \"nome\": \"NFe-123.xml\", \"texto\": \"Texto extraído da nota\"}]}"
        }
      ]
    }
  }
}
```

## Resposta agente → Sofia (Task concluída com Artifact)

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "result": {
    "task": {
      "id": "task-001",
      "contextId": "context-001",
      "status": { "state": "TASK_STATE_COMPLETED" },
      "artifacts": [
        {
          "artifactId": "art-001",
          "name": "analise-sofia-v1",
          "parts": [
            {
              "text": "{\"versaoContrato\": \"1.0\", \"idExecucao\": \"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e\", \"idSolicitacao\": \"e1b3a4cb-d729-4bd4-b796-b9c94be07b46\", \"agente\": {\"idAgente\": \"agente-fiscal\", \"nome\": \"Agente Fiscal\"}, \"status\": \"concluido\", \"analise\": \"Nenhuma divergência encontrada.\", \"achados\": []}"
            }
          ]
        }
      ]
    }
  }
}
```

## Agent Card

Publicado em `/.well-known/agent-card.json`. O endpoint fica em `supportedInterfaces`, não mais em `url`.

```json
{
  "name": "Agente Fiscal INFIS",
  "description": "Analisa documentos fiscais enviados pela Sofia",
  "version": "1.0.0",
  "supportedInterfaces": [
    { "url": "http://agente-fiscal:8080/a2a", "protocolBinding": "JSONRPC", "protocolVersion": "1.0" }
  ],
  "capabilities": { "streaming": false, "pushNotifications": false },
  "defaultInputModes": ["text/plain", "application/json"],
  "defaultOutputModes": ["text/plain", "application/json"],
  "skills": [
    { "id": "analisar_documentos", "name": "Análise documental", "description": "Analisa textos enviados pela Sofia", "tags": ["infis", "fiscal", "sofia"] }
  ]
}
```

## Correspondência com v0.3

| Item | v0.3 | v1.0 |
| --- | --- | --- |
| Envio | `message/send` | `SendMessage` |
| Consulta | `tasks/get` | `GetTask` |
| Cancelamento | `tasks/cancel` | `CancelTask` |
| Streaming | `message/stream` | `SendStreamingMessage` |
| Papel | `user` | `ROLE_USER` |
| Estado | `completed` | `TASK_STATE_COMPLETED` |
| Parte de texto | `{"kind": "text", "text": "..."}` | `{"text": "..."}` |
| Resultado do envio | `result` com `kind: task` | `result.task` |
| Endpoint no cartão | `url` e `preferredTransport` | `supportedInterfaces[]` |
