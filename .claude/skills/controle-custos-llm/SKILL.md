---
name: controle-custos-llm
description: Implementar o registro e a auditoria de consumo de tokens e custos de chamadas à OpenAI (geração e embeddings) no PostgreSQL de cada serviço do Agente-ECF, incluindo retentativas e chamadas auxiliares. Usar durante /opsx:apply de tarefas aprovadas que chamam provedores de IA ou geram relatórios de custo.
---

# Controle de consumo e custos de LLM

Adaptada da skill de mesmo nome do pacote INFIS para Spec Kit. Em conflito, `openspec/constitution.md` prevalece.

## Regras transversais

- Ler `openspec/constitution.md`, `openspec/config.yaml` e os artefatos da mudança em `openspec/changes/<id>/` antes de alterar arquivos. Se não existirem, informar e não inventar requisitos.
- Atuar somente nas tarefas aprovadas e citar os `REQ-<CAPACIDADE>-<NNN>` atendidos.
- .NET 10 e ASP.NET Core; separação Api.Agente/Api.RAG (P15); nenhuma biblioteca sem justificativa (P16).
- Nenhum segredo em código, logs, testes ou Git (SQ-03).
- Executar `dotnet build`, `dotnet test` e `openspec validate --strict`; informar o que não foi executado.
- Decisões relevantes vão para ADR (6.5). Commits e PRs seguem a skill `github-versionamento`.

## Pré-condições

- Fonte, moeda e vigência da tabela de preços definidas no plano técnico (DP-13). Nunca inventar preços atuais.
- Estratégia de consolidação do consumo da Api.RAG definida (DP-26) sem acesso cruzado a dados (P15).

## Fluxo de execução

1. Cada serviço registra no próprio PostgreSQL as chamadas que realiza: a Api.Agente (geração e auxiliares) e a Api.RAG (embeddings de ingestão e de consulta) (P15, P17).
2. Uma linha por tentativa, com os campos de GD-10: execução e correlação, serviço, provedor, modelo, tipo de chamada (geração, embedding, retentativa, auxiliar), datas, duração, status, tokens de entrada, de saída e de embeddings quando informados.
3. Valor desconhecido é nulo com indicador explícito de desconhecido, nunca zero (P17). O esquema impõe essa regra por restrição de banco.
4. Distinguir valor informado pelo provedor e valor estimado pelo sistema; guardar a referência e a versão da tabela de preços usada. Cálculos de custo usam `decimal`.
5. Centralizar a medição num ponto testável (decorator, filtro ou handler do cliente HTTP) que capture também retentativas, chamadas de ferramentas que gerem uso de LLM e embeddings.
6. Falha ao gravar o consumo não é silenciosa: gera log de erro e métrica observável (P17). A estratégia (fila, outbox ou melhor esforço documentado) fica no plano técnico, sem descartar o resultado da chamada.
7. Consultas agregadas restritas por organização e usuário autorizados; nenhum prompt completo, credencial ou dado fiscal nos registros (SQ-04).
8. Criar migrações e testes de concorrência, cálculo com `decimal`, preço ausente, tokens ausentes e múltiplas tentativas (SQ-09).

## Critérios de entrega

- Esquema auditável e relatório por execução, modelo e período.
- Demonstração de uma chamada medida e de um caso com custo desconhecido.
