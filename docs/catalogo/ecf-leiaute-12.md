# Catálogo estrutural da ECF, leiaute 12

Gerado por `.github/scripts/catalogo/gerar-catalogo.py` a partir da versão em Word do manual, com as páginas do PDF. Versão `ecf-l12-1.2.0`. É o que Kaio Macedo revisa (mudança 08, E02). Correções vão no gerador, nunca no JSON.

**Ordem dos blocos:** 0, C, E, J, K, L, M, N, P, Q, S, T, U, V, W, X, Y, 9.

**Divergências do próprio manual, sem achado (REQ-ESTRUT-011):** Registro 0010, p. 71: o campo FORMA_TRIB tem tamanho 1 na coluna, e os valores válidos [1;2;3;4;5;6;7;8;9;10] vão até 2 caracteres; vale a lista, que o programa da ECF aceita. Registro Y681, p. 596: o campo REG traz [X681] nos valores válidos, e não [Y681]. Ocorrência e nível divergentes entre o cabeçalho do registro e a Tabela de Registros (p. 43 a 58) ficam sem verificação.

| Registro | Nome | Nível | Pai | Ocorrência (cabeçalho / tabela) | Entrada / saída | Página |
| --- | --- | --- | --- | --- | --- | --- |
| 0000 | Abertura do Arquivo Digital e Identificação da Pessoa Jurídica | 0 | — | 1:1 / [1;1] | O / O | 63 |
| 0001 | Abertura do Bloco 0 | 1 | 0000 | 1:1 / [1;1] | O / O | 70 |
| 0010 | Parâmetros de Tributação | 2 | 0001 | 1:1 / [1;1] | O / O | 71 |
| 0020 | Parâmetros Complementares | 2 | 0001 | 1:1 / [1;1] | O / O | 81 |
| 0021 | Parâmetros de Identificação dos Tipos de Programa | 2 | 0001 | 0:1 / [0;1] | F / OC | 97 |
| 0030 | Dados Cadastrais | 2 | 0001 | 1:1 / [1;1] | O / O | 101 |
| 0035 | Identificação das SCP | 2 | 0001 | 0:N / [0:N] | F / OC | 103 |
| 0930 | Identificação dos Signatários da ECF | 2 | 0001 | 1:N / [1;N] | F / O | 105 |
| 0990 | Encerramento do Bloco 0 | 1 | 0000 | 1:1 / [1;1] | O / O | 108 |
| C001 | Abertura do Bloco C | 1 | 0000 | 1:1 / [1;1] | O / O | 109 |
| C040 | Identificador da ECD | 2 | C001 | 0:12 / [0;12] | N / OC | 110 |
| C050 | Plano de Contas da ECD | 3 | C040 | 0:N / [0;N] | N / OC | 112 |
| C051 | Plano de Contas Referencial | 4 | C050 | 0:N / Se C050.IND_CTA = “A” [0:N] Senão [0] (divergente, não verificada) | N / OC | 113 |
| C053 | Subcontas Correlatas | 4 | C050 | 0:N / [0;N] | N / OC | 114 |
| C100 | Centro de Custos | 3 | C040 | 0:N / [0;N] | N / OC | 115 |
| C150 | Identificação do Período dos Saldos Periódicos das Contas | 3 | C040 | 0:12 / [0;13] (divergente, não verificada) | N / OC | 116 |
| C155 | Detalhes dos Saldos Contábeis das Contas | 4 | C150 | 1:N / [0; N] (divergente, não verificada) | N / OC | 117 |
| C157 | Transferência de Saldos do Plano de Contas Anterior | 5 | C155 | 1:N / [0;N] (divergente, não verificada) | N / OC | 118 |
| C350 | Identificação da Data dos Saldos das Contas de Resultado Antes do Encerramento | 3 | C040 | 0:N / [0; N] | N / OC | 119 |
| C355 | Detalhes dos Saldos das Contas de Resultado Antes do Encerramento | 4 | C350 | 0:N / [0;N] | N / OC | 120 |
| C990 | Encerramento do Bloco C | 1 | 0000 | 1:1 / [1;1] | O / O | 121 |
| E001 | Abertura do Bloco E | 1 | 0000 | 1:1 / [1;1] | O / O | 122 |
| E010 | Saldos Finais Recuperados da ECF Anterior | 2 | E001 | 0:N / [0;N] | N / OC | 123 |
| E015 | Contas Contábeis Mapeadas | 3 | E010 | 0:N / [0;N] | N / OC | 124 |
| E020 | Saldos Finais das Contas na Parte B do e-Lalur da ECF Imediatamente Anterior | 2 | E001 | 0:N / [0;N] | N / OC | 126 |
| E030 | Identificação do Período | 2 | E001 | 0:13 / [0;13] | N / OC | 127 |
| E155 | Detalhes dos Saldos Contábeis Calculados com Base nas ECD | 3 | E030 | 1:N / [0;N] (divergente, não verificada) | N / OC | 129 |
| E355 | Detalhes dos Saldos das Contas de Resultado Antes do Encerramento | 3 | E030 | 0:N / [0;N] | N / OC | 130 |
| E990 | Encerramento do Bloco E | 1 | 0000 | 1:1 / [1;1] | N / O | 131 |
| J001 | Abertura do Bloco J | 1 | 0000 | 1:1 / [1;1] | F / O | 132 |
| J050 | Plano de Contas do Contribuinte | 2 | J001 | 0:N / [0;N] | F / OC | 133 |
| J051 | Plano de Contas Referencial | 3 | J050 | 0:N / Se J050.IND_CTA = “A” [1:N] Senão [0] (divergente, não verificada) | F / OC | 136 |
| J053 | Subcontas Correlatas | 3 | J050 | 0:N / [0;N] | F / F | 138 |
| J100 | Centro de Custos | 2 | J001 | 0:N / [0;N] | F / FC | 141 |
| J990 | Encerramento do Bloco J | 1 | 0000 | 1:1 / [1;1] | F / O | 142 |
| K001 | Abertura do Bloco K | 1 | 0000 | 1:1 / [1;1] | F / O | 143 |
| K030 | Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL no Ano-Calendário | 2 | K001 | 0:13 / [0;13] | F / OC | 144 |
| K155 | Detalhes dos Saldos Contábeis (Depois do Encerramento do Resultado do Período) | 3 | K030 | 0:N / [0;N] | F / OC | 147 |
| K156 | Mapeamento Referencial do Saldo | 4 | K155 | 1:N / [0;N] (divergente, não verificada) | F / OC | 150 |
| K355 | Saldos Finais das Contas Contábeis de Resultado Antes do Encerramento | 3 | K030 | 0:N / [0;N] | F / F | 152 |
| K356 | Mapeamento Referencial dos Saldos Finais das Contas Contábeis de Resultado Antes do Encerramento | 4 | K355 | 1:N / [0;N] (divergente, não verificada) | F / OC | 154 |
| K915 | Justificativa para Divergência de Saldos Contábeis das Contas Patrimoniais Recuperados da ECD | 2 | K001 | 1:N / [0;N] (divergente, não verificada) | F / FC | 156 |
| K935 | Justificativa para Divergência de Saldos Contábeis das Contas de Resultado Recuperados da ECD | 2 | K001 | 1:N / [0;N] (divergente, não verificada) | F / FC | 160 |
| K990 | Encerramento do Bloco K | 1 | 0000 | 1:1 / [1;1] | F / O | 163 |
| L001 | Abertura do Bloco L | 1 | 0000 | 1:1 / [1;1] | F / O | 221 |
| L030 | Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL no Ano-Calendário | 2 | L001 | 0:13 / [0;13] | F / OC | 222 |
| L100 | Balanço Patrimonial | 3 | L030 | 0:N / [0;N] | F / OC | 225 |
| L200 | Método de Avaliação do Estoque Final | 3 | L030 | 0:13 / [0;N] (divergente, não verificada) | F / OC | 230 |
| L210 | Informativo da Composição de Custos | 3 | L030 | 0:N / [0;N] | F / OC | 231 |
| L300 | Demonstração do Resultado Líquido no Período Fiscal | 3 | L030 | 0:N / [0;N] | F / OC | 233 |
| L990 | Encerramento do Bloco L | 1 | 0000 | 1:1 / [1;1] | F / O | 236 |
| M001 | Abertura do Bloco M | 1 | 0000 | 1:1 / [1;1] | F / O | 237 |
| M010 | Identificação da Conta na Parte B do e-Lalur e do e-Lacs | 2 | M001 | 0:N / [0;N] | F / FC | 238 |
| M030 | Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas pelo Lucro Real | 2 | M001 | 0:13 / [0;13] | F / OC | 242 |
| M300 | Demonstração do Lucro Real – Lançamentos da Parte A do e-Lalur | 3 | M030 | 1:N / [0;N] (divergente, não verificada) | F / OC | 245 |
| M305 | Conta da Parte B do e-Lalur | 4 | M300 | 0:N / [0;N] | F / OC | 251 |
| M310 | Contas Contábeis Relacionadas ao Lançamento da Parte A do e-Lalur | 4 | M300 | 0:N / [0;N] | F / OC | 253 |
| M312 | Números dos Lançamentos Relacionados à Conta Contábil | 5 | M310 | 0:N / [0;N] | F / OC | 255 |
| M315 | Identificação de Processos Judiciais e Administrativos Referentes ao Lançamento | 4 | M300 | 0:N / [0;N] | F / F | 256 |
| M350 | Demonstração da Base de Cálculo da CSLL – Lançamentos da Parte A do e-Lacs | 3 | M030 | 1:N / [0;N] (divergente, não verificada) | F / OC | 257 |
| M355 | Conta da Parte B do e-Lacs | 4 | M350 | 0:N / [0;N] | F / OC | 263 |
| M360 | Contas Contábeis Relacionadas ao Lançamento da Parte A do e-Lacs | 4 | M350 | 0:N / [0;N] | F / OC | 265 |
| M362 | Números dos Lançamentos Relacionados à Conta Contábil | 5 | M360 | 0:N / [0;N] | F / F | 267 |
| M365 | Identificação de Processos Judiciais e Administrativos Referentes ao Lançamento | 4 | M350 | 0:N / [0;N] | F / F | 268 |
| M410 | Lançamento na Conta da Parte B do e-Lalur e do e-Lacs sem Reflexo na Parte A | 3 | M030 | 0:N / [0;N] | F / F | 269 |
| M415 | Identificação de Processos Judiciais e Administrativos Referentes ao Lançamento | 4 | M410 | 0:N / [0;N] | F / F | 271 |
| M500 | Controle de Saldos das Contas da Parte B do e-Lalur e do e-Lacs | 3 | M030 | 0:N / [0;N] | F / F | 272 |
| M510 | Controle de Saldos das Contas Padrão da Parte B do e-Lalur e do e-Lacs | 3 | M030 | 0:N / [0;N] | F / F | 274 |
| M990 | Encerramento do Bloco M | 1 | 0000 | 1:1 / [1;1] | N / O | 276 |
| N001 | Abertura do Bloco N | 1 | 0000 | 1:1 / [1;1] | F / O | 277 |
| N030 | Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas pelo Lucro Real | 2 | N001 | 0:13 / [0;13] | F / OC | 278 |
| N500 | Base de Cálculo do IRPJ Sobre o Lucro Real Após as Compensações de Prejuízos | 3 | N030 | 1:13 / [0;N] (divergente, não verificada) | F / OC | 281 |
| N600 | Demonstração do Lucro da Exploração | 3 | N030 | 0:N / [0;N] | F / OC | 284 |
| N605 | Contas Contábeis Utilizadas na Apuração do Lucro da Exploração | 3 | N030 | 0:N / [0;N] | F / OC | 286 |
| N610 | Cálculo da Isenção e Redução do Imposto Sobre o Lucro Real | 3 | N030 | 1:N / [0;N] (divergente, não verificada) | F / OC | 290 |
| N615 | Informações da Base de Cálculo dos Incentivos Fiscais | 3 | N030 | 1:1 / [0;N] (divergente, não verificada) | F / OC | 293 |
| N620 | Apuração do IRPJ Mensal por Estimativa | 3 | N030 | 0:N / [0;N] | F / OC | 297 |
| N630 | Apuração do IRPJ Com Base no Lucro Real | 3 | N030 | 0:N / [0;N] | F / OC | 299 |
| N650 | Base de Cálculo da CSLL Após as Compensações da Base de Cálculo Negativa | 3 | N030 | 1:13 / [0;N] (divergente, não verificada) | F / OC | 302 |
| N660 | Apuração da CSLL Mensal por Estimativa | 3 | N030 | 1:N / [0;N] (divergente, não verificada) | F / OC | 306 |
| N670 | Apuração da CSLL Com Base no Lucro Real | 3 | N030 | 0:N / [0;N] | F / OC | 308 |
| N990 | Encerramento do Bloco N | 1 | 0000 | 1:1 / [1;1] | F / O | 310 |
| P001 | Abertura do Bloco P | 1 | 0000 | 1:1 / [1;1] | F / O | 327 |
| P030 | Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas pelo Lucro Presumido | 2 | P001 | 0:5 / [0;4] (divergente, não verificada) | F / OC | 328 |
| P100 | Balanço Patrimonial | 3 | P030 | 0:N / [0;N] | F / OC | 330 |
| P130 | Demonstração das Receitas Incentivadas do Lucro Presumido | 3 | P030 | 0:N / [0;N] | F / OC | 334 |
| P150 | Demonstrativo do Resultado Líquido no Período Fiscal | 3 | P030 | 0:N / [0;N] | F / OC | 337 |
| P200 | Apuração da Base de Cálculo do IRPJ com Base no Lucro Presumido | 3 | P030 | 0:N / [1;N] (divergente, não verificada) | F / O | 340 |
| P230 | Cálculo da Isenção e Redução do Lucro Presumido | 3 | P030 | 0:N / [0;N] | F / OC | 342 |
| P300 | Cálculo do IRPJ com Base no Lucro Presumido | 3 | P030 | 0:N / [1;N] (divergente, não verificada) | F / O | 344 |
| P400 | Apuração da Base de Cálculo da CSLL com Base no Lucro Presumido | 3 | P030 | 0:N / [1;N] (divergente, não verificada) | F / O | 346 |
| P500 | Cálculo da CSLL com Base no Lucro Presumido | 3 | P030 | 0:N / [1;N] (divergente, não verificada) | F / O | 348 |
| P990 | Encerramento do Bloco P | 1 | 0000 | 1:1 / [1;1] | F / O | 350 |
| Q001 | Abertura do Bloco Q | 1 | 0000 | 1:1 / [1;1] | F / O | 351 |
| Q100 | Demonstrativo do Livro Caixa | 2 | Q001 | 0:N / [0;N] | F / FC | 352 |
| Q990 | Encerramento do Bloco Q | 1 | 0000 | 1:1 / [1;1] | F / O | 354 |
| S001 | Abertura do Bloco S | 1 | 0000 | 1:1 / [1:1] | F / O | 355 |
| S030 | Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas como TEF | 2 | S001 | 0:12 / [0:12] | F / OC | 357 |
| S100 | Balanço Patrimonial | 3 | S030 | 1:N / [0:12] (divergente, não verificada) | F / OC | 359 |
| S150 | Demonstrativo do Resultado Líquido no Período Fiscal | 3 | S030 | 0:N / [0:12] (divergente, não verificada) | F / OC | 362 |
| S200 | Apuração dos Tributos a Pagar Incidentes em TEF/SAF | 3 | S030 | 0:N / [0:12] (divergente, não verificada) | F / OC | 365 |
| S300 | Cálculo do IRPJ | 3 | S030 | 0:N / [0:12] (divergente, não verificada) | F / OC | 367 |
| S500 | Cálculo da CSLL | 3 | S030 | 0:N / [0:12] (divergente, não verificada) | F / OC | 369 |
| S700 | Cálculo dos Demais Tributos | 3 | S030 | 0:N / [0:12] (divergente, não verificada) | F / OC | 371 |
| T001 | Abertura do Bloco T | 1 | 0000 | 1:1 / [1;1] | F / O | 384 |
| T030 | Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas pelo Lucro Arbitrado | 2 | T001 | 0:4 / [0;4] | F / OC | 385 |
| T120 | Apuração da Base de Cálculo do IRPJ com Base no Lucro Arbitrado | 3 | T030 | 0:N / [0;N] | F / OC | 387 |
| T150 | Cálculo do IRPJ com Base no Lucro Arbitrado | 3 | T030 | 0:N / [0;N] | F / OC | 389 |
| T170 | Apuração da Base de Cálculo da CSLL com Base no Lucro Arbitrado | 3 | T030 | 0:N / [0;N] | F / OC | 391 |
| T181 | Cálculo da CSLL com Base no Lucro Arbitrado | 3 | T030 | 0:N / [0;N] | F / OC | 393 |
| T990 | Encerramento do Bloco T | 1 | 0000 | 1:1 / [1;1] | F / O | 395 |
| U001 | Abertura do Bloco U | 1 | 0000 | 1:1 / [1;1] | F / O | 401 |
| U030 | Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Imunes e Isentas | 2 | U001 | 0:13 / [0;4] (divergente, não verificada) | F / OC | 402 |
| U100 | Balanço Patrimonial | 3 | U030 | 0:N / [0;N] | F / OC | 404 |
| U150 | Demonstração do Resultado | 3 | U030 | 0:N / [0;N] | F / OC | 409 |
| U180 | Cálculo do IRPJ das Empresas Imunes e Isentas | 3 | U030 | 0:N / [0;N] | F / OC | 413 |
| U182 | Cálculo da Contribuição Social sobre o Lucro Líquido (CSLL) das Empresas Imunes e Isentas | 3 | U030 | 1:N / [0;N] (divergente, não verificada) | F / OC | 415 |
| U990 | Encerramento do Bloco U | 1 | 0000 | 1:1 / [1;1] | F / O | 417 |
| V001 | Abertura do Bloco V | 1 | 0000 | 1:1 / [1;1] | F / O | 420 |
| V010 | DEREX - Instituição | 2 | V001 | 1:N / [0;N] (divergente, não verificada) | F / OC | 421 |
| V020 | Responsável pela Movimentação | 3 | V010 | 1:N / [1;N] | F / O | 422 |
| V030 | DEREX – Período - Mês | 3 | V010 | 1:12 / [1;12] | F / O | 423 |
| V100 | Demonstrativo dos Recursos em Moeda Estrangeira Decorrentes do Recebimento de Exportações | 4 | V030 | 1:N / [1;N] | F / O | 424 |
| V990 | Encerramento do Bloco V | 1 | 0000 | 1:1 / [1;1] | F / O | 426 |
| W001 | Abertura do Bloco W | 1 | 0000 | 1:1 / [1;1] | F / O | 431 |
| W100 | Informações sobre o Grupo Multinacional e a Entidade Declarante – Declaração País-a-País | 2 | W001 | 0:1 / [0;1] | F / OC | 432 |
| W200 | Declaração País-a-País | 3 | W100 | 0:N / [0;N] Observação: Quando for obrigatório, deverá aparecer, no mínimo, duas vezes. | F / OC | 444 |
| W250 | Declaração País-a-País – Entidades Integrantes | 4 | W200 | 1:N / [0;N] (divergente, não verificada) | F / OC | 455 |
| W300 | Observações Adicionais – Declaração País-a-País | 2 | W001 | 0:N / [0;N] | F / F | 461 |
| W990 | Encerramento do Bloco W | 1 | 0000 | 1:1 / [1;1] | F / O | 464 |
| X001 | Abertura do Bloco X | 1 | 0000 | 1:1 / [1;1] | F / O | 465 |
| X280 | Atividades Incentivadas – PJ em Geral | 2 | X001 | 0:N / [0;N] | F / OC | 466 |
| X292 | Operações com o Exterior – Pessoa Não Vinculada/Não Interposta/País sem Tributação Favorecida | 2 | X001 | 0:N / [0;N] | F / OC | 477 |
| X340 | Identificação da Participação no Exterior | 2 | X001 | 0:N / [0;N] | F / OC | 479 |
| X350 | Participações no Exterior – Resultado do Período de Apuração | 3 | X340 | 0:1 / [0;N] (divergente, não verificada) |  / OC | 483 |
| X351 | Demonstrativo de Resultados e de Imposto Pago no Exterior | 3 | X340 | 0:1 / [0;N] (divergente, não verificada) | F / F | 487 |
| X352 | Demonstrativo de Resultados no Exterior Auferidos por Intermédio de Coligadas | 3 | X340 | 0:1 / [0;N] (divergente, não verificada) | F / OC | 491 |
| X353 | Demonstrativo de Consolidação | 3 | X340 | 0:1 / [0;N] (divergente, não verificada) | F / OC | 492 |
| X354 | Demonstrativo de Prejuízos Acumulados | 3 | X340 | 0:1 / [0;N] (divergente, não verificada) | F / F | 494 |
| X355 | Demonstrativo de Rendas Ativas e Passivas | 3 | X340 | 0:1 / [0;N] (divergente, não verificada) | F / OC | 496 |
| X356 | Demonstrativo de Estrutura Societária | 3 | X340 | 0:1 / [0;N] (divergente, não verificada) | F / OC | 498 |
| X357 | Investidoras Diretas | 3 | X340 | 0:N / [0;N] | F / FC | 499 |
| X360 | Informações Gerais Sobre Preços de Transferência | 2 | X001 | 0:N / [0;N] | F / OC | 501 |
| X365 | Informações Sobre as Contrapartes nas Transações Controladas | 2 | X001 | 0:N / [0;N] | F / OC | 503 |
| X366 | Entidades Com as Quais Realiza Transações Controladas | 3 | X365 | 0:N / [0;N] | F / OC | 504 |
| X370 | Informações Sobre as Transações Controladas | 2 | X001 | 0:N / [0;N] | F / OC | 506 |
| X371 | Informações Sobre Ajustes Compensatórios | 3 | X370 | 0:N / [0;N] | F / OC | 512 |
| X375 | Informações Relacionadas aos Métodos | 3 | X370 | 0:N / [0;N] | F / OC | 513 |
| X390 | Origem e Aplicação de Recursos – Imunes e Isentas | 2 | X001 | 0:N / [0;N] | F / OC | 516 |
| X400 | Comércio Eletrônico e Tecnologia da Informação – Informações das Vendas | 2 | X001 | 0:N / [0;N] | F / OC | 518 |
| X410 | Comércio Eletrônico – Informação de Homepage/Servidor | 2 | X001 | 0:N / [0;N] | F / OC | 521 |
| X420 | Royalties Recebidos ou Pagos a Beneficiários do Brasil e do Exterior | 2 | X001 | 0:N / [0;N] | F / OC | 523 |
| X430 | Rendimentos Relativos a Serviços, Juros e Dividendos Recebidos do Brasil e do Exterior | 2 | X001 | 0:N / [0;N] | F / OC | 529 |
| X450 | Pagamentos ou Remessas a Título de Serviços, Juros e Dividendos a Beneficiários do Brasil e do Exterior | 2 | X001 | 0:N / [0;N] | F / OC | 536 |
| X451 | Pagamentos ou Remessas a Título de Serviços, Juros e Dividendos a Beneficiários do Brasil e do Exterior – Demais Informações | 3 (nível divergente) | X450 | 0:N / [0;N] | F / OC | 538 |
| X460 | Inovação Tecnológica e Desenvolvimento Tecnológico | 2 | X001 | 0:N / [0;N] | F / OC | 541 |
| X470 | Capacitação de Informática e Inclusão Digital | 2 | X001 | 0:N / [0;N] | F / OC | 543 |
| X480 | Benefícios Fiscais – Parte I | 2 | X001 | 0:N / [0;N] | F / OC | 546 |
| X485 | Benefícios Fiscais – Parte II | 2 | X001 | 0:N / [0;N] | F / OC | 548 |
| X490 | Polo Industrial de Manaus e Amazônia Ocidental | 2 | X001 | 0:N / [0;N] | F / OC | 554 |
| X500 | Zonas de Processamento de Exportação (ZPE) | 2 | X001 | 0:N / [0;N] | F / OC | 556 |
| X510 | Áreas de Livre Comércio (ALC) | 2 | X001 | 0:N / [0;N] | F / OC | 558 |
| X990 | Encerramento do Bloco X | 1 | 0000 | 1:1 / [1;1] | F / O | 560 |
| Y001 | Abertura do Bloco Y | 1 | 0000 | 1:1 / [1;1] | F / O | 561 |
| Y520 | Pagamentos/Recebimentos do Exterior ou de Não Residentes | 2 | Y001 | 0:N / [0;N] | F / OC | 562 |
| Y570 | Demonstrativo do Imposto de Renda e CSLL Retidos na Fonte | 2 | Y001 | 0:N / [0;N] | F / F | 565 |
| Y590 | Ativos no Exterior | 2 | Y001 | 0:N / [0;N] | F / OC | 569 |
| Y600 | Identificação e Remuneração de Sócios, Titulares, Dirigentes e Conselheiros | 2 | Y001 | 0:N / [0;N] | F / OC | 571 |
| Y612 | Identificação e Rendimentos de Dirigentes e Conselheiros – Imunes ou Isentas | 2 | Y001 | 0:N / [0;N] | F / OC | 577 |
| Y620 | Participações Avaliadas Pelo Método de Equivalência Patrimonial | 2 | Y001 | 0:N / [0;N] | F / OC | 579 |
| Y630 | Fundos/Clubes de Investimento | 2 | Y001 | 0:N / [0;N] | F / OC | 583 |
| Y640 | Participações em Consórcios de Empresas | 2 | Y001 | 0:N / [0;N] | F / OC | 585 |
| Y650 | Participantes do Consórcio | 3 (nível divergente) | Y640 | 0:N / [0;N] | F / OC | 587 |
| Y660 | Dados de Sucessoras | 2 | Y001 | 0:N / [0;N] | F / OC | 588 |
| Y672 | Outras Informações (Lucro Presumido ou Lucro Arbitrado) | 2 | Y001 | 0:1 / [0;1] | F / OC | 590 |
| Y680 | Mês das Informações de Optantes pelo Refis (Lucro Real, Presumido e Arbitrado) | 2 | Y001 | 0:12 / [0;12] | F / OC | 595 |
| Y681 | Informações de Optantes pelo Refis (Lucros Real, Presumido e Arbitrado) | 3 | Y680 | 0:N / [0;12] (divergente, não verificada) | F / OC | 596 |
| Y682 | Informações de Optantes pelo Refis – Imunes ou Isentas | 2 | Y001 | 0:12 / [0;12] | F / OC | 598 |
| Y720 | Informações de Períodos Anteriores | 2 | Y001 | 0:1 / [0;1] | F / OC | 600 |
| Y730 | Identificação de donatários/destinatários de deduções do IRPJ/CSLL | 2 | Y001 | 0:N / [0;N] | F / OC | 603 |
| Y750 | Informações da ECF Calculadas pelo PGE | 2 | Y001 | 0:N / [0;N] | F / O | 607 |
| Y800 | Outras Informações | 2 | Y001 | 0:N / [0;N] | F / F | 611 |
| Y990 | Encerramento do Bloco Y | 1 | 0000 | 1:1 / [1;1] | F / O | 612 |
| 9001 | Abertura do Bloco 9 | 1 | 0000 | 1:1 / [1;1] | O / O | 613 |
| 9100 | Avisos da Escrituração | 2 | 9001 | 0:N / [1;N] (divergente, não verificada) | F / O | 614 |
| 9900 | Registros do Arquivo | 2 | 9001 | 0:N / [1;N] (divergente, não verificada) | O / O | 616 |
| 9990 | Encerramento do Bloco 9 | 1 | 0000 | 1:1 / [1;1] | O / O | 618 |
| 9999 | Encerramento do Arquivo Digital | 1 | 0000 | 1:1 / [1;1] | O / O | 619 |

