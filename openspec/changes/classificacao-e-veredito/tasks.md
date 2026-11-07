# Tasks: Classificação das regras e veredito

Marcar cada tarefa somente com a verificação indicada executada. Plano em `design.md`.

## 1. Domínio

Skill: testes-agentes-ia

- [x] 1.1 Calcular a classe pela severidade (D2); verificar com testes as quatro severidades (REQ-RESULT-007).
- [x] 1.2 Adicionar tipo e nível oficiais ao achado e rejeitar as combinações proibidas (D3); verificar com testes os cenários de `Erro` com severidade menor, `Aviso` impeditivo e indício impeditivo (REQ-RESULT-008, REQ-RESULT-009).
- [x] 1.3 Calcular o veredito (D1); verificar com testes uma linha para cada condição, inclusive `recusado` com validações pendentes (REQ-RESULT-010).
- [x] 1.4 Adicionar orientação, fonte e valor encontrado (D4); verificar com testes o achado completo e o corte em 200 caracteres (REQ-RESULT-011).

## 2. Envelope e contrato

Skill: sofia-a2a-contrato

- [x] 2.1 Levar veredito, resumo e detalhes ao envelope e à síntese (D4, D5); verificar com round-trip e com a síntese de `conforme` (REQ-RESULT-010, REQ-RESULT-011).
- [x] 2.2 Levar os campos opcionais ao contrato SOFIA (D5); verificar que os testes de contrato da mudança 13 continuam aprovados e que o cliente A2A real recebe os campos novos (REQ-RESULT-012).

## 3. Persistência

Skill: testes-agentes-ia

- [x] 3.1 Criar a migração `0002` (D5); verificar com Testcontainers que ela preenche o veredito de execuções antigas e que rodar de novo não muda nada.
- [x] 3.2 Verificar com Testcontainers que texto, nome e pergunta continuam fora do banco e que só o valor do campo citado é guardado (SQ-04).

## 4. Documentação

- [x] 4.1 Atualizar `docs/contratos/envelope-resultado.md` e `docs/contratos/sofia-a2a.md` com vereditos, classes e detalhes; verificar com o teste que confere o exemplo do documento.
- [ ] 4.2 Atualizar o cofre Obsidian e `docs/roteiro.md`; verificar a tabela de mudanças.

## 5. Decisões da pesquisa

Skill: testes-agentes-ia

- [x] 5.0.1 Criar o campo `naoVerificados` e levá-lo ao contrato SOFIA (D6); verificar com testes que ele não muda status, veredito nem revisão humana e que fica vazio sem avaliação (REQ-RESULT-013).
- [x] 5.0.2 Ajustar o domínio (D6); verificar com testes o tipo não declarado, o nível opcional, `critica` sem regra e `grave` com revisão humana (REQ-RESULT-005, REQ-RESULT-008, REQ-RESULT-009).
- [x] 5.0.3 Registrar as decisões em `docs/classificacao-das-regras.md` e responder as propostas de 07, 08 e 09; verificar com `openspec validate --strict`.

## 6. Fechamento

- [ ] 6.1 Depois do merge, verificar em produção, com dados sintéticos, que `SendMessage` e `GET /analises/{id}` trazem o veredito `parcial`.
- [ ] 6.2 Executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution).

## Workflow follow-up

- Arquivar a mudança com `/opsx:archive` após o aceite.
- Complementar DP-07 e DP-08 nas decisões resolvidas da constitution.
