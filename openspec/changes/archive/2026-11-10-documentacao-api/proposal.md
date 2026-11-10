# Proposal: Documentação da API com Swagger

> **Estado:** pedido de Kaio Macedo em 2026-11-10: "Gere essa url exibindo a documentação do API com o swagger", com o endereço `https://agente-ecf-agente.vercel.app/swagger`. Mudança sem regra de domínio, com os artefatos gerados de uma vez (constitution, 6.1). Kaio Macedo pediu a implementação em seguida: "Sim, implementar".

## Why

A Api.Agente já publica o documento OpenAPI em `/openapi/v1.json`, mas não há página para lê-lo nem para testar as rotas. Quem integra o agente, como a Sofia e outros agentes da INFIS, precisa de um endereço com a documentação navegável. Além disso, o documento anuncia o servidor como `http://agente-ecf-agente.vercel.app/`, o que faria as chamadas de teste de uma página de documentação falharem em produção, que só aceita https.

## What Changes

- Serve a interface do Swagger em `/swagger` na Api.Agente, em todos os ambientes, inclusive produção, a partir do documento OpenAPI já publicado.
- Passa a declarar o servidor do documento OpenAPI de forma relativa, para que as chamadas de teste usem o esquema e o host da própria página.
- Serve os arquivos da interface pelo próprio serviço, sem script ou estilo de outro domínio.

## Capabilities

### New Capabilities
- `plataforma/documentacao-api`: página de documentação navegável da API REST e A2A de cada serviço, a partir do contrato OpenAPI (GD-01).

### Modified Capabilities
- Nenhuma: as rotas e os contratos não mudam.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | A documentação fica pública em produção, sem autenticação? | Sim. Kaio Macedo pediu o endereço de produção. As rotas documentadas já são públicas na primeira versão (SQ-12), e a página só as descreve, sem segredo nem configuração. | Kaio Macedo | 2026-11-10 | Não |
| E02 | A Api.RAG também ganha a página? | Não nesta mudança: o pedido cita só a Api.Agente. A capacidade fica em `plataforma/` para a Api.RAG seguir o mesmo padrão depois. | Pedido de Kaio Macedo | 2026-11-10 | Não |
| E03 | As chamadas de teste da página devem funcionar em produção? | **Proposta:** sim, com o servidor relativo, e só com dados sintéticos, como todo uso da primeira versão (SQ-12). | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Não |

## Fontes oficiais

Não se aplica: a mudança não tem regra de domínio fiscal nem contábil.

## Princípios, decisões e dependências

- **Princípios:** P16 e SQ-11 (um pacote novo, justificado no design); GD-01 (o contrato OpenAPI continua no repositório e passa a ter página); SQ-12 (sem autenticação, só dados sintéticos); P20.
- **Decisões pendentes:** nenhuma resolvida; DP-09 continua pendente, e a página segue a autenticação que a API tiver quando ela chegar.
- **Depende de:** 01 `fundacao-solucao` e 03 `execucao-e-resultado`, já em produção.
- **Skills:** `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- A página da Api.RAG (E02).
- Autenticação da página, que acompanha DP-09.
- Mudanças nas rotas, nos contratos REST e A2A ou no conteúdo do documento além do servidor relativo.

## Impact

- `Program.cs` e a configuração do OpenAPI da Api.Agente; um pacote só com a interface do Swagger; testes HTTP; `README.md` e `docs/roteiro.md`.
