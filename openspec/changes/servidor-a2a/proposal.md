# Proposal: Servidor A2A para a Sofia INFIS

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 13; PR #4) e revista em 2026-11-05 com as skills da turma. Kaio Macedo pediu a implementação, aprovou a emenda v2.0.0 da constitution (PR #8) e decidiu protocolo, rede e autenticação em 2026-11-05.

## Why

O Agente-ECF é um dos agentes da Sofia INFIS, que o aciona por A2A (constitution, 1.1). As skills `sofia-a2a-contrato`, `sofia-a2a-implementacao` e `sofia-cors-rest-a2a` padronizaram essa integração para todos os agentes: protocolo, contrato de negócio, cartão, CORS e conectividade. Fazer a integração primeiro, como esqueleto, permite que a Sofia alcance o agente desde já e que as próximas mudanças preencham a análise sem alterar o contrato.

## What Changes

- Publica o cartão do agente em `/.well-known/agent-card.json`, com a skill `analisar_documentos`.
- Atende `SendMessage`, `GetTask` e `CancelTask` no endpoint `/a2a` (A2A v1.0, JSON-RPC).
- Recebe `SolicitacaoAnaliseSofia` e devolve `ResultadoAnaliseSofia`, com validação, correlação e erros controlados.
- Enquanto a análise não existir, responde com status `parcial`, declara as validações não realizadas e registra tamanho e hash de cada documento. Nunca declara conformidade.
- Configura CORS com a política `SofiaBrowser` e o arquivo `compose.yaml` para a rede `sofia_network`.
- Aceita a Sofia INFIS sem credencial: a integração não usa token (SQ-12).

## Capabilities

### New Capabilities
- `integracao/servidor-a2a`: exposição do Agente-ECF a outros agentes, a começar pela Sofia INFIS, pelo protocolo A2A.
- `integracao/contrato-sofia`: contrato SOFIA v1.0, validação, correlação e tratamento de erros.
- `integracao/cors`: origens de navegador autorizadas e independência das chamadas entre servidores.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Qual versão do protocolo A2A e qual SDK a Sofia INFIS usa? | A2A v1.0 com JSON-RPC, padronizado por Kaio Macedo. O servidor usa o SDK oficial `A2A.AspNetCore` 1.0.0-preview4. | Kaio Macedo | 2026-11-05 | Não |
| E02 | Quais capacidades expor no início: só analisar e consultar, ou também conversar (DP-16)? | Só a skill `analisar_documentos`, com o resultado na própria resposta. Sem consulta nem conversa. | `sofia-a2a-implementacao`, RF01 | 2026-11-05 | Não |
| E03 | A Sofia precisa de streaming ou notificações de andamento? | Não: `streaming` e `pushNotifications` são `false` no cartão do perfil da turma. | `sofia-a2a-contrato`, exemplos | 2026-11-05 | Não |
| E04 | Que cabeçalho de correlação a Sofia envia (pendência E06 da fundação)? | O contrato correlaciona por `idExecucao` e `idSolicitacao`; a skill pede propagar `traceId` e request ID em cabeçalhos, sem fixar o nome. O serviço mantém `X-Correlation-Id` e registra os dois identificadores. | `sofia-a2a-implementacao` | 2026-11-05 | Não |
| E05 | O cliente da Sofia fala v0.3? O projeto `SofiaInfis` referencia `Microsoft.Agents.AI.A2A` 1.27.0-preview, que traz o SDK `A2A` 1.0.0-preview3 (protocolo v1.0), e a skill fixava v0.3.x. | Padronizar em v1.0, compatível com o pacote da Sofia. | Kaio Macedo | 2026-11-05 | Não |
| E06 | Há frontend da Sofia que chame o agente pelo navegador? | Não encontrado no projeto `SofiaInfis`. `Cors:AllowedOrigins` fica vazio até existir uma origem real. | Kaio Macedo confirma | 2026-11-05 | Não |
| E07 | Qual o limite de tamanho dos documentos e o que o Vercel aceita no corpo da requisição? | Limite configurável, com padrão de 10 MiB até DP-05 ser resolvida na mudança 07. O corpo aceito pelo Vercel é medido em produção, depois do merge. | Mudança 07; verificação da tarefa 7.3 | 2026-11-05 | Não |
| E08 | Em que rede a Sofia alcança o agente? A skill usa `agentes-network`; o compose de `SofiaInfis` usa `sofia_network`, e a API dela roda fora dele. | `sofia_network`, cujo nome real no Docker é `sofia-infis_sofia_network`. | Kaio Macedo | 2026-11-05 | Não |
| E09 | Como a organização chega, se o contrato não tem esse campo? | Valor padrão configurado (mudança 03, E07), até haver necessidade de isolar clientes. | Kaio Macedo | 2026-11-05 | Não |

## Fontes oficiais

| Documento | Emissor | Versão | Localização |
| --- | --- | --- | --- |
| [Especificação A2A](https://a2a-protocol.org/v1.0.0/specification/) | Projeto A2A | 1.0 | Seção 3 (operações), 4 (dados), 8 (cartão) e 9 (JSON-RPC) |
| Contrato SOFIA | Pacote da turma SOFIA/INFIS | 1.0 | `.claude/skills/sofia-a2a-contrato/references/` |

Sem regras de domínio fiscal.

## Princípios, decisões e dependências

- **Princípios:** P03, P05, P07, P08, P10, P13, P16, P19; RA-07; GD-01, GD-06; SQ-04, SQ-05, SQ-07, SQ-12, SQ-13.
- **Decisões pendentes:** resolve DP-16 no arquivamento (autenticação do A2A fica em DP-09, adiada pela SQ-12); DP-05 continua pendente na mudança 07.
- **Depende de:** 01 `fundacao-solucao`. A análise completa chega com as mudanças 03 a 10. A credencial fica adiada (SQ-12; `docs/roteiro.md`).
- **Skills:** `sofia-a2a-contrato`, `sofia-a2a-implementacao`, `sofia-cors-rest-a2a`, `vercel-dotnet-pgvector-atlas`, `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Capacidades não especificadas no cartão do agente.
- Cliente A2A, registry, despacho e agregação: são da Sofia.
- Cliente A2A do Agente-ECF para chamar outros agentes (RF05).
- Validações da ECF, persistência e análise com LLM (mudanças 03 a 10).
- Autenticação (mudança 02).

## Impact

- Novo pacote `A2A.AspNetCore` na Api.Agente; novos endpoints `/.well-known/agent-card.json` e `/a2a`, públicos enquanto valer a SQ-12.
- Novos arquivos `compose.yaml`, `docs/contratos/sofia-a2a.md` e `docs/contratos/agent-card.json`.

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4); revisão e implementação pedidas por ele em 2026-11-05, com as skills da turma.
