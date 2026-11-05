# Design: Registro de consumo de IA

## Context

A Api.RAG vai fazer as primeiras chamadas pagas do projeto, os embeddings da mudança 05. P17 e GD-10 exigem registrar cada chamada, inclusive retentativas, sem transformar desconhecido em zero. A Api.Agente ainda não chama nenhum provedor. A Api.RAG também ainda não tem banco.

## Goals / Non-Goals

**Goals:**

- Banco da Api.RAG em produção, com migrações pelo CD.
- Uma linha por chamada ou tentativa, com tokens, custo e a origem de cada valor.
- Consulta agregada, sem dados sensíveis.

**Non-Goals:**

- Faturamento, rateio e alertas de orçamento.
- Registro na Api.Agente: entra na mudança 10, com a primeira chamada dela.

## Decisions

### D1. Banco próprio da Api.RAG (E04; RA-05, P15)

Banco separado no Neon, `agente-ecf-conhecimento`, ligado ao projeto `agente-ecf-rag`. Mesma receita da Api.Agente: Dapper, Npgsql, migrações em SQL embutidas, `--migrar` no CD, conexão obrigatória e validada, GSS desligado e o banco em `/health/ready`. O código é copiado, sem biblioteca compartilhada (ADR 0001). A migração continua fora da inicialização normal.

`cadastrar-banco-neon.sh` passa a receber o serviço como parâmetro, e Kaio Macedo roda o comando antes do merge, porque a conexão passa a ser obrigatória. Segredos: `ConnectionStrings__Conhecimento` no Vercel e `CONEXAO_MIGRACAO_CONHECIMENTO` no GitHub.

### D2. Tabela `consumo_ia` (REQ-CONSUMO-001 a 003; GD-10)

| Coluna | Conteúdo |
| --- | --- |
| `id`, `id_operacao`, `correlacao` | Identificadores. Na ingestão, a operação é a ingestão do documento. |
| `provedor`, `modelo`, `tipo` | `openai`, o modelo e `embedding`, `geracao`, `retentativa` ou `auxiliar`. |
| `status`, `criada_em`, `duracao_ms`, `tentativa` | Resultado, data, duração e número da tentativa. |
| `tokens_entrada`, `tokens_saida`, `tokens_origem` | Nulos quando desconhecidos, nunca zero. Em embeddings, a saída é `nao-aplicavel`. |
| `custo_usd`, `custo_origem`, `tabela_precos_versao` | Custo estimado, ou nulo com origem `desconhecido`, e a versão da tabela. |

### D3. Preços em USD, versionados (E01; DP-13; REQ-CONSUMO-003)

A OpenAI cobra em dólar, e o `SofiaInfis` já usa USD. A tabela fica na configuração, em `Consumo:TabelasDePrecos`, com versão, início da vigência, fonte e preço por modelo. Tabela nova entra por mudança na configuração, e as antigas continuam ali, porque cada estimativa cita a versão usada.

Primeira versão, `2026-11`: `text-embedding-3-small` a US$ 0,02 por milhão de tokens, conferido em 2026-11-05 na [página oficial de preços](https://developers.openai.com/api/docs/pricing). Modelo fora da tabela fica com custo desconhecido.

### D4. Ponto único de medição (REQ-CONSUMO-001, 004)

`IClienteDeEmbeddings` na aplicação e, na infraestrutura, um decorador que mede e grava cada tentativa. As retentativas automáticas do SDK da OpenAI ficam desligadas, e quem repete é o nosso código, senão elas não seriam registradas. Falha ao gravar gera log de erro com a correlação e soma na métrica `agente_ecf.consumo.falhas_registro`, sem interromper a chamada.

### D5. Consulta agregada (E02, E03; DP-26; REQ-CONSUMO-005)

`GET /consumo?de=&ate=&modelo=&idOperacao=` devolve totais por modelo: chamadas, tokens e custo, além de quantas chamadas têm valores desconhecidos, que não entram como zero. Consolidar os dois serviços fica para quando houver necessidade: por enquanto, cada serviço responde pelo seu (P15). Sem restrição de perfil na primeira versão (SQ-12); a consulta não traz texto, prompts nem credenciais.

### D6. Testes

PostgreSQL real com Testcontainers, na imagem `pgvector/pgvector:pg16`, e cliente falso de embeddings: retentativa em duas linhas, tokens desconhecidos, modelo sem preço, estimativa com a versão da tabela, falha ao gravar e consulta sem dados sensíveis. Nenhum teste chama a OpenAI.

## Risks / Trade-offs

- [Preço muda] → a estimativa cita a versão da tabela; a tarifa real é a do painel da OpenAI.
- [Duas cópias do código de banco] → aceito pelo ADR 0001; a Api.Agente recebe o registro de consumo na mudança 10.
- [Conexão obrigatória] → o merge espera o banco existir, como na mudança 03.

## Migration Plan

1. Kaio Macedo roda o script e cria o banco da Api.RAG.
2. Merge; o CD aplica a migração e publica.
3. **Rollback:** promover o deploy anterior.

## Open Questions

Nenhuma bloqueante. Alertas de orçamento podem virar mudança própria.

## Skills aplicáveis

| Skill | Pré-condição | Situação |
| --- | --- | --- |
| `controle-custos-llm` | Fonte, moeda e vigência da tabela (DP-13); consolidação (DP-26). | Atendida (D3, D5). |
| `vercel-dotnet-pgvector-atlas` | ADR de hospedagem; migrações fora da inicialização. | Atendida (D1). |
| `testes-agentes-ia` | xUnit e Testcontainers. | Atendida (D6). |

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 a P12 | Não se aplica | Sem regra de domínio, fluxo ou LLM; só registro de chamadas. |
| P13 | Atende | Tokens, custos e tempos preenchidos pelo sistema (D2). |
| P14 | Não se aplica | Sem ferramentas. |
| P15 | Atende | Banco próprio e consulta por serviço (D1, D5). |
| P16 | Atende | Dapper e Npgsql, os mesmos da Api.Agente. |
| P17 | Atende | Toda tentativa registrada; desconhecido não vira zero (D2, D4). |
| P18 | Não se aplica | Sem memória. |
| P19 | Atende | Registro por chamada, com a versão da tabela de preços. |
| P20 | Atende | Esta mudança; tarefas ligadas aos requisitos. |
