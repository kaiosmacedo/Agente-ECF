# Roteiro do Agente-ECF

Sequência das mudanças OpenSpec que constroem o agente, com dependências, decisões pendentes (DP) e skills de cada uma. A constitution (`openspec/constitution.md`) prevalece sobre este roteiro. Atualizado em 2026-11-07.

## Como ler

- **Especificação aprovada:** proposta e specs aprovadas por Kaio Macedo; faltam esclarecimentos, plano técnico e tarefas.
- **Plano aprovado:** esclarecimentos, plano técnico e tarefas aprovados por Kaio Macedo; a implementação ainda não começou.
- **Em implementação:** especificação, plano e tarefas aprovados; código em andamento na branch `feat/<id>`.
- **Congelada:** trabalho preservado na branch, sem merge, à espera de uma condição externa.
- **Concluída:** mesclada na `main` e arquivada no OpenSpec.

Cada mudança segue o fluxo da constitution (6.1): especificação → esclarecimentos → plano técnico → tarefas → implementação → validação. A especificação inicial das mudanças 02 a 13 foi proposta em conjunto na branch `docs/roteiro-especificacoes`; a partir dos esclarecimentos, cada uma segue em sua branch `feat/<id>`.

## Situação

| Etapa | Situação |
| --- | --- |
| Constitution | v2.1.0 vigente (ratificada na v1.1.0; emendas aprovadas por Kaio Macedo). |
| CI/CD | CI com checks obrigatórios `OpenSpec` e `.NET`; CD publica no Vercel a cada merge na `main`. |
| Vercel | Em produção: https://agente-ecf-agente.vercel.app e https://agente-ecf-rag.vercel.app (time `agent-ecf`). |
| Mudança 01 | Concluída: mesclada (PR #2), em produção e arquivada em 2026-10-29; specs vigentes em `openspec/specs/plataforma/`. |
| Mudança 02 | Congelada em 2026-11-05, por decisão de Kaio Macedo: não há autenticação de usuário. Desde 2026-11-10, o A2A exige só a chave Bearer compartilhada da Sofia central (SQ-12). Código preservado na branch `feat/autenticacao-autorizacao` (PR #7, em rascunho). |
| Mudança 13 | Esqueleto em produção desde 2026-11-05 (PR #9): cartão, `SendMessage` A2A v1.0, contrato SOFIA v1.0 e CORS. Desde 2026-11-10, também o A2A 0.3.0 e a chave Bearer da Sofia central do treinamento, conferidos em produção com a chave; falta o professor cadastrar o agente no catálogo da Sofia central. Falta o teste com a Sofia real, quando o cliente A2A dela existir. A análise completa chega com as mudanças 03 a 10. |
| Mudança 03 | Em produção e arquivada desde 2026-11-05 (PR #10): nove etapas, status, achados e `GET /analises/{id}`, com banco no Neon. |
| Mudanças 04 a 08 | Em produção: consumo e base do manual (04, 05), busca no manual por MCP, desligada até a decisão da chave (06), recepção da ECF (07) e validação estrutural do leiaute 12 (08). A ECF com erro de estrutura impeditivo sai `recusado`, com o que corrigir; sem ele, `parcial`. Estado de cada uma na tabela abaixo. |
| Mudanças 09 a 12 | Especificações aprovadas por Kaio Macedo em 2026-10-29 (PR #4) e adequadas às regras SQ-12 e RA-07 em 2026-11-05. |

## Mudanças

| Nº | Mudança | Capacidades | Depende de | DPs | Skills | Estado |
| --- | --- | --- | --- | --- | --- | --- |
| 01 | `fundacao-solucao` | `plataforma/saude-servicos`, `configuracao-servicos`, `observabilidade`, `qualidade-entrega`, `entrega-servicos` | — | 17, 18 | `testes-agentes-ia`, `vercel-dotnet-pgvector-atlas`, `github-versionamento` | Concluída |
| 02 | `autenticacao-autorizacao` | `plataforma/autenticacao` | 01 | 09 | `testes-agentes-ia` | Congelada |
| 03 | `execucao-e-resultado` | `agente/execucao-analise`, `agente/contrato-resultado` | 01, 13 | 07, 08, 15, 23, 25 | `sofia-a2a-contrato`, `testes-agentes-ia`, `vercel-dotnet-pgvector-atlas` | Arquivada; em produção desde 2026-11-05 (PR #10) |
| 04 | `registro-consumo-ia` | `plataforma/consumo-custos` | 01 | 13, 26 | `controle-custos-llm`, `testes-agentes-ia` | Arquivada; em produção desde 2026-11-06 (PR #18) |
| 05 | `base-conhecimento-ingestao` | `rag/ingestao-documentos`, `rag/administracao-base` | 04 | 02, 03, 04, 24 | `rag-pgvector`, `controle-custos-llm`, `testes-agentes-ia` | Em produção, com os seis manuais carregados e a recuperação avaliada (`docs/avaliacao/recuperacao.md`); falta o Release com os originais |
| 06 | `recuperacao-conhecimento-mcp` | `rag/recuperacao-conhecimento`, `agente/consulta-conhecimento` | 03, 05 | 03, 14 | `mcp-client-server`, `rag-pgvector`, `vercel-dotnet-pgvector-atlas` | Implementada; chave decidida em 2026-11-10: a busca liga quando a chave for cadastrada no Vercel |
| 07 | `recepcao-identificacao-leiaute` | `agente/recepcao-arquivos`, `agente/identificacao-leiaute` | 03 | 04, 05, 11, 22, 25 | `testes-agentes-ia` | Concluída: em produção e arquivada em 2026-11-07 |
| 08 | `validacao-estrutural` | `agente/validacao-estrutural` | 07 | 04 | `testes-agentes-ia` | Concluída: em produção desde 2026-11-07 e arquivada em 2026-11-08. Estrutura do leiaute 12, com o catálogo `ecf-l12-1.5.0`, conferido no programa da ECF 12.2.8 pelo gabarito e pela decisão E10; o que segue declarado está em `docs/catalogo/revisao-ecf-leiaute-12.md` |
| 09 | `regras-negocio-irpj-csll` | `agente/regras-negocio` | 08 | 04, 06 | `testes-agentes-ia` | Especificação aprovada; E01 e E02 dependem das respostas do superior (`docs/perguntas-negocio-ecf.md`) |
| 10 | `analise-llm` | `agente/analise-llm` | 04, 06, 08 | 02, 12, 14 | `semantic-kernel-tools`, `controle-custos-llm`, `testes-agentes-ia` | Especificação aprovada; Kaio Macedo decidiu o modelo, o envio à OpenAI e a chave em 2026-11-10; plano no repositório (PR #40), à espera da aprovação, para retomar depois da autenticação com a Sofia |
| 11 | `ferramentas-agente` | `agente/ferramentas` | 10 | 10 | `semantic-kernel-tools`, `testes-agentes-ia` | Especificação aprovada; esclarecimentos decididos em 2026-11-10 |
| 12 | `memoria-conversacional` | `agente/memoria-conversacional` | 10 | 01, 11 | `memoria-mongodb`, `testes-agentes-ia` | Especificação aprovada; MongoDB Atlas e retenção decididos em 2026-11-10 |
| 13 | `servidor-a2a` | `integracao/servidor-a2a`, `integracao/contrato-sofia`, `integracao/cors` | 01 | 09, 16 | `sofia-a2a-contrato`, `sofia-a2a-implementacao`, `sofia-cors-rest-a2a`, `vercel-dotnet-pgvector-atlas`, `testes-agentes-ia` | Esqueleto em produção |
| 14 | `telemetria-servicos` | `plataforma/observabilidade` | 01 | — | `testes-agentes-ia` | Arquivada; em produção desde 2026-11-05 (PR #13) |
| 15 | `classificacao-e-veredito` | `agente/contrato-resultado` | 03 | 07, 08 | `sofia-a2a-contrato`, `testes-agentes-ia` | Arquivada; em produção desde 2026-11-06 (PR #17 e PR #21) |
| 16 | `documentacao-api` | `plataforma/documentacao-api` | 01, 03 | — | `testes-agentes-ia` | Concluída: em produção e arquivada em 2026-11-10 (PR #41). Página do Swagger em https://agente-ecf-agente.vercel.app/swagger |
| 17 | `perguntas-ecf` | `agente/perguntas-ecf` | 05, 06, 13 (e 10, conforme E02) | 02, 03, 12 | `semantic-kernel-tools`, `mcp-client-server`, `rag-pgvector`, `controle-custos-llm`, `sofia-a2a-contrato`, `testes-agentes-ia` | Aprovada por Kaio Macedo em 2026-11-12, com a emenda v2.2.0; implementada na branch `feat/perguntas-ecf`, desligada em produção até a chave da OpenAI no `agente-ecf-agente` |

`github-versionamento` vale para todas as mudanças. As mudanças 05 a 09 podem avançar em paralelo com 04 quando suas dependências estiverem mescladas.

## Ordem de execução

**Correção do Swagger, 12/11/2026:** `corrigir-upload-ecf-swagger` em produção pelo PR #47, com regressão HTTP e envio conferido no próprio formulário: HTTP 201 e `recusado` no exemplo com erro, `parcial` no controle sem erro estrutural. Arquivada em `openspec/changes/archive/2026-11-12-corrigir-upload-ecf-swagger/`, com REQ-DOCAPI-005 na spec vigente. Guia em `docs/demonstracao-ecf.md`. Não altera o processamento em fluxo nem as regras fiscais.

A numeração é estável. A ordem foi revista em 2026-11-05, com as skills da turma e a análise do projeto `SofiaInfis` (`docs/base-sofiainfis.md`). Depois, Kaio Macedo aprovou colocar a base de conhecimento antes da recepção de arquivos, com as fontes oficiais em `docs/fontes-oficiais.md`, e pediu a classificação das regras e o veredito (mudança 15).

1. **13, esqueleto:** feito. A Sofia alcança o agente e recebe uma resposta válida, com status `parcial`.
2. **14, telemetria:** feito. Traces e métricas, sem dependência externa, deixam as próximas mudanças observáveis.
3. **03, execução e resultado:** registro das nove etapas, status, contrato de resultado e banco.
4. **15, classificação e veredito:** define o que a resposta diz: regras impeditivas, graves e em observação, um veredito claro e o detalhe de cada erro. Vem antes porque as validações já nascem nesse formato, e a Sofia conhece a resposta cedo.
5. **04, 05 e 06, consumo e base de conhecimento:** popula o RAG com os manuais dos leiautes 7 a 12 e deixa o agente consultá-los. Aceleradas pelo RAG do `SofiaInfis`. Os PDFs têm cerca de 6,5 MB, acima do limite do Vercel, de cerca de 4,5 MB, então a 05 precisa decidir como a ingestão recebe o arquivo.
6. **07, recepção e leiaute:** o maior risco, agora com a experiência da 05. Decide o que fazer com o limite do Vercel e com o corte de 20.000 caracteres da Sofia, o que pode exigir novo host (novo ADR). O plano aprovado em 2026-11-07 fixa 4 MiB, uma ECF por pedido e o cartão que diz à Sofia o que o agente analisa.
7. **08 e 09, validações:** as regras do manual viram validadores determinísticos, cada um com a fonte oficial (P02). É aqui que o agente passa a apontar o que está incorreto e o que corrigir.
8. **10, análise com LLM:** depende de 04, 06 e 08. Com ela, o status pode chegar a `concluido`, porque as nove etapas passam a existir.
9. **11 e 12, ferramentas e memória:** se ainda fizerem sentido no fluxo da Sofia. O contrato v1.0 não tem identificador de conversa.
10. **02, autenticação:** só se o responsável decidir adotá-la; não depende dos passos anteriores.

### Arquitetura final com a Sofia

Kaio Macedo descreveu em 2026-11-07 o fluxo final: a Sofia reconhece que recebeu uma ECF, encontra no cartão deste agente que ele trata ECF, envia o arquivo e recebe o resultado. Depois, ela leva o resultado a um agente validador, feito por outra pessoa, e só então responde ao usuário. Este repositório não muda nada por causa do validador: o agente não chama outros agentes (RA-07), e o resultado já traz o que um terceiro precisa para conferir cada achado (mudança 07, D10).

### O que o agente entrega ao final

Receber uma ECF, por exemplo da Sofia, e responder se ela está correta e, se não estiver, o que corrigir. Cada mudança contribui com uma parte:

| Parte | Mudança |
| --- | --- |
| Registro da análise e resultado | 03 |
| Classificação das regras, veredito e detalhe de cada erro | 15 |
| Manual pesquisável, para consultar e citar | 04, 05 e 06 |
| Recepção do arquivo e identificação do leiaute | 07 |
| **A validação em si:** regras do manual em código, com a localização de cada uma | 08 e 09 |
| Explicação e indícios, sem substituir as regras | 10 |

- **O RAG não valida sozinho:** ele localiza e cita o manual. Quem decide se uma regra foi violada são os validadores (P02).
- **"Incorreto" vem antes de "correto":** uma regra violada já basta para apontar um erro, a partir da 08. "Correto" exige todas as validações obrigatórias concluídas (P05), então só é possível depois da 10.
- **Janela de anos:** o agente cobre os anos que ainda podem ser retificados, hoje de 2020 a 2025, com os leiautes 7 a 12 (`docs/fontes-oficiais.md`). O catálogo começa pelo 12 e registra as diferenças dos anteriores.

## Autenticação adiada (SQ-12)

A primeira versão não tem autenticação de usuário (constitution, SQ-12). Desde 2026-11-10, o A2A exige só a chave Bearer compartilhada da Sofia central do treinamento, que autentica o serviço, não a pessoa. As exigências abaixo ficam adiadas e só voltam se a mudança 02 for retomada. Até lá, os serviços só recebem dados sintéticos ou anonimizados, e a identidade declarada pelo cliente não é verificada.

| Mudança | O que fica adiado | Referência |
| --- | --- | --- |
| 03 | Restringir `GET /analises/{id}` à organização e responder 404 para recurso de outra organização | REQ-EXEC-005, REQ-AUTH-004 |
| 04 | Restringir a consulta de consumo por perfil, com 403 | REQ-CONSUMO-005, REQ-AUTH-002 |
| 05 | Restringir a ingestão e a administração da base ao perfil administrador | RA-04, REQ-AUTH-002 |
| 06 | Autenticar a Api.Agente na chamada MCP à Api.RAG | RA-03, REQ-AUTH-005 |
| 07 | Exigir credencial em `POST /analises` | REQ-RECEPCAO-001, REQ-AUTH-001 |
| 11 | Verificar usuário e organização autenticados na autorização de ferramentas | REQ-FERRAM-002, SQ-02 |
| 12 | Isolar a memória pela identidade autenticada | REQ-MEMORIA-001, SQ-02 |
| 13 | Trocar a chave compartilhada por credencial própria de cada cliente A2A, com identidade verificada | REQ-A2A-003, REQ-A2A-010, REQ-AUTH-001 |

**Retomada:**

1. Definir o que a skill `sofia-cors-rest-a2a` deixa em aberto: emissor, audience de cada agente, scopes ou papéis, como a Sofia obtém o token e se o cartão fica público. O contrato SOFIA v1.0 não tem organização, então é preciso decidir também como ela chega (`X-Organizacao-Id` ou outro meio, sempre fora do contrato).
2. Comparar a definição com a especificação da mudança 02 e com o trabalho preservado no PR #7 (ADR 0003, validação de JWT, perfis e `X-Organizacao-Id`). A diretriz da skill é compatível com a validação de JWT já pronta.
3. Concluir a mudança 02, com CORS antes da autenticação no pipeline. A política de autenticação por padrão protege os endpoints existentes; cada um passa a declarar o perfil exigido.
4. Reaplicar as exigências da tabela, com testes.
5. Encerrar a regra SQ-12 por emenda à constitution.

## Fluxo de validação (constitution P03) por mudança

| Etapa | Mudança que entrega |
| --- | --- |
| 1. Receber o arquivo e identificar usuário, contexto e execução | 03 (execução, com identidade declarada, SQ-12), 07 (arquivo), 02 (identidade verificada, na retomada) |
| 2. Validar tamanho, formato e condições | 07 |
| 3. Identificar tipo e versão do leiaute | 07 |
| 4. Validar estrutura, registros e schema | 08 e 09 |
| 5. Consultar o manual por MCP | 06 (base em 05) |
| 6. Executar ferramentas externas autorizadas | 11 |
| 7. Produzir a análise com o LLM | 10 |
| 8. Validar a saída estruturada | 03 (contrato) e 10 (validação) |
| 9. Retornar o resultado com evidências | 03 |

## Organização das specs

As capacidades ficam em `openspec/specs/` (e nos deltas das mudanças) agrupadas por domínio. Cada requisito usa o prefixo da sua capacidade, que não pode ser reutilizado por outra.

| Domínio | Capacidade | Prefixo |
| --- | --- | --- |
| `plataforma/` (os dois serviços) | `saude-servicos` | `REQ-SAUDE` |
| | `configuracao-servicos` | `REQ-CONFIG` |
| | `observabilidade` | `REQ-OBSERV` |
| | `qualidade-entrega` | `REQ-QUALIDADE` |
| | `entrega-servicos` | `REQ-ENTREGA` |
| | `autenticacao` | `REQ-AUTH` |
| | `consumo-custos` | `REQ-CONSUMO` |
| | `documentacao-api` | `REQ-DOCAPI` |
| `agente/` (Api.Agente) | `execucao-analise` | `REQ-EXEC` |
| | `contrato-resultado` | `REQ-RESULT` |
| | `recepcao-arquivos` | `REQ-RECEPCAO` |
| | `identificacao-leiaute` | `REQ-LEIAUTE` |
| | `validacao-estrutural` | `REQ-ESTRUT` |
| | `regras-negocio` | `REQ-NEGOCIO` |
| | `consulta-conhecimento` | `REQ-CONSULTA` |
| | `analise-llm` | `REQ-ANALISE` |
| | `ferramentas` | `REQ-FERRAM` |
| | `memoria-conversacional` | `REQ-MEMORIA` |
| | `perguntas-ecf` | `REQ-PERGUNTA` |
| `rag/` (Api.RAG) | `ingestao-documentos` | `REQ-INGEST` |
| | `administracao-base` | `REQ-BASE` |
| | `recuperacao-conhecimento` | `REQ-RECUP` |
| `integracao/` (outros agentes) | `servidor-a2a` | `REQ-A2A` |
| | `contrato-sofia` | `REQ-SOFIA` |
| | `cors` | `REQ-CORS` |

## Skills do projeto

O que reaproveitar do projeto `SofiaInfis`, por mudança, está em `docs/base-sofiainfis.md`.

As skills ficam em `.claude/skills/` e foram adaptadas do pacote INFIS para Spec Kit. O `tasks.md` de cada mudança indica a skill de cada grupo de tarefas, e o plano técnico confirma as pré-condições que ela declara.

| Skill | Uso |
| --- | --- |
| `github-versionamento` | Branches, commits e PRs de todas as mudanças. |
| `testes-agentes-ia` | Cenários, testes e evidências de todas as mudanças. |
| `vercel-dotnet-pgvector-atlas` | Deploy (01) e provas de conceito de MCP e A2A no Vercel (06, 13). |
| `sofia-a2a-contrato` | Contrato SOFIA v1.0: DTOs, validação e testes de contrato (13); alinhamento de status, severidade e documentos (03, 07). |
| `sofia-a2a-implementacao` | Servidor A2A v1.0 e cartão do agente (13). |
| `sofia-cors-rest-a2a` | CORS e conectividade (13); diretriz de autenticação de serviço (02). |
| `controle-custos-llm` | Registro de consumo (04) e chamadas a provedores em 05 e 10. |
| `rag-pgvector` | Ingestão e recuperação (05, 06). |
| `mcp-client-server` | Comunicação MCP entre os serviços (06). |
| `semantic-kernel-tools` | Análise com LLM e ferramentas (10, 11). |
| `memoria-mongodb` | Memória conversacional (12), após o ADR de DP-01. |

Sem skill dedicada por enquanto: contrato de resultado (03) e leitura e validação de arquivos ECF (07 a 09). Essas mudanças seguem a constitution e as specs; uma skill pode ser criada quando o padrão se estabilizar.

## Pré-condições transversais

- **Dados reais:** só depois de DP-11 (retenção e LGPD) e DP-12 (política de dados com a OpenAI). Até lá, apenas dados sintéticos ou anonimizados.
- **Antes de dados reais, medir ECF reais:** o limite de 4 MiB da mudança 07 vem do Vercel, não do tamanho de um arquivo real. Se os arquivos reais passarem dele, a Api.Agente precisa de outro host, por novo ADR.
- **Banco de dados:** PostgreSQL gerenciado no Neon, escolhido por Kaio Macedo em 2026-11-05 (mudança 03).
- **Armazenamento de arquivos:** o disco do Vercel não é durável; as mudanças 05 e 07 definem onde guardar documentos e arquivos recebidos, se forem guardados.
- **MCP e A2A no Vercel:** prova de conceito antes de 06 (ADR 0002), feita em 2026-11-07: o MCP sem estado funciona entre instâncias (ADR 0005, aceito); confirmado no Vercel na 06 (`text/event-stream`, PR #24). Na 13, o A2A não usa streaming e funcionou em produção. Em instâncias que não duram, `GetTask` não é garantido; a resposta de `SendMessage` já é final.
- **Tamanho no Vercel:** o corpo da requisição é recusado entre 4,4 MB e 4,6 MB (medido em 2026-11-05), com HTTP 413. A mudança 07 precisa decidir o que fazer com arquivos reais maiores; na rede Docker esse limite não existe.
- **Fontes oficiais:** as regras de 07 a 09 só entram com localização no Manual da ECF, leiaute 12 (DP-04), registrado em `docs/fontes-oficiais.md`. A extração dessas localizações é a primeira tarefa dos esclarecimentos de cada uma.
