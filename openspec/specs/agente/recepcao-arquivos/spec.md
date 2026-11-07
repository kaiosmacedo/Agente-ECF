# agente/recepcao-arquivos Specification

## Purpose
Receber o arquivo da ECF enviado pelo usuário ou pela orquestradora, criar a execução e validar as condições de processamento antes de ler o conteúdo completo.

## Requirements

### Requirement: REQ-RECEPCAO-001 Envio cria a execução
A Api.Agente SHALL oferecer `POST /analises`, sem exigir credencial na primeira versão (SQ-12), que recebe um arquivo, cria a execução, registra o hash do arquivo e devolve o identificador da execução e o status.

#### Scenario: Envio válido
- **WHEN** um usuário envia um arquivo sintético dentro dos limites
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

### Requirement: REQ-RECEPCAO-005 Formato aceito: texto da ECF
A etapa 2 SHALL aceitar como ECF o documento em texto cuja primeira linha é o Registro 0000, com `REG` igual a `0000` e `NOME_ESC` igual a `LECF`, delimitados pelo caractere `|`. Pelo `POST /analises`, o arquivo SHALL ser lido como ISO 8859-1; por A2A, o texto chega já decodificado pela Sofia, e o hash é calculado sobre ele (GD-06).
Fonte: Manual da ECF, leiaute 12 (23/07/2026), seção 2.2, p. 32 (formato texto, ISO 8859-1, delimitador `|`), e Registro 0000, p. 63 (`REG` e `NOME_ESC`).

#### Scenario: ECF sintética
- **GIVEN** um texto sintético cuja primeira linha começa com `|0000|LECF|`
- **WHEN** a etapa 2 executa
- **THEN** o documento é aceito como ECF

#### Scenario: Documento que não é ECF
- **GIVEN** um texto sintético que começa com `<NFe>`
- **WHEN** a etapa 2 executa
- **THEN** o documento não é aceito como ECF

### Requirement: REQ-RECEPCAO-006 Uma ECF por solicitação
Cada solicitação SHALL ter exatamente um documento aceito como ECF. Os demais documentos MUST constar das observações como não analisados por este agente, sem virar achado. Sem nenhuma ECF, a etapa 2 SHALL rejeitar a solicitação com o código `nenhuma_ecf`; com mais de uma, com o código `mais_de_uma_ecf`.

#### Scenario: ECF e outro documento
- **GIVEN** uma solicitação com uma ECF sintética e uma NF-e sintética
- **WHEN** a solicitação é processada
- **THEN** a ECF é analisada
- **AND** as observações informam que a NF-e não foi analisada por este agente

#### Scenario: Nenhuma ECF
- **GIVEN** uma solicitação só com uma NF-e sintética
- **WHEN** a solicitação é processada
- **THEN** o status é `entrada-rejeitada` com o código `nenhuma_ecf`

#### Scenario: Duas ECF
- **GIVEN** uma solicitação com duas ECF sintéticas
- **WHEN** a solicitação é processada
- **THEN** o status é `entrada-rejeitada` com o código `mais_de_uma_ecf`

### Requirement: REQ-RECEPCAO-007 Documento cortado antes de chegar ao agente
Um documento que termina com o marcador `[DOCUMENTO TRUNCADO]`, que o código atual da Sofia acrescenta ao cortar cada arquivo em 20.000 caracteres (`docs/base-sofiainfis.md`), SHALL ter o tipo e a versão identificados, mas as validações que dependem do arquivo inteiro MUST NOT ser executadas, e o resultado SHALL declarar a limitação "documento cortado antes de chegar ao agente".

#### Scenario: ECF cortada pela Sofia
- **GIVEN** uma ECF sintética do leiaute 12 que termina com `[DOCUMENTO TRUNCADO]`
- **WHEN** a solicitação é processada
- **THEN** a execução registra o tipo ECF e a versão 12
- **AND** as etapas 4 e 5 constam como não executadas
- **AND** o resultado lista a limitação "documento cortado antes de chegar ao agente"
- **AND** nenhum achado estrutural é produzido

### Requirement: REQ-RECEPCAO-008 Mesmo resultado nos dois canais
O `POST /analises` e o `SendMessage` por A2A SHALL usar o mesmo serviço de aplicação, de modo que o mesmo texto de ECF produza as mesmas etapas, o mesmo status, o mesmo veredito e os mesmos achados nos dois canais.

#### Scenario: Mesma ECF pelos dois canais
- **GIVEN** uma ECF sintética sem acentos
- **WHEN** ela é enviada pelo `POST /analises` e por A2A
- **THEN** os dois resultados têm o mesmo status, o mesmo veredito e os mesmos achados
