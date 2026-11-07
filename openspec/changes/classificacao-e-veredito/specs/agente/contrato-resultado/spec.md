## ADDED Requirements

### Requirement: REQ-RESULT-007 Classe do achado
Cada achado SHALL ter uma classe calculada pelo sistema a partir da severidade: `critica` é `impeditiva`, `alta` é `grave`, e `media` ou `baixa` é `observacao`. A classe MUST NOT substituir a categoria.

#### Scenario: Achado com severidade alta
- **GIVEN** um achado com severidade `alta`
- **WHEN** o resultado é montado
- **THEN** a classe do achado é `grave`

### Requirement: REQ-RESULT-008 Tipo oficial da regra
Um achado produzido por regra do manual SHALL informar o código e o tipo (`Erro`, `Aviso` ou não declarado) e, quando o manual o declarar, o nível da regra (1.1, 1.2, 2 ou 3). Regra do tipo `Erro` MUST ter severidade `critica`, e regra de outro tipo MUST NOT ter severidade `critica`. Fonte: Manual da ECF, leiaute 12, capítulo 3, página 38.

#### Scenario: Erro do manual com severidade menor
- **WHEN** um achado de regra do tipo `Erro` é registrado com severidade `media`
- **THEN** o registro é rejeitado

#### Scenario: Aviso do manual como impeditivo
- **WHEN** um achado de regra do tipo `Aviso` é registrado com severidade `critica`
- **THEN** o registro é rejeitado

#### Scenario: Regra sem tipo declarado
- **WHEN** um achado de regra sem tipo declarado é registrado com severidade `alta`
- **THEN** o achado é aceito e informa o tipo como não declarado

### Requirement: REQ-RESULT-009 Só validador impede
Somente achados das categorias erro estrutural e inconsistência de negócio MAY ter severidade `critica`. Indício da IA, ausência de evidências e falha técnica MUST NOT ser impeditivos. Sem regra do tipo `Erro`, só a violação de leiaute MAY ser impeditiva.

#### Scenario: Indício da IA como impeditivo
- **WHEN** um achado com categoria indício da IA é registrado com severidade `critica`
- **THEN** o registro é rejeitado

#### Scenario: Inconsistência de negócio sem regra
- **WHEN** uma inconsistência de negócio sem regra do manual é registrada com severidade `critica`
- **THEN** o registro é rejeitado

### Requirement: REQ-RESULT-010 Veredito calculado
O resultado SHALL trazer um veredito calculado pelo sistema, nesta ordem: `recusado`, se houver achado impeditivo; `nao-avaliado`, se o status for `entrada-rejeitada` ou `falha-tecnica`; `parcial`, se o status for `parcial`; `conforme-com-ressalvas`, se o status for `concluida` e houver achados; e `conforme`, se o status for `concluida` e não houver achados. O LLM MUST NOT definir nem alterar o veredito, e `conforme` MUST NOT ser declarado com validação obrigatória não concluída (P05).

#### Scenario: Regra impeditiva violada com validações pendentes
- **GIVEN** uma execução com um achado impeditivo e validações obrigatórias ainda não concluídas
- **WHEN** o resultado é montado
- **THEN** o veredito é `recusado`

#### Scenario: Sem achados e com validação pendente
- **GIVEN** uma execução sem achados cuja etapa 4 não foi executada
- **WHEN** o resultado é montado
- **THEN** o veredito é `parcial`
- **AND** o resultado não declara o documento conforme

#### Scenario: Entrada rejeitada
- **GIVEN** uma execução cuja etapa 2 rejeitou o arquivo
- **WHEN** o resultado é montado
- **THEN** o veredito é `nao-avaliado`

#### Scenario: Validações concluídas com avisos
- **GIVEN** uma execução com todas as validações obrigatórias concluídas e um achado da classe `observacao`
- **WHEN** o resultado é montado
- **THEN** o veredito é `conforme-com-ressalvas`

#### Scenario: Validações concluídas sem achados
- **GIVEN** uma execução com todas as validações obrigatórias concluídas e sem achados
- **WHEN** o resultado é montado
- **THEN** o veredito é `conforme`
- **AND** a síntese informa que o resultado vale para as validações realizadas e é apoio à conferência

### Requirement: REQ-RESULT-011 Detalhe do achado
Um achado SHALL informar, além do que REQ-RESULT-003 exige, a orientação de correção e a fonte (documento, versão e localização) quando vier de regra, e, por registro envolvido, o valor encontrado quando a regra o usar, com até 200 caracteres. A orientação MUST NOT alterar o arquivo analisado. O resultado SHALL trazer a contagem de achados por classe.

#### Scenario: Erro em um campo
- **GIVEN** uma regra do manual violada em um campo de um registro
- **WHEN** o resultado é montado
- **THEN** o achado informa o código da regra, o registro, a linha, o campo, o valor encontrado, a orientação e a fonte
- **AND** a contagem por classe inclui esse achado

#### Scenario: Valor longo
- **GIVEN** um valor encontrado com mais de 200 caracteres
- **WHEN** o achado é registrado
- **THEN** o valor guardado é cortado em 200 caracteres e indica o corte

### Requirement: REQ-RESULT-012 Campos opcionais para a Sofia
O resultado do contrato SOFIA v1.0 SHALL levar o veredito, a contagem por classe e, em cada achado, a classe, os registros, a orientação e a fonte, como campos opcionais, sem alterar os campos da v1.0.

#### Scenario: Cliente da v1.0
- **GIVEN** um cliente que conhece só os campos da v1.0
- **WHEN** ele recebe um resultado
- **THEN** os campos da v1.0 continuam com os mesmos nomes e significados

### Requirement: REQ-RESULT-013 O que não foi verificado
O resultado de uma execução avaliada SHALL listar o que o arquivo não prova, com o motivo, em campo próprio. A lista MUST NOT mudar o status, o veredito nem a revisão humana. Validação obrigatória não executada continua em limitações (REQ-RESULT-006).

#### Scenario: Confronto com outras obrigações
- **GIVEN** uma execução com todas as validações obrigatórias concluídas e sem achados
- **WHEN** o resultado é montado
- **THEN** a lista informa que o confronto com DCTF, DCOMP e pagamentos não foi verificado, com o motivo
- **AND** o status é `concluida`, e o veredito é `conforme`

#### Scenario: Execução sem avaliação
- **GIVEN** uma execução cuja etapa 2 rejeitou o arquivo
- **WHEN** o resultado é montado
- **THEN** a lista está vazia

## MODIFIED Requirements

### Requirement: REQ-RESULT-005 Revisão humana calculada
A indicação de revisão humana SHALL ser calculada pelo sistema e MUST ser positiva quando houver falha técnica, validação obrigatória não concluída, indício da IA, ausência de evidências ou achado da classe `grave`.

#### Scenario: Indício da IA presente
- **GIVEN** uma execução com um achado da categoria indício da IA
- **WHEN** o resultado é montado
- **THEN** a indicação de revisão humana é positiva

#### Scenario: Recomendação de dispensa ignorada
- **GIVEN** uma execução com ausência de evidências e uma recomendação do modelo para dispensar a revisão
- **WHEN** o resultado é montado
- **THEN** a indicação de revisão humana continua positiva

#### Scenario: Achado grave
- **GIVEN** uma execução com um achado da classe `grave`
- **WHEN** o resultado é montado
- **THEN** a indicação de revisão humana é positiva
