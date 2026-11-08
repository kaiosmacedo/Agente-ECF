# Gabarito da validação estrutural

ECF sintéticas da mudança 08 (D10). Cada arquivo planta um único erro de estrutura na ECF válida; o programa da ECF (PGE) é rodado com cada uma, sem assinar nem transmitir, e o que ele aponta é comparado com a regra esperada do agente. Gerado por `.github/scripts/catalogo/gerar-gabarito.py`.

| Arquivo | Erro plantado | Regra do agente | Linha | O que o PGE apontou |
| --- | --- | --- | --- | --- |
| `00-valida.txt` | Nenhum erro de estrutura. | `nenhuma` | — | (a preencher) |
| `01-valor-fora-da-lista.txt` | IND_SIT_INI_PER do 0000 igual a 9 (p. 63). | `EST-CAMPO-006` | 1 | (a preencher) |
| `02-data-invalida.txt` | DT_FIN do 0000 igual a 31022025 (p. 34 e 64). | `EST-CAMPO-005` | 1 | (a preencher) |
| `03-registro-inexistente.txt` | Linha |Z999|1| no Bloco 0. | `EST-ARQ-002` | 7 | (a preencher) |
| `04-campos-a-mais.txt` | Registro 0001 com um campo a mais (p. 70). | `EST-ARQ-003` | 2 | (a preencher) |
| `05-sem-delimitador.txt` | Registro C001 sem o | inicial (p. 32). | `EST-ARQ-001` | 7 | (a preencher) |
| `06-sem-9999.txt` | Arquivo sem o Registro 9999 (p. 58 e 619). | `EST-OCOR-001` | — | (a preencher) |
| `07-abertura-repetida.txt` | Registro 0001 duas vezes (REGRA_OCORRENCIA_UNITARIA_ARQ, p. 70). | `REGRA_OCORRENCIA_UNITARIA_ARQ` | 3 | (a preencher) |
| `08-total-de-linhas.txt` | 9999.QTD_LIN maior que o total (p. 619). | `EST-TOT-003` | — | (a preencher) |
| `09-contagem-do-bloco.txt` | 0990.QTD_LIN errado (p. 108). | `EST-TOT-001` | 6 | (a preencher) |
| `10-contagem-do-9900.txt` | 9900 do 0030 com quantidade 2 (p. 616). | `EST-TOT-002` | — | (a preencher) |
| `11-filho-sem-pai.txt` | Registro J051 sem o J050 antes dele (p. 59). | `EST-HIER-001` | — | (a preencher) |