## 0000: Abertura do Arquivo Digital e Identificação da Pessoa Jurídica

Página 63. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [0000] | Sim | tipo, até 4, obrigatório |
| 2 | NOME_ESC | C | 4 | - | [LECF] | Sim | tipo, até 4, valores, obrigatório |
| 3 | COD_VER | C | 4 | - | - | Sim | tipo, até 4, obrigatório |
| 4 | CNPJ | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 5 | NOME | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 6 | IND_SIT_INI_PER | N | 1 | - | [0; 1; 2; 3; 4;5;6;7;8] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |
| 7 | SIT_ESPECIAL | C | 1 | - | [0; 1; 2; 3; 4; 5; 6; 7; 8; 9] | Sim | tipo, até 1, valores, obrigatório |
| 8 | PAT_REMAN_CIS | N | 8 | 4 | [0 a 100] | Não | tipo, até 8, 4 decimais, valores |
| 9 | DT_SIT_ESP | N | 8 | - | - | Não | tipo, data, até 8, 0 decimais |
| 10 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 11 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 12 | RETIFICADORA | C | 1 | - | [S; N; F] | Sim | tipo, até 1, valores, obrigatório |
| 13 | NUM_REC | C | 40 | - | - | Não | tipo, até 40 |
| 14 | TIP_ECF | N | 1 | - | [0; 1; 2] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |
| 15 | COD_SCP | C | 14 | - | - | Não | tipo, até 14 |
| 16 | EVENTO_SCP | N | 1 | - | [0;1] | Não | tipo, até 1, 0 decimais, valores |

