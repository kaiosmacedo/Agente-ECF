## Purpose

Empacotar cada serviço do Agente-ECF em contêiner executável na plataforma de hospedagem escolhida e publicá-lo nos ambientes definidos, sem levar segredos na imagem.

## ADDED Requirements

### Requirement: REQ-ENTREGA-001 Imagem de contêiner por serviço
Cada serviço SHALL ser empacotado em imagem de contêiner que inicia sem dependências externas e responde à verificação de vivacidade. A construção da imagem e esse teste MUST rodar no pipeline de cada PR.

#### Scenario: Imagem inicia e responde
- **GIVEN** a imagem da Api.Agente construída no pipeline
- **WHEN** o contêiner inicia sem variáveis de ambiente além das obrigatórias
- **THEN** `GET /health/live` responde HTTP 200 em até 30 segundos

### Requirement: REQ-ENTREGA-002 Porta definida pela plataforma
O serviço em contêiner SHALL escutar em todas as interfaces na porta informada pela variável de ambiente `PORT` e, na ausência dela, na porta 8080.

#### Scenario: Porta informada pela plataforma
- **GIVEN** o contêiner da Api.RAG iniciado com `PORT=3000`
- **WHEN** um cliente envia `GET /health/live` na porta 3000
- **THEN** a resposta tem status HTTP 200

#### Scenario: Porta padrão
- **GIVEN** o contêiner da Api.RAG iniciado sem a variável `PORT`
- **WHEN** um cliente envia `GET /health/live` na porta 8080
- **THEN** a resposta tem status HTTP 200

### Requirement: REQ-ENTREGA-003 Imagem sem segredos
A imagem de contêiner MUST NOT conter arquivos `.env`, arquivos de configuração locais, código-fonte de testes, especificações nem o histórico do repositório.

#### Scenario: Conteúdo da imagem
- **WHEN** o pipeline lista os arquivos da imagem construída
- **THEN** nenhum arquivo `.env`, `appsettings.*.local.json`, diretório `.git`, `tests` ou `openspec` está presente

### Requirement: REQ-ENTREGA-004 Publicação em produção a cada merge
Cada merge na `main` SHALL publicar a Api.Agente e a Api.RAG no ambiente de produção da hospedagem, somente depois de todas as verificações do pipeline aprovadas. PRs e outras branches MUST NOT gerar publicação.

#### Scenario: Merge na main
- **GIVEN** um PR com todas as verificações aprovadas
- **WHEN** o PR é mesclado na `main`
- **THEN** os dois serviços são publicados em produção a partir do commit mesclado

#### Scenario: PR aberto
- **WHEN** um PR é aberto ou atualizado
- **THEN** nenhuma publicação é criada na hospedagem

### Requirement: REQ-ENTREGA-005 Verificação após a publicação
Após cada publicação em produção, o pipeline SHALL verificar `GET /health/live` de cada serviço publicado. Falha ou ausência de resposta HTTP 200 em até 60 segundos MUST marcar a publicação como falha.

#### Scenario: Serviço publicado saudável
- **GIVEN** a Api.Agente publicada em produção
- **WHEN** o pipeline consulta `GET /health/live` na URL de produção
- **THEN** a verificação recebe HTTP 200 e a publicação é marcada como concluída

#### Scenario: Serviço publicado sem resposta
- **GIVEN** a Api.RAG publicada em produção sem responder
- **WHEN** a verificação não recebe HTTP 200 em 60 segundos
- **THEN** a publicação é marcada como falha no pipeline
