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
| `descricao-300-tipo-vazio.txt` | D6 e D7 | `L100.DESCRICAO` com 300 caracteres e `TIPO` vazio | Importou, sem apontamento, e mostra a descrição e o tipo da tabela dele, e não os do arquivo. | `EST-CAMPO-001` | `parcial` |
| `fone-vazio.txt` | D7 | `0930.FONE` vazio | Importou e validou, sem erro nesse campo. | `EST-CAMPO-001` | `parcial` |
| `sem-c-e.txt` | D8 | Sem `C001`, `C990` e `E001` | Importou e validou com os mesmos 22 erros da ECF base, nenhum sobre esses registros. | `EST-OCOR-001` | `parcial` |
| `sem-0030.txt` | D8 | Controle: sem `0030` | Importou e criou o registro vazio; na validação, "O campo é obrigatório e não foi preenchido" em 9 campos. | `EST-OCOR-001` | `recusado` |
| `y800-varias-linhas.txt` | D9 | Y800 em várias linhas, com `HASH` vazio e `Y990` pela contagem lógica | Importou, sem apontamento. | nenhuma | `parcial` |
| `y990-contagem-fisica.txt` | D9 | Controle: o mesmo arquivo, com o `Y990` contando as linhas físicas | Recusou: "Quantidade de linhas" do bloco. A contagem vale por registro lógico. | `EST-TOT-001` | `recusado` |
| `w300-varias-linhas.txt` | D9 | W300 em várias linhas, até `W300FIM` | Importou, sem apontamento. | nenhuma | `parcial` |
| `nome-0x96.txt` | D10 | `0000.NOME` com o byte 0x96, um travessão em Windows-1252 | Importou, sem apontamento. O manual só proíbe os caracteres 0 a 31 e o `\|` (p. 33). | nenhuma | `parcial` |
| `9100-longo.txt` | D4 | 9100 com 40 caracteres em `CONTEUDO` e 21 em `VALOR_ESPERADO` | Importou, sem apontamento. | nenhuma | `parcial` |
| `tin-20.txt` | D4 | `W100.TIN_SUBSTITUTA` com 20 caracteres | Importou, sem apontamento. | nenhuma | `parcial` |
| `lista-x.txt` | D11 | `0020.IND_ADM_FUN_CLU` igual a X, fora da lista `S` e `N` do manual | Importou e validou com os mesmos erros da ECF base, nenhum sobre esse campo. | `EST-CAMPO-006` | `parcial` |
| `nome-300.txt` | Limite | Controle: `0000.NOME` com 300 caracteres | Recusou: "O tamanho do campo está incorreto", com 255 posições esperadas, embora o descritor não indique limite. | `EST-CAMPO-003` | `recusado` |
| `w200-so-estrangeiro.txt` | D7 | W200 só com os valores em moeda estrangeira | Importou e gravou o W200; o sistema calcula os valores em reais (p. 445 a 449). | `EST-CAMPO-001` | `parcial` |
| `cr-puro.txt` | Leitura | Fim de linha só com CR | Importou, sem apontamento. | nenhuma | `parcial` |
| `csll-2.txt` | Tabela | Controle: `0020.IND_ALIQ_CSLL` igual a 2, fora da lista | Recusou: "O campo possui um valor diferente de seus valores válidos." | `EST-CAMPO-006` | `recusado` |
