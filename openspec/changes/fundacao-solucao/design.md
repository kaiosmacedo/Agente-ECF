# Design: Fundação da solução

> **Estado:** rascunho aguardando a revisão de Kaio Macedo, junto com a especificação e as tarefas (ver `proposal.md`, seção Aprovação).

## Context

O repositório tem a constitution v1.2.0, o OpenSpec e o CI com os jobs `OpenSpec` e `.NET`; o job `.NET` não faz nada enquanto não houver solução. Não há código. Os motivos e o escopo estão em `proposal.md`, e os requisitos nas specs desta mudança. Restrições que moldam o desenho:

- .NET 10 e ASP.NET Core (RA-01); dois serviços independentes, sem biblioteca compartilhada de domínio ou dados (RA-02, RA-10, P15).
- Um projeto por serviço, com camadas em pastas verificadas por testes (E02, ADR 0001); xUnit (E03).
- Hospedagem no Vercel com contêineres, runtime em beta que roda como Vercel Functions sem estado, com porta definida por `PORT` (padrão 80) e desligamento após ociosidade (E01, ADR 0002). Fonte: guia do Vercel sobre contêineres, atualizado em 2026-10-29.
- Deploy só em produção, a cada merge na `main` (E04); a configuração da conta do Vercel é feita com o responsável (E05).

## Goals / Non-Goals

**Goals:**
- Estrutura que as próximas mudanças apenas preenchem, sem reorganizar pastas, build ou pipeline.
- Cada requisito da especificação verificado por teste automatizado ou etapa do pipeline.
- Paridade entre o contêiner construído no CI e o construído pelo Vercel (mesmo Dockerfile e mesmo contexto).

**Non-Goals:**
- Abstrações para bancos, OpenAI, MCP ou A2A; entram com as mudanças que as usarem (P16).
- Exportação de telemetria (OpenTelemetry) para plataforma externa.
- Domínio próprio, CDN ou ambientes de preview.

## Decisions

### D1. Solução e projetos (ADR 0001)

```
Agente-ECF.slnx
global.json  Directory.Build.props  .editorconfig
src/Api.Agente/  Api.Agente.csproj  Program.cs  Dockerfile  .dockerignore  vercel.json
                 Endpoints/  Aplicacao/  Dominio/  Infraestrutura/
src/Api.RAG/     (mesma estrutura)
tests/Api.Agente.Tests/   tests/Api.RAG.Tests/
```

- Namespaces seguem as pastas (`Api.Agente.Dominio`, `Api.Agente.Infraestrutura`...), e é sobre eles que os testes de arquitetura atuam.
- Minimal APIs do ASP.NET Core; nenhum controller.
- **Alternativas:** projetos por camada (isolamento pelo compilador, porém oito projetos de produção; descartada pelo responsável em E02); biblioteca comum para correlação e saúde (descartada: acopla os serviços, e o código duplicado é pequeno e testado nos dois).

### D2. Build comum

- `global.json` fixa o SDK 10.0 com `rollForward: latestFeature`.
- `Directory.Build.props`: `net10.0`, `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended` e `EnforceCodeStyleInBuild` (REQ-QUALIDADE-002).
- `.editorconfig` define o padrão verificado por `dotnet format --verify-no-changes`.
- Sem gerenciamento central de pacotes: as versões ficam em cada `.csproj`, para que a pasta do serviço restaure sozinha no contêiner (D7).
- **Alternativa:** `Directory.Packages.props` (versões centralizadas). Descartada porque obrigaria o contexto do contêiner a incluir a raiz do repositório, quebrando a paridade com o Root Directory do Vercel.

### D3. Saúde (REQ-SAUDE-001 a 003)

- Health checks nativos do ASP.NET Core. `/health/live` não executa verificações; `/health/ready` executa apenas as marcadas com a tag `ready` (nenhuma nesta mudança).
- Um escritor de resposta próprio devolve apenas `{"status":"Healthy|Unhealthy"}`, sem descrições nem exceções (REQ-SAUDE-003, SQ-01).
- Endpoints anônimos por construção: não há autenticação nesta mudança, e eles serão marcados como anônimos quando a autenticação chegar (DP-09).
- **Alternativa:** biblioteca de UI de health checks. Descartada: expõe detalhes e adiciona dependência.

### D4. Configuração (REQ-CONFIG-001 e 002)

