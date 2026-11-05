## Purpose

Garantir que apenas pessoas, agentes e serviços autenticados acessem as funções do Agente-ECF, com autorização por perfil e por organização: a identidade vem somente da credencial, e a organização de cada requisição precisa estar autorizada para essa identidade.

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
Cada endpoint funcional SHALL declarar os perfis que o acessam. Uma identidade autenticada sem nenhum desses perfis MUST receber HTTP 403, sem execução da operação.

#### Scenario: Cliente agente na administração da base
- **GIVEN** a Sofia INFIS autenticada apenas com o perfil cliente agente
- **WHEN** ela chama o endpoint de ingestão da base de conhecimento
- **THEN** a resposta tem status HTTP 403
- **AND** nenhum documento é ingerido

### Requirement: REQ-AUTH-003 Identidade da credencial e organização autorizada
A identidade do chamador SHALL ser obtida exclusivamente da credencial. A organização da requisição SHALL ser a informada pelo chamador, quando ele estiver autorizado para mais de uma, ou a única autorizada, e MUST constar das organizações autorizadas para essa identidade; caso contrário, a requisição MUST ser recusada sem executar a operação.

#### Scenario: Agente autorizado para várias organizações
- **GIVEN** a Sofia INFIS autorizada para `org-a` e `org-b`
- **WHEN** a requisição informa a organização `org-b`
- **THEN** a operação é registrada para `org-b` e para a identidade da Sofia

#### Scenario: Organização não autorizada
- **GIVEN** a Sofia INFIS autorizada para `org-a` e `org-b`
- **WHEN** a requisição informa a organização `org-c`
- **THEN** a resposta tem status HTTP 403
- **AND** nenhuma etapa do processamento é executada

#### Scenario: Organização não informada com várias autorizadas
- **GIVEN** uma identidade autorizada para `org-a` e `org-b`
- **WHEN** a requisição não informa a organização
- **THEN** a resposta tem status HTTP 400 e indica que a organização é obrigatória

#### Scenario: Uma única organização autorizada
- **GIVEN** uma identidade autorizada somente para `org-a`
- **WHEN** a requisição não informa a organização
- **THEN** a operação é registrada para `org-a`

#### Scenario: Identidade informada pelo cliente
- **GIVEN** uma credencial da identidade `sofia-infis`
- **WHEN** o corpo da requisição informa `usuario=outro-agente`
- **THEN** a operação é registrada para `sofia-infis`

### Requirement: REQ-AUTH-004 Isolamento por organização
Recursos de uma organização (execuções, resultados, conversas e consumo) MUST NOT ser acessíveis por identidades não autorizadas para essa organização. A tentativa SHALL responder HTTP 404, sem revelar a existência do recurso.

#### Scenario: Consulta de execução de outra organização
- **GIVEN** uma execução registrada para `org-a`
- **WHEN** uma identidade autorizada somente para `org-b` consulta essa execução
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

### Requirement: REQ-AUTH-007 Perfis do Agente-ECF
O sistema SHALL reconhecer os perfis administrador do agente, operador da orquestradora, cliente agente e usuário de análise, atribuídos pelo provedor de identidade. Perfis não reconhecidos MUST NOT conceder acesso a endpoints funcionais.

#### Scenario: Somente perfil desconhecido
- **GIVEN** uma credencial válida cujo único perfil é `perfil-desconhecido`
- **WHEN** ela chama um endpoint funcional
- **THEN** a resposta tem status HTTP 403

#### Scenario: Perfil reconhecido
- **GIVEN** uma credencial válida com o perfil cliente agente
- **WHEN** ela consulta a própria identidade
- **THEN** a resposta lista o perfil cliente agente

### Requirement: REQ-AUTH-008 Consulta da identidade autenticada
A Api.Agente SHALL oferecer `GET /identidade`, autenticado, que devolve o identificador da identidade, seus perfis reconhecidos, as organizações autorizadas e a organização resolvida para a requisição, e MUST NOT devolver o token nem outros segredos.

#### Scenario: Sofia confere a configuração
- **GIVEN** a Sofia INFIS autorizada para `org-a` e `org-b`
- **WHEN** ela envia `GET /identidade` informando a organização `org-a`
- **THEN** a resposta traz a identidade `sofia-infis`, o perfil cliente agente, as organizações `org-a` e `org-b` e a organização resolvida `org-a`
- **AND** a resposta não contém o token enviado
