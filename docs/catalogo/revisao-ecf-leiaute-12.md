# Revisão do catálogo estrutural da ECF, leiaute 12

Material da revisão de Kaio Macedo (mudança 08, tarefa 1.4, E02). O catálogo vem do manual. Aqui ele é conferido contra o manual, por uma segunda leitura, e contra o programa oficial da ECF. Kaio Macedo aprovou a decisão E10 em 2026-11-08, e o catálogo `ecf-l12-1.5.0` já a aplica, com as correções da verificação independente.

## Fontes

| Fonte | Versão | Identificação |
| --- | --- | --- |
| Manual da ECF, leiaute 12, versão em Word | Publicado em 23/07/2026 | SHA-256 em `docs/fontes-oficiais.md` |
| Descritor do leiaute no programa da ECF | Programa 12.2.8, estrutura 12003, versão 1 | `descritor/escrituracao/estrutura12003/v1/descritor.xml`, dentro de `lib/br.gov.serpro.sped.irpjpva/irpjpva-infra.jar`; SHA-256 `37af2548f8654b6e6a8ec8381e97cfdc703dad5e6bace31f28ed006375253b5a` |
| Tabelas de valores do programa | Programa 12.2.8 | `recursos/tabelas/`, na pasta de instalação |

O descritor e as tabelas foram só lidos; nenhum código do programa foi descompilado, e os arquivos não estão neste repositório. O que vem deles está em `.github/scripts/catalogo/ajustes-do-programa-12.json`, gerado por `extrair-ajustes-do-programa.py` e aplicado por `gerar-catalogo.py`.

## O que está conferido

| Conferência | Resultado |
| --- | --- |
| Segunda leitura do Word, sem usar o gerador | 1.122 campos, nenhuma diferença em número, nome, tipo, tamanho, decimal, valores e obrigatoriedade |
| Registros contra o programa | Os 188 existem, na mesma ordem e no mesmo bloco; ao contrário, só o `S990` faltava (D1) |
| Campos contra o programa | Mesma quantidade e mesma posição em todos os registros; em 53, o nome difere, como `QTD_LIN` e `QTD_LIN_0`, sem efeito, porque a leitura é por posição |
| Registro pai | Igual em todos os registros |
| Tabelas de valores | Das 19 listas em que o programa usa tabela, 17 foram comparadas nas tabelas, e só `0010.TIP_ENT` tem um valor a mais no programa (D5) |

**Defeito corrigido no catálogo `ecf-l12-1.3.0`:** 8 campos dos registros M305, M310, M355, M360, M500 e M510 traziam na lista o texto da referência, como `M010.COD_CTA_B`, e o agente recusava toda linha real.

## Decisão E10: onde o agente recusava o que o programa aceita

Onde o manual e o programa divergem, o agente não recusa o que o programa aceita. Valor aceito pelo programa não gera achado; obrigatoriedade (D7) e registros ausentes (D8) ficam em `observacao`, citando o manual. A E05 continua como Kaio Macedo decidiu: a falta do 9999 recusa o arquivo.

