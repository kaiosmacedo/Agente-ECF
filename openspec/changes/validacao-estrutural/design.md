# Design: Validação estrutural da ECF

> **Estado:** aprovado por Kaio Macedo em 2026-11-07.

## Context

Depois da mudança 07, o agente sabe que recebeu uma ECF inteira e de qual leiaute. Esta mudança é a primeira que pode dizer à Sofia INFIS "esta ECF está incorreta, e é isto que precisa ser corrigido". Ela confere a etapa 4 do fluxo (P03): se cada linha é um registro do leiaute, se os campos têm o tipo, o tamanho e os valores que o manual define, se os registros aparecem na hierarquia e na quantidade previstas e se as contagens de encerramento batem.

Fica de fora o conteúdo de negócio: as regras `REGRA_...` que comparam valores, cruzam registros ou calculam o IRPJ e a CSLL (mudança 09). Com isso, depois desta mudança o veredito pode ser `recusado`, com os erros e o que corrigir, ou `parcial`. Ele ainda não pode ser `conforme`: faltam as regras de negócio (09) e a análise com LLM (10), e o status só fica `concluida` com todas as etapas obrigatórias (P05).

O catálogo parte da versão em Word do Manual do Leiaute 12, porque a extração do PDF desalinha a coluna de tipo das tabelas (`docs/fontes-oficiais.md`). O assistente extrai, e Kaio Macedo aprova (E02).

## Goals / Non-Goals

**Goals:**
- Catálogo versionado do leiaute 12: registros, campos e a fonte de cada um, gerado da versão em Word e revisável.
- Validadores determinísticos da estrutura, parametrizados pelo catálogo, com testes de caso válido e inválido.
- Achados de erro estrutural com regra, registro, linha, campo, valor encontrado, orientação e página do manual.
- Lista do que a etapa 4 não verificou, por grupo.
- Arquivos sintéticos para o gabarito no programa oficial (decisão 6).

**Non-Goals:**
- Regras `REGRA_...` de conteúdo, cruzamentos e apuração (09).
- Catálogos dos leiautes 7 a 11 (D7).
- Tabelas externas e dinâmicas (E04).
- Corrigir ou gerar o arquivo (constitution, 1.3).

## Decisions

### D1. Catálogo como dado versionado (REQ-ESTRUT-001, 002; E02)

- **Gerador:** `.github/scripts/catalogo/gerar-catalogo.py`, em Python só com a biblioteca padrão (`zipfile` e `xml.etree`), sem pacote novo. Ele baixa a versão em Word do endereço oficial, confere o SHA-256 registrado em `docs/fontes-oficiais.md` e lê as tabelas de cada registro.
- **Saída:** `src/Api.Agente/Dominio/Leiautes/ecf-leiaute-12.json`, embutido no assembly, com a versão do catálogo, o hash da fonte e, para cada registro: código, nome, bloco, nível, ocorrência, registro pai e página; para cada campo: número, nome, tipo, tamanho, decimais, valores válidos, obrigatoriedade e página.
- **Revisão:** o gerador também escreve `docs/catalogo/ecf-leiaute-12.md`, uma tabela legível por registro, que é o que Kaio Macedo revisa no PR (E02). Diferença entre o JSON e o Word é defeito do gerador, corrigido nele, nunca no JSON à mão.
- **Teste do catálogo:** cada registro e cada campo tem página; cada campo tem tipo conhecido; cada registro filho tem pai existente. Regra sem página é recusada com "fonte ausente" (REQ-ESTRUT-001).
- **Versão:** `ecf-l12-<versão do catálogo>`, gravada no envelope em `versoes.validadores`, hoje `nenhum` (P19).

### D2. Regras estruturais (E01)

Medido na versão em Word do Leiaute 12: 188 registros em 18 blocos e 1.122 campos. Nas tabelas de campos há 3.805 verificações que se fazem só com o próprio campo. Entre as 819 aplicações de regras `REGRA_...` (389 códigos), 113 são de estrutura e 698 de conteúdo de negócio; a classificação de cada uma vai para revisão junto com o catálogo.

