# Design: Perguntas sobre a ECF

> **Estado:** plano técnico escrito pelo assistente em 2026-11-12, depois da aprovação de Kaio Macedo à proposta, às specs e à emenda v2.2.0 ("Aprovo"). Segue as respostas aprovadas de E01 a E13.

## Context

A Sofia central encaminha ao agente perguntas sobre a ECF sem arquivo. Nesses pedidos, o único documento é `sofia-question-context`, e o texto dele é a própria pergunta. A Api.Agente já tem as peças de que a função precisa:

- o cliente MCP da ferramenta `buscar_no_manual` (mudança 06);
- o delimitador de fontes (`DelimitadorDeFontes`);
- o contrato SOFIA v1.0;
- o repositório PostgreSQL com Dapper.

Falta o que a mudança 10 também precisa: o cliente do modelo, a validação da saída e o consumo no banco da Api.Agente. Pela E02, esta mudança traz só o mínimo disso, e a 10 reaproveita.

## Goals / Non-Goals

**Goals:** responder a pergunta só com os trechos de uma versão do manual, citando a fonte (REQ-PERGUNTA-001 a 010). A resposta sai igual pelo A2A e pelo `POST /perguntas`, com registro e consumo, e a função fica desligada até a chave e a avaliação.

**Non-Goals:**

- as etapas 7 e 8 da análise de arquivo (mudança 10);
- memória de conversa (mudança 12);
- comparação entre versões do manual;
- qualquer mudança na Api.RAG ou na Sofia central.

## Decisions

### D1. Serviço próprio, fora das nove etapas (REQ-PERGUNTA-001, 008)

A aplicação ganha `ServicoDePerguntas`, em `Aplicacao/Perguntas/`. Ele não passa pelo `ServicoDeAnalise`, porque a pergunta não tem arquivo, e as nove etapas de P03 descrevem a análise de um arquivo. A ordem é sempre a mesma:

1. Validar a pergunta: de 1 a 500 caracteres, depois de tirar os espaços das pontas; senão, `pergunta_acima_do_limite` ou `pergunta_invalida`.
2. Conferir o limite diário (D9).
3. Resolver a versão do manual (D3).
4. Buscar os trechos (D4).
5. Redigir e validar a resposta (D5, D6).
6. Montar o resultado (D7) e registrar (D9).

- **Por que separado:** reaproveitar o envelope da análise obrigaria a inventar estados para as nove etapas e um veredito sem arquivo, o que confunde a Sofia e contraria P05.
- **Alternativa descartada:** uma décima etapa no fluxo da análise. Ela misturaria dois contratos de resultado.

### D2. Como o pedido chega ao modo pergunta (REQ-PERGUNTA-001; REQ-RECEPCAO-006)

- **No A2A:** o `ProcessadorDeSolicitacoes` valida o contrato como hoje. Com `Perguntas:Habilitada=true`, um único documento e o texto dele igual à `pergunta` (comparação ordinal, sem espaços nas pontas), o pedido vai ao `ServicoDePerguntas`. A regra não depende do id `sofia-question-context`: vale para qualquer cliente que mande a pergunta como documento. Pedido com outro documento segue para a análise e, sem ECF, recebe `nenhuma_ecf`.
- **No REST:** o endpoint `POST /perguntas` recebe JSON `{ "pergunta": "...", "versaoDoManual": "leiaute-11" }`. A versão é opcional e, quando vem, prevalece sobre a citada na pergunta. Com a função desligada, responde 404, como uma rota que não existe.

### D3. Versão do manual (REQ-PERGUNTA-002; E04)

A versão sai do texto da pergunta, sem diferenciar maiúsculas nem acentos:

- **Leiaute citado:** "leiaute 11", "leiaute-11" ou "layout 11" dá `leiaute-11`.
- **Ano-calendário citado:** "ano-calendário 2022", "ano calendário de 2022" ou "AC 2022" dá a versão cuja vigência é aquele ano.

A vigência sai de `docs/fontes/manifesto-rag.json` e fica numa tabela em código, testada contra o manifesto:

| Ano-calendário | Versão |
| --- | --- |
| 2020 | `leiaute-7` |
| 2021 | `leiaute-8` |
| 2022 | `leiaute-9` |
| 2023 | `leiaute-10` |
| 2024 | `leiaute-11` |
| 2025 | `leiaute-12` |

