# Proposal: Execução da análise e contrato de resultado

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 03; PR #4). Próxima etapa: esclarecimentos. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

## Why

As etapas de validação, RAG e LLM das próximas mudanças precisam de uma espinha comum: o registro da execução com o estado de cada uma das nove etapas (P03), o cálculo determinístico do status (P05) e um resultado com categorias, evidências, limitações e revisão humana (P04, P06, P08, P09). Definir isso antes das etapas evita que cada mudança invente seu próprio formato e garante que o agente nunca declare conformidade indevida.

## What Changes

- Cria o registro da execução: identificador gerado pelo sistema, identidade, hash do arquivo e estado das nove etapas.
- Implementa o cálculo determinístico do status e da necessidade de revisão humana.
- Define o envelope da API e o contrato de achados (categorias, origem, evidências, severidade, limitações), versionados.
- Expõe a consulta do resultado de uma execução (`GET /analises/{id}`), sem autenticação na primeira versão (SQ-12).
- Persiste execuções e resultados no PostgreSQL da Api.Agente (DP-25).

A criação de execuções a partir do envio do arquivo entra na mudança 07; aqui, os testes criam execuções diretamente.

## Capabilities

### New Capabilities
- `agente/execucao-analise`: ciclo de vida da execução, estados das etapas, cálculo do status e auditoria.
- `agente/contrato-resultado`: envelope da API e contrato dos achados, das evidências, das limitações e da revisão humana.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Quais nomes de status usar internamente? A constitution exige distinguir ao menos entrada rejeitada, concluída, parcial e falha técnica (DP-08). Para a Sofia, o contrato v1.0 só admite `concluido`, `parcial` e `erro`, então cada status interno precisa de um mapeamento. | — | Kaio Macedo | — | Sim |
| E02 | Qual o critério de cada severidade (DP-07)? O contrato SOFIA v1.0 fixa a escala em `baixa`, `media`, `alta` e `critica` (opcional no achado), e a nossa precisa ser compatível. | — | Kaio Macedo | — | Sim |
| E03 | O resultado é entregue só em JSON ou também como relatório legível (HTML ou PDF)? Para quem (DP-15)? | — | Kaio Macedo | — | Não; o JSON vem primeiro |
| E04 | Uma referência citada pelo LLM que não existe: o item é rejeitado ou reclassificado como ausência de evidências (DP-23)? | — | Kaio Macedo | — | Não; vale a partir da mudança 10 |
| E05 | Por quanto tempo guardar execuções e resultados (DP-11)? | — | Kaio Macedo | — | Não, para dados sintéticos |
| E06 | Qual provedor de PostgreSQL gerenciado usar com o Vercel (por exemplo, Neon ou Supabase), como no DIMP? | — | Kaio Macedo | — | Sim, para persistir |
| E07 | Como a primeira versão obtém usuário e organização, sem autenticação (SQ-12)? Pelo contrato SOFIA, o usuário é `usuario.nome` (dado pessoal, minimizar persistência). O contrato não tem organização: usar valor padrão configurado ou cabeçalho declarado? | — | Kaio Macedo | — | Sim |

## Fontes oficiais

Não se aplica: o contrato não contém regras fiscais. As regras que produzem achados entram nas mudanças 08 e 09.

## Princípios, decisões e dependências

- **Princípios:** P03, P04, P05, P06, P08, P09, P12, P13, P19; GD-01 a GD-05; SQ-12.
- **Decisões pendentes:** resolve DP-07, DP-08 e DP-25; depende de DP-11 para dados reais; DP-15 e DP-23 podem ser resolvidas aqui ou adiadas para as mudanças 10 e posteriores.
- **Depende de:** 01 `fundacao-solucao` e 13 `servidor-a2a` (contrato SOFIA). As exigências de autenticação ficam adiadas (SQ-12; `docs/roteiro.md`). A execução guarda `idExecucao` e `idSolicitacao` da Sofia, além do próprio identificador, e deduplica retentativas por `idSolicitacao` quando houver efeito persistido.
- **Skills:** `sofia-a2a-contrato`, `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Recepção do arquivo (07), validações (08, 09), RAG (06) e LLM (10).
- Relatório legível em HTML ou PDF, se E03 decidir por ele (mudança própria).

## Impact

- Novo endpoint `GET /analises/{id}` na Api.Agente e o primeiro banco da Api.Agente (PostgreSQL, com migrações e Testcontainers).
- Contratos versionados do envelope e dos achados em `docs/contratos/` (local definido no plano técnico).

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