- Fontes padrão do ASP.NET Core: `appsettings.json`, `appsettings.{Ambiente}.json` e variáveis de ambiente, nesta ordem de prioridade crescente.
- Opções tipadas com validação na inicialização (`ValidateOnStart`). As mensagens usam somente o nome da opção.
- Primeira opção real: `Observabilidade:NivelMinimo`, com padrão `Information`.
- Um teste em cada serviço lê os `appsettings*.json` versionados e falha se uma chave com nome de segredo (senha, password, chave, key, token, secret, segredo, connectionstring) tiver valor.

### D5. Correlação e logs (REQ-OBSERV-001 a 003)

- Middleware próprio, o primeiro do pipeline: valida `X-Correlation-Id` com `^[A-Za-z0-9._-]{1,128}$`, gera um GUID em formato `N` quando ausente ou inválido, devolve o valor na resposta e abre um escopo de log com `CorrelationId`, `Servico` e `Versao`.
- Logger nativo em JSON (`AddJsonConsole`, `IncludeScopes = true`) na saída padrão. Nenhuma biblioteca extra (P16).
- `HttpLogging` desligado; nenhum corpo, `Authorization` ou `Cookie` é registrado. Valor inválido de correlação nunca é escrito em log (SQ-05).
- Versão do serviço: `AssemblyInformationalVersion`, preenchida no CI com o SHA do commit e no contêiner por argumento de build.
- **Alternativa:** Serilog. Descartada nesta fase: o logger nativo atende aos requisitos sem dependência nova.

### D6. Testes (ADR 0001)

- xUnit v3, `Microsoft.AspNetCore.Mvc.Testing` (WebApplicationFactory) para testes HTTP em memória e um provedor de log em memória para verificar o conteúdo dos logs.
- `NetArchTest.Rules` para as regras de camadas por namespace (REQ-QUALIDADE-003). O isolamento entre serviços (REQ-QUALIDADE-004) é verificado pelas referências do assembly e pelos `ProjectReference` do `.csproj`.
- Testcontainers fica registrado como padrão para integração com bancos, mas não entra nesta mudança, que não tem bancos.

### D7. Contêineres (REQ-ENTREGA-001 a 003)

- Um `Dockerfile` por serviço em `src/<Servico>/`, com contexto de build na própria pasta: o mesmo que o Vercel usa com Root Directory na pasta do serviço.
- Multi-stage: `mcr.microsoft.com/dotnet/sdk:10.0` para publicar e `mcr.microsoft.com/dotnet/aspnet:10.0` para executar, com o usuário não root da imagem.
- O `Program.cs` faz bind em `http://0.0.0.0:{PORT}`, com 8080 quando `PORT` não existir (REQ-ENTREGA-002). No Vercel, o projeto define `PORT=8080`.
- `.dockerignore` exclui `bin/`, `obj/`, `.env*`, `appsettings.*.local.json` e arquivos de IDE (REQ-ENTREGA-003); testes e `openspec/` já ficam fora do contexto por estarem fora da pasta do serviço.

### D8. Integração contínua (REQ-QUALIDADE-001, 002 e 005; REQ-ENTREGA-001 e 003)

- O job `.NET` passa a executar: `dotnet format --verify-no-changes`, build Release (avisos como erros), testes, verificação de pacotes vulneráveis e, por serviço, `docker build`, execução do contêiner, `GET /health/live` em até 30 s e listagem de arquivos da imagem.
- Os nomes dos jobs continuam `OpenSpec` e `.NET`, que já são os checks obrigatórios da `main` (constitution 6.8).
- Dependabot passa a cobrir NuGet (`/src/*`, `/tests/*`) e imagens Docker (`/src/*`).

### D9. Entrega contínua (REQ-ENTREGA-004 e 005; ADR 0002)

- Novo workflow `cd.yml`, disparado por `workflow_run` quando o `CI` termina com sucesso em push na `main`. Assim, o deploy nunca roda sem as verificações aprovadas.
- Um job por serviço: instala a Vercel CLI em versão fixa e executa `vercel deploy --prod` com `VERCEL_TOKEN`, `VERCEL_ORG_ID` e o ID do projeto do serviço, usando o GitHub Environment `producao`.
- Depois do deploy, o job consulta `GET /health/live` na URL de produção, com novas tentativas até 60 s; sem HTTP 200, o job falha (REQ-ENTREGA-005).
- O deploy automático da integração Git do Vercel fica desligado em cada projeto (`vercel.json`), para que PRs e branches não publiquem nada.
- **Alternativa:** integração Git nativa do Vercel. Descartada porque publicaria sem esperar o CI e geraria previews, que o responsável não quer (E04).

