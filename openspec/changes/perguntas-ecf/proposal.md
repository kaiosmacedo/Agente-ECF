# Proposal: Perguntas sobre a ECF

> **Estado:** proposta escrita pelo assistente em 2026-11-12, a pedido de Kaio Macedo, à espera da aprovação dele (P20). Exige emenda à constitution (seção 1.3) e as respostas dos esclarecimentos bloqueantes (E01 a E03) antes das specs e do plano técnico.

## Why

A Sofia central já encaminha ao agente as perguntas que citam a ECF, o LALUR, o LACS, o IRPJ ou a CSLL, mesmo sem arquivo. Nesses casos, manda um pseudodocumento `sofia-question-context` com o texto da pergunta. Hoje o agente responde `erro` com `nenhuma_ecf`, e o parecer da Sofia sai sem a contribuição do especialista. A base de conhecimento já tem os seis manuais oficiais da ECF indexados (leiautes 7 a 12, mudança 05), e a busca por MCP está pronta (mudança 06). Com isso, o agente pode responder a pergunta só com o que os manuais dizem, citando documento, versão e página.

## What Changes

- **Emenda à constitution (MINOR, v2.2.0).** A seção 1.3 diz que "nesta fase, o agente apenas valida o arquivo recebido". A emenda acrescenta uma segunda função: responder perguntas sobre a ECF fundamentadas só nas fontes oficiais registradas, com a fonte citada, sem interpretação tributária sem fundamento (1.3) e sem substituir o profissional.
- **Modo pergunta na Api.Agente.** Quando o pedido só traz a pergunta, o agente responde a pergunta em vez de `nenhuma_ecf`. Isso vale para o pseudodocumento da Sofia central, cujo texto é a própria pergunta, e para o endpoint REST novo. Com um documento de outro tipo, a resposta continua `nenhuma_ecf` (REQ-RECEPCAO-006; E09). Com uma ECF no pedido, nada muda: o fluxo de análise segue como hoje.
- **Recuperação por MCP (P15).** A pergunta vai à ferramenta `buscar_no_manual` da Api.Agente para a Api.RAG, filtrada por tipo `manual-ecf` e por uma única versão (P11): a que a pergunta citar ou, sem citação, o leiaute 12 (E04).
- **Resposta fundamentada.** O texto da resposta sai só dos trechos recuperados, com documento, versão e página de cada trecho usado (P06, P07). Sem trecho relevante, a resposta diz que os manuais registrados não tratam do assunto, sem completar com conhecimento do modelo (P01). A forma da resposta, gerada pelo LLM ou só com os trechos, é a decisão E01.
- **Contrato SOFIA v1.0 sem alteração.** A resposta vai em `analise`, as fontes em `observacoes` e `achados` vai vazio. O `status` segue E03. O resultado inteiro cabe no limite da Sofia central: ela recusa o parecer final acima de 20.000 caracteres (`FINAL_REVIEW_INPUT_TOO_LARGE`).
- **Canal REST para a demonstração.** Um endpoint `POST /perguntas` publicado no Swagger devolve o mesmo resultado do A2A (REQ-RECEPCAO-008; E05).
- **Cartão A2A.** A skill `analisar_documentos` ganha exemplo e tag de pergunta, e a versão do cartão sobe para 1.4.0. Não nasce outra skill: o catálogo da Sofia central só permite `analisar_documentos` a este agente.
- **Registro, custo e limite.** Cada pergunta vira uma execução registrada, sem guardar o texto da pergunta (DP-25). O registro guarda o hash, a versão consultada, os trechos usados, o modelo e o consumo no `consumo_ia`. Um limite diário configurável protege o endpoint pago e público (SQ-12; E07).
- **Avaliação antes de ligar em produção (P11, GD-08).** Um conjunto de perguntas sintéticas com a página esperada no manual mede se a resposta cita a página certa e recusa o que o manual não cobre. O conjunto inclui perguntas adversariais (P10).

## Capabilities

### New Capabilities
- `agente/perguntas-ecf`: responder perguntas sobre a ECF com base nos manuais oficiais registrados, pelos canais A2A e REST.

