# Agente-ECF

Agente de IA que analisa arquivos da Escrituração Contábil Fiscal (ECF), valida sua estrutura e a consistência das informações e gera um relatório de inconsistências com evidências e referências aos manuais oficiais.

**Situação:** fundação concluída e em produção (https://agente-ecf-agente.vercel.app e https://agente-ecf-rag.vercel.app); as próximas mudanças estão em esclarecimentos (`docs/roteiro.md`). A [constitution](openspec/constitution.md) v1.2.0 está vigente; a v1.1.0 foi ratificada por Kaio Macedo em 2026-10-29.

## Estrutura

| Caminho | Conteúdo |
| --- | --- |
| `src/Api.Agente/`, `src/Api.RAG/` | Serviços ASP.NET Core (.NET 10), um projeto por serviço, com as pastas `Endpoints`, `Aplicacao`, `Dominio` e `Infraestrutura` (ADR 0001). Cada um tem `Dockerfile` e `vercel.json`. |
| `tests/Api.Agente.Tests/`, `tests/Api.RAG.Tests/` | Testes xUnit: HTTP em memória, logs, configuração e regras de arquitetura. |
| `Agente-ECF.slnx`, `global.json`, `Directory.Build.props`, `.editorconfig`, `nuget.config` | Solução, SDK, regras de qualidade, formatação e origem dos pacotes. |
| `openspec/constitution.md` | Princípios, restrições e critérios obrigatórios. Fonte normativa do projeto. |
| `openspec/config.yaml` | Contexto e regras aplicados pelo OpenSpec a cada artefato. |
| `openspec/specs/` | Especificações vigentes, por capacidade. |
| `openspec/changes/` | Mudanças em andamento; as concluídas ficam em `changes/archive/`. |
| `docs/roteiro.md` | Roteiro das mudanças: ordem, dependências, decisões pendentes, skills e prefixos de requisito. |
| `docs/adr/` | Decisões arquiteturais (ADRs). |
| `docs/governanca/desvios.md` | Registro de desvios da constitution. |
| `.github/` | CI (`workflows/ci.yml`), CD (`workflows/cd.yml`), teste de contêineres, modelo de PR e Dependabot. |
| `docs/ci-cd.md` | Pipeline de CI/CD, configuração do Vercel, rotação do token e rollback. |
| `.claude/` | Comandos e skills do OpenSpec e skills do projeto: `github-versionamento`, `testes-agentes-ia`, `rag-pgvector`, `mcp-client-server`, `semantic-kernel-tools`, `controle-custos-llm`, `memoria-mongodb` (após o ADR de memória) e `vercel-dotnet-pgvector-atlas` (após o ADR de hospedagem). |

Serviços (.NET 10, ASP.NET Core): **Api.Agente**, que vai orquestrar a análise, integrar a OpenAI e expor REST e A2A, e **Api.RAG**, que vai gerenciar a base de conhecimento em PostgreSQL com PgVector e expor a recuperação por MCP. Hoje, os dois oferecem `/health/live`, `/health/ready`, correlação por `X-Correlation-Id` e logs em JSON.

## Build e testes

Requer o SDK do .NET 10. A partir da raiz:

```bash
dotnet build Agente-ECF.slnx -c Release
```

```bash
dotnet test --solution Agente-ECF.slnx -c Release
```

```bash
dotnet run --project src/Api.Agente
```

A Api.Agente escuta em `http://localhost:8080` e a Api.RAG em `http://localhost:8081` (perfil local). Em contêiner, a porta vem de `PORT`, com 8080 como padrão. Os demais comandos de verificação estão em `docs/ci-cd.md`.

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
