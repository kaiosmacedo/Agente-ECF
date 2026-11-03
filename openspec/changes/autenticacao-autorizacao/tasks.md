# Tasks

> **Estado:** rascunho aguardando aprovação de Kaio Macedo. Tarefas marcadas **(com Kaio Macedo)** exigem a conta do responsável no Microsoft Entra ID; o assistente conduz o passo a passo. O merge só acontece depois do grupo 8, para que a produção tenha a configuração obrigatória.

## 1. Base de autenticação

Skill: testes-agentes-ia

- [ ] 1.1 Adicionar `Microsoft.AspNetCore.Authentication.JwtBearer` aos dois serviços e as opções `Autenticacao` (Autoridade, Audiencia, EmissoresValidos) validadas na inicialização (D1); verificar com teste que a ausência de `Autenticacao:Autoridade` impede a inicialização citando a opção (REQ-AUTH-001).
- [ ] 1.2 Criar em cada projeto de testes o gerador de tokens de teste e ajustar a fábrica de testes com chave, emissor, audiência e autorizações de teste (D8); verificar que os 52 testes existentes continuam aprovados.
- [ ] 1.3 Configurar a política de fallback autenticada e marcar `/health/live` e `/health/ready` como anônimos (D2); verificar com testes 401 para token ausente, expirado, com assinatura inválida, emissor errado e audiência errada, e 200 nas verificações de saúde sem token (REQ-AUTH-001).

## 2. Perfis e políticas

Skill: testes-agentes-ia

- [ ] 2.1 Mapear a claim `roles` para os perfis do agente e da Api.RAG, com os nomes dos papéis em constantes únicas por serviço, e criar uma política por perfil (D3); verificar com teste que um token só com `perfil-desconhecido` recebe 403 (REQ-AUTH-007).
- [ ] 2.2 Verificar com teste que um cliente agente recebe 403 numa rota protegida pela política de administrador da base, sem executar a operação (REQ-AUTH-002), usando endpoint mapeado só nos testes.

## 3. Identidade e organização

Skill: testes-agentes-ia

- [ ] 3.1 Criar `IdentidadeChamador` em `Aplicacao` e a conversão do token em `Infraestrutura/Autenticacao`, a partir de `oid`, `idtyp` e `roles` (D4); verificar que os testes de arquitetura continuam aprovados e que um `usuario` informado no corpo não altera a identidade (REQ-AUTH-003).
- [ ] 3.2 Criar as opções `Autorizacoes` validadas na inicialização (identificador obrigatório, organizações com formato válido) (D5); verificar com teste que uma entrada sem identificador impede a inicialização citando a opção (REQ-AUTH-003).
- [ ] 3.3 Implementar o resolvedor de organização pelo cabeçalho `X-Organizacao-Id` com as seis regras de D5; verificar com testes os cenários de REQ-AUTH-003: várias autorizadas com `org-b`, `org-c` não autorizada (403), sem cabeçalho com várias (400), única organização sem cabeçalho e formato inválido (400).
- [ ] 3.4 Implementar `IdentidadeChamador.PodeAcessar` e verificar com teste unitário que uma identidade autorizada só para `org-b` não acessa recurso de `org-a` (REQ-AUTH-004); registrar no `tasks.md` da mudança 03 o teste HTTP 404 correspondente.

## 4. Consulta da identidade

Skill: testes-agentes-ia

- [ ] 4.1 Criar `GET /identidade` na Api.Agente, aceito para qualquer perfil reconhecido, devolvendo identificador, tipo, perfis, organizações autorizadas e organização resolvida (D5); verificar com teste o cenário da Sofia com `org-a` e que a resposta não contém o token (REQ-AUTH-008).
- [ ] 4.2 Documentar o endpoint no README e em `docs/autenticacao.md` com exemplos para a Sofia e a Maia; verificar que o exemplo documentado corresponde à resposta do teste.

## 5. Serviço a serviço na Api.RAG

Skill: mcp-client-server

- [ ] 5.1 Criar na Api.RAG a política `ServicoAgente` (papel `Rag.ServicoAgente`, identidade de aplicação) (D6); verificar com endpoint mapeado só nos testes: sem token, 401; token de aplicação sem o papel, 403; token com o papel, 200 (REQ-AUTH-005).

## 6. Registro de falhas

Skill: testes-agentes-ia

- [ ] 6.1 Registrar falhas de autenticação, autorização e organização com motivo, rota e status no escopo da correlação (D7); verificar com teste que `Bearer token-invalido-789` gera log de falha com o identificador de correlação e que nenhum registro contém o token (REQ-AUTH-006).

## 7. Documentação e decisões

- [ ] 7.1 Escrever `docs/autenticacao.md`: papéis, registros de aplicação, variáveis por serviço, formato das autorizações, uso do cabeçalho `X-Organizacao-Id` e obtenção de token por client credentials; verificar que os nomes de papéis e opções coincidem com o código.
- [ ] 7.2 Conferir o ADR 0003 no índice de `docs/adr/README.md` e deixar DP-09 pronta para ir às decisões resolvidas no arquivamento; verificar os links do índice.

## 8. Entra ID e produção

Skill: vercel-dotnet-pgvector-atlas

- [ ] 8.1 **(com Kaio Macedo)** Criar no Entra ID os registros de aplicação da Api.Agente e da Api.RAG, com Application ID URI, papéis de D3 e `accessTokenAcceptedVersion = 2`, e atribuir a Kaio Macedo os papéis de administrador; verificar os registros no portal.
- [ ] 8.2 **(com Kaio Macedo)** Conceder o papel `Agente.ClienteAgente` à aplicação da Sofia (com consentimento do administrador) e definir as organizações iniciais dela; verificar o identificador `oid` da aplicação da Sofia.
- [ ] 8.3 Cadastrar no Vercel as variáveis `Autenticacao__*` dos dois projetos e `Autorizacoes__*` da Api.Agente (valores não secretos); verificar com `vercel env ls` e com a validação de inicialização num contêiner local ou no CI.
- [ ] 8.4 Depois do merge, verificar que o CD publicou e que a saúde continua 200; com um token real de Kaio Macedo, verificar `GET /identidade` em produção (REQ-AUTH-001, REQ-AUTH-008).

## 9. Fechamento

- [ ] 9.1 Atualizar a nota da mudança e o registro no cofre Obsidian e o estado em `docs/roteiro.md`; verificar a tabela de mudanças.
- [ ] 9.2 Executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution); verificar que os itens aplicáveis estão atendidos ou justificados no PR.

## Workflow follow-up

- Arquivar a mudança com `/opsx:archive` após o aceite, criando a spec vigente `openspec/specs/plataforma/autenticacao/`.
- Mover DP-09 para as decisões resolvidas da constitution, com o ADR 0003.
