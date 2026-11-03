# Proposal: Autenticação e autorização

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4); esclarecimentos respondidos em 2026-11-01; ajustes de requisitos e recomendações R1 a R5 confirmados por Kaio Macedo em 2026-11-03. Plano técnico e tarefas aprovados por Kaio Macedo em 2026-11-03.

## Why

A constitution exige autenticação em todo ponto de entrada (SQ-01) e autorização por usuário e organização (SQ-02), e proíbe expor endpoints funcionais antes de DP-09. A próxima mudança com endpoint funcional (execução da análise ou ingestão de manuais) depende desta. A identidade autenticada também é a base do isolamento de execuções, memória e consumo.

Os principais chamadores serão agentes: a Sofia INFIS, por A2A, e, futuramente, a Maia, que também vai acionar e acompanhar o agente como o orquestrador faria. Eles atendem várias organizações clientes da INFIS. Por isso a autorização precisa decidir, a cada requisição, se aquela identidade pode atuar em nome da organização informada.

## What Changes

- Autentica as requisições REST da Api.Agente e da Api.RAG com tokens emitidos pelo provedor de identidade; as verificações de saúde continuam anônimas.
- Obtém a identidade do chamador somente da credencial. A organização de cada requisição é a informada pelo chamador, quando ele atua em mais de uma, ou a única autorizada, e precisa constar das autorizações dessa identidade.
- Define os perfis do agente: administrador do agente, operador da orquestradora, cliente agente (Sofia, Maia) e usuário de análise.
- Expõe a consulta da identidade autenticada, para os clientes conferirem credencial, perfis e organização resolvida.
- Autentica as chamadas entre serviços (MCP da Api.Agente para a Api.RAG) com credencial de serviço.
- Registra em ADR o mecanismo escolhido (DP-09).

## Capabilities

### New Capabilities
- `plataforma/autenticacao`: autenticação de pessoas, agentes e serviços, perfis, autorização por organização e isolamento dos recursos.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Quem chama o agente e qual provedor de identidade a INFIS usa? | Principalmente a Sofia INFIS, por A2A, com base no cartão do agente. Também haverá endpoints para outros usos, e futuramente a Maia vai acionar e controlar o agente como o orquestrador. O provedor de identidade não foi definido: ver E06 e R1. | Kaio Macedo | 2026-11-01 | Não (respondido) |
| E02 | Como a Sofia INFIS se autentica ao chamar o agente? | OAuth client credentials, com registro de aplicação próprio (R2, confirmada). | Kaio Macedo | 2026-11-03 | Não (respondido) |
| E03 | Existe organização a isolar? Um usuário pode atuar em mais de uma? | Sim: o objetivo é atender várias organizações clientes da INFIS, isoladas entre si; um usuário provavelmente atuará em mais de uma. Consequência: ajuste de REQ-AUTH-003 (organização autorizada por requisição) e recomendação R4. | Kaio Macedo | 2026-11-01 | Não (respondido) |
| E04 | Há perfis além de administrador da base e usuário de análise? | Sim: quem cuida da orquestradora (Sofia INFIS) e o desenvolvedor do agente (Kaio Macedo). Consequência: REQ-AUTH-007 e recomendação R5. | Kaio Macedo | 2026-11-01 | Não (respondido) |
| E05 | A Api.Agente se autentica na Api.RAG pelo mesmo provedor ou por chave de serviço? | Pelo mesmo provedor, com client credentials (R3, confirmada). | Kaio Macedo | 2026-11-03 | Não (respondido) |
| E06 | Confirma a recomendação R1 para o provedor de identidade? | Sim: OAuth 2.0/OIDC com Microsoft Entra ID. | Kaio Macedo | 2026-11-03 | Não (respondido) |
| E07 | Confirma a regra de organização por requisição da recomendação R4? | Sim: organização por requisição, aceita só se autorizada; autorizações em configuração no início. | Kaio Macedo | 2026-11-03 | Não (respondido) |
| E08 | O que o operador da orquestradora pode fazer (recomendação R5)? | Acompanhar execuções e falhas das organizações atendidas pela Sofia, sem criar análises nem administrar a base (R5, confirmada). | Kaio Macedo | 2026-11-03 | Não (respondido) |
| E09 | Qual identificador usar para cada organização (código interno do cliente na INFIS, CNPJ ou outro)? | Aberto. O desenho aceita um identificador opaco e não depende da escolha. | Kaio Macedo | — | Não |

