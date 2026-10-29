## Purpose

Verificar de forma determinística as regras de negócio da ECF, incluindo a apuração do IRPJ e da CSLL e os cruzamentos entre registros, sempre com fonte oficial registrada.

## ADDED Requirements

### Requirement: REQ-NEGOCIO-001 Regra de negócio com fonte e validador
Cada regra de negócio SHALL ter identificador, fonte oficial registrada (documento, versão e localização) e um validador determinístico com testes de caso válido e inválido. Regra sem fonte MUST NOT ser implementada.
Fonte: Manual da ECF, leiaute 12 (20/05/2026) e demais fontes registradas em E03; localização por regra.

#### Scenario: Regra sem fonte registrada
- **GIVEN** uma regra de cruzamento proposta sem fonte oficial
- **WHEN** o catálogo de regras de negócio é validado
- **THEN** a regra é recusada com o motivo "fonte ausente"

### Requirement: REQ-NEGOCIO-002 Inconsistência com os dois lados do cruzamento
Cada violação SHALL gerar um achado da categoria inconsistência de negócio que identifique a regra, a fonte e os registros e linhas dos dois lados do cruzamento.

#### Scenario: Valores divergentes entre registros
- **GIVEN** um arquivo sintético em que a regra de teste `NEG-TESTE-001` compara o registro A da linha 10 com o registro B da linha 80, e os valores divergem
- **WHEN** a validação de negócio executa
- **THEN** existe um achado de inconsistência de negócio citando `NEG-TESTE-001` e as linhas 10 e 80

### Requirement: REQ-NEGOCIO-003 Dado externo ausente
Quando uma regra depender de dado externo não fornecido (por exemplo, a ECF do período anterior), o resultado SHALL registrar ausência de evidências para essa regra e MUST NOT considerá-la atendida.

#### Scenario: Período anterior não enviado
- **GIVEN** uma regra que compara com o período anterior e nenhum arquivo do período anterior enviado
- **WHEN** a validação de negócio executa
- **THEN** o resultado registra ausência de evidências para a regra
- **AND** a revisão humana é indicada

### Requirement: REQ-NEGOCIO-004 Execução após a validação estrutural
As regras de negócio SHALL ser executadas somente sobre registros cuja validação estrutural foi concluída, e MUST NOT avaliar registros com erro estrutural que impeça a leitura dos campos envolvidos.

#### Scenario: Registro com erro estrutural
- **GIVEN** um registro envolvido numa regra com erro estrutural no campo usado pela regra
- **WHEN** a validação de negócio executa
- **THEN** a regra não é avaliada para esse registro
- **AND** o resultado indica a regra como não realizada para ele
