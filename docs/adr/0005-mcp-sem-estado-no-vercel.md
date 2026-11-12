# 0005 — MCP sem estado no Vercel

| Campo | Valor |
| --- | --- |
| Estado | Aceito |
| Data | 2026-11-07 |
| Responsável pela decisão | Kaio Macedo |
| Relacionado a | DP-18, ADR 0002, RA-03, P15, mudança `recuperacao-conhecimento-mcp` (E01) |

## Contexto

A Api.Agente consulta a base de conhecimento da Api.RAG só por MCP (P15, RA-03). As duas rodam no Vercel (ADR 0002), onde as instâncias não duram e não há afinidade: duas requisições seguidas do mesmo cliente podem cair em instâncias diferentes. O ADR 0002 deixou para a prova de conceito da mudança 06 saber se o MCP funciona assim, e a skill `mcp-client-server` exige este ADR antes da implementação.

O transporte HTTP do MCP (Streamable HTTP) guarda, por padrão, uma sessão em memória, identificada pelo cabeçalho `Mcp-Session-Id`. Uma chamada que cai em outra instância não encontra a sessão.

## Prova de conceito

Feita em 2026-11-07, só em `localhost`, com duas instâncias do mesmo servidor atrás de um proxy que alterna a instância a cada requisição, e de outro que derruba a primeira no meio. Relatório completo no scratchpad da sessão do assistente; o resumo:

| Teste | Resultado |
| --- | --- |
| Pacotes | `ModelContextProtocol.AspNetCore` e `ModelContextProtocol.Core` 2.4.0, estáveis. O SDK usa por padrão a revisão 2026-07-28 da especificação do MCP e aceita as anteriores, a partir da 2024-11-05. |
| Modo sem estado (`HttpServerSessionMode.Stateless`), requisições alternadas entre as instâncias | `tools/list` e `tools/call` funcionaram, inclusive com a primeira instância derrubada. Valeu na revisão 2026-07-28, que não tem `initialize`, e na 2025-11-25, com `initialize` numa instância e as chamadas na outra. |
| Mesmo teste com sessão (padrão do SDK) | Falhou com 404 "Session not found". O teste distingue os dois modos. |
| `POST tools/call` direto, sem `initialize` e sem `Mcp-Session-Id` | 200. O servidor sem estado nunca emite `Mcp-Session-Id`. |
| Tipo da resposta | Sempre `text/event-stream`, com um único evento; só erros HTTP vêm em `application/json`. A versão 2.4.0 não responde em JSON simples. Os cabeçalhos só saem quando o resultado fica pronto. |
| Argumento inválido | Erro da ferramenta chega ao cliente como resultado com `isError`; ferramenta inexistente, como erro JSON-RPC `-32602`. |
| Timeout do cliente | Chega como `TaskCanceledException`, e o servidor cancela a ferramenta quando a conexão cai. |
| Correlação | O cabeçalho `X-Correlation-Id` enviado pelo cliente chega à ferramenta pelo `IHttpContextAccessor`. |

## Decisão

- A Api.RAG expõe as ferramentas MCP em `/mcp`, com o Streamable HTTP do SDK oficial em modo sem estado.
- A Api.Agente usa o cliente do mesmo SDK, aceitando `application/json` e `text/event-stream`.
- Versões fixadas: `ModelContextProtocol.AspNetCore` 2.4.0 na Api.RAG e `ModelContextProtocol.Core` 2.4.0 na Api.Agente (SQ-11). A 2.4.0 saiu no dia da prova de conceito; a verificação de vulnerabilidades do CI vale para ela como para os demais pacotes.
- Nenhuma ferramenta depende de estado entre chamadas. Cada chamada leva tudo o que precisa: consulta, tipo, versão e execução.

## Alternativas

- **Sessão com afinidade:** o Vercel não oferece afinidade entre instâncias. Descartada.
- **Sessão guardada fora da memória, em banco ou cache:** resolveria, mas acrescenta dependência sem necessidade, porque nenhuma ferramenta precisa de estado.
- **REST entre os serviços:** violaria P15 e RA-03.
- **Host persistente para a Api.RAG:** é a alternativa se a verificação em produção falhar (Consequências). Exigiria novo ADR que substitui o 0002 na parte da Api.RAG.

## Consequências

- O MCP funciona com a hospedagem atual, sem serviço novo.
- Recursos que dependem de sessão, como notificações do servidor e assinatura de recursos, ficam fora. Nenhum é necessário para a busca.
- **Confirmado no Vercel** (tarefa 6.2 da mudança 06): em 2026-11-07, `tools/list` e `busca_desativada` em `text/event-stream`; em 2026-11-12, com a chave, `tools/call` com 5 trechos do leiaute 12, sem sessão. O tempo máximo de execução não foi medido à parte. Texto original da pendência: que a resposta `text/event-stream` passa sem ficar retida, o tempo máximo de execução da função para a busca e o limite de 4,5 MB do corpo, que não pesa para consultas pequenas. Se o transporte falhar em produção, a busca fica desativada por configuração e a etapa 5 vira limitação até a decisão sobre o host.

## Verificação

- Teste de integração da mudança 06 com duas instâncias da Api.RAG atrás de um proxy que alterna as requisições.
- Teste de arquitetura: a Api.Agente não tem conexão com o banco da Api.RAG (P15). Que nenhuma ferramenta MCP guarda estado entre chamadas é coberto pelo teste com duas instâncias alternadas (`McpTests`), não por um teste de arquitetura.
- Em produção, `tools/list` e `tools/call` respondidos por instâncias diferentes (tarefa 6.2 da mudança 06).
