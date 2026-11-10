# Proposal: Ferramentas do agente

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 11; PR #4). Esclarecimentos propostos em 2026-11-10, à espera da aprovação de Kaio Macedo. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

## Why

A etapa 6 do fluxo (P03) executa ferramentas externas quando necessário e autorizado. Com Function Calling, o texto do modelo vira ação; a constitution exige catálogo completo por ferramenta, autorização verificada a cada chamada e resultados registrados (P14). Nesta fase o agente só valida o arquivo, então ferramentas com efeito externo ficam bloqueadas (constitution, 1.3).

## What Changes

- Cria o catálogo de ferramentas da Api.Agente, com os campos exigidos por P14.
- Expõe ao modelo, por Semantic Kernel, somente ferramentas com catálogo completo.
- Verifica autorização e valida argumentos a cada chamada, com timeout e limite de chamadas por execução.
- Registra chamadas e resultados para conferir as citações do modelo.
- Implementa as ferramentas definidas em E01, se houver.

## Capabilities

### New Capabilities
- `agente/ferramentas`: catálogo, autorização, validação e registro das ferramentas usadas pelo agente.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Quais ferramentas externas o agente precisa (por exemplo, consultas a serviços públicos)? Pode ser nenhuma nesta fase (DP-10). | **Proposta:** nenhuma ferramenta externa nesta fase (DP-10). Consultas a serviços públicos, como o cadastro de CNPJ, não têm fonte registrada nem custo aprovado, e o agente só valida (constitution, 1.3). | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Sim |
| E02 | O modelo deve poder consultar partes do arquivo analisado por ferramenta (por exemplo, "obter registros de um tipo")? | **Proposta:** sim, com uma ferramenta interna, só de leitura e sem custo: `consultar_linhas`, que devolve até 20 linhas do arquivo da própria execução, por número de linha ou código de registro. Ela evita mandar o arquivo inteiro ao modelo (REQ-ANALISE-007) e passa pelo catálogo, pela autorização e pelo registro de P14. | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Não |
| E03 | Limite de chamadas de ferramenta por execução e timeout (DP-14). | **Proposta:** até 5 chamadas por execução e 2 segundos de timeout, sem retentativa, porque a leitura é local. Valores iniciais (DP-14). | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Não |

## Fontes oficiais

Não se aplica, salvo se uma ferramenta de E01 consultar serviço oficial; nesse caso, a fonte é registrada na especificação da ferramenta.

## Princípios, decisões e dependências

- **Princípios:** P07, P10, P14, P19; RA-06; SQ-06, SQ-12 (a política de cada ferramenta continua valendo; usuário e organização são declarados).
- **Decisões pendentes:** resolve DP-10.
- **Depende de:** 10 `analise-llm`.
- **Skills:** `semantic-kernel-tools`, `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Ferramentas com efeito externo (escrita, envio a terceiros ou custo), proibidas nesta fase.
- Validadores determinísticos como ferramentas escolhidas pelo modelo: eles rodam no fluxo (P02).

## Impact

- Catálogo de ferramentas e plugins do Semantic Kernel na infraestrutura da Api.Agente.

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
