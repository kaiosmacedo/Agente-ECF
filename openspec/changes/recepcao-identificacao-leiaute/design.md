# Design: Recepção do arquivo e identificação do leiaute

> **Estado:** proposto em 2026-11-07, aguardando a aprovação de Kaio Macedo (P20). Nenhum código entra antes disso.

## Context

As etapas 1 a 3 do fluxo (P03) existem hoje só como esqueleto: o `SendMessage` registra a execução, guarda hash e tamanho de cada documento e responde `parcial`, com as etapas 3 a 8 não executadas. O objetivo do responsável é deixar o agente pronto para a Sofia INFIS: ela reconhece que recebeu uma ECF, encontra no cartão que este agente trata ECF, envia o arquivo, e o agente devolve o resultado. Na arquitetura final, a Sofia leva esse resultado a um agente validador, feito por outra pessoa, antes de responder ao usuário. Este repositório não muda nada por causa dele (D10).

Esta mudança faz o agente saber o que recebeu: uma ECF inteira, de qual leiaute, ou outra coisa. As validações do conteúdo ficam nas mudanças 08 e 09, e a consulta ao manual, na 06.

## Goals / Non-Goals

**Goals:**
- Um serviço de aplicação para os dois canais: `SendMessage` (A2A) e `POST /analises` (REST).
- Reconhecer a ECF pelo conteúdo, recusar o que não é ECF e declarar o que não foi analisado.
- Identificar o leiaute pelo campo `COD_VER` do Registro 0000, para os leiautes 7 a 12.
- Não validar arquivo cortado como se estivesse inteiro.
- Cartão que diz à Sofia o que o agente analisa e devolve.

**Non-Goals:**
- Validar registros e campos (08) e regras de negócio (09).
- Arquivos compactados, mais de uma ECF por pedido e comparação entre períodos (09, DP-06).
- Guardar o arquivo recebido (D4).
- Chamar o agente validador ou qualquer outro agente (RA-07).

## Decisions

### D1. Um serviço para os dois canais (REQ-RECEPCAO-008; REQ-A2A-002)

O `ProcessadorDeSolicitacoes` vira o serviço de análise da aplicação, que recebe um pedido normalizado: identificadores, identidade declarada (SQ-12) e a lista de documentos, cada um com identificador, nome e texto. O adaptador A2A converte a `SolicitacaoAnaliseSofia`; o `POST /analises` converte o arquivo enviado. Nenhuma regra fica nos endpoints (P16).

| Canal | Identificadores | Texto e hash | Retentativa |
| --- | --- | --- | --- |
| A2A | `idExecucao` e `idSolicitacao` da Sofia | Texto já decodificado pela Sofia; SHA-256 sobre o texto em UTF-8 (GD-06) | Mesmo `idSolicitacao` devolve o resultado registrado, como hoje |
| REST | Gerados pelo sistema | Bytes do arquivo lidos como ISO 8859-1 (D3); SHA-256 sobre os bytes recebidos | Cada envio é uma execução nova |

A migração `0004_canal` acrescenta a coluna `canal` (`a2a` ou `rest`) em `execucoes`, para a auditoria (P19).

### D2. Ordem das verificações da etapa 2 (REQ-RECEPCAO-002 a 007; REQ-SOFIA-004)

Para no primeiro problema, sem ler o resto (SQ-05):

1. Contrato SOFIA, quando A2A (já existe).
2. Tamanho total dos textos, contra `Recepcao:TamanhoMaximoDosDocumentosEmBytes` (D4).
3. Para cada documento: é ECF se a primeira linha começa com `|0000|LECF|` (REQ-RECEPCAO-005). Linhas em branco ou espaços antes não são aceitos, porque os registros começam sempre na primeira coluna (p. 32). Só uma marca de codificação UTF-8 (BOM) no início é ignorada, porque não é conteúdo.
4. Quantidade de ECF: nenhuma, código `nenhuma_ecf`; mais de uma, `mais_de_uma_ecf` (REQ-RECEPCAO-006).
5. Na ECF: tamanho de linha e quantidade de linhas (REQ-RECEPCAO-004), contados durante a leitura.
6. Marcador de corte da Sofia no fim do texto (D6).

Os documentos que não são ECF entram nas observações, com o identificador, como "não analisado por este agente: não é uma ECF". Eles não viram achado, porque não são problema da ECF (P04).

