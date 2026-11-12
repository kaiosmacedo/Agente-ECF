# plataforma/documentacao-api Specification

## Purpose
Oferecer, em cada serviço do Agente-ECF, uma página de documentação navegável da API, gerada do contrato OpenAPI publicado, para quem integra o agente consultar e testar as rotas.

## Requirements

### Requirement: REQ-DOCAPI-001 Interface do Swagger em /swagger
A Api.Agente SHALL servir em `/swagger` a interface do Swagger com o documento OpenAPI da própria API, em todos os ambientes, inclusive produção.

#### Scenario: Abrir a documentação
- **WHEN** um navegador pede `GET /swagger`
- **THEN** a resposta é a página da interface do Swagger, direto ou por redirecionamento para `/swagger/index.html`
- **AND** a página carrega o documento de `/openapi/v1.json`

### Requirement: REQ-DOCAPI-002 Servidor relativo no documento
O documento OpenAPI SHALL declarar o servidor de forma relativa, e MUST NOT anunciar um endereço `http://`, para que as chamadas de teste da página usem o esquema e o host da própria página.

#### Scenario: Serviço atrás de proxy com TLS
- **GIVEN** o serviço recebe a requisição em http, atrás de um proxy que termina o https, como no Vercel
- **WHEN** um cliente pede `GET /openapi/v1.json`
- **THEN** o servidor declarado é `/`
- **AND** o documento não contém `http://` em `servers`

### Requirement: REQ-DOCAPI-003 Página sem recurso externo e sem segredo
A página MUST NOT carregar script ou estilo de outro domínio, e o documento OpenAPI MUST NOT conter segredo nem valor de configuração.

#### Scenario: Recursos da página
- **WHEN** a página `/swagger/index.html` é carregada
- **THEN** todo script e estilo que ela referencia vem do próprio serviço

#### Scenario: Configuração com segredo
- **GIVEN** o serviço configurado com uma conexão de banco e uma chave sintéticas
- **WHEN** um cliente pede `GET /openapi/v1.json`
- **THEN** nenhum desses valores aparece no documento

### Requirement: REQ-DOCAPI-004 Rotas publicadas no documento
O documento OpenAPI SHALL listar as rotas públicas da Api.Agente: `POST /analises`, `GET /analises/{id}`, `POST /perguntas`, `POST /a2a` e `GET /.well-known/agent-card.json`.

#### Scenario: Rotas documentadas
- **WHEN** um cliente pede `GET /openapi/v1.json`
- **THEN** o documento traz essas cinco rotas, com os métodos indicados

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
