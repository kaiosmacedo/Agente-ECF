## Purpose

Registrar cada execução de análise do Agente-ECF com o estado das nove etapas obrigatórias e calcular de forma determinística o status, sem declarar conformidade indevida.

## ADDED Requirements

### Requirement: REQ-EXEC-001 Registro da execução
Cada execução SHALL receber um identificador único gerado pelo sistema e registrar usuário, organização, data de criação e o estado de cada uma das nove etapas da constitution (concluída, não aplicável, com falha ou não executada).

#### Scenario: Execução criada
- **WHEN** uma execução é criada para um usuário de `org-a`
- **THEN** ela tem um identificador gerado pelo sistema
- **AND** registra as nove etapas, todas inicialmente como não executadas

### Requirement: REQ-EXEC-002 Etapas dependentes após falha
Quando uma etapa falhar, as etapas que dependem dela SHALL ser registradas como não executadas, e MUST NOT ser apresentadas como concluídas.

#### Scenario: Falha na identificação do leiaute
- **GIVEN** uma execução cuja etapa 3 terminou com falha
- **WHEN** o status é calculado
- **THEN** as etapas 4 a 8 constam como não executadas

### Requirement: REQ-EXEC-003 Etapa não aplicável com justificativa
Uma etapa SHALL ser marcada como não aplicável somente com uma justificativa prevista na especificação, e a justificativa MUST constar do resultado.

#### Scenario: Ferramentas externas não necessárias
- **GIVEN** uma execução em que nenhuma ferramenta externa é necessária
- **WHEN** a etapa 6 é registrada
- **THEN** ela consta como não aplicável com a justificativa registrada

### Requirement: REQ-EXEC-004 Status calculado pelo sistema
O status SHALL ser calculado por regra determinística a partir dos estados das etapas e dos achados, e MUST NOT indicar análise concluída quando alguma validação obrigatória estiver com falha, não executada ou não aplicável sem justificativa.

#### Scenario: Validação obrigatória não concluída
- **GIVEN** uma execução sem achados cuja etapa 4 não foi executada
- **WHEN** o status é calculado
- **THEN** o status é de análise parcial ou de falha técnica
- **AND** o resultado não declara o documento conforme

#### Scenario: Entrada rejeitada
- **GIVEN** uma execução cuja etapa 2 rejeitou o arquivo
- **WHEN** o status é calculado
- **THEN** o status é de entrada rejeitada

### Requirement: REQ-EXEC-005 Consulta do resultado
A Api.Agente SHALL expor `GET /analises/{id}` que devolve o envelope do resultado a usuários da mesma organização da execução.

#### Scenario: Consulta pela mesma organização
- **GIVEN** uma execução de `org-a`
- **WHEN** um usuário autenticado de `org-a` envia `GET /analises/{id}`
- **THEN** a resposta tem status HTTP 200 com o envelope do resultado

### Requirement: REQ-EXEC-006 Registro de auditoria
Cada execução SHALL registrar o hash do arquivo, as versões do contrato, dos validadores e dos componentes usados e os estados das etapas, de forma que o resultado possa ser explicado depois.

#### Scenario: Execução auditável
- **WHEN** uma execução termina
- **THEN** o registro contém o hash do arquivo, as versões usadas e os estados das nove etapas