**Proposta para a primeira entrega (E01):**

| Regra | O que verifica | Fonte (leiaute 12) | Classe |
| --- | --- | --- | --- |
| `EST-ARQ-001` | A linha começa e termina com `\|` | p. 32 | `impeditiva` |
| `EST-ARQ-002` | O código do registro existe no leiaute | Tabela de Registros, p. 43 a 58 | `impeditiva` |
| `EST-ARQ-003` | A linha não tem mais campos que o registro | p. 32 e tabela do registro | `impeditiva` |
| `EST-CAMPO-001` | Campo obrigatório preenchido | Coluna "Obrigatório" da tabela | `impeditiva` |
| `EST-CAMPO-002` | Tipo: `C` sem `\|` nem caracteres de 0 a 31; `N` só algarismos e vírgula; `NS` com sinal opcional | p. 33 | `impeditiva` |
| `EST-CAMPO-003` | Tamanho máximo; `C` sem tamanho até 255; tamanho exato nos campos por posição | p. 33, 59, 73 e 74 | `impeditiva` |
| `EST-CAMPO-004` | Casas decimais: nenhuma com "-", no máximo as indicadas com número | p. 34 e 59 | `impeditiva` |
| `EST-CAMPO-005` | Data `ddmmaaaa` válida no calendário, nos campos `DT_` do tipo `N` 8 | p. 34 | `impeditiva` |
| `EST-CAMPO-006` | Valor dentro da lista ou do intervalo da coluna "Valores Válidos"; nos campos por posição, cada caractere | p. 37, 73, 74 e tabela do registro | `impeditiva` |
| `EST-HIER-001` | Registro filho com o pai aberto antes dele | p. 32 e 59 | `impeditiva` |
| `EST-OCOR-001` | Os 13 registros obrigatórios na entrada e na saída estão presentes | p. 43 e 58 | `impeditiva` |
| `EST-OCOR-002` | Ocorrência máxima por registro pai, onde cabeçalho e Tabela de Registros concordam | p. 43 e 59 | `impeditiva` |
| `REGRA_OCORRENCIA_UNITARIA_ARQ` | Cada abertura de bloco (x001) aparece uma vez no arquivo | p. 70 e seguintes, tipo `Erro` | `impeditiva` |
| `EST-ORDEM-001` | Os blocos seguem a ordem da Relação de Blocos e da Tabela de Registros | p. 42 e 58 | `impeditiva` |
| `EST-TOT-001` | Quantidade de cada x990 igual aos registros do bloco, abertura e encerramento incluídos | p. 38, 41, 42 e 108; decisão E06 | `impeditiva` no Bloco 0 e nos blocos com `IND_DAD` igual a 0, exceto C e E; `observacao` nos demais |
| `EST-TOT-002` | Cada registro presente aparece no 9900 com a quantidade certa, o próprio 9900 incluído | p. 616 | `observacao` |
| `EST-TOT-003` | Quantidades do 9990 e do 9999, este com o total de linhas do arquivo | p. 41, 618 e 619 | `observacao` |
| `REGRA_DUPLICIDADE_DESPREZADA` | Registro repetido com a mesma chave sob o mesmo pai, nos 59 dos 60 registros com a regra que têm chave no cabeçalho | p. 607 e demais, tipo `Aviso` | `observacao` (`baixa`) |

**Classes:** violação de leiaute é `impeditiva`, pelo critério aprovado na mudança 15 (`docs/classificacao-das-regras.md`, linha `impeditiva`, que cita as p. 33 a 36 e 43): o manual diz que erros de estrutura impedem a importação (p. 38). As contagens não têm tipo declarado no manual e, salvo a decisão E06, ficam em `observacao`. Pela E06, tomada depois do gabarito, o x990 errado de um bloco que a importação lê é `impeditiva`, porque o programa da ECF recusa o arquivo. `REGRA_DUPLICIDADE_DESPREZADA` é `Aviso` de comportamento de importação, por isso `baixa`.

