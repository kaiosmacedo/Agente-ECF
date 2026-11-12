# Proposal: Corrigir o envio da ECF pelo Swagger

## Why

Em 12/11/2026, Kaio Macedo selecionou `ECF-DEMO-com-erro.txt` no Swagger e recebeu HTTP 400: falta a parte `arquivo`. O documento de produção descreve `multipart/form-data` como o binário `IFormFile` na raiz. O seletor aparece, mas o Swagger não monta a parte nomeada exigida por REQ-RECEPCAO-001. O envio direto do mesmo arquivo com a parte `arquivo` retornou HTTP 201 e o veredito `recusado`, confirmando que o defeito está na documentação do upload.

## What Changes

- Descreve o corpo multipart como objeto com a propriedade binária obrigatória `arquivo`.
- Testa o schema publicado e uma requisição HTTP construída com o nome declarado nele, incluindo a recusa quando o arquivo não é anexado.
- Registra o resultado da conferência em produção com os dois arquivos sintéticos da demonstração.

## Capabilities

### New Capabilities

- Nenhuma.

### Modified Capabilities

- `plataforma/documentacao-api`: documentar corretamente a parte do formulário para executar o upload no Swagger.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | A seleção do arquivo ocorreu? | Sim, a segunda imagem do usuário mostra o arquivo selecionado. A primeira mostra um curl sem parte de formulário. | Kaio Macedo e imagens do pedido | 2026-11-12 | Não |
| E02 | Mudar o parser, as regras ou os limites? | Não. Corrigir a descrição para o contrato já implementado, que exige `arquivo`. | REQ-RECEPCAO-001 e AnalisesEndpoints | 2026-11-12 | Não |
| E03 | Publicar e conferir? | Correção necessária ao pedido de deixar a API utilizável para a demonstração. O merge segue a delegação da seção 6.6, com CI verde. | Pedido do usuário e constitution 2.1.0 | 2026-11-12 | Não |

## Fontes oficiais

Sem nova regra fiscal ou contábil. Fontes técnicas: [Swagger, upload multipart](https://swagger.io/docs/specification/v3_0/describing-request-body/file-upload/) e [ASP.NET Core 10, transformadores OpenAPI](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-10.0), consultadas em 12/11/2026.

## Princípios e dependências

P06, P12, P16, P20, GD-01 e SQ-09/SQ-12. Depende de `documentacao-api` e `recepcao-identificacao-leiaute`, arquivadas. Nenhuma DP alterada. Skills: `testes-agentes-ia` e `github-versionamento`.

## Fora do escopo

Regras fiscais, LLM/RAG, autenticação, formato do resultado, parser em fluxo, limites e a proposta `perguntas-ecf` de outra frente. Somente dados sintéticos nos testes.

## Impact

Metadados de `POST /analises`, testes de documentação, guia da demonstração, roteiro e cofre Obsidian. Sem pacote, serviço, banco ou configuração novos.
