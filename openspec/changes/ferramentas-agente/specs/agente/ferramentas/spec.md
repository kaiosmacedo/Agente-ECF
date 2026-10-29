## Purpose

Controlar as ferramentas que o modelo pode usar na Api.Agente, com catálogo completo, autorização a cada chamada, argumentos validados e resultados registrados.

## ADDED Requirements

### Requirement: REQ-FERRAM-001 Catálogo completo antes da exposição
Uma ferramenta SHALL ser exposta ao modelo somente se o catálogo registrar nome, responsabilidade, contratos de entrada e saída, regra de autorização, timeout, política de retentativa e idempotência e indicação de efeito externo.

#### Scenario: Ferramenta sem timeout no catálogo
- **GIVEN** uma ferramenta registrada sem timeout
- **WHEN** o agente monta as ferramentas disponíveis ao modelo
- **THEN** a ferramenta não é exposta e a falha de catálogo é registrada

### Requirement: REQ-FERRAM-002 Escolha do modelo não autoriza
A autorização SHALL ser verificada a cada chamada com base no usuário, na organização, na execução e na regra da ferramenta. A escolha da ferramenta pelo modelo MUST NOT conceder autorização.

#### Scenario: Chamada sem autorização
- **GIVEN** um usuário sem permissão para a ferramenta de teste `consulta-teste`
- **WHEN** o modelo solicita `consulta-teste`
- **THEN** a chamada é negada sem executar a ferramenta
- **AND** a negação é registrada

### Requirement: REQ-FERRAM-003 Argumentos validados
Os argumentos gerados pelo modelo SHALL ser validados como entrada não confiável, e argumentos inválidos MUST impedir a execução.

#### Scenario: Argumento fora do contrato
- **WHEN** o modelo chama uma ferramenta com um argumento que viola o contrato de entrada
- **THEN** a ferramenta não é executada e o modelo recebe erro de argumento inválido

### Requirement: REQ-FERRAM-004 Falha e timeout sem sucesso fabricado
Uma ferramenta que falhar ou exceder o timeout SHALL produzir resultado de falha técnica, e o agente MUST NOT apresentá-la como bem-sucedida.

#### Scenario: Timeout de ferramenta
- **GIVEN** uma ferramenta que não responde dentro do timeout
- **WHEN** o modelo a chama
- **THEN** o resultado registrado é de falha técnica

### Requirement: REQ-FERRAM-005 Resultados registrados para conferência
Cada chamada SHALL registrar argumentos, resultado, duração, status e correlação, com dados sensíveis mascarados, para que o resultado citado pelo modelo possa ser conferido.

#### Scenario: Citação de resultado de ferramenta
- **GIVEN** uma chamada de ferramenta registrada na execução
- **WHEN** o modelo cita o resultado dessa chamada
- **THEN** a validação semântica encontra o resultado registrado

### Requirement: REQ-FERRAM-006 Efeito externo bloqueado nesta fase
Ferramentas marcadas com efeito externo (escrita, envio a terceiros ou custo) MUST NOT ser expostas ao modelo enquanto a constitution limitar o agente à validação.

#### Scenario: Ferramenta com efeito externo
- **GIVEN** uma ferramenta registrada com efeito externo
- **WHEN** o agente monta as ferramentas disponíveis
- **THEN** a ferramenta não é exposta ao modelo
