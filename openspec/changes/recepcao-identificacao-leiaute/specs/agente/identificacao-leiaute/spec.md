## Purpose

Identificar o tipo de documento e a versão do leiaute de cada arquivo recebido, conforme os campos definidos no manual oficial, antes de qualquer validação dependente de versão.

## ADDED Requirements

### Requirement: REQ-LEIAUTE-001 Identificação pelo manual oficial
A etapa 3 SHALL identificar o tipo de documento e a versão do leiaute a partir do registro e dos campos definidos no manual oficial, e registrar ambos na execução.
Fonte: Manual da ECF, leiaute 12 (20/05/2026); registro e campos de identificação a extrair em E05.

#### Scenario: Arquivo do leiaute 12
- **GIVEN** um arquivo sintético que informa a versão 12 nos campos de identificação definidos no manual
- **WHEN** a etapa 3 executa
- **THEN** a execução registra o tipo ECF e a versão 12 do leiaute

### Requirement: REQ-LEIAUTE-002 Versão não suportada
Quando a versão identificada não tiver manual e validadores cadastrados, as validações dependentes de versão MUST NOT ser executadas, e a limitação SHALL constar do resultado.

#### Scenario: Versão sem suporte
- **GIVEN** um arquivo sintético que informa uma versão sem validadores cadastrados
- **WHEN** a etapa 3 executa
- **THEN** a versão é registrada
- **AND** as etapas 4 e 5 constam como não executadas com a limitação "versão do leiaute não suportada"

### Requirement: REQ-LEIAUTE-003 Identificação ambígua ou impossível
Se o tipo ou a versão não puderem ser identificados de forma inequívoca, a etapa 3 SHALL terminar com falha, e o resultado MUST NOT declarar conformidade.

#### Scenario: Campo de versão ausente
- **GIVEN** um arquivo sintético sem os campos de identificação
- **WHEN** a etapa 3 executa
- **THEN** a etapa consta com falha
- **AND** o status não é de análise concluída
