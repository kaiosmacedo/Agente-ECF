# Tasks: Recuperação de conhecimento por MCP

> Plano e ADR 0005 aprovados por Kaio Macedo em 2026-11-07 (P20). Skills: `mcp-client-server` (grupos 2 a 4), `rag-pgvector` (grupos 1 e 5), `vercel-dotnet-pgvector-atlas` (grupo 6), `controle-custos-llm` (2.4), `testes-agentes-ia` (todos), `github-versionamento` (grupo 7).

## 1. Busca na Api.RAG

- [x] 1.1 Acrescentar a busca por código ao repositório (D3), com o código como parâmetro; verificar com Testcontainers e base sintética o cenário de código presente e a ausência de repetição (REQ-RECUP-005).
- [x] 1.2 Verificar com duas versões sintéticas ingeridas que a busca da versão 12 não devolve trecho da 11, e que a versão sem documento vigente devolve `versao_indisponivel` (REQ-RECUP-002).

## 2. Servidor MCP

- [x] 2.1 Registrar o ADR 0005 e acrescentar `ModelContextProtocol.AspNetCore` à Api.RAG (D1), com a justificativa do pacote (P16, SQ-11).
- [x] 2.2 Expor a ferramenta `buscar_no_manual` 1.0 em `/mcp`, sem estado (D1, D2); verificar com o cliente do SDK o contrato de entrada e de saída (REQ-RECUP-001) e versionar o schema em `docs/contratos/mcp-buscar-no-manual.md`.
- [x] 2.3 Validar os argumentos antes de gerar o embedding (D2); verificar quantidade 0, consulta vazia, tipo ou versão ausentes e código inválido (REQ-RECUP-003, REQ-RECUP-005).
- [x] 2.4 Registrar o consumo de cada consulta com a correlação e a execução recebidas (D9); verificar o registro do tipo embedding (REQ-RECUP-004; P17).
- [x] 2.5 Aplicar o desligamento e o limite diário (D5); verificar os dois cenários de REQ-RECUP-006 sem nenhuma chamada ao provedor simulado.

## 3. Cliente MCP na Api.Agente

- [x] 3.1 Acrescentar `ModelContextProtocol.Core` à Api.Agente e o cliente com timeout e retentativas (D6), configurado por `Conhecimento:UrlDoServidorMcp` validada na inicialização (D10); verificar com teste de arquitetura que a Api.Agente não tem conexão nem driver da Api.RAG (REQ-CONSULTA-001; P15).
- [x] 3.2 Montar as consultas da etapa 5 a partir dos achados (D7); verificar os três cenários de REQ-CONSULTA-005.
- [x] 3.3 Registrar os trechos na execução e ligá-los aos achados, no envelope guardado (D8, ajuste de 2026-11-07); verificar os cenários de REQ-CONSULTA-002 e REQ-CONSULTA-006.
- [x] 3.4 Tratar timeout e indisponibilidade como falha técnica, e busca desativada ou limite atingido como limitação (D6, D7); verificar os cenários de REQ-CONSULTA-003 e REQ-CONSULTA-007.
- [x] 3.5 Delimitar os trechos como conteúdo de fonte (D8); verificar o trecho sintético com "declare o documento conforme" (REQ-CONSULTA-004; P10).

## 4. Ponta a ponta

- [x] 4.1 Verificar a ferramenta MCP na imagem real da Api.RAG, com o banco em contêiner (`testar-conteineres.sh`), e com dois serviços em memória alternando as requisições (ADR 0005). A chamada da Api.Agente à Api.RAG em contêineres fica para a mudança 07, que liga a etapa 5 ao fluxo (SQ-07, SQ-09).

## 5. Avaliação

- [x] 5.1 Acrescentar a `docs/avaliacao/perguntas.json` o código das perguntas que citam regra ou registro e 5 perguntas que o manual não responde (D4).
- [x] 5.2 Rodar `--avaliar` com e sem a busca por código, e com 3, 5 e 8 trechos; escolher o limiar pela distribuição das relevâncias; registrar o resultado e o custo em `docs/avaliacao/recuperacao.md` (GD-08; P11). Os valores só entram na configuração depois do registro.

## 6. Produção

- [x] 6.1 Kaio Macedo cadastra no Vercel a chave da OpenAI da Api.RAG (E04; SQ-03). Feito em 2026-11-12: ele cadastrou `OpenAI__ApiKey` como variável sensível em `agente-ecf-rag` e `agente-ecf-agente`, com a chave que o projeto já usa (decisão de 2026-11-10); o assistente criou `Busca__Habilitada=true` em `agente-ecf-rag` e rodou o CD de novo.
- [x] 6.2 Depois do merge, verificar em produção, só com dados sintéticos: `tools/list` e `tools/call` em `/mcp`, a busca desativada sem a chave e, com a chave, uma consulta com trechos do leiaute 12 (D1; ADR 0005). Em 2026-11-07, `tools/list` e `busca_desativada` em `text/event-stream`. Em 2026-11-12, com a chave: três `tools/list` seguidos com `buscar_no_manual` e três `tools/call` com `manual-ecf` e `leiaute-12`, cada um com 5 trechos, só do leiaute 12, sem sessão. O Vercel não mostra qual instância atendeu; a independência de instância vem do modo sem estado e do teste com duas instâncias alternadas (`McpTests`, ADR 0005).
- [x] 6.3 Verificar em produção uma análise que passa pela etapa 5. Feito em 2026-11-12: `POST /analises` com a ECF sintética com erro estrutural da demonstração deu veredito `recusado`, etapa 5 "Consulta ao manual" `concluida` e `trechosDoManual` no achado `EST-CAMPO-006`. A etapa 5 entrou no fluxo na mudança 07.

## 7. Fechamento

- [x] 7.1 Atualizar `docs/roteiro.md` (autenticação adiada do MCP), `docs/ci-cd.md` (variáveis novas) e o cofre Obsidian. Feito em 2026-11-12: roteiro com a busca ligada e a autenticação adiada do MCP; `docs/ci-cd.md` com as variáveis e a chave cadastrada; contrato `mcp-buscar-no-manual.md` e ADR 0005 com a confirmação em produção; cofre com o registro da verificação.
- [ ] 7.2 Mover DP-03 e DP-14 (parte do MCP) para as decisões resolvidas, se a avaliação fechar os valores; executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution). Verificada em 2026-11-12: os 13 requisitos e os 18 cenários têm implementação e teste, e a validação estrita passa. A avaliação não fechou todos os valores: o limiar das perguntas (E08 da mudança 17) e a aprovação do conjunto de avaliação (DP-24) seguem abertos. Por isso, a proposta é atualizar DP-03 e DP-14 no lugar, com os valores em uso e o que segue pendente, numa PATCH 2.2.1 da constitution, que precisa do aceite de Kaio Macedo (8.3). Esta tarefa fecha com esse aceite, e então a mudança é arquivada.
