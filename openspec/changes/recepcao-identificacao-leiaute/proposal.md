# Proposal: Recepção do arquivo e identificação do leiaute

> **Estado:** especificada para validação de Kaio Macedo (roteiro, mudança 07). Plano técnico e tarefas só depois da aprovação e dos esclarecimentos bloqueantes.

## Why

As etapas 1 a 3 do fluxo (P03) recebem o arquivo, validam tamanho, formato e condições de processamento e identificam o tipo de documento e a versão do leiaute. Sem essa identificação, nenhuma validação dependente de versão pode rodar, e o resultado não pode declarar conformidade. É o primeiro endpoint de análise usado pela Sofia INFIS e pelos usuários.

## What Changes

- Cria `POST /analises`, autenticado, que recebe o arquivo, cria a execução e registra o hash (etapa 1).
- Valida tamanho, formato e limites de processamento antes de ler o conteúdo completo (etapa 2).
- Identifica o tipo de documento e a versão do leiaute pelos campos definidos no Manual da ECF (etapa 3).
- Trata versão não suportada ou identificação ambígua como limitação, sem executar validações dependentes de versão.
- Aplica a política de armazenamento do arquivo recebido (DP-11, GD-06).

## Capabilities

### New Capabilities
- `agente/recepcao-arquivos`: recebimento do arquivo, criação da execução e validação das condições de processamento.
- `agente/identificacao-leiaute`: identificação do tipo de documento e da versão do leiaute.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Quais formatos aceitar: o arquivo texto da ECF, compactado (ZIP), outros (DP-05)? | — | Kaio Macedo | — | Sim |
| E02 | Qual o tamanho máximo esperado de um arquivo ECF real (MB) (DP-05)? | — | Kaio Macedo | — | Sim |
| E03 | A análise responde na mesma requisição ou de forma assíncrona, com consulta posterior (DP-22)? O limite de duração das funções do Vercel pesa nessa escolha. | — | Kaio Macedo | — | Sim |
| E04 | O arquivo recebido deve ser guardado? Por quanto tempo e onde (DP-11, GD-06)? | — | Kaio Macedo | — | Sim, para dados reais |
| E05 | Registro e campos do Manual da ECF, leiaute 12, que identificam o tipo e a versão do leiaute, e a codificação de caracteres do arquivo. A extração é feita do manual e validada por Kaio Macedo (P01). | — | Extração do manual, validação de Kaio Macedo | — | Sim |
| E06 | A primeira entrega suporta só o leiaute 12, ou também versões anteriores (DP-04)? | — | Kaio Macedo | — | Sim |

## Fontes oficiais

| Documento | Emissor | Versão ou vigência | Localização |
| --- | --- | --- | --- |
| Manual da ECF, leiaute 12 | Receita Federal do Brasil, portal SPED | Publicado em 20/05/2026 | Registro e campos de identificação: a extrair (E05) |

## Princípios, decisões e dependências

- **Princípios:** P01, P03, P05, P08, P10, P19; GD-06; SQ-01, SQ-05.
- **Decisões pendentes:** resolve DP-05 e DP-22; depende de DP-04 e DP-11; usa DP-25 (mudança 03).
- **Depende de:** 03 `execucao-e-resultado` (e, por ela, 02).
- **Skills:** `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Validação estrutural completa do arquivo (mudança 08) e regras de negócio (09).
- Correção ou geração de arquivos (constitution, 1.3).

## Impact

- Endpoint `POST /analises`, leitura segura de arquivos grandes e, conforme E04, armazenamento de objetos.

## Aprovação

Pendente da validação de Kaio Macedo.
