## Purpose

Expor pela Api.RAG, como servidor MCP, a busca nos manuais oficiais filtrada por tipo de documento e versão, devolvendo trechos com referências recuperáveis.

## ADDED Requirements

### Requirement: REQ-RECUP-001 Ferramenta MCP de busca com contrato versionado
A Api.RAG SHALL expor uma ferramenta MCP de busca que recebe consulta, tipo de documento, versão do manual e quantidade máxima de trechos, e devolve cada trecho com texto, relevância e referência (documento, versão e localização), em contrato versionado.

#### Scenario: Busca com resultado
- **GIVEN** o Manual da ECF, leiaute 12, ingerido
- **WHEN** a ferramenta é chamada com tipo ECF, versão 12 e uma consulta sintética
- **THEN** cada trecho devolvido informa documento, versão, localização e relevância

### Requirement: REQ-RECUP-002 Filtro obrigatório por tipo e versão
A busca SHALL considerar somente chunks vigentes do tipo e da versão informados e MUST NOT devolver trechos de outra versão. Sem documento vigente para a versão, o resultado SHALL vir vazio com o motivo "versão indisponível".

#### Scenario: Duas versões ingeridas
- **GIVEN** as versões 11 e 12 de um manual ingeridas
- **WHEN** a busca é feita para a versão 12
- **THEN** nenhum trecho da versão 11 é devolvido

#### Scenario: Versão não ingerida
- **WHEN** a busca é feita para uma versão sem documento vigente
- **THEN** o resultado vem vazio com o motivo "versão indisponível"

### Requirement: REQ-RECUP-003 Argumentos validados antes da busca
Argumentos inválidos (tipo ou versão ausentes, quantidade fora do intervalo configurado, consulta vazia) SHALL gerar erro tipado, e a busca MUST NOT ser executada.

#### Scenario: Quantidade fora do intervalo
- **WHEN** a ferramenta é chamada pedindo 0 trechos
- **THEN** a chamada retorna erro tipado de argumento inválido
- **AND** nenhum embedding é gerado

### Requirement: REQ-RECUP-004 Consumo da consulta registrado
O embedding gerado para cada consulta SHALL ser registrado como consumo no banco da Api.RAG.

#### Scenario: Consulta realizada
- **WHEN** uma busca gera o embedding da consulta
- **THEN** existe um registro de consumo do tipo embedding com a correlação da chamada