| Código | Status | Quando |
| --- | --- | --- |
| `documentos_acima_do_limite` | `entrada-rejeitada` | Tamanho total acima do limite |
| `nenhuma_ecf` | `entrada-rejeitada` | Nenhum documento começa com `\|0000\|LECF\|` |
| `mais_de_uma_ecf` | `entrada-rejeitada` | Duas ou mais ECF no mesmo pedido |
| `linha_acima_do_limite` | `entrada-rejeitada` | Linha maior que o limite |
| `linhas_acima_do_limite` | `entrada-rejeitada` | Quantidade de linhas acima do limite |

Para a Sofia, `entrada-rejeitada` continua sendo `status: erro`, com o código em `erro.codigo`, como na mudança 13.

### D3. `POST /analises` (REQ-RECEPCAO-001, 004, 005)

- `multipart/form-data` com uma única parte de arquivo, `arquivo`. A resposta é `201 Created` com o envelope do resultado, o mesmo de `GET /analises/{id}`, e o cabeçalho `Location`.
- O arquivo é lido até o limite configurado mais um byte. Se passar, a execução é registrada com a etapa 2 com falha e o código `documentos_acima_do_limite`, sem ler o resto (REQ-RECEPCAO-002). O limite do Kestrel para a rota fica 64 KiB acima, para caber o envelope do multipart; acima dele, e acima do limite do Vercel, a resposta é `413` sem execução, porque o corpo nem chegou ao serviço.
- O arquivo é lido em fluxo, como ISO 8859-1, a codificação da seção 2.2 do manual (p. 32). Toda sequência de bytes é válida em ISO 8859-1, então a leitura não falha; um arquivo gravado em outra codificação mostra acentos trocados, e isso fica para a validação de campos (08).
- O nome do arquivo não é usado em caminho, comando nem consulta (REQ-RECEPCAO-004): nada é gravado em disco (D4). Ele entra só nas observações, cortado em 100 caracteres e sem caracteres de controle.
- Sem credencial (SQ-12). Não aciona provedor pago, então não precisa de limite de uso nesta mudança. Quando a mudança 10 ligar o LLM ao fluxo, ela aplica o limite.

### D4. Tamanho, linhas e guarda do arquivo (E02, E04; DP-05, DP-11)

| Opção | Padrão | Justificativa |
| --- | --- | --- |
| `Recepcao:TamanhoMaximoDosDocumentosEmBytes` | `4194304` (4 MiB) | O Vercel recusa corpo entre 4,4 MB e 4,6 MB (`docs/contratos/sofia-a2a.md`). Acima de 4 MiB de texto, o corpo com o envelope JSON quase sempre passa disso. Substitui `A2A:TamanhoMaximoDosDocumentosEmBytes`. |
| `Recepcao:TamanhoMaximoDaLinhaEmCaracteres` | igual ao tamanho máximo | O manual não limita o campo `ARQ_RTF` do Registro Y800, que carrega um arquivo RTF inteiro em uma linha (leiaute 12, p. 609 a 611). Um limite menor recusaria arquivo válido. |
| `Recepcao:QuantidadeMaximaDeLinhas` | `1000000` | Proteção contra esgotamento (SQ-05). Com 4 MiB, um arquivo real fica bem abaixo disso. |

**Arquivo real maior que 4 MiB:** não há arquivo real para medir, e a primeira versão só recebe dados sintéticos (SQ-12). Antes de dados reais, alguém precisa medir o tamanho de ECF reais. Se passarem do limite, a Api.Agente vai para outro host, por novo ADR. Isso entra em `docs/roteiro.md`, em "Antes de dados reais".

**Guarda:** o texto não é guardado, só hash, tamanho e canal, como hoje (DP-25). O valor encontrado em cada achado, cortado em 200 caracteres, já é guardado desde a mudança 15. A política de retenção para dados reais continua em DP-11.

### D5. Identificação do leiaute (REQ-LEIAUTE-001 a 004; E05, E06, E08)

A versão vem do campo 3 do Registro 0000, `COD_VER`, cujo valor está numa observação do próprio campo em cada manual. Os exemplos de preenchimento não servem de fonte: o do Leiaute 12 traz `0011` com datas de 2025 (p. 69), enquanto a observação diz `0012` (p. 63).

