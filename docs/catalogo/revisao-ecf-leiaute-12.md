# Revisão do catálogo estrutural da ECF, leiaute 12

Material para a revisão de Kaio Macedo (mudança 08, tarefa 1.4, E02). O catálogo vem do manual. Aqui ele é conferido contra o manual, por uma segunda leitura, e contra o programa oficial da ECF. Decisão pendente: E10, em `openspec/changes/validacao-estrutural/proposal.md`.

## Fontes

| Fonte | Versão | Identificação |
| --- | --- | --- |
| Manual da ECF, leiaute 12, versão em Word | Publicado em 23/07/2026 | SHA-256 em `docs/fontes-oficiais.md` |
| Descritor do leiaute no programa da ECF | Programa 12.2.8, estrutura 12003, versão 1 | `descritor/escrituracao/estrutura12003/v1/descritor.xml`, dentro de `lib/br.gov.serpro.sped.irpjpva/irpjpva-infra.jar`; SHA-256 `37af2548f8654b6e6a8ec8381e97cfdc703dad5e6bace31f28ed006375253b5a` |
| Tabelas de valores do programa | Programa 12.2.8 | `recursos/tabelas/`, na pasta de instalação |

O descritor e as tabelas foram só lidos; nenhum código do programa foi descompilado, e os arquivos não estão neste repositório. O catálogo revisado é o `ecf-l12-1.3.0`.

## O que está conferido

| Conferência | Resultado |
| --- | --- |
| Segunda leitura do Word, sem usar o gerador | 1.122 campos, nenhuma diferença em número, nome, tipo, tamanho, decimal, valores e obrigatoriedade |
| Registros contra o programa | Os 188 existem, na mesma ordem e no mesmo bloco; ao contrário, só o `S990` falta (D1) |
| Campos contra o programa | Mesma quantidade e mesma posição em todos os registros; em 53, o nome difere, como `QTD_LIN` e `QTD_LIN_0`, sem efeito, porque a leitura é por posição |
| Registro pai | Igual em todos os registros |
| Tabelas de valores | Das 19 listas em que o programa usa tabela, 13 são iguais; ver D5 e "Sem efeito" |

**Defeito corrigido no catálogo `ecf-l12-1.3.0`:** 8 campos dos registros M305, M310, M355, M360, M500 e M510 traziam na lista o texto da referência, como `M010.COD_CTA_B`, e o agente recusava toda linha real.

## Onde o agente recusa o que o programa aceita

Estas são as divergências que importam. O manual diz uma coisa, o programa define outra, e hoje o agente segue o manual e recusa. A coluna "Agente" foi testada em produção com ECF sintética em 2026-11-08; **o comportamento do programa vem do descritor e ainda não foi testado na tela, exceto o que o gabarito já cobriu.**

| # | Divergência | Manual | Programa | Agente hoje | Proposta (E10) |
| --- | --- | --- | --- | --- | --- |
| D1 | Registro `S990` | Não existe (nenhuma menção) | Encerramento do Bloco S, obrigatório | Recusa o arquivo que o traz (`EST-ARQ-002`) | Aceitar: incluir no catálogo |
| D2 | `0000.NUM_REC` | `C`, 40 (p. 65) | Exatamente 41 | Recusa 41 e aceita 40 (`EST-CAMPO-003`) | Até 41 |
| D3 | Sinal em 21 campos | `N`, sem sinal (p. 33) | `NS` | Recusa valor negativo (`EST-CAMPO-002`) | Aceitar o sinal |
| D4 | Texto em 16 campos | `N` ou `NS` | `C` | Recusa texto (`EST-CAMPO-002`) | Tipo `C` |
| D5 | Valores: `M300` e `M350.TIPO_LANCAMENTO` `R` (rótulo); `0010.TIP_ENT` `16` | Sem esses valores | Aceita | Recusa (`EST-CAMPO-006`) | Aceitar |
| D6 | Tamanho em 60 campos | Limite menor | Limite maior | Recusa acima do limite do manual (`EST-CAMPO-003`) | Limite do programa |
| D7 | Obrigatoriedade em 45 campos | "Sim" | Opcional | Recusa vazio (`EST-CAMPO-001`) | `observacao`, que não recusa e segue citando o manual |
| D8 | Registros `C001`, `C990` e `E001` | `O`/`O`, mas o bloco não é importado (p. 41) | Não exige na importação, e o próprio programa preenche esses blocos | Recusa a falta (`EST-OCOR-001`) | `observacao`, depois de testar na tela |

