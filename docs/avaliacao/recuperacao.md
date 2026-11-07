# Avaliação da recuperação

Mede se a busca encontra a página certa do manual (P11, GD-08; mudança 05, D6). Avaliação feita em 2026-11-07, no Manual do Leiaute 12, com o modelo `text-embedding-3-small`, 1536 dimensões e busca exata por cosseno, filtrada por tipo e versão.

- **Perguntas:** 30, em `docs/avaliacao/perguntas.json`, sobre regras, registros, blocos, tabelas e penalidades. O campo `evidencia` traz o texto que prova a resposta na página.
- **Acerto nos 5 primeiros:** pelo menos um dos 5 primeiros trechos está em uma das páginas esperadas.
- **Posição recíproca média:** 1 quando a página certa vem em primeiro, 0,5 em segundo, e assim por diante; 0 quando não aparece.

## Resultado

| Tamanho do trecho | Sobreposição | Trechos | Acerto nos 5 primeiros | Posição recíproca média |
| --- | --- | --- | --- | --- |
| 800 | 150 | 2137 | 96,7% (29 de 30) | 0,858 |
| 1200 | 200 | 1455 | 93,3% (28 de 30) | 0,850 |
| **1600** | **300** | **1157** | **96,7% (29 de 30)** | **0,868** |
| 2400 | 300 | 849 | 93,3% (28 de 30) | 0,836 |

**Valores adotados:** 1600 e 300, em `Ingestao:TamanhoDoTrecho` e `Ingestao:SobreposicaoDoTrecho`. Com 30 perguntas, a diferença entre as combinações é de uma pergunta, então o resultado é um empate técnico. A escolha fica com a melhor posição média e com o trecho maior, que mantém mais contexto de cada regra.

## O que mudou durante a avaliação

A primeira rodada deu 86,7%, e dois ajustes vieram dela:

- **Sumário fora da base:** as páginas 1 a 9 do manual são sumário, com os mesmos títulos das seções, e apareciam acima do conteúdo. As linhas com pontilhado deixaram de ser ingeridas.
- **Páginas esperadas de duas perguntas:** a P03 também é respondida na página 40 (seção 3.2, com os níveis), e a P04 também na página 64 (campo `DT_FIN`). Conferi o texto dessas páginas antes de incluí-las.

## Limites

- **Pergunta que continuava errada:** a P15, sobre a regra de natureza permitida para contas patrimoniais, não achava as páginas 147 e 148, porque as palavras dela lembram outras regras de natureza. A busca por código da mudança 06 resolveu (seção seguinte).
- **Conjunto pequeno e feito por quem construiu a busca:** as perguntas foram rascunhadas pelo assistente, e o resultado mede só o Leiaute 12. Kaio Macedo pediu para prosseguir sem esperar, então perguntas e parâmetros ficam abertos à revisão dele. Refazer a carga completa custa cerca de US$ 0,05.
- **Mede a página, não a resposta:** achar a página certa não garante que o trecho responda bem. `topK` e limiares ficam para a mudança 06.

## Busca por código, quantidade e limiar (mudança 06)

Avaliação de 2026-11-07, depois do merge da mudança 06, no banco de produção, com os valores de segmentação adotados acima (mudança 06, D4; GD-08). O conjunto ganhou o campo `codigo` em 22 perguntas: o código da regra (4) ou do registro (18), que é o que um achado leva na etapa 5. Ganhou também 5 perguntas que o manual não responde (P31 a P35), para medir o limiar. Uma busca de 8 trechos por pergunta dá o acerto com 3, 5 e 8.

| Busca | 3 trechos | 5 trechos | 8 trechos | Posição recíproca média (5) | Erradas (5) |
| --- | --- | --- | --- | --- | --- |
| Só semântica | 93,3% | 96,7% | 96,7% | 0,868 | P15 |
| **Com código** | **96,7%** | **100% (30 de 30)** | **100%** | **0,925** | nenhuma |

**Limiar:** a menor relevância entre as páginas certas foi 0,4800, e a maior entre as perguntas sem resposta, 0,5458. Nenhum valor separa as duas sem perder acerto, então fica sem limiar: a etapa 5 não declara ausência de evidência pela relevância (D4).

**Valores adotados:** 5 trechos por consulta (`Busca:QuantidadePadrao`), busca por código ligada (a Api.Agente envia o código da regra ou do registro do achado) e nenhum limiar. Com 3 trechos, o acerto cai; com 8, não sobe.

**Custo:** 70 embeddings, 1.192 tokens, cerca de US$ 0,00002 (`GET /consumo`, finalidade `avaliacao`).

**Limites:** as 5 perguntas sem resposta e os códigos foram rascunhados pelo assistente. Com o código, a busca acha o trecho que cita a regra, o que é o caso de uso da etapa 5, mas favorece as perguntas que já trazem o código; perguntas sem código seguem a busca semântica.

## Carga completa

Em 2026-11-07, com os valores adotados:

| Leiaute | Ano-calendário | Páginas | Trechos |
| --- | --- | --- | --- |
| 12 | 2025 | 622 | 1157 |
| 11 | 2024 | 600 | 1117 |
| 10 | 2023 | 677 | 1290 |
| 9 | 2022 | 626 | 1177 |
| 8 | 2021 | 609 | 1131 |
| 7 | 2020 | 634 | 1192 |
| **Total** | | **3768** | **7064** |

O consumo acumulado, incluindo as rodadas de comparação, foi de 466 chamadas, 6.158.313 tokens e US$ 0,12 estimados (`GET /consumo`).
