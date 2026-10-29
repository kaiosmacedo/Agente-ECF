## Purpose

Receber o arquivo da ECF enviado pelo usuário ou pela orquestradora, criar a execução e validar as condições de processamento antes de ler o conteúdo completo.

## ADDED Requirements

### Requirement: REQ-RECEPCAO-001 Envio cria a execução
A Api.Agente SHALL oferecer `POST /analises`, autenticado, que recebe um arquivo, cria a execução, registra o hash do arquivo e devolve o identificador da execução e o status.

#### Scenario: Envio válido
- **WHEN** um usuário autenticado envia um arquivo sintético dentro dos limites
- **THEN** a resposta traz o identificador da execução e o status
- **AND** a execução registra o hash SHA-256 do arquivo

### Requirement: REQ-RECEPCAO-002 Tamanho acima do limite
Um arquivo acima do tamanho máximo configurado SHALL ser rejeitado na etapa 2, e as etapas seguintes MUST NOT ser executadas.

#### Scenario: Arquivo grande demais
- **GIVEN** o limite configurado de 10 MB
- **WHEN** um usuário envia um arquivo de 11 MB
- **THEN** o status da execução é de entrada rejeitada
- **AND** as etapas 3 a 9 constam como não executadas

### Requirement: REQ-RECEPCAO-003 Formato não aceito
Um arquivo em formato fora da lista configurada SHALL ser rejeitado na etapa 2, com o motivo no resultado.

#### Scenario: Formato não aceito
- **GIVEN** a lista de formatos aceitos definida em E01
- **WHEN** um usuário envia um arquivo em outro formato
- **THEN** o status é de entrada rejeitada com o motivo "formato não aceito"

### Requirement: REQ-RECEPCAO-004 Limites de processamento
A leitura do arquivo SHALL respeitar limites configuráveis de tamanho de linha e de quantidade de linhas, e o nome do arquivo MUST NOT ser usado para compor caminhos ou comandos.

#### Scenario: Linha acima do limite
- **GIVEN** um limite configurado de tamanho de linha
- **WHEN** o arquivo contém uma linha maior que o limite
- **THEN** o status é de entrada rejeitada com o motivo "linha acima do limite"

#### Scenario: Nome de arquivo com caminho
- **WHEN** um usuário envia um arquivo chamado `../../etc/passwd`
- **THEN** nenhum arquivo é lido ou gravado fora da área de processamento
