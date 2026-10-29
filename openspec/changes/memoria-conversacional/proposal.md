# Proposal: Memória conversacional

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 12; PR #4). Próxima etapa: esclarecimentos. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

## Why

A Api.Agente deve gerenciar memória conversacional, para que perguntas de acompanhamento sobre uma análise aproveitem o histórico. A constitution exige isolamento por usuário, conversa e organização, retenção definida, limite de histórico enviado ao modelo e separação total entre memória e conhecimento oficial (P18).

## What Changes

- Persiste mensagens de conversa no armazenamento decidido em ADR (DP-01), com isolamento por usuário, conversa e organização.
- Limita o histórico enviado ao modelo por parâmetro configurável, em ordem cronológica.
- Aplica a política de retenção, expiração e exclusão (DP-11) e permite excluir uma conversa.
- Impede que a memória seja usada como fonte, evidência ou referência.
- Define o comportamento quando a memória estiver indisponível.

## Capabilities

### New Capabilities
- `agente/memoria-conversacional`: histórico de conversas isolado, limitado e com retenção, separado do conhecimento oficial.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | MongoDB (por exemplo, MongoDB Atlas, como no DIMP) ou Redis (DP-01)? O ADR compara os dois. | — | Kaio Macedo | — | Sim |
| E02 | Por quanto tempo guardar as conversas e quem pode excluí-las (DP-11)? | — | Kaio Macedo | — | Sim, para dados reais |
| E03 | Qual o uso esperado da conversa: perguntas sobre uma análise já feita, conversa livre sobre a ECF, ou os dois? | — | Kaio Macedo | — | Sim |
| E04 | A Sofia INFIS já guarda o histórico das conversas? Se sim, como evitar memória duplicada? | — | Kaio Macedo | — | Não |

## Fontes oficiais

Não se aplica: sem regras de domínio fiscal.

## Princípios, decisões e dependências

- **Princípios:** P10, P18; GD-09, GD-11; SQ-02, SQ-04.
- **Decisões pendentes:** resolve DP-01 (ADR); depende de DP-11.
- **Depende de:** 02 `autenticacao-autorizacao` e 10 `analise-llm`.
- **Skills:** `memoria-mongodb` (se E01 escolher MongoDB), `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Uso da memória como fonte de conhecimento (proibido por P18).
- Memória compartilhada entre organizações ou usuários.

## Impact

- Armazenamento de memória (MongoDB ou Redis), endpoint de conversa ou integração com a análise, e ADR de DP-01.

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