| # | Divergência | Manual | Programa | Agente antes | Agente agora |
| --- | --- | --- | --- | --- | --- |
| D1 | Registro `S990` | Não existe (nenhuma menção) | Encerramento do Bloco S | Recusava (`EST-ARQ-002`) | Reconhece, sem exigir; contagem errada é `observacao` |
| D2 | `0000.NUM_REC` | `C`, 40 (p. 65) | Exatamente 41 | Recusava 41 (`EST-CAMPO-003`) | Até 41 |
| D3 | Sinal em 21 campos | `N`, sem sinal (p. 33) | `NS` | Recusava negativo (`EST-CAMPO-002`) | Aceita o sinal, e continua recusando letras |
| D4 | Texto em 16 campos | `N` ou `NS` | `C` | Recusava texto (`EST-CAMPO-002`) | Tipo `C` |
| D5 | `M300` e `M350.TIPO_LANCAMENTO` `R` (rótulo); `0010.TIP_ENT` `16` (Saúde, desde 2025) | Sem esses valores | Aceita | Recusava (`EST-CAMPO-006`) | Aceita |
| D6 | Tamanho em 76 campos | Limite menor | Limite maior | Recusava acima do manual (`EST-CAMPO-003`) | Limite do programa |
| D7 | Obrigatoriedade em 72 campos | "Sim" | Opcional | Recusava vazio (`EST-CAMPO-001`) | `observacao` |
| D8 | Registros `C001`, `C990` e `E001` | `O`/`O`, mas o bloco não é importado (p. 41) | Preenche sozinho | Recusava a falta (`EST-OCOR-001`) | `observacao` |
| D9 | Y800 e W300 em várias linhas; `Y800.HASH` vazio | "Vazio" na importação (p. 611), e "Sim" na coluna | Aceita | Recusava (`EST-ARQ-001`, `EST-CAMPO-001` e `002`) | Aceita |
| D10 | Caracteres acima de 31 no tipo `C`, como 0x96 | Proíbe só 0 a 31 e o `\|` (p. 33) | Aceita | Recusava (`EST-CAMPO-002`) | Aceita |
| D11 | Listas que só o manual tem, em 89 campos | Lista os valores | Não confere | Recusava (`EST-CAMPO-006`) | `observacao` |

`0030` continua obrigatório, e a falta dele recusa o arquivo: o programa o cria vazio na importação, e a validação acusa os campos obrigatórios. Na proposta, D7 tinha 45 campos, porque a primeira comparação era por nome. Agora são 72: os mesmos 45, mais 18 que têm outro nome no programa, como `M500.COD_TRIBUTO` e `TRIBUTO`, e 9 do W200 que o sistema calcula quando o valor em moeda estrangeira é informado (p. 445 a 449).

Os campos de D4 também deixam de ter o limite numérico do manual: valem como texto, com o limite do programa, se houver.

**A chave de duplicidade também segue o programa,** em 21 registros (`0035`, `0930`, `C157`, `K915`, `K935`, `M510`, `N500`, `N650`, `V020`, `V030`, `V100`, `X357`, `X370`, `X485`, `Y590`, `Y680`, `Y682`, `Y730`, `Y750`, `Y800`, `9900`). O achado é só um aviso, mas não diz mais "retire a linha repetida" de uma linha legítima, como dois ativos iguais em países diferentes no `Y590`.

### Testes na tela do programa

Em 2026-11-08, 26 arquivos foram importados no programa 12.2.8, sem assinar nem transmitir, e cada cópia foi excluída dele. Os arquivos e os resultados estão em `tests/Api.Agente.Tests/Dados/ecf/programa/resultados.md`, e o teste `DivergenciasDoProgramaTests` confere o agente com os mesmos arquivos.

- **Confirmados:** D1, inclusive o `S990` com contagem errada, que o programa não confere; D2, em que ele aceita 41 e recusa 40; D3 no `W200`; D4 no `W100.TIN_SUBSTITUTA` e no 9100; D5 nos dois valores, com o código 17 como controle recusado; D6 em `0930.FONE` e `L100.DESCRICAO`; D7 em `0930.FONE`, também na validação, em `L100.TIPO` e nos valores em reais do W200, que o programa grava só com os valores em moeda estrangeira; D8, também na validação; D9, inclusive que o programa recusa o `Y990` que conta as linhas físicas; D10; D11 em `0020.IND_ADM_FUN_CLU`, também na validação; e o fim de linha só com CR.
- **O que o programa faz com o `L100`:** ele mostra a descrição e o tipo da tabela dele, e não os do arquivo. Por isso a descrição de 300 caracteres e o `TIPO` vazio não atrapalham.
- **Controles que continuam recusando:** `0000.NOME` com 300 caracteres, com 255 posições esperadas, embora o descritor não indique limite; `0020.IND_ALIQ_CSLL` igual a 2; `0010.TIP_ENT` igual a 17; e o `0030` ausente, na validação.
- **Aviso de linhas desprezadas:** em 4 casos, o programa avisou que "uma ou mais linhas" foram desprezadas, mas o relatório veio vazio. No W200, conferimos que a linha foi gravada. A causa do aviso não foi identificada.
- **Não testados na tela, só no descritor:** os demais campos de D3, D4, D6, D7 e D11.

