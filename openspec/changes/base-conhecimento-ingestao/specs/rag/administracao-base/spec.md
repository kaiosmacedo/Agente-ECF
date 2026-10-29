## Purpose

Permitir que administradores consultem e mantenham os documentos da base de conhecimento da Api.RAG, preservando o histórico de versões para auditoria.

## ADDED Requirements

### Requirement: REQ-BASE-001 Listagem de documentos
A Api.RAG SHALL listar os documentos da base com emissor, título, tipo, versão, vigência, estado (vigente, substituído ou desativado) e data de ingestão, com filtros por tipo e versão, somente para administradores.

#### Scenario: Filtro por versão
- **GIVEN** a base com as versões A e B de um manual
- **WHEN** um administrador lista os documentos filtrando pela versão B
- **THEN** a resposta contém somente a versão B

### Requirement: REQ-BASE-002 Consulta de um documento
A Api.RAG SHALL devolver os metadados de um documento, a quantidade de chunks e o estado da última ingestão.

#### Scenario: Documento ingerido com sucesso
- **WHEN** um administrador consulta um documento ingerido
- **THEN** a resposta traz os metadados, a quantidade de chunks e o estado da ingestão

### Requirement: REQ-BASE-003 Desativação sem exclusão
Um administrador SHALL poder desativar um documento. O documento desativado MUST deixar de ser recuperado como vigente e SHALL continuar disponível para auditoria de execuções anteriores.

#### Scenario: Documento desativado
- **GIVEN** um manual vigente usado por execuções anteriores
- **WHEN** um administrador o desativa
- **THEN** o documento não aparece mais nas buscas de vigentes
- **AND** continua consultável pelo identificador
