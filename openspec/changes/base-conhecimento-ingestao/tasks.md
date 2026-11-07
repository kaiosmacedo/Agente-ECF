# Tasks: Base de conhecimento e ingestão de manuais

Marcar cada tarefa somente com a verificação indicada executada. Plano em `design.md`. Depende da mudança 04.

## 1. Esquema

Skill: rag-pgvector

- [x] 1.1 Registrar o ADR 0004 de embeddings (D1); verificar o índice de ADRs.
- [x] 1.2 Criar a extensão `vector`, `documentos` e `chunks` (D2); verificar com Testcontainers a migração e as restrições de metadados obrigatórios.

## 2. Ingestão

Skill: rag-pgvector

- [x] 2.1 Validar metadados, formato e tamanho (D3); verificar os cenários de versão ausente e formato não aceito (REQ-INGEST-001, REQ-INGEST-002).
- [x] 2.2 Extrair por página e segmentar (D3); verificar com PDF sintético que cada chunk informa documento, versão, página, modelo e dimensão (REQ-INGEST-004).
- [x] 2.3 Implementar o fluxo de D4; verificar os cenários de reenvio, nova versão e provedor indisponível (REQ-INGEST-003, REQ-INGEST-005, REQ-INGEST-006).
- [x] 2.4 Verificar com documento sintético que uma instrução embutida fica só como conteúdo (REQ-INGEST-007; P10).
- [x] 2.5 Criar `POST /documentos`, desativado por configuração em produção (D5); verificar os dois estados (SQ-12).
- [x] 2.6 Criar `--ingerir` e o manifesto (D5); verificar a recusa de arquivo com hash diferente.

## 3. Administração

Skill: testes-agentes-ia

- [x] 3.1 Criar a listagem, a consulta e a desativação (D9); verificar o filtro por versão, o documento ingerido e a desativação sem exclusão (REQ-BASE-001 a REQ-BASE-003).

## 4. Avaliação

Skill: rag-pgvector

- [ ] 4.1 Rascunhar as 30 perguntas com a página esperada, a partir do Leiaute 12 (D6); verificar por script que a evidência está na página; **Kaio Macedo aprova**, como decidiu em 2026-11-05.
- [x] 4.2 Criar `--avaliar` (D6); verificar com base sintética que o cálculo de acerto está correto.

## 5. População

Skill: vercel-dotnet-pgvector-atlas

- [ ] 5.1 Publicar os PDFs no Release privado (D8), com `publicar-fontes.sh`, conferindo os hashes dos arquivos oficiais; verificar o Release e os hashes baixados de volta.
- [x] 5.2 Criar `popular-rag.sh` (D5, D7); verificar com ferramentas e valores falsos que nenhum valor aparece na saída e que a chave não vai para a linha de comando.
- [ ] 5.3 Depois do merge, **(Kaio Macedo)** rodar o script com o Leiaute 12; verificar o documento, os chunks e o custo em `consumo_ia`.
- [ ] 5.4 Rodar a avaliação e registrar o resultado em `docs/avaliacao/recuperacao.md` (GD-08); **Kaio Macedo aprova os parâmetros**.
- [ ] 5.5 **(Kaio Macedo)** Rodar o script com os leiautes 7 a 11; verificar os seis documentos vigentes e rodar de novo para confirmar que nada duplica.

## 6. Fechamento

- [ ] 6.1 Documentar a ingestão e a atualização de manuais; atualizar o cofre Obsidian e `docs/roteiro.md`.
- [ ] 6.2 Executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution).

## Workflow follow-up

- Arquivar a mudança com `/opsx:archive` após o aceite.
- Mover DP-02 (embeddings), DP-03 (segmentação), DP-04 e DP-24 para as decisões resolvidas, no que couber, e atualizar a versão do manual citada em DP-04.