### Campos de cada divergência

**D3, sinal:** `N615.BASE_CALC`, `W200.VL_REC_NAO_REL_EST`, `W200.VL_REC_NAO_REL`, `W200.VL_REC_REL_EST`, `W200.VL_REC_REL`, `W200.VL_REC_TOTAL_EST`, `W200.VL_REC_TOTAL`, `W200.VL_IR_PAGO_EST`, `W200.VL_IR_PAGO`, `W200.VL_IR_DEVIDO_EST`, `W200.VL_IR_DEVIDO`, `W200.VL_CAP_SOC_EST`, `W200.VL_CAP_SOC`, `W200.VL_LUC_ACUM_EST`, `W200.VL_LUC_ACUM`, `W200.VL_ATIV_TANG_EST`, `W200.VL_ATIV_TANG`, `X280.REC_LIQ_INCENTIVO`, `X280.VL_INCENTIVO`, `X370.VL_TRANSACAO`, `X370.VL_COMPENSATORIO`.

**D4, texto:** `0010.FORMA_TRIB`, `0010.IND_REC_RECEITA`, `0020.IND_ALIQ_CSLL`, `0030.NUM_TEL`, `M300.IND_RELACAO`, `M350.IND_RELACAO`, `W100.TIN_SUBSTITUTA`, `X280.NCM_INCENTIVO`, `X340.PAIS`, `X357.PAIS`, `Y600.QUALIF_REP_LEG`, `Y800.TIPO_DOC`, `9100.NOM_REGRA`, `9100.CONTEÚDO`, `9100.VALOR_ESPERADO`, `9100.CNAE`.

**D6, descrições (255 no manual, 512 no programa):** 46 campos: `L100.DESCRICAO`, `L210.DESCRICAO`, `L300.DESCRICAO`, `M300.DESCRICAO`, `M350.DESCRICAO`, `N500.DESCRICAO`, `N600.DESCRICAO`, `N610.DESCRICAO`, `N620.DESCRICAO`, `N630.DESCRICAO`, `N650.DESCRICAO`, `N660.DESCRICAO`, `N670.DESCRICAO`, `P100.DESCRICAO`, `P130.DESCRICAO`, `P150.DESCRICAO`, `P200.DESCRICAO`, `P230.DESCRICAO`, `P300.DESCRICAO`, `P400.DESCRICAO`, `P500.DESCRICAO`, `S100.DESCRICAO`, `S150.DESCRICAO`, `S200.DESCRICAO`, `S300.DESCRICAO`, `S500.DESCRICAO`, `S700.DESCRICAO`, `T120.DESCRICAO`, `T150.DESCRICAO`, `T170.DESCRICAO`, `T181.DESCRICAO`, `U100.DESCRICAO`, `U150.DESCRICAO`, `U180.DESCRICAO`, `U182.DESCRICAO`, `X292.DESCRICAO`, `X390.DESCRICAO`, `X451.DESCRICAO`, `X460.DESCRICAO`, `X470.DESCRICAO`, `X480.DESCRICAO`, `X490.DESCRICAO`, `X500.DESCRICAO`, `X510.DESCRICAO`, `Y681.DESCRICAO`, `Y750.DESCRICAO`.

