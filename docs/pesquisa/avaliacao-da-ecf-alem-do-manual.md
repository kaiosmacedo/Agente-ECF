# Separar bloqueio de risco na ECF

Um agente validador da ECF pode e deve ir além do manual em quatro frentes: coerência com a lei (limite de 30% na compensação de prejuízo, sentido dos lançamentos na Parte B), continuidade de saldos entre anos e entre ECD e ECF, gatilhos oficiais recentes (Outras Exclusões acima de R$ 20 milhões) e declaração expressa do que o arquivo não prova. A gravidade tem uma única fronteira oficial: **`Erro` impede a conclusão da funcionalidade e `Aviso` é "alerta apenas informativo"**, e nenhuma fonte pública que encontrei, oficial ou de mercado, define "grave". Por isso proponho manter `impeditiva` restrita a `Erro` e tratar `grave` como decisão de produto com três condições verificáveis, o que respeita P01 porque, no código atual, `grave` e `observacao` levam ao mesmo veredito. A separação importa porque os erros mais caros passam pelo programa oficial (PGE): a ação da Receita de 2026 que confronta a ECF com DCTF e pagamentos enviou avisos a **29.061 empresas, com R$ 4,91 bilhões de divergência**, e esse confronto não pode ser feito só com o arquivo. Nesse caso o agente mostra o lado da ECF e declara o confronto como não verificado. Duas ressalvas valem para tudo: nenhuma nota executou o PGE, e o projeto ainda não tem validador, então "verificável hoje" significa verificável com as fontes já disponíveis, depois das mudanças 08 e 09. O relatório termina com critérios, exemplos com código e página, a divisão do que é verificável e sete decisões para você aprovar.

**Como ler as marcas.** Citações começam com *Oficial* (Receita Federal, Sped, Planalto) ou *Não oficial* (escritórios, portais e fóruns, inclusive quando relatam a Receita). Três rótulos em negrito dão o grau de certeza: **Inferência** é conclusão minha, **Não confirmado** é o que nenhuma fonte ou teste fechou, e **Lido por resumo** é fonte que não foi lida na íntegra. "Manual L12" é o [Manual do Leiaute 12, atualização de 23/07/2026](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/manual_ecf_leiaute_12_20_05_2026_ac_2025_sit_esp_2026.pdf/@@display-file/file), versão registrada em `docs/fontes-oficiais.md`, e toda página citada é a do rodapé "Página N de 622". "Planilha L12" são as [Tabelas Dinâmicas do Leiaute 12, versão de 24/09/2026](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/tabelas-dinamicas-e-planos-de-contas-referenciais-leiaute-12-atualizacao-28-05-2026/tabelas_dinamicas_ecf_leiaute_12_28_05_2026_ac_2025_sit_esp_2026.xlsx/@@display-file/file).

## Só a fronteira entre `Erro` e `Aviso` é oficial; "grave" é decisão de produto

