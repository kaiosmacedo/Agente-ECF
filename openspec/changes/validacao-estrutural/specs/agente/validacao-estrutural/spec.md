## Purpose

Validar de forma determinística a estrutura, os registros e os campos do arquivo da ECF segundo o leiaute oficial da versão identificada, com cada regra ligada à sua fonte.

## ADDED Requirements

### Requirement: REQ-ESTRUT-001 Regra com fonte e validador
Cada regra estrutural SHALL ter identificador, fonte (documento, versão e localização no manual ou, nos ajustes da decisão E10, no programa da ECF) e um validador determinístico com testes de caso válido e inválido. Regra sem localização nessas fontes MUST NOT ser implementada.
Fonte: Manual da ECF, leiaute 12 (23/07/2026); localização registrada por regra no catálogo (E02).

#### Scenario: Regra sem localização
- **GIVEN** uma regra candidata sem localização no manual
- **WHEN** o catálogo é validado
- **THEN** a regra é recusada com o motivo "fonte ausente"

### Requirement: REQ-ESTRUT-002 Catálogo de validações obrigatórias por versão
O catálogo SHALL declarar quais regras são obrigatórias para cada versão do leiaute. Regras obrigatórias ainda não implementadas MUST constar do resultado como validações não realizadas.
Fonte: Manual da ECF, leiaute 12 (23/07/2026); conjunto obrigatório definido em E01.

#### Scenario: Regra obrigatória pendente
- **GIVEN** uma regra obrigatória do leiaute 12 sem validador implementado
- **WHEN** um arquivo do leiaute 12 é validado
- **THEN** o resultado lista a regra como validação não realizada
- **AND** o status não é de análise concluída

### Requirement: REQ-ESTRUT-003 Achado de erro estrutural com evidência
Cada violação SHALL gerar um achado da categoria erro estrutural com o identificador da regra, a fonte, o registro, a linha e o campo, quando disponíveis.

#### Scenario: Violação de regra estrutural
- **GIVEN** um arquivo sintético que viola a regra de teste `EST-TESTE-001` na linha 42
- **WHEN** a etapa 4 executa
- **THEN** existe um achado de erro estrutural citando `EST-TESTE-001`, a fonte e a linha 42

### Requirement: REQ-ESTRUT-004 Resultado reproduzível
A validação SHALL produzir o mesmo conjunto de achados para o mesmo arquivo e a mesma versão do catálogo de regras.

#### Scenario: Duas execuções do mesmo arquivo
- **WHEN** o mesmo arquivo é validado duas vezes com a mesma versão do catálogo
- **THEN** os achados são idênticos nas duas execuções

### Requirement: REQ-ESTRUT-005 XSD somente para XML com schema oficial
A validação por XSD SHALL ser aplicada somente a documentos XML para os quais exista schema oficial cadastrado; os demais formatos MUST usar validadores compatíveis com seu leiaute.

#### Scenario: Arquivo texto
- **GIVEN** um arquivo em formato texto
- **WHEN** a etapa 4 executa
- **THEN** nenhuma validação XSD é aplicada

### Requirement: REQ-ESTRUT-006 Falha do validador não é erro do documento
Uma exceção ou falha interna de validador SHALL gerar achado da categoria falha técnica e MUST NOT gerar erro estrutural.

#### Scenario: Validador com exceção
- **GIVEN** um validador que lança exceção durante a execução
- **WHEN** a etapa 4 executa
- **THEN** existe um achado de falha técnica para o validador
- **AND** nenhum erro estrutural é atribuído ao documento por essa falha

### Requirement: REQ-ESTRUT-007 Linha e registro do leiaute
Cada linha SHALL começar e terminar com `|`, ter um código de registro do leiaute e no máximo os campos que o registro define. A violação SHALL gerar achado de classe `impeditiva`.
Fonte: Manual da ECF, leiaute 12 (23/07/2026), p. 32 (delimitador, primeira coluna e ordem dos campos) e Tabela de Registros, p. 43 a 58.