## 0001: Abertura do Bloco 0

Página 70. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [0001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## 0010: Parâmetros de Tributação

Página 71. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [0010] | Sim | tipo, até 4, obrigatório |
| 2 | HASH_ECF_ANTERIOR | C | 40 | - | - | Não | tipo, até 40 |
| 3 | OPT_REFIS | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 4 | FORMA_TRIB | N | 1 | - | [1;2;3;4;5;6;7;8;9;10] | Sim | tipo, até 2, 0 decimais, valores, obrigatório |
| 5 | FORMA_APUR | C | 1 | - | [T;A] | Não | tipo, até 1, valores |
| 6 | COD_QUALIF_PJ | C | 2 | - | [01;02;03;04] | Não | tipo, até 2, valores |
| 7 | FORMA_TRIB_PER | C | 4 | - | [0;R;P;A;E;S] | Não | tipo, exatamente 4, valores por posição |
| 8 | MES_BAL_RED | C | 12 | - | [0;E;B] | Não | tipo, exatamente 12, valores por posição |
| 9 | TIP_ESC_PRE | C | 1 | - | [L;C] | Não | tipo, até 1, valores |
| 10 | TIP_ENT | N | 2 | - | [01;02;03;04;05;06;07;08;09;10;11;12;13;14;15;99] | Não | tipo, até 2, 0 decimais, valores |
| 11 | FORMA_APUR_I | C | 1 | - | [A;T;D] | Não | tipo, até 1, valores |
| 12 | APUR_CSLL | C | 1 | - | [A;T;D] | Não | tipo, até 1, valores |
| 13 | IND_REC_RECEITA | N | 1 | - | [1;2] | Não | tipo, até 1, 0 decimais, valores |

## 0020: Parâmetros Complementares

Página 81. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [0020] | Sim | tipo, até 4, obrigatório |
| 2 | IND_ALIQ_CSLL | N | 1 | - | [1;3;4] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |
| 3 | IND_QTE_SCP | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 4 | IND_ADM_FUN_CLU | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 5 | IND_PART_CONS | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 6 | IND_OP_EXT | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 7 | IND_OP_VINC | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 8 | IND_PJ_ENQUAD | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 9 | IND_PART_EXT | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 10 | IND_ATIV_RURAL | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 11 | IND_LUC_EXP | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 12 | IND_RED_ISEN | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 13 | IND_FIN | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 14 | IND_PART_COLIG | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 15 | IND_ REC_EXT | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 16 | IND_ATIV_EXT | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 17 | IND_PGTO_EXT | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 18 | IND_E-COM_TI | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 19 | IND_ROY_REC | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 20 | IND_ROY_PAG | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 21 | IND_REND_SERV | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 22 | IND_PGTO_REM | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 23 | IND_INOV_TEC | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 24 | IND_CAP_INF | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 25 | IND_PJ_HAB | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 26 | IND_POLO_AM | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 27 | IND_ZON_EXP | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 28 | IND_AREA_COM | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 29 | IND_PAIS_A_PAIS | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 30 | IND_DEREX | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 31 | POSSUI_CEBRAS | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 32 | CEBAS | C | 255 | - |  | OC Deve ser preenchi | tipo, até 255 |

## 0021: Parâmetros de Identificação dos Tipos de Programa

Página 97. Chave: REG. Regras do registro: REGRA_0021_PREENCHIDO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [0021] | Sim | tipo, até 4, obrigatório |
| 2 | IND_REPES | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 3 | IND_RECAP | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 4 | IND_PADIS | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 5 | IND_REIDI | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 6 | IND_RECINE | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 7 | IND_RETID | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 8 | IND_OLEO_BUNKER | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 9 | IND_REPORTO | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 10 | IND_RET_II | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 11 | IND_RET_PMCMV | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 12 | IND_RET_EEI | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 13 | IND_EBAS | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 14 | IND_REPETRO_ INDUSTRIALIZACAO | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 15 | IND_REPETRO_ NACIONAL | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 16 | IND_REPETRO_ PERMANENTE | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 17 | IND_REPETRO_ TEMPORARIO | C | 1 | - | [S;N] | Não | tipo, até 1, valores |
| 18 | IND_REDATA | C | 1 | - | [S;N] | Não | tipo, até 1, valores |

## 0030: Dados Cadastrais

Página 101. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [0030] | Sim | tipo, até 4, obrigatório |
| 2 | COD_NAT | N | 4 | - | Código da Natureza Jurídica | Sim | tipo, até 4, 0 decimais, obrigatório |
| 3 | CNAE_FISCAL | N | 7 | - | CNAE Fiscal | Sim | tipo, até 7, 0 decimais, obrigatório |
| 4 | ENDERECO | C | 150 | - | - | Sim | tipo, até 150, obrigatório |
| 5 | NUM | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 6 | COMPL | C | 50 | - | - | Não | tipo, até 50 |
| 7 | BAIRRO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |
| 8 | UF | C | 2 | - | Sigla da UF | Sim | tipo, até 2, obrigatório |
| 9 | COD_MUN | C | 7 | - | Código do Município | Sim | tipo, até 7, obrigatório |
| 10 | CEP | C | 8 | - | - | Sim | tipo, até 8, obrigatório |
| 11 | NUM_TEL | N | 15 | - | - | Não | tipo, até 15, 0 decimais |
| 12 | EMAIL | C | 115 | - | - | Sim | tipo, até 115, obrigatório |

## 0035: Identificação das SCP

Página 103. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [0035] | Sim | tipo, até 4, obrigatório |
| 2 | COD_SCP | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 3 | NOME_SCP | C | - | - | - | Não | tipo, até 255 |

## 0930: Identificação dos Signatários da ECF

Página 105. Chave: REG. Regras do registro: REGRA_OBRIGATORIO_ASSIN_CONTADOR, REGRA_OCORRENCIA_0930, REGRA_OCORRENCIA_0930_1, REGRA_OCORRENCIA_0930_2.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [0930] | Sim | tipo, até 4, obrigatório |
| 2 | IDENT_NOM | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | IDENT_CPF_CNPJ | C | CPF (11) CNPJ(14) | - | - | Sim | tipo, obrigatório |
| 4 | IDENT_QUALIF | C | 3 | - | - | Sim | tipo, até 3, obrigatório |
| 5 | IND_CRC | C | - | - | - | Não | tipo, até 255 |
| 6 | EMAIL | C | 60 | - | - | Sim | tipo, até 60, obrigatório |
| 7 | FONE | C | 14 | - | - | Sim | tipo, até 14, obrigatório |

## 0990: Encerramento do Bloco 0

Página 108. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [0990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## C001: Abertura do Bloco C

Página 109. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## C040: Identificador da ECD

Página 110. Chave: HASH_ECD. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C040] | Sim | tipo, até 4, obrigatório |
| 2 | HASH_ECD | C | 40 | - | - | Sim | tipo, até 40, obrigatório |
| 3 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 5 | IND_SIT_ESP | N | 1 | - | - | Não | tipo, até 1, 0 decimais |
| 6 | CNPJ | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 7 | NUM_ORD | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |
| 8 | NIRE | N | 11 | - | - | Não | tipo, até 11, 0 decimais |
| 9 | NAT_LIVR | C | 80 | - | - | Sim | tipo, até 80, obrigatório |
| 10 | COD_VER_LC | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 11 | IND_ESC | C | 1 | - | [G; R; B] | Sim | tipo, até 1, valores, obrigatório |
| 12 | IDENT_MF | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 13 | IND_ESC_CONS | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 14 | IND_CENTRALIZADA | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |
| 15 | IND_MUDANC_PC | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |
| 16 | COD_PLAN_REF | C | 2 | - | [1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11] ou Vazio | Não | tipo, até 2 |

## C050: Plano de Contas da ECD

Página 112. Chave: DT_ALT, COD_CTA. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C050] | Sim | tipo, até 4, obrigatório |
| 2 | DT_ALT | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | COD_NAT | C | 2 | - | - | Sim | tipo, até 2, obrigatório |
| 4 | IND_CTA | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NÍVEL | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |
| 6 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |

## C051: Plano de Contas Referencial

Página 113. Chave: COD_CCUS, COD_CTA_REF. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C051] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 3 | COD_CTA_REF | C | - | - | - | Sim | tipo, até 255, obrigatório |

## C053: Subcontas Correlatas

Página 114. Chave: COD_CNT_CORR. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C053] | Sim | tipo, até 4, obrigatório |
| 2 | COD_IDT | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | COD_CNT_CORR | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | NAT_SUB_CNT | C | 2 | - | - | Sim | tipo, até 2, obrigatório |

## C100: Centro de Custos

Página 115. Chave: DT_ALT, COD_CCUS. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C100] | Sim | tipo, até 4, obrigatório |
| 2 | DT_ALT | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | CCUS | C | - | - | - | Sim | tipo, até 255, obrigatório |

## C150: Identificação do Período dos Saldos Periódicos das Contas

Página 116. Chave: DT_INI, DT_FIN. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C150] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |

## C155: Detalhes dos Saldos Contábeis das Contas

Página 117. Chave: COD_CTA, COD_CCUS. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C155] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 4 | VL_SLD_INI | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VL_SLD_INI | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 6 | VL_DEB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | VL_CRED | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | VL_SLD_FIN | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_VL_SLD_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 10 | LINHA_ECD | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## C157: Transferência de Saldos do Plano de Contas Anterior

Página 118. Chave: COD_CTA, COD_CCUS. Regras do registro: REGRA_C157_OBRIGATORIO, REGRA_CONTA_SUBSTITUTA_SALDO_ZERO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C157] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 4 | VL_SLD_FIN | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VL_SLD_FIN | C | 1 | - | [D; C] | Não | tipo, até 1, valores |
| 6 | LINHA_ECD | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## C350: Identificação da Data dos Saldos das Contas de Resultado Antes do Encerramento

Página 119. Chave: DT_RES. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C350] | Sim | tipo, até 4, obrigatório |
| 2 | DT_RES | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |

## C355: Detalhes dos Saldos das Contas de Resultado Antes do Encerramento

Página 120. Chave: COD_CTA, COD_CCUS. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C355] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 4 | VL_CTA | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VL_CTA | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 6 | LINHA_ECD | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## C990: Encerramento do Bloco C

