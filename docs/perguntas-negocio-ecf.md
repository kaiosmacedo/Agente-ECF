# Perguntas de negócio da ECF para o superior

Levantamento de 2026-11-10, preparado para Kaio Macedo levar ao superior. Cópia do documento compartilhável [Agente-ECF: perguntas sobre regras de negócio](https://claude.ai/artifact/CSXzmX6yUxNPbNbz6uDR9f). **Estado:** sem respostas ainda. As perguntas 2, 5, 7 e 9 destravam a mudança 09 (`regras-negocio-irpj-csll`, esclarecimentos E01 e E02); as demais podem vir depois.

Cada pergunta traz a nossa recomendação. Basta responder "concordo", "ajustar" (com o ajuste) ou "não sei". "Não sei" também resolve: o agente passa a declarar o ponto como não conferido, em vez de supor uma regra (P01, P08).

## Onde o agente está

| Parte | Situação |
| --- | --- |
| Receber a ECF pela Sofia e por API | No ar |
| Reconhecer a ECF e o leiaute (7 a 12) | No ar |
| Conferir a estrutura do leiaute 12 | No ar, conferida no programa da ECF 12.2.8 |
| Consultar o manual da ECF | Pronta, desligada até o cadastro da chave da OpenAI no Vercel |
| Conferir o cálculo do IRPJ e da CSLL e os cruzamentos | Não começou: depende dos blocos 1 a 3 |
| Explicar e priorizar os achados com IA | Plano da mudança 10 à espera de aprovação |
| Login e dados reais | Adiado (SQ-12): hoje só dados fictícios |

## Bloco 1 — Como o agente será usado

| Nº | Pergunta | Opções | Recomendação | Por que importa |
| --- | --- | --- | --- | --- |
| 1 | Quem vai ler o resultado do agente? | (a) equipe da INFIS; (b) o cliente; (c) os dois | (a) | Define a linguagem e o quanto o agente explica |
| 2 | Em que momento a ECF é enviada, e em qual forma? | (a) TXT do sistema contábil, antes de importar; (b) arquivo exportado pelo programa, com os blocos C e E; (c) ECF transmitida | Aceitar as três; só (b) e (c) saem "sem pendências" quando o regime exige recuperar a ECD | O TXT não traz os saldos da ECD; o programa acusa "Não houve recuperação de ECD" no lucro real |
| 3 | Quais perfis de empresa a INFIS atende mais? | Lucro real anual e trimestral, presumido, arbitrado, imunes e isentas, financeiras e seguradoras, atividade rural, multinacionais | Indicar os mais frequentes, com proporção aproximada | O programa tem tabelas de cálculo por perfil |
| 4 | Conferir ECF de anos anteriores, como em retificações? | (a) só 2025 (leiaute 12); (b) também 2020 a 2024 (leiautes 7 a 11) | (a) agora | Cada ano tem leiaute e tabelas próprios |

## Bloco 2 — Quais regras conferir primeiro

O cálculo do IRPJ e da CSLL está nas tabelas de regras do próprio programa da ECF, indicadas pelo manual (p. 40): 3.067 regras vigentes para 2025, sendo 2.558 de relacionamento da linha com conta e 509 de valor. Num teste em 2026-11-08, o programa aplicou essas regras. O manual tem ainda 626 regras próprias, e o maior grupo, com 253, é o de preenchimento.

| Nº | Pergunta | Opções | Recomendação |
| --- | --- | --- | --- |
| 5 | O objetivo é antecipar o que o programa aponta, ou ir além? | (a) reproduzir o programa: as 3.067 regras de cálculo e cerca de 200 do manual (blocos J a U); (b) incluir já riscos de malha | (a); os riscos extras entram depois, um a um |
| 6 | As regras dos blocos W, X e Y (cerca de 330, mais cerca de 95 do bloco 0 e dos blocos S e V) podem ficar para uma segunda etapa? | Sim ou não | Sim, salvo se multinacionais forem frequentes; enquanto isso, ficam declaradas como não conferidas |
| 7 | Comparar a ECF com outras declarações? | (a) não agora, um arquivo por vez; (b) ECF anterior e ECD; (c) também DCTFWeb e PER/DCOMP | (a); o arquivo exportado pelo programa já traz os saldos recuperados |
| 8 | Listar valores para conferir fora da ECF? | Sim ou não | Sim: IRPJ e CSLL a pagar por período, sem conclusão, para conferir com a DCTFWeb ([Receita Federal, ação de 2026](https://www.gov.br/receitafederal/pt-br/assuntos/noticias/2026/junho/receita-federal-inicia-nova-edicao-da-acao-de-conformidade-201cinsuficiencia-de-declaracao-e-recolhimento-de-irpj-csll201d)) |

## Bloco 3 — Gravidade de cada apontamento

Três níveis: **recusar** (não entregar assim), **risco fiscal** (entrega possível, mas pode gerar multa ou autuação; pede revisão humana) e **observação**. Só a divisão do manual entre erro e aviso é oficial (p. 38); "risco fiscal" é critério do produto.

| Nº | Pergunta | Recomendação |
| --- | --- | --- |
| 9 | Todo erro do programa recusa o arquivo? | Sim, como o programa. Mudança em relação à lista de 2026-11-06: a retenção deduzida diferente do Y570 passa a recusar, porque o programa dá erro acima de R$ 0,50 |
| 10 | A lista de riscos fiscais está certa? | Manter as sete: saldo da Parte B diferente do ano anterior (p. 239); conta da Parte B que some (p. 126 e 239); K155 e K355 diferentes da ECD (p. 148 e 156); lançamento na Parte B com sentido trocado (p. 252); compensação acima de 30%; Outras Exclusões acima de R$ 20 milhões e de 30% ([comunicado de 30/06/2026](https://www.gov.br/sped/pt-br/assuntos/comunicados/ecf/ecf-outras-exclusoes-sem-relacionamento)); Finor ou Finam acima de 6% (p. 296) |
| 11 | Na prática, o programa bloqueia a entrega nos três conflitos do manual (saldo da Parte B diferente do ano anterior, p. 239; conta da Parte B que some, p. 126 e 239; conta criada antes do início da ECF que não veio do ano anterior, p. 240)? | Risco fiscal até alguém com experiência confirmar |
| 12 | Valor informado em linha calculada diferente do recálculo? | Observação, com os dois valores |
| 13 | Que tolerância de arredondamento usar? | Só as do programa: R$ 0,50 nas retenções e R$ 0,15 em algumas deduções; nenhuma outra |

## Bloco 4 — O que o agente pode dizer

| Nº | Pergunta | Recomendação |
| --- | --- | --- |
| 14 | Citar consequências legais (multa de 3% por inexatidão no lucro real, malha)? | Citar a norma e o tipo de risco, sem calcular multa e sem afirmar que a Receita aprova, recusa ou autua |
| 15 | Sugerir como corrigir? | Sim, como orientação baseada na mensagem oficial; o agente nunca altera o arquivo |
| 16 | Mostrar os 315 avisos informativos do programa? | Sim, como observação, depois dos erros e dos riscos fiscais |

## Bloco 5 — Responsabilidade, dados reais e custo

| Nº | Pergunta | Recomendação | Situação em 2026-11-10 |
| --- | --- | --- | --- |
| 17 | Quem valida as regras fiscais antes de entrarem? | Um contador revisa as respostas às perguntas 9 a 16 antes do uso com dados reais | Em aberto |
| 18 | Quando receber ECF reais de clientes? | Só depois de login e política de retenção | Kaio Macedo decidiu guardar por tempo indeterminado, no momento; login segue adiado (SQ-12) |
| 19 | Dados de clientes podem ir para a OpenAI? | Só os apontamentos, com condições verificadas | Kaio Macedo decidiu: podem ir |
| 20 | Que conta paga a IA em produção? | Chave própria, com limite mensal | Kaio Macedo decidiu: a chave que o projeto já usa; falta cadastrá-la no Vercel |
