# Integração com a Sofia INFIS por A2A

Como a Sofia aciona o Agente-ECF. Requisitos em `openspec/changes/servidor-a2a/` e regras em RA-07, SQ-12 e SQ-13 da constitution. O contrato de negócio é compartilhado entre os agentes da turma e está em [`.claude/skills/sofia-a2a-contrato/references/contrato-sofia-v1.md`](../../.claude/skills/sofia-a2a-contrato/references/contrato-sofia-v1.md).

**Situação:** esqueleto da integração, em produção desde 2026-11-05. Protocolo e contrato são reais, mas a análise ainda não existe: toda solicitação válida responde com status `parcial`, sem achados, e declara as validações não realizadas. Sem autenticação nem token (SQ-12): use somente dados sintéticos ou anonimizados.

## Identificação

| Item | Valor |
| --- | --- |
| Cartão | `GET /.well-known/agent-card.json` (cópia versionada em [`agent-card.json`](agent-card.json)) |
| Endpoint A2A | `supportedInterfaces[0].url` do cartão; em produção, `https://agente-ecf-agente.vercel.app/a2a` |
| Protocolo | A2A `1.0`, JSON-RPC 2.0, com o cabeçalho `A2A-Version: 1.0` |
| SDK do servidor | `A2A.AspNetCore` 1.0.0-preview4 |
| `agente.idAgente` | `agente-ecf` |
| Skill | `analisar_documentos` |
| Streaming e notificações | Não |

As skills da turma fixavam v0.3.x "até decisão explícita de migração". Kaio Macedo padronizou v1.0 em 2026-11-05, a versão que o pacote `Microsoft.Agents.AI.A2A` do projeto `SofiaInfis` traz. O contrato de negócio (`versaoContrato: "1.0"`) não mudou.

## O que é de A2A e o que é do contrato

| Camada | Campos | Quem define |
| --- | --- | --- |
| JSON-RPC | `jsonrpc`, `id`, `method`, `params`, `result`, `error` | Protocolo |
| Mensagem A2A | `message.role`, `message.messageId`, `message.parts[].text` | Protocolo |
| Tarefa A2A | `task.id`, `task.contextId`, `task.status.state`, `task.artifacts[].artifactId`, `.name`, `.parts[].text` | Protocolo |
| `SolicitacaoAnaliseSofia` | JSON dentro de `message.parts[0].text` | Contrato SOFIA v1.0 |
| `ResultadoAnaliseSofia` | JSON dentro de `task.artifacts[0].parts[0].text` | Contrato SOFIA v1.0 |

## Exemplo

