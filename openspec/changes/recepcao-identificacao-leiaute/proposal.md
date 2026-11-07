# Proposal: Recepção do arquivo e identificação do leiaute

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 07; PR #4). Em 2026-11-07, o assistente propôs as respostas dos esclarecimentos, os requisitos novos (REQ-RECEPCAO-005 a 008, REQ-LEIAUTE-004, REQ-A2A-007 e 008), o plano técnico e as tarefas, que aguardam a aprovação de Kaio Macedo.

## Why

As etapas 1 a 3 do fluxo (P03) recebem o arquivo, validam tamanho, formato e condições de processamento e identificam o tipo de documento e a versão do leiaute. Sem essa identificação, nenhuma validação dependente de versão pode rodar, e o resultado não pode declarar conformidade. É o primeiro endpoint de análise usado pela Sofia INFIS e pelos usuários.

## What Changes

- Cria `POST /analises`, sem autenticação na primeira versão (SQ-12), que recebe o arquivo, cria a execução e registra o hash (etapa 1).
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
| E01 | Quais formatos aceitar: o arquivo texto da ECF, compactado (ZIP), outros (DP-05)? Pela Sofia, o documento já chega como texto (`documentos[].texto`), sem binário; o `POST /analises` direto segue valendo para outros usos? | **Proposta:** só o texto da ECF, reconhecido pela primeira linha `\|0000\|LECF\|` (manual, p. 32 e 63). Por A2A, o texto da Sofia; pelo `POST /analises`, um arquivo em `multipart/form-data`, lido como ISO 8859-1. Sem ZIP na primeira versão. O REST continua, pelo mesmo serviço (design, D1 a D3). | Assistente; aprovação de Kaio Macedo | 2026-11-07 | Não, depois da aprovação |
| E02 | Qual o tamanho máximo esperado de um arquivo ECF real (MB) (DP-05)? O Vercel recusa corpo acima de cerca de 4,5 MB (medido em 2026-11-05, HTTP 413); na rede Docker esse limite não existe. Um arquivo real caberia, ou é preciso outro host (novo ADR)? E, se a Sofia enviar vários documentos, como tratar os que não forem ECF? | **Proposta:** limite de 4 MiB de texto nos dois canais. Não há ECF real para medir, e a primeira versão só recebe dados sintéticos (SQ-12); a medição e um eventual novo host ficam para antes de dados reais. Uma ECF por pedido; os demais documentos ficam nas observações como não analisados; sem ECF, `nenhuma_ecf`; com duas, `mais_de_uma_ecf` (D2, D4). | Assistente; aprovação de Kaio Macedo | 2026-11-07 | Não, depois da aprovação |
| E03 | A análise responde na mesma requisição ou de forma assíncrona, com consulta posterior (DP-22)? O limite de duração das funções do Vercel pesa nessa escolha. | **Proposta:** na mesma requisição, como hoje. Recepção, identificação e validação estrutural são leitura linear do texto, sem provedor pago. No Vercel, `GetTask` não é garantido entre instâncias, o que pesa contra o assíncrono. A mudança 10, que liga o LLM, reavalia com o tempo medido aqui. | Assistente; aprovação de Kaio Macedo | 2026-11-07 | Não, depois da aprovação |
| E04 | O arquivo recebido deve ser guardado? Por quanto tempo e onde (DP-11, GD-06)? | **Proposta:** não guardar o texto, só hash, tamanho e canal, como hoje (DP-25). A retenção para dados reais continua em DP-11 (D4). | Assistente; aprovação de Kaio Macedo | 2026-11-07 | Não para dados sintéticos; DP-11 antes de dados reais |
| E05 | Registro e campos do Manual da ECF, leiaute 12, que identificam o tipo e a versão do leiaute, e a codificação de caracteres do arquivo. A extração é feita do manual e validada por Kaio Macedo (P01). | **Extração proposta:** o leiaute vem do `COD_VER`, campo 3 do Registro 0000, com o valor na observação do campo: `0007` a `0012` para os leiautes 7 a 12 (p. 55, 56, 55, 60, 59 e 63 de cada manual). O Leiaute 10 diverge: o texto diz `00010` num campo de 4 caracteres, e o exemplo usa `0010`; a proposta é aceitar `0010`. O ano de `DT_FIN` e o `SIT_ESPECIAL` confirmam o leiaute (p. 10). Codificação ISO 8859-1, delimitador `\|` e CR LF (leiaute 12, p. 32). Tabela completa no design, D5. | Extração do assistente; validação de Kaio Macedo | 2026-11-07 | Não, depois da validação |
| E07 | A Sofia corta cada arquivo em 20.000 caracteres e acrescenta `[DOCUMENTO TRUNCADO]`. O corte será retirado para os agentes? Enquanto existir, o agente deve detectar o marcador e declarar a limitação, sem validar um arquivo cortado como se estivesse inteiro. | **Proposta:** o agente detecta o marcador, identifica o leiaute e não roda as etapas 4 e 5, com a limitação "documento cortado antes de chegar ao agente" (D6). O guia para a Sofia pede que o corte não se aplique aos agentes (D9); retirá-lo é com quem mantém a Sofia. | Assistente; a retirada do corte é de quem mantém a Sofia | 2026-11-07 | Não, depois da aprovação |
| E06 | A primeira entrega suporta só o leiaute 12, ou também versões anteriores (DP-04)? | Os leiautes 7 a 12, os anos que ainda podem ser retificados (manual, seção 1.14). O catálogo começa pelo 12 e registra as diferenças dos anteriores. | Kaio Macedo | 2026-11-06 | Não |
| E08 | O agente recebe o TXT de importação ou o arquivo exportado do programa oficial? Os blocos C e E e as linhas calculadas só existem no segundo. | Os dois, e o agente identifica qual recebeu. Quando o regime exige recuperação, só o exportado pode chegar a `conforme`; o TXT termina em `parcial` (`docs/classificacao-das-regras.md`, decisão 5). **Complemento de 2026-11-07:** o manual não define marcador que distinga os dois arquivos, só indícios (p. 17, 41, 43 a 58, 607 e 614). Os indícios dependem do catálogo de registros e entram na mudança 08 (design, D5). | Kaio Macedo | 2026-11-06 | Não |

