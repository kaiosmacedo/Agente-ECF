# Proposal: Regras de negócio de IRPJ, CSLL e cruzamentos

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 09; PR #4). Próxima etapa: esclarecimentos. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

## Why

O objetivo do agente inclui verificar a consistência da apuração do IRPJ e da CSLL, os cruzamentos entre registros relacionados e a compatibilidade com informações contábeis e de períodos anteriores, quando aplicável. A constitution exige que essas regras sejam determinísticas e tenham fonte oficial (P01, P02), e que a falta de dados externos vire ausência de evidências, nunca aprovação (P08).

## What Changes

- Cria o catálogo de regras de negócio com fonte oficial, começando pelo escopo definido em E01.
- Implementa cada regra como validador determinístico de cruzamento entre registros, com testes.
- Produz achados da categoria inconsistência de negócio citando os registros dos dois lados do cruzamento.
- Trata regra que depende de dados externos indisponíveis (período anterior, escrituração contábil) como ausência de evidências.
- Executa as regras somente sobre registros com validação estrutural concluída.

## Capabilities

### New Capabilities
- `agente/regras-negocio`: validação determinística de regras de negócio e cruzamentos da ECF, incluindo IRPJ e CSLL.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Quais regras de IRPJ, CSLL e cruzamentos entram na primeira entrega (DP-06)? | — | Kaio Macedo | — | Sim |
| E02 | Para comparar com períodos anteriores, o usuário envia a ECF anterior? A escrituração contábil (ECD) também entra (DP-06)? | — | Kaio Macedo | — | Sim, para as regras que dependem disso |
| E03 | Quais fontes oficiais além do manual da ECF fundamentam as regras (por exemplo, legislação ou atos normativos)? Cada uma precisa de registro com versão e localização (P01). | — | Kaio Macedo | — | Sim |
| E04 | Quem tem responsabilidade de domínio fiscal para validar cada regra antes da implementação? | — | Kaio Macedo | — | Sim |

## Fontes oficiais

| Documento | Emissor | Versão ou vigência | Localização |
| --- | --- | --- | --- |
| Manual da ECF, leiaute 12 | Receita Federal do Brasil, portal SPED | Publicado em 20/05/2026 | A extrair por regra (E01, E03) |

Outras fontes entram somente quando registradas em E03.

## Princípios, decisões e dependências

- **Princípios:** P01, P02, P04, P06, P08; SQ-09, SQ-10.
- **Decisões pendentes:** resolve DP-06; depende de DP-04.
- **Depende de:** 08 `validacao-estrutural`.
- **Skills:** `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Interpretações tributárias sem fonte oficial (constitution, 1.3).
- Indícios identificados pelo LLM (mudança 10).

## Impact

- Catálogo de regras de negócio, validadores de cruzamento e fixtures sintéticas na Api.Agente; possível entrada de arquivos complementares (E02).

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
