# plataforma/observabilidade Specification

## Purpose
Permitir rastrear cada requisição do Agente-ECF entre serviços e na orquestradora por um identificador de correlação, com logs estruturados que não exponham dados sensíveis.

## Requirements

### Requirement: REQ-OBSERV-001 Identificador de correlação
Cada serviço SHALL associar um identificador de correlação a cada requisição HTTP: reutilizar o valor recebido no cabeçalho `X-Correlation-Id` quando válido, ou gerar um novo, e devolvê-lo no mesmo cabeçalho da resposta. Valor válido tem de 1 a 128 caracteres entre letras, dígitos, hífen, sublinhado e ponto.

#### Scenario: Identificador recebido e válido
- **WHEN** um cliente envia `GET /health/live` com `X-Correlation-Id: sofia-123.abc`
- **THEN** a resposta contém `X-Correlation-Id: sofia-123.abc`

#### Scenario: Identificador ausente
- **WHEN** um cliente envia `GET /health/live` sem o cabeçalho `X-Correlation-Id`
- **THEN** a resposta contém `X-Correlation-Id` com um valor novo e não vazio

#### Scenario: Identificador inválido
- **WHEN** um cliente envia `X-Correlation-Id` com 129 caracteres ou com quebra de linha
- **THEN** a resposta contém um `X-Correlation-Id` novo, diferente do recebido
- **AND** o valor recebido não aparece nos logs

### Requirement: REQ-OBSERV-002 Logs estruturados
Cada serviço SHALL emitir logs em JSON na saída padrão. Cada registro produzido durante uma requisição MUST conter o identificador de correlação, o nome do serviço e a versão do serviço.

#### Scenario: Registro de requisição
- **WHEN** um cliente envia `GET /health/live` com `X-Correlation-Id: teste-log-1`
- **THEN** cada registro de log dessa requisição é um objeto JSON válido
- **AND** contém o identificador `teste-log-1`, o nome do serviço e a versão do serviço

### Requirement: REQ-OBSERV-003 Logs sem dados sensíveis
Os logs MUST NOT conter corpos de requisição ou de resposta, valores dos cabeçalhos `Authorization` e `Cookie` nem valores de opções de configuração.

#### Scenario: Cabeçalho de autorização não registrado
- **WHEN** um cliente envia `GET /health/live` com `Authorization: Bearer token-secreto-123`
- **THEN** nenhum registro de log contém `token-secreto-123`
