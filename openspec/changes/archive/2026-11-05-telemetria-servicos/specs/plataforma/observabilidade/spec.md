## Purpose

Permitir acompanhar cada requisição dos serviços do Agente-ECF por traces e métricas, sem depender de um destino externo e sem expor dados sensíveis.

## ADDED Requirements

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
