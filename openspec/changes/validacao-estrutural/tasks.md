# Tasks: Validação estrutural da ECF

> Plano aprovado por Kaio Macedo em 2026-11-07 (P20). Começam depois da mudança 07 em produção. Skills: `testes-agentes-ia` (todos), `sofia-a2a-contrato` (5.2), `github-versionamento` (grupo 7).

## 1. Catálogo

- [x] 1.1 Criar `gerar-catalogo.py` (D1), que baixa a versão em Word do Leiaute 12 do endereço oficial e confere o SHA-256 de `docs/fontes-oficiais.md`; verificar a recusa de arquivo com hash diferente.
- [x] 1.2 Gerar `ecf-leiaute-12.json` e `docs/catalogo/ecf-leiaute-12.md`, com as decisões de D2 registradas no catálogo; verificar 188 registros, 1.122 campos e página em cada um (REQ-ESTRUT-001).
- [x] 1.3 Testar o catálogo: registro e campo sem página recusados com "fonte ausente"; tipos conhecidos; pai existente para cada filho (REQ-ESTRUT-001).
- [ ] 1.4 Kaio Macedo revisa o `.md` gerado no PR (E02). Correções entram no gerador, nunca no JSON à mão.

## 2. Validadores

- [x] 2.1 Criar a leitura em uma passada, com o Y800 em várias linhas até `Y800FIM` e o conteúdo depois do 9999 fora da validação (D2, D6); verificar a ordem e a reprodutibilidade dos achados (REQ-ESTRUT-004).
- [x] 2.2 Implementar `EST-ARQ-001` a `003`; verificar os cenários de REQ-ESTRUT-007.
- [x] 2.3 Implementar `EST-CAMPO-001` a `006`; verificar os cenários de REQ-ESTRUT-008.
- [x] 2.4 Implementar `EST-HIER-001`, `EST-OCOR-001`, `EST-OCOR-002`, `REGRA_OCORRENCIA_UNITARIA_ARQ` e `EST-ORDEM-001`; verificar os cenários de REQ-ESTRUT-009.
- [x] 2.5 Implementar `EST-TOT-001` a `003` e `REGRA_DUPLICIDADE_DESPREZADA`; verificar o cenário de REQ-ESTRUT-010 e a duplicidade por chave.
- [x] 2.6 Gerar do catálogo, para cada campo, uma linha válida e uma inválida por verificação aplicável; verificar que nenhum campo fica sem verificação (P02; D12).
- [x] 2.7 Proteger cada regra contra exceção (D8); verificar o cenário de REQ-ESTRUT-006.

## 3. Resultado

- [x] 3.1 Montar o achado com regra, registro, linha, campo, valor, orientação do catálogo, fonte e classe (D3); verificar o cenário de REQ-ESTRUT-003 e o achado de `IND_SIT_INI_PER` de REQ-ESTRUT-008.
- [x] 3.2 Aplicar os limites de achados (D4); verificar o cenário de REQ-ESTRUT-013.
- [x] 3.3 Listar o que não foi verificado e as divergências do manual (D5); verificar os cenários de REQ-ESTRUT-002 e REQ-ESTRUT-011.
- [x] 3.4 Registrar os indícios de arquivo gerado (D5); verificar o cenário de REQ-ESTRUT-012.
- [x] 3.5 Deixar a etapa 4 não executada para os leiautes 7 a 11 (D7) e gravar a versão do catálogo no envelope (D1); verificar os dois casos.
- [x] 3.6 Verificar que nenhuma validação XSD é aplicada ao texto (REQ-ESTRUT-005).

## 4. Qualidade

- [x] 4.1 Verificar com uma ECF sintética de 4 MiB o tempo da etapa 4 e registrar no PR (D12): o teste inteiro, com a validação, levou 2,5 s em 2026-11-07.
- [x] 4.2 Verificar que uma instrução escrita num campo `C` não muda achados nem veredito (P10).

## 5. Integração

- [x] 5.1 Verificar por A2A, com Testcontainers, uma ECF sintética válida (`parcial`, sem achado estrutural) e uma com erro impeditivo (`recusado`, com o que corrigir).
- [x] 5.2 Atualizar o cartão para a versão 1.2.0 (D11) e a cópia em `docs/contratos/agent-card.json`; verificar com teste de contrato.
- [x] 5.3 Ligar a etapa 5 ao fluxo depois da etapa 4, com `EtapaDeConsultaAoManual` da mudança 06 (D7 dela), e verificar em contêineres a chamada da Api.Agente à Api.RAG com a busca desligada: etapa 5 não executada e limitação "consulta ao manual desativada" (REQ-CONSULTA-007).

## 6. Gabarito

- [x] 6.1 Preparar as ECF sintéticas com erro plantado e a planilha do erro esperado de cada uma (D10). Corrigidas no PR #28: um erro por arquivo, e o teste `Gabarito` confere a planilha pelo serviço de análise.
- [ ] 6.2 Baixar o programa da ECF do portal oficial e importar nele os arquivos, sem assinar nem transmitir, anotando o que ele aponta (feito pelo assistente, a pedido de Kaio Macedo em 2026-11-07). **Pendente:** o programa da ECF é uma aplicação Java de interface gráfica, sem modo de linha de comando para validar arquivo, e a sessão de 2026-11-07 não tinha o controle da tela (computer use). Fazer numa sessão com Computer use ligado no app do Claude, ou por uma pessoa, com os 12 arquivos de `tests/Api.Agente.Tests/Dados/ecf/gabarito/`, preenchendo a última coluna de `erros-esperados.md`.
- [ ] 6.3 Comparar com o agente e registrar as divergências; resolver antes do arquivamento, no gerador ou como esclarecimento (D10).

## 7. Entrega e fechamento

- [x] 7.1 Depois do merge, verificar em produção, só com dados sintéticos, a ECF válida e as com erro plantado. Feito em 2026-11-07: a primeira rodada, depois do PR #27, achou três defeitos, corrigidos no PR #28 (design, "Correções da verificação em produção"). Na segunda, o cartão está em 1.2.0, os 12 arquivos do gabarito deram exatamente as regras da planilha, com a etapa 4 concluída, de 0,7 a 1,9 s por pedido, e a Api.RAG não fez nenhuma chamada de embedding.
- [x] 7.2 Atualizar `docs/contratos/sofia-a2a.md`, `docs/fontes-oficiais.md` (catálogo), o cofre Obsidian e `docs/roteiro.md`. O exemplo do guia da Sofia é uma chamada real à produção, com a ECF sintética `01-valor-fora-da-lista.txt`.
- [ ] 7.3 Atualizar DP-04 com o catálogo e as tabelas pendentes; executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution).
