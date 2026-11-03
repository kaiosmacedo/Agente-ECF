# Design: Autenticação e autorização

> **Estado:** rascunho aguardando aprovação de Kaio Macedo, junto com as tarefas. As decisões R1 a R5 da proposta foram confirmadas em 2026-11-03 e estão formalizadas no ADR 0003.

## Context

A fundação entregou os dois serviços sem autenticação: só existem `/health/live` e `/health/ready`, anônimos (specs vigentes em `openspec/specs/plataforma/`). As respostas e as recomendações confirmadas estão em `proposal.md`; os requisitos, em `specs/plataforma/autenticacao/spec.md`. Restrições que moldam o desenho:

- Os principais chamadores são agentes (Sofia INFIS e, depois, Maia) que atuam por várias organizações; pessoas usam o administrador e, no futuro, a análise direta.
- Provedor: Microsoft Entra ID, por OAuth 2.0/OIDC, sem amarrar o código ao fornecedor (R1).
- Os serviços rodam no Vercel, sem estado local durável (ADR 0002). Configuração obrigatória ausente impede a inicialização (REQ-CONFIG-002), e isso vale para as novas opções de autenticação.
- Ainda não há endpoints funcionais além de `GET /identidade`, criado aqui; a ingestão (05), a execução (03) e o MCP (06) vão aplicar as políticas desta mudança.

## Goals / Non-Goals

**Goals:**
- Autenticação e autorização prontas para que as próximas mudanças apenas declarem perfis e escopo de organização em cada endpoint.
- Segurança por padrão: endpoint novo exige autenticação, salvo marcação explícita de anônimo.
- Testes sem depender do Entra ID: tokens assinados por chave gerada nos testes.

**Non-Goals:**
- Obter tokens de serviço na Api.Agente para chamar a Api.RAG: entra na mudança 06, que cria o cliente MCP. Aqui, a Api.RAG passa a exigir o papel de serviço.
- Gestão de organizações e autorizações por API ou tela (R4: configuração no início).
- Autenticação do transporte A2A (mudança 13), que reutiliza esta base.

## Decisions

### D1. Validação de tokens JWT (ADR 0003)

- Pacote oficial `Microsoft.AspNetCore.Authentication.JwtBearer` nos dois serviços.
- Opções `Autenticacao`, validadas na inicialização: `Autoridade` (por exemplo, `https://login.microsoftonline.com/<tenant>/v2.0`), `Audiencia` (Application ID URI da API) e `EmissoresValidos` (opcional). O código não usa bibliotecas específicas do Entra; trocar de provedor é configuração.
- Tokens v2 do Entra (`accessTokenAcceptedVersion = 2` nos registros de aplicação).
- **Alternativa:** `Microsoft.Identity.Web`. Descartada: amarra ao Entra e acrescenta dependências que esta mudança não usa.

### D2. Segurança por padrão (REQ-AUTH-001)

- Política de fallback exigindo usuário autenticado: todo endpoint novo nasce protegido.
- `/health/live` e `/health/ready` marcados como anônimos.
- 401 para token ausente, expirado, com assinatura, emissor ou audiência inválidos.

### D3. Perfis a partir dos papéis do provedor (REQ-AUTH-002, REQ-AUTH-007)

Os papéis (app roles) chegam na claim `roles`, tanto para pessoas quanto para aplicações.

| Perfil | Papel no Entra (Api.Agente) | Quem recebe |
| --- | --- | --- |
| Administrador do agente | `Agente.Administrador` | Kaio Macedo |
| Operador da orquestradora | `Agente.OperadorOrquestrador` | Equipe que cuida da Sofia |
| Cliente agente | `Agente.ClienteAgente` | Aplicações da Sofia e da Maia |
| Usuário de análise | `Agente.UsuarioAnalise` | Pessoas, no futuro |

Na Api.RAG: `Rag.Administrador` (administração da base) e `Rag.ServicoAgente` (aplicação da Api.Agente, para o MCP).