**Decisões sobre o que o manual deixa em aberto (para aprovação, E01):**

- **Tamanho exato:** a p. 59 diz que o número na coluna é o tamanho exato, mas a p. 34 mostra valores de tamanho variável em campos de valor, como `1255,42`. A primeira entrega confere só o máximo; o tamanho exato fica como validação não realizada (REQ-ESTRUT-011). O gabarito mostrou que o programa só o exige em alguns campos (E09).
- **Campos finais vazios omitidos:** os exemplos do 0000 têm 15 campos, e a tabela, 16. Linha com menos campos só gera achado se faltar campo obrigatório.
- **Contagem dos encerramentos:** a p. 42 diz que cada bloco tem abertura, dados e encerramento, e a p. 41, que o arquivo vai do registro inicial ao final. Por isso as contagens incluem a abertura, o encerramento e o próprio 9999. O gabarito confirmou o x990: para o Bloco 0 de 6 linhas, com abertura e encerramento, o programa espera 6 (arquivo 09). O total do 9999 o programa não confere (E07).
- **Vírgula em `NS`:** a definição da p. 33 não cita a vírgula, mas há campos `NS` com decimais, como `9100.CONTEÚDO`. Vírgula aceita quando o campo tem decimais.
- **Pai de cada registro:** o registro anterior de nível imediatamente superior na Tabela de Registros (p. 43 a 58), pela ordem sequencial (p. 58).
- **Erros do próprio manual:** o encerramento do Bloco 9 é `9990`, como no Capítulo 4 (p. 618), e não `9099`, como na Tabela de Registros (p. 58). O Bloco S não tem registro de encerramento no manual, então nenhuma regra o exige.
- **Y800 em várias linhas:** o conteúdo RTF colado no `ARQ_RTF` pode ter quebras de linha (p. 610), e o registro termina com o campo `IND_FIM_RTF`, igual a `Y800FIM` (p. 611). As linhas entre o início do Y800 e o `Y800FIM` são lidas como um único registro. É decisão de leitura sobre uma lacuna, conferida no gabarito.

### D3. Achado (REQ-ESTRUT-003; mudança 15)

Cada violação vira um achado de erro estrutural, origem validador, com:

- `regra`: o identificador da regra de D2 e a instância, por exemplo `EST-CAMPO-002` em `0000.COD_VER`;
- `registros`: registro, linha, campo e valor encontrado, cortado em 200 caracteres (mudança 15);
- `fonte`: Manual da ECF, leiaute 12, a página da tabela do registro;
- `orientacao`: montada do catálogo, por exemplo "Preencha `COD_VER` com um dos valores válidos: 0012". A orientação vem só do catálogo, nunca de texto livre (P06);
- severidade e classe pela tabela de D2.

### D4. Limite de achados (P08)

Um arquivo muito errado pode gerar um achado por linha. `ValidacaoEstrutural:MaximoDeAchadosPorRegra` (padrão 20) e `ValidacaoEstrutural:MaximoDeAchados` (padrão 200) limitam a lista. A contagem continua: a limitação diz "N achados da regra X não listados", com o total por regra. A etapa 4 fica concluída, porque todas as regras rodaram; só a lista é resumida.

### D5. O que a etapa 4 não verifica (REQ-ESTRUT-002)

Cada item entra em `naoVerificados`, com o motivo, e a etapa 4 fica concluída com limitação, o que mantém o status `parcial` (P05):

