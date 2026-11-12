## MODIFIED Requirements

### Requirement: REQ-A2A-007 Cartão descreve a especialidade ECF
A skill `analisar_documentos` SHALL dizer que o agente valida arquivos da ECF (Escrituração Contábil Fiscal, do SPED) dos leiautes 7 a 12, anos-calendário 2020 a 2025; como reconhecer uma ECF (texto cuja primeira linha começa com `|0000|LECF|`); o que ele devolve; que outros documentos não são analisados; e, com o modo pergunta ligado, que ele responde perguntas sobre a ECF sem arquivo, com base nos manuais oficiais dos leiautes 7 a 12 e com a fonte citada (REQ-PERGUNTA-001). A descrição MUST NOT anunciar validação nem função que ainda não existe.
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
