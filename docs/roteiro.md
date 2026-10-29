# Roteiro do Agente-ECF

Sequência das mudanças OpenSpec que constroem o agente, com dependências, decisões pendentes (DP) e skills de cada uma. A constitution (`openspec/constitution.md`) prevalece sobre este roteiro. Atualizado em 2026-10-29.

## Como ler

- **Especificação aprovada:** proposta e specs aprovadas por Kaio Macedo; faltam esclarecimentos, plano técnico e tarefas.
- **Em implementação:** especificação, plano e tarefas aprovados; código em andamento na branch `feat/<id>`.
- **Concluída:** mesclada na `main` e arquivada no OpenSpec.

Cada mudança segue o fluxo da constitution (6.1): especificação → esclarecimentos → plano técnico → tarefas → implementação → validação. A especificação inicial das mudanças 02 a 13 foi proposta em conjunto na branch `docs/roteiro-especificacoes`; a partir dos esclarecimentos, cada uma segue em sua branch `feat/<id>`.

## Situação

| Etapa | Situação |
| --- | --- |
| Constitution | v1.3.1 vigente (ratificada na v1.1.0; emendas aprovadas por Kaio Macedo). |
| CI/CD | CI com checks obrigatórios `OpenSpec` e `.NET`; CD publica no Vercel a cada merge na `main`. |
| Vercel | Em produção: https://agente-ecf-agente.vercel.app e https://agente-ecf-rag.vercel.app (time `agent-ecf`). |
| Mudança 01 | Concluída: mesclada (PR #2), em produção e arquivada em 2026-10-29; specs vigentes em `openspec/specs/plataforma/`. |
| Mudanças 02 a 13 | Especificações aprovadas por Kaio Macedo em 2026-10-29 (PR #4); próxima etapa: esclarecimentos da mudança 02. |

## Mudanças

| Nº | Mudança | Capacidades | Depende de | DPs | Skills | Estado |
| --- | --- | --- | --- | --- | --- | --- |
| 01 | `fundacao-solucao` | `plataforma/saude-servicos`, `configuracao-servicos`, `observabilidade`, `qualidade-entrega`, `entrega-servicos` | — | 17, 18 | `testes-agentes-ia`, `vercel-dotnet-pgvector-atlas`, `github-versionamento` | Concluída |
| 02 | `autenticacao-autorizacao` | `plataforma/autenticacao` | 01 | 09 | `testes-agentes-ia` | Especificação aprovada |
| 03 | `execucao-e-resultado` | `agente/execucao-analise`, `agente/contrato-resultado` | 01, 02 | 07, 08, 15, 23, 25 | `testes-agentes-ia` | Especificação aprovada |
| 04 | `registro-consumo-ia` | `plataforma/consumo-custos` | 01, 02 | 13, 26 | `controle-custos-llm`, `testes-agentes-ia` | Especificação aprovada |
| 05 | `base-conhecimento-ingestao` | `rag/ingestao-documentos`, `rag/administracao-base` | 02, 04 | 02, 03, 04, 24 | `rag-pgvector`, `controle-custos-llm`, `testes-agentes-ia` | Especificação aprovada |
| 06 | `recuperacao-conhecimento-mcp` | `rag/recuperacao-conhecimento`, `agente/consulta-conhecimento` | 03, 05 | 03, 14 | `mcp-client-server`, `rag-pgvector`, `vercel-dotnet-pgvector-atlas` | Especificação aprovada |
| 07 | `recepcao-identificacao-leiaute` | `agente/recepcao-arquivos`, `agente/identificacao-leiaute` | 03 | 04, 05, 11, 22, 25 | `testes-agentes-ia` | Especificação aprovada |
| 08 | `validacao-estrutural` | `agente/validacao-estrutural` | 07 | 04 | `testes-agentes-ia` | Especificação aprovada |
| 09 | `regras-negocio-irpj-csll` | `agente/regras-negocio` | 08 | 04, 06 | `testes-agentes-ia` | Especificação aprovada |
| 10 | `analise-llm` | `agente/analise-llm` | 04, 06, 08 | 02, 12, 14 | `semantic-kernel-tools`, `controle-custos-llm`, `testes-agentes-ia` | Especificação aprovada |
| 11 | `ferramentas-agente` | `agente/ferramentas` | 10 | 10 | `semantic-kernel-tools`, `testes-agentes-ia` | Especificação aprovada |
| 12 | `memoria-conversacional` | `agente/memoria-conversacional` | 02, 10 | 01, 11 | `memoria-mongodb`, `testes-agentes-ia` | Especificação aprovada |
| 13 | `servidor-a2a` | `integracao/servidor-a2a` | 02, 03, 10 | 09, 16 | `vercel-dotnet-pgvector-atlas`, `testes-agentes-ia` | Especificação aprovada |

`github-versionamento` vale para todas as mudanças. As mudanças 05 a 09 podem avançar em paralelo com 04 quando suas dependências estiverem mescladas.

## Fluxo de validação (constitution P03) por mudança

| Etapa | Mudança que entrega |
| --- | --- |
| 1. Receber o arquivo e identificar usuário, contexto e execução | 02 (identidade), 03 (execução), 07 (arquivo) |
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

## Skills do projeto

As skills ficam em `.claude/skills/` e foram adaptadas do pacote INFIS para Spec Kit. O `tasks.md` de cada mudança indica a skill de cada grupo de tarefas, e o plano técnico confirma as pré-condições que ela declara.

| Skill | Uso |
| --- | --- |
| `github-versionamento` | Branches, commits e PRs de todas as mudanças. |
| `testes-agentes-ia` | Cenários, testes e evidências de todas as mudanças. |
| `vercel-dotnet-pgvector-atlas` | Deploy (01) e provas de conceito de MCP e A2A no Vercel (06, 13). |
| `controle-custos-llm` | Registro de consumo (04) e chamadas a provedores em 05 e 10. |
| `rag-pgvector` | Ingestão e recuperação (05, 06). |
| `mcp-client-server` | Comunicação MCP entre os serviços (06). |
| `semantic-kernel-tools` | Análise com LLM e ferramentas (10, 11). |
| `memoria-mongodb` | Memória conversacional (12), após o ADR de DP-01. |

Sem skill dedicada por enquanto: autenticação (02), contrato de resultado (03), leitura e validação de arquivos ECF (07 a 09) e A2A (13). Essas mudanças seguem a constitution e as specs; uma skill pode ser criada quando o padrão se estabilizar.

## Pré-condições transversais

- **Dados reais:** só depois de DP-11 (retenção e LGPD) e DP-12 (política de dados com a OpenAI). Até lá, apenas dados sintéticos ou anonimizados.
- **Banco de dados:** a primeira mudança com PostgreSQL (03 ou 04) define, em esclarecimento, o provedor gerenciado compatível com o Vercel (pendência de DP-18).
- **Armazenamento de arquivos:** o disco do Vercel não é durável; as mudanças 05 e 07 definem onde guardar documentos e arquivos recebidos, se forem guardados.
- **MCP e A2A no Vercel:** provas de conceito antes de 06 e 13 (ADR 0002).
- **Fontes oficiais:** as regras de 07 a 09 só entram com localização no Manual da ECF, leiaute 12 (DP-04). A extração dessas localizações é a primeira tarefa dos esclarecimentos de cada uma.