Requisição (`POST` no endereço do cartão, com `Content-Type: application/json` e `A2A-Version: 1.0`):

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
          "text": "{\"versaoContrato\":\"1.0\",\"idExecucao\":\"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e\",\"idSolicitacao\":\"e1b3a4cb-d729-4bd4-b796-b9c94be07b46\",\"usuario\":{\"nome\":\"Maria Oliveira\"},\"pergunta\":\"Analise o documento\",\"documentos\":[{\"idDocumento\":\"doc-001\",\"nome\":\"documento-sintetico.txt\",\"texto\":\"Documento sintético de teste.\"}]}"
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
    "task": {
      "id": "5cd591b8-6cd1-4dd2-87d2-ede0a99ac066",
      "contextId": "4441e6c2-5d89-4715-9a9f-f9e4885d029e",
      "status": { "state": "TASK_STATE_COMPLETED", "timestamp": "2026-11-05T13:58:45.7168463+00:00" },
      "artifacts": [
        {
          "artifactId": "a147e2de-93ac-4c60-a764-efeae072d3df",
          "name": "analise-sofia-v1",
          "parts": [
            {
              "text": {
                "versaoContrato": "1.0",
                "idExecucao": "8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e",
                "idSolicitacao": "e1b3a4cb-d729-4bd4-b796-b9c94be07b46",
                "agente": { "idAgente": "agente-ecf", "nome": "Agente ECF" },
                "status": "parcial",
                "analise": "1 documento(s) recebido(s). Esta versão ainda não executa as validações da ECF: nenhum achado foi produzido e nenhuma conformidade é declarada.",
                "achados": [],
                "observacoes": [
                  "Documento doc-001 recebido: 29 caracteres, SHA-256 52ab0f0ad162f0e68f6e9424057bacdf43b209861b2475733611a420202a974d.",
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
}
```

## Desfechos

| Situação | `task.status.state` | `status` do contrato | `erro.codigo` |
| --- | --- | --- | --- |
| Solicitação válida | `TASK_STATE_COMPLETED` | `parcial` | — |
| Campo obrigatório ausente ou inválido, `idDocumento` repetido | `TASK_STATE_COMPLETED` | `erro` | `solicitacao_invalida` |
| `versaoContrato` diferente de `1.0` | `TASK_STATE_COMPLETED` | `erro` | `versao_contrato_nao_suportada` |
| Texto dos documentos acima do limite | `TASK_STATE_COMPLETED` | `erro` | `documentos_acima_do_limite` |
| Mensagem sem parte de texto, texto fora da estrutura JSON do contrato, `idExecucao` ou `idSolicitacao` que não são UUID | `TASK_STATE_FAILED` | sem resultado | motivo em `status.message` |

`concluido` fica reservado para quando todas as validações obrigatórias existirem (P05).

| Erro JSON-RPC | Quando |
| --- | --- |
| `-32700` | O corpo não é JSON. |
| `-32601` | Método desconhecido, inclusive os da v0.3, como `message/send`. |
| `-32602` | Parâmetros inválidos. |
| `-32001` | Tarefa não encontrada. |
| `-32002` | `CancelTask` em tarefa já terminada. |
| `-32003` | Configuração de notificações. |
| `-32004` | `SendStreamingMessage` e `SubscribeToTask` em tarefa terminada. |
| `-32009` | Versão desconhecida em `A2A-Version`. |

A resposta de `SendMessage` já é final, mesmo com `returnImmediately`. No Vercel, as instâncias não duram, então `GetTask` não é garantido. `ListTasks` responde sempre com lista vazia: sem identidade verificada, nenhum chamador vê as tarefas dos outros.

## Limite de tamanho

| Onde | Limite | O que acontece acima dele |
| --- | --- | --- |
| Vercel (produção) | Corpo da requisição entre 4,4 MB e 4,6 MB, medido em 2026-11-05 | HTTP 413 `FUNCTION_PAYLOAD_TOO_LARGE`, sem resposta JSON-RPC. A Sofia deve tratar como falha técnica. |
| Serviço | `A2A:TamanhoMaximoDosDocumentosEmBytes` (10 MiB) | Resultado com status `erro` e código `documentos_acima_do_limite`. Em produção, o limite do Vercel vem antes. |
| Kestrel (Docker) | 30 MB no corpo, padrão do ASP.NET Core | Erro HTTP 413. |

O corpo inclui o envelope e o escape do JSON, então o texto útil é um pouco menor. A decisão para arquivos reais fica na mudança 07 (DP-05).

## Configuração

Nenhum destes valores é segredo.

| Opção (variável de ambiente) | Padrão | Uso |
| --- | --- | --- |
| `A2A:UrlPublica` (`A2A__UrlPublica`) | Em produção, a URL do Vercel; fora dela, `http://localhost:8080/a2a` | Endereço anunciado no cartão |
| `A2A:TamanhoMaximoDosDocumentosEmBytes` | `10485760` (provisório até DP-05, na mudança 07) | Limite do texto somado dos documentos |
| `Cors:AllowedOrigins:<n>` (`Cors__AllowedOrigins__<n>`) | Vazio | Origens de navegador, só se houver frontend |

**CORS:** a chamada da Sofia é entre servidores e não depende de CORS. Preencha `Cors:AllowedOrigins` só se um frontend no navegador chamar o agente, com a origem desse frontend (esquema, host e porta, sem barra final). CORS não substitui autenticação.

**Autenticação:** a integração não usa token, por decisão de Kaio Macedo (SQ-12). O que voltaria se isso mudar está na seção "Autenticação adiada" de [`docs/roteiro.md`](../roteiro.md).

**Organização:** o contrato v1.0 não tem esse campo. A mudança 03 usa um valor padrão configurado.

**Privacidade:** `usuario.nome`, `pergunta` e o texto dos documentos não vão para logs, para a resposta nem para o armazenamento das tarefas. Os logs trazem `idExecucao`, `idSolicitacao`, status e o `X-Correlation-Id`.

## Como executar

Testes:

```bash
dotnet test --solution Agente-ECF.slnx -c Release
```

Na rede da Sofia, com Docker. A rede é a `sofia_network` do compose de `SofiaInfis`, cujo nome real é `sofia-infis_sofia_network`. Sem aquele compose no ar, crie a rede uma vez:

```bash
docker network create sofia-infis_sofia_network
```

```bash
docker compose up --build agente-ecf
```

```bash
curl http://localhost:8085/.well-known/agent-card.json
```

De outro contêiner na mesma rede, o endereço é o nome do serviço, não `localhost`:

```bash
docker run --rm --network sofia-infis_sofia_network curlimages/curl:8.10.1 http://agente-ecf:8080/.well-known/agent-card.json
```

A API da Sofia hoje roda fora do compose dela. Nesse caso, informe o endereço que ela enxerga:

```bash
A2A_URL_PUBLICA=http://localhost:8085/a2a docker compose up --build agente-ecf
```
