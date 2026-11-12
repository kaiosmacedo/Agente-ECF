# Design: Upload multipart nomeado no OpenAPI

## Context

`Accepts<IFormFile>` gera uma referência binária na raiz do multipart. O parser lê as seções e exige `Content-Disposition` com `name="arquivo"`. A documentação precisa expressar esse nome, preservando o parser em fluxo e os limites existentes.

## Decisions

1. Acrescentar um transformador OpenAPI somente à operação `POST /analises`, no próprio mapeamento. O corpo obrigatório usa `multipart/form-data`, schema de objeto, `properties.arquivo` string/binary e `required: [arquivo]`. Reutilizar `ParteDoArquivo` para evitar nomes divergentes.
2. Não alterar o tratamento HTTP nem usar binding automático de formulário, que mudaria a leitura em fluxo. Sem pacote adicional, ADR ou parâmetro novo.
3. Testar o documento, usar a propriedade nele declarada para construir e enviar multipart com ECF sintética e confirmar o achado. Enviar multipart sem a parte para manter a recusa controlada. A UI deve renderizar o campo nomeado, e a produção deve receber o arquivo.
4. Executar formatação, build, suíte .NET, OpenSpec estrito e CI. Publicar por PR com a delegação existente. Conferir o schema e o upload em produção antes de fechar a validação de implantação. Arquivar após todas as tarefas e documentar no Obsidian.

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Não se aplica | Nenhuma regra fiscal nova. |
| P02 | Atende | Validadores preservados. |
| P03 | Atende | Fluxo de análise preservado e exercitado pelo teste HTTP. |
| P04 | Atende | Categoria do achado de teste permanece erro estrutural. |
| P05 | Atende | Veredito recusado no erro conhecido e parcial no controle. |
| P06 | Atende | Teste confere registro e fundamento do achado. |
| P07 | Não se aplica | Sem LLM. |
| P08 | Atende | Limitações existentes preservadas. |
| P09 | Atende | Revisão humana existente preservada. |
| P10 | Atende | O arquivo permanece dado para o parser existente. |
| P11 | Não se aplica | Sem alteração de recuperação. |
| P12 | Atende | Contrato OpenAPI corrigido e testado. |
| P13 | Atende | Metadados continuam no sistema. |
| P14 | Não se aplica | Nenhuma ferramenta nova. |
| P15 | Atende | Sem mudança na fronteira MCP ou dados. |
| P16 | Atende | Transformador localizado no endpoint, sem dependência no domínio. |
| P17 | Não se aplica | Não há chamada paga. |
| P18 | Não se aplica | Sem memória. |
| P19 | Atende | Execução HTTP persiste pelos mecanismos existentes. |
| P20 | Atende | Change validado antes do código, com tarefas e cenários. |

## Skills

`testes-agentes-ia` e `github-versionamento` lidas. Preconditions: .NET 10 disponível; Docker iniciado para Testcontainers; remoto e branch inspecionados. Worktree isolada de `origin/main`, sem modificar a branch de proposta `perguntas-ecf` da outra frente.

## Risks / Trade-offs

O schema pode estar correto e o navegador ainda reter o documento antigo. Conferir o OpenAPI publicado e orientar recarregar a página. Testes de contrato complementam o teste no navegador, sem provar a renderização visual sozinhos.
