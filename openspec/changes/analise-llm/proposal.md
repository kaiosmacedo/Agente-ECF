# Proposal: Análise com LLM

> **Estado:** especificada para validação de Kaio Macedo (roteiro, mudança 10). Plano técnico e tarefas só depois da aprovação e dos esclarecimentos bloqueantes.

## Why

As etapas 7 e 8 do fluxo (P03) produzem a análise com o LLM e validam a saída estruturada. O modelo explica, prioriza e aponta indícios sobre o que os validadores já determinaram (P02), sem fabricar evidências (P07), sem ser manipulado pelo conteúdo do arquivo (P10) e sempre com saída estruturada validada antes do aceite (P12).

## What Changes

- Orquestra a etapa 7 com Semantic Kernel e OpenAI, com modelo e parâmetros por configuração externa.
- Envia ao modelo somente os trechos necessários: achados determinísticos, trechos do manual delimitados e partes do arquivo, sem segredos.
- Exige saída estruturada no contrato JSON versionado do LLM e a valida em sintaxe, schema e semântica antes do aceite.
- Aplica retentativas limitadas; esgotadas, registra falha técnica e mantém os achados determinísticos.
- Registra cada tentativa como consumo e a versão do template de prompt para auditoria.
- Versiona os prompts no repositório.

## Capabilities

### New Capabilities
- `agente/analise-llm`: produção e validação da análise do LLM sobre os resultados determinísticos e os trechos do manual.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Qual modelo da OpenAI usar, com suporte a saída estruturada, e quais parâmetros (DP-02)? | — | Kaio Macedo | — | Sim |
| E02 | Qual a política de dados com a OpenAI: retenção, região, uso para treinamento, anonimização (DP-12)? | — | Kaio Macedo | — | Sim, para dados reais |
| E03 | Quantas retentativas e qual timeout para o LLM (DP-14)? | — | Plano técnico | — | Não |
| E04 | Idioma, tom e tamanho da síntese entregue ao usuário e à Sofia. | — | Kaio Macedo | — | Não |
| E05 | Política para referência não verificável: rejeitar o item ou reclassificar (DP-23, da mudança 03). | — | Kaio Macedo | — | Sim, se não resolvida em 03 |

## Fontes oficiais

Não se aplica diretamente: o LLM usa os trechos recuperados das fontes ingeridas e não cria regras.

## Princípios, decisões e dependências

- **Princípios:** P02, P04, P07, P09, P10, P12, P13, P17, P19; RA-06; GD-02, GD-06; SQ-06, SQ-09.
- **Decisões pendentes:** resolve DP-02 (modelo de geração), DP-14 e DP-23; depende de DP-12 para dados reais.
- **Depende de:** 04 `registro-consumo-ia`, 06 `recuperacao-conhecimento-mcp` e 08 `validacao-estrutural`.
- **Skills:** `semantic-kernel-tools`, `controle-custos-llm`, `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Ferramentas externas (mudança 11) e memória conversacional (12).
- Avaliação de qualidade com o modelo real dentro do CI (feita fora do CI, conforme SQ-09).

## Impact

- Integração com a OpenAI e o Semantic Kernel na infraestrutura da Api.Agente, contrato JSON do LLM, prompts versionados e conjunto de avaliação.

## Aprovação

Pendente da validação de Kaio Macedo.
