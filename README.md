# Agente-ECF

Agente de IA que analisa arquivos da Escrituração Contábil Fiscal (ECF), valida sua estrutura e a consistência das informações e gera um relatório de inconsistências com evidências e referências aos manuais oficiais.

**Situação:** especificação. A [constitution](openspec/constitution.md) v1.2.0 está vigente; a v1.1.0 foi ratificada por Kaio Macedo em 2026-10-29. Nenhum código de implementação foi criado.

## Estrutura

| Caminho | Conteúdo |
| --- | --- |
| `openspec/constitution.md` | Princípios, restrições e critérios obrigatórios. Fonte normativa do projeto. |
| `openspec/config.yaml` | Contexto e regras aplicados pelo OpenSpec a cada artefato. |
| `openspec/specs/` | Especificações vigentes, por capacidade. |
| `openspec/changes/` | Mudanças em andamento; as concluídas ficam em `changes/archive/`. |
| `docs/adr/` | Decisões arquiteturais (ADRs). |
| `docs/governanca/desvios.md` | Registro de desvios da constitution. |
| `.github/` | CI (`workflows/ci.yml`), modelo de PR e Dependabot. Detalhes em `docs/ci-cd.md`. |
| `docs/ci-cd.md` | Pipeline de CI/CD: checks obrigatórios e entrega planejada no Vercel. |
| `.claude/` | Comandos e skills do OpenSpec e skills do projeto: `github-versionamento`, `testes-agentes-ia`, `rag-pgvector`, `mcp-client-server`, `semantic-kernel-tools`, `controle-custos-llm`, `memoria-mongodb` (após o ADR de memória) e `vercel-dotnet-pgvector-atlas` (após o ADR de hospedagem). |

Serviços previstos (.NET 10, ASP.NET Core): **Api.Agente**, que orquestra a análise, integra a OpenAI e expõe REST e A2A, e **Api.RAG**, que gerencia a base de conhecimento em PostgreSQL com PgVector e expõe a recuperação por MCP.

## Fluxo de trabalho

constitution → especificação → esclarecimentos → plano técnico → tarefas → implementação → validação

```bash
openspec list
```

No Claude Code, uma mudança começa com `/opsx:new <id>` e avança um artefato por vez com `/opsx:continue`, para que os esclarecimentos venham antes do plano técnico. A implementação usa `/opsx:apply`, a conferência `/opsx:verify` e o encerramento `/opsx:archive`. Cada mudança tem a sua branch `feat/<id>` e entra na `main` por PR. Antes de concluir, valide os artefatos:

```bash
openspec validate --strict
```

A documentação de visão geral fica no cofre Obsidian da INFIS, na nota `Projetos/Agente-ECF`.
