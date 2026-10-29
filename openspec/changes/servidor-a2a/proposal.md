# Proposal: Servidor A2A para a Sofia INFIS

> **Estado:** especificada para validação de Kaio Macedo (roteiro, mudança 13). Plano técnico e tarefas só depois da aprovação e dos esclarecimentos bloqueantes.

## Why

O Agente-ECF é um dos agentes da Sofia INFIS, que deve poder acioná-lo por A2A (constitution, 1.1). A constitution exige que o servidor A2A use o mesmo fluxo, a mesma autenticação e o mesmo contrato de resultado da API REST, e que não exponha capacidade não especificada (RA-07).

## What Changes

- Publica o cartão do agente com as capacidades especificadas: analisar um arquivo da ECF e consultar o resultado de uma análise.
- Recebe tarefas A2A e as executa pelo mesmo fluxo da `POST /analises`, com o mesmo contrato de resultado.
- Autentica a Sofia INFIS conforme o ADR de DP-09 e DP-16.
- Propaga a correlação da Sofia para os logs e reflete o status da execução no estado da tarefa A2A.
- Executa antes uma prova de conceito do transporte A2A no Vercel (ADR 0002).

## Capabilities

### New Capabilities
- `integracao/servidor-a2a`: exposição do Agente-ECF a outros agentes, a começar pela Sofia INFIS, pelo protocolo A2A.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Qual versão do protocolo A2A e qual SDK a Sofia INFIS usa? | — | Kaio Macedo | — | Sim |
| E02 | Quais capacidades expor no início: só analisar e consultar, ou também conversar (DP-16)? | — | Kaio Macedo | — | Sim |
| E03 | A Sofia precisa de streaming ou notificações de andamento? A prova de conceito verifica o suporte no Vercel. | — | Kaio Macedo | — | Sim |
| E04 | Que cabeçalho de correlação a Sofia envia (pendência E06 da fundação)? | — | Kaio Macedo | — | Não |

## Fontes oficiais

Não se aplica: sem regras de domínio fiscal.

## Princípios, decisões e dependências

- **Princípios:** P03, P05, P10, P19; RA-07; SQ-01, SQ-07.
- **Decisões pendentes:** resolve DP-16; usa DP-09.
- **Depende de:** 02 `autenticacao-autorizacao`, 03 `execucao-e-resultado` e 10 `analise-llm` (para a análise completa).
- **Skills:** `vercel-dotnet-pgvector-atlas` (prova de conceito), `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Capacidades não especificadas no cartão do agente.
- Cliente A2A do Agente-ECF para chamar outros agentes.

## Impact

- Servidor A2A na Api.Agente, cartão do agente versionado e, conforme a prova de conceito, possível host persistente (novo ADR).

## Aprovação

Pendente da validação de Kaio Macedo.
