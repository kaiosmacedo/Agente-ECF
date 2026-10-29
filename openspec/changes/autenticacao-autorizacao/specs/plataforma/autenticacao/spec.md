## Purpose

Garantir que apenas usuários e serviços autenticados acessem as funções do Agente-ECF, com autorização por perfil e organização e identidade derivada somente da credencial.

## ADDED Requirements

### Requirement: REQ-AUTH-001 Autenticação dos endpoints funcionais
Todo endpoint funcional da Api.Agente e da Api.RAG SHALL exigir credencial válida e responder HTTP 401 quando ela estiver ausente, expirada ou inválida. As verificações de saúde MUST continuar acessíveis sem credencial.

#### Scenario: Requisição sem credencial
- **WHEN** um cliente chama um endpoint funcional sem credencial
- **THEN** a resposta tem status HTTP 401
- **AND** nenhuma etapa do processamento é executada

#### Scenario: Saúde continua anônima
- **WHEN** um cliente sem credencial envia `GET /health/live`
- **THEN** a resposta tem status HTTP 200

### Requirement: REQ-AUTH-002 Autorização por perfil
Cada endpoint funcional SHALL declarar o perfil exigido. Um usuário autenticado sem o perfil MUST receber HTTP 403, sem execução da operação.

#### Scenario: Usuário de análise na administração da base
- **GIVEN** um usuário autenticado apenas com o perfil de análise
- **WHEN** ele chama o endpoint de ingestão da base de conhecimento
- **THEN** a resposta tem status HTTP 403
- **AND** nenhum documento é ingerido

### Requirement: REQ-AUTH-003 Identidade derivada da credencial
O usuário e a organização de cada requisição SHALL ser obtidos exclusivamente da credencial autenticada. Valores de usuário ou organização enviados no corpo, na query ou em cabeçalhos MUST NOT substituir os da credencial.

#### Scenario: Organização informada pelo cliente
- **GIVEN** uma credencial da organização `org-a`
- **WHEN** a requisição informa `organizacao=org-b` no corpo
- **THEN** a operação é registrada para `org-a`

### Requirement: REQ-AUTH-004 Isolamento por organização
Recursos de uma organização (execuções, resultados, conversas e consumo) MUST NOT ser acessíveis por usuários de outra organização. A tentativa SHALL responder HTTP 404, sem revelar a existência do recurso.

#### Scenario: Consulta de execução de outra organização
- **GIVEN** uma execução criada por um usuário de `org-a`
- **WHEN** um usuário de `org-b` consulta essa execução
- **THEN** a resposta tem status HTTP 404

### Requirement: REQ-AUTH-005 Autenticação entre serviços
A Api.RAG SHALL aceitar chamadas MCP somente com credencial de serviço válida emitida para a Api.Agente, e MUST rejeitar chamadas sem ela.

#### Scenario: Chamada MCP sem credencial de serviço
- **WHEN** um cliente chama uma ferramenta MCP da Api.RAG sem credencial de serviço
- **THEN** a chamada é rejeitada sem executar a ferramenta

### Requirement: REQ-AUTH-006 Falhas registradas sem credenciais
Cada falha de autenticação ou autorização SHALL ser registrada em log com correlação, motivo e rota, e os logs MUST NOT conter tokens, chaves ou senhas.

#### Scenario: Token inválido
- **WHEN** um cliente envia `Authorization: Bearer token-invalido-789`
- **THEN** um registro de log indica falha de autenticação com o identificador de correlação
- **AND** nenhum registro contém `token-invalido-789`
