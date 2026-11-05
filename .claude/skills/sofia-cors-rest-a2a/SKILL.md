---
name: sofia-cors-rest-a2a
description: Padronizar a configuração segura de CORS, conectividade HTTP e autorização do Agente-ECF com a SOFIA/INFIS em ASP.NET Core .NET 10. Usar quando o agente deve aceitar chamadas REST ou A2A da Sofia, expor o Agent Card, habilitar clientes web autorizados, configurar Docker ou testar preflight OPTIONS, autenticação e integração ponta a ponta.
---

# CORS e integração REST/A2A com a Sofia

Adaptada da skill de mesmo nome do pacote da turma SOFIA/INFIS, escrita para Spec Kit. Em conflito com o fluxo do projeto, `openspec/constitution.md` prevalece.

**Decisões do responsável (2026-11-05):** a rede definitiva é `sofia_network`, do compose da Sofia, cujo nome real no Docker é `sofia-infis_sofia_network`; `agentes-network`, em `references/dotnet.md`, é só o exemplo da turma. A integração não usa token (SQ-12).

**Mudanças do roteiro** (`docs/roteiro.md`): 13 `servidor-a2a` (CORS e conectividade) e 02 `autenticacao-autorizacao` (autenticação, na retomada).

## Objetivo

Adaptar o projeto com alterações mínimas para que a Sofia invoque o agente via A2A e REST, respeitando `sofia-a2a-contrato` e `sofia-a2a-implementacao`.

## Regra arquitetural obrigatória

- Diferenciar **CORS** (política que navegadores aplicam a requisições entre origens) de **acesso backend-backend** (HTTP/A2A entre a Sofia e o agente, sem verificação de CORS). Configurar CORS, isoladamente, não permite conexões A2A.
- Garantir rede, DNS, porta, bind em `0.0.0.0`, TLS quando aplicável, autenticação e autorização do chamador.
- Com frontend da Sofia no navegador, permitir **a origem desse frontend**, não o backend. Origem = esquema + host + porta, sem path e sem barra final.
- Aplicar CORS só aos endpoints com clientes de navegador, com política nomeada e escopo restrito.
- Nunca usar `AllowAnyOrigin()` com `AllowCredentials()` nem `SetIsOriginAllowed(_ => true)` com credenciais. Sem wildcard aberto em produção. **CORS nunca substitui autenticação.**

## O que esta skill define sobre autenticação

- A credencial fica no transporte: autenticação de serviço, por exemplo JWT/OAuth2 com audience e scopes, em HTTPS. Sem credencial válida, endpoint protegido responde 401 ou 403.
- Integrar com a autenticação existente sem desabilitar proteções. Autenticação vem depois de CORS no pipeline.
- **Não define** emissor, valores de audience, scopes nem como a Sofia obtém o token. Isso só precisa ser definido se a mudança 02 for retomada (`docs/roteiro.md`). Enquanto valer a regra SQ-12, os endpoints ficam sem credencial.

## Procedimento no OpenSpec

1. Inspecionar `Program.cs`, `appsettings*.json`, rotas, compose e autenticação existentes; preservar convenções.
2. Registrar na spec: REST e A2A funcionam a partir da Sofia; chamadas de navegador só das origens autorizadas; origem desconhecida não recebe permissão de CORS; endpoints protegidos exigem token.
3. No `design.md`: política `SofiaBrowser`, configuração `Cors:AllowedOrigins` por ambiente, posição dos middlewares, endpoints reais e descoberta do cartão.
4. No `tasks.md`: configuração; testes de `OPTIONS`; testes HTTP sem `Origin`; autenticação; DNS e portas do contêiner; verificação ponta a ponta.
5. Implementar conforme [references/dotnet.md](references/dotnet.md), integrando com o que já existe.
6. Verificar com [references/testes.md](references/testes.md) e reportar evidências, endpoints testados e riscos.

## Contrato funcional que deve permanecer intacto

- Receber da Sofia nome do usuário, pergunta e documentos já convertidos em texto.
- Responder com a análise e os identificadores de correlação (`idExecucao`, `idSolicitacao`).
- Não inventar rotas padronizadas. Alinhar as rotas com o contrato, incluindo `/.well-known/agent-card.json` e o endpoint A2A real.
- CORS não transforma uma rota REST comum em servidor A2A.

## Critérios de aceite

- Com origem permitida, o preflight `OPTIONS` recebe `Access-Control-Allow-Origin` correspondente, métodos e cabeçalhos adequados.
- Com origem não permitida, o preflight não inclui permissão de CORS.
- Requisições backend-backend sem `Origin` passam pelo pipeline normal, sem depender de cabeçalhos CORS.
- GET do cartão e POST A2A/REST trafegam pela rede usada pela Sofia, com resposta conforme o contrato.
- Sem credencial válida, endpoint protegido retorna 401 ou 403 (adiado pela SQ-12).
- Sem documentos fiscais e nomes em logs indiscriminados; usar correlação e telemetria sem dados sensíveis.

## Entregáveis

Trechos alterados, explicação breve das escolhas, comandos de verificação e checklist dos critérios de aceite. Evitar reescrever o projeto quando uma configuração pontual basta.