Página 121. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [C990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## E001: Abertura do Bloco E

Página 122. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [E001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## E010: Saldos Finais Recuperados da ECF Anterior

Página 123. Chave: COD_CTA_REF. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [E010] | Sim | tipo, até 4, obrigatório |
| 2 | COD_NAT | C | 4 | - | - | Sim | tipo, até 4, obrigatório |
| 3 | COD_CTA_REF | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | DESC_CTA_REF | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 5 | VAL_CTA_REF | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | IND_VAL_CTA_REF | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## E015: Contas Contábeis Mapeadas

Página 124. Chave: COD_CTA, COD_CCUS. Regras do registro: REGRA_COMPATIBILIDADE_C155_E015, REGRA_COMPATIBILIDADE_C157_E015, REGRA_COMPATIBILIDADE_E015_C155_C157, REGRA_EXISTENCIA_K155_E015, REGRA_EXISTENCIA_MAPEAMENTO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [E015] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 4 | DESC_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 5 | VAL_CTA | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | IND_VAL_CTA | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## E020: Saldos Finais das Contas na Parte B do e-Lalur da ECF Imediatamente Anterior

Página 126. Chave: COD_CTA_B, TRIBUTO. Regras do registro: REGRA_EXISTENCIA_M010.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [E020] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA_B | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESC_CTA_LAL | C | - | - | - | Não | tipo, até 255 |
| 4 | DT_AP_LAL | C | 8 | - | - | Não | tipo, data, até 8 |
| 5 | DT_LIM_LAL | C | 8 | - | - | Não | tipo, data, até 8 |
| 6 | TRIBUTO | C | 1 | - | [I; C; A] | Não | tipo, até 1, valores |
| 7 | VL_SALDO_FIN | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 8 | IND_VL_SALDO_FIN | C | 1 |  | [D; C] | Não | tipo, até 1, valores |
| 9 | COD_PB_RFB | C | 6 | - | - | Sim | tipo, até 6, obrigatório |

## E030: Identificação do Período

Página 127. Chave: PER_APUR. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [E030] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | PER_APUR | C | 3 | - | [A00; A01; A02; A03; A04; A05; A06; A07; A08; A09; A10; A11; A12; T01; T02; T03; | Sim | tipo, até 3, valores, obrigatório |

## E155: Detalhes dos Saldos Contábeis Calculados com Base nas ECD

Página 129. Chave: COD_CTA, COD_CCUS. Regras do registro: REGRA_CONTA_FORA_ULTIMO_PLANO_DE_CONTAS.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [E155] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 4 | VL_SLD_INI | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VL_SLD_INI | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 6 | VL_DEB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | VL_CRED | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | VL_SLD_FIN | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_VL_SLD_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## E355: Detalhes dos Saldos das Contas de Resultado Antes do Encerramento

Página 130. Chave: COD_CTA, COD_CCUS. Regras do registro: REGRA_CONTA_FORA_ULTIMO_PLANO_DE_CONTAS.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [E355] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 4 | VL_SLD_FIN | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VL_SLD_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## E990: Encerramento do Bloco E

Página 131. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [E990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## J001: Abertura do Bloco J

Página 132. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [J001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## J050: Plano de Contas do Contribuinte

Página 133. Chave: DT_ALT, COD_CTA. Regras do registro: REGRA_REGISTRO_OBRIGATORIO_J051.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [J050] | Sim | tipo, até 4, obrigatório |
| 2 | DT_ALT | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | COD_NAT | C | 2 | - | [01; 02; 03; 04; 05; 09] | Sim | tipo, até 2, valores, obrigatório |
| 4 | IND_CTA | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NÍVEL | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |
| 6 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |

## J051: Plano de Contas Referencial

Página 136. Chave: COD_CTA_REF, COD_CCUS. Regras do registro: REGRA_CCUS_UNICO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [J051] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 3 | COD_CTA_REF | C | - | - | - | Sim | tipo, até 255, obrigatório |

## J053: Subcontas Correlatas

Página 138. Chave: COD_CNT_CORR. Regras do registro: REGRA_COD_CTA_DUPLICIDADE, REGRA_REGISTRO_PARA_CONTA_ANALITICA, REGRA_SUB_CONTA_PAI.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [J053] | Sim | tipo, até 4, obrigatório |
| 2 | COD_IDT | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | COD_CNT_CORR | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | NAT_SUB_CNT | C | 2 | - | - | Sim | tipo, até 2, obrigatório |

## J100: Centro de Custos

Página 141. Chave: DT_ALT, COD_CCUS. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [J100] | Sim | tipo, até 4, obrigatório |
| 2 | DT_ALT | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | CCUS | C | - | - | - | Sim | tipo, até 255, obrigatório |

## J990: Encerramento do Bloco J

Página 142. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [J990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## K001: Abertura do Bloco K

Página 143. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [K001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## K030: Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL no Ano-Calendário

Página 144. Chave: PER_APUR. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ALTERADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [K030] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | PER_APUR | C | 3 | - | [A00; A01; A02; A03; A04; A05; A06; A07; A08; A09; A10; A11; A12; T01; T02; T03; | Sim | tipo, até 3, valores, obrigatório |

## K155: Detalhes dos Saldos Contábeis (Depois do Encerramento do Resultado do Período)

Página 147. Chave: COD_CTA, COD_CCUS. Regras do registro: REGRA_BATIMENTO_K155, REGRA_COMPATIBILIDADE_E155_K155, REGRA_COMPATIBILIDADE_K155_E155, REGRA_CONTA_ANALITICA, REGRA_EXISTENCIA_E015_K155, REGRA_NATUREZA_PERMITIDA_PATRIMONIAL, REGRA_SALDO_CONTABIL_MENOR, REGRA_VALIDACAO_DEB_DIF_CRED, REGRA_VALIDACAO_SOMA_SALDO_FINAL, REGRA_VALIDACAO_SOMA_SALDO_INICIAL.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [K155] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 4 | VL_SLD_INI | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VL_SLD_INI | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 6 | VL_DEB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | VL_CRED | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | VL_SLD_FIN | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_VL_SLD_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## K156: Mapeamento Referencial do Saldo

Página 150. Chave: COD_CTA_REF. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [K156] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA_REF | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | VL_SLD_INI | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | IND_VL_SLD_INI | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 5 | VL_DEB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | VL_CRED | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | VL_SLD_FIN | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | IND_VL_SLD_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## K355: Saldos Finais das Contas Contábeis de Resultado Antes do Encerramento

Página 152. Chave: COD_CTA, COD_CCUS. Regras do registro: REGRA_COMPATIBILIDADE_E355_K355, REGRA_COMPATIBILIDADE_K355_E355, REGRA_CONTA_ANALITICA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [K355] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 4 | VL_SLD_FIN | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VL_SLD_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## K356: Mapeamento Referencial dos Saldos Finais das Contas Contábeis de Resultado Antes do Encerramento

Página 154. Chave: COD_CTA_REF. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [K356] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA_REF | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | VL_SLD_FIN | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | IND_VL_SLD_FIN | C | 1 | - | - | Sim | tipo, até 1, obrigatório |

## K915: Justificativa para Divergência de Saldos Contábeis das Contas Patrimoniais Recuperados da ECD

Página 156. Chave: COD_CTA, COD_CCUS, ID_REGRA. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [K915] | Sim | tipo, até 4, obrigatório |
| 2 | PER_APUR | C | 3 | - | [A00; A01; A02; A03; A04; A05; A06; A07; A08; A09; A10; A11; A12; T01; T02; T03; | Sim | tipo, até 3, valores, obrigatório |
| 3 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 5 | ID_REGRA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 6 | VL_SLD_INI_ESP | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 7 | IND_VL_SLD_INI_ESP | C | 1 | - | [“D”; “C”] | N | tipo, até 1, valores |
| 8 | VL_DEB_ESP | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 9 | VL_CRED_ESP | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 10 | VL_SLD_FIN_ESP | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 11 | IND_VL_SLD_FIN_ESP | C | 1 | - | [“D”; “C”] | N | tipo, até 1, valores |
| 12 | SLD_INI_PRE | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 13 | IND_SLD_INI_PRE | C | 1 | - | [“D”; “C”] | N | tipo, até 1, valores |
| 14 | VL_DEB_PRE | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 15 | VL_CRED_PRE | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 16 | SLD_FIN_PRE | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 17 | IND_SLD_FIN_PRE | C | 1 | - | [“D”; “C”] | N | tipo, até 1, valores |
| 18 | JUSTIFICATIVA | C | 500 | - | - | S | tipo, até 500, obrigatório |

## K935: Justificativa para Divergência de Saldos Contábeis das Contas de Resultado Recuperados da ECD

Página 160. Chave: COD_CTA, COD_CCUS, ID_REGRA. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [K935] | Sim | tipo, até 4, obrigatório |
| 2 | PER_APUR | C | 3 | - | [A00; A01; A02; A03; A04; A05; A06; A07; A08; A09; A10; A11; A12; T01; T02; T03; | Sim | tipo, até 3, valores, obrigatório |
| 3 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 5 | ID_REGRA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 6 | VL_SLD_FIN_ESP | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 7 | IND_VL_SLD_FIN_ESP | C | 1 | - | [“D”; “C”] | N | tipo, até 1, valores |
| 8 | SLD_FIN_PRE | N | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 9 | IND_SLD_FIN_PRE | C | 1 | - | [“D”; “C”] | N | tipo, até 1, valores |
| 10 | JUSTIFICATIVA | C | 500 | - | - | S | tipo, até 500, obrigatório |

## K990: Encerramento do Bloco K

Página 163. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [K990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## L001: Abertura do Bloco L

Página 221. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [L001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## L030: Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL no Ano-Calendário

Página 222. Chave: PER_APUR. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ALTERADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [L030] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | PER_APUR | C | 3 | - | [A00; A01; A02; A03; A04; A05; A06; A07; A08; A09; A10; A11; A12; T01; T02; T03; | Sim | tipo, até 3, valores, obrigatório |

## L100: Balanço Patrimonial

Página 225. Chave: CODIGO. Regras do registro: REGRA_BALANCO_SALDO_FINAL, REGRA_BALANCO_SALDO_INICIAL, REGRA_COMPARA_CONT_REF_SF, REGRA_COMPARA_CONT_REF_SI, REGRA_COMPATIBILIDADE_L100_E010, REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [L100] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | TIPO | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NIVEL | N | 3 | - | - | Não | tipo, até 3, 0 decimais |
| 6 | COD_NAT | C | 2 | - | - | Não | tipo, até 2 |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | VAL_CTA_REF_INI | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_VAL_CTA_REF_INI | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 10 | VAL_CTA_REF_DEB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 11 | VAL_CTA_REF_CRED | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 12 | VAL_CTA_REF_FIN | C | 19 | 2 | - | Sim | tipo, até 19, obrigatório |
| 13 | IND_ VAL_CTA_REF_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## L200: Método de Avaliação do Estoque Final

Página 230. Chave: —. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [L200] | Sim | tipo, até 4, obrigatório |
| 2 | IND_AVAL_ESTOQ | C | 1 | - | [1; 2; 3; 4; 5; 6; 7; 8] | Sim | tipo, até 1, valores, obrigatório |

## L210: Informativo da Composição de Custos

Página 231. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [L210] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - |  | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - |  | Não | tipo, até 255 |
| 4 | VALOR | C | - | - |  | Não | tipo, até 255 |

## L300: Demonstração do Resultado Líquido no Período Fiscal

Página 233. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [L300] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | TIPO | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NIVEL | N | 3 | - | - | Não | tipo, até 3, 0 decimais |
| 6 | COD_NAT | C | 2 | - | [04] | Não | tipo, até 2, valores |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | VALOR | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_ VALOR | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## L990: Encerramento do Bloco L

Página 236. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [L990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## M001: Abertura do Bloco M

Página 237. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## M010: Identificação da Conta na Parte B do e-Lalur e do e-Lacs

Página 238. Chave: COD_CTA_B, COD_TRIBUTO. Regras do registro: REGRA_DIVERGENCIA_E020_M010, REGRA_EXISTENCIA_M010, REGRA_SALDOS_M010_E020.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M010] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA_B | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESC_CTA_LAL | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | DT_AP_LAL | C | 8 | - | - | Sim | tipo, data, até 8, obrigatório |
| 5 | COD_PB_RFB | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 6 | DT_LIM_LAL | C | 8 | - | - | Não | tipo, data, até 8 |
| 7 | COD_TRIBUTO | C | 1 | - | [I; C] | Sim | tipo, até 1, valores, obrigatório |
| 8 | VL_SALDO_INI | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_VL_SALDO_INI | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 10 | CNPJ_SIT_ESP | C | 14 | - | - | Não | tipo, até 14 |

## M030: Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas pelo Lucro Real

Página 242. Chave: PER_APUR. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ALTERADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M030] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | PER_APUR | C | 3 | - | [A00; A01; A02; A03; A04; A05; A06; A07; A08; A09; A10; A11; A12; T01; T02; T03; | Sim | tipo, até 3, valores, obrigatório |

## M300: Demonstração do Lucro Real – Lançamentos da Parte A do e-Lalur

Página 245. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA, REGRA_VALOR_DETALHADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M300] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - |  | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - |  | Não | tipo, até 255 |
| 4 | TIPO_LANCAMENTO | C | 1 | - | [A; E; P; L] | Não | tipo, até 1, valores |
| 5 | IND_RELACAO | N | 1 | - | [1; 2; 3; 4] | Não | tipo, até 1, 0 decimais, valores |
| 6 | VALOR | NS | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 7 | HIST_LAN_LAL | C | 500 | - | - | Não | tipo, até 500 |

## M305: Conta da Parte B do e-Lalur

Página 251. Chave: COD_CTA_B. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M305] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA_B | C | - | - | [M010.COD_CTA_B] | Sim | tipo, até 255, valores, obrigatório |
| 3 | VL_CTA | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | IND_ VL_CTA | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## M310: Contas Contábeis Relacionadas ao Lançamento da Parte A do e-Lalur

Página 253. Chave: COD_CTA, COD_CCUS. Regras do registro: REGRA_REGISTRO_M312_OBRIGATORIO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M310] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | [J050.COD_CTA] | Sim | tipo, até 255, valores, obrigatório |
| 3 | COD_CCUS | C | - | - | [J100_COD_CCUS] | Não | tipo, até 255, valores |
| 4 | VL_CTA | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VL_CTA | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## M312: Números dos Lançamentos Relacionados à Conta Contábil

Página 255. Chave: NUM_LCTO. Regras do registro: REGRA_FINANCEIRAS_NAO_OBRIGATORIO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M312] | Sim | tipo, até 4, obrigatório |
| 2 | NUM_LCTO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |

## M315: Identificação de Processos Judiciais e Administrativos Referentes ao Lançamento

Página 256. Chave: IND_PROC, NUM_PROC. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M315] | Sim | tipo, até 4, obrigatório |
| 2 | IND_PROC | C | 1 | - | [1; 2] | Sim | tipo, até 1, valores, obrigatório |
| 3 | NUM_PROC | C | 20 | - | - | Sim | tipo, até 20, obrigatório |

## M350: Demonstração da Base de Cálculo da CSLL – Lançamentos da Parte A do e-Lacs

Página 257. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA, REGRA_VALOR_DETALHADO, REGRA_VALOR_DETALHADO_CSLL.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M350] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - |  | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - |  | Não | tipo, até 255 |
| 4 | TIPO_LANCAMENTO | C | 1 | - | [A; E; P; L] | Não | tipo, até 1, valores |
| 5 | IND_RELACAO | N | 1 | - | [1; 2; 3; 4] | Não | tipo, até 1, 0 decimais, valores |
| 6 | VALOR | NS | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 7 | HIST_LAN_LAL | C | 500 | - | - | Não | tipo, até 500 |

## M355: Conta da Parte B do e-Lacs

Página 263. Chave: COD_CTA_B. Regras do registro: REGRA_SALDO_DISPONIVEL_PARTE_B.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M355] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA_B | C | - | - | [M010.COD_CTA_B] | Sim | tipo, até 255, valores, obrigatório |
| 3 | VL_CTA | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | IND_ VL_CTA | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## M360: Contas Contábeis Relacionadas ao Lançamento da Parte A do e-Lacs

Página 265. Chave: COD_CTA, COD_CCUS. Regras do registro: REGRA_REGISTRO_M362_OBRIGATORIO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M360] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | [J050.COD_CTA] | Sim | tipo, até 255, valores, obrigatório |
| 3 | COD_CCUS | C | - | - | [J100_COD_CCUS] | Não | tipo, até 255, valores |
| 4 | VL_CTA | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VL_CTA | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## M362: Números dos Lançamentos Relacionados à Conta Contábil

Página 267. Chave: NUM_LCTO. Regras do registro: REGRA_FINANCEIRAS_NAO_OBRIGATORIO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M362] | Sim | tipo, até 4, obrigatório |
| 2 | NUM_LCTO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |

## M365: Identificação de Processos Judiciais e Administrativos Referentes ao Lançamento

Página 268. Chave: IND_PROC, NUM_PROC. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M365] | Sim | tipo, até 4, obrigatório |
| 2 | IND_PROC | C | 1 | - | [1; 2] | Sim | tipo, até 1, valores, obrigatório |
| 3 | NUM_PROC | C | 20 | - | - | Sim | tipo, até 20, obrigatório |

## M410: Lançamento na Conta da Parte B do e-Lalur e do e-Lacs sem Reflexo na Parte A

Página 269. Chave: —. Regras do registro: REGRA_BC_NEGATIVA, REGRA_PREJUIZO_FISCAL.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M410] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA_B | C | - | - | - | Não | tipo, até 255 |
| 3 | COD_TRIBUTO | C | 1 | - | [I; C] | Sim | tipo, até 1, valores, obrigatório |
| 4 | VAL_LAN_LALB_PB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VAL_LAN_LALB_PB | C | - | - | [CR; DB; PF; BC] | Sim | tipo, até 255, valores, obrigatório |
| 6 | COD_CTA_B_CTP | C | - | - | - | Não | tipo, até 255 |
| 7 | HIST_LAN_LALB | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 8 | IND_LAN_ANT | C | 1 |  | [S; N] | Sim | tipo, até 1, valores, obrigatório |

## M415: Identificação de Processos Judiciais e Administrativos Referentes ao Lançamento

Página 271. Chave: IND_PROC, NUM_PROC. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M415] | Sim | tipo, até 4, obrigatório |
| 2 | IND_PROC | C | 1 | - | [1; 2] | Sim | tipo, até 1, valores, obrigatório |
| 3 | NUM_PROC | C | 20 | - | - | Sim | tipo, até 20, obrigatório |

## M500: Controle de Saldos das Contas da Parte B do e-Lalur e do e-Lacs

Página 272. Chave: COD_CTA_B, COD_TRIBUTO. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M500] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA_B | C | - | - | [M010.COD_CTA_B] | Sim | tipo, até 255, valores, obrigatório |
| 3 | COD_TRIBUTO | C | 1 | - | [I; C] | Sim | tipo, até 1, valores, obrigatório |
| 4 | SD_INI_LAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_ SD_INI_LAL | C | 1 | - | [C; D] | Sim | tipo, até 1, valores, obrigatório |
| 6 | VL_LCTO_PARTE_A | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | IND_ VL_LCTO_PARTE_A | C | 1 | - | [C; D] | Sim | tipo, até 1, valores, obrigatório |
| 8 | VL_LCTO_PARTE B | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_ VL_LCTO_PARTE B | C | 1 | - | [C; D] | Sim | tipo, até 1, valores, obrigatório |
| 10 | SD_FIM_LAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 11 | IND_ SD_FIM_LAL | C | 1 | - | [C; D] | Sim | tipo, até 1, valores, obrigatório |

