# Base reaproveitada do SofiaInfis

Análise de 2026-11-05 do projeto local `SofiaInfis`: o que serve de base para as próximas mudanças do Agente-ECF. É uma cópia local sem Git, com cerca de 1.270 linhas e nenhum teste. A tabela `conhecimentos` só existe no banco local: o projeto não tem o script de criação nem migrações.

**Regra:** o código é copiado e adaptado dentro da tarefa de cada mudança, sem referência ao projeto nem biblioteca compartilhada (RA-10, P15).

## O que o projeto tem hoje

| Frente | Situação |
| --- | --- |
| RAG | Funciona: ingestão com chunks de tamanho fixo e busca por similaridade com PgVector. Sem filtro por documento ou versão, limiar, índice declarado, retentativas ou registro de consumo dos embeddings. |
| LLM | Chamada direta à OpenAI, com prompt de regras claras e contagem de tokens. Sem saída estruturada. |
| Extração de documentos | PDF com PdfPig e TXT em UTF-8 estrito. |
| Observabilidade | **Só os pacotes.** OpenTelemetry está referenciado, mas nada é configurado no código. Há apenas `ILogger` nos controllers e um cronômetro. O Agente-ECF já está à frente, com logs em JSON, correlação e saúde. |
| A2A, MongoDB, SQL Server, Agent Framework | Pacotes referenciados e sem uso no código. |
| Testes, CI, validação de configuração | Não há. |

## O que reaproveitar

| Item do SofiaInfis | Onde | Como |
| --- | --- | --- |
| Dapper + Npgsql (`ConhecimentoService`) | 03, 04, 05 e 06 | Mesmo acesso a dados nos dois projetos, sem EF Core. Menos dependências e menos tempo de inicialização em contêiner que escala do zero. |
| Busca `embedding <=> CAST(@x AS vector)`, `1 - distância` e `ConverterVetor` com cultura invariante | 06 | Base da consulta. Falta filtro obrigatório por tipo e versão (P11) e limiar para declarar ausência de evidências (P08). |
| Ingestão: embeddings antes da transação, gravação em transação, conferência da dimensão | 05 | Base do fluxo. Falta tabela de documentos, versão, idempotência por hash, timeout e consumo (P17). |
| Chunks de 1200 caracteres com sobreposição de 200 | 05 | Só como valor inicial a avaliar; configurável (P11, GD-08). |
| `text-embedding-3-small`, 1536 dimensões | 05 (DP-02) | Candidato no ADR de embeddings. |
| Extração de PDF com PdfPig (`ExtracaoDocumentoService`) | 05 | Base da leitura do manual, sem truncar e com a versão estável do pacote (0.1.16), não a alfa. |
| `OpenAI:Pricing`: USD por milhão de tokens, separado por modelo e tipo | 04 (DP-13) | Forma da tabela de preços. Hoje nenhum código a lê; os valores precisam ser conferidos com a OpenAI. |
| Regras do prompt: não inventar, separar fato de hipótese, não afirmar o que não executou, tratar documentos como dados, delimitadores de fonte | 10 | Ponto de partida do prompt, versionado (P19) e com saída estruturada. |
| Imagens `pgvector/pgvector:pg16` e `mongo:8.0` | Compose e Testcontainers de 03, 05 e 12 | Mesmas versões no ambiente local e nos testes. |
| `AddOpenApi` / `MapOpenApi` | 03 | Documento OpenAPI da API REST (GD-01), sem Swashbuckle nem Scalar por enquanto. |
| Pacotes OpenTelemetry | 14 | Mesma família, na versão estável 1.20.1. Exportador OTLP em vez do console, que quebraria os logs em JSON (REQ-OBSERV-002). |

## O que não copiar

| No SofiaInfis | Por quê | No Agente-ECF |
| --- | --- | --- |
| Chave da OpenAI em `appsettings.json` | SQ-03 | Armazenamento seguro; a chave nunca vai para arquivo versionado. |
| `Usage?.InputTokenCount ?? 0` | Consumo desconhecido viraria zero (P17) | Campo nulo e estado explícito. |
| Tokens de embedding não contados | P17, GD-10 | Registrar por chamada. |
| Truncar em 20.000 caracteres | Análise parcial sem aviso formal (P08) | Erro controlado ou limitação declarada. |
| Busca sem filtro e sem limiar | Mistura versões de manuais (P11) | Filtro obrigatório e limiar avaliado. |
| `DocumentoReferencia` igual ao identificador do primeiro chunk | Sem rastreabilidade do documento (P06) | Tabela de documentos com versão. |
| Tabela sem migração | Banco não reproduzível | Scripts SQL versionados. |
| Chamadas externas sem timeout nem retentativa | SQ-06 | Timeout e retentativas limitadas. |

## O que afeta a integração com a Sofia

Pontos do código atual da Sofia que impedem um arquivo ECF real de chegar inteiro ao agente:

| Ponto | Efeito | Encaminhamento |
| --- | --- | --- |
| Corta cada arquivo em 20.000 caracteres e acrescenta `[DOCUMENTO TRUNCADO]` | A validação estrutural analisaria um arquivo cortado e acusaria erros falsos. | Mudança 07, E07. |
| Recusa arquivo acima de 5 MB | Mesmo limite prático do Vercel, de cerca de 4,5 MB. | Mudança 07, E02. |
| Lê TXT em UTF-8 estrito | Um arquivo em outra codificação falha na Sofia, antes de chegar ao agente. | Mudança 07, E05: conferir a codificação no manual. |
| Não tem cliente A2A | A integração real ainda não pode ser testada. | Mudança 13, tarefa 7.4. |
