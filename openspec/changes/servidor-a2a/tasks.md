# Tasks: Servidor A2A para a Sofia INFIS

Marcar cada tarefa somente com a verificação indicada executada. Plano em `design.md`.

## 1. Contrato SOFIA

Skill: sofia-a2a-contrato

- [x] 1.1 Criar os DTOs do contrato v1.0 e o validador em `Aplicacao/ContratoSofia` (D2); verificar com testes o round-trip da solicitação de exemplo, a ausência de cada campo obrigatório, documento vazio, `idDocumento` repetido e versão não suportada (REQ-SOFIA-001, REQ-SOFIA-002).
- [x] 1.2 Criar `ProcessadorDeSolicitacoes` (D3, D4); verificar com testes o status `parcial`, a correlação dos IDs, tamanho e hash de dois documentos, ausência de achados e o limite sem truncar (REQ-A2A-002, REQ-SOFIA-003, REQ-SOFIA-004).

## 2. Servidor A2A e cartão

Skill: sofia-a2a-implementacao

- [x] 2.1 Adicionar `A2A.AspNetCore` 1.0.0-preview4 e criar `AgenteA2A`, opções e endpoints (D1, D3); verificar com o cliente do SDK o fluxo ponta a ponta: descoberta pelo cartão, `SendMessage`, Task concluída e Artifact com JSON válido (REQ-A2A-005); verificar localmente a interoperabilidade com `Microsoft.Agents.AI.A2A` 1.27.0-preview, o pacote da Sofia.
- [x] 2.2 Publicar o cartão com `A2A:UrlPublica` (D7) e a cópia em `docs/contratos/agent-card.json`; verificar com testes os campos obrigatórios, a skill única e a igualdade com a cópia (REQ-A2A-001).
- [x] 2.3 Criar `ArmazemDeTarefas` (D5, D6); verificar com testes `GetTask`, `CancelTask` em tarefa terminada, tarefa inexistente, método desconhecido, JSON malformado, versão de protocolo, `SendStreamingMessage`, `SubscribeToTask`, notificações e `ListTasks` (REQ-A2A-006).
- [x] 2.4 Tratar mensagem sem texto, JSON inválido e IDs inválidos como Task com falha; verificar com testes (REQ-A2A-005).

## 3. Segurança e observabilidade

Skill: testes-agentes-ia

- [x] 3.1 Registrar logs só com identificadores e status (D5); verificar com teste que nome, pergunta e texto não aparecem nos logs, na resposta nem em `GetTask`, e que o `X-Correlation-Id` chega aos logs (REQ-SOFIA-005, REQ-A2A-005).
- [x] 3.2 Verificar com teste adversarial que um documento com instrução embutida não muda o resultado nem declara conformidade (REQ-A2A-004).
- [x] 3.3 Atualizar os testes de arquitetura para impedir `Aplicacao` e `Dominio` de dependerem do pacote `A2A`; verificar que passam (D2).

## 4. CORS

Skill: sofia-cors-rest-a2a

- [x] 4.1 Criar opções validadas e a política `SofiaBrowser` com escopo em `/a2a` e no cartão (D8); verificar com testes o preflight de origem autorizada e não autorizada, a chamada sem `Origin`, `Vary: Origin`, a saúde fora do escopo e a origem inválida na configuração (REQ-CORS-001, REQ-CORS-002, REQ-CORS-003).

## 5. Contêiner e rede

Skill: vercel-dotnet-pgvector-atlas

- [x] 5.1 Criar `compose.yaml` (D9); verificar com `docker compose` que o cartão responde em `localhost:8085` e, de outro contêiner na rede `sofia-infis_sofia_network`, em `http://agente-ecf:8080`.
- [ ] 5.2 Incluir o cartão e `SendMessage` em `.github/scripts/testar-conteineres.sh`; verificar na execução local das mesmas chamadas e no CI.

## 6. Documentação

Skill: sofia-a2a-contrato

- [x] 6.1 Escrever `docs/contratos/sofia-a2a.md`: campos de A2A e do contrato, mapeamento, erros, limites, execução e testes; verificar que os exemplos correspondem às fixtures dos testes.
- [x] 6.2 Atualizar o README e `docs/ci-cd.md`; verificar os comandos citados.

## 7. Produção e fechamento

Skill: vercel-dotnet-pgvector-atlas

- [ ] 7.1 Depois do merge, verificar que o CD publicou e que a saúde continua 200.
- [ ] 7.2 Verificar em produção o cartão com a `url` pública e um `SendMessage` com dados sintéticos (REQ-A2A-001, REQ-A2A-005).
- [ ] 7.3 Medir em produção o maior corpo aceito pelo Vercel em `SendMessage`, só com dados sintéticos, e registrar o resultado em `docs/contratos/sofia-a2a.md` e em E07.
- [ ] 7.4 Com Kaio Macedo, fazer o teste com a Sofia real, quando o cliente A2A dela existir.
- [ ] 7.5 Atualizar o cofre Obsidian e `docs/roteiro.md`; executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution).

## Workflow follow-up

- O arquivamento espera o teste com a Sofia real (7.4). Ao arquivar, mover DP-16 para as decisões resolvidas.
