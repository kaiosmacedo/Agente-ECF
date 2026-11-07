# Tasks: Validação estrutural da ECF

> Plano aprovado por Kaio Macedo em 2026-11-07 (P20). Começam depois da mudança 07 em produção. Skills: `testes-agentes-ia` (todos), `sofia-a2a-contrato` (5.2), `github-versionamento` (grupo 7).

## 1. Catálogo

- [ ] 1.1 Criar `gerar-catalogo.py` (D1), que baixa a versão em Word do Leiaute 12 do endereço oficial e confere o SHA-256 de `docs/fontes-oficiais.md`; verificar a recusa de arquivo com hash diferente.
- [ ] 1.2 Gerar `ecf-leiaute-12.json` e `docs/catalogo/ecf-leiaute-12.md`, com as decisões de D2 registradas no catálogo; verificar 188 registros, 1.122 campos e página em cada um (REQ-ESTRUT-001).
- [ ] 1.3 Testar o catálogo: registro e campo sem página recusados com "fonte ausente"; tipos conhecidos; pai existente para cada filho (REQ-ESTRUT-001).
- [ ] 1.4 Kaio Macedo revisa o `.md` gerado no PR (E02). Correções entram no gerador, nunca no JSON à mão.

## 2. Validadores

- [ ] 2.1 Criar a leitura em uma passada, com o Y800 em várias linhas até `Y800FIM` e o conteúdo depois do 9999 fora da validação (D2, D6); verificar a ordem e a reprodutibilidade dos achados (REQ-ESTRUT-004).
- [ ] 2.2 Implementar `EST-ARQ-001` a `003`; verificar os cenários de REQ-ESTRUT-007.
- [ ] 2.3 Implementar `EST-CAMPO-001` a `006`; verificar os cenários de REQ-ESTRUT-008.
- [ ] 2.4 Implementar `EST-HIER-001`, `EST-OCOR-001`, `EST-OCOR-002`, `REGRA_OCORRENCIA_UNITARIA_ARQ` e `EST-ORDEM-001`; verificar os cenários de REQ-ESTRUT-009.
- [ ] 2.5 Implementar `EST-TOT-001` a `003` e `REGRA_DUPLICIDADE_DESPREZADA`; verificar o cenário de REQ-ESTRUT-010 e a duplicidade por chave.
- [ ] 2.6 Gerar do catálogo, para cada campo, uma linha válida e uma inválida por verificação aplicável; verificar que nenhum campo fica sem verificação (P02; D12).
- [ ] 2.7 Proteger cada regra contra exceção (D8); verificar o cenário de REQ-ESTRUT-006.

## 3. Resultado

- [ ] 3.1 Montar o achado com regra, registro, linha, campo, valor, orientação do catálogo, fonte e classe (D3); verificar o cenário de REQ-ESTRUT-003 e o achado de `IND_SIT_INI_PER` de REQ-ESTRUT-008.
- [ ] 3.2 Aplicar os limites de achados (D4); verificar o cenário de REQ-ESTRUT-013.
- [ ] 3.3 Listar o que não foi verificado e as divergências do manual (D5); verificar os cenários de REQ-ESTRUT-002 e REQ-ESTRUT-011.
- [ ] 3.4 Registrar os indícios de arquivo gerado (D5); verificar o cenário de REQ-ESTRUT-012.
- [ ] 3.5 Deixar a etapa 4 não executada para os leiautes 7 a 11 (D7) e gravar a versão do catálogo no envelope (D1); verificar os dois casos.
- [ ] 3.6 Verificar que nenhuma validação XSD é aplicada ao texto (REQ-ESTRUT-005).

## 4. Qualidade

- [ ] 4.1 Verificar com uma ECF sintética de 4 MiB o tempo da etapa 4 e registrar no PR (D12).
- [ ] 4.2 Verificar que uma instrução escrita num campo `C` não muda achados nem veredito (P10).

## 5. Integração

- [ ] 5.1 Verificar por A2A, com Testcontainers, uma ECF sintética válida (`parcial`, sem achado estrutural) e uma com erro impeditivo (`recusado`, com o que corrigir).
- [ ] 5.2 Atualizar o cartão para a versão 1.2.0 (D11) e a cópia em `docs/contratos/agent-card.json`; verificar com teste de contrato.

## 6. Gabarito

- [ ] 6.1 Preparar as ECF sintéticas com erro plantado e a planilha do erro esperado de cada uma (D10).
- [ ] 6.2 Baixar o programa da ECF do portal oficial e importar nele os arquivos, sem assinar nem transmitir, anotando o que ele aponta (feito pelo assistente, a pedido de Kaio Macedo em 2026-11-07).
- [ ] 6.3 Comparar com o agente e registrar as divergências; resolver antes do arquivamento, no gerador ou como esclarecimento (D10).

## 7. Entrega e fechamento

- [ ] 7.1 Depois do merge, verificar em produção, só com dados sintéticos, a ECF válida e as com erro plantado.
- [ ] 7.2 Atualizar `docs/contratos/sofia-a2a.md`, `docs/fontes-oficiais.md` (catálogo), o cofre Obsidian e `docs/roteiro.md`.
- [ ] 7.3 Atualizar DP-04 com o catálogo e as tabelas pendentes; executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution).