### Modified Capabilities
- `agente/recepcao-arquivos`: o pedido que só traz a pergunta vai ao modo pergunta, e não mais a `nenhuma_ecf` (REQ-RECEPCAO-006).
- `integracao/servidor-a2a`: a descrição da skill cita as perguntas (REQ-A2A-007). A versão 1.4.0 do cartão segue a REQ-A2A-008, que a mudança 13 altera; o valor do cenário é atualizado depois que a 13 for arquivada.
- `plataforma/documentacao-api`: o endpoint novo entre as rotas publicadas (REQ-DOCAPI-004).

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | A resposta é escrita pelo LLM a partir dos trechos (A) ou é só a lista dos trechos mais relevantes, com página (B)? | **Proposta:** A, com B como reserva quando o LLM estiver desligado ou falhar. A resposta A lê melhor na Sofia. Ela usa o modelo já decidido na mudança 10 (`gpt-4.1-mini`), com temperatura 0, e cita só identificadores de trechos recebidos. O sistema confere cada citação antes de aceitar a resposta (P07, P12). | Kaio Macedo | — | Sim |
| E02 | A geração depende da infraestrutura da mudança 10 (Semantic Kernel, cliente da OpenAI, `consumo_ia`, validação da saída), cujo plano espera aprovação. A 17 entra antes da 10, com o mínimo para a pergunta reaproveitado depois pela 10, ou depois dela? | **Proposta:** antes da 10, só com o mínimo: cliente do modelo, prompt `pergunta-v1`, validação da saída e consumo. A 10 reaproveita essa base quando for implementada. | Kaio Macedo | — | Sim |
| E03 | Que `status` do contrato SOFIA a resposta recebe? Pela P05, `concluido` hoje fica reservado à análise com todas as validações obrigatórias. | **Proposta:** `concluido` quando a resposta cita ao menos um trecho do manual; `parcial` quando o manual não trata do assunto ou só a reserva B saiu; `erro` com `consulta_indisponivel` quando a busca falha. A emenda deixa claro que a P05 vale para a análise de arquivo. | Kaio Macedo | — | Sim |
| E04 | Que versão do manual consultar? | **Proposta:** a versão que a pergunta citar, pelo leiaute ("leiaute 11") ou pelo ano-calendário, com a vigência do `manifesto-rag.json`. Sem citação, o leiaute 12. Uma consulta por pedido, sem misturar versões (P11). Comparar versões fica fora. | Assistente; aprovação de Kaio Macedo | 2026-11-12 | Não |
| E05 | Como expor a função no REST? | **Proposta:** `POST /perguntas`, com corpo JSON (`pergunta`, `versaoDoManual` opcional), aberto como os demais endpoints REST (SQ-12) e no Swagger. | Assistente; aprovação de Kaio Macedo | 2026-11-12 | Não |
| E06 | O que guardar de cada pergunta? | **Proposta:** como a análise (DP-25): sem o texto da pergunta. O registro guarda o hash, o tamanho, a versão consultada, os trechos usados (identificador, página e relevância), o modelo, o consumo e os tempos. | DP-25 | 2026-11-12 | Não |
| E07 | Qual o limite diário de perguntas? | **Proposta:** 500 por dia, configurável, contadas no `consumo_ia`. Acima disso, `erro` com `limite_de_uso`. O custo por resposta é medido na avaliação, com o preço do modelo conferido na página oficial (DP-13). | Assistente; aprovação de Kaio Macedo | 2026-11-12 | Não |
| E08 | Com que relevância mínima um trecho entra na resposta? | **Aberto:** sai da avaliação, como pede a P11 (DP-03), com perguntas que os manuais não respondem. A mudança 06 ainda não tem limiar avaliado. | Avaliação no plano técnico | — | Não |
| E09 | Que pedidos sem ECF vão ao modo pergunta? | **Proposta:** só os que não trazem outro documento além da pergunta: o pseudodocumento `sofia-question-context` da Sofia central, cujo texto é a própria pergunta, e o `POST /perguntas`. Com uma NF-e ou outro documento, `nenhuma_ecf`, como hoje, para não responder pelo manual a um pedido de validação de outro arquivo. | Assistente; aprovação de Kaio Macedo | 2026-11-12 | Não |
| E10 | O que fazer com um pedido que traz uma ECF e também uma pergunta? | **Proposta:** a análise da ECF, como hoje. Responder a pergunta junto com a análise é assunto da mudança 12 (memória conversacional) ou de outra mudança. | Assistente; aprovação de Kaio Macedo | 2026-11-12 | Não |
| E11 | A função em produção precisa da chave da OpenAI no Vercel, na Api.RAG para o embedding da pergunta e na Api.Agente para a geração. | **Pendente com Kaio Macedo:** cadastrar `OpenAI__ApiKey` nos projetos `agente-ecf-rag` e `agente-ecf-agente` (mudança 06, tarefa 6.1). Sem a chave, o desenvolvimento e os testes seguem, e a produção responde `erro` com `consulta_indisponivel`. | Kaio Macedo | — | Não, para o desenvolvimento |
| E12 | Qual o tamanho máximo da pergunta? | **Proposta:** 500 caracteres, o limite da ferramenta `buscar_no_manual` (mudança 06). Acima disso, `erro` com `pergunta_acima_do_limite`. | Assistente; aprovação de Kaio Macedo | 2026-11-12 | Não |
| E13 | Qual o tamanho máximo da resposta? | **Proposta:** 3.000 caracteres em `analise`. A Sofia central junta a pergunta e o resultado de todos os especialistas num parecer de até 20.000 caracteres e recusa o que passar disso. | Assistente; aprovação de Kaio Macedo | 2026-11-12 | Não |