- Uma política por perfil (`AdministradorAgente`, `OperadorOrquestrador`, `ClienteAgente`, `UsuarioAnalise`, `AdministradorRag`, `ServicoAgente`). Cada endpoint declara as políticas que aceita.
- Papéis desconhecidos são ignorados: sem papel reconhecido, os endpoints funcionais respondem 403.
- Os nomes dos papéis ficam em constantes de um único arquivo por serviço, para não se espalharem pelo código.

### D4. Identidade do chamador (REQ-AUTH-003, REQ-AUTH-008)

- `IdentidadeChamador` é um registro simples na pasta `Aplicacao` (identificador, tipo pessoa ou aplicação, perfis, organizações autorizadas), sem dependência do ASP.NET Core, para ser usado pelas próximas mudanças (P16).
- A conversão do token para `IdentidadeChamador` fica em `Infraestrutura/Autenticacao`. Identificador: claim `oid` (objeto do usuário ou da aplicação no tenant). Tipo: claim `idtyp` (`app` para aplicações).
- Nenhum dado de identidade vem do corpo, da query ou de outros cabeçalhos.

### D5. Organização por requisição (REQ-AUTH-003, REQ-AUTH-004; ADR 0003)

- Cabeçalho `X-Organizacao-Id`, com identificador opaco de 1 a 64 caracteres entre letras, dígitos, hífen, sublinhado e ponto (E09 define a convenção; o desenho não depende dela).
- Autorizações em configuração validada na inicialização (R4):

```json
"Autorizacoes": {
  "Identidades": [
    { "Id": "<oid da aplicação da Sofia>", "Nome": "Sofia INFIS", "Organizacoes": ["org-a", "org-b"] }
  ]
}
```

- Regras do resolvedor, aplicado aos endpoints com escopo de organização:
  1. Cabeçalho com formato inválido: 400.
  2. Organização informada e autorizada: aceita.
  3. Organização informada e não autorizada: 403.
  4. Sem cabeçalho e uma única organização autorizada: usa essa.
  5. Sem cabeçalho e várias autorizadas: 400, indicando a organização obrigatória.
  6. Sem nenhuma organização autorizada: 403.
- `IdentidadeChamador.PodeAcessar(organizacao)` implementa a regra de REQ-AUTH-004. A resposta 404 para recurso de outra organização é testada na mudança 03, que cria o primeiro recurso com organização.
- `GET /identidade` não exige organização: informa a resolvida quando o cabeçalho é válido e autorizado, e responde 403 se ele indicar organização não autorizada.
- **Alternativa:** organizações como grupos ou papéis no Entra. Descartada por ora: exigiria um papel por organização e por cliente, e não muda o contrato do cabeçalho se for adotada depois.

### D6. Serviço a serviço na Api.RAG (REQ-AUTH-005)

- A Api.RAG valida os tokens com a sua audiência e aplica a política `ServicoAgente` (papel `Rag.ServicoAgente`, tipo aplicação) aos endpoints MCP que a mudança 06 criar.
- Nesta mudança, a política é verificada em teste com um endpoint mapeado somente no ambiente de testes.
- A obtenção do token pela Api.Agente (client credentials, R3) entra na mudança 06, junto com o segredo da aplicação, que o responsável cadastra no Vercel.

### D7. Registro de falhas (REQ-AUTH-006)

- Eventos do JwtBearer (`OnAuthenticationFailed`, `OnChallenge`, `OnForbidden`) e falhas do resolvedor de organização geram log estruturado com motivo, rota e status, dentro do escopo da correlação.
- Os logs nunca recebem o token: o motivo vem do tipo da falha, e as mensagens da biblioteca continuam com dados pessoais ocultos (padrão do IdentityModel).

### D8. Testes sem o Entra ID

