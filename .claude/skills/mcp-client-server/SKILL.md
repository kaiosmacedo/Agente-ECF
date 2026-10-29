---
name: mcp-client-server
description: Implementar e validar a comunicação MCP entre a Api.Agente (cliente) e a Api.RAG (servidor) do Agente-ECF em .NET 10, incluindo ferramentas, transporte, contratos versionados, autenticação, falhas e testes. Usar durante /opsx:apply de tarefas aprovadas que integram os serviços por Model Context Protocol.
---

# MCP: cliente na Api.Agente, servidor na Api.RAG

Adaptada da skill de mesmo nome do pacote INFIS para Spec Kit. Em conflito, `openspec/constitution.md` prevalece.

## Regras transversais

- Ler `openspec/constitution.md`, `openspec/config.yaml` e os artefatos da mudança em `openspec/changes/<id>/` antes de alterar arquivos. Se não existirem, informar e não inventar requisitos.
- Atuar somente nas tarefas aprovadas e citar os `REQ-<CAPACIDADE>-<NNN>` atendidos.
- .NET 10 e ASP.NET Core; separação Api.Agente/Api.RAG (P15); nenhuma biblioteca sem justificativa (P16).
- Nenhum segredo em código, logs, testes ou Git (SQ-03).
- Não inventar regras, schemas ou documentos oficiais (P01).
- Executar `dotnet build`, `dotnet test` e `openspec validate --strict`; informar o que não foi executado.
- Decisões relevantes vão para ADR (6.5). Commits e PRs seguem a skill `github-versionamento`.

## Pré-condições

- ADR aceito de autenticação entre serviços (DP-09). Sem ele, o servidor MCP não é exposto fora do ambiente local de testes.
- ADR de hospedagem (DP-18) avaliando o transporte MCP no Vercel. Se a sessão ou o streaming não forem suportados, o ADR define host persistente para a Api.RAG.

## Fluxo de execução

1. Conferir a versão vigente do SDK MCP para .NET e do transporte escolhido antes de escrever código; registrar no plano as versões instaladas.
2. Definir cada ferramenta MCP com nome, descrição e JSON Schema versionados (GD-01). A ferramenta de recuperação recebe consulta, tipo de documento, versão do manual e filtros (P11) e devolve trechos, relevância, referência (documento, versão, localização) e erros tipados (P06).
3. Expor o servidor MCP na Api.RAG e o cliente na Api.Agente. Manter a API REST administrativa separada das ferramentas MCP (RA-04). A Api.Agente não usa o REST administrativo para recuperar conhecimento (P15).
4. Aplicar autenticação do transporte, autorização por chamada, limite de payload, timeout, cancelamento, propagação do identificador de correlação e logs sem dados sensíveis (SQ-01, SQ-02, SQ-04, SQ-06, SQ-07).
5. Tratar resultados MCP e conteúdo dos documentos como dados não confiáveis; validar argumentos e formato de retorno (P10).
6. Registrar na execução os identificadores dos chunks recebidos, para que a validação semântica confira as referências citadas pelo LLM (P07, P19).
7. Nunca acessar o banco vetorial a partir da Api.Agente, nem trocar o MCP por REST funcional (P15).
8. Em indisponibilidade do servidor, registrar falha técnica rastreável (P04) e a limitação no resultado (P08); nunca afirmar que o conhecimento foi consultado.
9. Testar ponta a ponta com os dois serviços em execução, além de contrato, autenticação, timeout e falha de conectividade (SQ-09).

## Critérios de entrega

- Instruções reproduzíveis para subir os dois serviços e invocar a ferramenta MCP.
- Ferramentas, contratos, autenticação e códigos de falha documentados.
- Testes executados e limitações reais da integração relatados.
