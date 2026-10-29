## Purpose

Definir o envelope da API e o contrato dos achados do Agente-ECF, com categorias, evidências, severidade, limitações e revisão humana, preenchidos e validados pelo sistema.

## ADDED Requirements

### Requirement: REQ-RESULT-001 Envelope versionado preenchido pelo sistema
O envelope SHALL conter a versão do contrato, o identificador da execução e de correlação, o status, os estados das etapas, os achados, as limitações, a indicação de revisão humana, as datas e os tempos. Esses metadados MUST ser preenchidos pelo sistema.

#### Scenario: Envelope de uma execução
- **WHEN** um usuário consulta o resultado de uma execução
- **THEN** o envelope informa a versão do contrato, o identificador da execução e o identificador de correlação
- **AND** esses valores são os registrados pelo sistema

### Requirement: REQ-RESULT-002 Categoria única e origem permitida
Cada achado SHALL ter exatamente uma categoria entre erro estrutural, inconsistência de negócio, indício da IA, ausência de evidências e falha técnica, e uma origem. Achado com categoria incompatível com a origem MUST ser rejeitado.

#### Scenario: Indício com origem determinística
- **WHEN** um achado com categoria indício da IA é registrado com origem validador
- **THEN** o registro é rejeitado

#### Scenario: Erro estrutural vindo do LLM
- **WHEN** um achado com categoria erro estrutural é registrado com origem LLM
- **THEN** o registro é rejeitado

### Requirement: REQ-RESULT-003 Evidência mínima
Cada achado SHALL informar a execução, a origem, a regra, requisito ou fonte que o fundamenta e os registros envolvidos, com linha ou posição e campo quando disponíveis. Achado sem esses campos MUST ser rejeitado.

#### Scenario: Achado sem fundamento
- **WHEN** um achado é registrado sem regra, requisito ou fonte
- **THEN** o registro é rejeitado

### Requirement: REQ-RESULT-004 Severidade dentro da escala
Cada achado SHALL ter severidade da escala definida nesta mudança (E02), e a severidade MUST NOT substituir a categoria.

#### Scenario: Severidade fora da escala
- **WHEN** um achado é registrado com severidade inexistente na escala
- **THEN** o registro é rejeitado

### Requirement: REQ-RESULT-005 Revisão humana calculada
A indicação de revisão humana SHALL ser calculada pelo sistema e MUST ser positiva quando houver falha técnica, validação obrigatória não concluída, indício da IA ou ausência de evidências.

#### Scenario: Indício da IA presente
- **GIVEN** uma execução com um achado da categoria indício da IA
- **WHEN** o resultado é montado
- **THEN** a indicação de revisão humana é positiva

#### Scenario: Recomendação de dispensa ignorada
- **GIVEN** uma execução com ausência de evidências e uma recomendação do modelo para dispensar a revisão
- **WHEN** o resultado é montado
- **THEN** a indicação de revisão humana continua positiva

### Requirement: REQ-RESULT-006 Limitações e validações não realizadas
O resultado SHALL listar cada validação obrigatória não realizada, com o motivo, e as limitações da análise.

#### Scenario: Manual da versão indisponível
- **GIVEN** uma execução cuja consulta ao manual não encontrou a versão aplicável
- **WHEN** o resultado é montado
- **THEN** a limitação consta do resultado com o motivo
