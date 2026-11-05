# Design: Execução da análise e contrato de resultado

> **Estado:** rascunho aguardando aprovação de Kaio Macedo, junto com as tarefas. Esclarecimentos E01 a E07 respondidos em 2026-11-05.

## Context

A mudança 13 entregou o protocolo e o contrato SOFIA v1.0, com um `ProcessadorDeSolicitacoes` que só responde `parcial` e não guarda nada. Faltam o registro da execução com as nove etapas (P03), o cálculo determinístico do status (P05), o contrato interno de achados (P04, P06) e a persistência (DP-25). As etapas em si chegam nas mudanças 07 a 10; aqui elas ficam registradas como não executadas, e os testes criam execuções diretamente. Não há autenticação nem organização no contrato (SQ-12).

## Goals / Non-Goals

**Goals**

- Modelo de execução, etapas, achados, status e revisão humana no domínio, sem dependências de infraestrutura.
- Status e revisão humana calculados só pelo sistema, com uma tabela de regras testável.
- Execuções persistidas em PostgreSQL gerenciado (Neon), sem guardar o texto dos documentos nem o nome do usuário.
- `GET /analises/{id}` com o envelope e `ResultadoAnaliseSofia` como projeção do mesmo resultado.
- Retentativa com o mesmo `idSolicitacao` devolve o resultado já registrado.

**Non-Goals**

- Recepção de arquivo, validações e LLM (mudanças 07 a 10).
- Relatório legível, autenticação e isolamento por organização verificada.
- Política de retenção (DP-11): só dados sintéticos até lá.

## Decisions

### D1. Domínio (REQ-EXEC-001 a 004; REQ-RESULT-002 a 005)

Em `Dominio/Execucoes`, sem dependência de ASP.NET Core, banco ou A2A:

- `Execucao`: identificador próprio, `idExecucao` e `idSolicitacao` da Sofia, usuário pseudonimizado, organização, data, nove `EtapaDaExecucao`, achados, limitações, hashes dos documentos e versões.
- `Etapa`: as nove etapas de P03, com as dependências entre elas. Estados: `Concluida`, `NaoAplicavel` (com justificativa), `ComFalha` e `NaoExecutada`.
- `Achado`: criado por um método que rejeita combinação inválida, com categoria (GD-04), origem, fundamento, registros e severidade. Categoria incompatível com a origem, fundamento ausente ou severidade fora da escala lançam exceção de domínio.

### D2. Status e revisão humana (REQ-EXEC-004, REQ-RESULT-005; E01)

Regra aplicada nesta ordem:

| Condição | Status interno | Contrato SOFIA |
| --- | --- | --- |
| Etapa 2 com falha | `entrada-rejeitada` | `erro` |
| Alguma etapa com falha técnica ou achado de falha técnica | `falha-tecnica` | `erro` |
| Validação obrigatória não concluída, ou limitações | `parcial` | `parcial` |
| Todas as validações obrigatórias concluídas | `concluida` | `concluido` |

`concluida` não significa "conforme": erros e inconsistências continuam nos achados. A revisão humana é positiva com falha técnica, validação obrigatória não concluída, indício da IA ou ausência de evidências, e nenhuma recomendação do modelo a dispensa (P09).

### D3. Severidade (REQ-RESULT-004; E02; DP-07)

Escala do contrato SOFIA: `baixa`, `media`, `alta` e `critica`. Cada regra determinística declara a sua; é uma escala do produto, não uma classificação da Receita Federal.

| Severidade | Critério |
| --- | --- |
| `critica` | O arquivo não pode ser analisado ou entregue como está, segundo a regra citada. |
| `alta` | Precisa de correção. |
| `media` | Uma pessoa precisa verificar. |
| `baixa` | Risco pequeno. |

### D4. Envelope e projeção (REQ-EXEC-005, REQ-RESULT-001, 006; RA-07)

- `Aplicacao/Execucoes` monta o `EnvelopeDoResultado`, versionado, com identificadores, status, etapas, achados, limitações, revisão humana, datas e tempos.
- `ResultadoAnaliseSofia` passa a ser projeção do mesmo resultado. Limitações e validações não realizadas vão para `observacoes`. Campos a mais (`categoria`, `origem`, `regra` no achado; `revisaoHumana` e `idExecucaoAgente` no resultado) entram como opcionais, sem mudar os da v1.0.
- `GET /analises/{id}` devolve o envelope, ou 404 se o identificador não existir. Sem credencial nem restrição por organização (SQ-12).
- O contrato fica em `docs/contratos/envelope-resultado.md` (GD-01).

### D5. Identidade na primeira versão (E07; SQ-12)

Um único ponto na aplicação, `IdentidadeDaRequisicao`:

- **Usuário:** `usuario.nome` do contrato. Só o SHA-256 do nome é persistido, como pseudônimo, nunca o nome; ele também fica fora dos logs (SQ-04).
- **Organização:** valor padrão de `Identidade:OrganizacaoPadrao`.
- Os dois são marcados como declarados, não verificados.

### D6. Persistência (DP-25; E06)

