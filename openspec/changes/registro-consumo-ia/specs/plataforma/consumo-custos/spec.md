## Purpose

Registrar com fidelidade, em cada serviço do Agente-ECF, o consumo e o custo de cada chamada a provedores de IA, inclusive retentativas, sem transformar valores desconhecidos em zero.

## ADDED Requirements

### Requirement: REQ-CONSUMO-001 Uma linha por chamada ou tentativa
Cada chamada a provedor de IA SHALL gerar um registro no banco do serviço que a realizou, com execução, correlação, provedor, modelo, tipo de chamada (geração, embedding, retentativa ou auxiliar), data, duração e status.

#### Scenario: Retentativa registrada
- **GIVEN** uma chamada de geração que falha uma vez e é repetida com sucesso
- **WHEN** o consumo da execução é consultado
- **THEN** existem dois registros: um com status de falha e um de retentativa com sucesso

### Requirement: REQ-CONSUMO-002 Desconhecido nunca vira zero
Tokens ou custos não informados pelo provedor SHALL ser registrados como desconhecidos, com indicador explícito, e MUST NOT ser gravados como zero.

#### Scenario: Provedor sem contagem de tokens
- **GIVEN** uma resposta do provedor sem contagem de tokens de saída
- **WHEN** o consumo é registrado
- **THEN** os tokens de saída constam como desconhecidos
- **AND** o valor gravado não é zero

### Requirement: REQ-CONSUMO-003 Origem do valor e tabela de preços
Cada valor SHALL indicar se foi informado pelo provedor ou estimado pelo sistema. A estimativa de custo MUST referenciar a versão vigente da tabela de preços; sem tabela vigente para o modelo, o custo SHALL constar como desconhecido.

#### Scenario: Modelo sem preço cadastrado
- **GIVEN** uma chamada a um modelo ausente da tabela de preços vigente
- **WHEN** o consumo é registrado
- **THEN** o custo consta como desconhecido e sem referência de tabela

#### Scenario: Estimativa com tabela vigente
- **GIVEN** uma tabela de preços versão 2026-10 vigente para o modelo usado
- **WHEN** o consumo é registrado com tokens informados
- **THEN** o custo estimado referencia a versão 2026-10 da tabela

### Requirement: REQ-CONSUMO-004 Falha ao gravar é observável
Uma falha ao gravar o registro de consumo SHALL gerar log de erro com o identificador de correlação e incrementar uma métrica de falhas de registro, e MUST NOT ser silenciosa.

#### Scenario: Banco indisponível ao registrar
- **GIVEN** o banco de consumo indisponível
- **WHEN** uma chamada de embedding termina
- **THEN** um log de erro com o identificador de correlação é emitido
- **AND** a métrica de falhas de registro aumenta

### Requirement: REQ-CONSUMO-005 Consulta agregada restrita
O serviço SHALL oferecer consulta agregada de consumo por execução, modelo e período somente a perfis autorizados, e a consulta MUST NOT expor prompts, conteúdo de documentos ou credenciais.

#### Scenario: Consulta sem perfil autorizado
- **WHEN** um usuário de análise consulta o consumo agregado
- **THEN** a resposta tem status HTTP 403