## M510: Controle de Saldos das Contas Padrão da Parte B do e-Lalur e do e-Lacs

Página 274. Chave: COD_CTA_B, COD_TRIBUTO. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M510] | Sim | tipo, até 4, obrigatório |
| 2 | COD_PB_RFB | C | - | - | [M010.COD_CTA_B] | Sim | tipo, até 255, valores, obrigatório |
| 3 | DESCRICAO_PB_RFB | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | COD_TRIBUTO | C | 1 | - | [I; C] | Sim | tipo, até 1, valores, obrigatório |
| 5 | SD_INI_LAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | IND_ SD_INI_LAL | C | 1 | - | [C; D] | Sim | tipo, até 1, valores, obrigatório |
| 7 | VL_LCTO_PARTE_A | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | IND_ VL_LCTO_PARTE_A | C | 1 | - | [C; D] | Sim | tipo, até 1, valores, obrigatório |
| 9 | VL_LCTO_PARTE B | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 10 | IND_ VL_LCTO_PARTE B | C | 1 | - | [C; D] | Sim | tipo, até 1, valores, obrigatório |
| 11 | SD_FIM_LAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 12 | IND_ SD_FIM_LAL | C | 1 | - | [C; D] | Sim | tipo, até 1, valores, obrigatório |

## M990: Encerramento do Bloco M

Página 276. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [M990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## N001: Abertura do Bloco N

Página 277. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## N030: Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas pelo Lucro Real

Página 278. Chave: PER_APUR. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ALTERADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N030] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | PER_APUR | C | 3 | - | [A00; A01; A02; A03; A04; A05; A06; A07; A08; A09; A10; A11; A12; T01; T02; T03; | Sim | tipo, até 3, valores, obrigatório |

## N500: Base de Cálculo do IRPJ Sobre o Lucro Real Após as Compensações de Prejuízos

Página 281. Chave: REG. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N500] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## N600: Demonstração do Lucro da Exploração

Página 284. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N600] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## N605: Contas Contábeis Utilizadas na Apuração do Lucro da Exploração

Página 286. Chave: COD_CTA, COD_CCUS. Regras do registro: REGRA_SOMATORIO_N605_K355.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N605] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | N | 019 | 002 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VALOR | C | - | - | [D;C] | Sim | tipo, até 255, valores, obrigatório |

## N610: Cálculo da Isenção e Redução do Imposto Sobre o Lucro Real

Página 290. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N610] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## N615: Informações da Base de Cálculo dos Incentivos Fiscais

Página 293. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N615] | Sim | tipo, até 4, obrigatório |
| 2 | BASE_CALC | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 3 | PER_INCEN_ FINOR | N | 8 | 4 | - | Sim | tipo, até 8, 4 decimais, obrigatório |
| 4 | VL_LIQ_INCEN_ FINOR | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | PER_INCEN_ FINAM | N | 8 | 4 | - | Sim | tipo, até 8, 4 decimais, obrigatório |
| 6 | VL_LIQ_INCEN_ FINAM | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | VL_TOTAL | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## N620: Apuração do IRPJ Mensal por Estimativa

Página 297. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N620] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## N630: Apuração do IRPJ Com Base no Lucro Real

Página 299. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N630] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## N650: Base de Cálculo da CSLL Após as Compensações da Base de Cálculo Negativa

Página 302. Chave: REG. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N650] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | NS | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## N660: Apuração da CSLL Mensal por Estimativa

Página 306. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N660] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | NS | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## N670: Apuração da CSLL Com Base no Lucro Real

Página 308. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N670] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | NS | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## N990: Encerramento do Bloco N

Página 310. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [N990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## P001: Abertura do Bloco P

Página 327. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## P030: Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas pelo Lucro Presumido

Página 328. Chave: PER_APUR. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ALTERADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P030] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | PER_APUR | C | 3 | - | [A00; T01; T02; T03; T04] | Sim | tipo, até 3, valores, obrigatório |

## P100: Balanço Patrimonial

Página 330. Chave: CODIGO. Regras do registro: REGRA_BALANCO_SALDO_FINAL, REGRA_BALANCO_SALDO_INICIAL, REGRA_COMPARA_CONT_REF_SF, REGRA_COMPARA_CONT_REF_SI, REGRA_COMPATIBILIDADE_P100_E010, REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P100] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | TIPO | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NIVEL | N | 3 | - | - | Não | tipo, até 3, 0 decimais |
| 6 | COD_NAT | C | 2 | - | - | Não | tipo, até 2 |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | VAL_CTA_REF_INI | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_VAL_CTA_REF_INI | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 10 | VAL_CTA_REF_DEB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 11 | VAL_CTA_REF_CRED | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 12 | VAL_CTA_REF_FIN | C | 19 | 2 | - | Sim | tipo, até 19, obrigatório |
| 13 | IND_ VAL_CTA_REF_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## P130: Demonstração das Receitas Incentivadas do Lucro Presumido

Página 334. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P130] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## P150: Demonstrativo do Resultado Líquido no Período Fiscal

Página 337. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P150] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | TIPO | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NIVEL | N | 3 | - | - | Não | tipo, até 3, 0 decimais |
| 6 | COD_NAT | C | 2 | - | [04] | Não | tipo, até 2, valores |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | VALOR | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_ VALOR | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## P200: Apuração da Base de Cálculo do IRPJ com Base no Lucro Presumido

Página 340. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P200] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## P230: Cálculo da Isenção e Redução do Lucro Presumido

Página 342. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P230] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## P300: Cálculo do IRPJ com Base no Lucro Presumido

Página 344. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P300] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## P400: Apuração da Base de Cálculo da CSLL com Base no Lucro Presumido

Página 346. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P400] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## P500: Cálculo da CSLL com Base no Lucro Presumido

Página 348. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P500] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## P990: Encerramento do Bloco P

Página 350. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [P990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## Q001: Abertura do Bloco Q

Página 351. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Q001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## Q100: Demonstrativo do Livro Caixa

Página 352. Chave: —. Regras do registro: REGRA_ENTRADA_SAIDA, REGRA_PRIMEIRA_OCORRENCIA, REGRA_SALDO_LC, REGRA_SALDO_LC_FOLGA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Q100] | Sim | tipo, até 4, obrigatório |
| 2 | DATA | N | 8 | - | - | Sim | tipo, até 8, 0 decimais, obrigatório |
| 3 | NUM_DOC | C | - | - | - | Não | tipo, até 255 |
| 4 | HIST | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 5 | VL_ENTRADA | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 6 | VL_SAIDA | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 7 | SLD_FIN | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## Q990: Encerramento do Bloco Q

Página 354. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Q990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## S001: Abertura do Bloco S

