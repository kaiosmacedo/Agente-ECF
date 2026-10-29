---
name: memoria-mongodb
description: Implementar a memória conversacional da Api.Agente do Agente-ECF em MongoDB (.NET 10), com isolamento por usuário, conversa e organização, retenção, recuperação limitada e proteção de dados. Usar durante /opsx:apply de tarefas aprovadas que persistem ou recuperam histórico de conversas, somente depois do ADR que escolhe MongoDB (DP-01).
---

# Memória conversacional em MongoDB

Adaptada da skill de mesmo nome do pacote INFIS para Spec Kit. Em conflito, `openspec/constitution.md` prevalece.

**Mudanças do roteiro** (`docs/roteiro.md`): 12 `memoria-conversacional`, depois do ADR de DP-01. O `tasks.md` indica os grupos de tarefas que usam esta skill.

## Regras transversais

- Ler `openspec/constitution.md`, `openspec/config.yaml` e os artefatos da mudança em `openspec/changes/<id>/` antes de alterar arquivos. Se não existirem, informar e não inventar requisitos.
- Atuar somente nas tarefas aprovadas e citar os `REQ-<CAPACIDADE>-<NNN>` atendidos.
- .NET 10 e ASP.NET Core; separação Api.Agente/Api.RAG (P15); nenhuma biblioteca sem justificativa (P16).
- Nenhum segredo em código, logs, testes ou Git (SQ-03).
- Executar `dotnet build`, `dotnet test` e `openspec validate --strict`; informar o que não foi executado.
- Decisões relevantes vão para ADR (6.5). Commits e PRs seguem a skill `github-versionamento`.

## Pré-condições

- ADR aceito para DP-01 escolhendo MongoDB (por exemplo, MongoDB Atlas) em vez de Redis. Sem ele, parar e informar.
- Política de retenção, expiração e exclusão aprovada (DP-11) antes de qualquer uso com dados reais.
- Mecanismo de identidade definido (DP-09): o isolamento depende da identidade autenticada.

## Fluxo de execução

1. Modelar mensagens com identificador da conversa, do usuário, da organização (quando aplicável), papel, data, identificador de correlação e classificação de sensibilidade. O texto da mensagem nunca é chave de isolamento.
2. Criar índices compostos por organização, usuário e conversa, com ordenação por data. TTL somente se a política aprovada pedir.
3. Limitar o histórico enviado ao modelo por parâmetro configurável (P18), mantendo a ordem cronológica.
4. Não guardar segredos, arquivos ECF completos nem saídas sensíveis de ferramentas; aplicar minimização, mascaramento ou criptografia conforme o ADR (P18, SQ-04).
5. Filtrar toda consulta pela identidade autenticada; nunca aceitar identificador de usuário vindo do cliente sem vínculo com a autenticação (SQ-02).
6. Implementar expiração e exclusão conforme a política e documentar o efeito em backups (GD-11).
7. Manter a memória separada do conhecimento oficial: ela nunca serve de fonte, evidência ou referência (P18, GD-09).
8. Em falha do MongoDB, não inventar histórico; seguir o comportamento degradado definido na especificação e registrar a limitação (P08).
9. Testar isolamento entre usuários e organizações, concorrência, ordenação, retenção, exclusão e indisponibilidade (SQ-09).

## Critérios de entrega

- Configuração por variáveis de ambiente, com `.env.example` sem valores reais.
- Modelos, índices, repositório, testes e instruções de execução documentados.
