# Agente-ECF — instruções para agentes

- Leia `openspec/constitution.md` antes de especificar, planejar, implementar ou revisar. Ela prevalece sobre qualquer outro artefato do projeto.
- O trabalho segue o fluxo SDD com OpenSpec (`/opsx:new`, `/opsx:continue`, `/opsx:apply`, `/opsx:verify`, `/opsx:archive`). Não implemente funcionalidade sem mudança aprovada (P20).
- Não invente leiautes, registros, regras fiscais, schemas ou critérios de conformidade (P01). Sem fonte oficial, registre o esclarecimento ou a decisão pendente.
- Decisões pendentes estão na seção 9 da constitution e não são requisitos aprovados; as resolvidas ficam em 9.1.
- Use `/opsx:new` e `/opsx:continue` (um artefato por vez) para que os esclarecimentos venham antes do `design.md`.
- Commits, branches e PRs seguem a skill `github-versionamento` (seção 6.8 da constitution).
- Na implementação, use as skills do projeto conforme o tema: `testes-agentes-ia`, `rag-pgvector`, `mcp-client-server`, `semantic-kernel-tools`, `controle-custos-llm`, `memoria-mongodb` e `vercel-dotnet-pgvector-atlas`. Cada uma declara suas pré-condições (ADRs e decisões pendentes).
- O merge de PRs está delegado ao assistente (6.6), sempre com os checks `OpenSpec` e `.NET` verdes.
- Em revisões, cite o identificador violado (Pxx, RA-xx, GD-xx, SQ-xx, DoD-xx).
- Escreva artefatos em português; mantenha os cabeçalhos do OpenSpec e as palavras-chave normativas em inglês (MUST/SHALL, MUST NOT/SHALL NOT, MAY), como na constitution.