Página 355. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [S001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## S030: Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas como TEF

Página 357. Chave: PER_APUR. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ALTERADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [S030] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | PER_APUR | C | 3 | - | “A01”, “A02”, “A03”, “A04”, “A05”, “A06”, “A07”, “A08”, “A09”, “A10”, “A11”, “A1 | Sim | tipo, até 3, obrigatório |

## S100: Balanço Patrimonial

Página 359. Chave: CODIGO. Regras do registro: REGRA_BALANCO_SALDO_FINAL, REGRA_BALANCO_SALDO_INICIAL, REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [S100] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | TIPO | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NIVEL | N | 3 | - | - | Não | tipo, até 3, 0 decimais |
| 6 | COD_NAT | C | 2 | - | - | Não | tipo, até 2 |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | VAL_CTA_REF_INI | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_VAL_CTA_REF_INI | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 10 | VAL_CTA_REF_DEB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 11 | VAL_CTA_REF_CRED | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 12 | VAL_CTA_REF_FIN | C | 19 | 2 | - | Sim | tipo, até 19, obrigatório |
| 13 | IND_ VAL_CTA_REF_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## S150: Demonstrativo do Resultado Líquido no Período Fiscal

Página 362. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [S150] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | TIPO | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NIVEL | N | 3 | - | - | Não | tipo, até 3, 0 decimais |
| 6 | COD_NAT | C | 2 | - | [04] | Não | tipo, até 2, valores |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | VALOR | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_ VALOR | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## S200: Apuração dos Tributos a Pagar Incidentes em TEF/SAF

Página 365. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [S200] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## S300: Cálculo do IRPJ

Página 367. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [S300] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## S500: Cálculo da CSLL

Página 369. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [S500] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## S700: Cálculo dos Demais Tributos

Página 371. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [S700] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## T001: Abertura do Bloco T

Página 384. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [T001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## T030: Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Tributadas pelo Lucro Arbitrado

Página 385. Chave: PER_APUR. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ALTERADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [T030] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | PER_APUR | C | 3 | - | [T01; T02; T03; T04] | Sim | tipo, até 3, valores, obrigatório |

## T120: Apuração da Base de Cálculo do IRPJ com Base no Lucro Arbitrado

Página 387. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [T120] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## T150: Cálculo do IRPJ com Base no Lucro Arbitrado

Página 389. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [T150] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## T170: Apuração da Base de Cálculo da CSLL com Base no Lucro Arbitrado

Página 391. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [T170] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## T181: Cálculo da CSLL com Base no Lucro Arbitrado

Página 393. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [T181] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## T990: Encerramento do Bloco T

Página 395. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [T990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## U001: Abertura do Bloco U

Página 401. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [U001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## U030: Identificação dos Períodos e Formas de Apuração do IRPJ e da CSLL das Empresas Imunes e Isentas

Página 402. Chave: PER_APUR. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ALTERADA, REGRA_PERIODO_DESPREZADO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [U030] | Sim | tipo, até 4, obrigatório |
| 2 | DT_INI | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIN | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | PER_APUR | C | 3 | - | [A00; A01; A02; A03; A04; A05; A06; A07; A08; A09; A10; A11; A12; T01; T02; T03; | Sim | tipo, até 3, valores, obrigatório |

## U100: Balanço Patrimonial

Página 404. Chave: CODIGO. Regras do registro: REGRA_BALANCO_SALDO_FINAL, REGRA_BALANCO_SALDO_INICIAL, REGRA_COMPARA_CONT_REF_SF, REGRA_COMPARA_CONT_REF_SI, REGRA_COMPATIBILIDADE_U100_E010, REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [U100] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | TIPO | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NIVEL | N | 3 | - | - | Não | tipo, até 3, 0 decimais |
| 6 | COD_NAT | C | 2 | - | - | Não | tipo, até 2 |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | VAL_CTA_REF_INI | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_VAL_CTA_REF_INI | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |
| 10 | VAL_CTA_REF_DEB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 11 | VAL_CTA_REF_CRED | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 12 | VAL_CTA_REF_FIN | C | 19 | 2 | - | Sim | tipo, até 19, obrigatório |
| 13 | IND_ VAL_CTA_REF_FIN | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## U150: Demonstração do Resultado

Página 409. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [U150] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 50 | - | - | Sim | tipo, até 50, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | TIPO | C | 1 | - | [S; A] | Sim | tipo, até 1, valores, obrigatório |
| 5 | NIVEL | N | 3 | - | - | Não | tipo, até 3, 0 decimais |
| 6 | COD_NAT | C | 2 | - | [04] | Não | tipo, até 2, valores |
| 7 | COD_CTA_SUP | C | - | - | - | Não | tipo, até 255 |
| 8 | VALOR | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | IND_ VALOR | C | 1 | - | [D; C] | Sim | tipo, até 1, valores, obrigatório |

## U180: Cálculo do IRPJ das Empresas Imunes e Isentas

Página 413. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [U180] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## U182: Cálculo da Contribuição Social sobre o Lucro Líquido (CSLL) das Empresas Imunes e Isentas

Página 415. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [U182] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## U990: Encerramento do Bloco U

Página 417. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [U990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## V001: Abertura do Bloco V

Página 420. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [V001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## V010: DEREX - Instituição

Página 421. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [V010] | Sim | tipo, até 4, obrigatório |
| 2 | NOME_INSTITUICAO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | PAIS | C | 2 | - | Conforme tabela EFDFINANCEIRA_PAISES | Sim | tipo, até 2, obrigatório |
| 4 | TIP_MOEDA | C | - | - | Conforme tabela SPEDECF_CBC_MOEDA | Sim | tipo, até 255, obrigatório |

## V020: Responsável pela Movimentação

Página 422. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [V020] | Sim | tipo, até 4, obrigatório |
| 2 | NOME | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | ENDERECO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | TIPO_DO_C | C | - | - | Conforme tabela SPEDECF_DEREX_DOCUMENTO | Sim | tipo, até 255, obrigatório |
| 5 | NI | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 6 | IDENT_CONTA | C | - | - | - | Sim | tipo, até 255, obrigatório |

## V030: DEREX – Período - Mês

Página 423. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [V030] | Sim | tipo, até 4, obrigatório |
| 2 | MES | C | 2 | - | 01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12 | Sim | tipo, até 2, obrigatório |

## V100: Demonstrativo dos Recursos em Moeda Estrangeira Decorrentes do Recebimento de Exportações

Página 424. Chave: —. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [V100] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 2 | - | Conforme tabela SPEDECF_V100 | Sim | tipo, até 2, obrigatório |
| 3 | DESCRICAO | C | 1000 | - | - | - | tipo, até 1000 |
| 4 | VALOR | C | - | - | - | - | tipo, até 255 |

## V990: Encerramento do Bloco V

Página 426. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [V990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN_M | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## W001: Abertura do Bloco W

Página 431. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [W001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## W100: Informações sobre o Grupo Multinacional e a Entidade Declarante – Declaração País-a-País

Página 432. Chave: REG. Regras do registro: REGRA_W100_W250, REGRA_W100_Y600.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [W100] | Sim | tipo, até 4, obrigatório |
| 2 | NOME_MULTINACIONAL | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | IND_CONTROLADORA | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 4 | NOME_CONTROLADORA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 5 | JURISDICAO_CONTROLADORA | C | 2 | - | - | Sim | tipo, até 2, obrigatório |
| 6 | TIN_CONTROLADORA | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 7 | IND_ENTREGA | N | 1 | - | [1;2;3;4] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |
| 8 | IND_MODALIDADE | N | 1 | - | [1;2] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |
| 9 | NOME_SUBSTITUTA | C | - | - | - | Não | tipo, até 255 |
| 10 | JURISDICAO_SUBSTITUTA | C | 2 | - | - | Não | tipo, até 2 |
| 11 | TIN_SUBSTITUTA | N | 14 | - | - | Não | tipo, até 14, 0 decimais |
| 12 | DT_INI | N | 8 | - | - | Não | tipo, data, até 8, 0 decimais |
| 13 | DT_FIN | N | 8 | - | - | Não | tipo, data, até 8, 0 decimais |
| 14 | TIP_MOEDA | C | 3 | - | - | Não | tipo, até 3 |
| 15 | IND_IDIOMA | C | 2 | - | [PT, EN, ES] | Não | tipo, até 2, valores |

## W200: Declaração País-a-País

Página 444. Chave: JURISDICAO. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [W200] | Sim | tipo, até 4, obrigatório |
| 2 | JURISDICAO | C | 2 | - | - | Sim | tipo, até 2, obrigatório |
| 3 | VL_REC_NAO_REL_EST | N | 19 | 0 | - | Não | tipo, até 19, 0 decimais |
| 4 | VL_REC_NAO_REL | N | 19 | 0 | - | Sim | tipo, até 19, 0 decimais, obrigatório |
| 5 | VL_REC_REL_EST | N | 19 | 0 | - | Não | tipo, até 19, 0 decimais |
| 6 | VL_REC_REL | N | 19 | 0 | - | Sim | tipo, até 19, 0 decimais, obrigatório |
| 7 | VL_REC_TOTAL_EST | N | 19 | 0 | - | Não | tipo, até 19, 0 decimais |
| 8 | VL_REC_TOTAL | N | 19 | 0 | - | Sim | tipo, até 19, 0 decimais, obrigatório |
| 9 | VL_LUC_PREJ_ANTES_IR_EST | NS | 19 | 0 | - | Não | tipo, até 19, 0 decimais |
| 10 | VL_LUC_PREJ_ANTES_IR | NS | 19 | 0 | - | Sim | tipo, até 19, 0 decimais, obrigatório |
| 11 | VL_IR_PAGO_EST | N | 19 | 0 | - | Não | tipo, até 19, 0 decimais |
| 12 | VL_IR_PAGO | N | 19 | 0 | - | Sim | tipo, até 19, 0 decimais, obrigatório |
| 13 | VL_IR_DEVIDO_EST | N | 19 | 0 | - | Não | tipo, até 19, 0 decimais |
| 14 | VL_IR_DEVIDO | N | 19 | 0 | - | Sim | tipo, até 19, 0 decimais, obrigatório |
| 15 | VL_CAP_SOC_EST | N | 19 | 0 | - | Não | tipo, até 19, 0 decimais |
| 16 | VL_CAP_SOC | N | 19 | 0 | - | Sim | tipo, até 19, 0 decimais, obrigatório |
| 17 | VL_LUC_ACUM_EST | N | 19 | 0 | - | Não | tipo, até 19, 0 decimais |
| 18 | VL_LUC_ACUM | N | 19 | 0 | - | Sim | tipo, até 19, 0 decimais, obrigatório |
| 19 | VL_ATIV_TANG_EST | N | 19 | 0 | - | Não | tipo, até 19, 0 decimais |
| 20 | VL_ATIV_TANG | N | 19 | 0 | - | Sim | tipo, até 19, 0 decimais, obrigatório |
| 21 | NUM_EMP | N | 7 | 0 | - | Sim | tipo, até 7, 0 decimais, obrigatório |

## W250: Declaração País-a-País – Entidades Integrantes

Página 455. Chave: —. Regras do registro: REGRA_DECLARANTE_INTEGRANTE_PAIS_A_PAIS, REGRA_OBRIGATORIO_CAMPOS_W250.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [W250] | Sim | tipo, até 4, obrigatório |
| 2 | JUR_DIFERENTE | C | 2 | - | - | Não | tipo, até 2 |
| 3 | NOME | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | TIN | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 5 | JURISDICAO_TIN | C | 2 | - | - | Sim | tipo, até 2, obrigatório |
| 6 | NI | C | - | - | - | Não | tipo, até 255 |
| 7 | JURISDICAO_NI | C | 2 | - | - | Não | tipo, até 2 |
| 8 | TIPO_NI | C | - | - | - | Não | tipo, até 255 |
| 9 | TIP_END | C | 7 | - | [OECD301; OECD302; OECD303; OECD304; OECD305] | Sim | tipo, até 7, valores, obrigatório |
| 10 | ENDEREÇO | C | 150 | - | - | Sim | tipo, até 150, obrigatório |
| 11 | NUM_TEL | C | 15 | - | - | Não | tipo, até 15 |
| 12 | EMAIL | C | 115 | - | - | Não | tipo, até 115 |
| 13 | ATIV_1 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 14 | ATIV_2 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 15 | ATIV_3 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 16 | ATIV_4 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 17 | ATIV_5 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 18 | ATIV_6 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 19 | ATIV_7 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 20 | ATIV_8 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 21 | ATIV_9 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 22 | ATIV_10 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 23 | ATIV_11 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 24 | ATIV_12 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 25 | ATIV_13 | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 26 | DESC_OUTROS | C | 255 | - | - | Não | tipo, até 255 |
| 27 | OBSERVAÇÃO | C | 1000 | - | - | Não | tipo, até 1000 |

## W300: Observações Adicionais – Declaração País-a-País

Página 461. Chave: —. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [W300] | Sim | tipo, até 4, obrigatório |
| 2 | JURISDICAO | C | - | - | - | Não | tipo, até 255 |
| 3 | IND_REC_NAO_REL | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 4 | IND_REC_REL | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 5 | IND_REC_TOTAL | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 6 | IND_LUC_PREJ_ANTES_IR | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 7 | IND_IR_PAGO | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 8 | IND_IR_DEVIDO | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 9 | IND_CAP_SOC | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 10 | IND_LUC_ACUM | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 11 | IND_ATIV_TANG | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 12 | IND_NUM_EMP | C | 1 | - | [S; N] | Não | tipo, até 1, valores |
| 13 | OBSERVAÇÃO | C | 4000 | - | - | Sim | tipo, até 4000, obrigatório |
| 14 | FIM_OBSERVACAO | C | 7 | - | [W300FIM] | sim | tipo, até 7, valores, obrigatório |

## W990: Encerramento do Bloco W

Página 464. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [W990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## X001: Abertura do Bloco X

Página 465. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## X280: Atividades Incentivadas – PJ em Geral

Página 466. Chave: —. Regras do registro: REGRA_ISENCAO_AM_NE_REAL, REGRA_ISENCAO_MISTA, REGRA_ISENCAO_OU_REAL, REGRA_ISENCAO_PRESUMIDO, REGRA_ISENCAO_REDUCAO, REGRA_RECEITA_ISENCAO, REGRA_RECEITA_REDUCOES, REGRA_REDUCAO_100, REGRA_REDUCAO_12, REGRA_REDUCAO_25, REGRA_REDUCAO_33, REGRA_REDUCAO_50, REGRA_REDUCAO_70, REGRA_REDUCAO_75, REGRA_REDUCAO_REINVESTIMENTO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X280] | Sim | tipo, até 4, obrigatório |
| 2 | IND_ATIV | C | 2 | - | [00; 01; 02; 03; 04; 05; 06; 07; 08; 09; 10; 99] | Sim | tipo, até 2, valores, obrigatório |
| 3 | IND_CONCEDENTE | C | 2 | - | [AM; NE; OU] | Sim | tipo, até 2, valores, obrigatório |
| 4 | IND_PROJ | C | 2 | - | [00; 01; 02; 03; 04; 05; 06; 07; 08; 09; 10; 11; 99] | Sim | tipo, até 2, valores, obrigatório |
| 5 | ATO_CONC | C | 30 | - | - | Sim | tipo, até 30, obrigatório |
| 6 | VIG_INI | D | 8 | - | - | Sim | até 8, obrigatório |
| 7 | VIG_FIM | D | 8 | - | - | Sim | até 8, obrigatório |
| 8 | CNPJ_INCENTIVO | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 9 | NCM_INCENTIVO | N | 8 | - | - | Não | tipo, até 8, 0 decimais |
| 10 | REC_LIQ_INCENTIVO | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 11 | VL_INCENTIVO | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## X292: Operações com o Exterior – Pessoa Não Vinculada/Não Interposta/País sem Tributação Favorecida

Página 477. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X292] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X340: Identificação da Participação no Exterior

Página 479. Chave: NIF, CNPJ. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X340] | Sim | tipo, até 4, obrigatório |
| 2 | RAZ_SOCIAL | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | NIF | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | IND_CONTROLE | N | 2 | - | [1; 2; 3; 4; 5; 6; 7; 8; 9; 10;11] | Sim | tipo, até 2, 0 decimais, valores, obrigatório |
| 5 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 6 | IND_ISEN_PETR | C | - | - | [S; N] | Sim | tipo, até 255, valores, obrigatório |
| 7 | IND_CONSOL | C | - | - | [S; N] | Sim | tipo, até 255, valores, obrigatório |
| 8 | MOT_NAO_CONSOL | N | - | - | - | Não | tipo, 0 decimais |
| 9 | CNPJ | C | 14 | - | - | Não | tipo, até 14 |
| 10 | TIP_MOEDA | C | 3 | - | - | Sim | tipo, até 3, obrigatório |

## X350: Participações no Exterior – Resultado do Período de Apuração

Página 483. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X350] | Sim | tipo, até 4, obrigatório |
| 2 | REC_LIQ | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 3 | CUSTOS | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | LUC_BRUTO | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | REC_AUFERIDAS | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | REC_OUTRAS_OPER | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | DESP_BRASIL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | DESP_OPER | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | LUC_OPER | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 10 | REC_PARTIC | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 11 | REC_OUTRAS | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 12 | DESP_OUTRAS | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 13 | LUC_LIQ_ANT_IR | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 14 | LUC_ARB_ANT_IR | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 15 | IMP_DEV | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 16 | LUC_LIQ | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## X351: Demonstrativo de Resultados e de Imposto Pago no Exterior

Página 487. Chave: REG. Regras do registro: REGRA_X351_PREENCHER.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X351] | Sim | tipo, até 4, obrigatório |
| 2 | RES_INV_PER | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 3 | RES_INV_PER_REAL | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | RES_ISEN_PETR_PER | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | RES_ISEN_PETR_PER_REAL | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | RES_NEG_ACUM | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | RES_NEG_ACUM_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | RES_POS_TRIB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | RES_POS_TRIB_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 10 | IMP_LUCR | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 11 | IMP_LUCR_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 12 | IMP_PAG_REND | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 13 | IMP_PAG_REND_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 14 | IMP_RET_EXT | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 15 | IMP_RET_EXT_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 16 | IMP_RET_BR | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## X352: Demonstrativo de Resultados no Exterior Auferidos por Intermédio de Coligadas

Página 491. Chave: REG. Regras do registro: REGRA_X352_PREENCHER.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X352] | Sim | tipo, até 4, obrigatório |
| 2 | RES_PER | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 3 | RES_PER_REAL | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | LUC_DISP | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | LUC_DISP_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## X353: Demonstrativo de Consolidação

Página 492. Chave: REG. Regras do registro: REGRA_X353_PREENCHER.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X353] | Sim | tipo, até 4, obrigatório |
| 2 | RES_NEG_UTIL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 3 | RES_NEG_UTIL_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | SALDO_RES_NEG_NAO_UTIL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | SALDO_RES_NEG_NAO_UTIL_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | RES_PROP | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | RES_PROP_REAL | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## X354: Demonstrativo de Prejuízos Acumulados

Página 494. Chave: REG. Regras do registro: REGRA_X354_PREENCHER.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X354] | Sim | tipo, até 4, obrigatório |
| 2 | RES_NEG_ANT | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 3 | RES_NEG_ANT_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | SALDO_NEG_ACUM | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## X355: Demonstrativo de Rendas Ativas e Passivas

Página 496. Chave: REG. Regras do registro: REGRA_X355_PREENCHER.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X355] | Sim | tipo, até 4, obrigatório |
| 2 | REND_PASS_PROP | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 3 | REND_PASS_PROP_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | REND_TOTAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | REND_TOTAL_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | REND_ATIV_PROP | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | REND_ATIV_PROP_REAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | PERCENTUAL | N | 8 | 4 | - | Sim | tipo, até 8, 4 decimais, obrigatório |

## X356: Demonstrativo de Estrutura Societária

Página 498. Chave: REG. Regras do registro: REGRA_X356_PREENCHER.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X356] | Sim | tipo, até 4, obrigatório |
| 2 | PERC_PART | N | 8 | 4 | - | Sim | tipo, até 8, 4 decimais, obrigatório |
| 3 | ATIVO_TOTAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 4 | PAT_LIQUIDO | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## X357: Investidoras Diretas

