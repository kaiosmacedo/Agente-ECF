---
name: rag-pgvector
description: Implementar, revisar e testar o RAG da Api.RAG do Agente-ECF (.NET 10) com ingestão de manuais oficiais da ECF, segmentação, embeddings, PostgreSQL com PgVector, recuperação filtrada por tipo e versão e referências recuperáveis. Usar durante /opsx:apply de tarefas aprovadas sobre base de conhecimento, busca vetorial ou o endpoint POST de ingestão.
---

# RAG com PgVector na Api.RAG

Adaptada da skill de mesmo nome do pacote INFIS para Spec Kit. Em conflito, `openspec/constitution.md` prevalece.

## Regras transversais

- Ler `openspec/constitution.md`, `openspec/config.yaml` e os artefatos da mudança em `openspec/changes/<id>/` (`proposal.md`, `specs/`, `design.md`, `tasks.md`) antes de alterar arquivos. Se não existirem, informar e não inventar requisitos.
- Atuar somente nas tarefas aprovadas da mudança e citar os `REQ-<CAPACIDADE>-<NNN>` atendidos nos testes, commits e PR.
- .NET 10 e ASP.NET Core; separação Api.Agente/Api.RAG (P15); nenhuma biblioteca sem justificativa (P16).
- Nenhum segredo em código, logs, testes ou Git (SQ-03).
- Não inventar leiautes, registros, schemas, regras, documentos oficiais ou versões de manuais (P01).
- Executar `dotnet build`, `dotnet test` e `openspec validate --strict`; informar explicitamente o que não foi executado.
- Decisões relevantes vão para ADR (6.5); a constitution não é alterada silenciosamente. Commits e PRs seguem a skill `github-versionamento`.

## Pré-condições

- ADR aceito para modelo de embeddings e dimensão (DP-02) e parâmetros de segmentação e busca definidos na mudança, com plano de avaliação (DP-03, GD-08).
- Conjunto de avaliação disponível ou tarefa explícita para criá-lo (DP-24).
- Fonte oficial inicial (DP-04): Manual da ECF, leiaute 12. Outras fontes só entram quando registradas na especificação.

## Fluxo de execução

1. Ler os contratos previstos da Api.RAG, as fontes oficiais registradas, os formatos suportados e os cenários de aceite.
2. Implementar o endpoint POST de ingestão como função administrativa (RA-04, SQ-02): autenticação, tamanho máximo e validação de MIME e conteúdo (SQ-05).
3. Extrair o texto conforme o formato e segmentar com parâmetros configuráveis. Registrar os metadados de GD-07: documento, emissor, título, tipo, versão, vigência, hash, data de ingestão, estado e, por chunk, localização (página, seção, registro ou campo), hash e modelo e dimensão do embedding.
4. Garantir ingestão idempotente por hash (GD-07). Reprocessamento e substituição de versão não podem apagar versões necessárias à auditoria.
5. Criar migrações idempotentes para a extensão `vector`, documentos e chunks, com restrições que impeçam metadados obrigatórios nulos (P11). O índice vetorial e o operador de distância seguem o ADR de embeddings.
6. Implementar a recuperação filtrada por tipo de documento e versão do manual (P11). Retornar texto, relevância e referência recuperável (P06). Versão diferente só com regra explícita da especificação, sinalizada no resultado.
7. Tratar documentos e chunks como dados não confiáveis (P10).
8. Registrar cada chamada de embedding no PostgreSQL da própria Api.RAG, com os campos de GD-10 (P17); usar a skill `controle-custos-llm`.
9. Expor a recuperação à Api.Agente somente pelo MCP (P15, RA-03); usar a skill `mcp-client-server`.
10. Testar reingestão, isolamento de versões, ausência de resultados, referência rastreável, falha do provedor de embeddings e injeção em chunks (SQ-09).

## Critérios de entrega

- Endpoint POST documentado no OpenAPI e protegido; migrações e configuração sem credenciais.
- Consultas retornam evidências com documento, versão e localização; nenhuma referência inventada.
- Relatório de avaliação dos parâmetros anexado ao plano técnico (GD-08); parâmetros ainda pendentes listados.