### D10. Configuração no Vercel (conduzida com o responsável, E05)

- Dois projetos: `agente-ecf-agente` (Root Directory `src/Api.Agente`) e `agente-ecf-rag` (Root Directory `src/Api.RAG`), com o preset de contêiner.
- Variáveis de ambiente de produção: `PORT=8080` e `ASPNETCORE_ENVIRONMENT=Production`.
- Segredos no GitHub (`VERCEL_TOKEN`, `VERCEL_ORG_ID`, `VERCEL_PROJECT_ID_AGENTE`, `VERCEL_PROJECT_ID_RAG`) e URLs como variáveis do repositório. O responsável cadastra o token: o assistente não manipula credenciais.

## Risks / Trade-offs

- [Runtime de contêiner do Vercel em beta] → verificação de saúde após cada deploy; ADR 0002 lista a alternativa (Azure Container Apps) e prevê revisão se houver instabilidade.
- [Limites de duração, streaming e WebSocket não documentados para contêineres] → o servidor MCP (Streamable HTTP) e o A2A dependem de streaming; antes das mudanças `recuperacao-mcp` e `servidor-a2a`, uma prova de conceito confirma o suporte ou leva a um host persistente, registrado em novo ADR.
- [Instâncias desligadas após 5 min ociosas em produção] → a primeira chamada sofre partida a frio; o tempo será medido na verificação pós-deploy e considerado nos timeouts da Sofia INFIS.
- [Logs do Vercel ligados às requisições em andamento] → o identificador de correlação em cada registro permite separar requisições simultâneas.
- [Camadas isoladas só por teste, não pelo compilador] → testes de arquitetura dentro do check obrigatório `.NET`.
- [Middleware de correlação duplicado nos dois serviços] → código pequeno, coberto pelos mesmos testes nos dois serviços; reavaliar se crescer.
- [Token do Vercel em segredo do GitHub] → GitHub Environment `producao` e rotação documentada em `docs/ci-cd.md` (SQ-03).

## Migration Plan

Não há sistema anterior. Ordem de entrada: estrutura e testes → contêineres no CI → projetos no Vercel com o responsável → primeiro deploy pelo `cd.yml`. Rollback: promover a publicação anterior no painel do Vercel ou com `vercel rollback`, procedimento descrito em `docs/ci-cd.md`.

## Open Questions

- A chave exata do `vercel.json` para desligar o deploy pela integração Git será confirmada na documentação vigente durante a implementação; a decisão (desligar) não muda.
- Conta ou time do Vercel e as URLs de produção são definidos na configuração conjunta (E05) e não alteram tarefas.

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Não se aplica | Sem regras de domínio fiscal. |
| P02 | Não se aplica | Sem validações de documentos nesta mudança. |
| P03 | Não se aplica | O fluxo de análise começa nas mudanças de recepção e validação. |
| P04 | Não se aplica | Sem resultados de análise. |
| P05 | Não se aplica | Sem declaração de conformidade. |
| P06 | Não se aplica | Sem achados. |
| P07 | Não se aplica | Sem LLM. |
| P08 | Não se aplica | Sem análise. |
| P09 | Não se aplica | Sem análise. |
| P10 | Atende | `X-Correlation-Id` tratado como entrada não confiável: validado antes de uso e nunca registrado quando inválido (D5). |
| P11 | Não se aplica | Sem base de conhecimento. |
| P12 | Não se aplica | Sem saída de LLM. |
| P13 | Atende | O identificador de correlação é gerado ou validado pelo sistema (D5). |
| P14 | Não se aplica | Sem ferramentas. |
| P15 | Atende | REQ-QUALIDADE-004 verificado por teste; projetos e Dockerfiles independentes (D1, D6, D7). |
| P16 | Atende | Camadas verificadas por teste (D6); dependências novas justificadas: xUnit, Mvc.Testing e NetArchTest.Rules (ADR 0001); nenhuma abstração sem uso. |
| P17 | Não se aplica | Sem chamadas a provedores de IA. |
| P18 | Não se aplica | Sem memória. |
| P19 | Atende parcialmente | Versão do serviço e correlação em cada log (D5); o registro completo da execução entra com o fluxo de análise. |
| P20 | Atende | Esta mudança; tarefas ligadas aos requisitos em `tasks.md`. |
