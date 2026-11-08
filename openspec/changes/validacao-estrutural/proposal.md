# Proposal: Validação estrutural da ECF

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 08; PR #4). Em 2026-11-07, o assistente mediu a versão em Word do Leiaute 12 e propôs as respostas de E01 e E04, os requisitos novos (REQ-ESTRUT-007 a 013), o plano técnico e as tarefas. Kaio Macedo aprovou tudo em 2026-11-07 e pediu que o próprio assistente faça o gabarito no programa da ECF (design, D10). Implementada em 2026-11-07; o gabarito fecha a mudança.

## Why

A etapa 4 do fluxo (P03) valida estrutura, registros e campos do arquivo segundo o leiaute oficial. A constitution exige que essas regras sejam determinísticas, testadas e com fonte no manual (P01, P02), e que o conjunto de validações obrigatórias de cada versão esteja definido para que o agente nunca declare conformidade com validações pendentes (P05).

## What Changes

- Cria o catálogo de regras estruturais por versão do leiaute, extraído do Manual da ECF, com identificador, fonte e localização de cada regra.
- Implementa cada regra como validador determinístico, com testes de caso válido e inválido.
- Produz achados da categoria erro estrutural, com registro, linha e campo.
- Lista como validações não realizadas as regras obrigatórias ainda não implementadas.
- Trata falha interna de validador como falha técnica, não como erro do documento.

## Capabilities

