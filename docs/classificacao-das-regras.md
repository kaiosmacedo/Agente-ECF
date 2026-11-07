# Classificação das regras da ECF

Critérios aprovados por Kaio Macedo em 2026-11-06, a partir da pesquisa em `docs/pesquisa/avaliacao-da-ecf-alem-do-manual.md`, que traz as fontes e o grau de certeza de cada ponto. Sem especialista fiscal na equipe, o assistente propõe com a fonte de cada critério, e Kaio Macedo aprova (P01). Uma revisão pontual por contador das partes sobre consequências legais continua recomendada.

Só a divisão entre `Erro` e `Aviso` é oficial. Veredito, classes e a lista de `grave` são do produto, e a resposta do agente não os apresenta como classificação da Receita Federal.

## Decisões aprovadas

| Nº | Decisão | Onde vale |
| --- | --- | --- |
| 1 | Critérios das três classes, como abaixo. | Mudanças 15, 08 e 09 |
| 2 | Lista inicial de `grave`, com as linhas abaixo. Os demais `Aviso` ficam com severidade `media`. | Mudanças 08 e 09 |
| 3 | Conflitos do manual: em K155 e K355, as páginas 156, 159, 160 e 162 prevalecem sobre as páginas 148 e 153. `REGRA_SALDOS_M010_E020`, `REGRA_EXISTENCIA_M010`, `REGRA_CONTA_PERIODO_ANTERIOR` e `REGRA_VALOR_DETALHADO` ficam como `grave` até o gabarito. Responsável: Kaio Macedo. | Mudança 08 |
| 4 | O não verificado fica no campo `naoVerificados`, fora do cálculo do status, para a lista fixa. Gatilho presente no arquivo vira achado `ausencia-de-evidencias`. Validação obrigatória não executada segue em `limitacoes`. | Mudança 15 |
| 5 | Quando o regime exige recuperação, só o arquivo exportado do programa oficial pode chegar a `conforme`. O TXT de entrada termina em `parcial`. Com `TIP_ESC_PRE` = "L", os dados recuperados não se aplicam, com justificativa. | Mudanças 07, 08 e 09 |
| 6 | Gabarito no programa oficial: antes de fechar a mudança 08, uma pessoa roda arquivos sintéticos, sem assinar nem transmitir, cobrindo K155 × E155 com e sem justificativa, E020 × M010, conta da Parte B sumida, compensação acima de 30%, Outras Exclusões acima do gatilho, CNA divergente e conteúdo do 9100. Registra a versão do programa e das tabelas. | Mudança 08 |
| 7 | Regra pode ter tipo não declarado, e o nível só aparece quando o manual o declara. `critica` sem regra só para leiaute. `grave` aciona revisão humana. A síntese de `conforme-com-ressalvas` separa graves de observações. | Mudança 15 |

## Critério de cada classe

