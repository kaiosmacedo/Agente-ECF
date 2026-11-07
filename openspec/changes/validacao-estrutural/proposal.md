# Proposal: Validação estrutural da ECF

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 08; PR #4). Próxima etapa: esclarecimentos. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

## Why

A etapa 4 do fluxo (P03) valida estrutura, registros e campos do arquivo segundo o leiaute oficial. A constitution exige que essas regras sejam determinísticas, testadas e com fonte no manual (P01, P02), e que o conjunto de validações obrigatórias de cada versão esteja definido para que o agente nunca declare conformidade com validações pendentes (P05).

## What Changes

- Cria o catálogo de regras estruturais por versão do leiaute, extraído do Manual da ECF, com identificador, fonte e localização de cada regra.
- Implementa cada regra como validador determinístico, com testes de caso válido e inválido.
- Produz achados da categoria erro estrutural, com registro, linha e campo.
- Lista como validações não realizadas as regras obrigatórias ainda não implementadas.
- Trata falha interna de validador como falha técnica, não como erro do documento.

## Capabilities

### New Capabilities
- `agente/validacao-estrutural`: validação determinística da estrutura, dos registros e dos campos do arquivo por versão do leiaute.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Quais grupos de regras estruturais do manual entram na primeira entrega (por exemplo: registros existentes, hierarquia, campos obrigatórios, tipos e tamanhos)? | — | Kaio Macedo | — | Sim |
| E02 | O catálogo de regras é extraído do Manual da ECF, leiaute 12, pelo assistente e validado por quem tem responsabilidade de domínio? Quem valida (P01)? | O assistente extrai o catálogo da versão em Word do manual, com uma versão por regra, e Kaio Macedo aprova. Sem especialista fiscal na equipe (`docs/classificacao-das-regras.md`). | Kaio Macedo | 2026-11-06 | Não |
| E03 | Qual a relação com o programa validador oficial da ECF: reproduzir suas regras, complementar, ou só referenciar (DP-04)? | As regras vêm do manual. O programa oficial serve de gabarito: uma pessoa roda arquivos sintéticos, sem assinar nem transmitir, antes de fechar esta mudança (decisão 6). | Kaio Macedo | 2026-11-06 | Não |
| E04 | Regras que dependem de tabelas publicadas à parte do manual: quais tabelas e em que versão? | — | Kaio Macedo | — | Não; ficam como validações não realizadas até a fonte ser registrada |

## Fontes oficiais

| Documento | Emissor | Versão ou vigência | Localização |
| --- | --- | --- | --- |
| Manual da ECF, leiaute 12 | Receita Federal do Brasil, portal SPED | Publicado em 23/07/2026 | Seções de leiaute dos registros: a extrair por regra (E02) |

Nenhuma regra estrutural é aprovada sem localização no manual.

## Princípios, decisões e dependências

- **Princípios:** P01, P02, P04, P05, P06, P08; SQ-09, SQ-10.
- **Decisões pendentes:** depende de DP-04.
- **Depende de:** 07 `recepcao-identificacao-leiaute`.
- **Skills:** `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Cruzamentos entre registros e apuração do IRPJ e da CSLL (mudança 09).
- Interpretações tributárias.

## Impact

- Catálogo versionado de regras, validadores e fixtures sintéticas por regra na Api.Agente.

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
