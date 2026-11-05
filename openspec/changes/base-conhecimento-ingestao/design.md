# Design: Base de conhecimento e ingestão de manuais

## Context

A base precisa guardar os manuais dos leiautes 7 a 12 (`docs/fontes-oficiais.md`), com origem, versão e localização. Cada PDF tem de 6 a 7 MB, acima do limite do Vercel, de cerca de 4,5 MB, e gera milhares de chamadas ao banco e dezenas ao provedor. A mudança 04 entrega o banco da Api.RAG e o registro de consumo.

## Goals / Non-Goals

**Goals:**

- Ingestão idempotente, com página e seção em cada chunk, sem chunks parciais como vigentes.
- Popular a base com os seis manuais, com custo registrado.
- Parâmetros de segmentação avaliados antes da população completa (P11, GD-08).

**Non-Goals:**

- Busca pelo agente e MCP (mudança 06).
- Tabelas Dinâmicas no RAG: são dados para os validadores (E01).

## Decisions

### D1. Modelo de embeddings (E02; DP-02; ADR 0004)

`text-embedding-3-small`, 1536 dimensões, distância de cosseno. É o modelo do `SofiaInfis`, escolhido por Kaio Macedo. Modelo e dimensão ficam em cada chunk, e trocar de modelo exige reprocessar os vetores (P11).

### D2. Esquema (GD-07; REQ-INGEST-001, 004, 005)

- `documentos`: identificador, emissor, título, tipo (`manual-ecf`), versão (`leiaute-12`), ano-calendário, versão da publicação, vigência, hash único, endereço de origem, tamanho, páginas, estado (`processando`, `vigente`, `substituido`, `desativado`, `falhou`) e data de ingestão.
- `chunks`: identificador, documento, ordem, página, seção, texto, hash, modelo, dimensão e `embedding vector(1536)`.
- A migração cria a extensão `vector`.
- **Sem índice vetorial (ajuste na implementação):** a busca sempre filtra por tipo e versão (P11), e cada manual tem cerca de 2 mil trechos. A busca exata é rápida nesse tamanho, enquanto um índice HNSW perde resultados com filtro, porque escolhe os candidatos antes de filtrar. Registrado no ADR 0004.

### D3. Extração e segmentação (E05; DP-03; REQ-INGEST-002, 004, 007)

- **Extração:** `PdfPig` 0.1.16, a última versão estável. O `SofiaInfis` usa uma versão alpha. Texto extraído por página, com `ContentOrderTextExtractor`, sem o cabeçalho e o rodapé repetidos. Em teste com o Manual do Leiaute 12, as tabelas de regras saíram legíveis, em 4 segundos. A seção vem dos títulos ("Capítulo", "Seção", "Registro X:", "Bloco X:"), e um trecho novo começa em cada título. O conteúdo é sempre dado (P10).
- **Segmentação:** valor inicial de 1200 caracteres com sobreposição de 200, configurável, sem atravessar páginas, para a página citada estar sempre certa. É só o ponto de partida: os valores finais saem da avaliação de D6.
- **Limites:** só PDF, até 10 MB, recusado antes de extrair.

### D4. Fluxo sem chunks parciais (REQ-INGEST-003, 005, 006)

1. Calcula o hash. Se já houver documento `vigente` com esse hash, devolve o existente.
2. Grava o documento como `processando`, extrai, segmenta e gera os embeddings em lotes, com consumo registrado (mudança 04).
3. Em uma única transação, grava os chunks, torna o documento `vigente` e a versão anterior do mesmo tipo e versão, `substituido`.
4. Em falha, o documento fica como `falhou`, sem chunks. Enviar de novo refaz o processo.

### D5. Como o arquivo chega (E05; SQ-12)

O limite do Vercel recusa o PDF no endpoint, e a função não dura o suficiente para milhares de chunks. Então a população é um comando, como a migração: `dotnet Api.RAG.dll --ingerir <manifesto>`, com o mesmo serviço de aplicação.

- `POST /documentos` existe e segue os requisitos, para documentos menores, mas fica **desativado por configuração** em produção (`Ingestao:EndpointHabilitado`). SQ-12 exige limite de uso ou desativação em endpoint público que aciona provedor pago.
- `docs/fontes/manifesto-rag.json` lista arquivo, hash e metadados de cada manual. O comando recusa arquivo com hash diferente.
- `.github/scripts/popular-rag.sh`, rodado por Kaio Macedo, baixa os PDFs do Release, confere os hashes, lê a chave e a conexão, roda o comando em contêiner e mostra só o resumo: documentos, chunks, tokens e custo. **Alternativa:** enviar o arquivo em partes pelo endpoint, descartada por acrescentar código e ainda esbarrar no limite de duração. **Alternativa:** receber um endereço e baixar no servidor, descartada pelo risco de SSRF e pelo mesmo limite.

### D6. Avaliação antes da carga completa (E03; DP-24; GD-08; P11)