| O que não é verificado | Quantidade | Motivo |
| --- | --- | --- |
| Regras `REGRA_...` de conteúdo: valores, cruzamentos, cálculos e preenchimento condicionado | 698 aplicações | Mudança 09 |
| Regras `REGRA_...` de existência condicionada ao valor de um campo | 12 aplicações | Mudança 09, porque dependem do conteúdo |
| Dígito verificador: `REGRA_VALIDA_CNPJ`, `REGRA_VALIDA_CPF_CNPJ` e outras | 13 aplicações | O algoritmo não está no manual (P01) |
| `REGRA_LEIAUTE_VIGENTE` e `REGRA_SEM_LEIAUTE` | 2 | Dependem de tabelas fora do manual (mudança 07, D5) |
| Valores que remetem ao campo de outro registro, como `[M010.COD_CTA_B]` | 8 campos (M305, M310, M355, M360, M500 e M510) | É cruzamento entre registros: mudança 09 |
| Valores que dependem de tabela externa ou dinâmica | 141 campos | Tabelas fora do manual, como a planilha de tabelas dinâmicas (p. 620) e as do programa (E04) |
| Tamanho exato | — | Divergência entre as p. 34 e 59 |
| Ocorrência mínima e máxima onde cabeçalho e Tabela de Registros divergem | 47 registros | Divergência do manual (REQ-ESTRUT-011) |
| Ordem entre registros irmãos | — | O manual só diz que a ordem é sequencial (p. 58); a primeira entrega confere a ordem dos blocos e a hierarquia |
| Fim de linha CR LF | — | O texto que chega pela Sofia pode ter as quebras de linha trocadas; conferir geraria achado falso |
| Campos do tipo `D` | 2 (X280) | O tipo não está definido na p. 33 |
| Duplicidade no V100 | 1 | A chave do registro está vazia no cabeçalho (p. 424) |

**Indícios de arquivo gerado (REQ-ESTRUT-012; E08 da mudança 07):** a etapa registra, em `observacoes`, os registros com obrigatoriedade de entrada "N" encontrados (18 no leiaute 12, como C040 e E010; p. 43 a 58), o 9100 e o Y750 (p. 607 e 614) e o conteúdo depois do 9999 (p. 17). Por isso a regra `EST-OCOR-001` só exige os registros obrigatórios na entrada e na saída: um arquivo gerado pelo programa tem registros que a importação não aceita, e o agente não pode saber qual dos dois recebeu. O uso desses indícios no veredito, conforme a decisão 5, fica para a mudança 09.

### D6. Leitura em uma passada (REQ-ESTRUT-004)

O validador lê o texto linha a linha, uma vez, e mantém só o que as regras de hierarquia, ocorrência e contagem precisam: a pilha de registros abertos e as contagens por registro e por bloco. Os achados saem ordenados por linha e, na mesma linha, pelo identificador da regra. Nenhuma regra depende de data, hora ou ordem de execução, então o mesmo arquivo e a mesma versão do catálogo dão os mesmos achados.

### D7. Leiautes 7 a 11

O catálogo começa pelo 12 (E06 da mudança 07). Para os leiautes 7 a 11, a etapa 4 fica não executada com a limitação "catálogo de regras do leiaute N ainda não cadastrado", e o veredito é `parcial`. Os catálogos deles exigem a versão em Word de cada manual, que ainda não foi baixada, e entram em mudança própria, com as diferenças entre leiautes medidas.

### D8. Falha do validador (REQ-ESTRUT-006)

Cada regra roda protegida. Uma exceção vira achado de falha técnica com o identificador da regra e sem detalhe do conteúdo; a etapa 4 fica com falha de causa técnica, e as demais regras continuam. Nenhum erro estrutural é atribuído ao documento por essa falha.

### D9. XSD (REQ-ESTRUT-005)

A ECF é texto delimitado, sem schema XML oficial. Nenhuma validação XSD é aplicada, e o catálogo não tem campo para schema.

### D10. Gabarito no programa oficial (E03; decisão 6)

