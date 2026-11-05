# Proposal: Telemetria dos serviços

> **Estado:** proposta, plano técnico e tarefas aprovados por Kaio Macedo em 2026-11-05.

## Why

A constitution exige que os serviços exponham métricas de latência e erros (SQ-07), mas nenhuma mudança do roteiro cuida disso. Hoje há logs em JSON, correlação e saúde, sem traces nem métricas. Fazer isso antes do banco e das chamadas de IA deixa as próximas mudanças já observáveis. O projeto `SofiaInfis` referencia os pacotes de OpenTelemetry, mas não os configura (`docs/base-sofiainfis.md`).

## What Changes

- Gera traces das requisições e das chamadas HTTP de saída, continuando o `traceparent` recebido.
- Expõe métricas de duração e contagem das requisições, com nome e versão do serviço.
- Exporta por OTLP somente quando `Telemetria:Endpoint` estiver configurado.
- Mantém fora da telemetria o corpo das requisições, a query string, as credenciais e o conteúdo dos documentos.
- Oferece um painel local opcional, no `compose.yaml`.

Métricas de tokens e custos entram na mudança 04.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `plataforma/observabilidade`: acrescenta traces, métricas e exportação opcional.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Para onde exportar em produção (por exemplo, Grafana Cloud ou Azure Monitor)? Sem destino, a telemetria fica desligada em produção e os logs continuam no Vercel. | — | Kaio Macedo | — | Não |

## Fontes oficiais

Não se aplica: sem regras de domínio fiscal.

## Princípios, decisões e dependências

- **Princípios:** P13, P15, P16, P19; SQ-04, SQ-07.
- **Decisões pendentes:** nenhuma.
- **Depende de:** 01 `fundacao-solucao`.
- **Skills:** `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Métricas de tokens e custos (mudança 04).
- Contratar ou operar um destino de telemetria.
- Alertas e painéis de produção.

## Impact

- Pacotes `OpenTelemetry.*` nos dois serviços; pasta de telemetria em `Infraestrutura/Observabilidade`.
- Serviço opcional de painel no `compose.yaml`, com perfil próprio.

## Aprovação

Aprovada por Kaio Macedo em 2026-11-05, com a reorganização do roteiro.
