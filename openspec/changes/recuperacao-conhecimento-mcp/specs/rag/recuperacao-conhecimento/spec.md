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

### Requirement: REQ-RECUP-005 Busca pelo código de regra ou de registro
Quando a chamada informar um código, como o de uma regra (`REGRA_...`) ou de um registro, a busca SHALL trazer primeiro os trechos que contêm esse código no texto, ordenados por relevância, e completar com a busca semântica até a quantidade pedida, sem repetir trechos. O código MUST ser validado (letras maiúsculas, dígitos e sublinhado, até 60 caracteres) e passado como parâmetro da consulta ao banco, nunca concatenado (SQ-05).

#### Scenario: Código presente em um trecho
- **GIVEN** uma base sintética em que só um trecho contém `REGRA_TESTE_X`, e outros trechos são mais parecidos com a consulta
- **WHEN** a ferramenta é chamada com a consulta e o código `REGRA_TESTE_X`
- **THEN** o trecho que contém o código vem em primeiro lugar
- **AND** os demais trechos vêm da busca semântica, sem repetição

#### Scenario: Código inválido
- **WHEN** a ferramenta é chamada com o código `REGRA'; DROP`
- **THEN** a chamada retorna erro tipado de argumento inválido
- **AND** nenhum embedding é gerado

### Requirement: REQ-RECUP-006 Limite de uso e desligamento
Como a ferramenta é pública na primeira versão e gera custo no provedor (SQ-12), ela SHALL recusar a chamada com erro tipado, sem gerar embedding, quando estiver desativada por configuração ou quando as consultas do dia (UTC) registradas no consumo atingirem o limite configurado.

#### Scenario: Busca desativada
- **GIVEN** a busca desativada por configuração
- **WHEN** a ferramenta é chamada com argumentos válidos
- **THEN** a chamada retorna o erro tipado `busca_desativada`
- **AND** nenhum embedding é gerado

#### Scenario: Limite diário atingido
- **GIVEN** um limite configurado de 3 consultas por dia e 3 consultas já registradas hoje
- **WHEN** a ferramenta é chamada pela quarta vez
- **THEN** a chamada retorna o erro tipado `limite_de_uso_atingido`
- **AND** nenhum embedding é gerado