- **Mais de uma versão citada:** vale a mais recente, com a observação de que comparar versões não é atendido.
- **Versão fora de 7 a 12:** status `parcial` e a resposta de que a base tem os leiautes 7 a 12, sem busca.
- **Sem citação:** `leiaute-12`.

### D4. Busca (REQ-PERGUNTA-002, 007)

O serviço chama `IConsultaAoManual` com o tipo `manual-ecf`, a versão de D3 e a pergunta como consulta. São 5 trechos, a quantidade padrão avaliada na mudança 06.

- **Código literal:** quando a pergunta tem um código, ele vai no parâmetro `codigo`. Valem `REGRA_` seguido de letras, dígitos e sublinhado, um registro com letra e três dígitos (como `M300`) ou "registro" seguido de quatro dígitos (como `0000`). Um ano sozinho nunca é código.
- **Desfechos da busca:**

| Desfecho da busca | Resultado |
| --- | --- |
| `Trechos`, com algum | segue para a redação |
| `Trechos`, sem nenhum | `parcial`, "os manuais não tratam do assunto" |
| `BuscaDesativada`, `LimiteDeUsoAtingido` ou `FalhaTecnica` | `erro` com `consulta_indisponivel` |
| `VersaoIndisponivel` | `parcial`, com a versão pedida fora da base |

- **Relevância mínima (`Perguntas:RelevanciaMinima`):** fica vazia até a avaliação (D11; DP-03). Com valor, os trechos abaixo dele saem antes da redação. Se nenhum sobrar, a resposta diz que os manuais não tratam do assunto, sem chamada ao modelo (REQ-PERGUNTA-005).

### D5. Redação com Semantic Kernel (REQ-PERGUNTA-003; E01; RA-06)

A aplicação define a porta `IRedatorDeRespostas`. Ela recebe a pergunta e os trechos e devolve a saída bruta de uma tentativa, com os tokens informados pelo provedor. O adaptador `RedatorSemanticKernel` usa `Microsoft.SemanticKernel` 1.82.0 com o conector da OpenAI, a mesma escolha da mudança 10 (D1 dela). A seção `Llm` da configuração é a que a 10 vai usar:

| Configuração | Valor | Origem |
| --- | --- | --- |
| `Llm:Habilitada` | `false` em produção até a chave; `true` só na execução local com a chave | D9 da 10 |
| `Llm:Modelo` | `gpt-4.1-mini` | E01 da 10, decidido por Kaio Macedo |
| `Llm:Temperatura` | `0` | Menos variação e avaliação repetível |
| `Llm:TimeoutPorChamadaEmSegundos` | `30` | E03 da 10 |
| `Llm:Retentativas` | `2` | E03 da 10; só falha transitória |
| `OpenAI:ApiKey` | segredo, nunca no repositório | SQ-03 |

- **Prompt `pergunta-v1` (P19):** arquivo versionado, embutido no assembly. As instruções vão como mensagem de sistema. A pergunta vai delimitada em `<pergunta>`, e cada trecho com `DelimitadorDeFontes`, ambos escapados (P10).
- **Saída:** estruturada estrita (`json_schema`), com `respondida` (booleano), `resposta` (texto) e `citacoes` (lista de `trecho:<uuid>`). O modelo não preenche id de execução, status, custo nem data (P13).
- **Com `Llm:Habilitada=false` ou sem a chave:** o redator não é chamado, e sai a reserva (D6).
- **Com `Llm:Habilitada=true` e sem `OpenAI:ApiKey`:** o serviço não sobe, como a busca da Api.RAG (RA-09).

### D6. Validação e reserva (REQ-PERGUNTA-003, 004, 006, 007; P07, P12)

A resposta do modelo só é aceita se tudo abaixo valer:

- o JSON segue o schema;
- com `respondida=true`, `resposta` tem de 1 a 3.000 caracteres e ao menos uma citação;
- toda citação é um `trecho:<uuid>` recebido nesta busca;
- a resposta não contém "conforme" aplicado a uma ECF ou arquivo, numa lista fechada de frases testadas.

Com `respondida=false`, a resposta do modelo é descartada, e sai o texto fixo de que os manuais registrados (com a versão) não tratam do assunto.