A E05 fica como Kaio Macedo decidiu: a falta do 9999 continua recusando o arquivo. `0030` continua obrigatório: o manual e o programa o marcam como obrigatório, e não é um bloco que o programa preencha sozinho.

### Campos de cada divergência

**D3, sinal:** `N615.BASE_CALC`, `W200.VL_REC_NAO_REL_EST`, `W200.VL_REC_NAO_REL`, `W200.VL_REC_REL_EST`, `W200.VL_REC_REL`, `W200.VL_REC_TOTAL_EST`, `W200.VL_REC_TOTAL`, `W200.VL_IR_PAGO_EST`, `W200.VL_IR_PAGO`, `W200.VL_IR_DEVIDO_EST`, `W200.VL_IR_DEVIDO`, `W200.VL_CAP_SOC_EST`, `W200.VL_CAP_SOC`, `W200.VL_LUC_ACUM_EST`, `W200.VL_LUC_ACUM`, `W200.VL_ATIV_TANG_EST`, `W200.VL_ATIV_TANG`, `X280.REC_LIQ_INCENTIVO`, `X280.VL_INCENTIVO`, `X370.VL_TRANSACAO`, `X370.VL_COMPENSATORIO`.

**D4, texto:** `0010.FORMA_TRIB`, `0010.IND_REC_RECEITA`, `0020.IND_ALIQ_CSLL`, `0030.NUM_TEL`, `M300.IND_RELACAO`, `M350.IND_RELACAO`, `W100.TIN_SUBSTITUTA`, `X280.NCM_INCENTIVO`, `X340.PAIS`, `X357.PAIS`, `Y600.QUALIF_REP_LEG`, `Y800.TIPO_DOC`, `9100.NOM_REGRA`, `9100.CONTEÚDO`, `9100.VALOR_ESPERADO`, `9100.CNAE`.

**D6, descrições (255 no manual, 512 no programa):** 46 campos `DESCRICAO`: `L100.DESCRICAO`, `L210.DESCRICAO`, `L300.DESCRICAO`, `M300.DESCRICAO`, `M350.DESCRICAO`, `N500.DESCRICAO`, `N600.DESCRICAO`, `N610.DESCRICAO`, `N620.DESCRICAO`, `N630.DESCRICAO`, `N650.DESCRICAO`, `N660.DESCRICAO`, `N670.DESCRICAO`, `P100.DESCRICAO`, `P130.DESCRICAO`, `P150.DESCRICAO`, `P200.DESCRICAO`, `P230.DESCRICAO`, `P300.DESCRICAO`, `P400.DESCRICAO`, `P500.DESCRICAO`, `S100.DESCRICAO`, `S150.DESCRICAO`, `S200.DESCRICAO`, `S300.DESCRICAO`, `S500.DESCRICAO`, `S700.DESCRICAO`, `T120.DESCRICAO`, `T150.DESCRICAO`, `T170.DESCRICAO`, `T181.DESCRICAO`, `U100.DESCRICAO`, `U150.DESCRICAO`, `U180.DESCRICAO`, `U182.DESCRICAO`, `X292.DESCRICAO`, `X390.DESCRICAO`, `X451.DESCRICAO`, `X460.DESCRICAO`, `X470.DESCRICAO`, `X480.DESCRICAO`, `X490.DESCRICAO`, `X500.DESCRICAO`, `X510.DESCRICAO`, `Y681.DESCRICAO`, `Y750.DESCRICAO`.

**D6, demais:** `0000.NUM_REC` (40 no manual, [41] no programa), `0000.EVENTO_SCP` (1 no manual, 2 no programa), `0930.FONE` (14 no manual, 15 no programa), `L100.COD_NAT` (2 no manual, 4 no programa), `L300.COD_NAT` (2 no manual, 4 no programa), `P100.COD_NAT` (2 no manual, 4 no programa), `P150.COD_NAT` (2 no manual, 4 no programa), `S100.COD_NAT` (2 no manual, 4 no programa), `S150.COD_NAT` (2 no manual, 4 no programa), `U100.COD_NAT` (2 no manual, 4 no programa), `U150.COD_NAT` (2 no manual, 4 no programa), `V100.CODIGO` (2 no manual, 6 no programa), `X365.IDENTIFICADOR` (6 no manual, 10 no programa), `X370.IDENTIFICADOR` (6 no manual, 10 no programa).

