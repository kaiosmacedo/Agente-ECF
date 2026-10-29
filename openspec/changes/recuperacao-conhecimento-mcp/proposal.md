# Proposal: Recuperação de conhecimento por MCP

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 06; PR #4). Próxima etapa: esclarecimentos. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

## Why

A etapa 5 do fluxo (P03) consulta o manual da versão identificada. A constitution exige que essa consulta aconteça somente por MCP, com a Api.RAG como servidor e a Api.Agente como cliente (P15, RA-03), filtrada por tipo e versão (P11), com referências recuperáveis (P06) e registradas para conferir o que o LLM citar (P07).

## What Changes

- Expõe na Api.RAG uma ferramenta MCP de busca no manual, com contrato versionado de entrada e saída.
- Aplica filtro obrigatório por tipo de documento e versão do manual, sem misturar versões.
- Implementa o cliente MCP na Api.Agente para a etapa 5, com timeout, autenticação de serviço e registro dos chunks recebidos na execução.
- Trata indisponibilidade como falha técnica e limitação, nunca como consulta realizada.
- Registra o consumo dos embeddings de consulta na Api.RAG.
- Executa antes uma prova de conceito do MCP no Vercel (ADR 0002).

## Capabilities

### New Capabilities
- `rag/recuperacao-conhecimento`: ferramentas MCP de busca no manual expostas pela Api.RAG.
- `agente/consulta-conhecimento`: uso da recuperação pela Api.Agente na etapa 5 do fluxo.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | A prova de conceito confirma o transporte MCP com streaming no Vercel? Se não, a Api.RAG vai para um host persistente (novo ADR). | — | Assistente, com validação de Kaio Macedo | — | Sim |
| E02 | Quantos trechos retornar e com que limiar de relevância (DP-03)? Definido pela avaliação com o conjunto de DP-24. | — | Avaliação no plano técnico | — | Não |
| E03 | Timeout e retentativas da chamada MCP (DP-14). | — | Plano técnico | — | Não |

## Fontes oficiais

Não se aplica: a recuperação devolve trechos das fontes ingeridas na mudança 05, sem criar regras.

## Princípios, decisões e dependências

- **Princípios:** P06, P07, P08, P10, P11, P15, P17, P19; RA-03; SQ-01, SQ-06.
- **Decisões pendentes:** contribui para DP-03 e DP-14.
- **Depende de:** 03 `execucao-e-resultado` (registro da etapa 5) e 05 `base-conhecimento-ingestao`. Usa a autenticação de serviço de 02.
- **Skills:** `mcp-client-server`, `rag-pgvector`, `vercel-dotnet-pgvector-atlas` (prova de conceito), `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Uso dos trechos pelo LLM (mudança 10).
- Outras ferramentas MCP além da busca no manual.

## Impact

- Servidor MCP na Api.RAG, cliente MCP na Api.Agente, contrato versionado da ferramenta e possível ADR de hospedagem da Api.RAG (E01).

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
