# Design: Servidor A2A para a Sofia INFIS

> **Estado:** implementado a pedido de Kaio Macedo em 2026-11-05, com protocolo A2A v1.0, rede `sofia_network` e sem token, por decisão dele. A emenda v2.0.0 (PR #8) foi aprovada e mesclada.
>
> **Atualizado em 2026-11-12:** desde 2026-11-10 valem D10 a D14: A2A 0.3.0 da Sofia central no mesmo `/a2a` e chave Bearer compartilhada `SOFIA_A2A_TOKEN`. Onde este documento ainda fala em "sem token", em autenticação fora do escopo ou em `message/send` com -32601 (que hoje só vale com `A2A-Version: 1.0`), prevalecem D10 a D12. O professor incluiu o agente no catálogo da Sofia central, e o teste ponta a ponta com ECF sintética passou em 2026-11-12 (D13).

## Context

A fundação tem só as rotas de saúde. As skills da turma definem como cada agente conversa com a Sofia: A2A com JSON-RPC, contrato SOFIA v1.0 em uma parte de texto e CORS só para navegador. Elas fixavam v0.3.x "até decisão explícita de migração", e Kaio Macedo decidiu por v1.0, a versão do SDK que o repositório `SofiaInfis` referencia. Esse repositório ainda não tem código de cliente A2A. A análise da ECF só chega nas mudanças 03 a 10, então esta mudança entrega um esqueleto honesto: protocolo e contrato reais, com status `parcial`.

## Goals / Non-Goals

**Goals**

- Cartão e `message/send` conformes ao perfil da turma, testados com um cliente A2A real.
- Contrato SOFIA v1.0 validado e correlacionado, com erros controlados.
- Nenhum documento nem nome de usuário em logs ou na memória.
- Base para as próximas mudanças preencherem a análise sem alterar o adaptador.

**Non-Goals**

- Validar a ECF, persistir execuções e chamar LLM.
- Cliente A2A e agregação, que são da Sofia.
- Autenticação (SQ-12).

## Decisions

### D1. SDK oficial na linha 1.0

- `A2A.AspNetCore` 1.0.0-preview4, com alvo `net10.0`, e o cliente do mesmo pacote nos testes. A skill manda usar o SDK disponível. O SDK já valida o envelope, responde JSON malformado com `-32700` e recusa streaming e notificações conforme o cartão.
- **Alternativas:** a linha 0.3 (`A2A.AspNetCore` 0.3.4-preview), usada na primeira implementação, virou legada e exigia filtro e invólucro próprios. `A2A.V0_3Compat` atenderia clientes v0.3 ao mesmo tempo, mas é dependência sem uso depois da decisão pela v1.0.
- **Custo:** pacote em preview. Versão fixada. A interoperabilidade com o pacote da Sofia foi verificada localmente com `Microsoft.Agents.AI.A2A` 1.27.0-preview (SDK `A2A` 1.0.0-preview3).

### D2. Camadas

| Camada | Conteúdo |
| --- | --- |
| `Aplicacao/ContratoSofia` | DTOs do contrato, validador e `ProcessadorDeSolicitacoes`, sem depender de A2A nem de ASP.NET Core. |
| `Infraestrutura/Integracao` | `AgenteA2A` (`IAgentHandler`: mensagem → processador → Artifact), cartão, `ArmazemDeTarefas` e opções. |
| `Infraestrutura/Cors` | Opções validadas e política `SofiaBrowser`. |
| `Endpoints` | Mapeamento de `/a2a` e `/.well-known/agent-card.json`. |

Um teste de arquitetura impede `Aplicacao` e `Dominio` de dependerem do pacote `A2A`.

### D3. Mapeamento A2A e contrato

```text
POST /a2a (JSON-RPC SendMessage)
  params.message.parts[].text = SolicitacaoAnaliseSofia (JSON)
    -> ProcessadorDeSolicitacoes
  result.task = { status.state, artifacts[name=analise-sofia-v1].parts[].text = ResultadoAnaliseSofia (JSON) }
```

| Situação | Task | `status` do contrato | `erro.codigo` |
| --- | --- | --- | --- |
| Solicitação válida | `TASK_STATE_COMPLETED` | `parcial` | — |
| Campo inválido, com IDs válidos | `TASK_STATE_COMPLETED` | `erro` | `solicitacao_invalida` |
| Versão de contrato diferente de 1.0 | `TASK_STATE_COMPLETED` | `erro` | `versao_contrato_nao_suportada` |
| Textos acima do limite | `TASK_STATE_COMPLETED` | `erro` | `documentos_acima_do_limite` |
| Mensagem sem texto, JSON inválido ou IDs inválidos | `TASK_STATE_FAILED` | sem resultado | motivo em `status.message` |

Sem IDs válidos, não há como a Sofia correlacionar um resultado; o contrato prevê Task com falha nesse caso. `concluido` fica reservado para quando todas as validações obrigatórias existirem (P05).

### D4. Resposta do esqueleto

`analise` explica que as validações não são executadas nesta versão. `observacoes` traz, por documento, `idDocumento`, número de caracteres e SHA-256 do texto em UTF-8, e a lista de validações não realizadas (P08, GD-06). `achados` fica vazio (P07). A resposta é determinística para a mesma solicitação, então uma retentativa com o mesmo `idSolicitacao` não tem efeito colateral; a deduplicação persistida entra na mudança 03.

### D5. Privacidade e memória (SQ-04)

- `AgenteA2A` emite uma única tarefa já terminada, sem histórico, e `ArmazemDeTarefas` descarta qualquer histórico ao salvar. O documento não fica em memória nem volta na resposta. Como o primeiro evento já é final, a resposta é final mesmo com `returnImmediately`.
- `ArmazemDeTarefas` limita a 1000 tarefas, por 1 hora, em `MemoryCache`, só com status e resultado. No Vercel, as instâncias não duram e `GetTask` não é garantido; a resposta de `SendMessage` já é final.
- Sem identidade verificada, `ListTasks` responde sempre com lista vazia: nenhum chamador vê as tarefas dos outros.
- Logs: `idExecucao`, `idSolicitacao`, status e número de documentos. Nunca `usuario.nome`, `pergunta` ou `texto`.

### D6. Métodos fora do perfil

O SDK v1.0 lê as capacidades do cartão: `SendStreamingMessage` e `SubscribeToTask` em tarefa terminada recebem `-32004`, e a configuração de notificações, `-32003`. Métodos da v0.3, como `message/send`, recebem `-32601`, e uma versão desconhecida em `A2A-Version`, `-32009`.

### D7. Cartão

`supportedInterfaces` com `JSONRPC` e versão `1.0`, skill `analisar_documentos` e modos `text/plain` e `application/json`, porque o SDK recusa uma parte cujo tipo não esteja no cartão. A cópia versionada fica em `docs/contratos/agent-card.json` (GD-01), e um teste compara. `url` vem de `A2A:UrlPublica`, com padrão `http://localhost:8080/a2a`; o valor de produção fica em `appsettings.Production.json`, por não ser segredo.

### D8. CORS (SQ-13)

Política `SofiaBrowser`, com `Cors:AllowedOrigins` opcional e validada: o agente não tem navegador hoje, e a skill manda não exigir a configuração nesse caso. O escopo é só `/a2a` e o cartão, com `UseWhen` e `UseCors(política)`: `RequireCors` por endpoint não responde a preflight automaticamente. Métodos `GET`, `POST` e `OPTIONS`; cabeçalhos `Content-Type`, `Authorization`, `A2A-Version` e `X-Correlation-Id`; sem credenciais. Se a mudança 02 for retomada, CORS fica antes da autenticação.

### D9. Rede

`compose.yaml` na raiz: serviço `agente-ecf`, porta `8085:8080`, rede externa `sofia_network` (nome real `sofia-infis_sofia_network`, criada pelo compose da Sofia) e `A2A__UrlPublica` com o nome do serviço. Como a API da Sofia hoje roda fora do compose, o endereço pode ser trocado por `A2A_URL_PUBLICA`.

## Perfil 0.3.0 e chave da Sofia central (2026-11-10)

Kaio Macedo pediu para seguir a skill `sofia-a2a-autenticacao`, do professor: "Agora preciso que você faça o que está na skill anexada para o professor conseguir alcançar nosso agente corretamente através da sofia". A Sofia central (`https://sofia-infis.onrender.com/`) fala A2A 0.3.0 e autentica com uma chave Bearer compartilhada. O código dela foi lido em 2026-11-10 (`AgentDiscoveryService`, `JsonRpc03AgentClient`, `A2A03JsonCodec`), e os testes repetem as checagens dele.

### D10. Pacote de compatibilidade oficial (REQ-A2A-009)

`A2A.V0_3Compat` 1.0.0-preview4, do mesmo SDK oficial e com licença Apache-2.0, troca `MapA2A` por `MapA2AWithV03Compat` e o cartão por `MapAgentCardGetWithV03Compat`. O mesmo endpoint `/a2a` atende `message/send`, `tasks/get` e `tasks/cancel` (0.3.0) e `SendMessage`, `GetTask` e `CancelTask` (v1.0). O `AgenteA2A` não muda.

- **Por que:** mantém a v1.0 de RA-07 e acrescenta o 0.3.0 sem reescrever o servidor. A D1 tinha descartado o pacote por falta de cliente 0.3.0; a Sofia central é esse cliente.
- **Alternativas descartadas:** voltar ao SDK 0.3.4-preview, que tira a v1.0; escrever o JSON-RPC 0.3.0 à mão, que duplica o que o SDK já faz.
- **Verificado:** `message/send` sem `configuration`, como a Sofia central envia, devolve a Task já `completed`, porque o `AgenteA2A` emite a tarefa terminada no primeiro evento. O aviso do SDK sobre `blocking` vale para respostas iniciais, não para este caso.
- **Efeito colateral:** o endpoint de compatibilidade não emite a fonte de trace `A2A.AspNetCore`; o trace da requisição e o da fonte `A2A` continuam (REQ-OBSERV-004).

### D11. Cartão (REQ-A2A-001, 008, 009)

Sem `A2A-Version: 1.0`, o cartão sai no formato 0.3.0 com os campos da v1.0 juntos, como o SDK faz por padrão; a Sofia central lê só os campos dela. Com o cabeçalho, sai a v1.0. O cartão passa a 1.3.0 e declara o esquema `sofiaService`, `http` `bearer`, sem o valor da chave. O pacote serve o cartão também em `/`. As cópias ficam em `docs/contratos/agent-card.json` (v1.0) e `docs/contratos/agent-card-v0.3.json` (0.3.0).

### D12. Chave Bearer (REQ-A2A-003, 010; SQ-03)

O middleware `AutenticacaoDaSofia` vale só para `/`, `/a2a` e o cartão, depois do CORS, para o preflight passar:

- lê `SOFIA_A2A_TOKEN` da configuração; sem valor, responde 503, e o serviço continua no ar para o REST e a saúde, em vez de não subir;
- aceita só um cabeçalho `Bearer` com a chave exata, sem normalizar a caixa; compara os SHA-256 das duas chaves com `CryptographicOperations.FixedTimeEquals`, que não revela nem o tamanho;
- responde 401 com `WWW-Authenticate: Bearer` antes de qualquer processamento; 403 não se aplica, porque a chave dá acesso a tudo o que o A2A oferece;
- loga só o caminho recusado, nunca o cabeçalho.

O documento OpenAPI declara o esquema nas rotas do A2A, e o Swagger ganha o botão de autorizar.

### D13. Cadastro na Sofia central (E12)

A chave não registra o agente. O professor inclui o objeto de `docs/contratos/sofia-a2a.md` em `A2A:Agents`, com `IdAgente` `agente-ecf`, `ExpectedName` `Agente ECF`, `CardUri` e `AllowedDestinations` de produção e os IPs públicos do Vercel. O DNS deu dois endereços em 2026-11-10, que podem mudar; a política de destino da Sofia é com o professor.

### D14. Testes (SQ-09)

`AutenticacaoSofiaTests`: 401 sem chave, com chave errada, com caixa trocada e com outro esquema; 503 sem chave configurada; cartão nas checagens de descoberta da Sofia central; `message/send`, `tasks/get` e `tasks/cancel` como ela chama; erros JSON-RPC sem stack trace; chave fora de logs, respostas e documento OpenAPI; preflight de CORS sem chave. A fábrica de testes envia uma chave sintética por padrão.

## Risks / Trade-offs

- [Chave compartilhada por todos os agentes do treinamento, sem identificar o cliente] → serve ao treinamento; a identidade por cliente fica para a mudança 02.
- [IPs do Vercel compartilhados e variáveis] → a política de destino da Sofia central fica com o professor (D13).
- [`GetTask` sem garantia entre instâncias do Vercel] → a resposta de `message/send` já é final, e a Sofia só consulta quando não é.

- [Outros agentes da turma em v0.3] → a decisão pela v1.0 diverge da skill original. Um cliente v0.3 recebe `-32601`; `A2A.V0_3Compat` é a saída, se for necessário.
- [Endpoint público sem autenticação] → SQ-12: só dados sintéticos ou anonimizados, sem custo de provedor nesta mudança. Qualquer pessoa com a URL pode chamar, e a decisão é de Kaio Macedo.
- [Limite de corpo no Vercel] → medido em produção: entre 4,4 MB e 4,6 MB. Acima disso, a plataforma responde HTTP 413, sem resposta JSON-RPC. Na rede Docker esse limite não existe. Se um arquivo ECF real não couber, a mudança 07 precisa tratar, com novo ADR de hospedagem se preciso.
- [SDK em preview] → versão fixada e cobertura de contrato com o cliente do próprio SDK.
- [`GetTask` entre instâncias] → resposta de `SendMessage` é final; documentado.

## Migration Plan

1. Merge deste PR, depois da emenda v2.0.0 (PR #8, já mesclado).
2. O CD publica a Api.Agente; a verificação em produção cobre cartão, `SendMessage` e tamanho de corpo.
3. **Rollback:** promover o deploy anterior no Vercel (`docs/ci-cd.md`). Não há dados nem migrações.

## Open Questions

- Nenhuma para o servidor. A autenticação só volta se a mudança 02 for retomada (`docs/roteiro.md`).

## Skills aplicáveis

| Skill | Pré-condição | Situação |
| --- | --- | --- |
| `sofia-a2a-contrato` | Ler o contrato e os exemplos. | Atendida. |
| `sofia-a2a-implementacao` | Confirmar SDK antes de codificar. | Atendida (D1). |
| `sofia-cors-rest-a2a` | Inspecionar pipeline e configuração. | Atendida (D8). |
| `vercel-dotnet-pgvector-atlas` | ADR de hospedagem e DP-09 ou SQ-12 vigente. | Atendida com a emenda v2.0.0. |
| `testes-agentes-ia` | Testes determinísticos e adversariais. | Atendida. |
| `github-versionamento` | Branch por mudança e PR. | Atendida. |

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Não se aplica | Sem regras fiscais; o contrato é da turma e o protocolo tem fonte oficial. |
| P02 | Não se aplica | Sem validações de documentos fiscais ainda. |
| P03 | Atende | O adaptador só chama o serviço de aplicação; as etapas não implementadas são declaradas (D4). |
| P04 | Não se aplica | Sem achados nesta mudança; as categorias entram como campos opcionais na mudança 03. |
| P05 | Atende | Status `parcial`; `concluido` reservado (D3). |
| P06 | Não se aplica | Sem achados. |
| P07 | Atende | Nenhum achado ou evidência sem fundamento (D4). |
| P08 | Atende | Validações não realizadas declaradas nas observações (D4). |
| P09 | Não se aplica | Sem revisão humana no contrato v1.0; entra como campo opcional na mudança 03. |
| P10 | Atende | Documento e pergunta tratados como dado, com teste adversarial (REQ-A2A-004). |
| P11 | Não se aplica | Sem base de conhecimento. |
| P12 | Não se aplica | Sem LLM. |
| P13 | Atende | Status, hash e identificadores preenchidos pelo sistema. |
| P14 | Não se aplica | Sem ferramentas. |
| P15 | Atende | A Api.RAG não muda nem é citada; A2A só na Api.Agente. |
| P16 | Atende | Um pacote oficial novo, justificado em D1; sem projeto novo. |
| P17 | Não se aplica | Sem chamadas a provedores de IA. |
| P18 | Não se aplica | Sem memória conversacional. |
| P19 | Atende | Hash, identificadores e status registrados; o registro persistido entra na mudança 03. |
| P20 | Atende | Esta mudança; tarefas ligadas aos requisitos. |