## Fontes oficiais

| Documento | Emissor | Versão ou vigência | Localização |
| --- | --- | --- | --- |
| Manuais de Orientação dos Leiautes 7 a 12 da ECF | Receita Federal do Brasil (SPED) | Anos-calendário 2020 a 2025 e situações especiais de 2021 a 2026 | Portal SPED; registrados em `docs/fontes/manifesto-rag.json` (mudança 05) |

A mudança não cria regra fiscal: a resposta repete e cita o que os manuais dizem.

## Princípios, decisões e dependências

- **Princípios:** P01, P05 (E03), P06, P07, P10, P11, P12, P13, P15, P20; RA-07; SQ-12; GD-08.
- **Emenda:** seção 1.3 da constitution, MINOR (8.2).
- **Decisões pendentes:** depende de DP-02 (modelo, decidido para a 10), DP-03 (limiar, E08) e DP-12 (envio à OpenAI, decidido em 2026-11-10 só para dados sintéticos); segue DP-25 (o que não se guarda).
- **Depende de:** 05 `base-conhecimento-ingestao` (base), 06 `recuperacao-conhecimento-mcp` (busca e chave em produção), 13 `servidor-a2a` (cartão e canal) e, conforme E02, 10 `analise-llm`.
- **Skills:** `semantic-kernel-tools`, `mcp-client-server`, `rag-pgvector`, `controle-custos-llm`, `sofia-a2a-contrato`, `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Pergunta e ECF na mesma resposta (E09) e conversa com memória (mudança 12).
- Comparar versões do manual numa mesma resposta.
- Responder com base em fonte que não esteja registrada, como legislação, soluções de consulta ou jurisprudência.
- Mudar o contrato SOFIA v1.0 ou criar outra skill no cartão.
- Perguntas sobre outras escriturações (ECD, EFD).

## Impact

- **Api.Agente:** modo pergunta na recepção, cliente MCP reaproveitado, geração com o modelo, endpoint `POST /perguntas`, cartão 1.4.0, registro e consumo.
- **Api.RAG:** nenhuma mudança de código; mais consultas na ferramenta `buscar_no_manual`, dentro do limite da mudança 06.
- **Documentação:** `docs/contratos/sofia-a2a.md` (pergunta sem arquivo), `docs/roteiro.md` (mudança 17), constitution v2.2.0 e Swagger.
- **Sofia central:** nenhuma mudança. O agente já está no catálogo com os termos da ECF.

## Aprovação

Pendente: Kaio Macedo aprova a proposta, a emenda e os esclarecimentos E01 a E03 (P20). Depois vêm as specs, o plano técnico e as tarefas, na branch `feat/perguntas-ecf`.
