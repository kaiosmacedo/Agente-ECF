# Tasks: Registro de consumo de IA

Marcar cada tarefa somente com a verificação indicada executada. Plano em `design.md`.

## 1. Banco da Api.RAG

Skill: vercel-dotnet-pgvector-atlas

- [x] 1.1 Adicionar Dapper, Npgsql, `Migrador`, `--migrar`, `ConnectionStrings:Conhecimento` validada e o banco em `/health/ready` (D1); verificar com Testcontainers a migração, a repetição, a execução simultânea e a recusa sem a conexão.
- [x] 1.2 Aplicar a migração da Api.RAG no CD e subir o banco no teste de contêineres (D1); verificar localmente e no CI.
- [x] 1.3 Fazer `cadastrar-banco-neon.sh` receber o serviço como parâmetro (D1); verificar com ferramentas e valores falsos.

## 2. Registro

Skill: controle-custos-llm

- [x] 2.1 Criar `consumo_ia` e o repositório (D2); verificar com Testcontainers gravação e leitura (REQ-CONSUMO-001).
- [x] 2.2 Criar `IClienteDeEmbeddings` e o decorador, com retentativas no nosso código (D4); verificar com cliente falso que uma falha seguida de sucesso gera duas linhas (REQ-CONSUMO-001).
- [x] 2.3 Registrar tokens desconhecidos como nulos (D2); verificar com o cenário de provedor sem contagem (REQ-CONSUMO-002).
- [x] 2.4 Criar a tabela de preços versionada (D3); verificar com os cenários de modelo sem preço e de estimativa com a versão (REQ-CONSUMO-003).
- [x] 2.5 Tornar observável a falha ao gravar (D4); verificar o log com a correlação e a métrica (REQ-CONSUMO-004).

## 3. Consulta

Skill: testes-agentes-ia

- [x] 3.1 Criar `GET /consumo` (D5); verificar os totais por modelo e a ausência de dados sensíveis (REQ-CONSUMO-005).

## 4. Produção

- [ ] 4.1 **(Kaio Macedo)** Rodar o script para criar o banco da Api.RAG e cadastrar as conexões, antes do merge; verificar pelos nomes, sem exibir os valores (SQ-03).
- [ ] 4.2 Depois do merge, verificar que o CD aplicou a migração e publicou e que `/health/ready` e `GET /consumo` funcionam em produção.

## 5. Fechamento

- [ ] 5.1 Documentar a tabela de preços e a consulta; atualizar o cofre Obsidian e `docs/roteiro.md`.
- [ ] 5.2 Executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution).

## Workflow follow-up

- Arquivar a mudança com `/opsx:archive` após o aceite.
- Mover DP-13 e DP-26 para as decisões resolvidas da constitution.
- Na mudança 10, levar o registro de consumo à Api.Agente.
