# Tasks: Telemetria dos serviços

Marcar cada tarefa somente com a verificação indicada executada. Plano em `design.md`.

## 1. Traces e métricas

Skill: testes-agentes-ia

- [x] 1.1 Adicionar os pacotes de D1 e registrar a telemetria nos dois serviços (D3, D6), com o nome e a versão de `IdentidadeServico`; verificar com testes em cada serviço que a requisição gera trace e que `GET /health/live` não gera (REQ-OBSERV-004).
- [x] 1.2 Verificar com teste que o `traceparent` recebido continua no trace e aparece nos logs da requisição (REQ-OBSERV-004).
- [x] 1.3 Verificar com teste a métrica `http.server.request.duration` por rota e status (REQ-OBSERV-005).
- [x] 1.4 Incluir as fontes e os medidores do SDK A2A na Api.Agente, conferindo os nomes no pacote instalado (D3); verificar com teste que `SendMessage` gera os spans do SDK.

## 2. Exportação

Skill: testes-agentes-ia

- [x] 2.1 Criar a opção `Telemetria:Endpoint` validada e registrar o exportador OTLP só com ela (D5); verificar com testes o serviço sem a opção e a inicialização recusada com endereço inválido, sem revelar o valor (REQ-OBSERV-006).
- [x] 2.2 Verificar com teste que um destino indisponível não afeta a requisição (REQ-OBSERV-006).

## 3. Dados sensíveis

Skill: testes-agentes-ia

- [x] 3.1 Verificar com teste que nenhum atributo de trace contém query string, `Authorization`, nome do usuário ou texto de documento, inclusive em `SendMessage` (REQ-OBSERV-007).

## 4. Painel local e documentação

- [x] 4.1 Adicionar o perfil `observabilidade` ao `compose.yaml` (D7); verificar localmente que uma chamada a `/a2a` aparece no painel.
- [x] 4.2 Documentar em `docs/ci-cd.md` e no README a opção e o painel local; verificar os comandos citados.

## 5. Fechamento

- [x] 5.1 Depois do merge, verificar que o CD publicou, que a saúde continua 200 e que os logs seguem em JSON.
- [x] 5.2 Atualizar o cofre Obsidian e `docs/roteiro.md`; executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution).

## Workflow follow-up

- Arquivar a mudança com `/opsx:archive` após o aceite, atualizando a spec vigente `openspec/specs/plataforma/observabilidade/`.
