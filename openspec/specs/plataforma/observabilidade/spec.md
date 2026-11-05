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

### Requirement: REQ-OBSERV-004 Traces das requisições
Cada serviço SHALL gerar um trace para cada requisição HTTP e para cada chamada HTTP de saída, continuando o contexto W3C `traceparent` recebido. Os logs da requisição MUST trazer o mesmo `TraceId`, e as verificações de saúde MUST NOT gerar trace.

#### Scenario: Contexto recebido da Sofia
- **GIVEN** uma requisição com o cabeçalho `traceparent` do trace `4bf92f3577b34da6a3ce929d0e0e4736`
- **WHEN** o serviço a atende
- **THEN** o trace gerado usa esse identificador
- **AND** os logs da requisição trazem o mesmo `TraceId`

#### Scenario: Verificação de saúde
- **WHEN** um cliente chama `GET /health/live`
- **THEN** nenhum trace é gerado para essa chamada

### Requirement: REQ-OBSERV-005 Métricas de latência e erros
Cada serviço SHALL expor a duração e a contagem das requisições HTTP por rota e status, identificadas pelo nome e pela versão do serviço.

#### Scenario: Requisição atendida
- **WHEN** um cliente envia uma requisição a uma rota do serviço
- **THEN** a métrica `http.server.request.duration` registra a rota e o status da resposta

### Requirement: REQ-OBSERV-006 Exportação opcional
Traces e métricas SHALL ser exportados por OTLP somente quando `Telemetria:Endpoint` estiver configurado. Sem a opção, o serviço MUST funcionar normalmente. Um endereço inválido MUST impedir a inicialização, citando a opção sem revelar o valor.

#### Scenario: Sem destino configurado
- **GIVEN** um serviço sem `Telemetria:Endpoint`
- **WHEN** ele atende requisições
- **THEN** elas são atendidas normalmente e nada é exportado

#### Scenario: Endereço inválido
- **WHEN** o serviço inicia com `Telemetria:Endpoint` igual a `nao-e-url`
- **THEN** a inicialização falha citando `Telemetria:Endpoint`
- **AND** a mensagem não contém `nao-e-url`

### Requirement: REQ-OBSERV-007 Telemetria sem dados sensíveis
Traces e métricas MUST NOT conter corpo de requisição, query string, cabeçalho `Authorization`, nome do usuário ou texto de documentos.

#### Scenario: Chamada com dados sensíveis
- **GIVEN** uma requisição com a query `?chave=segredo-123` e um documento com o texto `conteudo-sigiloso-123`
- **WHEN** o serviço a atende
- **THEN** nenhum atributo de trace contém `segredo-123` nem `conteudo-sigiloso-123`
