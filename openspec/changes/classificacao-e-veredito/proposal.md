# Proposal: Classificação das regras e veredito

> **Estado:** pedida por Kaio Macedo em 2026-11-05. Especificação e plano técnico aguardam a aprovação dele.

## Why

O resultado da mudança 03 diz se a análise rodou e lista achados, mas não responde à pergunta de quem envia o arquivo: a ECF está correta? Kaio Macedo pediu três coisas: separar as regras por gravidade (impeditivas, graves que não impedem e falhas que ficam em observação), ter status que deixem claro o estado da ECF (de acordo e verificada, recusada na avaliação, parcial) e devolver o detalhe de cada erro encontrado.

O manual já dá a base oficial da separação: as regras têm tipo `Erro` ou `Aviso` e nível de validação (Manual do Leiaute 12, capítulo 3, página 38). A divisão entre grave e em observação não vem do manual, então é uma escala do produto.

## What Changes

- Cada achado ganha uma classe, calculada pela severidade: `impeditiva`, `grave` ou `observacao`.
- Achados de regra passam a informar o código, o tipo e o nível oficiais, e `Erro` no manual é sempre `impeditiva`.
- O resultado ganha um veredito calculado pelo sistema: `conforme`, `conforme-com-ressalvas`, `recusado`, `parcial` ou `nao-avaliado`. O status da mudança 03 continua dizendo se a análise rodou.
- Cada achado passa a informar também a orientação de correção, a fonte (documento, versão e localização) e o valor encontrado, e o resultado traz um resumo por classe.
- O contrato SOFIA recebe esses campos só como opcionais, sem mudar os da v1.0.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `agente/contrato-resultado`: classe, veredito e detalhe dos achados.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | Quais os nomes dos vereditos? Kaio Macedo citou "total de acordo e verificado", "recusado na avaliação" e "parcial". | `conforme`, `conforme-com-ressalvas`, `recusado`, `parcial` e `nao-avaliado`. | Kaio Macedo, na aprovação deste plano | — | Não |
| E02 | Quais os nomes das classes? Kaio Macedo citou impeditivas, graves que não impedem e falhas em observação. | `impeditiva`, `grave` e `observacao`, calculadas pela severidade de DP-07, sem criar outra escala (GD-05). | Kaio Macedo, na aprovação deste plano | — | Não |
| E03 | Quem decide quais regras são graves? O manual só separa `Erro` e `Aviso`, e define `Aviso` como "alerta apenas informativo". | Pendente. Até haver responsável, `Erro` é `critica`, e `Aviso` é `media`, ou seja, em observação. Nenhuma regra vira grave sem aprovação de quem responde pelo domínio. É a mesma pergunta de E02 da mudança 08. | Kaio Macedo | — | Não |
| E04 | O achado guarda o valor encontrado no arquivo? Ajuda a corrigir, mas é dado fiscal. | Sim, só o valor do campo, com até 200 caracteres. Hoje só há dados sintéticos (SQ-12). Antes de dados reais, DP-11 precisa decidir retenção e mascaramento. | Kaio Macedo, na aprovação deste plano | — | Não |

## Fontes oficiais

| Documento | Emissor | Versão ou vigência | Localização |
| --- | --- | --- | --- |
| Manual da ECF, leiaute 12 | Receita Federal do Brasil, portal SPED | Atualização de 23/07/2026 (`docs/fontes-oficiais.md`) | Capítulo 3, página 38: tipos `Erro` e `Aviso` e níveis 1.1, 1.2, 2 e 3 |

Só `Erro`, `Aviso` e os níveis vêm do manual. Veredito, classes e critérios de grave e observação são do produto, e a resposta não os apresenta como classificação da Receita Federal.

## Princípios, decisões e dependências

- **Princípios:** P01, P02, P04, P05, P06, P08, P09, P13; GD-02, GD-03, GD-04, GD-05, GD-06; RA-07; SQ-04, SQ-12.
- **Decisões pendentes:** complementa DP-07 e DP-08; não resolve DP-11.
- **Depende de:** 03 `execucao-e-resultado`.
- **Usada por:** 08, 09 e 10, que passam a declarar tipo, nível e severidade de cada regra.
- **Skills:** `sofia-a2a-contrato`, `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- O catálogo de regras e a classificação de cada uma (mudanças 08 e 09).
- Corrigir o arquivo: o agente só orienta a correção (seção 1.3 da constitution).
- Relatório legível em HTML ou PDF (DP-15).

## Impact

- Domínio de execuções, envelope, projeção para a Sofia, uma migração em `execucoes` e os documentos de contrato.

## Aprovação

Aguarda Kaio Macedo.
