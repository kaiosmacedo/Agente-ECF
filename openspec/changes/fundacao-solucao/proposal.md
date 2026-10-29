# Proposal: Fundação da solução

## Why

O Agente-ECF ainda não tem código. Antes de receber arquivos, consultar manuais ou chamar a OpenAI, os dois serviços precisam de uma base comum e verificável: estrutura em camadas, configuração validada, saúde observável, correlação entre chamadas, regras de arquitetura testadas, pipeline que bloqueia regressões e contêineres prontos para a hospedagem escolhida. Sem essa base, cada mudança seguinte reinventaria esses pontos e as violações da constitution (P15, P16, SQ-03, SQ-07) só seriam percebidas em revisão manual.

## What Changes

- Cria a solução .NET 10 com os serviços **Api.Agente** e **Api.RAG**, um projeto por serviço, com camadas em pastas (Endpoints, Aplicacao, Dominio, Infraestrutura), e um projeto de testes por serviço.
- Expõe em cada serviço verificações de saúde anônimas (vivacidade e prontidão), sem dados.
- Valida a configuração obrigatória na inicialização: configuração ausente ou inválida impede o serviço de subir, sem revelar valores.
- Propaga um identificador de correlação em cada requisição e grava logs estruturados sem dados sensíveis.
- Verifica automaticamente as regras de arquitetura (direção das camadas, domínio sem infraestrutura, Api.Agente sem acesso aos dados da Api.RAG).
- Completa o pipeline de integração: analisadores e formatação como bloqueio, testes, pacotes vulneráveis, construção das imagens de contêiner e atualização de dependências NuGet.
- Prepara cada serviço para rodar em contêiner no Vercel, publica em produção a cada merge na `main` e verifica a saúde após cada publicação. Registra em ADR a hospedagem e os riscos do MCP e do A2A nessa plataforma.
- Registra os ADRs de organização da solução (DP-17) e de hospedagem e CI/CD (DP-18).

## Capabilities

### New Capabilities
- `plataforma/saude-servicos`: verificações de vivacidade e prontidão de cada serviço, acessíveis sem autenticação e sem expor dados.
- `plataforma/configuracao-servicos`: origem externa da configuração e validação obrigatória na inicialização.
- `plataforma/observabilidade`: identificador de correlação por requisição e logs estruturados sem dados sensíveis.
- `plataforma/qualidade-entrega`: verificações automáticas que bloqueiam o merge na `main` (especificação, compilação, analisadores, testes, regras de arquitetura e dependências).
- `plataforma/entrega-servicos`: empacotamento de cada serviço em contêiner executável na hospedagem escolhida e publicação nos ambientes definidos.

### Modified Capabilities
- Nenhuma. Não há specs vigentes.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Onde os serviços vão rodar (DP-18)? | Vercel com contêineres, como no projeto AgenteAnaliseDIMP. | Kaio Macedo | 2026-10-29 | Não (respondido) |
| E02 | Como organizar a solução (DP-17)? | Um projeto por serviço, com camadas em pastas verificadas por testes de arquitetura. | Kaio Macedo | 2026-10-29 | Não (respondido) |
| E03 | Qual framework de testes? | xUnit, com Testcontainers para integração. | Kaio Macedo | 2026-10-29 | Não (respondido) |
| E04 | Quais ambientes recebem deploy e quando? | Somente produção, a cada merge na `main`; PRs não geram deploy. | Kaio Macedo | 2026-10-29 | Não (respondido) |
| E05 | Quem cria e conecta os projetos no Vercel? | Feito em conjunto quando necessário: o assistente executa o que puder sozinho e conduz o responsável, passo a passo, no que exigir a conta dele no Vercel ou no GitHub (login, criação de projetos, token e segredos). | Kaio Macedo | 2026-10-29 | Não (respondido) |
| E06 | Que cabeçalho de correlação a Sofia INFIS envia? | Sem resposta. Padrão adotado: `X-Correlation-Id`; revisar na mudança do A2A (DP-16). | Kaio Macedo | 2026-10-29 | Não |

## Aprovação

Especificação, plano técnico e tarefas **aprovados por Kaio Macedo em 2026-10-29**, revisados em conjunto antes do início da implementação, como o responsável optou. O plano técnico e as tarefas foram redigidos antes dessa aprovação, a pedido do responsável, e aprovados sem ajustes.

## Fontes oficiais

Não se aplica: esta mudança não contém regras de domínio fiscal ou contábil (P01). As referências técnicas de hospedagem ficam no ADR de DP-18.

## Princípios e decisões

- **Princípios impactados:** P15, P16, P19 (versões registradas nos logs), P20; RA-01, RA-02, RA-08, RA-09; SQ-01 (saúde anônima sem dados), SQ-03, SQ-04, SQ-05, SQ-07, SQ-08, SQ-09, SQ-11.
- **Decisões pendentes resolvidas:** DP-17 (ADR 0001) e DP-18 (ADR 0002).
- **Decisões pendentes de que depende:** nenhuma. DP-09 não é necessária, pois só há endpoints de saúde anônimos.

## Fora do escopo

- Endpoints funcionais (recepção de arquivos, análise, administração da base de conhecimento).
- Autenticação e autorização (DP-09), além do acesso anônimo às verificações de saúde.
- Bancos de dados (PostgreSQL, PgVector, MongoDB ou Redis), OpenAI, Semantic Kernel, MCP e A2A.
- Persistência de execuções e resultados (DP-25) e registro de consumo (P17), que entram com as mudanças que os exigirem.
- Exportação de telemetria para uma plataforma de observabilidade externa.

## Impact

- **Código:** nova solução em `src/` e `tests/`, arquivos comuns de build na raiz e `Dockerfile` por serviço.
- **Pipeline:** `.github/workflows/ci.yml` passa a exigir formatação, analisadores e construção das imagens; `.github/dependabot.yml` inclui NuGet.
- **Documentação:** ADRs 0001 e 0002 em `docs/adr/`, `docs/ci-cd.md` e constitution (DP-17 e DP-18 movidas para decisões resolvidas no arquivamento).
- **Operação:** projetos no Vercel conectados ao repositório, criados pelo responsável (E05).