- **PostgreSQL no Neon**, com banco próprio da Api.Agente (RA-05, P15). Provedor escolhido por Kaio Macedo em 2026-11-05.
- **EF Core com Npgsql:** `Microsoft.EntityFrameworkCore` e `Npgsql.EntityFrameworkCore.PostgreSQL`. Também atende consumo (04) e dá migrações versionadas. **Alternativa:** Npgsql com SQL próprio, com menos dependências, mas migrações manuais em cada mudança.
- **Modelo:** tabela `execucoes` com colunas para consulta e índice único em `id_solicitacao`, e o conteúdo de etapas, achados e limitações em `jsonb`.
- **Nunca guardados:** texto dos documentos, nome do usuário e pergunta. Só hash e tamanho (GD-06). Isso reduz o peso de DP-11, que continua pendente.
- **Migrações:** aplicadas pelo CD antes do deploy, com um bundle do EF, nunca na inicialização (skill `vercel-dotnet-pgvector-atlas`).
- **Conexão:** `ConnectionStrings__Execucoes`, obrigatória e validada, com TLS. O segredo é cadastrado por Kaio Macedo no Vercel e no GitHub Environment `producao`.
- **Saúde:** `/health/ready` passa a verificar o banco, sem expor detalhes (REQ-SAUDE).

### D7. Retentativas (RNF02 da skill)

`idSolicitacao` tem índice único. Uma retentativa devolve o resultado já registrado, sem criar outra execução. Duas chamadas simultâneas são resolvidas pelo índice: a que perder lê o registro da outra.

### D8. Auditoria (REQ-EXEC-006; P19)

Cada execução registra hash de cada documento, versões do contrato, dos validadores e dos componentes, e os estados das etapas. A versão dos componentes vem de `IdentidadeServico`.

### D9. Testes

- Domínio: tabela de status e revisão humana, com uma linha por condição; categoria e origem; evidência mínima; escala.
- Integração: PostgreSQL real com Testcontainers (SQ-09), para gravação, leitura, índice único e `GET /analises/{id}`. O CI do GitHub tem Docker.
- Contrato: projeção para `ResultadoAnaliseSofia` e round-trip do envelope; os testes da mudança 13 continuam passando.

## Risks / Trade-offs

- [Banco como dependência do fluxo A2A] → sem banco, a etapa 1 falha e a tarefa termina com falha técnica, sem resultado inventado.
- [Conexões no Vercel] → instâncias sobem e descem; usar o endereço com pooling do Neon e pool pequeno no Npgsql. Verificado em produção.
- [Neon como dependência externa] → provedor trocável por configuração, por ser PostgreSQL padrão.
- [Pseudônimo fraco] → SHA-256 sem sal de um nome é reversível por tentativa. Aceitável só com dados sintéticos; revisar em DP-11, antes de dados reais.
- [Endpoint público sem autenticação] → qualquer pessoa com o identificador lê o envelope. Só dados sintéticos (SQ-12), e o identificador é aleatório.

## Migration Plan

1. Com Kaio Macedo: criar o banco no Neon e cadastrar os segredos (tarefas 8.1 e 8.2), antes do merge, porque a conexão passa a ser obrigatória.
2. Merge; o CD aplica a migração e publica.
3. Verificar `/health/ready`, `SendMessage` e `GET /analises/{id}` em produção, com dados sintéticos.
4. **Rollback:** promover o deploy anterior no Vercel. A tabela nova não afeta a versão anterior.

## Open Questions

- DP-11 (retenção), antes de dados reais. DP-23 fica para a mudança 10, e DP-15, para uma mudança própria de relatório.

## Skills aplicáveis

| Skill | Pré-condição | Situação |
| --- | --- | --- |
| `sofia-a2a-contrato` | Contrato e mapeamento de status e severidade. | Atendida (D2, D3, D4). |
| `testes-agentes-ia` | xUnit e Testcontainers. | Atendida (D9). |
| `vercel-dotnet-pgvector-atlas` | ADR de hospedagem; SQ-12 vigente; migrações fora da inicialização. | Atendida (D6). |
| `github-versionamento` | Branch por mudança e PR. | Atendida. |

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Não se aplica | Sem regras fiscais; a escala de severidade é do produto (D3). |
| P02 | Não se aplica | Validadores chegam nas mudanças 08 e 09. |
| P03 | Atende | Nove etapas registradas, com dependências (D1). |
| P04 | Atende | Categoria única e origem permitida (D1). |
| P05 | Atende | Status por regra determinística; sem conformidade indevida (D2). |
| P06 | Atende | Evidência mínima obrigatória no achado (D1). |
| P07 | Atende | Achado sem fundamento é rejeitado (D1). |
| P08 | Atende | Validações não realizadas e limitações no resultado (D4). |
| P09 | Atende | Revisão humana calculada pelo sistema (D2). |
| P10 | Atende | Conteúdo recebido continua tratado como dado. |
| P11 | Não se aplica | Sem base de conhecimento. |
| P12 | Não se aplica | Sem LLM. |
| P13 | Atende | Metadados preenchidos pelo sistema (D4). |
| P14 | Não se aplica | Sem ferramentas. |
| P15 | Atende | Banco próprio da Api.Agente (D6). |
| P16 | Atende | Pacotes justificados em D6; sem novo projeto. |
| P17 | Não se aplica | Sem chamadas a provedores de IA. |
| P18 | Não se aplica | Sem memória conversacional. |
| P19 | Atende | Registro de hash, versões e etapas (D8). |
| P20 | Atende | Esta mudança; tarefas ligadas aos requisitos. |
