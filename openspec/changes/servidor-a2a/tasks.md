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
- [x] 5.2 Incluir o cartão e `SendMessage` em `.github/scripts/testar-conteineres.sh`; verificar na execução local das mesmas chamadas e no CI.

## 6. Documentação

Skill: sofia-a2a-contrato

- [x] 6.1 Escrever `docs/contratos/sofia-a2a.md`: campos de A2A e do contrato, mapeamento, erros, limites, execução e testes; verificar que os exemplos correspondem às fixtures dos testes.
- [x] 6.2 Atualizar o README e `docs/ci-cd.md`; verificar os comandos citados.

## 7. Produção e fechamento

Skill: vercel-dotnet-pgvector-atlas

- [x] 7.1 Depois do merge, verificar que o CD publicou e que a saúde continua 200.
- [x] 7.2 Verificar em produção o cartão com a `url` pública e um `SendMessage` com dados sintéticos (REQ-A2A-001, REQ-A2A-005).
- [x] 7.3 Medir em produção o maior corpo aceito pelo Vercel em `SendMessage`, só com dados sintéticos, e registrar o resultado em `docs/contratos/sofia-a2a.md` e em E07.
- [x] 7.4 Com Kaio Macedo, fazer o teste com a Sofia real, quando o cliente A2A dela existir. Em 2026-11-12, a pedido de Kaio Macedo (verificar e fechar as mudanças abertas depois da apresentação), o assistente conduziu o teste na conversa pública da Sofia central: com a ECF sintética da demonstração anexada (`ECF-DEMO-com-erro.txt`, 607 bytes) e o pedido de análise, por volta de 15h20, a orquestração fez a seleção por especialidade, chamou só o Agente ECF, que voltou `parcial`, e o parecer da Sofia repetiu o achado (IND_SIT_INI_PER do Registro 0000 inválido na linha 1). Antes, às 11h50 e 11h55, perguntas sem arquivo também chamaram o agente ("Concluído", 5,6 s e 3,9 s). Registro em `docs/contratos/sofia-a2a.md`.
- [ ] 7.5 Atualizar o cofre Obsidian e `docs/roteiro.md`; executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution). Verificada em 2026-11-12: os 17 requisitos e os 28 cenários têm implementação e teste, e o roteiro e o cofre foram atualizados. Antes de arquivar, faltam dois aceites de Kaio Macedo: (1) a PATCH 2.2.1 da constitution, que move DP-16 para as decisões resolvidas (8.3); e (2) o ajuste dos deltas pelo `/opsx:update` (P20): REQ-SOFIA-004 cita `A2A:TamanhoMaximoDosDocumentosEmBytes`, que a mudança 07 trocou por `Recepcao:TamanhoMaximoDosDocumentosEmBytes`; REQ-A2A-002 manda sempre `parcial`, mas a pergunta respondida sai `concluido` (mudança 17); REQ-A2A-008 fixa o cartão 1.3.0, mas com as perguntas ligadas ele é o 1.4.0; e REQ-SOFIA-003 pede o tamanho e o SHA-256 de cada documento nas observações, o que a resposta à pergunta não traz.

## Workflow follow-up

- O arquivamento espera o teste com a Sofia real (7.4). Ao arquivar, mover DP-16 para as decisões resolvidas.

## 8. Perfil 0.3.0 e chave da Sofia central (2026-11-10)

Skill: sofia-a2a-implementacao

- [x] 8.1 Trocar o mapeamento pelo pacote `A2A.V0_3Compat` e conferir `message/send`, `tasks/get` e `tasks/cancel` como a Sofia central chama (D10; REQ-A2A-009).
- [x] 8.2 Cartão 1.3.0 com o esquema Bearer, no formato 0.3.0 sem `A2A-Version` e v1.0 com ele, e as duas cópias versionadas (D11; REQ-A2A-001, 008, 009).
- [x] 8.3 Middleware da chave Bearer: 401 sem chave válida, 503 sem chave configurada, comparação em tempo constante e nada em log (D12; REQ-A2A-003, 010).
- [x] 8.4 Testes de `AutenticacaoSofiaTests` e ajuste dos testes existentes à chave sintética; 539 testes aprovados (D14).
- [x] 8.5 Emenda v2.1.0 (RA-07 e SQ-12), guia da Sofia com o objeto de cadastro, variáveis em `docs/ci-cd.md` e roteiro.
- [x] 8.6 Kaio Macedo cadastra a variável sensível `SOFIA_A2A_TOKEN` em `agente-ecf-agente` (SQ-03); depois do deploy, conferir em produção o 401 sem chave e, com o script `verificar-sofia-a2a.sh` rodado por ele, o cartão e o `message/send` com a chave. Feito em 2026-11-10: Kaio Macedo cadastrou a variável, e o assistente refez o deploy e, a pedido dele ("Rode você"), rodou o script com a chave lida da skill, sem exibi-la. Os 12 itens passaram: 401 sem chave, cartão 0.3.0 com nome, Bearer e URL https, e `message/send` com Task `completed`, um Artifact `analise-sofia-v1` e os mesmos IDs; resultado `parcial` para a ECF sintética válida.
- [x] 8.7 Entregar ao professor o objeto de cadastro e, quando ele incluir o agente em `A2A:Agents`, fazer o teste ponta a ponta pela Sofia central com documento sintético (E12; tarefa 7.4). O professor incluiu o agente: ele aparece no catálogo "Inteligência em conjunto" da Sofia central e é chamado por ela. Ponta a ponta com documento sintético: ver 7.4.

