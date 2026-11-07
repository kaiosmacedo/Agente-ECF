# agente/identificacao-leiaute Specification

## Purpose
Identificar o tipo de documento e a versão do leiaute de cada arquivo recebido, conforme os campos definidos no manual oficial, antes de qualquer validação dependente de versão.

## Requirements

### Requirement: REQ-LEIAUTE-001 Identificação pelo manual oficial
A etapa 3 SHALL identificar o leiaute pelo campo `COD_VER` do Registro 0000, conforme a observação desse campo no manual de cada leiaute, e registrar na execução o tipo ECF, o leiaute e o valor de `COD_VER`.
Fonte: observação do campo 3 do Registro 0000 nos manuais da ECF: leiaute 7, p. 55 (`0007`); leiaute 8, p. 56 (`0008`); leiaute 9, p. 55 (`0009`); leiaute 10, p. 60 (`00010` no texto, `0010` no exemplo da p. 65); leiaute 11, p. 59 (`0011`); leiaute 12, p. 63 (`0012`).

#### Scenario: Arquivo do leiaute 12
- **GIVEN** um arquivo sintético com `COD_VER` igual a `0012` e `DT_FIN` em 2025
- **WHEN** a etapa 3 executa
- **THEN** a execução registra o tipo ECF, o leiaute 12 e o `COD_VER` `0012`

#### Scenario: Arquivo do leiaute 10
- **GIVEN** um arquivo sintético com `COD_VER` igual a `0010` e `DT_FIN` em 2023
- **WHEN** a etapa 3 executa
- **THEN** a execução registra o leiaute 10

### Requirement: REQ-LEIAUTE-002 Versão não suportada
Quando `COD_VER` tiver quatro caracteres e não for de um leiaute de 7 a 12, a etapa 3 SHALL terminar concluída com o valor registrado, as validações dependentes de versão MUST NOT ser executadas, e a limitação SHALL constar do resultado.

#### Scenario: Versão sem suporte
- **GIVEN** um arquivo sintético com `COD_VER` igual a `0013`
- **WHEN** a etapa 3 executa
- **THEN** o valor `0013` é registrado
- **AND** as etapas 4 e 5 constam como não executadas com a limitação "versão do leiaute não suportada"

### Requirement: REQ-LEIAUTE-003 Identificação ambígua ou impossível
Se `COD_VER` estiver vazio ou fora do formato, ou se o leiaute não puder ser identificado de forma inequívoca, a etapa 3 SHALL terminar com falha atribuída ao documento, e o resultado MUST NOT declarar conformidade. Essa falha MUST NOT ser apresentada como falha técnica: o status SHALL ser `parcial`.

#### Scenario: Campo de versão ausente
- **GIVEN** um arquivo sintético com `COD_VER` vazio
- **WHEN** a etapa 3 executa
- **THEN** a etapa consta com falha atribuída ao documento
- **AND** o status é `parcial`, e não `falha-tecnica`

#### Scenario: Falha interna do identificador
- **GIVEN** um identificador que lança exceção
- **WHEN** a etapa 3 executa
- **THEN** a etapa consta com falha técnica
- **AND** o status é `falha-tecnica`

### Requirement: REQ-LEIAUTE-004 Leiaute coerente com o período
O leiaute de `COD_VER` SHALL ser aceito só quando o ano de `DT_FIN` for o ano-calendário dele, ou o seguinte com `SIT_ESPECIAL` de 1 a 6. Caso contrário, ou com `DT_FIN` fora do formato `ddmmaaaa`, a identificação MUST ser tratada como ambígua (REQ-LEIAUTE-003), com uma limitação que cita o `COD_VER` e o ano.
Fonte: Manual da ECF, leiaute 12 (23/07/2026), p. 10, 34, 63 e 64; observação do `COD_VER` em cada manual (REQ-LEIAUTE-001).

#### Scenario: Situação especial no ano seguinte
- **GIVEN** um arquivo sintético com `COD_VER` `0012`, `DT_FIN` em 2026 e `SIT_ESPECIAL` igual a `1`
- **WHEN** a etapa 3 executa
- **THEN** a execução registra o leiaute 12

#### Scenario: Versão de outro ano
- **GIVEN** um arquivo sintético com `COD_VER` `0012`, `DT_FIN` em 2023 e `SIT_ESPECIAL` igual a `0`
- **WHEN** a etapa 3 executa
- **THEN** a etapa consta com falha atribuída ao documento
- **AND** a limitação cita o `COD_VER` `0012` e o ano 2023
