# Proposal: Análise com LLM

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 10; PR #4). Esclarecimentos propostos em 2026-11-10, à espera da aprovação de Kaio Macedo. Plano técnico e tarefas só depois dos esclarecimentos bloqueantes.

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
| E01 | Qual modelo da OpenAI usar, com suporte a saída estruturada, e quais parâmetros (DP-02)? | **Proposta:** `gpt-4.1-mini`, o modelo de geração que a Sofia já usa, com saída estruturada estrita (`json_schema`) e temperatura 0, tudo por configuração (`Llm:Modelo`). Trocar de modelo é mudar a configuração, depois de nova avaliação. Sem a chave de produção, a etapa 7 fica desligada em produção e registra a limitação, como a busca da 06. | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Sim |
| E02 | Qual a política de dados com a OpenAI: retenção, região, uso para treinamento, anonimização (DP-12)? | **Proposta:** segue em aberto, com a pergunta 19 do levantamento para o superior. Até lá, só dados sintéticos (SQ-12), e o envio já é mínimo: achados, trechos do manual e as linhas citadas, nunca o arquivo inteiro (REQ-ANALISE-007). | Kaio Macedo, com o superior | 2026-11-10 | Sim, para dados reais |
| E03 | Quantas retentativas e qual timeout para o LLM (DP-14)? | **Proposta:** até 2 retentativas, só para resposta inválida ou falha transitória, com timeout de 30 segundos por chamada e de 60 segundos para a etapa inteira. Esgotado o limite, falha técnica, com os achados determinísticos mantidos (REQ-ANALISE-004). Valores iniciais, a medir em produção (DP-14). | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Não |
| E04 | Idioma, tom e tamanho da síntese entregue ao usuário e à Sofia. | **Proposta:** português do Brasil, em registro técnico para a equipe que revisa a ECF; síntese de até 5 frases e explicação de até 2 frases por achado, com os impeditivos e os riscos fiscais primeiro. O LLM não muda classe, veredito nem revisão humana. | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Não |
| E05 | Política para referência não verificável: rejeitar o item ou reclassificar (DP-23, da mudança 03). | **Proposta:** rejeitar o item, sem reclassificar, e registrar a ocorrência, como REQ-ANALISE-003 e REQ-MEMORIA-004 já pedem. Resolve DP-23. | Assistente; aprovação de Kaio Macedo | 2026-11-10 | Sim, se não resolvida em 03 |
| E06 | Como avaliar a análise com o modelo real, fora do CI (SQ-09, DoD-12), se a chave de produção está em aberto? | **Proposta:** como na carga da mudança 05: um script que Kaio Macedo roda, que lê a chave do `SofiaInfis` só na hora e a passa ao processo por variável de ambiente, com o conjunto de avaliação sintético e o consumo registrado. A chave não aparece na saída nem na linha de comando. | Kaio Macedo | 2026-11-10 | Não para implementar; sim para a avaliação |

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