- **Reserva:** se a resposta for recusada, se o modelo falhar depois das retentativas ou se o LLM estiver desligado, sai a resposta de reserva, montada pelo sistema. Ela traz os 3 trechos mais relevantes, cada um com documento, versão, página e até 600 caracteres do texto, e cabe em 3.000 caracteres.
- **Fontes:** só os trechos citados, ou os da reserva, entram em `fontes` e nas observações do A2A, no formato "Fonte: Manual de Orientação do Leiaute 12 da ECF, versão leiaute-12, p. 63".

### D7. Status e resultado (REQ-PERGUNTA-008; E03; RA-07)

| Situação | Status | Código |
| --- | --- | --- |
| Resposta aceita, com citação | `concluido` | — |
| Manuais não tratam do assunto, versão fora da base ou reserva | `parcial` | — |
| Pergunta vazia ou acima de 500 caracteres | `erro` | `pergunta_invalida` ou `pergunta_acima_do_limite` |
| Busca indisponível | `erro` | `consulta_indisponivel` |
| Limite diário | `erro` | `limite_de_uso` |
| Registro indisponível | `erro` | `falha_tecnica` |

O resultado interno é `RespostaDaPergunta`, contrato `resposta-pergunta` 1.0. Ele traz:

- `versaoContrato`, `idPergunta`, `status`, `resposta`, `versaoDoManual`, `fontes`, `observacoes` e `erro`;
- `versoes`, com o prompt e o modelo;
- `criadaEm` e `duracaoMs`.

O REST devolve esse objeto com HTTP 200; JSON malformado dá 400. O A2A o projeta no contrato SOFIA v1.0: `analise` recebe a resposta, `achados` vai vazio, `observacoes` recebe as fontes e as observações, e `erro` repete o código. `idExecucao` e `idSolicitacao` são os da Sofia, e `idExecucaoAgente` é o `idPergunta`. O resultado inteiro, com a resposta limitada a 3.000 caracteres, fica bem abaixo dos 20.000 do parecer final da Sofia central.

### D8. Cartão (REQ-A2A-007)

Com `Perguntas:Habilitada=true`, o cartão sai na versão 1.4.0. A descrição da skill acrescenta a frase sobre as perguntas, e vêm junto o exemplo "O que é o Registro 0000 da ECF?" e a tag `perguntas`. Desligado, o cartão continua o 1.3.0 de hoje, igual às cópias em `docs/contratos/`. A versão muda junto com o conteúdo (GD-01), e o deploy que ligar a função atualiza as cópias.

### D9. Registro e limite (REQ-PERGUNTA-009; DP-25)

Duas migrações entram na Api.Agente:

- **`0004_consumo_ia.sql`:** a tabela da mudança 10 (D8 dela), copiada da Api.RAG (RA-10). A finalidade é `pergunta` ou `analise`.
- **`0005_perguntas.sql`:** `id`, `id_solicitacao` (único, anulável no REST), `canal`, `status`, `codigo_erro`, `pergunta_sha256`, `pergunta_caracteres`, `versao_manual`, `trechos` (jsonb com id, página e relevância), `citados`, `redacao` (`aceita`, `reserva` ou `nao-chamada`), `modelo`, `criada_em` e `concluida_em`. Não há coluna de texto da pergunta nem da resposta.

O limite conta as perguntas registradas com status `concluido` ou `parcial` desde a meia-noite UTC (`Perguntas:LimitePorDia`, padrão 500).

- **Retentativa da Sofia:** com o mesmo `idSolicitacao`, a pergunta é respondida de novo, porque o texto da resposta não é guardado. O registro fica com a primeira resposta (`ON CONFLICT DO NOTHING`) e não conta duas vezes.
- **Falha do registro:** dá `erro` com `falha_tecnica`, como na análise.

### D10. Consumo (P17; skill `controle-custos-llm`)

Cada tentativa do redator grava uma linha em `consumo_ia`:

- tipo `geracao` ou `retentativa`, e status;
- duração e tokens informados pelo provedor, ou desconhecidos;
- custo pela tabela de preços `Consumo:TabelasDePrecos`, copiada da Api.RAG.

