# Proposal: Base de conhecimento e ingestão de manuais

> **Estado:** especificação aprovada por Kaio Macedo em 2026-10-29 (roteiro, mudança 05; PR #4). Esclarecimentos respondidos em 2026-11-05. Plano técnico e tarefas aguardam a aprovação de Kaio Macedo.

## Why

O agente precisa consultar o manual oficial da versão do leiaute identificada (P11) e citar trechos recuperáveis (P06). Para isso, a Api.RAG precisa ingerir manuais com origem, versão e localização, gerar chunks e embeddings em PostgreSQL com PgVector e manter versões substituídas para auditoria (GD-07). É a estrutura do RAG; a recuperação pelo agente vem na mudança 06.

## What Changes

- Cria o endpoint POST de ingestão, administrativo (sem exigir perfil na primeira versão, SQ-12), que recebe um documento oficial com metadados obrigatórios.
- Extrai o texto, segmenta com parâmetros configuráveis, gera embeddings e grava documentos e chunks com metadados de origem, versão e localização.
- Torna a ingestão idempotente por hash e mantém versões substituídas recuperáveis para auditoria, fora das buscas de vigentes.
- Registra o consumo dos embeddings (mudança 04).
- Oferece administração da base: listar, consultar e desativar documentos.
- Ingere as primeiras fontes oficiais: Manuais da ECF, leiautes 7 a 12 (DP-04).

## Capabilities

### New Capabilities
- `rag/ingestao-documentos`: recepção de documentos oficiais, segmentação, embeddings e versionamento.
- `rag/administracao-base`: consulta e manutenção dos documentos da base de conhecimento.

### Modified Capabilities
- Nenhuma.

## Esclarecimentos

| ID | Pergunta | Resposta | Fonte ou responsável | Data | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| E01 | A primeira ingestão é só o Manual da ECF, leiaute 12, ou inclui outros documentos oficiais (tabelas, regras de validação, manuais de anos anteriores) (DP-04)? | Os manuais em PDF dos leiautes 7 a 12 (anos-calendário 2020 a 2025), a janela de retificação citada na seção 1.14 do manual. As Tabelas Dinâmicas ficam como dados para os validadores, fora do RAG. Registro em `docs/fontes-oficiais.md`. | Kaio Macedo | 2026-11-05 | Não |
| E02 | Qual modelo de embeddings e dimensão (DP-02)? O ADR compara as opções da OpenAI disponíveis na conta; o `SofiaInfis` usa `text-embedding-3-small`, com 1536 dimensões. | `text-embedding-3-small`, com 1536 dimensões, registrado em ADR. A chave é a do projeto `SofiaInfis`, autorizada por Kaio Macedo para esse uso: lida na execução, nunca copiada para o repositório ou para os logs (SQ-03). | Kaio Macedo | 2026-11-05 | Não |
| E03 | Quem monta o conjunto de avaliação: perguntas sobre o manual e os trechos esperados (DP-24)? | O assistente rascunha as perguntas e os trechos esperados, com localização no manual, e quem conhece ECF confere. | Kaio Macedo | 2026-11-05 | Não |
| E04 | Os PDFs originais devem ser guardados? O disco do Vercel não é durável; seria preciso armazenamento de objetos (por exemplo, Vercel Blob). | Sim, porque o portal troca o arquivo sem mudar o endereço. Os originais ficam em um Release privado do repositório, sem novo fornecedor. | Kaio Macedo | 2026-11-05 | Não |
| E05 | Tamanho máximo esperado de um documento a ingerir (MB)? | 10 MB. O maior manual da janela tem 7 MB. | Kaio Macedo | 2026-11-05 | Não |

## Fontes oficiais

| Documento | Emissor | Versão ou vigência | Localização |
| --- | --- | --- | --- |
| Manuais da ECF, leiautes 7 a 12 | Receita Federal do Brasil, portal SPED | Versões, tamanhos e hashes em `docs/fontes-oficiais.md`; anos-calendário 2020 a 2025 | `docs/fontes-oficiais.md` |

A ingestão guarda o documento como dado. Nenhum requisito desta mudança depende do conteúdo do manual.

## Princípios, decisões e dependências

- **Princípios:** P06, P10, P11, P15, P17; GD-07, GD-08; SQ-05, SQ-12.
- **Decisões pendentes:** resolve DP-02 (ADR de embeddings) e DP-03; depende de DP-04 e DP-24.
- **Depende de:** 04 `registro-consumo-ia`. O perfil administrador fica adiado (SQ-12; `docs/roteiro.md`).
- **Skills:** `rag-pgvector`, `controle-custos-llm`, `testes-agentes-ia`, `github-versionamento`.
- **Base:** ingestão, extração de PDF e Dapper do `SofiaInfis`, com as adaptações de `docs/base-sofiainfis.md`.

## Fora do escopo

- Busca pelo agente e ferramentas MCP (mudança 06).
- Documentos que não sejam fontes oficiais.

## Impact

- Banco da Api.RAG (PostgreSQL com PgVector), migrações, endpoint de ingestão e de administração e ADR de embeddings.
- Possível armazenamento de objetos para os originais (E04).

## Aprovação

Especificação aprovada por Kaio Macedo em 2026-10-29 (PR #4). Esclarecimentos, plano técnico e tarefas seguem na branch `feat/<id>` da mudança.
