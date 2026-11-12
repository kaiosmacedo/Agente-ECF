## MODIFIED Requirements

### Requirement: REQ-DOCAPI-004 Rotas publicadas no documento
O documento OpenAPI SHALL listar as rotas públicas da Api.Agente: `POST /analises`, `GET /analises/{id}`, `POST /perguntas`, `POST /a2a` e `GET /.well-known/agent-card.json`.

#### Scenario: Rotas documentadas
- **WHEN** um cliente pede `GET /openapi/v1.json`
- **THEN** o documento traz essas cinco rotas, com os métodos indicados