Página 499. Chave: REG. Regras do registro: REGRA_X357_PREENCHER.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X357] | Sim | tipo, até 4, obrigatório |
| 2 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 3 | NIF/CNPJ | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | RAZAO_SOCIAL | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 5 | PERCENTUAL | N | 8 | 4 | - | Sim | tipo, até 8, 4 decimais, obrigatório |

## X360: Informações Gerais Sobre Preços de Transferência

Página 501. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X360] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X365: Informações Sobre as Contrapartes nas Transações Controladas

Página 503. Chave: IDENTIFICADOR. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X365] | Sim | tipo, até 4, obrigatório |
| 2 | IDENTIFICADOR | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | NOME_ENT | C | - | - | - | Não | tipo, até 255 |

## X366: Entidades Com as Quais Realiza Transações Controladas

Página 504. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X366] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X370: Informações Sobre as Transações Controladas

Página 506. Chave: IDENTIFICADOR, TIPO_TRANSACAO. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X370] | Sim | tipo, até 4, obrigatório |
| 2 | IDENTIFICADOR | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | TIPO_TRANSACAO | C | 2 | - | [01;02;03;04; 05;06;07; 08;09;10;11;12;13] | Sim | tipo, até 2, valores, obrigatório |
| 4 | NOME_ENT | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 5 | PAIS | C | - | - | TABELA: PAIS_SISCOMEX | Sim | tipo, até 255, obrigatório |
| 6 | COD_NCM | C | - | - | TABELA_NCM | Não | tipo, até 255 |
| 7 | TIPO_DEMAIS | C | 3 | - | [101 a 113;199 201; 202; 299 301 a 304; 399 401 a 406; 499 501 a 511; 599] | Não | tipo, até 3 |
| 8 | DESCR_BSDI | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 9 | VL_TRANSACAO | N | 019 | 002 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 10 | IND_AJUSTES | C | - | - | [S;N] | Sim | tipo, até 255, valores, obrigatório |
| 11 | VL_ESPONTANEO | N | 019 | 002 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 12 | VL_COMPENSATORIO | N | 019 | 002 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 13 | TIP_AJ_COMPENSATORIO | C | - | - | [01;02;03;99] | Não | tipo, até 255, valores |
| 14 | METODO | C | 3 | - | [PIC; PRL; MCL; MLT; MDL; OUT] | Sim | tipo, até 3, valores, obrigatório |
| 15 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 16 | COMP_INTENCIONAL | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 17 | SINERGIA | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 18 | IND_TRANS_COMBINADAS | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 19 | IND_DADOS_MULTIP | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |
| 20 | IND_SIMPLIFIC | C | 1 | - | [S;N] | Sim | tipo, até 1, valores, obrigatório |

## X371: Informações Sobre Ajustes Compensatórios

Página 512. Chave: COD_CTA, COD_CCUS. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X371] | Sim | tipo, até 4, obrigatório |
| 2 | COD_CTA | C | - | - | Códigos existentes no registro J050 | Sim | tipo, até 255, obrigatório |
| 3 | COD_CCUS | C | - | - | Códigos existentes no registro J100 | Não | tipo, até 255 |
| 4 | VALOR | N | 019 | 002 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | IND_VALOR | C | 1 | - | [D;C] | Sim | tipo, até 1, valores, obrigatório |

## X375: Informações Relacionadas aos Métodos

Página 513. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X375] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X390: Origem e Aplicação de Recursos – Imunes e Isentas

Página 516. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X390] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X400: Comércio Eletrônico e Tecnologia da Informação – Informações das Vendas

Página 518. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X400] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X410: Comércio Eletrônico – Informação de Homepage/Servidor

Página 521. Chave: PAIS. Regras do registro: REGRA_HOMEPAGE_SERVIDOR.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X410] | Sim | tipo, até 4, obrigatório |
| 2 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 3 | IND_HOME_DISP | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 4 | IND_SERV_DISP | C | 1 | - | [S, N] | Sim | tipo, até 1, valores, obrigatório |

## X420: Royalties Recebidos ou Pagos a Beneficiários do Brasil e do Exterior

Página 523. Chave: TIP_ROY, PAIS. Regras do registro: REGRA_ROYALTIES_BRASIL, REGRA_ROYALTIES_EXTERIOR, REGRA_X420_OBRIGATORIO_P, REGRA_X420_OBRIGATORIO_R, REGRA_X420_PREENCHIDO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X420] | Sim | tipo, até 4, obrigatório |
| 2 | TIP_ROY | C | 1 | - | [R; P] | Sim | tipo, até 1, valores, obrigatório |
| 3 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 4 | VL_EXPL_DIR_SW | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 5 | VL_EXPL_DIR_AUT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 6 | VL_EXPL_MARCA | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 7 | VL_EXPL_PAT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 8 | VL_EXPL_KNOW | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 9 | VL_EXPL_FRANQ | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 10 | VL_EXPL_INT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## X430: Rendimentos Relativos a Serviços, Juros e Dividendos Recebidos do Brasil e do Exterior

Página 529. Chave: PAIS. Regras do registro: REGRA_CAMPOS_X430.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X430] | Sim | tipo, até 4, obrigatório |
| 2 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 3 | VL_SERV_ASSIST | N | - | 2 | - | Não | tipo, 2 decimais |
| 4 | VL_SERV_SEM_ASSIST | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 5 | VL_SERV_SEM_ASSIST_EXT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 6 | VL_JURO | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 7 | VL_DEMAIS_JUROS | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 8 | VL_DIVID | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## X450: Pagamentos ou Remessas a Título de Serviços, Juros e Dividendos a Beneficiários do Brasil e do Exterior

Página 536. Chave: PAIS. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X450] | Sim | tipo, até 4, obrigatório |
| 2 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |

## X451: Pagamentos ou Remessas a Título de Serviços, Juros e Dividendos a Beneficiários do Brasil e do Exterior – Demais Informações

Página 538. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X451] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X460: Inovação Tecnológica e Desenvolvimento Tecnológico

Página 541. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X460] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X470: Capacitação de Informática e Inclusão Digital

Página 543. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X470] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X480: Benefícios Fiscais – Parte I

Página 546. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X480] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | NS | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## X485: Benefícios Fiscais – Parte II

Página 548. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X485] | Sim | tipo, até 4, obrigatório |
| 2 | TIPO_BENEF | C | 2 | - | [1;2;3;4;5;6; 7;8;9;10;11; 12;13;14;15; 16] | Sim | tipo, até 2, valores, obrigatório |
| 3 | ATO_DECL | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | CNPJ_INCORP | C | 014 |  | - | Não | tipo, até 14 |
| 5 | ID_OBRA_2018 | N | 018 |  | - | Não | tipo, até 18 |
| 6 | ID_OBRA_2020 | N | 018 |  | - | Não | tipo, até 18 |
| 7 | ID_OBRA_EEI | N | 018 |  | - | Não | tipo, até 18 |
| 8 | PORT_CEBAS | N | 007 |  | - | Não | tipo, até 7 |
| 9 | DT_DOU_PORT_CEBAS | N | 008 |  | DDMMAAAA | Não | tipo, data, até 8 |
| 10 | DT_INI_PORT_CEBAS | N | 008 |  | DDMMAAAA | Não | tipo, data, até 8 |
| 11 | DT_FIN_PORT_CEBAS | N | 008 |  | DDMMAAAA | Não | tipo, data, até 8 |

## X490: Polo Industrial de Manaus e Amazônia Ocidental

Página 554. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X490] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X500: Zonas de Processamento de Exportação (ZPE)

Página 556. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X500] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X510: Áreas de Livre Comércio (ALC)

Página 558. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X510] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## X990: Encerramento do Bloco X

Página 560. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## Y001: Abertura do Bloco Y

Página 561. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## Y520: Pagamentos/Recebimentos do Exterior ou de Não Residentes

Página 562. Chave: TIP_EXT, PAIS, FORMA, NAT_OPER. Regras do registro: REGRA_Y520_OBRIGATORIO_P, REGRA_Y520_OBRIGATORIO_R.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y520] | Sim | tipo, até 4, obrigatório |
| 2 | TIP_EXT | C | 1 | - | [R; P] | Sim | tipo, até 1, valores, obrigatório |
| 3 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 4 | FORMA | N | 1 | - | [1; 2; 3; 4; 5; 6] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |
| 5 | NAT_OPER | N | 5 | - | - | Sim | tipo, até 5, 0 decimais, obrigatório |
| 6 | VL_PERIODO | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## Y570: Demonstrativo do Imposto de Renda e CSLL Retidos na Fonte