| `COD_VER` | Leiaute | Vale para | Fonte |
| --- | --- | --- | --- |
| `0007` | 7 | Ano-calendário 2020 e situações especiais de 2021 | Manual do leiaute 7, p. 55 |
| `0008` | 8 | 2021 e situações especiais de 2022 | Leiaute 8, p. 56 |
| `0009` | 9 | 2022 e situações especiais de 2023 | Leiaute 9, p. 55 |
| `0010` | 10 | 2023 e situações especiais de 2024 | Leiaute 10, p. 60 e 65 (divergência abaixo) |
| `0011` | 11 | 2024 e situações especiais de 2025 | Leiaute 11, p. 59 e p. 10 |
| `0012` | 12 | 2025 e situações especiais de 2026 | Leiaute 12, p. 63 e p. 10 |

**Divergência no Leiaute 10 (P01):** a observação diz `00010`, com cinco caracteres, num campo `C` de tamanho 4, e o exemplo da p. 65 usa `0010`. Proposta: aceitar `0010`, que cabe no campo e segue o padrão dos demais. Kaio Macedo decide (E05).

Passos:

1. Os campos da primeira linha são separados pelo `|`. A posição dos campos do 0000 é a mesma nos seis manuais (exemplos: leiaute 7, p. 60; leiaute 12, p. 69): `COD_VER` é o 3º, `SIT_ESPECIAL` o 7º e `DT_FIN` o 11º.
2. `COD_VER` da tabela: leiaute identificado. Quatro caracteres fora da tabela: versão não suportada, com o valor registrado (REQ-LEIAUTE-002). Vazio ou com outro tamanho: falha atribuída ao documento (REQ-LEIAUTE-003).
3. Conferência com o período (REQ-LEIAUTE-004): `DT_FIN` no formato `ddmmaaaa` (p. 34); o ano tem de ser o ano-calendário do leiaute, ou o seguinte com `SIT_ESPECIAL` de 1 a 6 (p. 10). Fora disso, a identificação não é inequívoca (P03): falha atribuída ao documento, com a limitação "`COD_VER` 0012 não corresponde a `DT_FIN` em 2023, segundo o manual do leiaute 12 (p. 10 e 63)".
4. Registro na execução: tipo `ECF`, leiaute, `COD_VER` e ano de `DT_FIN`.

**O que fica fora:** a regra oficial `REGRA_LEIAUTE_VIGENTE` (`Erro`, nível 1.1, p. 39 e 65) usa a tabela "Controle de Versões" do documento "SPEDEFDIRPJ-RNG-Geral", que não está no manual. Sem essa fonte, o agente não afirma a regra (P01): a conferência do passo 3 só decide se a identificação é inequívoca, e a regra continua em `naoVerificados`.

**Arquivo de importação ou gerado pelo programa (E08):** o manual não define campo nem marcador que distinga os dois. Ele só documenta indícios: blocos C e E recuperados pelo programa (p. 41, 109 e 122), registros com obrigatoriedade de entrada "N" na Tabela de Registros (p. 43 a 58), o 9100 e o Y750 gerados pelo sistema (p. 607 e 614) e a assinatura depois do 9999 (p. 17). Esses indícios dependem do catálogo de registros, então entram na mudança 08, que registra o indício encontrado, sem afirmar o tipo do arquivo quando não houver indício.

### D6. Documento cortado pela Sofia (REQ-RECEPCAO-007; E07)

O código atual da Sofia corta cada arquivo em 20.000 caracteres e acrescenta `[DOCUMENTO TRUNCADO]`. Com esse corte, só uma ECF sintética muito pequena chega inteira.

- O agente procura o marcador no fim do texto, depois de tirar espaços e quebras de linha.
- Com o marcador, a etapa 3 roda, porque o Registro 0000 está na primeira linha, e as etapas 4 e 5 ficam não executadas com a limitação "documento cortado antes de chegar ao agente". Nenhum achado estrutural é produzido, porque o fim do arquivo não chegou.
- Sem o marcador, um arquivo incompleto por outro motivo é tratado pela validação estrutural (08), que confere o encerramento do arquivo.
- O guia para a Sofia (D9) pede que o corte não se aplique aos agentes. Retirar o corte é com quem mantém a Sofia, fora deste repositório.

### D7. Falha da etapa 3 por causa do documento (REQ-LEIAUTE-003)

Hoje, qualquer etapa com falha vira `falha-tecnica` (mudança 03, D2). Uma ECF sem `COD_VER` não é falha do sistema, e repetir a chamada não resolve. O registro da etapa ganha a causa da falha, `tecnica` ou `documento`:

