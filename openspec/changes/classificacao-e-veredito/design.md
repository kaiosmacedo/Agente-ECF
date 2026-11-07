# Design: Classificação das regras e veredito

## Context

A mudança 03 entregou status, achados com categoria, origem e severidade, e o envelope. Falta traduzir isso na resposta que o usuário espera: a ECF está correta e, se não estiver, o que corrigir. O manual classifica as regras em `Erro` e `Aviso` e em quatro níveis (capítulo 3, página 38).

## Goals / Non-Goals

**Goals:**

- Veredito calculado pelo sistema, separado do status.
- Classe do achado sem criar outra escala.
- Detalhe suficiente para a pessoa localizar e corrigir o erro.

**Non-Goals:**

- Catálogo de regras (08, 09), análise com LLM (10) e relatório legível (DP-15).

## Decisions

### D1. Veredito e status são coisas diferentes (REQ-RESULT-010; P05)

O status responde "a análise rodou até o fim?". O veredito responde "como está a ECF?". Uma ECF pode ser `recusado` com status `parcial`: um erro impeditivo já prova que ela não está correta, mesmo sem as outras validações. O contrário não vale: `conforme` exige status `concluida`.

| Ordem | Condição | Veredito | Significado para o usuário |
| --- | --- | --- | --- |
| 1 | Há achado impeditivo | `recusado` | Recusada na avaliação. Precisa de correção antes de seguir. |
| 2 | Status `entrada-rejeitada` ou `falha-tecnica` | `nao-avaliado` | Não foi possível avaliar. |
| 3 | Status `parcial` | `parcial` | Nada impeditivo até aqui, mas nem tudo foi validado. |
| 4 | Status `concluida`, com achados | `conforme-com-ressalvas` | Nada impeditivo; há pontos a verificar. |
| 5 | Status `concluida`, sem achados | `conforme` | De acordo e verificada nas validações realizadas. |

O cálculo fica em `Execucao.Veredito`, no domínio, como o status. **Alternativa:** novos valores no status, descartada porque misturaria "rodou" com "está correta" e mudaria o mapeamento para o contrato (E01 da mudança 03).

Até a mudança 10 existir, nenhuma ECF chega a `conforme`, porque as etapas 5, 7 e 8 não rodam. A partir da 08, ela já pode ser `recusado`.

### D2. Classe calculada pela severidade (REQ-RESULT-007; GD-05)

| Severidade | Classe | Origem do critério |
| --- | --- | --- |
| `critica` | `impeditiva` | Manual: `Erro` "impede a conclusão com sucesso da funcionalidade" |
| `alta` | `grave` | Produto: não impede, mas precisa de correção |
| `media`, `baixa` | `observacao` | Manual: `Aviso` é "alerta apenas informativo"; produto para os demais |

A classe é calculada, não guardada à parte, então não há duas fontes da mesma informação. A escala de DP-07 não muda.

### D3. Tipo e nível oficiais na regra (REQ-RESULT-008, 009)

`Achado.Criar` recebe, de forma opcional, `RegraDoManual(Codigo, Tipo, Nivel)` e rejeita as combinações proibidas: `Erro` sem `critica`, `Aviso` com `critica`, e `critica` fora de erro estrutural e inconsistência de negócio. Isso impede o LLM de recusar uma ECF sozinho (P02). O nível 1.1 será usado pelas mudanças 07 e 08 para interromper as etapas seguintes.

### D4. Detalhe do achado (REQ-RESULT-011; P06)

`Achado` ganha `Orientacao` e `Fonte(Documento, Versao, Localizacao)`, e `RegistroEnvolvido` ganha `ValorEncontrado`, cortado em 200 caracteres com indicação de corte. O envelope ganha `Veredito` e `Resumo(Impeditivas, Graves, Observacoes)`.

Hoje o banco não guarda nada vindo do arquivo além de hash e tamanho. O valor encontrado muda isso, então a regra de só usar dados sintéticos (SQ-12) precisa continuar valendo até DP-11 decidir retenção e mascaramento. O teste "texto, nome e pergunta não chegam ao banco" continua valendo: o valor é só o do campo citado.

### D5. Banco e contrato (REQ-RESULT-012)