#### Scenario: Registro inexistente
- **GIVEN** uma ECF sintética do leiaute 12 com a linha `|Z999|1|` na linha 7
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo de registro inexistente na linha 7

#### Scenario: Campos a mais
- **GIVEN** uma linha do Registro 0001 com três campos, quando o leiaute define dois
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo de quantidade de campos nessa linha

### Requirement: REQ-ESTRUT-008 Campos conforme a tabela do registro
Cada campo SHALL respeitar a obrigatoriedade, o tipo (`C`, `N` ou `NS`), o tamanho máximo, as casas decimais, o formato de data `ddmmaaaa` e os valores válidos listados na tabela do registro. A violação SHALL gerar achado de classe `impeditiva`, com a orientação montada pelo catálogo.
Fonte: Manual da ECF, leiaute 12 (23/07/2026), p. 33 a 37 e 59, e a tabela de campos de cada registro.

#### Scenario: Valor fora da lista
- **GIVEN** um Registro 0000 sintético com `IND_SIT_INI_PER` igual a `9`
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo citando o campo `IND_SIT_INI_PER`, o valor `9`, a p. 63 e os valores válidos

#### Scenario: Data inválida
- **GIVEN** um Registro 0000 sintético com `DT_FIN` igual a `31022025`
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo de data inválida no campo `DT_FIN`

#### Scenario: Coluna de valores que remete a outro registro
- **GIVEN** um Registro M305 sintético com `COD_CTA_B` igual a um código da empresa, e a coluna de valores válidos do campo traz `[M010.COD_CTA_B]` (p. 251)
- **WHEN** a etapa 4 executa
- **THEN** nenhum achado de valor é produzido para o campo, porque a referência é cruzamento entre registros, da mudança 09
- **AND** o resultado lista esses campos em `naoVerificados`

### Requirement: REQ-ESTRUT-009 Hierarquia, ocorrência e ordem dos blocos
Cada registro filho SHALL ter o registro pai aberto antes dele; os registros obrigatórios na entrada e na saída SHALL estar presentes; as ocorrências SHALL respeitar o máximo do registro; e os blocos SHALL seguir a ordem do manual. A violação SHALL gerar achado de classe `impeditiva`.
Fonte: Manual da ECF, leiaute 12 (23/07/2026), p. 32, 42, 43, 58, 59 e 70 (`REGRA_OCORRENCIA_UNITARIA_ARQ`).

#### Scenario: Arquivo sem o Registro 9999
- **GIVEN** uma ECF sintética do leiaute 12 sem o Registro 9999 e sem o marcador de corte
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo de registro obrigatório ausente, citando o 9999

#### Scenario: Filho sem pai
- **GIVEN** uma ECF sintética com um Registro J051 sem J050 antes dele
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo de hierarquia na linha do J051

### Requirement: REQ-ESTRUT-010 Contagens de encerramento
As quantidades dos registros x990, 9900, 9990 e 9999 SHALL conferir com as linhas do arquivo. A divergência SHALL gerar achado de classe `observacao`, porque o manual não declara tipo para ela, salvo o caso de REQ-ESTRUT-015.
Fonte: Manual da ECF, leiaute 12 (23/07/2026), p. 41 e 42 (composição do arquivo e dos blocos), 108, 616, 618 e 619.

#### Scenario: Total de linhas divergente
- **GIVEN** uma ECF sintética com 40 linhas e `9999.QTD_LIN` igual a `41`
- **WHEN** a etapa 4 executa
- **THEN** existe um achado de classe `observacao` citando o campo `QTD_LIN`, o valor `41` e a quantidade encontrada

### Requirement: REQ-ESTRUT-011 Divergência do manual não vira achado
Quando o próprio manual divergir sobre uma verificação, ela MUST NOT gerar achado e SHALL constar como validação não realizada, com as páginas da divergência, até a decisão registrada.