Antes de arquivar esta mudança, o assistente baixa o programa da ECF (PGE) do portal oficial, importa os arquivos sintéticos com erros plantados, sem assinar nem transmitir, e anota o que o programa aponta. Kaio Macedo pediu em 2026-11-07 que o próprio assistente faça isso. O assistente prepara os arquivos e uma planilha com o erro esperado de cada um. Divergência entre o agente e o programa é registrada e resolvida antes do arquivamento: ou o catálogo tem defeito, ou a regra é diferente do que o manual diz, e então vira esclarecimento. O PGE é um programa de computador com interface gráfica; se ele não puder ser operado daqui, o assistente registra o motivo e o que faltou.

### D11. Cartão 1.2.0 (REQ-A2A-007)

A descrição da skill passa a dizer que o agente valida a estrutura da ECF do leiaute 12: registros, campos, hierarquia e contagens. A versão do cartão sobe para 1.2.0.

### D12. Testes (SQ-09, SQ-10)

- **Por regra:** cada regra de D2 com um caso válido e um inválido, em linhas sintéticas.
- **Gerados do catálogo:** para cada campo do leiaute 12, uma linha sintética válida e uma inválida por verificação aplicável, geradas do próprio catálogo. Confirma que nenhum campo ficou sem verificação.
- **Arquivo inteiro:** uma ECF sintética do leiaute 12 estruturalmente válida, que não gera nenhum achado estrutural, e as variações com erro plantado do gabarito (D10).
- **Reprodutibilidade:** o mesmo arquivo validado duas vezes dá os mesmos achados (REQ-ESTRUT-004).
- **Falha:** regra que lança exceção vira falha técnica (REQ-ESTRUT-006).
- **Desempenho:** ECF sintética de 4 MiB validada dentro de um tempo registrado no PR.
- **Adversarial:** instrução escrita num campo `C` não muda achados nem veredito (P10).

## Ajustes na implementação (2026-11-07)

Mudanças em relação ao plano aprovado, sem efeito nos requisitos:

- **Campo `REG`:** é conferido pela existência do registro, e a coluna de valores válidos dele só serve para registrar divergência do manual. O Registro Y681 traz `[X681]` no `REG` (p. 596), erro de digitação do manual, que vai para a lista de divergências do catálogo (REQ-ESTRUT-011).
- **Listas de valores:** só listas de valores literais, separados por ponto e vírgula ou vírgula, são conferidas; listas com faixas por extenso, como `X370.TIPO_DEMAIS`, ficam sem verificação.
- **Tamanho de `N` e `NS`:** conta só os algarismos, sem sinal e vírgula, porque o manual não diz se eles entram no tamanho. Evita achado falso.
- **`NS` com vírgula:** o tipo aceita a vírgula; quem decide as casas decimais é `EST-CAMPO-004`.
- **Datas:** campos `DT_` de tamanho 8, do tipo `N` ou `C`.
- **`EST-OCOR-002` e as aberturas de bloco:** para os x001, vale `REGRA_OCORRENCIA_UNITARIA_ARQ`, para não dar dois achados do mesmo problema.
- **`9999.QTD_LIN`:** aceita a contagem de linhas físicas ou de registros, que só diferem quando o Y800 ocupa várias linhas.
- **Etapa 5 no fluxo:** a etapa 5 roda depois da etapa 4 concluída, com as consultas da mudança 06 (tarefa 5.3). Sem achado, ela fica não aplicável.
- **Gerador e arquivos de teste:** `gerar-catalogo.sh` baixa os originais e confere os hashes; `gerar-ecf-sintetica.py` faz a ECF estruturalmente válida dos testes; `gerar-gabarito.py` faz os 12 arquivos do gabarito, com `erros-esperados.md`.

### Correções da verificação em produção (2026-11-07)

A verificação da tarefa 7.1 achou três defeitos, corrigidos antes do gabarito no PGE:

