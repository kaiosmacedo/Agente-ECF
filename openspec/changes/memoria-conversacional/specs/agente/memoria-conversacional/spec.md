## Purpose

Guardar e recuperar o histórico das conversas com o Agente-ECF com isolamento por usuário, conversa e organização, retenção definida e separação do conhecimento oficial.

## ADDED Requirements

### Requirement: REQ-MEMORIA-001 Isolamento do histórico
O histórico SHALL ser isolado por usuário, conversa e organização, com base na identidade autenticada. A consulta de conversa de outro usuário ou organização MUST responder como não encontrada.

#### Scenario: Conversa de outro usuário
- **GIVEN** uma conversa do usuário A em `org-a`
- **WHEN** o usuário B de `org-a` tenta ler essa conversa
- **THEN** a resposta é de conversa não encontrada

### Requirement: REQ-MEMORIA-002 Histórico limitado e em ordem
O histórico enviado ao modelo SHALL respeitar o limite configurável de mensagens e manter a ordem cronológica.

#### Scenario: Conversa longa
- **GIVEN** uma conversa com 50 mensagens e o limite configurado de 20
- **WHEN** o histórico é preparado para o modelo
- **THEN** são enviadas as 20 mensagens mais recentes, em ordem cronológica

### Requirement: REQ-MEMORIA-003 Retenção e exclusão
As mensagens SHALL expirar conforme a política de retenção configurada, e o usuário MUST poder excluir uma conversa sua, que deixa de ser recuperável.

#### Scenario: Exclusão pelo usuário
- **WHEN** o usuário exclui uma conversa sua
- **THEN** a conversa não é mais recuperada nem enviada ao modelo

### Requirement: REQ-MEMORIA-004 Memória não é fonte
A memória MUST NOT ser usada como fonte de regra, evidência ou referência. Um item da análise que cite a memória como referência SHALL ser rejeitado.

#### Scenario: Referência à memória
- **GIVEN** uma resposta simulada do modelo que cita uma mensagem anterior da conversa como evidência
- **WHEN** a resposta é validada
- **THEN** o item é rejeitado

### Requirement: REQ-MEMORIA-005 Conteúdo sensível fora da memória
A memória MUST NOT guardar o arquivo ECF completo, segredos ou credenciais.

#### Scenario: Mensagem com o arquivo anexado
- **WHEN** uma conversa se refere a um arquivo analisado
- **THEN** a memória guarda a referência à execução, e não o conteúdo do arquivo

### Requirement: REQ-MEMORIA-006 Memória indisponível
Se o armazenamento de memória estiver indisponível, o agente SHALL continuar sem histórico, registrar a limitação no resultado e MUST NOT inventar histórico.

#### Scenario: Armazenamento fora do ar
- **GIVEN** o armazenamento de memória indisponível
- **WHEN** um usuário faz uma pergunta de acompanhamento
- **THEN** a resposta é produzida sem histórico
- **AND** a limitação "histórico indisponível" é registrada