| Situação | Etapa 3 | Status | Veredito |
| --- | --- | --- | --- |
| `COD_VER` de um leiaute de 7 a 12 | Concluída | Segue o fluxo | Segue o fluxo |
| `COD_VER` preenchido, fora dos leiautes 7 a 12 | Concluída, com a versão registrada | `parcial`; etapas 4 e 5 não executadas (REQ-LEIAUTE-002) | `parcial` |
| `COD_VER` ausente ou fora do formato | Com falha, causa `documento` | `parcial` | `parcial` |
| Exceção no identificador | Com falha, causa `tecnica` | `falha-tecnica` | `nao-avaliado` |

A regra do status muda só em um ponto: `falha-tecnica` passa a exigir etapa com falha de causa técnica ou achado de falha técnica. O cenário de REQ-EXEC-004 continua atendido, porque `parcial` também não declara conformidade. A migração `0005_causa_da_falha` acrescenta a coluna em `etapas`, com `tecnica` para as linhas existentes.

### D8. Cartão 1.1.0 (REQ-A2A-007, REQ-A2A-008)

O identificador da skill continua `analisar_documentos`, o padrão da turma (skill `sofia-a2a-implementacao`, RF01), para a Sofia achar o agente como hoje. Muda o texto:

| Campo | Valor |
| --- | --- |
| `description` | "Valida arquivos da Escrituração Contábil Fiscal (ECF) do SPED enviados pela Sofia INFIS e devolve o veredito, o que corrigir e o que não foi verificado." |
| `version` | `1.1.0` |
| `skills[0].name` | "Validação de arquivos da ECF" |
| `skills[0].description` | "Recebe o texto de um arquivo da ECF (Escrituração Contábil Fiscal, do SPED) dos leiautes 7 a 12, anos-calendário 2020 a 2025. Uma ECF é um texto cuja primeira linha começa com \|0000\|LECF\|. Envie uma ECF por pedido, com o arquivo inteiro. O agente identifica o leiaute pelo campo COD_VER e devolve o veredito, os achados com registro, linha e o que corrigir, e o que não foi verificado. Não analisa outros documentos, como NF-e, ECD ou EFD." |
| `skills[0].tags` | `infis`, `sofia`, `ecf`, `sped`, `escrituracao-contabil-fiscal` |
| `skills[0].examples` | "Valide esta ECF e diga o que precisa ser corrigido."; "Esta ECF do ano-calendário 2025 está correta?"; "Quais erros desta ECF impedem a entrega?" |
| `skills[0].inputModes` / `outputModes` | `text/plain` / `application/json` |

A mudança 08 acrescenta à descrição o que a validação estrutural cobre, quando ela existir. Até lá, a descrição não promete validação de registros (REQ-A2A-007).

### D9. Guia para a Sofia (`docs/contratos/sofia-a2a.md`)

Uma seção nova, para quem mantém a Sofia e para o agente validador:

- Como reconhecer uma ECF: a primeira linha começa com `|0000|LECF|`.
- O que enviar: uma ECF por pedido, o texto inteiro, sem o corte de 20.000 caracteres, com `tipo` `text/plain`.
- Limites: 4 MiB de texto; acima de cerca de 4,5 MB de corpo, o Vercel responde `413` sem JSON-RPC, e a Sofia deve tratar como falha técnica.
- Como ler o resultado: `veredito` diz se a ECF está correta; `achados`, o que corrigir; `naoVerificados` e `observacoes`, o que ficou de fora; `idExecucaoAgente`, a consulta detalhada.
- Os códigos de erro de D2.

### D10. O agente validador, depois deste agente

A Sofia leva o resultado deste agente a um agente validador e depois responde ao usuário. Para isso funcionar sem mudar nada aqui:

- O agente não chama o validador nem outro agente (RA-07). Quem encaminha é a Sofia.
- O resultado já é verificável por terceiros: os mesmos `idExecucao` e `idSolicitacao`, e, em cada achado, registro, linha, campo, valor encontrado e fonte com página. `GET /analises/{id}` devolve o envelope completo.
- O contrato SOFIA v1.0 não muda. Campos novos entram só como opcionais.

### D11. Testes (SQ-09, SQ-10)