- **Motivo das etapas não realizadas (REQ-RESULT-006):** a etapa 5 desligada aparecia como "ainda não implementada nesta versão", motivo que a mudança 13 dava a qualquer etapa não executada sem falha anterior. Agora a etapa que existe e não rodou guarda o motivo: busca desligada, limite diário ou manual ausente na etapa 5; leiaute sem catálogo, versão não suportada ou documento cortado na etapa 4, e então a etapa 5 depende dela. "Ainda não implementada" fica para as etapas 6 a 8. As limitações de REQ-CONSULTA-007 e da mudança 07 continuam com o mesmo texto.
- **Contagem do 9900 com código fora do leiaute:** a linha com código inexistente, já acusada por `EST-ARQ-002`, não entrava na contagem real, e o achado de `EST-TOT-002` dizia que o arquivo tinha 0 ocorrências de um registro que estava nele. Agora ela conta.
- **Gabarito com mais de um erro:** o gerador contava como uma linha os registros inseridos juntos, o que plantava erros de contagem nos arquivos 07 e 11; o J051 do arquivo 11 vinha sem o campo obrigatório `COD_CTA_REF`; o arquivo 06 mantinha o 9999 nas contagens; e a data inválida do arquivo 02 estava no `DT_FIN`, que a etapa 3 usa para identificar o leiaute, de modo que a etapa 4 nem rodava. A planilha passou a listar também as regras que acusam a consequência do erro plantado (arquivo 05), e o teste `Gabarito` confere a planilha pelo serviço de análise, da recepção à etapa 4, e não mais só pelo validador.

## Gabarito no programa da ECF (2026-11-08)

Rodado pelo assistente, com computer use, no programa da ECF 12.2.8, descritor 12003.1, baixado da área de downloads da Receita Federal (`docs/fontes-oficiais.md`). Nada foi assinado nem transmitido. A resposta do programa a cada arquivo está na última coluna de `tests/Api.Agente.Tests/Dados/ecf/gabarito/erros-esperados.md`.

O programa valida em duas fases, como a p. 38 descreve. A **importação** cobre a estrutura e já aplica parte das regras de campo; a maioria dos erros interrompe a importação, com "Importação não realizada". A **validação** roda depois de importar, com as regras de negócio. Os erros da validação do arquivo 00 são de conteúdo ou do que a importação não exige, como a recuperação da ECD e os registros 0930, J050, K155, K355, L200 e Y600, todos facultativos na entrada pela Tabela de Registros. Isso confirma a decisão da D5 de `EST-OCOR-001` só exigir os 13 registros obrigatórios na entrada e na saída.

| Arquivo | Regras do agente | Programa | Resultado |
| --- | --- | --- | --- |
| 00 | nenhuma | Importou | Concorda |
| 01 | `EST-CAMPO-006` | Recusou, linha 1 | Concorda |
| 02 | `EST-CAMPO-005` | Recusou ao abrir o arquivo | Concorda |
| 03 | `EST-ARQ-002` | Recusou, linha 7 | Concorda |
| 04 | `EST-ARQ-003` | Recusou, linha 2 | Concorda |
| 05 | `EST-ARQ-001`, `EST-OCOR-001` | Recusou, linha 7, e parou nesse erro | Concorda com a causa; a consequência o programa não chega a citar |
| 06 | `EST-OCOR-001` | **Importou, sem apontamento** | Diverge: E05 |
| 07 | `REGRA_OCORRENCIA_UNITARIA_ARQ` | Recusou, linha 3, e também acusou a ordem | Concorda; a ordem dos irmãos fica no E08 |
| 08 | `EST-TOT-003` | **Importou, sem apontamento** | Diverge: E07 |
| 09 | `EST-TOT-001` | Recusou, linha 6 | Concorda; o agente também recusa, pela decisão E06 |
| 10 | `EST-TOT-002` | **Importou, sem apontamento** | Diverge: E07 |
| 11 | `EST-HIER-001` | Recusou, linha 11; a linha 12 é consequência | Concorda |

