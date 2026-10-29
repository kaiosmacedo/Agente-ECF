# Tasks

> **Estado:** aprovadas por Kaio Macedo em 2026-10-29; implementação em andamento (`/opsx:apply`). Tarefas marcadas **(com Kaio Macedo)** exigem a conta do responsável no Vercel ou no GitHub; o assistente conduz o passo a passo.

## 1. Estrutura da solução

- [x] 1.1 Criar `global.json` (SDK 10.0, `rollForward: latestFeature`), `Directory.Build.props` e `.editorconfig` conforme D2 (REQ-QUALIDADE-002); verificar que `dotnet --version` na raiz resolve um SDK 10.0.
- [x] 1.2 Criar `Agente-ECF.slnx` e os projetos ASP.NET Core `src/Api.Agente` e `src/Api.RAG` com as pastas `Endpoints`, `Aplicacao`, `Dominio` e `Infraestrutura` (D1); verificar `dotnet build -c Release` sem avisos.
- [x] 1.3 Criar `tests/Api.Agente.Tests` e `tests/Api.RAG.Tests` com xUnit, `Microsoft.AspNetCore.Mvc.Testing` e `NetArchTest.Rules` (D6, ADR 0001); verificar `dotnet test` verde com um teste inicial em cada projeto.
- [x] 1.4 Verificar `dotnet format --verify-no-changes` sem divergências na solução (REQ-QUALIDADE-002).

## 2. Configuração

- [x] 2.1 Registrar nos dois serviços as opções `Observabilidade` (`NivelMinimo`, padrão `Information`) com validação na inicialização (D4); verificar com teste que `Observabilidade__NivelMinimo=Warning` prevalece sobre o arquivo (REQ-CONFIG-001).
- [x] 2.2 Escrever testes com a opção obrigatória de teste `Exemplo:Endereco` ausente e com valor `segredo-nao-url`; verificar que a inicialização falha citando o nome e que nem o erro nem os logs contêm o valor (REQ-CONFIG-002).
- [x] 2.3 Escrever em cada serviço o teste que varre os `appsettings*.json` versionados; verificar que falha com uma chave de segredo preenchida numa fixture e passa com os arquivos reais (REQ-CONFIG-001).

## 3. Observabilidade

- [x] 3.1 Implementar o middleware de correlação nos dois serviços (D5); verificar com testes os cenários de identificador válido (`sofia-123.abc`), ausente e inválido (129 caracteres e quebra de linha) (REQ-OBSERV-001).
- [x] 3.2 Configurar o logger JSON com escopo `CorrelationId`, `Servico` e `Versao`, com a versão vinda do assembly; verificar com provedor de log em memória que cada registro da requisição `teste-log-1` é JSON válido com os três campos (REQ-OBSERV-002).
- [x] 3.3 Verificar com testes que `Authorization: Bearer token-secreto-123` e o valor inválido de correlação não aparecem em nenhum registro de log (REQ-OBSERV-003, REQ-OBSERV-001).

## 4. Saúde

- [x] 4.1 Mapear `/health/live` e `/health/ready` com o escritor de resposta mínimo (D3) nos dois serviços; verificar com testes HTTP 200 e `Healthy` em ambos sem dependências registradas (REQ-SAUDE-001, REQ-SAUDE-002).
- [x] 4.2 Escrever o teste com verificação `ready` falsa que lança "senha inválida para host db-interno"; verificar HTTP 503, `Unhealthy` e ausência dos textos "senha inválida" e "db-interno" no corpo (REQ-SAUDE-002, REQ-SAUDE-003).
- [x] 4.3 Verificar com teste que a resposta de `/health/live` contém somente o campo de estado (REQ-SAUDE-003).

## 5. Regras de arquitetura

- [x] 5.1 Escrever em cada serviço os testes `NetArchTest.Rules` das regras de camadas (D6); verificar que passam e, com uma referência proibida introduzida temporariamente do Dominio para a Infraestrutura, que falham citando o tipo (evidência registrada no PR) (REQ-QUALIDADE-003).
- [x] 5.2 Escrever o teste de isolamento entre serviços (referências do assembly e `ProjectReference` dos `.csproj`); verificar que passa e que falha com uma referência cruzada temporária (REQ-QUALIDADE-004).

## 6. Contêineres