- **Unitários:** reconhecimento da ECF; tabela de `COD_VER`; marcador de corte; limites de linha; leitura em ISO 8859-1; causa da falha no status.
- **Integração:** os dois canais com Testcontainers e o mesmo texto, conferindo o mesmo resultado (REQ-RECEPCAO-008); ECF com NF-e; nenhuma ECF; duas ECF; corpo acima do limite no REST.
- **Contrato:** cartão servido igual a `docs/contratos/agent-card.json`, exceto a `url`; envelope com os códigos novos.
- **Adversarial:** texto de instrução dentro do campo `NOME` do Registro 0000 sintético não muda etapas, status nem veredito (P10).
- **Dados:** ECF sintéticas mínimas por leiaute em `tests/Api.Agente.Testes/Dados/ecf/`, com um `LEIAME.md` que diz que são sintéticas, o requisito e a página do manual que cada uma representa.

## Dimensionamento

Nenhuma chamada a provedor pago. A leitura de 4 MiB em fluxo é linear no tamanho. A tarefa de verificação em produção mede o tempo de um `SendMessage` com uma ECF sintética perto do limite, para a mudança 10 decidir se a análise continua síncrona (E03).

## Risks / Trade-offs

- **Corte da Sofia:** enquanto existir, arquivo real não chega inteiro. O agente declara a limitação, mas a integração útil depende de quem mantém a Sofia.
- **Limite de 4 MiB:** pode ser pequeno para ECF reais de empresas grandes. Fica medido antes de dados reais, com novo ADR se preciso.
- **Uma ECF por pedido:** a Sofia precisa mandar um pedido por arquivo. A comparação entre anos (09) revê isso.
- **Mudança na regra do status:** pequena, mas toca a mudança 03. Os testes de status existentes continuam valendo, e entram os casos de causa `documento`.

## Migration Plan

1. Migrações `0004_canal` e `0005_causa_da_falha`, com valores para as linhas existentes. O CD migra antes do deploy.
2. Deploy da Api.Agente. O cartão 1.1.0 entra junto.
3. Verificação em produção com ECF sintéticas: cartão, `SendMessage` com ECF, com NF-e, com marcador de corte, e `POST /analises`.
4. Volta: reverter o deploy no Vercel. As colunas novas têm valor padrão e não quebram a versão anterior.

## Open Questions

Nenhuma bloqueante. A medição de ECF reais fica para antes de dados reais (D4).

## Skills aplicáveis

- `sofia-a2a-contrato` e `sofia-a2a-implementacao`: cartão, códigos de erro e guia (D8, D9). Pré-condição: protocolo A2A v1.0 e SQ-12, já vigentes.
- `testes-agentes-ia`: D11.
- `github-versionamento`: branch, commits e PR.

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Atende | Reconhecimento, codificação e `COD_VER` com página do manual de cada leiaute (D2, D3, D5). Nenhuma regra sem fonte. |
| P02 | Atende | Reconhecimento e identificação por código determinístico, sem LLM. |
| P03 | Atende | Etapas 1 a 3 com estado registrado; etapas 4 e 5 não executadas quando o documento está cortado ou a versão não é suportada. |
| P04 | Atende | Documento que não é ECF vai para observações, não vira achado; falha do identificador é falha técnica. |
| P05 | Atende | Nenhum caso desta mudança chega a `concluida`; o status continua calculado pelo sistema (D7). |
| P06 | Atende | Documentos não analisados identificados pelo `idDocumento`. |
| P07 | Não se aplica | Sem LLM nesta mudança. |
| P08 | Atende | Limitações para documento cortado, versão não suportada e identificação impossível. |
| P09 | Atende | Revisão humana segue a regra existente: validação obrigatória não concluída a exige. |
| P10 | Atende | Texto recebido é dado; teste adversarial no Registro 0000 (D11). |
| P11 | Não se aplica | Sem recuperação nesta mudança. |
| P12 | Não se aplica | Sem LLM. |
| P13 | Atende | Identificadores, status e versões preenchidos pelo sistema. |
| P14 | Não se aplica | Sem ferramentas. |
| P15 | Atende | Nenhum acesso à Api.RAG. |
| P16 | Atende | Regras na aplicação e no domínio; endpoints só convertem. Nenhum pacote novo. |
| P17 | Não se aplica | Sem chamada a provedor pago. |
| P18 | Não se aplica | Sem memória. |
| P19 | Atende | Hash, canal, versão identificada e causa da falha registrados. |
| P20 | Atende | Especificação e esclarecimentos antes deste plano; tarefas ligadas a requisitos. |
