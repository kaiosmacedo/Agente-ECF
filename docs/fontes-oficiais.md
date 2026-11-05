# Fontes oficiais da ECF

Registro das fontes que dão origem às regras e à base de conhecimento do Agente-ECF (P01, DP-04). Só o que está aqui pode fundamentar uma regra. Levantamento feito em 2026-11-05, com a página oficial de [manuais e documentos técnicos](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos).

## Leiaute 12 (ano-calendário 2025 e situações especiais de 2026)

Emissor de todos: Receita Federal do Brasil, portal Sped.

| Documento | Versão | Arquivo | SHA-256 |
| --- | --- | --- | --- |
| [Manual de Orientação do Leiaute 12 da ECF, em PDF](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/manual_ecf_leiaute_12_20_05_2026_ac_2025_sit_esp_2026.pdf/@@display-file/file), anexo ao ADE Cofis nº 2/2026 | Atualização de 23/07/2026 ("julho/2026" no próprio documento) | `Manual_ECF_Leiaute_12_23_07_2026_AC_2025_SIT_ESP_2026.pdf`, 6.508.506 bytes, 622 páginas | `f583475c109bf3f2444b8953924503fb0e6fd7767e69c96958703c6ed80286eb` |
| [O mesmo manual, em Word](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/manual_ecf_leiaute_12_20_05_2026_ac_2025_sit_esp_2026.docx/@@display-file/file) | Atualização de 23/07/2026 | `Manual_ECF_Leiaute_12_23_07_2026_AC_2025_SIT_ESP_2026.docx`, 1.508.830 bytes | `942877bdd3d443444f35758e531b1ab30ce965a62ad148707ef2d68061b9f657` |
| [Tabelas Dinâmicas e Planos de Contas Referenciais](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/tabelas-dinamicas-e-planos-de-contas-referenciais-leiaute-12-atualizacao-28-05-2026/tabelas_dinamicas_ecf_leiaute_12_28_05_2026_ac_2025_sit_esp_2026.xlsx/@@display-file/file) | Atualização de 24/09/2026 | `Tabelas_Dinamicas_ECF_Leiaute_12_24_09_2026_AC_2025_SIT_ESP_2026.xlsx`, 1.750.838 bytes, 79 abas | `2a4c9688df7a86c1d9e4c179e8d0547c84b56746742ef7b6a73581fdff3b9ec7` |

## O que cada documento traz

As contagens são automáticas, sobre o texto extraído, e precisam ser conferidas quando o catálogo de regras for montado.

- **Manual:** 18 blocos, 188 registros com seção própria e cerca de 390 regras nomeadas (`REGRA_...`). As tabelas de regras trazem número, código, descrição, "como obter" e tipo (`Erro` ou `Aviso`). A seção 3.1, por exemplo, traz as "Regras de Validação de Estrutura".
- **Tabelas Dinâmicas:** planos de contas referenciais e tabelas por registro (`L100A`, `M300A`, `N630A`, `P150A`, `U100A` e outras). O manual as cita com frequência. São dados estruturados, então servem melhor consultadas por código do que por busca semântica.

## Divergências registradas

- **Versão do manual:** DP-04 e as propostas de 07 e 08 citam a publicação de 20/05/2026. O endereço ainda tem `20_05_2026` no nome, mas hoje entrega a atualização de 23/07/2026. Vale a versão mais recente, até o responsável decidir de outra forma; DP-04 precisa ser atualizada ao ser resolvida.
- **Mesmo endereço, conteúdo novo:** o portal troca o arquivo sem mudar o endereço. Por isso o hash identifica a versão, e o endereço sozinho não basta para auditoria.
- **Duas versões das tabelas:** a página ainda lista a atualização de 29/01/2026, além da de 24/09/2026. Vale a mais recente.

## Outras fontes localizadas, ainda fora do escopo

Entram só por decisão em E01 da mudança 05.

| Fonte | Onde |
| --- | --- |
| Manuais e tabelas dos leiautes 3 a 11, para anos anteriores | [Manuais e documentos técnicos](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos) |
| Programa da ECF, versão 12.2.8, publicada em 19/10/2026: valida e transmite a escrituração (E03 da mudança 08) | [Comunicado](https://www.gov.br/sped/pt-br/assuntos/comunicados/ecf/copy4_of_publicacao-da-versao-12-2-4-do-programa-da-ecf) e [download](https://www.gov.br/receitafederal/pt-br/centrais-de-conteudo/download/sped/ecf) |
| Normas citadas pela página da ECF: IN RFB nº 2.004/2021, IN RFB nº 1.700/2017, Decreto nº 9.580/2018 e Lei nº 12.973/2014 | [Página da ECF](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf) |

## Como atualizar

Ao sair uma nova versão, baixe o arquivo, registre aqui a versão, o tamanho e o hash, e reingira. A ingestão é idempotente por hash e mantém a versão anterior para auditoria (REQ-INGEST-003 e 005).
