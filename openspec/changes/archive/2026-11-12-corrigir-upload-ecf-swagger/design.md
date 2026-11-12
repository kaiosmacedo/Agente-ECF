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

## Validação executada em 12/11/2026

- Antes da correção, os três casos novos falharam por falta do schema de objeto/propriedade. Depois: documentação 9/9, build Release sem avisos ou erros, formatação limpa e suíte completa 542/542, sem ignorados. OpenSpec estrito válido antes do código e em 23 artefatos após integrar a proposta 17.
- PR #47 aprovado pela delegação de Kaio Macedo, com os checks OpenSpec e .NET verdes no CI #121, incluindo contêineres. Merge `1bca6fc`. CI de push #122 e CD aprovados; ambos os serviços publicados e saudáveis.
- Reproduzido no navegador o defeito antigo: arquivo selecionado, curl sem `-F`, HTTP 400 por parte ausente.
- Após a implantação, o documento publicado mostra objeto com `arquivo` obrigatório string/binary. Recarregada a página, o mesmo arquivo sintético foi selecionado pelo formulário e enviado pelo botão Execute: curl com `-F arquivo=@ECF-DEMO-com-erro.txt;type=text/plain`, HTTP 201, execução `0af70c01-d5c5-42a7-9c12-eeceef6c904c`, veredito `recusado` e um achado `EST-CAMPO-006 0000.IND_SIT_INI_PER`, linha 1. Resposta recuperada por GET e conferida; versão `0.1.0+1bca6fce9f65b810036f8839828bffb051b2246e`.
- Controle sem erro estrutural por REST: HTTP 201, execução `197baa81-50b5-48b5-927e-dc87b9a5ef51`, veredito `parcial` e zero achados. Não declara conformidade fiscal.
- Evidências extensas no Data da apresentação, síntese e links no Obsidian. Página do Swagger marcada como entrega para continuar aberta. Nenhum dado real de cliente, chamada à OpenAI, assinatura ou transmissão fiscal.

Definition of Done: requisitos/cenários ligados a testes, metadados na camada de endpoint, contratos HTTP existentes preservados, sem pacote/configuração ou desvio novo. P07/P10/P11/P14/P17/P18 não ganham comportamento novo; avaliações de IA, migração e ADR não se aplicam. Revisão pela delegação da seção 6.6 e documentação conferidas. O arquivamento consolida apenas REQ-DOCAPI-005 e preserva a proposta 17 aberta.
