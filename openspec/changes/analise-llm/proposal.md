# Proposal: Análise com LLM

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 10; PR #4). Kaio Macedo decidiu E01, E02 e E06 em 2026-11-10; E03 a E05 seguem como propostas, a aprovar com o plano. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

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
| E01 | Qual modelo da OpenAI usar, com suporte a saída estruturada, e quais parâmetros (DP-02)? | **Decidido:** `gpt-4.1-mini`, o modelo de geração que a Sofia já usa, com saída estruturada estrita (`json_schema`) e temperatura 0, tudo por configuração (`Llm:Modelo`). Trocar de modelo é mudar a configuração, depois de nova avaliação. Kaio Macedo: "podemos usar o modelo comentado sim". | Kaio Macedo | 2026-11-10 | Sim |
| E02 | Qual a política de dados com a OpenAI: retenção, região, uso para treinamento, anonimização (DP-12)? | **Decidido:** os dados podem ir para a OpenAI ("Pode ir para OpenAI"). O envio continua mínimo: achados, trechos do manual e as linhas citadas, nunca o arquivo inteiro (REQ-ANALISE-007). Enquanto a primeira versão não tiver autenticação, seguem só dados sintéticos (SQ-12). | Kaio Macedo | 2026-11-10 | Sim, para dados reais |
| E03 | Quantas retentativas e qual timeout para o LLM (DP-14)? | **Proposta:** até 2 retentativas, só para resposta inválida ou falha transitória, com timeout de 30 segundos por chamada e de 60 segundos para a etapa inteira. Esgotado o limite, falha técnica, com os achados determinísticos mantidos (REQ-ANALISE-004). Valores iniciais, a medir em produção (DP-14). | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Não |
| E04 | Idioma, tom e tamanho da síntese entregue ao usuário e à Sofia. | **Proposta:** português do Brasil, em registro técnico para a equipe que revisa a ECF; síntese de até 5 frases e explicação de até 2 frases por achado, com os impeditivos e os riscos fiscais primeiro. O LLM não muda classe, veredito nem revisão humana. | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Não |
| E05 | Política para referência não verificável: rejeitar o item ou reclassificar (DP-23, da mudança 03). | **Proposta:** rejeitar o item, sem reclassificar, e registrar a ocorrência, como REQ-ANALISE-003 e REQ-MEMORIA-004 já pedem. Resolve DP-23. | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Sim, se não resolvida em 03 |
| E06 | Como avaliar a análise com o modelo real, fora do CI (SQ-09, DoD-12), se a chave de produção está em aberto? | **Decidido:** a chave que o projeto já usa, a do `SofiaInfis`. O assistente roda a avaliação por script que lê a chave só na hora e a passa ao processo por variável de ambiente, sem exibi-la ("pode você mesmo rodar"). Em produção, Kaio Macedo cadastra a chave no Vercel como `OpenAI__ApiKey` (SQ-03). | Kaio Macedo | 2026-11-10 | Não |

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

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