#### Scenario: Ocorrência divergente entre cabeçalho e Tabela de Registros
- **GIVEN** o Registro C155, com ocorrência `1:N` no cabeçalho e `[0;N]` na Tabela de Registros
- **WHEN** a etapa 4 executa uma ECF sem C155
- **THEN** nenhum achado de ocorrência mínima é produzido para o C155
- **AND** a verificação consta como não realizada, com as duas páginas

#### Scenario: Valor da lista mais longo que a coluna Tamanho
- **GIVEN** um Registro 0010 sintético com `FORMA_TRIB` igual a `10`, valor da lista de um campo de tamanho 1 (p. 71 e 72)
- **WHEN** a etapa 4 executa
- **THEN** nenhum achado de tamanho é produzido para o campo
- **AND** a divergência consta do catálogo

### Requirement: REQ-ESTRUT-012 Indícios de arquivo gerado pelo programa
A etapa 4 SHALL registrar, sem afirmar o tipo do arquivo, os indícios que o manual documenta de arquivo gerado pelo programa da ECF: registros que não devem existir na importação, registros gerados pelo sistema e conteúdo depois do 9999. Esses registros MUST NOT gerar achado por estarem presentes.
Fonte: Manual da ECF, leiaute 12 (23/07/2026), p. 17, 41, 43 a 58, 607 e 614.

#### Scenario: ECF com o Bloco C recuperado
- **GIVEN** uma ECF sintética com o Registro C040
- **WHEN** a etapa 4 executa
- **THEN** o resultado registra o indício de que o Registro C040 não deve existir no arquivo de importação, com a página
- **AND** nenhum achado é produzido pela presença do C040

### Requirement: REQ-ESTRUT-013 Lista de achados limitada e declarada
A lista de achados SHALL respeitar os máximos configurados por regra e no total, e a contagem dos achados não listados MUST constar das limitações, por regra. O limite MUST NOT mudar o veredito: um achado impeditivo SHALL entrar na lista antes dos demais.

#### Scenario: Observações antes de um erro impeditivo
- **GIVEN** uma ECF sintética com 25 achados de classe `observacao` e, depois deles, um impeditivo da mesma regra
- **WHEN** a etapa 4 executa
- **THEN** o veredito é `recusado`, e a limitação informa os achados não listados

#### Scenario: Muitos achados da mesma regra
- **GIVEN** o máximo de 20 achados por regra e uma ECF sintética com 50 linhas que violam a mesma regra
- **WHEN** a etapa 4 executa
- **THEN** 20 achados dessa regra são listados
- **AND** a limitação informa os 30 achados não listados

### Requirement: REQ-ESTRUT-014 Campos preenchidos por posição
Um campo preenchido por posição, como "no formato XXXX", SHALL ter exatamente o tamanho da coluna, e cada caractere SHALL estar nos valores válidos. A violação SHALL gerar achado de classe `impeditiva`.
Fonte: Manual da ECF, leiaute 12 (23/07/2026), p. 73 (`0010.FORMA_TRIB_PER`) e 74 (`0010.MES_BAL_RED`); programa da ECF 12.2.8, que recusa `0` por tamanho e `RRRX` por valor e aceita `RRRR`.

#### Scenario: Campo por posição
- **GIVEN** um Registro 0010 sintético com `FORMA_TRIB_PER` igual a `RRRR`
- **WHEN** a etapa 4 executa
- **THEN** nenhum achado é produzido para o campo

#### Scenario: Campo por posição com tamanho errado
- **GIVEN** um Registro 0010 sintético com `FORMA_TRIB_PER` igual a `0`
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo de tamanho no campo `FORMA_TRIB_PER`, que precisa de 4 caracteres

#### Scenario: Campo por posição com caractere fora da lista
- **GIVEN** um Registro 0010 sintético com `FORMA_TRIB_PER` igual a `RRRX`
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo de valor no campo `FORMA_TRIB_PER`

