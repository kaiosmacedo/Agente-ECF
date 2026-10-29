## Purpose

Bloquear automaticamente a entrada na branch principal de alterações que violem a especificação, as regras de arquitetura da constitution, a qualidade do código ou a segurança das dependências.

## ADDED Requirements

### Requirement: REQ-QUALIDADE-001 Verificações obrigatórias antes do merge
Todo PR para a branch `main` SHALL executar automaticamente as verificações de especificação OpenSpec, compilação, analisadores, formatação, testes, construção das imagens de contêiner e dependências vulneráveis. O merge MUST ser bloqueado enquanto alguma verificação estiver falha ou pendente.

#### Scenario: Teste falhando
- **GIVEN** um PR para a `main` com um teste automatizado falhando
- **WHEN** as verificações terminam
- **THEN** o PR fica impedido de ser mesclado

#### Scenario: Especificação inválida
- **GIVEN** um PR que adiciona um requisito sem cenário
- **WHEN** as verificações terminam
- **THEN** a verificação de especificação falha
- **AND** o PR fica impedido de ser mesclado

### Requirement: REQ-QUALIDADE-002 Avisos tratados como erros
A compilação dos projetos SHALL tratar avisos do compilador e dos analisadores configurados como erros, e a verificação de formatação MUST falhar quando o código divergir do padrão do repositório.

#### Scenario: Aviso introduzido
- **GIVEN** uma alteração que introduz uma variável não utilizada
- **WHEN** o pipeline compila a solução
- **THEN** a compilação falha

### Requirement: REQ-QUALIDADE-003 Regras de camadas verificadas
As regras de dependência entre camadas SHALL ser verificadas por testes automatizados em cada serviço. O Dominio MUST NOT depender de Endpoints, Aplicacao, Infraestrutura, ASP.NET Core ou bibliotecas de infraestrutura; a Aplicacao MUST NOT depender de Endpoints nem de Infraestrutura.

#### Scenario: Domínio dependendo de infraestrutura
- **GIVEN** um tipo do Dominio da Api.Agente que referencia um tipo da Infraestrutura
- **WHEN** os testes de arquitetura executam
- **THEN** um teste falha e identifica o tipo infrator

### Requirement: REQ-QUALIDADE-004 Isolamento entre serviços
A Api.Agente MUST NOT referenciar código, migrações, dados ou credenciais da Api.RAG, e a Api.RAG MUST NOT referenciar os da Api.Agente. A regra SHALL ser verificada por teste automatizado.

#### Scenario: Referência cruzada entre serviços
- **GIVEN** o projeto da Api.Agente com uma referência ao projeto da Api.RAG
- **WHEN** os testes de arquitetura executam
- **THEN** um teste falha indicando a referência cruzada

### Requirement: REQ-QUALIDADE-005 Dependências vulneráveis
O pipeline SHALL falhar quando algum pacote direto ou transitivo da solução tiver vulnerabilidade conhecida publicada.

#### Scenario: Pacote vulnerável
- **GIVEN** a solução com um pacote que possui vulnerabilidade conhecida
- **WHEN** o pipeline verifica as dependências
- **THEN** a verificação falha e cita o pacote
