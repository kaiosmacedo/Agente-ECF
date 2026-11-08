# Divergências entre o manual e o programa da ECF

ECF sintéticas da decisão E10 (mudança 08). Cada arquivo foi importado no programa da ECF 12.2.8 (descritor 12003 v1) em 2026-11-08, sem assinar nem transmitir, e cada cópia foi excluída do programa. As duas colunas do agente são conferidas pelo teste `DivergenciasDoProgramaTests`, com o serviço de análise. Detalhes em `docs/catalogo/revisao-ecf-leiaute-12.md`.

| Arquivo | Ponto | Caso | O programa da ECF | Regras do agente | Veredito do agente |
| --- | --- | --- | --- | --- | --- |
| `s990.txt` | D1 | Bloco S com `IND_DAD` 0 e `S990`; o 9900 do `S001` ficou errado no arquivo | Importou, sem apontamento; ele não confere o 9900 (E07). | `EST-TOT-002` | `parcial` |
| `s990-contagem-errada.txt` | D1 | `S990` com a contagem errada; o mesmo 9900 errado | Importou, sem apontamento. | `EST-TOT-001`, `EST-TOT-002` | `parcial` |
| `num-rec-41.txt` | D2 | `NUM_REC` com 41 caracteres | Importou, sem apontamento. | nenhuma | `parcial` |
| `num-rec-40.txt` | D2 | `NUM_REC` com 40 caracteres | Recusou: "O tamanho do campo está incorreto." O agente não exige o tamanho exato (E09). | nenhuma | `parcial` |
| `w200-negativo.txt` | D3 | `W200.VL_LUC_ACUM` igual a -1000 | Importou, sem apontamento. | nenhuma | `parcial` |
| `tin-alfanumerico.txt` | D4 | `W100.TIN_SUBSTITUTA` igual a AB123 | Importou, sem apontamento. | nenhuma | `parcial` |
| `9100-texto.txt` | D4 | 9100 com texto em `NOM_REGRA`, `CONTEUDO` e `VALOR_ESPERADO` | Importou, sem apontamento. | nenhuma | `parcial` |
| `rotulo-m300.txt` | D5 | `M300.TIPO_LANCAMENTO` igual a R, no código 1 da tabela | Importou, sem apontamento. | nenhuma | `parcial` |
| `tip-ent-16.txt` | D5 | `0010.TIP_ENT` igual a 16 (Saúde), com `FORMA_TRIB` 8 | Aceitou o código e parou em outra regra de preenchimento do arquivo sintético. | nenhuma | `parcial` |
| `tip-ent-17.txt` | D5 | Controle: `0010.TIP_ENT` igual a 17, que não existe | Recusou: "O Código não foi localizado". | `EST-CAMPO-006` | `recusado` |
| `fone-15.txt` | D6 | `0930.FONE` com 15 caracteres | Importou, sem apontamento. | nenhuma | `parcial` |
| `descricao-300-tipo-vazio.txt` | D6 e D7 | `L100.DESCRICAO` com 300 caracteres e `TIPO` vazio | Importou, sem apontamento. | `EST-CAMPO-001` | `parcial` |
| `fone-vazio.txt` | D7 | `0930.FONE` vazio | Importou e validou, sem erro nesse campo. | `EST-CAMPO-001` | `parcial` |
| `sem-c-e.txt` | D8 | Sem `C001`, `C990` e `E001` | Importou e validou com os mesmos 22 erros da ECF base, nenhum sobre esses registros. | `EST-OCOR-001` | `parcial` |
| `sem-0030.txt` | D8 | Controle: sem `0030` | Importou e criou o registro vazio; na validação, "O campo é obrigatório e não foi preenchido" em 9 campos. | `EST-OCOR-001` | `recusado` |