**Achado sobre a base.** A ECF sintética anterior foi recusada pelo programa antes de validar, com `FORMA_TRIB_PER` igual a `0` ("O tamanho do campo está incorreto", 4 posições). Ela preenchia campos opcionais com o primeiro valor da lista, o que o programa recusou em `MES_BAL_RED`, `TIP_ESC_PRE`, `IND_RED_ISEN` e `IND_REC_RECEITA`. `gerar-ecf-sintetica.py` agora usa os exemplos de preenchimento do manual (p. 79, 94 e 102), com os indicadores do 0020 em `N` e sem o `IND_REC_RECEITA`, e o programa a importa.

**Corrigido no gerador do catálogo e no validador, com fonte no manual e no programa:**

- **Campos por posição (falso positivo, REQ-ESTRUT-014):** `FORMA_TRIB_PER` é "para cada trimestre no formato XXXX" (p. 73), e `MES_BAL_RED`, "no formato XXXXXXXXXXXX" (p. 74). O catálogo guardava a lista como valor inteiro, então uma ECF correta com `RRRR` recebia `EST-CAMPO-006` e o veredito `recusado`. Agora cada caractere é conferido na lista, com o tamanho exato. O programa confirma os dois lados: recusa `0` por tamanho e `RRRX` por valor, e aceita `RRRR`.
- **`FORMA_TRIB` igual a 10 (falso positivo, REQ-ESTRUT-011):** a coluna diz tamanho 1, e a lista traz `10` (p. 71 e 72). O programa aceita o valor, e o agente acusava `EST-CAMPO-003`. Vale a lista, e a divergência fica no catálogo (REQ-ESTRUT-011).
- **Versão:** catálogo `ecf-l12-1.1.0`.

**Para a mudança 09:** o programa recusa na importação valores incoerentes entre si que o agente ainda não confere, como `TIP_ESC_PRE` e `IND_REC_RECEITA` com `FORMA_TRIB` igual a 1 e `MES_BAL_RED` que não combina com `FORMA_TRIB_PER`. Essas regras estão em `naoVerificados`. Uma ECF com veredito `parcial` no agente ainda pode ser recusada pelo programa.

**Divergência entre as fontes:** o exemplo de preenchimento da p. 79 traz `IND_REC_RECEITA` igual a 1 com `FORMA_TRIB` igual a 1, e o programa recusa ("Campo não deve ser preenchido se a forma de tributação for igual a 1, 2, 6, 8 ou 9").

**Em produção, depois do PR #30:** os 12 arquivos deram exatamente as regras da planilha, de 0,3 a 2,5 s por pedido, com o catálogo `ecf-l12-1.1.0`. `RRRX` dá `EST-CAMPO-006`, e `FORMA_TRIB` igual a 10 não acusa tamanho.

**Decisões de Kaio Macedo (2026-11-08):** E05 a E09 do `proposal.md`, com as propostas do assistente. Só a E06 muda o comportamento.

**Alcance da E06, testado depois da decisão:** o `0990` e o `J990` já eram recusados, e o programa recusou também os blocos J a Y com `IND_DAD` igual a 0 (13 erros de "Quantidade de linhas do bloco"). Importou sem apontamento o `C990` e o `9990` errados, e os blocos J a Y com `IND_DAD` igual a 1. A regra segue esse recorte, para o agente não recusar o que o programa aceita. Com o catálogo `ecf-l12-1.2.0`, o `9990` também passou para `EST-TOT-003`, como a D2 sempre disse: o validador o conferia em `EST-TOT-001`.

### Revisão do catálogo (2026-11-08)

Antes da revisão de Kaio Macedo (tarefa 1.4), o assistente conferiu o catálogo por dois caminhos independentes do gerador:

- **Segunda leitura do Word:** um leitor escrito à parte conferiu número, nome, tipo, tamanho, decimal, valores e obrigatoriedade dos 1.122 campos, sem nenhuma diferença.
- **Descritor do programa:** comparado com `descritor/escrituracao/estrutura12003/v1/descritor.xml`, de `lib/br.gov.serpro.sped.irpjpva/irpjpva-infra.jar`, no programa da ECF 12.2.8. Os 188 registros existem nele, na mesma ordem e com a mesma quantidade de campos, na mesma posição.