### Requirement: REQ-ESTRUT-015 Contagem de bloco que a importação lê
A divergência no x990 de um bloco que a importação lê SHALL gerar achado de classe `impeditiva`, porque o programa da ECF recusa o arquivo. A importação lê o Bloco 0 e os blocos com `IND_DAD` igual a 0, exceto C e E, e o programa não confere o S990 nem o 9990.
Fonte: Manual da ECF, leiaute 12 (23/07/2026), p. 38 (erro de estrutura impede a importação) e 41 (os blocos C e E não são importados); programa da ECF 12.2.8; decisão E06, de Kaio Macedo.

#### Scenario: Contagem errada no Bloco 0
- **GIVEN** uma ECF sintética com o Bloco 0 de 6 linhas e `0990.QTD_LIN` igual a `9`
- **WHEN** a etapa 4 executa
- **THEN** existe um achado de classe `impeditiva` da regra `EST-TOT-001`

#### Scenario: Contagem errada em bloco sem dados
- **GIVEN** uma ECF sintética com `K001.IND_DAD` igual a `1` e `K990.QTD_LIN` errado
- **WHEN** a etapa 4 executa
- **THEN** o achado da regra `EST-TOT-001` tem classe `observacao`

#### Scenario: Contagem errada no Bloco C
- **GIVEN** uma ECF sintética com `C990.QTD_LIN` errado
- **WHEN** a etapa 4 executa
- **THEN** o achado da regra `EST-TOT-001` tem classe `observacao`, porque o Bloco C não é importado

### Requirement: REQ-ESTRUT-016 Divergência entre o manual e o programa da ECF
Onde o manual e o programa da ECF divergem, o agente MUST NOT recusar o que o programa aceita: o valor aceito SHALL passar sem achado, e o que só o manual obriga SHALL gerar `observacao`. O catálogo SHALL citar o programa como fonte. A única exceção é a falta do Registro 9999, que MUST continuar `impeditiva` (E05).
Fonte: decisão E10 (2026-11-08); programa da ECF 12.2.8, descritor 12003 v1; `tests/Api.Agente.Tests/Dados/ecf/programa/resultados.md`.

#### Scenario: Registro que só o programa tem
- **GIVEN** uma ECF sintética com o Bloco S e o Registro `S990`, que o manual não cita
- **WHEN** a etapa 4 executa
- **THEN** nenhum achado de `EST-ARQ-002` é produzido, e a contagem errada do `S990` gera só `observacao`

#### Scenario: Valor aceito pelo programa
- **GIVEN** uma ECF sintética com `W200.VL_LUC_ACUM` igual a `-1000`, `0000.NUM_REC` com 41 caracteres e `0010.TIP_ENT` igual a `16`
- **WHEN** a etapa 4 executa
- **THEN** nenhum achado é produzido para esses campos

#### Scenario: Valor que o programa também recusa
- **GIVEN** uma ECF sintética com `0010.TIP_ENT` igual a `17`, ou `W200.VL_LUC_ACUM` com letras
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo no campo

#### Scenario: Campo obrigatório só no manual
- **GIVEN** um Registro 0930 sintético com `FONE` vazio
- **WHEN** a etapa 4 executa
- **THEN** existe um achado de classe `observacao` que cita o manual e o programa, e o arquivo não é recusado por ele

#### Scenario: Registro que o programa preenche sozinho
- **GIVEN** uma ECF sintética sem `C001`, `C990` e `E001`
- **WHEN** a etapa 4 executa
- **THEN** existem achados de classe `observacao`, e o veredito não é `recusado` por causa deles

#### Scenario: Registro obrigatório nos dois
- **GIVEN** uma ECF sintética sem o Registro `0030`
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo de `EST-OCOR-001`

#### Scenario: Falta do Registro 9999
- **GIVEN** uma ECF sintética sem o Registro `9999`, que o programa importa sem apontamento
- **WHEN** a etapa 4 executa
- **THEN** existe um achado impeditivo de `EST-OCOR-001`, pela decisão E05
