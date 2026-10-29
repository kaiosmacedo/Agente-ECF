# Proposal: Base de conhecimento e ingestão de manuais

> **Estado:** especificada para validação de Kaio Macedo (roteiro, mudança 05). Plano técnico e tarefas só depois da aprovação e dos esclarecimentos bloqueantes.

## Why

O agente precisa consultar o manual oficial da versão do leiaute identificada (P11) e citar trechos recuperáveis (P06). Para isso, a Api.RAG precisa ingerir manuais com origem, versão e localização, gerar chunks e embeddings em PostgreSQL com PgVector e manter versões substituídas para auditoria (GD-07). É a estrutura do RAG; a recuperação pelo agente vem na mudança 06.

## What Changes

- Cria o endpoint POST de ingestão, administrativo, que recebe um documento oficial com metadados obrigatórios.
- Extrai o texto, segmenta com parâmetros configuráveis, gera embeddings e grava documentos e chunks com metadados de origem, versão e localização.
- Torna a ingestão idempotente por hash e mantém versões substituídas recuperáveis para auditoria, fora das buscas de vigentes.
- Registra o consumo dos embeddings (mudança 04).
- Oferece administração da base: listar, consultar e desativar documentos.
- Ingere a primeira fonte oficial: Manual da ECF, leiaute 12 (DP-04).

## Capabilities

### New Capabilities
- `rag/ingestao-documentos`: recepção de documentos oficiais, segmentação, embeddings e versionamento.
- `rag/administracao-base`: consulta e manutenção dos documentos da base de conhecimento.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | A primeira ingestão é só o Manual da ECF, leiaute 12, ou inclui outros documentos oficiais (tabelas, regras de validação, manuais de anos anteriores) (DP-04)? | — | Kaio Macedo | — | Sim |
| E02 | Qual modelo de embeddings e dimensão (DP-02)? O ADR compara as opções da OpenAI disponíveis na conta. | — | Kaio Macedo | — | Sim |
| E03 | Quem monta o conjunto de avaliação: perguntas sobre o manual e os trechos esperados (DP-24)? | — | Kaio Macedo | — | Sim, para fixar parâmetros |
| E04 | Os PDFs originais devem ser guardados? O disco do Vercel não é durável; seria preciso armazenamento de objetos (por exemplo, Vercel Blob). | — | Kaio Macedo | — | Sim |
| E05 | Tamanho máximo esperado de um documento a ingerir (MB)? | — | Kaio Macedo | — | Não |

## Fontes oficiais

| Documento | Emissor | Versão ou vigência | Localização |
| --- | --- | --- | --- |
| Manual da ECF, leiaute 12 | Receita Federal do Brasil, portal SPED | Publicado em 20/05/2026; ano-calendário 2025 e situações especiais de 2026 (conforme o nome do arquivo) | [arquivo do manual](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/manual_ecf_leiaute_12_20_05_2026_ac_2025_sit_esp_2026.pdf/@@display-file/file) |

A ingestão guarda o documento como dado. Nenhum requisito desta mudança depende do conteúdo do manual.

## Princípios, decisões e dependências

- **Princípios:** P06, P10, P11, P15, P17; GD-07, GD-08; SQ-02, SQ-05.
- **Decisões pendentes:** resolve DP-02 (ADR de embeddings) e DP-03; depende de DP-04 e DP-24.
- **Depende de:** 02 `autenticacao-autorizacao` (perfil administrador) e 04 `registro-consumo-ia`.
- **Skills:** `rag-pgvector`, `controle-custos-llm`, `testes-agentes-ia`, `github-versionamento`.

## Fora do escopo

- Busca pelo agente e ferramentas MCP (mudança 06).
- Documentos que não sejam fontes oficiais.

## Impact

- Banco da Api.RAG (PostgreSQL com PgVector), migrações, endpoint de ingestão e de administração e ADR de embeddings.
- Possível armazenamento de objetos para os originais (E04).

## Aprovação

Pendente da validação de Kaio Macedo.
