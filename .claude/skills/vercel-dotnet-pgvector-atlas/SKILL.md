---
name: vercel-dotnet-pgvector-atlas
description: Preparar, validar e implantar a Api.Agente e a Api.RAG (.NET 10, ASP.NET Core) no Vercel com contêineres Docker, PostgreSQL gerenciado com PgVector e MongoDB Atlas. Usar somente depois que os ADRs de hospedagem (DP-18) e de memória (DP-01) tiverem escolhido essas plataformas, em mudanças OpenSpec que peçam deploy, ambientes, migrações, segredos, diagnóstico de conectividade ou testes pós-implantação.
---

# Deploy do Agente-ECF: .NET 10 + PgVector + MongoDB Atlas no Vercel

Adaptada da skill usada no projeto AgenteAnaliseDIMP. As regras vêm de `openspec/constitution.md`; em conflito, a constitution prevalece.

## 0. Condição de uso

Esta skill **não decide** a hospedagem. Antes de qualquer passo, confirmar em `docs/adr/`:

- ADR aceito para DP-18 escolhendo Vercel com contêiner;
- ADR aceito para DP-01 escolhendo MongoDB Atlas (se a mudança envolver memória);
- ADR aceito para DP-09 (autenticação), pois nenhum endpoint funcional pode ser exposto sem ele.

Sem esses ADRs, parar e informar o usuário. Não criar Dockerfile nem configuração de deploy por antecipação.

## Contrato

- **Entrada:** solução .NET 10, mudança OpenSpec com `design.md` e `tasks.md` aprovados, projeto de cada serviço, ambientes, provedor PostgreSQL com `vector`, cluster Atlas e autorização de deploy.
- **Saída:** arquivos de infraestrutura coerentes com o design, evidências de build e teste, URL de deploy e verificações HTTP e de banco executadas. Nunca declarar deploy concluído sem prova.
- **Referência técnica a reconfirmar na documentação vigente:** o Vercel executa contêineres como funções com instâncias sem estado (`Dockerfile.vercel` ou `Dockerfile` raiz). Não é hospedagem persistente de banco nem de processos contínuos.

## Arquitetura de referência

```
Sofia INFIS / cliente HTTP
   |-- REST/A2A --> Api.Agente (contêiner) --TLS--> PostgreSQL da Api.Agente (consumo e custos)
   |                    |                  --TLS--> MongoDB Atlas (memória, se DP-01 = MongoDB)
   |                    |                  --TLS--> OpenAI
   |                    `-- MCP ---------> Api.RAG (contêiner ou host persistente)
   |                                            `--TLS--> PostgreSQL + PgVector da Api.RAG
   `-- REST admin --> Api.RAG
```

- **P15:** Api.Agente e Api.RAG usam bancos ou esquemas distintos, com credenciais distintas e sem permissões cruzadas. A Api.Agente nunca recebe a connection string da Api.RAG.
- **MCP:** o servidor MCP da Api.RAG (Streamable HTTP) e o servidor A2A podem exigir sessão, conexões longas ou streaming. Verificar os limites de duração, streaming e concorrência do plano Vercel; se não atenderem, registrar no ADR um host persistente para esse serviço.
- **Arquivos ECF:** o disco do contêiner não é armazenamento durável. O armazenamento dos arquivos recebidos segue GD-06 e DP-11.

## 1. Pré-voo SDD

1. Ler `openspec/constitution.md`, os ADRs aceitos e a mudança em `openspec/changes/<id>/` (`proposal.md`, `specs/`, `design.md`, `tasks.md`).
2. Identificar o `.csproj` de cada serviço, `net10.0`, porta HTTP, health check e dependências (Npgsql, Pgvector, MongoDB.Driver, Semantic Kernel, SDK MCP).
3. Validar no provedor PostgreSQL `CREATE EXTENSION IF NOT EXISTS vector;` com as permissões do usuário da aplicação. Nem todo PostgreSQL gerenciado oferece a extensão.
4. Confirmar que URLs de MCP, A2A e OpenAI não dependem de hostnames de Docker Compose.
5. Confirmar autenticação, quotas, regiões, restrições de IP e políticas de rede do PostgreSQL e do Atlas. Não liberar acesso universal sem análise e controles compensatórios registrados.
6. Confirmar DP-11 e DP-12 antes de qualquer ambiente com dados reais.

## 2. Dockerfile.vercel (.NET 10)

Um Dockerfile por serviço implantado. Adaptar o caminho do projeto à solução real (DP-17):

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/<Servico>/<Servico>.csproj"
RUN dotnet publish "src/<Servico>/<Servico>.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "<Servico>.dll"]
```

