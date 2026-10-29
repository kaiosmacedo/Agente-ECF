# plataforma/saude-servicos Specification

## Purpose
Permitir que a plataforma de hospedagem, o pipeline e a orquestradora saibam se cada serviço do Agente-ECF está vivo e pronto para atender, sem autenticação e sem expor dados.

## Requirements

### Requirement: REQ-SAUDE-001 Verificação de vivacidade
Cada serviço (Api.Agente e Api.RAG) SHALL responder a `GET /health/live` com HTTP 200 enquanto o processo estiver em execução, sem consultar dependências externas.

#### Scenario: Serviço em execução
- **GIVEN** a Api.Agente iniciada sem nenhuma dependência externa disponível
- **WHEN** um cliente envia `GET /health/live`
- **THEN** a resposta tem status HTTP 200
- **AND** o corpo informa o estado `Healthy`

### Requirement: REQ-SAUDE-002 Verificação de prontidão
Cada serviço SHALL responder a `GET /health/ready` com HTTP 200 somente quando todas as dependências obrigatórias registradas estiverem disponíveis, e com HTTP 503 quando alguma estiver indisponível.

#### Scenario: Sem dependências obrigatórias registradas
- **GIVEN** a Api.RAG iniciada sem dependências obrigatórias registradas
- **WHEN** um cliente envia `GET /health/ready`
- **THEN** a resposta tem status HTTP 200

#### Scenario: Dependência obrigatória indisponível
- **GIVEN** um serviço com uma dependência obrigatória registrada que está indisponível
- **WHEN** um cliente envia `GET /health/ready`
- **THEN** a resposta tem status HTTP 503
- **AND** o corpo informa o estado `Unhealthy`

### Requirement: REQ-SAUDE-003 Acesso anônimo sem dados
As verificações de saúde SHALL ser acessíveis sem autenticação e MUST NOT expor configuração, segredos, versões de dependências, nomes de host, caminhos ou mensagens de exceção.

#### Scenario: Resposta sem detalhes internos
- **WHEN** um cliente sem credenciais envia `GET /health/live`
- **THEN** a resposta tem status HTTP 200
- **AND** o corpo contém somente o estado geral

#### Scenario: Falha sem mensagem de exceção
- **GIVEN** uma dependência obrigatória que falha com a exceção "senha inválida para host db-interno"
- **WHEN** um cliente envia `GET /health/ready`
- **THEN** a resposta tem status HTTP 503
- **AND** o corpo não contém o texto "senha inválida" nem "db-interno"
