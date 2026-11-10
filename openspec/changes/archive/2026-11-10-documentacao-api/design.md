# Design: Documentação da API com Swagger

## Context

A Api.Agente gera o documento OpenAPI com `Microsoft.AspNetCore.OpenApi` (`AddOpenApi` e `MapOpenApi` em `Program.cs`), publicado em `/openapi/v1.json` em todos os ambientes. Em 2026-11-10, a produção o servia com quatro rotas (`POST /analises`, `GET /analises/{id}`, `POST /a2a` e `GET /.well-known/agent-card.json`), mas sem página de leitura, e com `servers` igual a `http://agente-ecf-agente.vercel.app/`. O Vercel termina o https e repassa a requisição ao contêiner em http, então o gerador do documento vê `http`.

## Goals / Non-Goals

**Goals:**

- Página do Swagger em `/swagger`, com os arquivos servidos pelo próprio serviço.
- Chamadas de teste da página funcionando em produção, pelo servidor relativo.

**Non-Goals:**

- Página da Api.RAG (E02), autenticação da página (DP-09) e qualquer mudança de rota ou contrato.

## Decisions

### D1. Interface com `Swashbuckle.AspNetCore.SwaggerUI` (REQ-DOCAPI-001, 003)

O pacote `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1 serve só a interface do Swagger, com os arquivos embutidos na própria biblioteca, e aponta para o documento que já existe: `UseSwaggerUI` com `SwaggerEndpoint("/openapi/v1.json", ...)` e `RoutePrefix = "swagger"`. Ele não gera documento nem substitui o `Microsoft.AspNetCore.OpenApi`.

- **Por que:** o pedido é a interface do Swagger; o pacote é o mais usado em ASP.NET Core, tem licença MIT, suporte ao .NET 10 e nenhuma dependência extra (P16, SQ-11).
- **Alternativas descartadas:** uma página estática que carrega a interface de uma CDN, que contraria REQ-DOCAPI-003 e põe código de terceiro no navegador a cada acesso; o Scalar, que não é a interface do Swagger pedida.
- **Validador on-line desligado:** a interface consulta, por padrão, um validador externo para mostrar um selo. A configuração o desliga, para que nenhuma requisição saia da página para outro domínio (REQ-DOCAPI-003).

### D2. Servidor relativo (REQ-DOCAPI-002)

Um transformador do documento OpenAPI troca `servers` por um único servidor `/`. A página faz as chamadas de teste no mesmo esquema e host em que foi aberta: https em produção, http no ambiente local.

- **Alternativa descartada:** ler `X-Forwarded-Proto` com `UseForwardedHeaders`. Funciona, mas exige confiar em cabeçalhos de qualquer origem, porque o Vercel não publica os IPs do proxy, e mexe no pipeline inteiro por causa de um detalhe do documento.

### D3. Segurança (E01; SQ-12)

A página fica pública, como as rotas que ela descreve. Ela não acrescenta rota de negócio nem expõe configuração: o documento vem dos endpoints, e um teste confere que uma conexão e uma chave sintéticas configuradas não aparecem nele. A skill `sofia-devops-free-cloud` pede avaliar a autorização antes de expor o Swagger em produção: a avaliação é esta, e a autenticação da página acompanha a da API quando DP-09 for resolvida.

### D4. Testes (SQ-09; skill `testes-agentes-ia`)

Testes HTTP em memória com `FabricaApi`, como em `SaudeTests`:

| Teste | Requisito |
| --- | --- |
| `GET /swagger` leva à página, e `GET /swagger/index.html` responde 200 com HTML | REQ-DOCAPI-001 |
| A configuração da página aponta para `/openapi/v1.json` | REQ-DOCAPI-001 |
| `servers` do documento é `/`, sem `http://` | REQ-DOCAPI-002 |
| Os scripts e estilos referenciados pela página são caminhos relativos ao serviço, e o validador externo está desligado | REQ-DOCAPI-003 |
| Conexão e chave sintéticas não aparecem no documento | REQ-DOCAPI-003 |
| As quatro rotas públicas, com os métodos, estão no documento | REQ-DOCAPI-004 |

## Risks / Trade-offs

- [Página pública facilita descobrir as rotas] → as rotas já são públicas e documentadas no repositório; a primeira versão só recebe dados sintéticos (SQ-12).
- [Atualizações do pacote mudarem a interface] → versão fixa, atualizada pelo Dependabot com CI.
- [O documento OpenAPI não descrever todo o contrato A2A] → a página mostra o que o documento já tem; o contrato completo continua em `docs/contratos/sofia-a2a.md`.

## Migration Plan

1. Merge e deploy pelo CD.
2. Conferir em produção que `/swagger` abre a página e que um teste de `GET /analises/{id}` com um id inexistente chega ao serviço por https.
3. **Rollback:** promover o deploy anterior no Vercel; a mudança não tem migração.

## Skills aplicáveis

| Skill | Uso | Pré-condições |
| --- | --- | --- |
| `testes-agentes-ia` | Testes HTTP | — |
| `github-versionamento` | Branch `feat/documentacao-api`, PR e merge | — |

Nenhum ADR novo: a mudança não altera arquitetura, hospedagem nem contrato.

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Não se aplica | Sem regra de domínio. |
| P02 | Não se aplica | Sem validação nem regra de negócio. |
| P03 | Não se aplica | O fluxo de análise não muda. |
| P04 | Não se aplica | Sem resultado novo. |
| P05 | Não se aplica | O status não muda. |
| P06 | Não se aplica | Sem achado novo. |
| P07 | Não se aplica | Sem LLM. |
| P08 | Não se aplica | Sem validação nova. |
| P09 | Não se aplica | Sem decisão de revisão humana. |
| P10 | Atende | O documento descreve rotas; nenhum conteúdo recebido entra na página. |
| P11 | Não se aplica | Sem RAG. |
| P12 | Não se aplica | Sem saída de LLM. |
| P13 | Não se aplica | Sem metadados novos. |
| P14 | Não se aplica | Sem ferramentas. |
| P15 | Atende | Só a Api.Agente muda; nenhum dado de outro serviço. |
| P16 | Atende | Um pacote, só de interface, justificado em D1; a configuração fica em `Program.cs`, fora do domínio. |
| P17 | Não se aplica | Sem chamada a provedor de IA. |
| P18 | Não se aplica | Sem memória. |
| P19 | Não se aplica | A execução não muda. |
| P20 | Atende | Proposta, spec, design e tarefas antes da implementação, que Kaio Macedo pediu em seguida. |
