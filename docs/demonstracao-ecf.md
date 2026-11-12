# Demonstração da validação da ECF

Abrir https://agente-ecf-agente.vercel.app/swagger e expandir `POST /analises`. Clicar em `Try it out`, escolher a ECF no campo **arquivo** e clicar em `Execute`. Depois de uma atualização da API, recarregar a página para buscar o OpenAPI novo.

Só usar arquivos sintéticos ou anonimizados, conforme SQ-12.

## Arquivos e resultados

| Arquivo do repositório | Resultado esperado |
| --- | --- |
| `tests/Api.Agente.Tests/Dados/ecf/gabarito/01-valor-fora-da-lista.txt` | HTTP 201, veredito `recusado`, achado `EST-CAMPO-006 0000.IND_SIT_INI_PER`, linha 1, valor encontrado 9. A orientação indica os valores de 0 a 8, com fonte no Manual do leiaute 12, página 63. |
| `tests/Api.Agente.Tests/Dados/ecf/gabarito/00-valida.txt` | HTTP 201, veredito `parcial`, sem achado estrutural. As validações pendentes aparecem em `limitacoes`; não significa conformidade fiscal. |

O HTTP 201 confirma que o serviço processou e registrou a análise. O veredito `recusado` descreve o arquivo analisado. HTTP 400 com falta da parte `arquivo` é erro de envio, antes da análise, e não um achado fiscal.

## Contrato do upload

`POST /analises` recebe `multipart/form-data`, com o arquivo na parte obrigatória chamada `arquivo`. No OpenAPI, ela fica em `requestBody.content["multipart/form-data"].schema.properties.arquivo`, como string/binary em um objeto. O Swagger precisa dessa propriedade para montar o anexo com o nome correto (REQ-DOCAPI-005).

A correção `corrigir-upload-ecf-swagger` atende o defeito relatado em 12/11/2026: o seletor aceitava a escolha do arquivo, mas o binário descrito na raiz não gerava a parte nomeada.