**D7, obrigatoriedade:** `0020.IND_ALIQ_CSLL`, `0020.IND_QTE_SCP`, `0930.FONE`, `C040.IDENT_MF`, `C050.IND_CTA`, `C053.COD_IDT`, `C053.COD_CNT_CORR`, `C053.NAT_SUB_CNT`, `C155.IND_VL_SLD_INI`, `C155.IND_VL_SLD_FIN`, `E020.COD_PB_RFB`, `E155.IND_VL_SLD_INI`, `E155.IND_VL_SLD_FIN`, `E355.IND_VL_SLD_FIN`, `L100.TIPO`, `L300.TIPO`, `M500.COD_CTA_B`, `M500.SD_INI_LAL`, `M500.VL_LCTO_PARTE_A`, `M500.SD_FIM_LAL`, `M510.SD_INI_LAL`, `M510.VL_LCTO_PARTE_A`, `M510.SD_FIM_LAL`, `P150.TIPO`, `S150.TIPO`, `U150.TIPO`, `W100.IND_CONTROLADORA`, `W100.NOME_CONTROLADORA`, `W100.JURISDICAO_CONTROLADORA`, `W100.TIN_CONTROLADORA`, `W100.IND_ENTREGA`, `W100.IND_MODALIDADE`, `W250.JURISDICAO_TIN`, `X351.RES_POS_TRIB`, `X370.VL_ESPONTANEO`, `X370.VL_COMPENSATORIO`, `X370.IND_TRANS_COMBINADAS`, `X370.IND_DADOS_MULTIP`, `X370.IND_SIMPLIFIC`, `Y612.VL_REM_TRAB`, `Y612.VL_DEM_REND`, `Y612.VL_IR_RET`, `Y720.LUC_LIQ`, `Y720.DT_LUC_LIQ`, `Y720.INTIMACAO`.

## Onde o agente aceita o que o programa recusa

Não dá falso alarme, e o veredito segue `parcial` até as mudanças 09 e 10. A proposta é deixar como está e reavaliar na mudança 09.

| Divergência | Quantidade | Campos |
| --- | --- | --- |
| Obrigatório no programa, "Não" no manual | 25 | `0021.IND_REPES`, `0021.IND_RECAP`, `0021.IND_PADIS`, `0021.IND_REIDI`, `0021.IND_RECINE`, `0021.IND_RETID`, `0021.IND_OLEO_BUNKER`, `0021.IND_REPORTO`, `0021.IND_RET_II`, `0021.IND_RET_EEI`, `0021.IND_REDATA`, `M410.COD_CTA_B`, `P100.DESCRICAO`, `S100.DESCRICAO`, `W300.IND_REC_NAO_REL`, `W300.IND_REC_REL`, `W300.IND_REC_TOTAL`, `W300.IND_LUC_PREJ_ANTES_IR`, `W300.IND_IR_PAGO`, `W300.IND_IR_DEVIDO`, `W300.IND_CAP_SOC`, `W300.IND_LUC_ACUM`, `W300.IND_ATIV_TANG`, `W300.IND_NUM_EMP`, `X280.VL_INCENTIVO` |
| Data no programa, sem conferência de data no catálogo | 7 | `Q100.DATA`, `X280.VIG_INI`, `X280.VIG_FIM`, `Y620.DATA_AQUIS`, `Y630.DAT_ABERT`, `Y630.DAT_ENCER`, `Y730.DATA` |
| Tamanho exato no programa | 465 campos | Decidido na E09: fica o máximo |

## Sem efeito no resultado

- **Chave de duplicidade:** difere em 23 registros (`0035`, `0930`, `C157`, `K915`, `K935`, `M010`, `M500`, `M510`, `N500`, `N650`, `V020`, `V030`, `V100`, `X357`, `X370`, `X485`, `Y590`, `Y680`, `Y682`, `Y730`, `Y750`, `Y800`, `9900`). O achado é só um aviso, mas a proposta da E10 é seguir a chave do programa, para não dizer "retire a linha repetida" de uma linha legítima, como dois ativos iguais em países diferentes no `Y590`.
- **Listas que só o catálogo confere:** 77 campos, quase todos `S`/`N` ou `D`/`C`, em que o programa não tem lista no campo. O manual é a fonte e o valor fora da lista é improvável.
- **Catálogo mais permissivo:** `P030.PER_APUR` `A00`, `9001.IND_DAD` `1`, `0000.IND_SIT_INI_PER` `3`, `0000.SIT_ESPECIAL` `7` e os valores de `Y612.QUALIF`, em que o programa usa uma tabela geral.
