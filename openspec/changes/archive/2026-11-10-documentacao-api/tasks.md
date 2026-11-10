# Tasks: Documentação da API com Swagger

> Pedido de Kaio Macedo em 2026-11-10. Implementação pedida por ele em seguida: "Sim, implementar" (P20). Skills: `testes-agentes-ia` (grupos 1 e 2), `github-versionamento` (grupo 3).

## 1. Página e documento

Skill: testes-agentes-ia

- [x] 1.1 Acrescentar `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1 e servir a interface em `/swagger`, apontando para `/openapi/v1.json`, com o validador externo desligado (D1); verificar com teste que `GET /swagger/index.html` responde 200 com HTML e que `GET /swagger` leva à página (REQ-DOCAPI-001).
- [x] 1.2 Trocar `servers` do documento OpenAPI por `/` (D2); verificar com teste que o documento declara `/` e não contém `http://` em `servers` (REQ-DOCAPI-002).

## 2. Testes de segurança e contrato

Skill: testes-agentes-ia

- [x] 2.1 Testar que os scripts e estilos da página são servidos pelo próprio serviço e que a configuração não aponta para o validador externo (REQ-DOCAPI-003).
- [x] 2.2 Testar que uma conexão e uma chave sintéticas configuradas não aparecem no documento (REQ-DOCAPI-003).
- [x] 2.3 Testar que o documento traz `POST /analises`, `GET /analises/{id}`, `POST /a2a` e `GET /.well-known/agent-card.json` (REQ-DOCAPI-004).
- [x] 2.4 Rodar `dotnet test`, `dotnet format --verify-no-changes` e `openspec validate --all --strict`, sem falhas.

## 3. Entrega e fechamento

Skill: github-versionamento

- [x] 3.1 Registrar a capacidade `plataforma/documentacao-api`, o prefixo `REQ-DOCAPI` e a mudança 16 em `docs/roteiro.md`, e o endereço da página no `README.md`.
- [x] 3.2 Depois do merge, conferir em produção que `https://agente-ecf-agente.vercel.app/swagger` abre a página e que um teste de `GET /analises/{id}` com id inexistente chega ao serviço por https. Feito em 2026-11-10, depois do PR #41: `/swagger` redireciona para `swagger/index.html`, que abre com as quatro rotas e o servidor `/`; os arquivos da interface respondem 200; `GET /analises/{id}` com id inexistente responde 404 por https.
- [x] 3.3 Atualizar o cofre Obsidian, executar `/opsx:verify`, conferir a Definition of Done (seção 7 da constitution) e arquivar a mudança. Verificação em 2026-11-10: 9 de 9 tarefas; 4 requisitos e 5 cenários, cada um com teste em `DocumentacaoApiTests`; sem problema crítico.
