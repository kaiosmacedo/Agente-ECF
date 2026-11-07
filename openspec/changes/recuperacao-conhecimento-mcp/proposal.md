# Proposal: Recuperação de conhecimento por MCP

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 06; PR #4). Em 2026-11-07, o assistente fez a prova de conceito do MCP e propôs as respostas dos esclarecimentos, os requisitos novos (REQ-RECUP-005 e 006, REQ-CONSULTA-005 a 007), o ADR 0005, o plano técnico e as tarefas, que aguardam a aprovação de Kaio Macedo.

## Why

A etapa 5 do fluxo (P03) consulta o manual da versão identificada. A constitution exige que essa consulta aconteça somente por MCP, com a Api.RAG como servidor e a Api.Agente como cliente (P15, RA-03), filtrada por tipo e versão (P11), com referências recuperáveis (P06) e registradas para conferir o que o LLM citar (P07).

## What Changes

- Expõe na Api.RAG uma ferramenta MCP de busca no manual, com contrato versionado de entrada e saída.
- Aplica filtro obrigatório por tipo de documento e versão do manual, sem misturar versões.
- Implementa o cliente MCP na Api.Agente para a etapa 5, com timeout e registro dos chunks recebidos na execução; a autenticação de serviço fica adiada (SQ-12).
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
| E01 | A prova de conceito confirma o transporte MCP com streaming no Vercel? Se não, a Api.RAG vai para um host persistente (novo ADR). | **Proposta, com prova de conceito local em 2026-11-07:** sim, em modo sem estado. Com o SDK oficial 2.4.0, `tools/list` e `tools/call` funcionaram em requisições alternadas entre duas instâncias, inclusive com uma derrubada; com sessão, o mesmo teste falhou com 404. A resposta é `text/event-stream` com um único evento. Falta confirmar no Vercel, na primeira entrega (tarefa 6.2). Detalhes e alternativa no ADR 0005. | Assistente, com validação de Kaio Macedo | 2026-11-07 | Não, depois da validação |
| E02 | Quantos trechos retornar e com que limiar de relevância (DP-03)? Definido pela avaliação com o conjunto de DP-24. | **Proposta:** 5 trechos, o valor já avaliado (96,7% de acerto nos 5 primeiros, `docs/avaliacao/recuperacao.md`). O limiar sai da avaliação de D4, com perguntas que o manual não responde. | Avaliação no plano técnico | 2026-11-07 | Não |
| E03 | Timeout e retentativas da chamada MCP (DP-14). | **Proposta:** 10 segundos por chamada e até 2 retentativas, só para falha transitória, com espera de 0,5 e 1 segundo (D6). | Plano técnico | 2026-11-07 | Não |
| E04 | Que chave da OpenAI a Api.RAG usa em produção para gerar o embedding de cada consulta? A chave do `SofiaInfis` foi autorizada para popular a base. | **Proposta:** uma chave própria do Agente-ECF, com limite de gasto no painel da OpenAI, cadastrada por Kaio Macedo no Vercel (SQ-03). Até o cadastro, a busca fica desativada em produção, e a etapa 5 registra a limitação, sem falha técnica (D5, D10). | Kaio Macedo | — | Não para implementar; sim para usar em produção |
| E05 | A busca deve achar uma regra pelo código, como `REGRA_...`? A pergunta P15 da avaliação falhou por isso. | **Proposta:** sim. Trechos que contêm o código vêm primeiro, completados pela busca semântica, com avaliação antes de adotar (D3, D4; REQ-RECUP-005). | Assistente; aprovação de Kaio Macedo | 2026-11-07 | Não |
| E06 | Qual o limite de uso da ferramenta, que é pública na primeira versão e gera custo (SQ-12)? | **Proposta:** 2.000 consultas por dia, contadas no `consumo_ia`, e desligamento por configuração. Cada consulta custa cerca de US$ 0,0000004, então o limite protege mais contra abuso do que contra custo (D5; REQ-RECUP-006). | Assistente; aprovação de Kaio Macedo | 2026-11-07 | Não |
| E07 | O que a etapa 5 consulta antes da análise com LLM (mudança 10)? | **Proposta:** uma consulta por regra violada, até 10 por execução, para ligar o trecho do manual a cada achado. Sem achado, a etapa fica não aplicável, com justificativa prevista. A mudança 10 amplia as consultas para a análise (D7; REQ-CONSULTA-005). | Assistente; aprovação de Kaio Macedo | 2026-11-07 | Não |

## Fontes oficiais

Não se aplica: a recuperação devolve trechos das fontes ingeridas na mudança 05, sem criar regras.

## Princípios, decisões e dependências

- **Princípios:** P06, P07, P08, P10, P11, P15, P17, P19; RA-03; SQ-06, SQ-12.
- **Decisões pendentes:** contribui para DP-03 e DP-14.
- **Depende de:** 03 `execucao-e-resultado` (registro da etapa 5) e 05 `base-conhecimento-ingestao`. A autenticação de serviço fica adiada (SQ-12; `docs/roteiro.md`).
- **Skills:** `mcp-client-server`, `rag-pgvector`, `vercel-dotnet-pgvector-atlas` (prova de conceito), `testes-agentes-ia`, `github-versionamento`.
- **Base:** consulta vetorial do `SofiaInfis`, com filtro e limiar (`docs/base-sofiainfis.md`).

## Fora do escopo

- Uso dos trechos pelo LLM (mudança 10).
- Outras ferramentas MCP além da busca no manual.

## Impact

- Servidor MCP na Api.RAG, cliente MCP na Api.Agente, contrato versionado da ferramenta e possível ADR de hospedagem da Api.RAG (E01).

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
