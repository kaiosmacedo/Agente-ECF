## ADDED Requirements

### Requirement: REQ-DOCAPI-005 Upload da ECF com a parte nomeada
O documento OpenAPI de `POST /analises` SHALL descrever um corpo obrigatório `multipart/form-data` como objeto com a propriedade obrigatória `arquivo`, do tipo string e formato binary, para que o Swagger envie o conteúdo em uma parte de formulário com esse nome, conforme REQ-RECEPCAO-001.

#### Scenario: Documento de upload
- **WHEN** o cliente pede `GET /openapi/v1.json`
- **THEN** o corpo de `POST /analises` tem schema de objeto
- **AND** sua propriedade `arquivo` é string/binary e está em `required`

#### Scenario: Envio da ECF pela parte declarada
- **GIVEN** o cliente usa o nome da propriedade de arquivo declarado no OpenAPI
- **AND** uma ECF sintética do leiaute 12 com IND_SIT_INI_PER igual a 9
- **WHEN** envia o conteúdo no multipart de `POST /analises`
- **THEN** recebe HTTP 201, veredito `recusado` e o achado `EST-CAMPO-006` em `0000.IND_SIT_INI_PER`, linha 1

#### Scenario: Arquivo não anexado
- **WHEN** o cliente envia multipart sem a parte de arquivo `arquivo`
- **THEN** recebe HTTP 400 controlado, informando a parte ausente
- **AND** o serviço continua disponível