O manual classifica as inconsistências em `Erro`, que "impede a conclusão com sucesso da funcionalidade", e `Aviso`, "alerta apenas informativo", e organiza as validações em níveis: 1.1 e 1.2 na importação, 2 nos campos e 3 entre registros, este só na verificação de pendências ([Oficial: Manual L12, p. 38](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/manual_ecf_leiaute_12_20_05_2026_ac_2025_sit_esp_2026.pdf/@@display-file/file)). A frase mais direta sobre transmissão é antiga: "Advertências não impedem a transmissão" ([Oficial: FAQ de 2016, questão 8](http://sped.rfb.gov.br/pagina/show/2025)). A confirmação atual está nos registros K915 e K935: com a justificativa preenchida, "o erro se torna aviso, permitindo a entrega da ECF" (Manual L12, p. 156 e 160). **Inferência:** que todo `Erro` bloqueie a transmissão é leitura forte dessas passagens, sem texto que o diga com essas palavras. Assinatura e hash da ECF anterior só são conferidos na transmissão, contra dados externos (p. 14 e 104), então um arquivo pode importar bem e ainda não ser transmissível.

Acima dessa fronteira não há escala pública. A busca em Receita, CFC, Fenacon, IBRACON e grandes firmas não achou critério de severidade: a Grant Thornton lista cinco pontos de atenção, a BDO recomenda revisão e a Deloitte descreve um produto, sempre sem escala ([Não oficial: Grant Thornton](https://www.grantthornton.com.br/insights/artigos-e-publicacoes/ecf-revisao-das-informacoes-declaradas-para-manter-o-compliance-tributario/); [Não oficial: BDO](https://www.bdo.com.br/pt-br/noticias/2026/prazo-para-a-entrega-do-ecf-2026-ja-comecou;-saiba-o-que-muda); [Não oficial: Deloitte](https://www.deloitte.com/br/pt/services/tax/services/ecf-tool.html)). Materiais pagos como IOB, Econet e Thomson Reuters não foram consultados. Portanto `grave` só pode ser decisão de produto, como DP-07 e o design da mudança 15 já registram. Isso não fere P01, porque a classe não é critério de conformidade: em `Execucao.Veredito`, só `impeditiva` leva a `recusado`, e qualquer outro achado leva a `conforme-com-ressalvas`. P01 continua exigindo fonte oficial para cada checagem, e é nisso que a proposta se apoia.

O catálogo medido na versão Word do manual tem **382 códigos e 808 instâncias: 523 `Erro`, 231 `Aviso`, 32 `Habilita`, 19 sem tipo e 3 condicionais**. Seis códigos de comportamento de importação respondem por 181 dos `Aviso`, e sobram 50 sobre conteúdo. O número 382 é piso, porque as regras das tabelas dinâmicas ficam em 75 arquivos `_REGRAS` do programa, fora do manual (p. 40). Três atalhos falham na hora de classificar. A palavra "erro" engana: `REGRA_PEA` (p. 252) e `REGRA_EXISTENCIA_K155_E015` (p. 125) dizem "o erro ocorre" e são `Aviso`. O código não basta: `REGRA_COMPARA_CONT_REF_SF` é `Aviso` em L100, P100 e U100 e `Erro` em S100 (p. 225-226, 360), e `REGRA_COMPATIBILIDADE_L100_E010` aparece duas vezes na p. 226, uma com aviso e outra com erro. O nome também não garante o sentido entre leiautes: `REGRA_REGISTRO_M312_OBRIGATORIO` era `Erro` no leiaute 7 e é `Aviso` no 12 (p. 253). A unidade de classificação precisa ser a instância: código, registro e leiaute.

Os conflitos do manual atingem justamente os saldos que mais custam. A Parte B do e-Lalur e do e-Lacs é o controle de valores que afetam anos futuros, como prejuízo fiscal e base negativa. `REGRA_SALDOS_M010_E020`, que compara o saldo inicial dessa parte com o final do ano anterior, diz "O erro ocorre" e não traz a frase de tipo (p. 239). `REGRA_EXISTENCIA_M010` "gera um erro" na p. 126 e a "advertência ocorre" na p. 239. `REGRA_CONTA_PERIODO_ANTERIOR` tem `Erro` na coluna e "gera um aviso" no texto (p. 240). `REGRA_COMPATIBILIDADE_K155_E155` "gera um aviso" na p. 148, e a mesma situação é "erro" nas p. 156-157. Este último caso tem saída parcial: `REGRA_K915_JUSTIFICATIVA` é `Erro` quando a justificativa tem menos de 20 caracteres (p. 159), então K915 sem justificativa já é impeditivo por regra própria. **Não confirmado:** qual versão vale nos demais casos. Só a execução do PGE decide.

## Os erros mais caros passam pelo programa e cobram a conta anos depois

As normas dão ao agente matéria que o leiaute não cobre. A ECF cabe a todas as pessoas jurídicas, de forma centralizada pela matriz, salvo Simples Nacional, órgãos públicos, autarquias, fundações públicas e inativas, e deve conter "todas as operações que influenciem a composição da base de cálculo e o valor devido do IRPJ e da CSLL" ([Oficial: IN 2.004, arts. 1º e 2º](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/114966)). O prazo geral é o último dia útil de julho do ano seguinte, com regra própria para extinção, fusão, cisão e incorporação, e a assinatura exige certificado ICP-Brasil (art. 3º). No lucro real, a ECF é o próprio Lalur: a Parte A traz lucro líquido, ajustes e lucro real, e a Parte B controla prejuízos e valores futuros, com sinais definidos em norma ([Oficial: IN 1.700, art. 310](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/81268); [Oficial: RIR/2018, art. 277](https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/decreto/d9580.htm)). A compensação de prejuízo e base negativa é limitada a 30% do lucro líquido ajustado e condicionada à guarda de comprovantes, com exceções para atividade rural, inativas e liquidação, e vedações em mudança de controle e ramo e em sucessão ([Oficial: Lei 9.065, arts. 15 e 16](https://www.planalto.gov.br/ccivil_03/leis/l9065.htm); [Oficial: RIR/2018, arts. 580 a 586](https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/decreto/d9580.htm)). Desses textos saem checagens que os códigos `REGRA_` do manual não fazem.

Quando a ECF não cumpre isso, a IN 2.004/2021 reparte as multas: lucro real responde pelo art. 8º-A do DL 1.598/1977, e as demais pessoas jurídicas pelo art. 12 da Lei 8.218/1991 ([Oficial: IN 2.004, art. 6º](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/114966)).

| Situação | Lucro real | Demais pessoas jurídicas | Redução prevista |
| --- | --- | --- | --- |
| Atraso ou não entrega | 0,25% do lucro líquido por mês ou fração, até 10%; teto de R$ 100 mil (receita até R$ 3,6 milhões) ou R$ 5 milhões | 0,02% da receita bruta por dia, até 1% | Real: 90%, 75%, metade ou 25%, conforme o momento. Demais: à metade antes de ofício; "a 75%" na intimação |
| Inexatidão, incorreção ou omissão | **3% do valor, mínimo de R$ 100, sem o teto do § 1º** | 5% da operação, até 1% da receita bruta | Real: nada se corrigir antes de procedimento de ofício; metade no prazo da intimação. Demais: como acima |
| Requisitos de apresentação | Sem equivalente | 0,5% da receita bruta | Demais: como acima |

Fontes da tabela: [Oficial: DL 1.598, art. 8º-A](https://www.planalto.gov.br/ccivil_03/decreto-lei/del1598.htm); [Oficial: Lei 8.218, art. 12](https://www.planalto.gov.br/ccivil_03/leis/l8218.htm). A multa por atraso é gerada no momento da transmissão fora do prazo, e a retificação não a altera (Manual L12, p. 17 e 22). Fora da multa da obrigação, o tributo a menor sofre multa de ofício de 75%, de 100% nos casos qualificados e de 150% na reincidência ([Oficial: Lei 9.430, art. 44](https://www.planalto.gov.br/ccivil_03/leis/l9430.htm)), e a estimativa não paga, multa isolada de 50% ([Oficial: MFD 10.003](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/pj-parametro-10.003)). Não escriturar ou não apresentar a ECF leva ao arbitramento do lucro ([Oficial: IN 1.700, art. 226, VIII](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/81268); [Oficial: Lei 8.981, art. 47](https://www.planalto.gov.br/ccivil_03/leis/l8981.htm)), e a falsificação tem multa e via penal ([Oficial: DL 1.598, art. 7º, § 1º](https://www.planalto.gov.br/ccivil_03/decreto-lei/del1598.htm); [Oficial: Lei 8.137, art. 1º](https://www.planalto.gov.br/ccivil_03/leis/l8137.htm)).

O ponto de maior exposição é o 3%, calculado sobre "a diferença do valor inexato, incorreto ou omitido" ([Oficial: IN 1.700, art. 312, parágrafo único, I](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/81268)). **Inferência:** erro grande sem efeito no imposto pode gerar multa alta. O CARF, tribunal administrativo federal, está dividido, e os casos a seguir foram **lidos por resumo** de terceiros, sem os acórdãos. Manteve a multa o acórdão 1102-001.827, por voto de qualidade: R$ 33,7 milhões por saldos do M500 de 2017 diferentes dos finais de 2016, em divergência de R$ 2,25 bilhões ([Não oficial: Rota da Jurisprudência](https://rotadajurisprudencia.com.br/2026/02/por-voto-de-qualidade-carf-mantem-multa-regulamentar-de-r-337-milhoes-por-erro-na-ecf/)). Manteve também o 1301-008.089, por maioria, sobre a Parte B, com R$ 29,8 milhões reduzidos a R$ 14,9 milhões ([Não oficial: Rota da Jurisprudência](https://rotadajurisprudencia.com.br/2026/03/por-maioria-carf-mantem-multa-por-erro-em-obrigacao-acessoria-e-reforca-carater-objetivo-da-penalidade/)). Cancelou o 1201-007.197, por unanimidade, mais de R$ 53 milhões por erro de classificação na ficha L100 sem impacto tributário ([Não oficial: APET](https://apet.org.br/coluna/carf-cancela-multa-de-r-53-milhoes-por-erro-contabil-sem-impacto-tributario/)). O 1301-007.728 cancelou só a parte do e-Lacs, por entender que a regra alcança o Lalur ([Não oficial: Tributo Devido](https://tributodevido.com.br/portal/csll-multa-lacs-omissao/)).

Daí saem duas orientações para o produto. Corrigir antes de qualquer procedimento de ofício afasta o 3%, o que dá valor prático a um achado `grave` apontado antes da fiscalização. E o agente não deve calcular nem prometer multa: a LC 236, de 04/09/2026, criou no CTN teto de 75%, 100% e 150% sobre o tributo ou crédito afetado, exceto "multas isoladas desvinculadas" ([Oficial: LC 236/2026, art. 1º](https://www.planalto.gov.br/ccivil_03/leis/lcp/Lcp236.htm)). **Não confirmado:** se esse teto alcança o 3%. Não encontrei manifestação da Receita, da PGFN ou do CARF.

Os limites de retificação também ficam fora da validação comum. O prazo de cinco anos está no Manual L12 (p. 17) e na pergunta 022 do [P&R 2024](https://www.gov.br/receitafederal/pt-br/centrais-de-conteudo/publicacoes/perguntas-e-respostas/ecf/2024-perguntas-e-respostas-da-pessoa-juridica.pdf), que remete genericamente ao CTN, e não na IN 2.004. **Inferência:** a janela de seis anos-calendário em `docs/fontes-oficiais.md` vem de um exemplo do manual e deve ser tratada como orientação, não como regra legal fechada. A IN veda mudar o regime por retificação, salvo para lucro arbitrado, tira o efeito da retificadora que reduz tributo já enviado à PGFN, sob procedimento fiscal ou parcelado, e impõe cascata: Parte B alterada obriga a retificar anos seguintes, ECD substituta obriga a ECF e valor alterado obriga a DCTF ([Oficial: IN 2.004, arts. 7º a 10](https://normasinternet2.receita.fazenda.gov.br/#/consulta/externa/114966)). A guarda de documentos segue decadência e prescrição, e comprovantes com efeito futuro, como os de prejuízo fiscal, vão até a decadência dos exercícios afetados ([Oficial: Lei 9.430, art. 37](https://www.planalto.gov.br/ccivil_03/leis/l9430.htm); [Oficial: Lei 9.065, arts. 15 e 16](https://www.planalto.gov.br/ccivil_03/leis/l9065.htm)). O agente não vê PGFN, parcelamento, intimação nem documentos, então só pode alertar.

Entre os erros comuns, **só um tem frequência medida**: IRPJ ou CSLL a pagar na ECF sem correspondência em DCTF, DCOMP ou pagamento. Na edição de 2025, 15.999 das 28.443 empresas avisadas (56%) acabaram autuadas, em cerca de R$ 3,1 bilhões ([Oficial: Receita Federal, jun/2026](https://www.gov.br/receitafederal/pt-br/assuntos/noticias/2026/junho/receita-federal-inicia-nova-edicao-da-acao-de-conformidade-201cinsuficiencia-de-declaracao-e-recolhimento-de-irpj-csll201d)). Parte B, plano referencial e ECD contra ECF aparecem em quatro autores independentes cada, sem dado de frequência, e parte deles vende revisão de ECF ([Não oficial: BDO](https://www.bdo.com.br/pt-br/noticias/2026/prazo-para-a-entrega-do-ecf-2026-ja-comecou;-saiba-o-que-muda); [Não oficial: Negruni/Contábeis](https://mauronegruni.com.br/2026/08/09/ecd-e-ecf-no-lucro-real-veja-como-evitar-inconsistencias-retificacoes-e-multas/); [Não oficial: palestra relatada pelo CFC](https://cfc.org.br/noticias/ecf-tire-as-suas-duvidas-antes-da-entrega-da-escrituracao/)). Adicional do IRPJ, Imunes e Isentas e erro de regime não têm evidência confiável de recorrência. Descartei percentuais que circulam sem levantamento identificado, como 60% das rejeições no bloco P e 70% das inconsistências.

| Erro | Reação do programa, segundo o manual | Quando costuma aparecer |
| --- | --- | --- |
| A pagar diferente de DCTF, DCOMP ou DARF | Nenhuma regra localizada | Malha |
| Saldo da Parte B alterado | `REGRA_SALDOS_M010_E020`, sem tipo declarado (p. 239) | Validação, se for `Erro`; senão, fiscalização |
| Saldo da Parte B que some no M010 | `REGRA_EXISTENCIA_M010`, em conflito (p. 126 e 239) | Fiscalização |
| Compensação acima de 30% | Sem `REGRA_` no manual | Malha ou fiscalização |
| Saldo K155 ou K355 diferente da ECD | Vira aviso com justificativa (p. 156 e 160) | Entrega permitida; depois, fiscalização |
| Retenção deduzida sem lastro | `REGRA_EXISTE_IR_RET` só exige o campo preenchido (p. 567) | Malha ou PER/DCOMP |
| Regime juridicamente errado | Só coerência de campos (p. 77-78) | Fiscalização; a retificação não pode mudar o regime (p. 17) |

**Não confirmado:** os arquivos `_REGRAS` podem bloquear parte do que consta aqui como "sem regra". Eles não foram lidos.

## A Receita confronta a ECF com outras obrigações, e o arquivo mostra só um lado

A base oficial dos cruzamentos está nas páginas da Malha Fiscal Digital (MFD), a ação que cruza dados e avisa o contribuinte para se regularizar sem multa de ofício ([Oficial: MFD](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/malha-fiscal-digital)). O manual não sustenta esses confrontos: a busca no texto dá zero ocorrências de DCTFWeb, EFD-Contribuições, EFD ICMS/IPI, e-Financeira, EFD-Reinf, DIRF e malha, e o cruzamento só aparece como benefício geral do Sped (p. 11). As páginas da MFD foram **lidas por resumo**, salvo o PDF de orientações do lucro real anual.

| Confronto oficial | Lado da ECF | Fonte oficial | Limite da fonte |
| --- | --- | --- | --- |
| A pagar × DCTF, DCOMP e pagamento | P300(15), P500(13), N630A(26), N630B(23), N670(21); postergados P300(17), P500(15), N630A(28), N630B(25), N670(23) | [MFD 10.002](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/pj-parametro-10.002); [MFD 10.003](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/pj-parametro-10.003) | Exemplos de 2019 a 2021. Conferi na Planilha L12: mesmas linhas, mesmo significado |
| Estimativas deduzidas × pagas; retenções × Y570 ou DIRF | N620, N660, N630, N670, Y570 | [Orientações 10.003, p. 5-7](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/clique_lucro_real_anual__site_rfb_orientacoes_fape_insuf_lra___v01_producao.pdf) | Exemplos de 2021 |
| Receita financeira e JCP × DIRF | P200(11, 12), P400(7, 8) | [MFD 10.004](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/pj-parametro-10.004) | Lucro presumido; DIRF extinta a partir de 2025 |
| Receita × NF-e e CT-e | P200, P300, P400, P500 | [MFD 10.009](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/pj-parametro-10.009) | Ano-calendário de 2020 |
| Receita zerada × NF-e, e-Financeira, DIRF, Decred, EFD-Contribuições, EFD ICMS/IPI | P130 a P500, Q100 | [MFD 10.001](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/operacao-10-001-ecf-zerada) | 2018 e 2019; só receita zerada no presumido |
| Saldo negativo × PER/DCOMP | N630, N670, P300, P500 | Manual L12, p. 22; [guia PER/DCOMP](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/restituicao-ressarcimento-reembolso-e-compensacao/per_dcomp-web_-saldo-negativo-de-irpj-ou-csll.pdf) | Atual |
| ECD e ECF anterior | Blocos C e E | Manual L12, p. 12, 14 e 32 | Recuperação e batimento; não achei malha própria de ECF × ECD |

Três limites evitam vínculos falsos. Para EFD-Reinf e eSocial **não encontrei fonte oficial** que os compare com a ECF, apesar de textos de mercado afirmarem isso ([Não oficial: IOB](https://noticias.iob.com.br/duvidas-da-ecf/)). **Inferência:** como sucedem a DIRF, é razoável esperar que passem a ser usados, mas falta publicação. EFD-Contribuições e EFD ICMS/IPI só têm vínculo oficial com a ECF na operação de receita zerada; as operações [20.001](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/pj-parametro-20.001) e [30.001](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/pj-operacao-30.001) (oficiais) as comparam com a DCTF, sem citar a ECF. E a Receita não publica tolerâncias nem parâmetros futuros, de modo que não ter aviso de malha não prova ausência de divergência.

O agente precisa indexar a obrigação pelo ano-calendário. Até 2024 a referência é DCTF e DIRF. A partir de 2025 são a DCTFWeb com o Módulo de Inclusão de Tributos ([Oficial: Receita Federal, dez/2024](https://www.gov.br/receitafederal/pt-br/assuntos/noticias/2024/dezembro/publicada-instrucao-normativa-que-institui-o-modulo-de-inclusao-de-tributos-2013-mit-na-dctfweb-e-substitui-a-dctf)), eSocial e EFD-Reinf no lugar da DIRF ([Oficial: suporte à Dirf](https://www.gov.br/receitafederal/pt-br/canais_atendimento/fale-conosco/suporte-a-dirf)) e e-Financeira no lugar da Decred ([Oficial: Ministério da Fazenda, jan/2025](https://www.gov.br/fazenda/pt-br/canais_atendimento/imprensa/notas-a-imprensa/2025/janeiro/receita-federal-esclarece-evolucao-na-e-financeira)). O próprio Manual L12 ainda fala só em "DCTF" (p. 17, 23 e 26), e as páginas da MFD citam DIRF e anos antigos, então nenhum desses textos deve ser copiado para uma mensagem do agente. A LC 224/2025 não troca obrigação, mas muda presunção e alíquotas de CSLL a partir de 2026 ([Oficial: LC 224/2025](https://www.planalto.gov.br/ccivil_03/leis/lcp/lcp224.htm)), e 126 linhas calculadas da Planilha L12 só valem desde 2026, o que torna obrigatório filtrar por vigência.

Desde 2026, a divergência também pesa no programa Sintonia, que classifica o contribuinte por, entre outros fatores, "exatidão das informações prestadas nas declarações e nas escriturações" ([Oficial: LC 225/2026, art. 30](https://www.planalto.gov.br/ccivil_03/leis/lcp/lcp225.htm)). O indicador 4.1.1 compara DCTF e ECF, e o 4.1.4 penaliza retificações, segundo o Anexo da IN 2.316/2026, que só li em **reprodução** ([Não oficial: LegisWeb](https://www.legisweb.com.br/legislacao/?id=493299)). **Inferência:** aconselhar "retifique" tem custo próprio, então o agente apresenta o achado sem prometer efeito na nota. A classificação é conhecida só pelo contribuinte, salvo autorização, e está fora do alcance do agente. O Confia, voltado a grandes empresas, não traz regra específica sobre a ECF nas fontes lidas ([Oficial: Receita Federal, mar/2026](https://www.gov.br/receitafederal/pt-br/assuntos/noticias/2026/marco/bons-contribuintes-terao-mais-beneficios-junto-a-receita-federal-1)); a IN 2.317/2026 não foi lida.

## O que o arquivo prova depende de qual arquivo chega

Os documentos do projeto não dizem qual arquivo o agente recebe, e a resposta muda o escopo. Os blocos C e E são recuperados pelo sistema: não existem no arquivo de importação e aparecem na saída (p. 41 e 43-45). Na importação, os registros dinâmicos precisam trazer só código e valor das linhas `E` (p. 38), e K915, K935 e 9100 são inseridos pelo programa (p. 156, 160 e 614). **Inferência:** o TXT gerado por um ERP e o arquivo exportado do PGE são objetos diferentes. No primeiro, a ausência desses elementos é legítima e não pode virar erro nem aprovação. Quem usa `TIP_ESC_PRE` = "L" também não tem C, E, J e K (p. 13). Antes de qualquer checagem, o agente precisa detectar o tipo de arquivo pela presença de C, E, 9100, `HASH_ECF_ANTERIOR` e assinatura depois do 9999.

| Camada | Instâncias | Códigos | Exemplos | Situação |
| --- | --- | --- | --- | --- |
| A. Só o arquivo | 523 | 334 | `REGRA_VALIDA_CNPJ` p. 39; `REGRA_BATIMENTO_K155` p. 147-148 | Verificável |
| B. Arquivo e Planilha L12 | 63 | 10 | `REGRA_NAO_EXISTE_COD_CTA_REF` p. 137; `REGRA_OBRIGATORIO_TIPO_DIFERENTE_R` p. 231 | Verificável; a planilha já está registrada |
| C. Tabelas do programa fora da planilha | 14 | 7 | `REGRA_CALCULA_REAL_CNA` p. 450-452; `REGRA_Y612_QUALIF` p. 578 | Não verificado até registrar a fonte |
| D. Dados recuperados em C e E | 22 | 20 | `REGRA_EXISTENCIA_M010` p. 239; `REGRA_COMPATIBILIDADE_K155_E155` p. 148 | Só com arquivo do PGE; prova coerência interna, não que o bloco reflita a ECD real |
| E. A própria ECD | 2 | 2 | `REGRA_C157_OBRIGATORIO` p. 118 | Exige a ECD |
| F. Importação ou estado do programa | 182 | 7 | `REGRA_LINHA_DESPREZADA` p. 226; `REGRA_INTERPOLACAO_PGE` p. 66 | Não se aplica ao arquivo final como está escrita |
| G. Algoritmo não descrito | 2 | 2 | `REGRA_DV_RECIBO` p. 66; `REGRA_VALIDA_HASH_ARQUIVO` p. 611 | Não verificado |

A divisão em camadas é **Inferência** minha sobre medição local, com confiança alta na contagem por código e tipo e média nas fronteiras. Só o nível 1.1 tem lista no manual (10 códigos, p. 39); a separação entre 2 e 3 é aproximação, e o 1.2 não pode ser medido.

O cálculo de IRPJ e CSLL mora nas tabelas dinâmicas, com 485 linhas calculadas: 208 CA, que o usuário pode editar (p. 35), e 277 CNA, que não pode. Um leitor sintático entendeu 469 das 483 fórmulas em texto, e as 14 restantes têm defeito na própria planilha. O obstáculo é o significado: `T_DRE`, `DEBITO`, `CREDITO`, `BAL_RED` e `ARRED` não têm definição nas fontes lidas, e adotar um sentido por conta própria violaria P01. Também não há regra oficial para divergência em CNA. A única descrição é um fórum antigo, segundo o qual o valor do TXT é ignorado e recalculado ([Não oficial: Portal SPED Brasil](https://portalspedbrasil.com.br/?p=6497)). Por isso divergência em linha calculada não pode ser `impeditiva` sem regra específica.

Ler o registro 9100 é o ganho mais barato: ele traz nome da regra, mensagem, registro, campo, conteúdo informado e valor calculado (p. 614), e a Receita manda consultá-lo ao tratar a malha ([Oficial: MFD 10.002](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/revisao-de-declaracao-malha/pj-parametro-10.002)). **Não confirmado:** se ele se conserva em todo arquivo, porque nenhum arquivo real foi examinado.

## As notas divergem em pontos que mudam a proposta

| Estado da evidência | O que está nele |
| --- | --- |
| Oficial, lido no texto | Manual L12 e Planilha L12 (cópias locais); IN 2.004 e IN 1.700; DL 1.598, Leis 8.218, 9.430, 9.065, 8.981 e 8.137, CTN, RIR/2018 e LC 236; orientações da MFD 10.003, guia PER/DCOMP, Manual da ECD L9, P&R 2024 e 2025 |
| Oficial, **lido por resumo** | Páginas MFD 10.001 a 10.009, notícias da Receita, LC 224 e LC 225. Reli a notícia de jun/2026 hoje, pela mesma ferramenta, e os números bateram |
| Oficial, só por reprodução ou busca | IN 2.316/2026 (LegisWeb); IN 2.278/2025 e IN 2.237/2024 (resultado de busca); Relatório Anual da Fiscalização 2025-2026, que pediu autenticação e não foi usado como base; aviso de licença da página de download do PGE |
| Não oficial, página aberta | Resumos de acórdãos (Rota da Jurisprudência, APET, Tributo Devido), BDO, Grant Thornton, Negruni, CFC, VML/Contábeis, IOB, Martello, GSGA, ZNA. Só priorizam; não fundam critério |
| Não oficial, só trecho de busca | Machado Meyer (acórdão 1301-006.987, página com erro 403), JOTA (só o título), Domínio, Fiscal.io. Não sustentam nada aqui |
| **Não confirmado** | Comportamento real do PGE; conteúdo dos arquivos `_REGRAS`; se todo `Erro` bloqueia a transmissão; o que o servidor reaplica na recepção; bloqueio fora da janela de retificação; bloqueio sem Requerimento Web |
| **Inferência** | Camadas, ordem de insumos, leitura de "entrada" e "saída", e toda atribuição a `grave` e `observacao` |

Na síntese, conferi o Manual L12 (p. 38, 125, 126, 148, 156, 159, 226, 239, 240, 252 e 331) e a Planilha L12 onde as notas conflitavam ou deixavam lacuna. O resultado:

| Ponto | O que as notas diziam | O que vale | Base |
| --- | --- | --- | --- |
| `REGRA_SALDOS_M010_E020` | `Erro` em duas notas; sem tipo em uma | Sem tipo declarado | Manual L12, p. 239; a mesma redação existe em regra `Aviso` (p. 125) |
| `REGRA_VALIDA_CONTA_BALANCO_DRE`, `REGRA_LUC_BRUTO_VALOR` | Contradição ou `Aviso` | `Erro`; a extração do PDF desalinhou a coluna | Versão Word, p. 254, 266 e 485 |
| Limite de 30% | Sem fonte por linha | M300A/173 limita a 30% de M300/169, e 173 + 174 a M300/171 | Planilha L12, aba M300A |
| Estimativa deduzida acima do calculado | Candidata a grave | `observacao`: a orientação manda informar também "estimativas recolhidos a maior" | Planilha L12, N630A/24 |
| Campos da malha | Podem variar por ano | Mesmo significado no leiaute 12 | Planilha L12, N630A, N630B, N670, P300, P500 |
| Item 1.33, Outras Exclusões | "341.01 dos registros M350A/M350R" | 341.01 fica em M300R e M350R. Bancos e seguradoras usam 199.01 e 138.01, que o item não cita; não estender sem fonte | Planilha L12 |
| "Crítico" para omissão de receita, Parte B ou fraude | Sugerido em nota | Incompatível: `critica` é `impeditiva` e dá `recusado`. O teto para risco fiscal é `grave`; fraude fica como indício com revisão humana | `Achado.cs`, `Execucao.cs` |
| Multa de R$ 33,7 milhões | "3% sobre R$ 2,25 bilhões" | É exatamente 1,5%. **Inferência:** compatível com a redução à metade do § 3º; o resumo não o afirma | Cálculo meu |
| Multa de ofício | "75% a 225%" em página da MFD | 75%, 100% e 150% | Lei 9.430, art. 44, redação da Lei 14.689/2023 |
| Prazo em situação especial | P&R 2024 contraria a IN | Vale a IN: janeiro a abril até julho; maio a dezembro até o 3º mês seguinte | IN 2.004, art. 3º, § 2º; [FAQ de 2026](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/perguntas-frequentes) |
| Malha de 2025 | 5.536 empresas e R$ 3,55 bilhões, ou 28.443 e R$ 4,65 bilhões | **Não resolvido.** Uso a notícia de jun/2026; a de [set/2025](https://www.gov.br/receitafederal/pt-br/assuntos/noticias/2025/setembro/receita-federal-inicia-nova-edicao-da-acao-de-conformidade-201cinsuficiencia-de-declaracao-e-recolhimento-de-irpj-csll) recusou conexão hoje | Duas fontes oficiais |
| Versão e endereços | Propostas 08 e 09 citam manual de 20/05/2026; FAQ de 2016 aparece em `/show/2025` e `/show/2026` | Alinhar a 23/07/2026 (P01, DP-04). O endereço da FAQ não pôde ser conferido: o host recusou conexão | `docs/fontes-oficiais.md` |

## Conclusão e proposta para aprovação

A pesquisa começou perguntando quais regras são graves, e a decisão de maior efeito acabou sendo outra: onde mora o não verificado. Em `Execucao.Status`, qualquer item em `Limitacoes` leva a `parcial`; em `ExigeRevisaoHumana`, qualquer achado de ausência de evidências aciona revisão. Se os confrontos externos entrarem de forma permanente em um desses lugares, nenhuma ECF chega a `conforme` e o veredito deixa de distinguir. Se ficarem de fora sem registro, `conforme` vira a falsa segurança que P05 e P08 proíbem.

Sem especialista fiscal na equipe, o melhor substituto para os conflitos do manual é um gabarito do próprio PGE: arquivos sintéticos rodados por uma pessoa, sem assinar nem transmitir, com o relatório guardado como referência. Ele custa pouco e resolve de uma vez as dúvidas de tipo que nenhuma fonte fecha. Os textos sobre consequências legais continuam sendo leitura de quem não é da área, então convém uma revisão pontual por contador antes do uso com dados reais (DP-11).

### Critério de cada classe

| Classe (severidade) | Critério proposto | Fonte do critério | Oficial ou produto | Efeito |
| --- | --- | --- | --- | --- |
| `impeditiva` (`critica`) | Violação de instância cujo tipo declarado, ou resolvido pela condição do próprio texto, é `Erro`; ou violação de leiaute que impede determinar a estrutura | Manual L12, p. 38 (`Erro`, nível 1), p. 33-36 e 43 (leiaute) | **Oficial**, com **Inferência** em dois elos: "funcionalidade" inclui transmissão, e leiaute violado equivale a `Erro` | `recusado` |
| `grave` (`alta`) | Não é `Erro` e cumpre as três condições: (1) a checagem é determinística e tem fonte oficial; (2) o valor entra na apuração de IRPJ ou CSLL, em saldo que passa ao ano seguinte ou em gatilho oficial; (3) há consequência oficial nomeada | IN 2.004, arts. 2º, 6º e 7º a 10; DL 1.598, art. 8º-A, II; Lei 8.218, art. 12, II; Lei 9.430, art. 44; MFD 10.002 e 10.003; Manual L12, p. 30-31 | **Decisão de produto** sobre fatos oficiais. Não muda o veredito | `conforme-com-ressalvas`, ou `parcial` se faltou validação; revisão humana (decisão 7) |
| `observacao` (`media`, `baixa`) | Todo o resto: `Aviso` sem efeito direto no tributo, heurística ou fonte oficial só de ano antigo. `media` é o padrão; `baixa` fica para comportamento de importação | Manual L12, p. 38 (`Aviso`); [FAQ de 2016, questão 8](http://sped.rfb.gov.br/pagina/show/2025) | **Oficial** para `Aviso`; **produto** para os demais e para `media` e `baixa` | `conforme-com-ressalvas`, ou `parcial` se faltou validação |

Três regras valem para todas as classes. Fonte não oficial pode ordenar prioridade, mas nunca cria checagem nem promove classe sozinha. A classe não substitui a categoria (P04, GD-05): um gatilho sem evidência pode ser `grave` e `ausencia-de-evidencias` ao mesmo tempo. E o agente nunca afirma que a Receita aprovou ou recusou, que não há risco de malha, ou quanto será a multa.

### Exemplos de `impeditiva`

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

### Lista inicial de `grave`

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

### Exemplos de `observacao`

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

### O que o agente verifica, o que pede insumo e o que declara como não verificado

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

### Decisões a aprovar

| Nº | Decisão | Recomendação | Alternativa e seu custo | O que fecha |
| --- | --- | --- | --- | --- |
| 1 | Critérios das três classes | Aprovar a tabela de critérios | Ampliar `impeditiva` para risco fiscal: `recusado` passaria a afirmar algo que o programa oficial não faz | E03 da mudança 15; E02 da 08 |
| 2 | Lista inicial de `grave` | Aprovar as 11 linhas; demais `Aviso` ficam `media` | Começar só por Parte B e K155: menos revisão agora, e fica de fora o limite de 30% | E03 da 15; insumo para E01 e E03 da 09 |
| 3 | Precedência nos conflitos (exigida por P01) | K155 e K355: p. 156, 159, 160 e 162 prevalecem sobre p. 148 e 153. `REGRA_SALDOS_M010_E020`, `REGRA_EXISTENCIA_M010`, `REGRA_CONTA_PERIODO_ANTERIOR` e `REGRA_VALOR_DETALHADO`: `grave` até o gabarito | Tratar como `impeditiva` já: risco de recusa falsa em saldo legítimo | Registro de divergência com responsável |
| 4 | Onde mora o não verificado | Campo opcional `naoVerificados`, fora do cálculo do status, para a lista fixa. Gatilho presente no arquivo vira achado `ausencia-de-evidencias`. Validação obrigatória não executada segue em `Limitacoes` | Só `Limitacoes`: toda ECF fica `parcial`. Só achado: toda ECF fica com ressalva e revisão humana | Conjunto obrigatório de P05; altera a mudança 15 |
| 5 | Qual arquivo pode chegar a `conforme` | Só o exportado do PGE, quando o regime exige recuperação. TXT de entrada termina em `parcial`. Com `TIP_ESC_PRE` = "L", a camada D é não aplicável, com justificativa | Aceitar TXT como completo: `conforme` sem testar saldos recuperados, contra P05 | DP-05; E02 da 09 |
| 6 | Gabarito no PGE | Uma pessoa roda, antes de fechar a 08, sem assinar nem transmitir: K155 × E155 com e sem justificativa; E020 × M010; conta da Parte B sumida; compensação acima de 30%; Outras Exclusões acima do gatilho; CNA divergente; conteúdo do 9100. Registrar versão do programa e das tabelas | Automatizar: nenhuma fonte oficial documenta execução em lote, e o aviso de licença precisa ser lido antes | E03 da 08 |
| 7 | Ajustes no domínio | `TipoDaRegra` ganha "não declarado" (hoje só `Erro` e `Aviso`, e o envelope mostra `Aviso` para tudo que não é `Erro`). `Nivel` passa a opcional ou com origem "derivado", pois só o 1.1 é declarado. `critica` sem regra só para leiaute. `grave` aciona revisão humana (acima do mínimo de P09). O texto de `conforme-com-ressalvas` separa graves de observações | Manter: 51 instâncias sem encaixe (32 `Habilita`, 19 sem tipo) e nível apresentado como oficial sem ser | Mudanças 08 e 15 |
