## Purpose

Aplicar no Agente-ECF o contrato SOFIA v1.0 (`SolicitacaoAnaliseSofia` e `ResultadoAnaliseSofia`), compartilhado entre os agentes da turma, com validação, correlação e tratamento de erros, sem misturá-lo ao protocolo A2A.

## ADDED Requirements

### Requirement: REQ-SOFIA-001 Solicitação validada
Uma `SolicitacaoAnaliseSofia` SHALL exigir `versaoContrato` igual a `1.0`, `idExecucao` e `idSolicitacao` em formato UUID, `usuario.nome`, `pergunta` e ao menos um documento com `idDocumento`, `nome` e `texto` não vazios. `idDocumento` MUST ser único na solicitação. Com os identificadores válidos, a falha de validação MUST gerar resultado com status `erro`, citando os campos e sem repetir os valores.

#### Scenario: Documento sem texto
- **GIVEN** uma solicitação com identificadores válidos e um documento com `texto` vazio
- **WHEN** ela é processada
- **THEN** o resultado tem status `erro` e `erro.codigo` igual a `solicitacao_invalida`
- **AND** a mensagem cita `documentos[0].texto`

#### Scenario: Versão de contrato não suportada
- **WHEN** a solicitação informa `versaoContrato` igual a `2.0`
- **THEN** o resultado tem status `erro` e `erro.codigo` igual a `versao_contrato_nao_suportada`

### Requirement: REQ-SOFIA-002 Resultado completo e correlacionado
Um `ResultadoAnaliseSofia` SHALL conter `versaoContrato`, os mesmos `idExecucao` e `idSolicitacao` da solicitação, `agente.idAgente` igual a `agente-ecf`, `agente.nome`, `status`, `analise` e `achados`. `status` MUST ser `concluido`, `parcial` ou `erro`.

#### Scenario: Round-trip do contrato
- **GIVEN** a solicitação de exemplo do contrato
- **WHEN** o resultado é serializado e lido novamente
- **THEN** os identificadores são iguais aos da solicitação
- **AND** os nomes dos campos JSON são os do contrato

### Requirement: REQ-SOFIA-003 Rastreabilidade dos documentos sem inventar evidências
O resultado SHALL registrar nas observações cada `idDocumento` recebido, com o tamanho e o hash SHA-256 do texto. O sistema MUST NOT produzir achados ou evidências sem validação que os fundamente, e MUST NOT declarar conformidade enquanto houver validação obrigatória não realizada.

#### Scenario: Dois documentos
- **GIVEN** uma solicitação com `doc-001` e `doc-002`
- **WHEN** o resultado é montado
- **THEN** as observações citam os dois identificadores com tamanho e hash corretos
- **AND** `achados` está vazio

### Requirement: REQ-SOFIA-004 Limite de tamanho sem truncar
O tamanho total dos textos SHALL respeitar o limite configurado em `A2A:TamanhoMaximoDosDocumentosEmBytes`. Acima dele, o resultado MUST ter status `erro`, com código `documentos_acima_do_limite`, e o conteúdo MUST NOT ser truncado nem analisado em parte.

#### Scenario: Documentos acima do limite
- **GIVEN** o limite configurado de 1000 bytes e um documento de 2000 bytes
- **WHEN** a solicitação é processada
- **THEN** o resultado tem status `erro` e `erro.codigo` igual a `documentos_acima_do_limite`

### Requirement: REQ-SOFIA-005 Dados pessoais e documentos fora dos logs e da memória
Os logs MUST NOT conter `usuario.nome`, `pergunta` nem o texto dos documentos. A resposta e o armazenamento das tarefas MUST NOT reter o histórico da mensagem recebida.

#### Scenario: Nome e documento fora dos logs
- **GIVEN** uma solicitação do usuário sintético `Maria Oliveira` com o texto `conteudo-sigiloso-123`
- **WHEN** a tarefa termina
- **THEN** nenhum registro de log contém `Maria Oliveira` nem `conteudo-sigiloso-123`
- **AND** a resposta e `GetTask` não contêm `conteudo-sigiloso-123`
