## Purpose

Validar de forma determinística a estrutura, os registros e os campos do arquivo da ECF segundo o leiaute oficial da versão identificada, com cada regra ligada à sua fonte.

## ADDED Requirements

### Requirement: REQ-ESTRUT-001 Regra com fonte e validador
Cada regra estrutural SHALL ter identificador, fonte (documento, versão e localização no manual) e um validador determinístico com testes de caso válido e inválido. Regra sem localização no manual MUST NOT ser implementada.
Fonte: Manual da ECF, leiaute 12 (20/05/2026); localização registrada por regra no catálogo (E02).

#### Scenario: Regra sem localização
- **GIVEN** uma regra candidata sem localização no manual
- **WHEN** o catálogo é validado
- **THEN** a regra é recusada com o motivo "fonte ausente"

### Requirement: REQ-ESTRUT-002 Catálogo de validações obrigatórias por versão
O catálogo SHALL declarar quais regras são obrigatórias para cada versão do leiaute. Regras obrigatórias ainda não implementadas MUST constar do resultado como validações não realizadas.
Fonte: Manual da ECF, leiaute 12 (20/05/2026); conjunto obrigatório definido em E01.

#### Scenario: Regra obrigatória pendente
- **GIVEN** uma regra obrigatória do leiaute 12 sem validador implementado
- **WHEN** um arquivo do leiaute 12 é validado
- **THEN** o resultado lista a regra como validação não realizada
- **AND** o status não é de análise concluída

### Requirement: REQ-ESTRUT-003 Achado de erro estrutural com evidência
Cada violação SHALL gerar um achado da categoria erro estrutural com o identificador da regra, a fonte, o registro, a linha e o campo, quando disponíveis.

#### Scenario: Violação de regra estrutural
- **GIVEN** um arquivo sintético que viola a regra de teste `EST-TESTE-001` na linha 42
- **WHEN** a etapa 4 executa
- **THEN** existe um achado de erro estrutural citando `EST-TESTE-001`, a fonte e a linha 42

### Requirement: REQ-ESTRUT-004 Resultado reproduzível
A validação SHALL produzir o mesmo conjunto de achados para o mesmo arquivo e a mesma versão do catálogo de regras.

#### Scenario: Duas execuções do mesmo arquivo
- **WHEN** o mesmo arquivo é validado duas vezes com a mesma versão do catálogo
- **THEN** os achados são idênticos nas duas execuções

### Requirement: REQ-ESTRUT-005 XSD somente para XML com schema oficial
A validação por XSD SHALL ser aplicada somente a documentos XML para os quais exista schema oficial cadastrado; os demais formatos MUST usar validadores compatíveis com seu leiaute.

#### Scenario: Arquivo texto
- **GIVEN** um arquivo em formato texto
- **WHEN** a etapa 4 executa
- **THEN** nenhuma validação XSD é aplicada

### Requirement: REQ-ESTRUT-006 Falha do validador não é erro do documento
Uma exceção ou falha interna de validador SHALL gerar achado da categoria falha técnica e MUST NOT gerar erro estrutural.

#### Scenario: Validador com exceção
- **GIVEN** um validador que lança exceção durante a execução
- **WHEN** a etapa 4 executa
- **THEN** existe um achado de falha técnica para o validador
- **AND** nenhum erro estrutural é atribuído ao documento por essa falha
