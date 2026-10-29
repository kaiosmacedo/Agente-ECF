# CI/CD do Agente-ECF

Pipeline de integração e entrega, conforme a [constitution](../openspec/constitution.md) (SQ-08, SQ-09, SQ-11, seções 6.6 e 6.8) e o [ADR 0002](adr/0002-hospedagem-vercel-e-ci-cd.md).

## Situação

| Parte | Estado | Onde |
| --- | --- | --- |
| CI de especificação | Ativo | Job `OpenSpec` em `.github/workflows/ci.yml` |
| CI .NET: formatação, build com avisos como erros, testes, pacotes vulneráveis e contêineres | Ativo | Job `.NET` no mesmo workflow |
| CD em produção no Vercel | Ativo desde 2026-10-29 | `.github/workflows/cd.yml` |
| Atualização de dependências | Ativo para GitHub Actions, NuGet e imagens Docker | `.github/dependabot.yml` |

## Integração contínua

O workflow `CI` roda em todo PR para a `main`, em push na `main` e sob demanda. Os jobs `OpenSpec` e `.NET` são **checks obrigatórios** da `main`: nenhum merge acontece com check falho ou pendente, inclusive nos merges feitos pelo assistente por delegação (seção 6.6).

| Job | Etapas | Requisitos |
| --- | --- | --- |
| `OpenSpec` | Presença da constitution, leitura do `openspec/config.yaml` e `openspec validate --all --strict`. | REQ-QUALIDADE-001 |
| `.NET` | Restaurar; `dotnet format --verify-no-changes`; build Release com avisos como erros e o SHA na versão; testes (Microsoft Testing Platform); pacotes vulneráveis; para cada serviço, `docker build`, vivacidade em até 30 s na porta 8080 e com `PORT=3000`, logs em JSON com correlação e listagem dos arquivos da imagem. | REQ-QUALIDADE-001, 002, 003, 004 e 005; REQ-ENTREGA-001, 002 e 003; REQ-OBSERV-002 |

- As actions são fixadas por SHA; o Dependabot propõe as atualizações.
- O CI **não chama a OpenAI** (SQ-09).
- O teste dos contêineres está em `.github/scripts/testar-conteineres.sh`.
- Versão do OpenSpec no CI: `1.15.0`, igual à usada localmente. Atualizar as duas juntas.

## Entrega contínua

O workflow `CD` dispara quando o `CI` termina com sucesso em push na `main` (ou seja, depois de um merge). Para cada serviço, ele:

1. instala a Vercel CLI `65.7.0`;
2. publica em produção com `vercel deploy --prod`, usando o token e o ID do projeto guardados no GitHub Environment `producao`;
3. consulta `GET /health/live` na URL de produção por até 60 s e falha se não receber HTTP 200, registrando o tempo até a resposta.

PRs e outras branches não publicam: o CD só reage ao CI de push na `main`, e o `vercel.json` de cada serviço desliga o deploy pela integração Git do Vercel (`"git": { "deploymentEnabled": false }`).

## Configuração do Vercel

Configuração feita em 2026-10-29 no time **`agent-ecf`** (plano Hobby), com a conta de Kaio Macedo autenticada na Vercel CLI. O assistente executou os passos sem credenciais; o token é criado e cadastrado somente pelo responsável.

| Item | Estado |
| --- | --- |
| Projetos `agente-ecf-agente` e `agente-ecf-rag` | Criados pela CLI (`vercel project create`). |
| Preset e Root Directory | `Container`; `src/Api.Agente` e `src/Api.RAG` (`vercel project update --framework container --root-directory ...`). |
| Variáveis de Production | `PORT=8080` e `ASPNETCORE_ENVIRONMENT=Production`, tipo Config (`vercel env add ... --no-sensitive`). |
| Integração Git | Não conectada: o deploy é feito pelo `cd.yml`. Se for conectada, o `vercel.json` impede deploys automáticos. |
| Segredos `VERCEL_ORG_ID`, `VERCEL_PROJECT_ID_AGENTE`, `VERCEL_PROJECT_ID_RAG` | Cadastrados no Environment `producao` (identificadores, não credenciais). |
| Variáveis `VERCEL_URL_AGENTE`, `VERCEL_URL_RAG` | `https://agente-ecf-agente.vercel.app` e `https://agente-ecf-rag.vercel.app`, confirmadas no primeiro deploy (2026-10-29). |
| Segredo `VERCEL_TOKEN` | Cadastrado por Kaio Macedo em 2026-10-29 (passos abaixo, para rotação ou recriação). |

**Token (responsável):**

1. Em https://vercel.com/account/settings/tokens, criar o token `agente-ecf-github-cd`, com escopo no time `agent-ecf` e validade definida.
2. Em https://github.com/kaiosmacedo/Agente-ECF/settings/environments, abrir `producao` → **Add environment secret** → nome `VERCEL_TOKEN` → colar o token. Alternativa no terminal, que pede o valor sem exibi-lo:

   ```bash
   gh secret set VERCEL_TOKEN --env producao --repo kaiosmacedo/Agente-ECF
   ```

3. Conferir: `gh secret list --env producao --repo kaiosmacedo/Agente-ECF` mostra os quatro nomes, sem os valores.

Se o primeiro deploy atribuir outro domínio de produção, atualizar a variável correspondente com `gh variable set`.

## Rotação do token

1. Criar um token novo no Vercel.
2. Atualizar o segredo com `gh secret set VERCEL_TOKEN --env producao --repo kaiosmacedo/Agente-ECF`.
3. Disparar o `CI` na `main` (Actions → CI → Run workflow) e confirmar o `CD` verde.
4. Revogar o token antigo no Vercel.

## Rollback

No painel do Vercel, abrir o projeto afetado → Deployments → escolher a publicação anterior saudável → **Promote to Production**. Pela CLI, com o projeto vinculado localmente (`vercel link`), `vercel rollback` faz o mesmo. Depois, corrigir a causa num PR; o próximo merge publica a correção.

## Rodar localmente

```bash
openspec validate --all --strict
```

```bash
dotnet format Agente-ECF.slnx --verify-no-changes
```

```bash
dotnet build Agente-ECF.slnx -c Release
```

```bash
dotnet test --solution Agente-ECF.slnx -c Release
```

```bash
bash .github/scripts/testar-conteineres.sh
```

O último comando exige o Docker em execução.