**Defeito corrigido (catálogo `ecf-l12-1.3.0`):** o gerador tomava como valor literal a referência da coluna de valores válidos, como `[M010.COD_CTA_B]`, em 8 campos dos registros M305, M310, M355, M360, M500 e M510. O agente recusava toda linha real desses registros, porque só aceitava o texto `M010.COD_CTA_B`. A referência agora é cruzamento, da mudança 09, e consta de `naoVerificados`.

## Dimensionamento

Sem provedor pago. Uma passada linear sobre até 4 MiB de texto. O catálogo do leiaute 12 tem 188 registros e 1.122 campos, algumas dezenas de KB em JSON.

## Risks / Trade-offs

- **Gerador do catálogo:** um defeito nele vira dezenas de achados falsos. Por isso a revisão humana do `.md` gerado, os testes do catálogo e o gabarito no PGE (D10).
- **Sem `conforme` ainda:** a Sofia vai receber `recusado` ou `parcial`. É o comportamento correto até as mudanças 09 e 10 (P05), e o cartão não promete mais que isso.
- **Leiautes 7 a 11 sem validação:** declarados como limitação (D7).

## Migration Plan

1. Deploy da Api.Agente com o catálogo embutido. Nenhuma migração de banco.
2. Verificação em produção com ECF sintéticas: uma válida e algumas com erro plantado.
3. Gabarito no PGE (D10) antes do arquivamento.
4. Volta: reverter o deploy no Vercel.

## Open Questions

- Nenhuma. O gabarito (D10) foi feito em 2026-11-08, e Kaio Macedo decidiu os esclarecimentos E05 a E09. Para arquivar, faltam as tarefas 1.4 e 7.3.

## Skills aplicáveis

- `testes-agentes-ia`: D12.
- `sofia-a2a-contrato`: cartão 1.2.0 (D11).
- `github-versionamento`: branch, commits e PR.

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Atende | Cada regra e cada campo do catálogo com a página do manual, gerados da versão em Word com hash conferido (D1). Tabelas externas fora, até terem fonte registrada (D5). |
| P02 | Atende | Validadores determinísticos com casos válido e inválido; nada em prompt (D2, D12). |
| P03 | Atende | Etapa 4 com estado registrado; não executada para leiautes sem catálogo (D7). |
| P04 | Atende | Violação é erro estrutural; exceção do validador é falha técnica (D3, D8). |
| P05 | Atende | Validações não implementadas listadas; sem `conforme` antes das mudanças 09 e 10 (D5). |
| P06 | Atende | Registro, linha, campo, valor, regra e página em cada achado; orientação do catálogo (D3). |
| P07 | Não se aplica | Sem LLM. |
| P08 | Atende | Lista do que não foi verificado e resumo dos achados acima do limite (D4, D5). |
| P09 | Atende | Achados graves e validação não concluída exigem revisão humana, pela regra da mudança 15. |
| P10 | Atende | Conteúdo dos campos é dado; teste adversarial (D12). |
| P11 | Não se aplica | Sem recuperação. |
| P12 | Não se aplica | Sem LLM. |
| P13 | Atende | Versão do catálogo e demais metadados preenchidos pelo sistema (D1). |
| P14 | Não se aplica | Sem ferramentas. |
| P15 | Atende | Catálogo na Api.Agente; nenhum acesso à Api.RAG. |
| P16 | Atende | Regras no domínio; gerador sem pacote novo (D1). |
| P17 | Não se aplica | Sem provedor pago. |
| P18 | Não se aplica | Sem memória. |
| P19 | Atende | Versão do catálogo e hash da fonte no envelope (D1). |
| P20 | Atende | Esclarecimentos e tarefas ligadas a requisitos antes do código. |
