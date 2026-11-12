# integracao/servidor-a2a Specification

## Purpose
Fazer o cartão do agente dizer à Sofia INFIS, sem ambiguidade, que documentos o Agente-ECF analisa e o que ele devolve, para que ela encaminhe as ECF por conta própria.

## Requirements

### Requirement: REQ-A2A-007 Cartão descreve a especialidade ECF
A skill `analisar_documentos` SHALL dizer que o agente valida a ECF do SPED dos leiautes 7 a 12 (anos-calendário 2020 a 2025), que começa com `|0000|LECF|`; o que devolve; que outros documentos não são analisados; e, com o modo pergunta ligado, que responde perguntas sobre a ECF citando o manual (REQ-PERGUNTA-001). A descrição MUST NOT anunciar validação nem função que ainda não existe.
Fonte: Manual da ECF, leiaute 12 (23/07/2026), seção 1.14, p. 17, seção 2.2, p. 32, e Registro 0000, p. 63.

#### Scenario: Leitura do cartão pela Sofia
- **WHEN** um cliente A2A lê `/.well-known/agent-card.json`
- **THEN** a descrição da skill `analisar_documentos` cita a ECF, os leiautes 7 a 12 e a primeira linha `|0000|LECF|`
- **AND** diz que documentos de outro tipo não são analisados

#### Scenario: Cartão com o modo pergunta ligado
- **GIVEN** `Perguntas__Habilitada=true`
- **WHEN** um cliente A2A lê o cartão
- **THEN** a descrição da skill diz que o agente responde perguntas sobre a ECF com base nos manuais, citando a fonte
- **AND** a skill tem um exemplo de pergunta sem arquivo e a tag `perguntas`

#### Scenario: Cartão com o modo pergunta desligado
- **GIVEN** `Perguntas__Habilitada=false`
- **WHEN** um cliente A2A lê o cartão
- **THEN** a descrição da skill não anuncia as perguntas

### Requirement: REQ-A2A-008 Exemplos, tags e versão do cartão
A skill `analisar_documentos` SHALL trazer exemplos de pedido e as tags `ecf` e `sped`, e a versão do cartão SHALL mudar a cada alteração (GD-01), com a cópia em `docs/contratos/agent-card.json` igual ao cartão servido.

#### Scenario: Exemplos e versão
- **WHEN** um cliente A2A lê o cartão
- **THEN** a skill tem pelo menos um exemplo de pedido e as tags `ecf` e `sped`
- **AND** a versão do cartão é `1.1.0`

#### Scenario: Cópia versionada igual ao cartão servido
- **WHEN** os testes de contrato executam
- **THEN** o cartão servido é igual a `docs/contratos/agent-card.json`, exceto pela `url`