## Recomendações para decisão

| ID | Recomendação | Motivo | Alternativa |
| --- | --- | --- | --- |
| R1 | Tokens OAuth 2.0 / OpenID Connect (JWT), validados por emissor e audiência configuráveis, sem amarrar o código a um fornecedor. Provedor: **Microsoft Entra ID**. | A INFIS já usa Microsoft 365 (o OneDrive corporativo indica isso), então pessoas e aplicações já podem ser registradas lá, sem nova ferramenta. É padrão aberto: trocar de provedor vira configuração. | Outro provedor OIDC usado pela Sofia; chaves de API próprias, mais simples, mas sem expiração nem perfis nativos. |
| R2 | A Sofia e a Maia recebem registros de aplicação próprios no provedor e obtêm token de serviço por OAuth **client credentials**. | Sem segredo fixo compartilhado com o agente; tokens expiram; cada agente tem identidade e perfis próprios, o que permite auditar quem pediu cada análise. | Chave de API por agente, guardada como segredo e rotacionada manualmente. |
| R3 | A Api.Agente obtém token de serviço por client credentials para chamar a Api.RAG por MCP. | Mesmo mecanismo da R2, sem segredo estático entre os serviços (SQ-03). | Chave de serviço compartilhada entre os dois serviços. |
| R4 | A identidade vem do token. A organização vem de um cabeçalho da requisição e só é aceita se a identidade estiver autorizada para ela. No início, as autorizações (quem atua por quais organizações) ficam em configuração validada na inicialização; quando houver gestão de clientes, migram para banco. | Atende agentes e usuários que atuam por várias organizações sem emitir um token por organização. A configuração evita criar banco nesta mudança (P16), já que os clientes iniciais são poucos (Sofia e Maia). | Organizações como grupos ou papéis no provedor de identidade; cadastro em banco desde já. |
| R5 | Perfis: **administrador do agente** (desenvolvedor: base de conhecimento, consumo e configuração); **operador da orquestradora** (equipe da Sofia: acompanhar execuções e falhas das organizações atendidas pela Sofia, sem criar análises nem administrar a base); **cliente agente** (Sofia e Maia: criar análises e consultar resultados das organizações autorizadas); **usuário de análise** (pessoas que usarem os endpoints diretamente, no futuro). | Separa quem administra, quem acompanha e quem executa (menor privilégio, SQ-02). | Um perfil único para a equipe da Sofia e o desenvolvedor. |

## Fontes oficiais

Não se aplica: sem regras de domínio fiscal.

## Princípios, decisões e dependências

- **Princípios:** P14 (autorização independente da escolha do modelo), P15, P18 (isolamento), P19 (auditoria por identidade); SQ-01, SQ-02, SQ-03, SQ-04.
- **Decisões pendentes:** resolve DP-09 (ADR); prepara DP-16 (autenticação do A2A).
- **Depende de:** 01 `fundacao-solucao` (concluída).
- **Skills:** `testes-agentes-ia`, `github-versionamento`.
- **Observação para a mudança 13:** a Maia é um segundo cliente A2A previsto; o que significa "controlar" o agente será esclarecido na especificação do servidor A2A.

## Fora do escopo

- Cadastro e gestão de usuários e aplicações (feitos no provedor de identidade).
- Tela ou API de gestão das autorizações por organização (inicialmente em configuração, conforme R4).
- Autorização das ferramentas do agente (mudança 11) e do A2A (mudança 13), que reutilizam esta base.

## Impact

- Pipeline dos dois serviços (autenticação, políticas por perfil e resolução da organização), endpoint `GET /identidade`, configuração do provedor e das autorizações por organização.
- Registros de aplicação no provedor de identidade para a Sofia, a Maia e a Api.Agente, criados com o responsável.
- ADR novo em `docs/adr/` (DP-09).

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Ajustes de 2026-11-01 (REQ-AUTH-002, REQ-AUTH-003, REQ-AUTH-007 e REQ-AUTH-008) e recomendações R1 a R5 confirmados por Kaio Macedo em 2026-11-03. Plano técnico e tarefas aprovados por Kaio Macedo em 2026-11-03.