- [x] 6.1 Fazer o `Program.cs` dos dois serviços escutar em `0.0.0.0` na porta de `PORT`, com 8080 como padrão (D7); verificar com teste a resolução da URL com e sem `PORT` (REQ-ENTREGA-002).
- [ ] 6.2 Criar `Dockerfile` e `.dockerignore` em `src/Api.Agente` e `src/Api.RAG` (D7); verificar localmente `docker build` e `GET /health/live` com `PORT=3000` e sem `PORT` (REQ-ENTREGA-001, REQ-ENTREGA-002).
- [ ] 6.3 Verificar pela listagem de arquivos de cada imagem que não há `.env`, `appsettings.*.local.json`, `.git`, `tests` nem `openspec` (REQ-ENTREGA-003).

## 7. Pipeline de integração

- [ ] 7.1 Estender o job `.NET` do `ci.yml` com formatação, build com o SHA na versão, testes, pacotes vulneráveis e, por serviço, construção, execução, verificação de saúde em até 30 s e listagem da imagem (D8); verificar o job verde no PR (REQ-QUALIDADE-001, REQ-QUALIDADE-002, REQ-QUALIDADE-005, REQ-ENTREGA-001, REQ-ENTREGA-003).
- [ ] 7.2 Incluir NuGet (`/src/*`, `/tests/*`) e Docker (`/src/*`) no `dependabot.yml`; verificar após o merge que o GitHub aceita a configuração, sem erro na aba Dependabot (REQ-QUALIDADE-005).
- [x] 7.3 Verificar com `gh api` que a proteção da `main` exige os checks `OpenSpec` e `.NET` e bloqueia merge com check falho ou pendente (REQ-QUALIDADE-001).
- [ ] 7.4 Atualizar a seção de integração contínua de `docs/ci-cd.md` com as novas etapas; verificar que os comandos locais documentados executam como escrito.

## 8. Entrega no Vercel

- [ ] 8.1 Criar `vercel.json` em cada serviço com o preset de contêiner e o deploy pela integração Git desligado, com a chave confirmada na documentação vigente (D9, D10); verificar que um PR não gera publicação no Vercel (REQ-ENTREGA-004).
- [ ] 8.2 Criar `.github/workflows/cd.yml` disparado pelo sucesso do `CI` em push na `main`, com Vercel CLI em versão fixa, Environment `producao`, deploy por serviço e verificação de `/health/live` com novas tentativas até 60 s (D9); verificar que o GitHub aceita o workflow e que ele não dispara em PRs (REQ-ENTREGA-004, REQ-ENTREGA-005).
- [ ] 8.3 **(com Kaio Macedo)** Criar no Vercel os projetos `agente-ecf-agente` e `agente-ecf-rag`, com Root Directory na pasta de cada serviço, preset de contêiner, `PORT=8080` e `ASPNETCORE_ENVIRONMENT=Production` (D10); verificar as configurações no painel do Vercel.
- [ ] 8.4 **(com Kaio Macedo)** Criar o token do Vercel e cadastrar `VERCEL_TOKEN`, `VERCEL_ORG_ID`, `VERCEL_PROJECT_ID_AGENTE` e `VERCEL_PROJECT_ID_RAG` no Environment `producao`, e as URLs de produção como variáveis do repositório; verificar com `gh secret list --env producao` que os nomes existem, sem ver os valores.
- [ ] 8.5 Documentar em `docs/ci-cd.md` o passo a passo do Vercel, a rotação do token e o rollback; verificar que o procedimento corresponde ao executado em 8.3 e 8.4.
- [ ] 8.6 Após o merge desta mudança, verificar a execução do `cd.yml`: deploy dos dois serviços e `/health/live` com HTTP 200 em produção, registrando o tempo de partida a frio no PR ou no ADR 0002 (REQ-ENTREGA-004, REQ-ENTREGA-005).

## 9. Fechamento

- [x] 9.1 Atualizar o `README.md` com a estrutura `src/` e `tests/` e os comandos de build e teste; verificar que os comandos executam como escritos.
- [x] 9.2 Marcar os ADRs 0001 e 0002 como Aceitos após a aprovação e atualizar o índice em `docs/adr/README.md`; verificar os links do índice.
- [ ] 9.3 Atualizar a nota `Projetos/Agente-ECF` do cofre Obsidian com o estado entregue (DoD-18); verificar a tabela de mudanças da nota.
- [ ] 9.4 Executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution); verificar que todos os itens aplicáveis estão atendidos ou justificados no PR.

## Workflow follow-up

- Arquivar a mudança com `/opsx:archive` após o aceite, criando as specs vigentes em `openspec/specs/`.
- Mover DP-17 e DP-18 para as decisões resolvidas da constitution, com os ADRs 0001 e 0002 aceitos.