**D6, demais:** `0000.NUM_REC` (até 41), `0000.EVENTO_SCP` (até 2), `0010.FORMA_TRIB` (até 2), `0010.IND_REC_RECEITA` (até 1), `0020.IND_ALIQ_CSLL` (até 1), `0030.NUM_TEL` (até 15), `0930.FONE` (até 15), `L100.COD_NAT` (até 4), `L300.COD_NAT` (até 4), `M300.IND_RELACAO` (até 1), `M350.IND_RELACAO` (até 1), `P100.COD_NAT` (até 4), `P150.COD_NAT` (até 4), `S100.COD_NAT` (até 4), `S150.COD_NAT` (até 4), `U100.COD_NAT` (até 4), `U150.COD_NAT` (até 4), `V100.CODIGO` (até 6), `W100.TIN_SUBSTITUTA` (até 255), `X280.NCM_INCENTIVO` (até 8), `X340.PAIS` (até 255), `X357.PAIS` (até 255), `X365.IDENTIFICADOR` (até 10), `X370.IDENTIFICADOR` (até 10), `Y600.QUALIF_REP_LEG` (até 2), `Y800.TIPO_DOC` (até 3), `9100.NOM_REGRA` (até 255), `9100.CONTEÚDO` (até 255), `9100.VALOR_ESPERADO` (até 255), `9100.CNAE` (até 7).

**D7, obrigatoriedade:** `0020.IND_ALIQ_CSLL`, `0020.IND_QTE_SCP`, `0020.POSSUI_CEBRAS`, `0930.FONE`, `C040.IDENT_MF`, `C050.IND_CTA`, `C053.COD_IDT`, `C053.COD_CNT_CORR`, `C053.NAT_SUB_CNT`, `C155.IND_VL_SLD_INI`, `C155.IND_VL_SLD_FIN`, `C355.IND_VL_CTA`, `E020.COD_PB_RFB`, `E155.IND_VL_SLD_INI`, `E155.IND_VL_SLD_FIN`, `E355.IND_VL_SLD_FIN`, `L100.TIPO`, `L300.TIPO`, `M500.COD_CTA_B`, `M500.COD_TRIBUTO`, `M500.SD_INI_LAL`, `M500.IND_ SD_INI_LAL`, `M500.VL_LCTO_PARTE_A`, `M500.IND_ VL_LCTO_PARTE_A`, `M500.VL_LCTO_PARTE B`, `M500.IND_ VL_LCTO_PARTE B`, `M500.SD_FIM_LAL`, `M500.IND_ SD_FIM_LAL`, `M510.DESCRICAO_PB_RFB`, `M510.SD_INI_LAL`, `M510.IND_ SD_INI_LAL`, `M510.VL_LCTO_PARTE_A`, `M510.IND_ VL_LCTO_PARTE_A`, `M510.VL_LCTO_PARTE B`, `M510.IND_ VL_LCTO_PARTE B`, `M510.SD_FIM_LAL`, `M510.IND_ SD_FIM_LAL`, `P150.TIPO`, `S150.TIPO`, `U150.TIPO`, `W100.IND_CONTROLADORA`, `W100.NOME_CONTROLADORA`, `W100.JURISDICAO_CONTROLADORA`, `W100.TIN_CONTROLADORA`, `W100.IND_ENTREGA`, `W100.IND_MODALIDADE`, `W200.VL_REC_NAO_REL`, `W200.VL_REC_REL`, `W200.VL_REC_TOTAL`, `W200.VL_LUC_PREJ_ANTES_IR`, `W200.VL_IR_PAGO`, `W200.VL_IR_DEVIDO`, `W200.VL_CAP_SOC`, `W200.VL_LUC_ACUM`, `W200.VL_ATIV_TANG`, `W250.JURISDICAO_TIN`, `X351.RES_POS_TRIB`, `X370.VL_ESPONTANEO`, `X370.VL_COMPENSATORIO`, `X370.COMP_INTENCIONAL`, `X370.SINERGIA`, `X370.IND_TRANS_COMBINADAS`, `X370.IND_DADOS_MULTIP`, `X370.IND_SIMPLIFIC`, `Y600.IND_QUALIF`, `Y612.VL_REM_TRAB`, `Y612.VL_DEM_REND`, `Y612.VL_IR_RET`, `Y720.LUC_LIQ`, `Y720.DT_LUC_LIQ`, `Y720.REC_BRUT_ANT`, `Y720.INTIMACAO`.

