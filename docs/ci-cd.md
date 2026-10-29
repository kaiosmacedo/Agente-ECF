# CI/CD do Agente-ECF

Pipeline de integração e entrega, conforme a [constitution](../openspec/constitution.md) (SQ-08, SQ-09, SQ-11, seções 6.6 e 6.8).

## Situação

| Parte | Estado | Onde |
| --- | --- | --- |
| CI de especificação | Ativo | Job `OpenSpec` em `.github/workflows/ci.yml` |
| CI .NET (build, testes, pacotes vulneráveis) | Ativo, sem efeito até existir a solução | Job `.NET` no mesmo workflow |
| Analisadores, cobertura e testes de arquitetura | Planejado | Mudança `fundacao-solucao` |
| CD (deploy) | Planejado: Vercel com contêineres | ADR de hospedagem na mudança `fundacao-solucao` (DP-18) |
| Atualização de dependências | Ativo para GitHub Actions | `.github/dependabot.yml` |

## Integração contínua

O workflow `CI` roda em todo PR para a `main`, em push na `main` e sob demanda.

| Job | O que verifica | Quando bloqueia |
| --- | --- | --- |
| `OpenSpec` | Presença da constitution, leitura do `openspec/config.yaml` e `openspec validate --all --strict` sobre specs e mudanças. | Qualquer erro de validação. |
| `.NET` | Localiza a solução (`*.slnx` ou `*.sln`), restaura, compila em Release, executa os testes e procura pacotes vulneráveis. Sem solução, registra um aviso e passa. | Falha de build, teste ou pacote vulnerável. |

- Os dois jobs são **checks obrigatórios** da `main`. Nenhum merge acontece com check falho ou pendente, inclusive nos merges feitos pelo assistente por delegação (seção 6.6).
- As actions são fixadas por SHA, e o Dependabot propõe as atualizações.
- O CI **não chama a OpenAI** (SQ-09). Testes com modelo real são avaliações separadas.
- Versão do OpenSpec no CI: `1.15.0`, igual à usada localmente. Atualizar as duas juntas.

## Entrega contínua (planejada)

Decisão do responsável em 2026-10-29: hospedar a Api.Agente e a Api.RAG no **Vercel com contêineres**, como no projeto AgenteAnaliseDIMP. O ADR de hospedagem da mudança `fundacao-solucao` vai detalhar:

- ambientes (preview e produção) e quando cada um recebe deploy;
- viabilidade do servidor MCP da Api.RAG e do servidor A2A no modelo de funções sem estado do Vercel, ou a necessidade de host persistente;
- provedores de PostgreSQL com PgVector e de MongoDB Atlas, quando as mudanças de RAG e memória chegarem;
- onde ficam os segredos (Vercel e GitHub) e como são rotacionados (SQ-03).

O procedimento de deploy segue a skill `vercel-dotnet-pgvector-atlas`, que só se aplica depois desse ADR aceito.

## Rodar localmente

```bash
openspec validate --all --strict
```

```bash
dotnet build -c Release
```

```bash
dotnet test -c Release
```
