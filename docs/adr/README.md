# Architecture Decision Records

Registro das decisões arquiteturais do Agente-ECF, conforme a seção 6.5 da [constitution](../../openspec/constitution.md).

- Arquivo: `docs/adr/NNNN-titulo-em-kebab-case.md`, com numeração sequencial a partir de `0001`.
- Estados: Proposto, Aceito, Rejeitado, Substituído.
- Um ADR aceito não é reescrito. Uma nova decisão gera novo ADR, que indica qual substitui.

## Índice

| ADR | Título | Estado | Data |
| --- | --- | --- | --- |
| — | Nenhum ADR registrado. | — | — |

## Decisões pendentes que exigem ADR

| DP | Assunto |
| --- | --- |
| DP-01 | Armazenamento da memória conversacional: MongoDB ou Redis. |
| DP-02 | Modelo de embeddings e dimensão dos vetores. |
| DP-09 | Autenticação e autorização (REST, MCP, A2A e entre serviços). |
| DP-11 | Retenção e exclusão de dados (LGPD), quando não aprovada como política. |
| DP-12 | Política de dados com a OpenAI. |
| DP-17 | Organização física da solução, quando não resolvida no design. |
| DP-18 | Hospedagem, ambientes e CI/CD. |
| DP-25 | Armazenamento das execuções e resultados, quando não resolvido no design. |

## Modelo

```markdown
# NNNN — Título da decisão

| Campo | Valor |
| --- | --- |
| Estado | Proposto |
| Data | AAAA-MM-DD |
| Responsáveis | |
| Decisões pendentes resolvidas | DP-xx |
| Princípios relacionados | Pxx, RA-xx |
| Substitui / Substituído por | — |

## Contexto

Problema, restrições e forças envolvidas.

## Decisão

O que foi decidido, em termos verificáveis.

## Alternativas consideradas

| Alternativa | Vantagens | Desvantagens | Motivo da rejeição |
| --- | --- | --- | --- |

## Consequências

Efeitos positivos, negativos e riscos; impacto em specs, contratos e operação.

## Verificação

Como confirmar, em revisão ou teste, que a decisão está sendo cumprida.
```
