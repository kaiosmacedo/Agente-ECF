## Purpose

Garantir que cada serviço do Agente-ECF obtenha a configuração de fontes externas ao código e se recuse a iniciar com configuração obrigatória ausente ou inválida, sem revelar valores.

## ADDED Requirements

### Requirement: REQ-CONFIG-001 Configuração externa ao código
Cada serviço SHALL obter sua configuração de arquivos de configuração sem segredos e de variáveis de ambiente, com as variáveis de ambiente prevalecendo sobre os arquivos. Os arquivos de configuração versionados MUST NOT conter valores secretos.

#### Scenario: Variável de ambiente prevalece sobre o arquivo
- **GIVEN** o arquivo de configuração do serviço define `Observabilidade:NivelMinimo` como `Information`
- **AND** a variável de ambiente `Observabilidade__NivelMinimo` vale `Warning`
- **WHEN** o serviço inicia
- **THEN** o nível mínimo de log efetivo é `Warning`

#### Scenario: Arquivos versionados sem segredos
- **WHEN** o pipeline verifica os arquivos de configuração versionados de cada serviço
- **THEN** nenhuma chave cujo nome indique segredo (senha, chave, token, segredo ou connection string) possui valor preenchido

### Requirement: REQ-CONFIG-002 Validação na inicialização
Cada serviço SHALL validar todas as opções obrigatórias que declara antes de aceitar requisições. Com alguma opção ausente ou inválida, o serviço MUST encerrar a inicialização com erro que identifica a opção pelo nome, sem revelar seu valor.

#### Scenario: Opção obrigatória ausente
- **GIVEN** um serviço que declara a opção obrigatória `Exemplo:Endereco`
- **AND** a opção não está definida em nenhuma fonte
- **WHEN** o serviço inicia
- **THEN** a inicialização termina com erro
- **AND** a mensagem de erro cita `Exemplo:Endereco`

#### Scenario: Opção inválida sem revelar o valor
- **GIVEN** um serviço que exige que `Exemplo:Endereco` seja uma URL absoluta
- **AND** a opção vale `segredo-nao-url`
- **WHEN** o serviço inicia
- **THEN** a inicialização termina com erro que cita `Exemplo:Endereco`
- **AND** nem a mensagem de erro nem os logs contêm `segredo-nao-url`
