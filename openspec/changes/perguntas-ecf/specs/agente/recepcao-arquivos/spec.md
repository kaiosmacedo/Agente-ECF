## MODIFIED Requirements

### Requirement: REQ-RECEPCAO-006 Uma ECF por solicitação
Cada solicitação SHALL ter exatamente um documento aceito como ECF. Os demais documentos MUST constar das observações como não analisados por este agente, sem virar achado. Sem nenhuma ECF, a etapa 2 SHALL rejeitar a solicitação com o código `nenhuma_ecf`, exceto o pedido que só traz a pergunta, que vai ao modo pergunta quando ele estiver ligado (REQ-PERGUNTA-001); com mais de uma, com o código `mais_de_uma_ecf`.

#### Scenario: ECF e outro documento
- **GIVEN** uma solicitação com uma ECF sintética e uma NF-e sintética
- **WHEN** a solicitação é processada
- **THEN** a ECF é analisada
- **AND** as observações informam que a NF-e não foi analisada por este agente

#### Scenario: Nenhuma ECF
- **GIVEN** uma solicitação só com uma NF-e sintética
- **WHEN** a solicitação é processada
- **THEN** o status é `entrada-rejeitada` com o código `nenhuma_ecf`

#### Scenario: Só a pergunta
- **GIVEN** uma solicitação cujo único documento tem o mesmo texto da pergunta, com o modo pergunta ligado
- **WHEN** a solicitação é processada
- **THEN** ela vai ao modo pergunta (REQ-PERGUNTA-001), sem o código `nenhuma_ecf`

#### Scenario: Duas ECF
- **GIVEN** uma solicitação com duas ECF sintéticas
- **WHEN** a solicitação é processada
- **THEN** o status é `entrada-rejeitada` com o código `mais_de_uma_ecf`