- Um gerador de tokens de teste, em cada projeto de testes, assina JWTs com uma chave RSA criada na execução e define `oid`, `idtyp`, `roles`, emissor, audiência e validade.
- A fábrica de testes configura o JwtBearer com essa chave e com opções de teste para `Autenticacao` e `Autorizacoes`.
- Validação real contra o Entra ID: tarefa de verificação em produção com o responsável, depois do merge.

### D9. Configuração em produção

- Variáveis no Vercel por serviço, sem segredos nesta mudança: `Autenticacao__Autoridade`, `Autenticacao__Audiencia` e, na Api.Agente, `Autorizacoes__Identidades__0__Id`, `...__Nome` e `...__Organizacoes__0`.
- Como as opções são obrigatórias, os registros no Entra e as variáveis no Vercel precisam existir **antes do merge**; caso contrário, o deploy sobe serviços que não iniciam. As tarefas do grupo 8 tratam disso com o responsável.

## Risks / Trade-offs

- [Autorizações em configuração exigem novo deploy para incluir organização] → aceitável com poucos clientes (Sofia e Maia); migração para banco prevista quando houver gestão de clientes.
- [Variáveis indexadas no Vercel são propensas a erro] → validação na inicialização com mensagens que citam a opção e `GET /identidade` para conferência.
- [Merge antes da configuração derrubaria a produção] → tarefas de configuração antes do merge e verificação de saúde no CD (que falha o pipeline se os serviços não subirem).
- [Dependência do Entra ID] → padrão OIDC; o código só conhece emissor, audiência e claims padrão.
- [Claim `idtyp` ausente em tokens de alguns provedores] → tipo da identidade é informativo; a autorização depende só de papéis e organizações.

## Migration Plan

1. Kaio Macedo cria os registros de aplicação e papéis no Entra ID (passo a passo em `docs/autenticacao.md`).
2. O assistente cadastra as variáveis não secretas no Vercel e confere com `vercel env ls`.
3. Merge do PR: o CD publica e verifica a saúde.
4. Kaio Macedo obtém um token e chama `GET /identidade` em produção.

Rollback: promover o deploy anterior no Vercel; as variáveis novas não afetam a versão anterior.

## Open Questions

- E09: convenção do identificador de organização (código interno ou CNPJ). Não altera o desenho nem as tarefas.
- Identidade da aplicação da Maia: será incluída nas autorizações quando ela existir, sem mudança de código.

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Não se aplica | Sem regras de domínio fiscal. |
| P02 | Não se aplica | Sem validações de documentos. |
| P03 | Atende | A identidade e a organização resolvidas alimentam a etapa 1 do fluxo (usada na mudança 03). |
| P04 | Não se aplica | Sem achados. |
| P05 | Não se aplica | Sem status de análise. |
| P06 | Não se aplica | Sem achados. |
| P07 | Não se aplica | Sem LLM. |
| P08 | Não se aplica | Sem análise. |
| P09 | Não se aplica | Sem análise. |
| P10 | Atende | Cabeçalho de organização validado como entrada não confiável; identidade nunca vem do corpo (D4, D5). |
| P11 | Não se aplica | Sem base de conhecimento. |
| P12 | Não se aplica | Sem saída de LLM. |
| P13 | Atende | Identidade e organização preenchidas pelo sistema a partir do token e da configuração (D4, D5). |
| P14 | Atende | A política por perfil e organização é a base da autorização independente da escolha do modelo (mudança 11). |
| P15 | Atende | Cada serviço valida a própria audiência; a Api.RAG exige papel de serviço para o MCP (D6). |
| P16 | Atende | Um pacote oficial novo (JwtBearer), justificado em D1; sem biblioteca específica do Entra; identidade em `Aplicacao` sem dependências de infraestrutura. |
| P17 | Não se aplica | Sem chamadas a provedores de IA. |
| P18 | Atende | Base do isolamento por usuário, conversa e organização (D5). |
| P19 | Atende | Identidade e organização disponíveis para o registro de auditoria das execuções (mudança 03). |
| P20 | Atende | Esta mudança; tarefas ligadas aos requisitos. |
