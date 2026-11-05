## Purpose

Permitir que clientes de navegador autorizados alcancem o cartão e o endpoint A2A do Agente-ECF, sem confundir CORS com a conectividade entre servidores nem com autenticação.

## ADDED Requirements

### Requirement: REQ-CORS-001 Origens autorizadas
A Api.Agente SHALL aplicar a política `SofiaBrowser` ao cartão e ao endpoint A2A, permitindo somente as origens de `Cors:AllowedOrigins`. Uma origem não listada MUST NOT receber permissão de CORS.

#### Scenario: Preflight de origem autorizada
- **GIVEN** `Cors:AllowedOrigins` com `http://localhost:4200`
- **WHEN** um navegador envia `OPTIONS /a2a` com `Origin: http://localhost:4200` e método `POST`
- **THEN** a resposta traz `Access-Control-Allow-Origin: http://localhost:4200` e permite `POST` e `Content-Type`

#### Scenario: Preflight de origem não autorizada
- **WHEN** um navegador envia `OPTIONS /a2a` com `Origin: https://site-nao-autorizado.example`
- **THEN** a resposta não traz `Access-Control-Allow-Origin`

### Requirement: REQ-CORS-002 Chamadas entre servidores independem de CORS
Requisições sem `Origin` SHALL passar pelo fluxo normal, mesmo sem origens configuradas. Sem cliente de navegador, a lista de origens MAY ficar vazia, e os endpoints fora do escopo da política MUST NOT receber cabeçalhos de CORS.

#### Scenario: Sofia chamando sem Origin
- **GIVEN** `Cors:AllowedOrigins` vazio
- **WHEN** um servidor envia `POST /a2a` sem `Origin`
- **THEN** a tarefa é processada

#### Scenario: Saúde fora do escopo
- **WHEN** um navegador de origem autorizada chama `GET /health/live`
- **THEN** a resposta não traz `Access-Control-Allow-Origin`

### Requirement: REQ-CORS-003 Origens explícitas validadas
Cada item de `Cors:AllowedOrigins` SHALL ser uma origem `http` ou `https` com esquema, host e porta, sem caminho, barra final ou curinga. Um item inválido MUST impedir a inicialização, citando a posição e sem revelar o valor.

#### Scenario: Curinga na configuração
- **WHEN** o serviço inicia com `Cors:AllowedOrigins:0` igual a `*`
- **THEN** a inicialização falha citando `Cors:AllowedOrigins:0`
