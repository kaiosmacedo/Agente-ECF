# Design: Telemetria dos serviços

> **Estado:** aprovado por Kaio Macedo em 2026-11-05, junto com as tarefas.

## Context

Os dois serviços têm logs em JSON, correlação por `X-Correlation-Id` e saúde. Os logs já trazem `TraceId` e `SpanId` do ASP.NET Core, mas nada é medido nem exportado, e SQ-07 exige métricas. O SDK A2A já emite traces e métricas, que hoje ninguém coleta.

## Goals / Non-Goals

**Goals**

- Traces e métricas padronizados nos dois serviços, sem custo e sem destino obrigatório.
- Continuidade do trace vindo da Sofia.
- Ver os dados localmente, sem contratar nada.

**Non-Goals**

- Métricas de tokens e custos (mudança 04).
- Destino de produção, alertas e painéis.

## Decisions

### D1. OpenTelemetry, versões estáveis

`OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http` e `OpenTelemetry.Exporter.OpenTelemetryProtocol`, todos 1.20.1. É o padrão aberto, a mesma família referenciada no `SofiaInfis`, onde há uma versão `rc`. **Alternativa:** só `System.Diagnostics.Metrics`, sem exportação, o que não atenderia a chegada a um destino.

### D2. Sem exportador de console

O `SofiaInfis` referencia o exportador de console. Aqui ele quebraria REQ-OBSERV-002, que exige toda a saída em JSON e é verificada no CI. Sem destino configurado, nada é exportado.

### D3. O que é coletado

- **Traces:** ASP.NET Core, HttpClient e, na Api.Agente, as fontes do SDK A2A, sem as rotas de saúde. Os nomes das fontes são conferidos no SDK instalado.
- **Métricas:** ASP.NET Core, HttpClient e os medidores do SDK A2A.
- **Recurso:** `service.name` e `service.version`, de `IdentidadeServico`.

### D4. Dados sensíveis (SQ-04)

As instrumentações não capturam corpo nem cabeçalhos. A query string fica mascarada, comportamento padrão delas, conferido por teste. O SDK A2A registra identificadores, nunca o conteúdo das partes; também conferido por teste.

### D5. Opções

`Telemetria:Endpoint`, opcional e validada como URL. Coleta sempre ligada, para os testes e para os logs terem o trace; o exportador só é registrado com o endereço configurado. Uma falha do destino é tratada pelo exportador em segundo plano, sem afetar a requisição.

### D6. Um registro por serviço

Código espelhado em `Infraestrutura/Observabilidade` de cada serviço, como no restante da fundação (ADR 0001, RA-10), sem biblioteca compartilhada.

### D7. Painel local

Perfil `observabilidade` no `compose.yaml`, com o Aspire Dashboard, gratuito e sem persistência, apontado pelos serviços via `Telemetria__Endpoint`. Fica fora do fluxo padrão.

### D8. Testes

Exportador em memória (`OpenTelemetry.Exporter.InMemory`, 1.20.1, só nos testes) para conferir traces, continuidade do `traceparent`, exclusão da saúde, métrica por rota e ausência de dados sensíveis.

## Risks / Trade-offs

- [Coleta sem destino gasta um pouco de CPU] → instrumentação padrão, de custo baixo; desligar seria mais caro de testar.
- [Dados só locais até haver destino] → aceito; os logs continuam no Vercel. E01 fica aberto, sem bloquear.

## Migration Plan

Merge e publicação pelo CD, sem configuração nova em produção. **Rollback:** promover o deploy anterior no Vercel.

## Open Questions

- E01: destino de produção, quando for necessário.

## Skills aplicáveis

| Skill | Pré-condição | Situação |
| --- | --- | --- |
| `testes-agentes-ia` | Testes determinísticos. | Atendida (D8). |
| `github-versionamento` | Branch por mudança e PR. | Atendida. |

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Não se aplica | Sem regras fiscais. |
| P02 | Não se aplica | Sem validações. |
| P03 | Não se aplica | Sem etapas do fluxo. |
| P04 | Não se aplica | Sem achados. |
| P05 | Não se aplica | Sem status. |
| P06 | Não se aplica | Sem achados. |
| P07 | Não se aplica | Sem LLM. |
| P08 | Não se aplica | Sem análise. |
| P09 | Não se aplica | Sem análise. |
| P10 | Não se aplica | Conteúdo recebido não entra na telemetria (D4). |
| P11 | Não se aplica | Sem base de conhecimento. |
| P12 | Não se aplica | Sem LLM. |
| P13 | Atende | Identidade do serviço preenchida pelo sistema (D3). |
| P14 | Não se aplica | Sem ferramentas. |
| P15 | Atende | Registro por serviço, sem biblioteca compartilhada (D6). |
| P16 | Atende | Pacotes justificados em D1 e D8; sem projeto novo. |
| P17 | Não se aplica | Consumo entra na mudança 04. |
| P18 | Não se aplica | Sem memória. |
| P19 | Atende | Traces ajudam a explicar cada execução. |
| P20 | Atende | Esta mudança; tarefas ligadas aos requisitos. |