- Migração `0002`: coluna `veredito`, para consulta, e atualização do `jsonb` das execuções já gravadas (`parcial` continua `parcial`; `entrada-rejeitada` e `falha-tecnica` viram `nao-avaliado`).
- `ResultadoAnaliseSofia` ganha `veredito` e `resumoAchados`, e `AchadoSofia` ganha `classe`, `registros`, `orientacao` e `fonte`, todos opcionais, como em D4 da mudança 03.
- A síntese é montada pelo sistema a partir do veredito. Em `conforme`, ela informa que o resultado vale para as validações realizadas e é apoio à conferência (seção 1.3 da constitution).

### D6. Decisões da pesquisa (2026-11-06)

Kaio Macedo aprovou as decisões de `docs/classificacao-das-regras.md`, que ajustam este plano:

- **Não verificado (decisão 4):** campo `naoVerificados`, com item e motivo, fora do cálculo do status. Colocar a lista em limitações deixaria toda ECF em `parcial`, e como achado, toda ECF com ressalva. A lista é fixa, vem da pesquisa e fica vazia em `nao-avaliado`.
- **Domínio (decisão 7):** `TipoDaRegra` ganha "não declarado", porque 19 aplicações do manual não trazem tipo. O nível fica opcional, porque só o 1.1 é declarado. `critica` sem regra só para leiaute. `grave` aciona revisão humana, acima do mínimo de P09. A síntese de `conforme-com-ressalvas` separa graves de observações.
- **Migração `0003`:** preenche `naoVerificados` nas execuções já gravadas.

### D7. Testes

- Domínio: uma linha por condição da tabela de D1 e cada combinação proibida de D3.
- Contrato: round-trip do envelope, os testes da mudança 13 continuam passando, e o cliente A2A real recebe os campos novos.
- Persistência: migração em PostgreSQL real, com uma execução antiga, e valor encontrado cortado.

## Risks / Trade-offs

- [`conforme` entendido como aprovação da Receita Federal] → síntese e documento deixam claro que é apoio à conferência; o nome não usa "aprovado".
- [Valor encontrado no banco] → limite de 200 caracteres e só dados sintéticos até DP-11.
- [Regra classificada errada] → `Erro` e `Aviso` vêm do manual; grave só com aprovação de quem responde pelo domínio (E03).

## Migration Plan

1. Merge e migração pelo CD, com o banco já existente.
2. Campos só opcionais, então a Sofia não precisa mudar nada.
3. **Rollback:** promover o deploy anterior. A coluna nova não afeta a versão anterior.

## Open Questions

- E03: quem responde pelo domínio ao classificar regras como graves.
- DP-11, antes de dados reais.

## Skills aplicáveis

| Skill | Pré-condição | Situação |
| --- | --- | --- |
| `sofia-a2a-contrato` | Campos a mais só opcionais. | Atendida (D5). |
| `testes-agentes-ia` | xUnit e Testcontainers. | Atendida (D6). |
| `github-versionamento` | Branch por mudança e PR. | Atendida. |

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Atende | `Erro`, `Aviso` e níveis citam o manual; o que é do produto está marcado como tal (D2). |
| P02 | Atende | Só validador impede; o LLM não recusa (D3). |
| P03 | Não se aplica | Fluxo não muda. |
| P04 | Atende | Categoria continua única; a classe não a substitui. |
| P05 | Atende | `conforme` só com status `concluida` (D1). |
| P06 | Atende | Regra, registro, linha, campo, orientação e fonte (D4). |
| P07 | Não se aplica | Sem LLM. |
| P08 | Atende | Limitações continuam no resultado. |
| P09 | Atende | Revisão humana continua calculada, à parte do veredito. |
| P10 | Atende | Valor encontrado é dado, nunca instrução. |
| P11 a P12 | Não se aplica | Sem base de conhecimento nem LLM. |
| P13 | Atende | Veredito e classe preenchidos pelo sistema. |
| P14 a P18 | Não se aplica | Sem ferramentas, chamadas entre serviços, provedor ou memória; sem novo pacote. |
| P19 | Atende | Veredito fica registrado na execução. |
| P20 | Atende | Esta mudança; tarefas ligadas aos requisitos. |
