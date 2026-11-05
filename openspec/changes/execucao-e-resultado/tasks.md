# Tasks: Execução da análise e contrato de resultado

Marcar cada tarefa somente com a verificação indicada executada. Plano em `design.md`.

## 1. Domínio da execução

Skill: testes-agentes-ia

- [x] 1.1 Criar `Execucao`, `Etapa` e os estados em `Dominio/Execucoes` (D1); verificar com teste que uma execução nasce com as nove etapas não executadas e identificador gerado pelo sistema (REQ-EXEC-001).
- [x] 1.2 Implementar as dependências entre etapas (D1); verificar com teste que uma falha na etapa 3 deixa as etapas 4 a 8 como não executadas (REQ-EXEC-002).
- [x] 1.3 Exigir justificativa prevista para etapa não aplicável (D1); verificar com teste o caso da etapa 6 e a recusa sem justificativa (REQ-EXEC-003).
- [x] 1.4 Implementar `Achado` com categoria, origem, fundamento e severidade (D1, D3); verificar com testes os cenários de indício com origem validador, erro estrutural com origem LLM, achado sem fundamento e severidade fora da escala (REQ-RESULT-002, REQ-RESULT-003, REQ-RESULT-004).
- [x] 1.5 Implementar o cálculo de status e de revisão humana (D2); verificar com testes uma linha para cada condição, inclusive a recomendação do modelo ignorada (REQ-EXEC-004, REQ-RESULT-005).

## 2. Contrato de resultado

Skill: sofia-a2a-contrato

- [x] 2.1 Criar o envelope versionado e `docs/contratos/envelope-resultado.md` (D4); verificar com teste de round-trip e que os metadados vêm do sistema (REQ-RESULT-001).
- [x] 2.2 Listar limitações e validações não realizadas (D4); verificar com o cenário do manual indisponível (REQ-RESULT-006).
- [x] 2.3 Projetar o resultado para `ResultadoAnaliseSofia`, com os mapeamentos de D2 e os campos opcionais de D4; verificar com testes que os testes de contrato da mudança 13 continuam aprovados e que o cliente A2A recebe os campos opcionais sem quebrar.

## 3. Identidade declarada

Skill: testes-agentes-ia

- [x] 3.1 Criar `IdentidadeDaRequisicao` e `Identidade:OrganizacaoPadrao` validada (D5); verificar com testes que o nome não é persistido nem registrado em log e que a organização padrão é aplicada (REQ-EXEC-001).

## 4. Persistência

Skill: testes-agentes-ia

- [x] 4.1 Adicionar Dapper e Npgsql, o `Migrador`, o script inicial e `ConnectionStrings:Execucoes` validada (D6); verificar com Testcontainers, na imagem `pgvector/pgvector:pg16`, que a migração cria a tabela, que rodar de novo não muda nada, que duas execuções simultâneas não conflitam e que a falta da conexão impede a inicialização sem revelar o valor (REQ-CONFIG-002).
- [x] 4.2 Criar o repositório de execuções (D6); verificar com Testcontainers gravação e leitura completas e que texto, nome e pergunta não aparecem no banco (GD-06, SQ-04).
- [x] 4.3 Tratar retentativas pelo índice único em `idSolicitacao` (D7); verificar com Testcontainers que duas chamadas, em sequência e simultâneas, geram uma só execução.
- [x] 4.4 Incluir o banco em `/health/ready` (D6); verificar com teste 503 sem banco e sem detalhes na resposta (REQ-SAUDE).

## 5. Fluxo e consulta

Skill: sofia-a2a-implementacao

- [x] 5.1 Fazer `ProcessadorDeSolicitacoes` criar e registrar a execução (D4); verificar ponta a ponta com o cliente A2A que a solicitação vira execução com status `parcial`, e que falha do banco termina em tarefa com falha técnica (REQ-A2A-002, REQ-EXEC-001).
- [x] 5.2 Criar `GET /analises/{id}` e publicar `/openapi/v1.json` (D4); verificar com testes 200 com o envelope, 404 para identificador inexistente e a rota no documento OpenAPI (REQ-EXEC-005, GD-01).
- [x] 5.3 Registrar hash, versões e etapas (D8); verificar com teste o cenário de execução auditável (REQ-EXEC-006).

## 6. Entrega

Skill: vercel-dotnet-pgvector-atlas

- [x] 6.1 Aplicar a migração no CD antes do deploy, com `--migrar` (D6); verificar a execução contra um PostgreSQL em contêiner e a alteração em `.github/workflows/cd.yml`.
- [x] 6.2 Ajustar o teste de contêineres para subir com PostgreSQL e adicionar o banco ao `compose.yaml`; verificar no CI e localmente.

## 7. Documentação e decisões

- [x] 7.1 Documentar envelope, status, severidade e configuração; atualizar `docs/contratos/sofia-a2a.md` e o README; verificar que os exemplos correspondem aos testes.
- [x] 7.2 Deixar DP-07, DP-08 e DP-25 prontas para as decisões resolvidas no arquivamento, com DP-15 definida como só JSON por agora; verificar a seção 9 da constitution.

## 8. Neon e produção

Skill: vercel-dotnet-pgvector-atlas

- [ ] 8.1 **(com Kaio Macedo)** Criar o banco no Neon, de preferência pela integração do Vercel com o projeto `agente-ecf-agente`; verificar que o banco existe e usa TLS.
- [ ] 8.2 **(Kaio Macedo)** Cadastrar `ConnectionStrings__Execucoes` no Vercel e o segredo da migração no GitHub Environment `producao`; verificar com `vercel env ls` e no ambiente, sem exibir o valor (SQ-03).
- [ ] 8.3 Depois do merge, verificar que o CD aplicou a migração e publicou, que `/health/ready` responde 200 e que `SendMessage` e `GET /analises/{id}` funcionam em produção com dados sintéticos.

## 9. Fechamento

- [ ] 9.1 Atualizar o cofre Obsidian e `docs/roteiro.md`; verificar a tabela de mudanças.
- [ ] 9.2 Executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution).

## Workflow follow-up

- Arquivar a mudança com `/opsx:archive` após o aceite, criando as specs vigentes em `openspec/specs/agente/`.
- Mover DP-07, DP-08 e DP-25 para as decisões resolvidas da constitution.