| Classe (severidade) | Critério proposto | Fonte do critério | Oficial ou produto | Efeito |
| --- | --- | --- | --- | --- |
| `impeditiva` (`critica`) | Violação de instância cujo tipo declarado, ou resolvido pela condição do próprio texto, é `Erro`; ou violação de leiaute que impede determinar a estrutura | Manual L12, p. 38 (`Erro`, nível 1), p. 33-36 e 43 (leiaute) | **Oficial**, com **Inferência** em dois elos: "funcionalidade" inclui transmissão, e leiaute violado equivale a `Erro` | `recusado` |
| `grave` (`alta`) | Não é `Erro` e cumpre as três condições: (1) a checagem é determinística e tem fonte oficial; (2) o valor entra na apuração de IRPJ ou CSLL, em saldo que passa ao ano seguinte ou em gatilho oficial; (3) há consequência oficial nomeada | IN 2.004, arts. 2º, 6º e 7º a 10; DL 1.598, art. 8º-A, II; Lei 8.218, art. 12, II; Lei 9.430, art. 44; MFD 10.002 e 10.003; Manual L12, p. 30-31 | **Decisão de produto** sobre fatos oficiais. Não muda o veredito | `conforme-com-ressalvas`, ou `parcial` se faltou validação; revisão humana (decisão 7) |
| `observacao` (`media`, `baixa`) | Todo o resto: `Aviso` sem efeito direto no tributo, heurística ou fonte oficial só de ano antigo. `media` é o padrão; `baixa` fica para comportamento de importação | Manual L12, p. 38 (`Aviso`); [FAQ de 2016, questão 8](http://sped.rfb.gov.br/pagina/show/2025) | **Oficial** para `Aviso`; **produto** para os demais e para `media` e `baixa` | `conforme-com-ressalvas`, ou `parcial` se faltou validação |

Três regras valem para todas as classes. Fonte não oficial pode ordenar prioridade, mas nunca cria checagem nem promove classe sozinha. A classe não substitui a categoria (P04, GD-05): um gatilho sem evidência pode ser `grave` e `ausencia-de-evidencias` ao mesmo tempo. E o agente nunca afirma que a Receita aprovou ou recusou, que não há risco de malha, ou quanto será a multa.

## Exemplos de `impeditiva`

Todos têm tipo `Erro` no Manual L12; a fonte e a natureza são as da linha `impeditiva` acima.

| Código | Página | O que verifica |
| --- | --- | --- |
| `REGRA_VALIDA_CNPJ`, `REGRA_ANO_DIFERENTE` | 39 | Nível 1.1: dígito do CNPJ; ano igual em início e fim |
| `REGRA_REC_ANTERIOR_OBRIGATORIO` | 66 | Retificadora precisa do recibo anterior |
| `REGRA_OCORRENCIA_UNITARIA_ARQ` | 70 | Registro único por arquivo |
| `REGRA_TRIBUT_INVALIDA` | 77 | Coerência entre forma de tributação e períodos |
| `REGRA_OBRIGATORIO_ASSIN_CONTADOR` | 105-106 | Presença de 0930 do contador; não prova certificado válido |
| `REGRA_BATIMENTO_K155` | 147-148 | Saldo final de um período igual ao inicial do seguinte |
| `REGRA_VALIDACAO_SOMA_SALDO_INICIAL` | 147-148 | `Erro` só com `FORMA_TRIB_PER` = "R"; nos demais, `observacao` |
| `REGRA_MAPEAMENTO`, `REGRA_VALIDACAO_SALDO_FINAL` | 149 | Soma do K156 igual ao K155; saldo final fecha |
| `REGRA_K915_JUSTIFICATIVA`, `REGRA_K935_JUSTIFICATIVA` | 159, 162 | Justificativa com 20 ou mais caracteres |
| `REGRA_BALANCO_SALDO_FINAL` | 225-226 | Ativo igual a passivo em A00, A12 ou T04 |
| `REGRA_SALDO_DISPONIVEL_PARTE_B` | 263 | Uso limitado ao saldo da conta da Parte B |
| `REGRA_PREJUIZO_FISCAL`, `REGRA_BC_NEGATIVA` | 269-270 | Lançamentos iguais à base de cálculo |
| `REGRA_EXISTE_IR_RET` | 567 | Y570 preenchido quando há dedução de retenção |

## Lista inicial de `grave`

Todas as linhas são decisão de produto. A multa por inexatidão (DL 1.598, art. 8º-A, II; Lei 8.218, art. 12, II) é a consequência de fundo de todas, e a coluna traz a âncora específica. "Provisória" significa que a linha muda se o gabarito mostrar `Erro`.

| Achado | Manual L12 | Tipo declarado | Âncora oficial | Apoio não oficial | Firmeza |
| --- | --- | --- | --- | --- | --- |
| `REGRA_SALDOS_M010_E020` | p. 238-239 | Sem tipo | [IN 2.004, art. 7º, § 3º](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/114966); batimento na [página da ECF](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf) | Acórdãos 1102-001.827 e 1301-008.089, por resumo | Provisória |
| `REGRA_EXISTENCIA_M010` | p. 126, 238-239 | Conflito | Idem | Idem | Provisória |
| `REGRA_CONTA_PERIODO_ANTERIOR` | p. 240 | Conflito | Idem | Nenhum | Provisória |
| `REGRA_VALOR_DETALHADO`, `_CSLL` | p. 245-247, 259 | Sem tipo | [IN 2.004, art. 2º](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/114966) | Nenhum | Provisória |
| `REGRA_PEA` | p. 252, 264 | `Aviso` | [IN 1.700, art. 310, § 3º](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/81268) | BDO | Firme |
| `REGRA_COMPATIBILIDADE_K155_E155`, `_E155_K155`, `_K355_E355`, `_E355_K355` | p. 148, 153, 156-157, 160-161 | `Aviso`; "erro" com K915 ou K935 sem justificativa | [IN 2.004, art. 8º](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/114966); Manual L12, p. 15-17 | BDO, Negruni, VML, CFC | Firme com justificativa |
| `REGRA_COMPATIBILIDADE_C155_E015`, `_C157_E015`, `_E015_C155_C157`; `REGRA_EXISTENCIA_K155_E015`, `_E015_K155` | p. 125, 149 | `Aviso` | IN 2.004, art. 8º; Manual L12, p. 16 | Negruni | Firme |
| `REGRA_PERCENTUAL_MAIOR_6`, `_GERAL`; `REGRA_BASE_CAL_IGUAL`, `_ISENCAO_REDUCAO` | p. 296 | `Aviso` | [Orientações 10.003, p. 5](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/clique_lucro_real_anual__site_rfb_orientacoes_fape_insuf_lra___v01_producao.pdf): dedução acima do permitido | Nenhum | Firme; exemplo de 2021 |
| Compensação acima de 30%, sem `REGRA_` | p. 179-181, 216-217 | Sem regra | [Planilha L12](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/tabelas-dinamicas-e-planos-de-contas-referenciais-leiaute-12-atualizacao-28-05-2026/tabelas_dinamicas_ecf_leiaute_12_28_05_2026_ac_2025_sit_esp_2026.xlsx/@@display-file/file), M300A/173; [Lei 9.065, arts. 15 e 16](https://www.planalto.gov.br/ccivil_03/leis/l9065.htm); Orientações 10.003, p. 5 | [Fórum de 2017](https://www.contabeis.com.br/forum/contabilidade/257563/compensacao-prejuizo-fiscal-lucro-real-trimestral/), fraco | Firme; exclui rural, inativas e liquidação |
| Outras Exclusões acima de R$ 20 milhões e de 30%, sem `REGRA_` | p. 30-31 | Sem regra | [Comunicado de 30/06/2026](https://www.gov.br/sped/pt-br/assuntos/comunicados/ecf/ecf-outras-exclusoes-sem-relacionamento) | [GSGA](https://gsga.com.br/pt/informativo/receita-federal-atualiza-manual-da-ecf-com-a-criacao-de-novos-registros-orientacoes-sobre-preenchimento-e-controle-de-exclusoes-fiscais), [Martello](https://www.martello.com.br/post/post18092026) | Firme como conferência, categoria `ausencia-de-evidencias` |
| Retenção deduzida acima do Y570, sem `REGRA_` de valor | p. 565-567 | Sem regra | Orientações 10.003, p. 5-7: o excesso é glosado | Grant Thornton, BDO | Firme como conferência; a Receita também aceita DIRF como lastro |

## Exemplos de `observacao`

| Achado | Manual L12 | Tipo declarado | Por que não é `grave` |
| --- | --- | --- | --- |
| `REGRA_EXISTENCIA_MAPEAMENTO` | p. 124 | `Aviso` | Mudança de mapeamento sem efeito direto no tributo |
| `REGRA_REGISTRO_M312_OBRIGATORIO`, `_M362_` | p. 253, 265 | `Aviso` | A própria Receita rebaixou de `Erro` a `Aviso` no leiaute 10 |
| `REGRA_RELACIONAMENTO_PATRIMONIAL`, `REGRA_INDICADOR_CONTABIL_DIFERENTE` | p. 254, 266 | `Aviso` | Qualidade do vínculo com a conta contábil |
| `REGRA_COMPARA_CONT_REF_SI`, `_SF` (L100, P100, U100) | p. 225-226, 330-331, 404-405 | `Aviso` | Balanço sem efeito direto; mencionar o risco do 3%, ainda discutido no CARF |
| `REGRA_RECEITA_ISENCAO`, `_REDUCOES` | p. 466, 474 | `Aviso` | O erro pode estar só no lado informativo, o X280 |
| `REGRA_POS_TRIB_VALOR`; `REGRA_Y672_CAPITAL_NAO_INFORMADO` | p. 489; p. 592 | `Aviso` | População restrita ou preenchimento |
| `REGRA_LINHA_DESPREZADA` (`media`); `REGRA_LINHA_ATUALIZADA`, `REGRA_DUPLICIDADE_DESPREZADA` (`baixa`) | p. 226; p. 226; p. 144 | `Aviso` | Só quando vierem no 9100; a linha desprezada merece destaque porque o valor foi descartado |
| Divergência em linha CA ou CNA de arquivo do PGE, sem `REGRA_` | Planilha L12; p. 35 | Sem regra | Sem fonte para tolerância. N630(3), N630(4) e N670(2) sobem a `grave` depois do gabarito, pois as Orientações 10.003, p. 5, citam erro de cálculo nelas |
| Receita zerada, sem `REGRA_` | Não localizada | Sem regra | [MFD 10.001](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/operacao-10-001-ecf-zerada) cobre 2018 e 2019; sai como `ausencia-de-evidencias` |

Nessa tabela, tudo é **produto**, salvo o tipo `Aviso`, que é oficial. Os 13 códigos `Habilita` (32 instâncias, em W100, W200 e W250) parecem comportamento de tela e não geram achado até decisão em contrário. Mapeamento conceitualmente suspeito só pode sair como `indicio-da-ia`, nunca de validador.

## O que o agente verifica, o que pede insumo e o que declara como não verificado

Hoje nenhuma checagem roda: `MontadorDoEnvelope` declara os validadores como "nenhum", e as mudanças 08 e 09 só têm especificação. A tabela abaixo diz o que cada uma pode entregar com as fontes atuais.

| Grupo | O que entra | Como sai no resultado |
| --- | --- | --- |
| **Só com o arquivo** | Camadas A e B (586 instâncias, 344 códigos): estrutura, formato, datas, preenchimento, aritmética, consistência interna e domínio | Achado com regra, registro e página |
| **Só com o arquivo** | Compensação acima de 30%; gatilho de Outras Exclusões; retenção × Y570; recálculo das linhas com fórmula de significado evidente, só depois do gabarito | Achado com fonte fora do manual |
| **Só com o arquivo** | Lado ECF dos confrontos: P300(15), P500(13), N630A(26), N630B(23), N670(21), P200(11, 12), P400(7, 8), saldo negativo | Lista de valores a conferir, sem conclusão |
| **Só com arquivo do PGE** | Camada D (22 instâncias, 20 códigos); C × E × K; K915 e K935 por valor; 9100 | "Coerente internamente, sem evidência externa"; ausente, é "não aplicável a este arquivo" |

| Insumo a pedir (ordem é **Inferência** minha, por volume da malha) | O que libera |
| --- | --- |
| Relatórios de DCTF ou DCTFWeb e PER/DCOMP, por período e código | A pagar × declarado; fórmula do indicador 4.1.1 só como informação, nunca como nota do Sintonia |
| Arquivo da ECD, conferido por `C040.HASH_ECD` (p. 110) | Saldos e plano reais; camada E |
| ECF anterior e, em retificadora, a ECF retificada | Parte B e hash reais; mudança de regime (IN 2.004, art. 7º, § 2º) |
| Informes de retenção | Y570 contra as fontes pagadoras |
| EFD-Contribuições, EFD ICMS/IPI, NF-e e CT-e | Receita, com vínculo oficial limitado às operações 10.001 e 10.009 |
| Protocolo do Requerimento Web, recibo com data, DCTF de inatividade | Gatilho cumprido; prazo e multa por atraso; inatividade |
| Gabarito do PGE | Conflitos de tipo, tolerâncias, CNA e parte do que está nos `_REGRAS` |

Todo insumo além de dado sintético depende de DP-11, e E02 da mudança 09 ainda está sem resposta.

| Declarar sempre como não verificado, salvo evidência fornecida | Motivo |
| --- | --- |
| Validade de certificado e procuração; ECD e ECF anterior ativas na base; hash na base; cadastro do CNPJ; bloqueio de versão; recepção; multa por atraso | Conferidos na transmissão ou na base da Receita (p. 14, 28, 104) |
| Pagamento efetivo; PGFN; parcelamento; intimação; aviso de malha; autos; nota do Sintonia | Sistemas da Receita, com acesso autenticado |
| e-Financeira; EFD-Reinf e eSocial de terceiros; DIRF de fontes pagadoras; Siscomex | Dados prestados por terceiros |
| Documentos que comprovam prejuízo fiscal e base negativa; guarda de documentos | Estão fora da ECF, e a lei condiciona a compensação a eles |
| `REGRA_DV_RECIBO` (p. 66); `REGRA_VALIDA_HASH_ARQUIVO` (p. 611); dígito de CNPJ alfanumérico; camada C; 75 arquivos `_REGRAS`; fórmulas com funções sem definição | Sem algoritmo ou fonte acessível; adotar um violaria P01 |
