# Proposal: Memória conversacional

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 12; PR #4). Esclarecimentos propostos em 2026-11-10, à espera da aprovação de Kaio Macedo. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

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
| E01 | MongoDB (por exemplo, MongoDB Atlas, como no DIMP) ou Redis (DP-01)? O ADR compara os dois. | **Proposta:** o PostgreSQL que a Api.Agente já usa (Neon), em vez de MongoDB ou Redis: nenhum fornecedor, conta ou segredo novo, e os testes já sobem esse banco. A retenção vira data de expiração, filtrada na leitura e apagada na gravação. Fica fora das duas opções da DP-01, então o ADR registra a troca, e a skill `memoria-mongodb` não se aplica. Alternativa: MongoDB Atlas, com a skill pronta e expiração nativa, mas que exige criar o cluster e cadastrar a conexão. | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Sim |
| E02 | Por quanto tempo guardar as conversas e quem pode excluí-las (DP-11)? | **Proposta:** enquanto houver só dados sintéticos, 30 dias, configurável, e exclusão da conversa pelo próprio usuário declarado. A política para dados reais segue em aberto, com a pergunta 18 do levantamento para o superior (DP-11). | Kaio Macedo, com o superior | 2026-11-10 | Sim, para dados reais |
| E03 | Qual o uso esperado da conversa: perguntas sobre uma análise já feita, conversa livre sobre a ECF, ou os dois? | **Proposta:** só perguntas de acompanhamento sobre uma análise já feita, vindas da Sofia e ligadas ao `idExecucao` e ao `contextId` do A2A. A conversa livre sobre a ECF fica para depois. Isso pede uma nova skill no cartão, com versão nova e campos só opcionais no contrato SOFIA. | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Sim |
| E04 | A Sofia INFIS já guarda o histórico das conversas? Se sim, como evitar memória duplicada? | **Fato e proposta:** a cópia local do `SofiaInfis` (2026-11-05) não guarda histórico, e o pacote do MongoDB está lá sem uso (`docs/base-sofiainfis.md`). O agente guarda só as perguntas e as respostas dele sobre cada análise. Confirmar com quem mantém a Sofia antes do teste real. | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Não |

## Fontes oficiais

Não se aplica: sem regras de domínio fiscal.

## Princípios, decisões e dependências

- **Princípios:** P10, P18; GD-09, GD-11; SQ-04, SQ-12.
- **Decisões pendentes:** resolve DP-01 (ADR); depende de DP-11.
- **Depende de:** 10 `analise-llm`. O isolamento pela identidade autenticada fica adiado (SQ-12; `docs/roteiro.md`).
- **Skills:** `memoria-mongodb` (se E01 escolher MongoDB), `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Uso da memória como fonte de conhecimento (proibido por P18).
- Memória compartilhada entre organizações ou usuários.

## Impact

- Armazenamento de memória (MongoDB ou Redis), endpoint de conversa ou integração com a análise, e ADR de DP-01.

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
