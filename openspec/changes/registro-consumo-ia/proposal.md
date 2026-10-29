# Proposal: Registro de consumo e custos de IA

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 04; PR #4). Próxima etapa: esclarecimentos. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

## Why

A constitution exige registrar toda chamada a provedor de IA, inclusive retentativas, sem transformar consumo desconhecido em zero (P17, GD-10). A primeira chamada à OpenAI acontece na ingestão de manuais (mudança 05), que gera embeddings. Por isso o registro precisa existir antes, nos dois serviços, cada um no próprio banco (P15).

## What Changes

- Registra no PostgreSQL de cada serviço uma linha por chamada ou tentativa a provedor de IA (geração, embedding, retentativa e auxiliar), com os campos de GD-10.
- Distingue valor informado pelo provedor de valor estimado pelo sistema e mantém desconhecido como desconhecido.
- Mantém tabela de preços versionada com vigência e referencia a versão em cada estimativa.
- Torna observável a falha ao gravar o consumo (log de erro e métrica).
- Oferece consulta agregada por execução, modelo e período para perfis autorizados.

## Capabilities

### New Capabilities
- `plataforma/consumo-custos`: registro, estimativa e consulta do consumo de chamadas a provedores de IA, em cada serviço.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Em que moeda registrar o custo (USD, como a OpenAI cobra, ou BRL com conversão) e qual a fonte da tabela de preços (DP-13)? | — | Kaio Macedo | — | Sim |
| E02 | É preciso um relatório consolidado dos dois serviços? Se sim, por consulta a cada serviço ou por exportação (DP-26)? | — | Kaio Macedo | — | Não |
| E03 | Quem pode consultar os custos (perfil administrador, responsável financeiro)? | — | Kaio Macedo | — | Não |
| E04 | Provedor de PostgreSQL gerenciado (o mesmo da mudança 03, E06). | — | Kaio Macedo | — | Sim, para persistir |

## Fontes oficiais

Não se aplica: sem regras de domínio fiscal. A tabela de preços é referência comercial do provedor, versionada com vigência.

## Princípios, decisões e dependências

- **Princípios:** P13, P15, P17, P19; GD-10; SQ-02, SQ-04.
- **Decisões pendentes:** resolve DP-13 e DP-26.
- **Depende de:** 01 `fundacao-solucao` e 02 `autenticacao-autorizacao` (consulta restrita).
- **Skills:** `controle-custos-llm`, `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Faturamento, cobrança ou rateio entre clientes.
- Alertas de orçamento (podem virar mudança própria).

## Impact

- Esquema de consumo nos bancos da Api.Agente e da Api.RAG e um ponto central de medição usado pelas mudanças 05 e 10.

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