Carrega primeiro só o Leiaute 12. O assistente rascunha 30 perguntas com a página esperada, cobrindo regras, registros e tabelas, um script confere que a evidência está na página, e Kaio Macedo aprova: não há especialista fiscal na equipe por enquanto. Um comando `--avaliar` mede o acerto nos 5 primeiros trechos, com 2 ou 3 combinações de tamanho e sobreposição. O resultado fica em `docs/avaliacao/recuperacao.md`, e os valores aprovados valem para os demais manuais. `topK` e limiares ficam para a mudança 06.

### D7. Chave da OpenAI (E02; SQ-03)

Kaio Macedo autorizou usar a chave do projeto `SofiaInfis` para os embeddings. O script a lê do arquivo daquele projeto na hora de rodar e a passa só para o processo em contêiner, por variável de ambiente. Ela não vai para o repositório, os logs, o Vercel nem o GitHub.

Sem a chave em produção, a Api.RAG não consegue gerar embeddings por lá. A mudança 06 precisa dela no Vercel para a busca, e essa decisão fica para aquele plano.

### D8. Originais (E04)

Os PDFs ficam em um Release privado do repositório, com os hashes de `docs/fontes-oficiais.md`, porque o portal troca o arquivo sem mudar o endereço. Na primeira carga, `popular-rag.sh` baixa dos endereços oficiais, confere os hashes e cria o Release. As Tabelas Dinâmicas entram no Release quando a mudança 08 as usar.

### D9. Administração (REQ-BASE-001 a 003)

`GET /documentos`, `GET /documentos/{id}` e `POST /documentos/{id}/desativar`, sem perfil na primeira versão (SQ-12). Não acionam provedor pago.

### D10. Testes

PostgreSQL real com Testcontainers, cliente falso de embeddings e PDF sintético gerado no teste: metadado ausente, formato, reenvio, chunks com página, nova versão, falha do provedor e instrução embutida. Nenhum teste chama a OpenAI nem depende dos manuais reais.

## Dimensionamento

Estimativa a partir do texto extraído dos seis manuais: 12,2 milhões de caracteres, cerca de 12 mil chunks e 3 milhões de tokens.

| Item | Estimativa | Limite |
| --- | --- | --- |
| Custo dos embeddings | Cerca de US$ 0,06 por carga completa, a US$ 0,02 por milhão de tokens | O valor real fica em `consumo_ia` |
| Espaço no banco | Cerca de 150 MB, com o índice | 1 GB por projeto no plano gratuito do Neon |

## Risks / Trade-offs

- [Tabelas do PDF saem embaralhadas na extração] → a avaliação pega isso com perguntas sobre regras; se falhar, considerar extrair por posição. Os validadores não dependem do RAG.
- [Chave compartilhada com o `SofiaInfis`, em texto puro naquele projeto] → uso limitado à carga; recomendável criar uma chave própria e trocar a atual.
- [Comando roda fora do Vercel] → aceito: é uma operação eventual, feita quando sai manual novo.

## Migration Plan

1. Merge com o endpoint desativado; o CD aplica a migração.
2. Kaio Macedo roda o script com o Leiaute 12; avaliação; ele roda de novo com os demais manuais.
3. **Rollback:** desativar documentos não afeta execuções anteriores.

## Open Questions

- E01 da mudança 06: o MCP funciona no Vercel?

## Skills aplicáveis

| Skill | Pré-condição | Situação |
| --- | --- | --- |
| `rag-pgvector` | ADR de embeddings; parâmetros com plano de avaliação; conjunto de avaliação. | Atendida (D1, D6). |
| `controle-custos-llm` | Mudança 04. | Depende da 04. |
| `testes-agentes-ia` | xUnit e Testcontainers. | Atendida (D10). |

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Atende | Só fontes oficiais, com hash (`docs/fontes-oficiais.md`). |
| P02 a P05 | Não se aplica | Sem validações nem fluxo de análise. |
| P06 | Atende | Página e seção em cada chunk (D2, D3). |
| P07 a P09 | Não se aplica | Sem análise. |
| P10 | Atende | Conteúdo tratado como dado (D3). |
| P11 | Atende | Versão, hash, parâmetros avaliados e troca de modelo com reprocessamento (D1, D3, D6). |
| P12 | Não se aplica | Sem LLM de geração. |
| P13 | Atende | Metadados preenchidos pelo sistema. |
| P14 | Não se aplica | Sem ferramentas. |
| P15 | Atende | Banco próprio da Api.RAG. |
| P16 | Atende | `PdfPig`, para extrair PDF, e o SDK `OpenAI`, para embeddings; os dois já usados no `SofiaInfis`. |
| P17 | Atende | Consumo registrado (mudança 04). |
| P18 | Não se aplica | Sem memória. |
| P19 | Atende | Documento, versão e hash auditáveis; versões antigas mantidas. |
| P20 | Atende | Esta mudança; tarefas ligadas aos requisitos. |