## Fontes oficiais

| Documento | Emissor | Versão ou vigência | Localização |
| --- | --- | --- | --- |
| Manual da ECF, leiaute 12 | Receita Federal do Brasil, portal SPED | Publicado em 23/07/2026 | p. 10 (leiaute e período), p. 17 (anos que podem ser retificados), p. 32 (características do arquivo), p. 34 (datas), p. 63 a 69 (Registro 0000) |
| Manuais da ECF, leiautes 7 a 11 | Receita Federal do Brasil, portal SPED | Versões e hashes em `docs/fontes-oficiais.md` | Observação do `COD_VER` no Registro 0000: leiaute 7, p. 55; 8, p. 56; 9, p. 55; 10, p. 60 e 65; 11, p. 59 |

## Princípios, decisões e dependências

- **Princípios:** P01, P03, P05, P08, P10, P19; GD-06; SQ-05, SQ-12.
- **Decisões pendentes:** resolve DP-05 e DP-22; depende de DP-04 e DP-11; usa DP-25 (mudança 03).
- **Depende de:** 03 `execucao-e-resultado`. A entrada pela Sofia usa o contrato da mudança 13.
- **Maior risco do roteiro:** o código atual da Sofia corta arquivos e o Vercel limita o corpo; veja E02, E07 e `docs/base-sofiainfis.md`.
- **Skills:** `sofia-a2a-contrato`, `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Validação estrutural completa do arquivo (mudança 08) e regras de negócio (09).
- Correção ou geração de arquivos (constitution, 1.3).

## Impact

- Endpoint `POST /analises`, leitura segura de arquivos grandes e, conforme E04, armazenamento de objetos.

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
