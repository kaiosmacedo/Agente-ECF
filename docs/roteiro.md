# Roteiro do Agente-ECF

Sequência das mudanças OpenSpec que constroem o agente, com dependências, decisões pendentes (DP) e skills de cada uma. A constitution (`openspec/constitution.md`) prevalece sobre este roteiro. Atualizado em 2026-11-05.

## Como ler

- **Especificação aprovada:** proposta e specs aprovadas por Kaio Macedo; faltam esclarecimentos, plano técnico e tarefas.
- **Em implementação:** especificação, plano e tarefas aprovados; código em andamento na branch `feat/<id>`.
- **Congelada:** trabalho preservado na branch, sem merge, à espera de uma condição externa.
- **Concluída:** mesclada na `main` e arquivada no OpenSpec.

Cada mudança segue o fluxo da constitution (6.1): especificação → esclarecimentos → plano técnico → tarefas → implementação → validação. A especificação inicial das mudanças 02 a 13 foi proposta em conjunto na branch `docs/roteiro-especificacoes`; a partir dos esclarecimentos, cada uma segue em sua branch `feat/<id>`.

## Situação

| Etapa | Situação |
| --- | --- |
| Constitution | v2.0.0 vigente (ratificada na v1.1.0; emendas aprovadas por Kaio Macedo). |
| CI/CD | CI com checks obrigatórios `OpenSpec` e `.NET`; CD publica no Vercel a cada merge na `main`. |
| Vercel | Em produção: https://agente-ecf-agente.vercel.app e https://agente-ecf-rag.vercel.app (time `agent-ecf`). |
| Mudança 01 | Concluída: mesclada (PR #2), em produção e arquivada em 2026-10-29; specs vigentes em `openspec/specs/plataforma/`. |
| Mudança 02 | Congelada em 2026-11-05: a integração com a Sofia não usa token, por decisão de Kaio Macedo. Código preservado na branch `feat/autenticacao-autorizacao` (PR #7, em rascunho). |
| Mudança 13 | Esqueleto em produção desde 2026-11-05 (PR #9): cartão, `SendMessage` A2A v1.0, contrato SOFIA v1.0 e CORS. Falta o teste com a Sofia real, quando o cliente A2A dela existir. A análise completa chega com as mudanças 03 a 10. |
| Mudanças 03 a 12 | Especificações aprovadas por Kaio Macedo em 2026-10-29 (PR #4) e adequadas às regras SQ-12 e RA-07 em 2026-11-05; próxima etapa: esclarecimentos da mudança 03. |

## Mudanças

| Nº | Mudança | Capacidades | Depende de | DPs | Skills | Estado |
| --- | --- | --- | --- | --- | --- | --- |
| 01 | `fundacao-solucao` | `plataforma/saude-servicos`, `configuracao-servicos`, `observabilidade`, `qualidade-entrega`, `entrega-servicos` | — | 17, 18 | `testes-agentes-ia`, `vercel-dotnet-pgvector-atlas`, `github-versionamento` | Concluída |
| 02 | `autenticacao-autorizacao` | `plataforma/autenticacao` | 01 | 09 | `testes-agentes-ia` | Congelada |
| 03 | `execucao-e-resultado` | `agente/execucao-analise`, `agente/contrato-resultado` | 01, 13 | 07, 08, 15, 23, 25 | `testes-agentes-ia` | Especificação aprovada |
| 04 | `registro-consumo-ia` | `plataforma/consumo-custos` | 01 | 13, 26 | `controle-custos-llm`, `testes-agentes-ia` | Especificação aprovada |
| 05 | `base-conhecimento-ingestao` | `rag/ingestao-documentos`, `rag/administracao-base` | 04 | 02, 03, 04, 24 | `rag-pgvector`, `controle-custos-llm`, `testes-agentes-ia` | Especificação aprovada |
| 06 | `recuperacao-conhecimento-mcp` | `rag/recuperacao-conhecimento`, `agente/consulta-conhecimento` | 03, 05 | 03, 14 | `mcp-client-server`, `rag-pgvector`, `vercel-dotnet-pgvector-atlas` | Especificação aprovada |
| 07 | `recepcao-identificacao-leiaute` | `agente/recepcao-arquivos`, `agente/identificacao-leiaute` | 03 | 04, 05, 11, 22, 25 | `testes-agentes-ia` | Especificação aprovada |
| 08 | `validacao-estrutural` | `agente/validacao-estrutural` | 07 | 04 | `testes-agentes-ia` | Especificação aprovada |
| 09 | `regras-negocio-irpj-csll` | `agente/regras-negocio` | 08 | 04, 06 | `testes-agentes-ia` | Especificação aprovada |
| 10 | `analise-llm` | `agente/analise-llm` | 04, 06, 08 | 02, 12, 14 | `semantic-kernel-tools`, `controle-custos-llm`, `testes-agentes-ia` | Especificação aprovada |
| 11 | `ferramentas-agente` | `agente/ferramentas` | 10 | 10 | `semantic-kernel-tools`, `testes-agentes-ia` | Especificação aprovada |
| 12 | `memoria-conversacional` | `agente/memoria-conversacional` | 10 | 01, 11 | `memoria-mongodb`, `testes-agentes-ia` | Especificação aprovada |
| 13 | `servidor-a2a` | `integracao/servidor-a2a`, `integracao/contrato-sofia`, `integracao/cors` | 01 | 09, 16 | `sofia-a2a-contrato`, `sofia-a2a-implementacao`, `sofia-cors-rest-a2a`, `vercel-dotnet-pgvector-atlas`, `testes-agentes-ia` | Esqueleto em produção |
| 14 | `telemetria-servicos` | `plataforma/observabilidade` | 01 | — | `testes-agentes-ia` | Em implementação |

`github-versionamento` vale para todas as mudanças. As mudanças 05 a 09 podem avançar em paralelo com 04 quando suas dependências estiverem mescladas.

## Ordem de execução

A numeração é estável. A ordem foi revista em 2026-11-05, com as skills da turma e a análise do projeto `SofiaInfis` (`docs/base-sofiainfis.md`). Depois, Kaio Macedo pediu para colocar a base de conhecimento antes da recepção de arquivos, com as fontes oficiais já localizadas em `docs/fontes-oficiais.md`.

1. **13, esqueleto:** feito. A Sofia alcança o agente e recebe uma resposta válida, com status `parcial`.
2. **14, telemetria:** traces e métricas, sem dependência externa. Deixa as próximas mudanças observáveis.
3. **03, execução e resultado:** registro das nove etapas, status, contrato de resultado e banco.
4. **04, 05 e 06, consumo e base de conhecimento:** popula o RAG com o Manual da ECF, leiaute 12, e deixa o agente consultá-lo. Aceleradas pelo RAG do `SofiaInfis`. O PDF tem 6,5 MB, acima do limite do Vercel, de cerca de 4,5 MB, então a 05 precisa decidir como a ingestão recebe o arquivo.
5. **07, recepção e leiaute:** o maior risco, agora com a experiência da 05. Decide o que fazer com o limite do Vercel e com o corte de 20.000 caracteres da Sofia, o que pode exigir novo host (novo ADR).
6. **08 e 09, validações:** as regras do manual viram validadores determinísticos, cada um com a fonte oficial (P02). É aqui que o agente passa a apontar o que está incorreto e o que corrigir.
7. **10, análise com LLM:** depende de 04, 06 e 08. Com ela, o status pode chegar a `concluido`, porque as nove etapas passam a existir.
8. **11 e 12, ferramentas e memória:** se ainda fizerem sentido no fluxo da Sofia. O contrato v1.0 não tem identificador de conversa.
9. **02, autenticação:** só se o responsável decidir adotá-la; não depende dos passos anteriores.

### O que o agente entrega ao final

Receber uma ECF, por exemplo da Sofia, e responder se ela está correta e, se não estiver, o que corrigir. Cada mudança contribui com uma parte:

| Parte | Mudança |
| --- | --- |
| Registro da análise e resultado | 03 |
| Manual pesquisável, para consultar e citar | 04, 05 e 06 |
| Recepção do arquivo e identificação do leiaute | 07 |
| **A validação em si:** regras do manual em código, com a localização de cada uma | 08 e 09 |
| Explicação e indícios, sem substituir as regras | 10 |

- **O RAG não valida sozinho:** ele localiza e cita o manual. Quem decide se uma regra foi violada são os validadores (P02).
- **"Incorreto" vem antes de "correto":** uma regra violada já basta para apontar um erro, a partir da 08. "Correto" exige todas as validações obrigatórias concluídas (P05), então só é possível depois da 10.
- **Falta um campo de veredito:** o contrato tem status e achados, mas nenhum campo que diga "correto" ou "incorreto", e `concluido` não significa "conforme". A resposta precisa ser calculada pelo sistema, e o texto deve deixar claro que vale para as validações realizadas e que o resultado é apoio à conferência (seção 1.3). A definir nos esclarecimentos da 08.

## Autenticação adiada (SQ-12)

A primeira versão não tem autenticação (constitution, SQ-12), e a integração com a Sofia não usa token. As exigências abaixo ficam adiadas e só voltam se a mudança 02 for retomada. Até lá, os serviços só recebem dados sintéticos ou anonimizados, e a identidade declarada pelo cliente não é verificada.

| Mudança | O que fica adiado | Referência |
| --- | --- | --- |
| 03 | Restringir `GET /analises/{id}` à organização e responder 404 para recurso de outra organização | REQ-EXEC-005, REQ-AUTH-004 |
| 04 | Restringir a consulta de consumo por perfil, com 403 | REQ-CONSUMO-005, REQ-AUTH-002 |
| 05 | Restringir a ingestão e a administração da base ao perfil administrador | RA-04, REQ-AUTH-002 |
| 06 | Autenticar a Api.Agente na chamada MCP à Api.RAG | RA-03, REQ-AUTH-005 |
| 07 | Exigir credencial em `POST /analises` | REQ-RECEPCAO-001, REQ-AUTH-001 |
| 11 | Verificar usuário e organização autenticados na autorização de ferramentas | REQ-FERRAM-002, SQ-02 |
| 12 | Isolar a memória pela identidade autenticada | REQ-MEMORIA-001, SQ-02 |
| 13 | Exigir credencial da Sofia INFIS e de outros clientes na chamada A2A; declarar o esquema de segurança no cartão, se ele deixar de ser público | REQ-A2A-003, REQ-AUTH-001 |

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
- **Banco de dados:** a primeira mudança com PostgreSQL (03 ou 04) define, em esclarecimento, o provedor gerenciado compatível com o Vercel (pendência de DP-18).
- **Armazenamento de arquivos:** o disco do Vercel não é durável; as mudanças 05 e 07 definem onde guardar documentos e arquivos recebidos, se forem guardados.
- **MCP e A2A no Vercel:** prova de conceito antes de 06 (ADR 0002). Na 13, o A2A não usa streaming e funcionou em produção. Em instâncias que não duram, `GetTask` não é garantido; a resposta de `SendMessage` já é final.
- **Tamanho no Vercel:** o corpo da requisição é recusado entre 4,4 MB e 4,6 MB (medido em 2026-11-05), com HTTP 413. A mudança 07 precisa decidir o que fazer com arquivos reais maiores; na rede Docker esse limite não existe.
- **Fontes oficiais:** as regras de 07 a 09 só entram com localização no Manual da ECF, leiaute 12 (DP-04), registrado em `docs/fontes-oficiais.md`. A extração dessas localizações é a primeira tarefa dos esclarecimentos de cada uma.