- **Porta:** conferir na documentação vigente como o Vercel encaminha o tráfego. Se a plataforma informar `PORT`, a aplicação faz bind em `0.0.0.0` nessa porta. `EXPOSE` sozinho não configura roteamento.
- **`.dockerignore`:** `.git`, `.env`, `.env.*`, `**/bin`, `**/obj`, `.vs`, `.vercel`, `node_modules`, `openspec/`, `docs/` e arquivos de segredos.

## 3. Configuração e segredos

Nomes no padrão do `IConfiguration` (ajustar ao que o design definir):

| Variável | Serviço | Observação |
| --- | --- | --- |
| `ConnectionStrings__PgVector` | Api.RAG | TLS obrigatório (`SSL Mode=Require` ou validação mais forte). |
| `ConnectionStrings__Consumo` | Api.Agente | Banco ou esquema próprio da Api.Agente (P17). |
| `ConnectionStrings__MongoDb` e `MongoDb__DatabaseName` | Api.Agente | Usuário de menor privilégio; database por ambiente. |
| `OpenAI__ApiKey` | Api.Agente e Api.RAG | Somente no painel de segredos do ambiente. |
| `Mcp__RagEndpoint` | Api.Agente | URL do servidor MCP da Api.RAG. |

- Segredos ficam em **Vercel Project Settings > Environment Variables** ou via CLI autenticada; nunca no repositório, em logs ou em prompts (SQ-03).
- `vercel env ls` confere presença, não valores. Preview e Production são separados.
- A configuração é validada na inicialização; configuração obrigatória ausente impede o serviço de subir (RA-09).

## 4. Banco e migrações

- Migrações aplicadas por pipeline ou tarefa administrativa antes do tráfego, nunca a cada cold start.
- Dimensão do vetor, tipo de índice (HNSW ou IVFFlat) e operador de distância seguem o ADR de embeddings (DP-02) e a avaliação de recuperação (GD-08).
- Restrições de banco garantem os metadados obrigatórios de documentos e chunks (GD-07) e o indicador de consumo desconhecido (P17).
- Atlas: índices por `organizacaoId`, `usuarioId`, `conversaId` e `createdAt`, com TTL conforme a política de retenção (P18, DP-11).
- Um único `MongoClient` e pool de conexões por processo; timeouts e retentativas limitadas (SQ-06).

## 5. Validação local

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
openspec validate --strict
docker build -f Dockerfile.vercel -t <servico>:local .
docker run --rm -p 8080:8080 --env-file .env.production.local <servico>:local
```

`.env.production.local` nunca é commitado. Usar credenciais de desenvolvimento isoladas e dados sintéticos (SQ-10). Testar `GET /health` sem expor segredos e os endpoints funcionais com autenticação.

## 6. Publicação no Vercel

```powershell
npm.cmd install -g vercel
vercel login
vercel link
vercel env ls
vercel deploy
# Promover somente depois dos testes no preview:
vercel deploy --prod
```

- Conferir o `Root Directory`, a detecção do Dockerfile, as permissões e o suporte a contêineres no plano.
- Não inventar `rewrites`, `services` ou portas sem checar a configuração real.
- Com o repositório conectado ao Vercel, pushes podem disparar deploy automaticamente; evitar deploys duplicados e deploy a partir de branches sem PR aprovado.

## 7. Aceite pós-deploy

- Health check de cada serviço e autorização dos endpoints (SQ-01, SQ-02).
- Consulta vetorial real filtrada por tipo e versão de manual (P11); chamada MCP da Api.Agente à Api.RAG (P15); gravação e leitura de memória isolada por usuário (P18), se previstas na mudança.
- Correlação propagada e logs sem dados sensíveis (SQ-04, SQ-07); consumo registrado nas chamadas à OpenAI (P17).
- Latência, timeouts, TLS e cold start verificados; rollback preparado.
- Relatar URL, commit, ambiente, testes executados e limitações. Nunca afirmar sucesso sem evidência.

## Fontes para checagem antes de implementar

- https://vercel.com/docs/functions/runtimes
- https://vercel.com/kb/guide/does-vercel-support-docker-deployments
- https://vercel.com/changelog/deploy-a-container-from-a-plain-dockerfile
- https://www.mongodb.com/docs/atlas/
- https://github.com/pgvector/pgvector