Página 565. Chave: CNPJ_FON, COD_REC. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y570] | Sim | tipo, até 4, obrigatório |
| 2 | CNPJ_FON | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 3 | NOM_EMP | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | IND_ORG_PUB | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 5 | COD_REC | N | 4 | - | - | Sim | tipo, até 4, 0 decimais, obrigatório |
| 6 | VL_REND | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | IR_RET | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 8 | CSLL_RET | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## Y590: Ativos no Exterior

Página 569. Chave: TIP_ATIVO, DISCRIMINACAO. Regras do registro: REGRA_IND_PART_EXT_Y590.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y590] | Sim | tipo, até 4, obrigatório |
| 2 | TIP_ATIVO | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 3 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 4 | DISCRIMINACAO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 5 | VL_ANT | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | VL_ATUAL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## Y600: Identificação e Remuneração de Sócios, Titulares, Dirigentes e Conselheiros

Página 571. Chave: —. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y600] | Sim | tipo, até 4, obrigatório |
| 2 | DT_ALT_SOC | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | DT_FIM_SOC | N | 8 | - | - | Não | tipo, data, até 8, 0 decimais |
| 4 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 5 | IND_QUALIF | C | 2 | - | [PF; PJ; FI] | Sim | tipo, até 2, valores, obrigatório |
| 6 | CPF_CNPJ | C | 14 | - | - | Não | tipo, até 14 |
| 7 | NOM_EMP | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 8 | QUALIF | C | 2 | - | [01; 02; 03; 04; 05; 06; 07; 08; 09; 10; 11; 12; 13; 14; 15; 16; 17; 18] | Sim | tipo, até 2, valores, obrigatório |
| 9 | PERC_CAP_TOT | N | 8 | 4 | - | Sim | tipo, até 8, 4 decimais, obrigatório |
| 10 | PERC_CAP_VOT | N | 8 | 4 | - | Sim | tipo, até 8, 4 decimais, obrigatório |
| 11 | CPF_REP_LEG | N | 11 | - | - | Não | tipo, até 11, 0 decimais |
| 12 | QUALIF_REP_LEG | N | 2 | - | [01; 02; 03; 04; 05; 06] | Não | tipo, até 2, 0 decimais, valores |
| 13 | VL_REM_TRAB | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 14 | VL_LUC_DIV | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 15 | VL_JUR_CAP | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 16 | VL_DEM_REND | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 17 | VL_IR_RET | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## Y612: Identificação e Rendimentos de Dirigentes e Conselheiros – Imunes ou Isentas

Página 577. Chave: CPF, QUALIF. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y612] | Sim | tipo, até 4, obrigatório |
| 2 | CPF | N | 11 | - | - | Sim | tipo, até 11, 0 decimais, obrigatório |
| 3 | NOME | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | QUALIF | N | 2 | - | [10; 11; 12; 13; 14; 15; 16; 17] | Sim | tipo, até 2, 0 decimais, valores, obrigatório |
| 5 | VL_REM_TRAB | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | VL_DEM_REND | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 7 | VL_IR_RET | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## Y620: Participações Avaliadas Pelo Método de Equivalência Patrimonial

Página 579. Chave: —. Regras do registro: REGRA_IND_PART_COLIG_Y620.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y620] | Sim | tipo, até 4, obrigatório |
| 2 | DT_EVENTO | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 3 | IND_RELAC | N | 1 | - | - | Sim | tipo, até 1, 0 decimais, obrigatório |
| 4 | PAIS | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 5 | CNPJ | C | 14 | - | - | Não | tipo, até 14 |
| 6 | NOM_EMP | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 7 | VALOR_REAIS | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | VALOR_ESTR | NS | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 9 | PERC_CAP_TOT | N | 8 | 4 | - | Sim | tipo, até 8, 4 decimais, obrigatório |
| 10 | PERC_CAP_VOT | N | 8 | 4 | - | Sim | tipo, até 8, 4 decimais, obrigatório |
| 11 | RES_EQ_PAT | NS | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 12 | DATA_AQUIS | C | 8 | - | - | Sim | tipo, até 8, obrigatório |
| 13 | IND_PROC_CART | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 14 | NUM_PROC_CART | C | - | - | - | Não | tipo, até 255 |
| 15 | NOME_CART | C | - | - | - | Não | tipo, até 255 |
| 16 | IND_PROC_RFB | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 17 | NUM_PROC_RFB | C | - | - | - | Não | tipo, até 255 |

## Y630: Fundos/Clubes de Investimento

Página 583. Chave: CNPJ. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y630] | Sim | tipo, até 4, obrigatório |
| 2 | CNPJ | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 3 | QTE_QUOT | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |
| 4 | QTE_QUOTA | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |
| 5 | PATR_FIN_PER | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 6 | DAT_ABERT | N | 8 | - | - | Sim | tipo, até 8, 0 decimais, obrigatório |
| 7 | DAT_ENCER | N | 8 | - | - | Não | tipo, até 8, 0 decimais |

## Y640: Participações em Consórcios de Empresas

Página 585. Chave: CNPJ. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y640] | Sim | tipo, até 4, obrigatório |
| 2 | CNPJ | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 3 | COND_DECL | N | 1 | - | [1; 2] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |
| 4 | VL_CONS | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 5 | CNPJ_LID | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 6 | VL_DECL | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## Y650: Participantes do Consórcio

Página 587. Chave: CNPJ. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y650] | Sim | tipo, até 4, obrigatório |
| 2 | CNPJ | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 3 | VL_PART | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |

## Y660: Dados de Sucessoras

Página 588. Chave: CNPJ. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y660] | Sim | tipo, até 4, obrigatório |
| 2 | CNPJ | C | 14 | - | - | Sim | tipo, até 14, obrigatório |
| 3 | NOM_EMP | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | PERC_PAT_LIQ | N | 8 | 4 | - | Não | tipo, até 8, 4 decimais |

## Y672: Outras Informações (Lucro Presumido ou Lucro Arbitrado)

Página 590. Chave: REG. Regras do registro: REGRA_Y672_PREENCHIDO.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y672] | Sim | tipo, até 4, obrigatório |
| 2 | VL_CAPITAL_ANT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 3 | VL_CAPITAL | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 4 | VL_ESTOQUE_ANT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 5 | VL_ESTOQUES | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 6 | VL_CAIXA_ANT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 7 | VL_CAIXA | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 8 | VL_APLIC_FIN_ANT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 9 | VL_APLIC_FIN | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 10 | VL_CTA_REC_ANT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 11 | VL_CTA_REC | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 12 | VL_CTA_PAG_ANT | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 13 | VL_CTA_PAG | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 14 | VL_COMPRA_MERC | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 15 | VL_COMPRA_ATIVO | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 16 | VL_RECEITAS | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 17 | TOT_ATIVO | N | 19 | 2 | - | Não | tipo, até 19, 2 decimais |
| 18 | IND_AVAL_ESTOQ | C | 1 | - | [1; 2; 3; 4; 5; 6; 7; 8] | Não | tipo, até 1, valores |

## Y680: Mês das Informações de Optantes pelo Refis (Lucro Real, Presumido e Arbitrado)

Página 595. Chave: —. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y680] | Sim | tipo, até 4, obrigatório |
| 2 | MES | C | 2 | - | [01; 02; 03; 04; 05; 06; 07; 08; 09; 10; 11; 12] | Sim | tipo, até 2, valores, obrigatório |

## Y681: Informações de Optantes pelo Refis (Lucros Real, Presumido e Arbitrado)

Página 596. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [X681] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## Y682: Informações de Optantes pelo Refis – Imunes ou Isentas

Página 598. Chave: —. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y682] | Sim | tipo, até 4, obrigatório |
| 2 | MES | C | 2 | - | [01; 02; 03; 04; 05; 06; 07; 08; 09; 10; 11; 12] | Sim | tipo, até 2, valores, obrigatório |
| 3 | ACRES_PATR | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |

## Y720: Informações de Períodos Anteriores

Página 600. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y720] | Sim | tipo, até 4, obrigatório |
| 2 | LUC_LIQ | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 3 | DT_LUC_LIQ | N | 8 | - | - | Sim | tipo, data, até 8, 0 decimais, obrigatório |
| 4 | REC_BRUT_ANT | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 5 | INTIMACAO | C | 1 | - | [S; N] | Sim | tipo, até 1, valores, obrigatório |
| 6 | INT_ATRASO | C | 1 | - | [S; N] | Não | tipo, até 1, valores |

## Y730: Identificação de donatários/destinatários de deduções do IRPJ/CSLL

Página 603. Chave: DEDUCAO, TIPO_DOACAO, DT_DOACAO, DESTINATARIO. Regras do registro: REGRA_DUPLICIDADE_REGI, REGRA_OBRIGATORIEDADE_REG.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y730] | Sim | tipo, até 4, obrigatório |
| 2 | DEDUCAO | N | 4 | - | - | Sim | tipo, até 4, 0 decimais, obrigatório |
| 3 | TIPO | N | 4 | - | - | Sim | tipo, até 4, 0 decimais, obrigatório |
| 4 | DATA | N | 8 | - | - | Sim | tipo, até 8, 0 decimais, obrigatório |
| 5 | TIPO_DESTINATARIO | C | 2 | - | [PF;PJ] | Sim | tipo, até 2, valores, obrigatório |
| 6 | DESTINATARIO | C | 14 |  | - | Sim | tipo, até 14, obrigatório |
| 7 | VALOR | N | 19 | 2 | - | Sim | tipo, até 19, 2 decimais, obrigatório |
| 8 | OBSERVACAO | C | - |  | - | Não | tipo, até 255 |

## Y750: Informações da ECF Calculadas pelo PGE

Página 607. Chave: CODIGO. Regras do registro: REGRA_DUPLICIDADE_DESPREZADA, REGRA_LINHA_ATUALIZADA, REGRA_LINHA_DESPREZADA.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y750] | Sim | tipo, até 4, obrigatório |
| 2 | CODIGO | C | 6 | - | - | Sim | tipo, até 6, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Não | tipo, até 255 |
| 4 | VALOR | C | - | - | - | Não | tipo, até 255 |

## Y800: Outras Informações

Página 611. Chave: —. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y800] | Sim | tipo, até 4, obrigatório |
| 2 | TIPO_DOC | N | 3 | - | - | Sim | tipo, até 3, 0 decimais, obrigatório |
| 3 | DESCRICAO | C | - | - | - | Sim | tipo, até 255, obrigatório |
| 4 | HASH | C | 41 | - | - | Sim | tipo, até 41, obrigatório |
| 5 | ARQ_RTF | C | Não há limite | - | - | Sim | tipo, obrigatório |
| 6 | IND_FIM_RTF | C | 7 | - | [Y800FIM] | Sim | tipo, até 7, valores, obrigatório |

## Y990: Encerramento do Bloco Y

Página 612. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [Y990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## 9001: Abertura do Bloco 9

Página 613. Chave: REG. Regras do registro: REGRA_OCORRENCIA_UNITARIA_ARQ.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [9001] | Sim | tipo, até 4, obrigatório |
| 2 | IND_DAD | N | 1 | - | [0;1] | Sim | tipo, até 1, 0 decimais, valores, obrigatório |

## 9100: Avisos da Escrituração

Página 614. Chave: —. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [9100] | Sim | tipo, até 4, obrigatório |
| 2 | NOM_REGRA | N | - | - | - | S | tipo, 0 decimais, obrigatório |
| 3 | MSG_REGRA | C | - | - | - | N | tipo, até 255 |
| 4 | REGISTRO | C | 4 | - | - | S | tipo, até 4, obrigatório |
| 5 | CAMPO | C | - | - | - | N | tipo, até 255 |
| 6 | CONTEÚDO | NS | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 7 | VALOR_ESPERADO | NS | 19 | 2 | - | N | tipo, até 19, 2 decimais |
| 8 | PER_APUR | C | 3 | - | - | N | tipo, até 3 |
| 9 | COD_CTA | C | - | - | - | N | tipo, até 255 |
| 10 | COD_CCUS | C | - | - | - | N | tipo, até 255 |
| 11 | COD_CTA_REF | C | - | - | - | N | tipo, até 255 |
| 12 | CODIGO | C | - | - | - | N | tipo, até 255 |
| 13 | NUM_ORDEM | C | - | - | - | N | tipo, até 255 |
| 14 | CNPJ_ESTAB | C | 14 | - | - | N | tipo, até 14 |
| 15 | CNAE | N | 7 | - | - | N | tipo, até 7, 0 decimais |
| 16 | COD_CTA_B | C | - | - | - | N | tipo, até 255 |
| 17 | COD_TRIBUTO | C | - | - | - | N | tipo, até 255 |

## 9900: Registros do Arquivo

Página 616. Chave: —. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [9900] | Sim | tipo, até 4, obrigatório |
| 2 | REG_BLC | C | 4 | - | - | Sim | tipo, até 4, obrigatório |
| 3 | QTD_REG_BLC | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |
| 4 | VERSAO | C | 4 |  | - | Não | tipo, até 4 |
| 5 | ID_TAB_DIN | C | - |  | - | Não | tipo, até 255 |

## 9990: Encerramento do Bloco 9

Página 618. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [9990] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |

## 9999: Encerramento do Arquivo Digital

Página 619. Chave: REG. Regras do registro: —.

| Nº | Campo | Tipo | Tamanho | Dec. | Valores válidos | Obrigatório | Verificações |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | REG | C | 4 | - | [9999] | Sim | tipo, até 4, obrigatório |
| 2 | QTD_LIN | N | - | - | - | Sim | tipo, 0 decimais, obrigatório |
