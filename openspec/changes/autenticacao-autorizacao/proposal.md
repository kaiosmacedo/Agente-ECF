# Proposal: Autenticação e autorização

> **Estado:** especificada para validação de Kaio Macedo (roteiro, mudança 02). Plano técnico e tarefas só depois da aprovação e dos esclarecimentos bloqueantes.

## Why

A constitution exige autenticação em todo ponto de entrada (SQ-01) e autorização por usuário e organização (SQ-02), e proíbe expor endpoints funcionais antes de DP-09. A próxima mudança com endpoint funcional (execução da análise ou ingestão de manuais) depende desta. A identidade autenticada também é a base do isolamento de execuções, memória e consumo.

## What Changes

- Autentica as requisições REST da Api.Agente e da Api.RAG; as verificações de saúde continuam anônimas.
- Autoriza por perfil (administrador da base de conhecimento, usuário de análise) e por organização.
- Deriva usuário e organização exclusivamente da credencial autenticada.
- Autentica as chamadas entre serviços (MCP da Api.Agente para a Api.RAG) com credencial de serviço.
- Registra em ADR o mecanismo escolhido (DP-09).

## Capabilities

### New Capabilities
- `plataforma/autenticacao`: autenticação de usuários e serviços, autorização por perfil e organização e isolamento dos recursos por organização.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Qual provedor de identidade a INFIS usa (por exemplo, Microsoft Entra ID) e as pessoas acessam o agente diretamente ou só pela Sofia INFIS? | — | Kaio Macedo | — | Sim |
| E02 | Como a Sofia INFIS se autentica ao chamar o agente (OAuth client credentials, chave de API, outro)? | — | Kaio Macedo | — | Sim, para a integração com a Sofia |
| E03 | Existe o conceito de organização (cliente da INFIS) a isolar? Um usuário pode atuar em mais de uma organização? | — | Kaio Macedo | — | Sim |
| E04 | Há perfis além de administrador da base e usuário de análise? | — | Kaio Macedo | — | Não |
| E05 | A Api.Agente se autentica na Api.RAG com o mesmo provedor (client credentials) ou com chave de serviço? | — | Kaio Macedo | — | Não (decidido no ADR) |

## Fontes oficiais

Não se aplica: sem regras de domínio fiscal.

## Princípios, decisões e dependências

- **Princípios:** P14 (autorização independente da escolha do modelo), P15, P18 (isolamento), SQ-01, SQ-02, SQ-03, SQ-04.
- **Decisões pendentes:** resolve DP-09 (ADR); prepara DP-16 (autenticação do A2A).
- **Depende de:** 01 `fundacao-solucao`.
- **Skills:** `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Cadastro e gestão de usuários (feitos no provedor de identidade).
- Autorização das ferramentas do agente (mudança 11) e do A2A (mudança 13), que reutilizam esta base.

## Impact

- Pipeline dos dois serviços (middleware de autenticação e políticas de autorização), configuração do provedor e segredos de serviço.
- ADR novo em `docs/adr/` (DP-09).

## Aprovação

Pendente da validação de Kaio Macedo.
