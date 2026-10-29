# 0002 — Hospedagem no Vercel e pipeline de CI/CD

| Campo | Valor |
| --- | --- |
| Estado | Aceito (Kaio Macedo, 2026-10-29) |
| Data | 2026-10-29 |
| Responsáveis | Kaio Macedo (decisão), assistente de IA (redação) |
| Decisões pendentes resolvidas | DP-18 |
| Princípios relacionados | P15, RA-01, RA-02, RA-09, SQ-03, SQ-06, SQ-07, SQ-08, SQ-11 |
| Substitui / Substituído por | — |

## Contexto

DP-18 (hospedagem, ambientes, contêineres e CI/CD) estava pendente. Em 2026-10-29, o responsável escolheu o Vercel com contêineres, como no projeto AgenteAnaliseDIMP, com deploy somente em produção a cada merge na `main` e configuração da conta feita em conjunto. O CI no GitHub Actions já está ativo.

Fatos da plataforma, segundo o [guia do Vercel sobre contêineres](https://vercel.com/kb/guide/does-vercel-support-docker-deployments) (atualizado em 2026-10-29) e o [anúncio de 2026-10-27](https://vercel.com/changelog/deploy-a-container-from-a-plain-dockerfile):

- o runtime de contêiner está em **beta** em todos os planos e roda imagens OCI como Vercel Functions, com escala automática;
- memória e disco locais não são duráveis; instâncias ociosas desligam após 5 minutos em produção e recebem `SIGTERM` com 30 segundos para encerrar;
- o tráfego vai para a porta 80, salvo se o projeto definir `PORT`;
- duração máxima, streaming e WebSocket não estão descritos para contêineres.

## Decisão

1. **Hospedagem:** Api.Agente e Api.RAG no Vercel, em dois projetos com o preset de contêiner e Root Directory na pasta de cada serviço. O projeto define `PORT=8080`.
2. **Ambientes:** apenas produção. A integração Git do Vercel não publica; PRs e branches não geram deploy.
3. **CI:** GitHub Actions, workflow `CI`, com os checks obrigatórios `OpenSpec` e `.NET` (constitution 6.8). O job `.NET` inclui a construção e o teste das imagens de contêiner.
4. **CD:** workflow `cd.yml`, disparado após o `CI` concluir com sucesso em push na `main`. Publica cada serviço com a Vercel CLI em versão fixa (`vercel deploy --prod`) e verifica `GET /health/live` em produção, falhando se não houver HTTP 200 em 60 segundos.
5. **Segredos:** `VERCEL_TOKEN`, `VERCEL_ORG_ID` e os IDs dos projetos ficam em segredos do GitHub Environment `producao`, cadastrados pelo responsável. Segredos da aplicação ficam nas variáveis de ambiente do Vercel.
6. **MCP e A2A:** antes das mudanças `recuperacao-mcp` e `servidor-a2a`, uma prova de conceito confirma se o Vercel suporta o transporte com streaming exigido. Se não suportar, um novo ADR define host persistente para o serviço afetado.

## Alternativas consideradas

| Alternativa | Vantagens | Desvantagens | Motivo da rejeição |
| --- | --- | --- | --- |
| Integração Git nativa do Vercel | Sem workflow de CD nem token no GitHub. | Publica sem esperar o CI; gera previews; verificação de saúde separada. | Contraria E04 e a exigência de deploy só após verificações aprovadas. |
| Azure Container Apps | Contêineres persistentes, streaming e WebSocket documentados. | Outra plataforma para a equipe; custo e configuração próprios. | O responsável escolheu o Vercel; fica como alternativa se o beta ou o streaming não atenderem. |
| Um projeto Vercel com vários serviços (`services` no `vercel.json`) | Um único projeto. | Roteamento e build compartilhados entre serviços independentes. | Projetos separados preservam a independência dos serviços (P15). |

## Consequências

- O deploy fica auditável no GitHub, depois do CI, com verificação de saúde em produção.
- Não há ambiente de preview; a validação antes da produção depende do CI e dos testes.
- A dependência de um runtime em beta exige acompanhamento; a verificação pós-deploy e esta revisão prevista mitigam o risco.
- O responsável precisa criar os projetos, o token e os segredos, com o passo a passo de `docs/ci-cd.md`.

## Verificação

- Um PR não cria nenhuma publicação no Vercel.
- Um merge na `main` gera uma execução do `cd.yml` com deploy dos dois serviços e verificação de saúde aprovada.
- O ADR é revisado antes das mudanças `recuperacao-mcp` e `servidor-a2a`, ou se o runtime de contêiner sair do beta com mudanças relevantes.