## Onde o agente aceita o que o programa recusa

Não dá falso alarme, e o veredito segue `parcial` até as mudanças 09 e 10. Fica como está, para reavaliar na mudança 09.

| Divergência | Quantidade | Campos |
| --- | --- | --- |
| Obrigatório no programa, "Não" no manual | 31 | `0021.IND_REPES`, `0021.IND_RECAP`, `0021.IND_PADIS`, `0021.IND_REIDI`, `0021.IND_RECINE`, `0021.IND_RETID`, `0021.IND_OLEO_BUNKER`, `0021.IND_REPORTO`, `0021.IND_RET_II`, `0021.IND_RET_PMCMV`, `0021.IND_RET_EEI`, `0021.IND_EBAS`, `0021.IND_REPETRO_ INDUSTRIALIZACAO`, `0021.IND_REPETRO_ NACIONAL`, `0021.IND_REPETRO_ PERMANENTE`, `0021.IND_REPETRO_ TEMPORARIO`, `0021.IND_REDATA`, `M410.COD_CTA_B`, `P100.DESCRICAO`, `S100.DESCRICAO`, `W300.IND_REC_NAO_REL`, `W300.IND_REC_REL`, `W300.IND_REC_TOTAL`, `W300.IND_LUC_PREJ_ANTES_IR`, `W300.IND_IR_PAGO`, `W300.IND_IR_DEVIDO`, `W300.IND_CAP_SOC`, `W300.IND_LUC_ACUM`, `W300.IND_ATIV_TANG`, `W300.IND_NUM_EMP`, `X280.VL_INCENTIVO` |
| Data no programa, sem conferência de data no catálogo | 7 | `Q100.DATA`, `X280.VIG_INI`, `X280.VIG_FIM`, `Y620.DATA_AQUIS`, `Y630.DAT_ABERT`, `Y630.DAT_ENCER`, `Y730.DATA` |
| Tamanho exato no programa | 464 campos, como `0000.NUM_REC`, que ele recusa com 40 | Decidido na E09: fica o máximo |

## Riscos de recusa falsa que seguem declarados

O agente ainda segue o manual nestes pontos, e nada mostra que o programa aceite mais. Se aparecer um caso real, a correção é a mesma da E10.

- **Tamanho que só o catálogo limita:** 185 campos, em que o descritor não indica limite. Em 163 deles, é o máximo de 255 do manual (p. 33 e 59), que o programa também aplica, como mostrou o `0000.NOME`. Nos outros 22, é um número da coluna, como `L100.CODIGO` (50), `M312.NUM_LCTO` (50) e `W100.TIP_MOEDA` (3). Acima dele, o agente recusa, e o programa talvez aceite até 255; são códigos de tabela e valores que um arquivo válido não ultrapassa.
- **Ocorrência máxima:** `C040`, `E030`, `K030`, `L030`, `M030`, `N030`, `S030`, `T030`, `V030`, `Y680` e `Y682` têm máximo no manual e não têm no descritor. O teste com 14 `L030` parou em "Data inválida", porque as datas precisam casar com o período; um arquivo válido não passa do máximo.
- **Tabelas:** a lista de `Y612.QUALIF` não foi comparada, porque o programa filtra uma tabela geral. A de `0020.IND_ALIQ_CSLL` vem de uma classe do programa, e só o valor 2 foi testado, recusado.

## Sem efeito no resultado

- **Catálogo mais permissivo:** `P030.PER_APUR` `A00`, `9001.IND_DAD` `1`, `0000.IND_SIT_INI_PER` `3` e `0000.SIT_ESPECIAL` `7`.