O preço do `gpt-4.1-mini` entra com a vigência e a fonte da página oficial, conferido no dia (DP-13). Sem preço conferido, o custo fica desconhecido, nunca zero. A gravação do consumo não derruba a resposta: falha nela vira log de erro.

### D11. Avaliação (REQ-PERGUNTA-010; P11, GD-08; E08)

O conjunto `docs/avaliacao/perguntas-respostas.json` tem perguntas sintéticas:

- 15 do conjunto da mudança 05, com a página e a evidência já conferidas;
- 5 que os manuais não respondem;
- 3 com instrução maliciosa.

O script `.github/scripts/avaliar-respostas.sh` sobe a Api.RAG como o `popular-rag.sh` faz: em contêiner, com a base de produção e a chave lida do arquivo do `SofiaInfis` só na hora (E11). Depois sobe a Api.Agente local com a função e o LLM ligados e manda cada pergunta ao `POST /perguntas`. O resultado vai para `docs/avaliacao/respostas.md`.

- **Critério de aceite proposto:**
  - em pelo menos 12 das 15 perguntas respondíveis (80%), a resposta sai `concluido` e cita uma página esperada;
  - as 5 fora do manual saem `parcial`;
  - nenhuma das 3 maliciosas afirma conformidade nem muda o status.
- **Limiar:** a avaliação também mede a relevância do melhor trecho nas perguntas respondíveis e nas não respondíveis. O limiar só é adotado se separar as duas sem perder acerto.

## Risks / Trade-offs

- **O modelo escreve algo que não está no trecho:** a citação conferida não prova o conteúdo. A mitigação é o prompt estrito, a temperatura 0, a avaliação e a lista de frases proibidas, e o risco fica nas limitações do resultado.
- **Retentativa gera texto diferente:** aceitável, porque o texto não é guardado (DP-25) e a fonte é a mesma base.
- **Custo:** o limite diário e o desligamento por configuração protegem o endpoint público (SQ-12).
- **Dependência do formato da Sofia central:** se ela mudar o pseudodocumento, o pedido cai em `nenhuma_ecf`, nunca numa resposta errada.

## Conformidade com a constitution

| Item | Situação | Como |
| --- | --- | --- |
| 1.3 (v2.2.0) | Atende | Resposta só com trechos e fonte; nunca declara conformidade de documento (D6). |
| P01, P07 | Atende | Citações conferidas; sem trecho, "não tratam do assunto" (D4, D6). |
| P10 | Atende | Pergunta e trechos delimitados e escapados (D5). |
| P11 | Atende | Uma versão por pergunta; limiar só depois da avaliação (D3, D4, D11). |
| P12, P13 | Atende | Saída estruturada estrita, validada; metadados do sistema (D5, D6, D7). |
| P15 | Atende | Busca só pela ferramenta MCP (D4). |
| P17 | Atende | Consumo por tentativa (D10). |
| P19 | Atende | Prompt `pergunta-v1` versionado (D5). |
| RA-06 | Atende | Semantic Kernel na infraestrutura (D5). |
| RA-07 | Atende | Contrato SOFIA v1.0 sem campo novo (D7). |
| SQ-03, SQ-12 | Atende | Chave só em variável de ambiente; limite diário; dados sintéticos (D9, D11). |
| DP-25 | Atende | Sem texto da pergunta nem da resposta no banco (D9). |

## Migration Plan

1. O PR entra com `Perguntas:Habilitada=false` e `Llm:Habilitada=false` em produção. O CD aplica as migrações 0004 e 0005, e nada muda para a Sofia.
2. A avaliação roda localmente com a chave (D11), e o resultado vai para `docs/avaliacao/respostas.md`.
3. Kaio Macedo cadastra `OpenAI__ApiKey` em `agente-ecf-rag` e `agente-ecf-agente` (E11).
4. Num PR seguinte, o assistente liga `Busca:Habilitada` na Api.RAG e `Perguntas:Habilitada` e `Llm:Habilitada` na Api.Agente em produção. Atualiza as cópias do cartão 1.4.0 e verifica com o script da Sofia.

**Rollback:** desligar `Perguntas:Habilitada`, e o pedido volta a `nenhuma_ecf`. As tabelas novas não afetam a análise.

## Open Questions

- E08: o limiar de relevância, que sai da avaliação.
