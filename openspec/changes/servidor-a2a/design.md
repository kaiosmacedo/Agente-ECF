# Design: Servidor A2A para a Sofia INFIS

> **Estado:** implementado a pedido de Kaio Macedo em 2026-11-05, para revisão no PR. O merge depende da emenda v2.0.0 da constitution (PR #8).

## Context

A fundação tem só as rotas de saúde. As skills da turma definem como cada agente conversa com a Sofia: A2A v0.3.x com JSON-RPC, contrato SOFIA v1.0 em `TextPart` e CORS só para navegador. O repositório `SofiaInfis` ainda não tem cliente A2A. A análise da ECF só chega nas mudanças 03 a 10, então esta mudança entrega um esqueleto honesto: protocolo e contrato reais, com status `parcial`.

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

### D1. SDK oficial na linha 0.3

- `A2A.AspNetCore` 0.3.4-preview, com o cliente do mesmo pacote nos testes. A skill manda usar o SDK disponível e só escrever adaptador próprio se não houver um.
- **Alternativas:** a linha 1.0 (`A2A.AspNetCore` 1.0.0-preview4) fala o protocolo v1.0, contrário ao perfil fixado; ela tem camada de compatibilidade com v0.3 (`A2A.V0_3Compat`), útil se a turma migrar. Um JSON-RPC próprio teria menos dependências, mas mais protocolo para manter.
- **Custo:** pacote em preview, sem alvo `net10.0` (usa o de `net9.0`), com o aviso de que a linha 0.3 virou legada. Versão fixada; revisão se a turma migrar de perfil.

### D2. Camadas

| Camada | Conteúdo |
| --- | --- |
| `Aplicacao/ContratoSofia` | DTOs do contrato, validador e `ProcessadorDeSolicitacoes`, sem depender de A2A nem de ASP.NET Core. |
| `Infraestrutura/Integracao` | `AgenteA2A` (mensagem → processador → Artifact), cartão, `ArmazemDeTarefas` e opções. |
| `Infraestrutura/Cors` | Opções validadas e política `SofiaBrowser`. |
| `Endpoints` | Mapeamento de `/a2a` e `/.well-known/agent-card.json` e conferência do envelope JSON-RPC. |

Um teste de arquitetura impede `Aplicacao` e `Dominio` de dependerem do pacote `A2A`.

### D3. Mapeamento A2A e contrato

```text
POST /a2a (JSON-RPC message/send)
  params.message.parts[kind=text].text = SolicitacaoAnaliseSofia (JSON)
    -> ProcessadorDeSolicitacoes
  result = Task { status.state, artifacts[name=analise-sofia-v1].parts[kind=text].text = ResultadoAnaliseSofia (JSON) }
```

| Situação | Task | `status` do contrato | `erro.codigo` |
| --- | --- | --- | --- |
| Solicitação válida | `completed` | `parcial` | — |
| Campo inválido, com IDs válidos | `completed` | `erro` | `solicitacao_invalida` |
| Versão de contrato diferente de 1.0 | `completed` | `erro` | `versao_contrato_nao_suportada` |
| Textos acima do limite | `completed` | `erro` | `documentos_acima_do_limite` |
| Mensagem sem texto, JSON inválido ou IDs inválidos | `failed` | sem resultado | motivo em `status.message` |

Sem IDs válidos, não há como a Sofia correlacionar um resultado; o contrato prevê Task `failed` nesse caso. `concluido` fica reservado para quando todas as validações obrigatórias existirem (P05).

### D4. Resposta do esqueleto

`analise` explica que as validações não são executadas nesta versão. `observacoes` traz, por documento, `idDocumento`, número de caracteres e SHA-256 do texto em UTF-8, e a lista de validações não realizadas (P08, GD-06). `achados` fica vazio (P07). A resposta é determinística para a mesma solicitação, então uma retentativa com o mesmo `idSolicitacao` não tem efeito colateral; a deduplicação persistida entra na mudança 03.

### D5. Privacidade e memória (SQ-04)

- O `TaskManager` guarda a mensagem no histórico da tarefa. `AgenteA2A` limpa o histórico antes de salvar e de responder, então o documento não fica em memória nem volta na resposta.
- `ArmazemDeTarefas` limita a 1000 tarefas, por 1 hora, em `MemoryCache`, só com status e resultado. No Vercel, as instâncias não duram e `tasks/get` não é garantido; a resposta de `message/send` já é final.
- Logs: `idExecucao`, `idSolicitacao`, status e número de documentos. Nunca `usuario.nome`, `pergunta` ou `texto`.

### D6. Métodos fora do perfil

O SDK expõe streaming e notificações mesmo com o cartão dizendo que não há, e não consegue devolver erro JSON-RPC depois de abrir o fluxo. Um filtro no endpoint confere o envelope antes do SDK: `message/stream` e `tasks/resubscribe` recebem `-32004`, e JSON malformado recebe `-32700`. O corpo fica em memória nessa leitura, sem arquivo temporário com o documento. O armazém recusa notificações com `-32003`.

### D7. Cartão

`protocolVersion` `0.3.0`, `preferredTransport` `JSONRPC`, skill `analisar_documentos`, `defaultInputModes` e `defaultOutputModes` iguais a `["text"]`, como no exemplo da turma. A cópia versionada fica em `docs/contratos/agent-card.json` (GD-01), e um teste compara. `url` vem de `A2A:UrlPublica`, porque atrás do proxy do Vercel o esquema e o host da requisição não são confiáveis; sem a opção, vale o endereço da requisição. O valor de produção fica em `appsettings.Production.json`, por não ser segredo.

### D8. CORS (SQ-13)

Política `SofiaBrowser`, com `Cors:AllowedOrigins` opcional e validada: o agente não tem navegador hoje, e a skill manda não exigir a configuração nesse caso. O escopo é só `/a2a` e o cartão, com `UseWhen` e `UseCors(política)`: `RequireCors` por endpoint não responde a preflight automaticamente. Métodos `GET`, `POST` e `OPTIONS`; cabeçalhos `Content-Type`, `Authorization` e `X-Correlation-Id`; sem credenciais. Na retomada da mudança 02, CORS fica antes da autenticação.

### D9. Rede

`compose.yaml` na raiz: serviço `agente-ecf`, porta `8085:8080`, rede externa `agentes-network`, como na skill, e `A2A__UrlPublica` com o nome do serviço. A rede real da Sofia é o esclarecimento E08.

## Risks / Trade-offs

- [Cliente da Sofia em v1.0] → E05: o servidor fala v0.3, como a skill fixa. Se o cliente não usar v0.3, a chamada falha com método desconhecido. Levar ao professor; `A2A.V0_3Compat` é a saída de cada lado.
- [Endpoint público sem autenticação] → SQ-12: só dados sintéticos ou anonimizados, sem custo de provedor nesta mudança. Qualquer pessoa com a URL pode chamar.
- [Limite de corpo no Vercel] → medido em produção (tarefa 7.3). Se for menor que um arquivo ECF real, a mudança 07 precisa tratar, com novo ADR de hospedagem se preciso.
- [SDK em preview e linha legada] → versão fixada e cobertura de contrato com o cliente do próprio SDK. Esse SDK exige `acceptedOutputModes` quando a mensagem traz `configuration`, embora a especificação o trate como opcional; o cliente .NET sempre o envia, e o exemplo da turma não usa `configuration`.
- [`tasks/get` entre instâncias] → resposta de `message/send` é final; documentado.

## Migration Plan

1. Merge da emenda v2.0.0 (PR #8) e depois deste PR.
2. O CD publica a Api.Agente; a verificação em produção cobre cartão, `message/send` e tamanho de corpo.
3. **Rollback:** promover o deploy anterior no Vercel (`docs/ci-cd.md`). Não há dados nem migrações.

## Open Questions

- E05 e E08: protocolo do cliente e rede da Sofia, com o professor.
- Mecanismo de autenticação: `docs/roteiro.md`, seção "Autenticação adiada".

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
