---
name: semantic-kernel-tools
description: Projetar, implementar e testar Function Calling com Semantic Kernel na Api.Agente do Agente-ECF (.NET 10), com plugins tipados, autorização independente da escolha do modelo, timeouts, auditoria e proteção contra ações indevidas. Usar durante /opsx:apply de tarefas aprovadas que criam ou alteram ferramentas do agente.
---

# Ferramentas Semantic Kernel na Api.Agente

Adaptada da skill de mesmo nome do pacote INFIS para Spec Kit. Em conflito, `openspec/constitution.md` prevalece.

**Mudanças do roteiro** (`docs/roteiro.md`): 10 `analise-llm` e 11 `ferramentas-agente`. O `tasks.md` indica os grupos de tarefas que usam esta skill.

## Regras transversais

- Ler `openspec/constitution.md`, `openspec/config.yaml` e os artefatos da mudança em `openspec/changes/<id>/` antes de alterar arquivos. Se não existirem, informar e não inventar requisitos.
- Atuar somente nas tarefas aprovadas e citar os `REQ-<CAPACIDADE>-<NNN>` atendidos.
- .NET 10 e ASP.NET Core; separação Api.Agente/Api.RAG (P15); nenhuma biblioteca sem justificativa (P16).
- Nenhum segredo em código, logs, testes ou Git (SQ-03).
- Não inventar regras fiscais, schemas ou documentos oficiais (P01).
- Executar `dotnet build`, `dotnet test` e `openspec validate --strict`; informar o que não foi executado.
- Decisões relevantes vão para ADR (6.5). Commits e PRs seguem a skill `github-versionamento`.

## Limites do Agente-ECF

- Nesta fase o agente apenas valida o arquivo (constitution, 1.3). Ferramenta que altere, transmita ou corrija dados está fora do escopo.
- Ferramentas externas só existem quando especificadas (DP-10). Ferramentas com efeito externo (escrita, envio a terceiros, custo) precisam de autorização explícita definida na especificação (P14).
- Validadores determinísticos (P02) podem ser expostos como ferramentas, mas seu resultado nunca é recalculado ou alterado pelo modelo.

## Fluxo de execução

1. Na especificação, separar consultas, validações determinísticas e operações com efeito colateral.
2. Definir um plugin por responsabilidade e uma função tipada por operação, com o catálogo de P14: nome, responsabilidade, contratos, validação de argumentos, autenticação e autorização, timeout, falhas, retentativa e idempotência, testes e registro.
3. Conferir as APIs do Semantic Kernel instaladas antes de escrever código; não copiar exemplos de versões incompatíveis. Tipos do Semantic Kernel ficam na infraestrutura, nunca no domínio (RA-06, P16).
4. Validar no servidor todos os argumentos gerados pelo modelo, o tamanho do payload e o contexto de usuário e organização (P10, P14).
5. Verificar a autorização a cada chamada, independentemente da escolha do modelo (P14, SQ-02).
6. Aplicar cancelamento, timeout, retentativas limitadas e limite de chamadas por execução; prevenir loops e repetição de efeitos (SQ-06, DP-14).
7. Registrar chamada, argumentos, resultado, duração, status e correlação com dados sensíveis mascarados (P19, SQ-04). O resultado registrado permite verificar o que o modelo citar (P07).
8. Devolver resultados estruturados; uma falha vira falha técnica (P04), nunca sucesso fabricado.
9. Testar chamada autorizada, negada, argumento inválido, duplicação, timeout, falha simulada e injeção no resultado da ferramenta (SQ-09).

## Critérios de entrega

- Plugins com contratos claros, testes automatizados e política de autorização explícita.
- Demonstração do ciclo LLM → ferramenta → resultado → resposta, sem omitir erros.