### New Capabilities
- `agente/validacao-estrutural`: validação determinística da estrutura, dos registros e dos campos do arquivo por versão do leiaute.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Quais grupos de regras estruturais do manual entram na primeira entrega (por exemplo: registros existentes, hierarquia, campos obrigatórios, tipos e tamanhos)? | **Proposta, com a medição da versão em Word do Leiaute 12 (188 registros, 1.122 campos):** linha e registro do leiaute; campos (obrigatoriedade, tipo, tamanho máximo, decimais, data e valores válidos); hierarquia; registros obrigatórios na entrada e na saída; ocorrência máxima; ordem dos blocos; contagens de encerramento; e duas regras oficiais de ocorrência (`REGRA_OCORRENCIA_UNITARIA_ARQ` e `REGRA_DUPLICIDADE_DESPREZADA`). Onde o manual diverge de si mesmo, a verificação fica como não realizada (design, D2 e D5). | Assistente; aprovação de Kaio Macedo | 2026-11-07 | Não, depois da aprovação |
| E02 | O catálogo de regras é extraído do Manual da ECF, leiaute 12, pelo assistente e validado por quem tem responsabilidade de domínio? Quem valida (P01)? | O assistente extrai o catálogo da versão em Word do manual, com uma versão por regra, e Kaio Macedo aprova. Sem especialista fiscal na equipe (`docs/classificacao-das-regras.md`). | Kaio Macedo | 2026-11-06 | Não |
| E03 | Qual a relação com o programa validador oficial da ECF: reproduzir suas regras, complementar, ou só referenciar (DP-04)? | As regras vêm do manual. O programa oficial serve de gabarito: uma pessoa roda arquivos sintéticos, sem assinar nem transmitir, antes de fechar esta mudança (decisão 6). | Kaio Macedo | 2026-11-06 | Não |
| E04 | Regras que dependem de tabelas publicadas à parte do manual: quais tabelas e em que versão? | **Levantamento:** 141 campos dependem de tabela externa ou dinâmica, como a planilha de tabelas dinâmicas do Anexo I (p. 620), as tabelas distribuídas com o programa (país, moeda, plano referencial) e as do IBGE e dos Correios (p. 60). Ficam como validações não realizadas nesta mudança; registrá-las como fonte é trabalho da mudança 09 ou de uma própria (design, D5). | Assistente | 2026-11-07 | Não |
| E05 | O programa da ECF 12.2.8 importa e valida uma ECF sem o Registro 9999, sem nenhum apontamento (arquivo 06). O manual o dá como obrigatório (Tabela de Registros, `O`/`O`, p. 58; ocorrência 1:1, p. 619). O agente continua recusando (`EST-OCOR-001`, `impeditiva`)? | Mantém a recusa, porque o manual é a fonte (P01) e a falta do 9999 é o único sinal de arquivo cortado. | Kaio Macedo, com a proposta do assistente; evidência em design, "Gabarito no programa da ECF" | 2026-11-08 | Não |
| E06 | Com um x990 errado, o programa recusa a importação (arquivo 09: "Quantidade de linhas do bloco 0 informada não confere com a calculada pelo sistema"). O manual não tem regra própria para isso, e a D2 pôs `EST-TOT-001` em `observacao`. Ela sobe para `impeditiva`? | Sobe para `impeditiva`, pela p. 38 (erro de estrutura impede a importação) e pelo programa, e o veredito passa de `parcial` para `recusado`. **Alcance, com o teste feito depois da decisão:** só onde o programa recusa, que é o Bloco 0 e os blocos J a Y com `IND_DAD` igual a 0. Ele ignora a contagem dos blocos com `IND_DAD` igual a 1, do C, que a p. 41 diz não ser importado, assim como o E, e do 9990; nesses, segue `observacao`. | Kaio Macedo, com a proposta do assistente | 2026-11-08 | Não |
| E07 | O programa não aponta o 9900 errado (arquivo 10) nem o `9999.QTD_LIN` errado (arquivo 08), na importação nem na validação. O manual manda informar essas quantidades (p. 616 e 619). `EST-TOT-002` e `EST-TOT-003` ficam como estão? | Ficam em `observacao`, que não recusa o arquivo e segue o manual. | Kaio Macedo, com a proposta do assistente | 2026-11-08 | Não |
| E08 | O programa confere a ordem dos registros irmãos (arquivo 07: "Registro encontrado é diferente do registro esperado", com 0010 e 0020 esperados). O manual só diz que a ordem é sequencial (p. 58), e o agente declara isso em `naoVerificados` (D5). Uma regra nova entra na 08? | Não entra na 08. A regra seguiria a ordem da Tabela de Registros e precisa de aprovação (E01) e de um gabarito próprio; até lá, a limitação continua declarada. | Kaio Macedo, com a proposta do assistente | 2026-11-08 | Não |
| E09 | O programa exige tamanho exato no CEP (`C` 8) e nos campos por posição, mas aceita valores mais curtos em `0030.NUM` (`C` 6), `0020.IND_QTE_SCP` (`N` 3), `0030.NUM_TEL` (`N` 15), `ENDERECO`, `COMPL`, `BAIRRO` e `EMAIL`. O manual fala em tamanho exato "no caso geral" (p. 59) e para códigos de identificação (p. 35), sem lista de campos. Até onde o agente confere? | Mantém o máximo e o exato só nos campos por posição. A ampliação para os códigos de identificação da p. 35 depende de conferir o programa campo a campo, e a limitação continua declarada. | Kaio Macedo, com a proposta do assistente | 2026-11-08 | Não |
| E10 | O manual e o descritor do programa da ECF 12.2.8 divergem em pontos em que o agente, por seguir o manual, recusa o que o programa aceita: `S990`, `NUM_REC`, sinal, texto, valores, tamanhos, obrigatoriedade e os registros `C001`, `C990` e `E001` (`docs/catalogo/revisao-ecf-leiaute-12.md`, D1 a D8). O agente deixa de recusar nesses casos? | **Pendente.** Proposta do assistente: onde manual e programa divergem, o agente não recusa o que o programa aceita. Valor aceito pelo programa não gera achado; obrigatoriedade (D7) e registros ausentes (D8) ficam em `observacao`, citando o manual. A E05 continua como está. D8 depende de teste na tela do programa. | Kaio Macedo, com a proposta do assistente | 2026-11-08 | Sim, para arquivar |

## Fontes oficiais

| Documento | Emissor | Versão ou vigência | Localização |
| --- | --- | --- | --- |
| Manual da ECF, leiaute 12, versão em Word | Receita Federal do Brasil, portal SPED | Publicado em 23/07/2026; SHA-256 em `docs/fontes-oficiais.md` | Capítulo 2, p. 32 a 38 (arquivo, campos, tipos de inconsistência); Seções 4.1 a 4.3, p. 41 a 59 (blocos, Tabela de Registros, tabela de campos); Capítulo 4, p. 61 a 619 (cada registro); a página de cada regra e campo fica no catálogo (E02) |

Nenhuma regra estrutural é aprovada sem localização no manual.

## Princípios, decisões e dependências

- **Princípios:** P01, P02, P04, P05, P06, P08; SQ-09, SQ-10.
- **Decisões pendentes:** depende de DP-04.
- **Depende de:** 07 `recepcao-identificacao-leiaute`.
- **Skills:** `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Cruzamentos entre registros e apuração do IRPJ e da CSLL (mudança 09).
- Interpretações tributárias.

## Impact

- Catálogo versionado de regras, validadores e fixtures sintéticas por regra na Api.Agente.

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
