# 0003 — Autenticação e autorização

| Campo | Valor |
| --- | --- |
| Estado | Aceito (decisões R1 a R5 confirmadas por Kaio Macedo em 2026-11-03) |
| Data | 2026-11-03 |
| Responsáveis | Kaio Macedo (decisão), assistente de IA (redação) |
| Decisões pendentes resolvidas | DP-09 |
| Princípios relacionados | P14, P15, P18, P19, SQ-01, SQ-02, SQ-03, SQ-04 |
| Substitui / Substituído por | — |

## Contexto

A constitution exige autenticação em todos os pontos de entrada (SQ-01) e autorização por usuário e organização (SQ-02), e proíbe endpoints funcionais antes de decidir o mecanismo (DP-09). Os principais chamadores serão agentes: a Sofia INFIS, por A2A, e, futuramente, a Maia. Eles atuam por várias organizações clientes da INFIS, que precisam ficar isoladas. Também haverá pessoas: o desenvolvedor do agente, a equipe que cuida da Sofia e, no futuro, usuários de análise. A INFIS usa Microsoft 365.

## Decisão

1. **Protocolo:** OAuth 2.0 / OpenID Connect com tokens JWT. Os serviços validam emissor, audiência, assinatura e validade a partir de configuração, sem bibliotecas específicas de um fornecedor.
2. **Provedor:** Microsoft Entra ID, com um registro de aplicação por API (Api.Agente e Api.RAG) e tokens v2.
3. **Agentes clientes:** a Sofia e a Maia usam registros de aplicação próprios e obtêm tokens por client credentials.
4. **Serviço a serviço:** a Api.Agente obtém token por client credentials para chamar a Api.RAG; a Api.RAG exige o papel `Rag.ServicoAgente`.
5. **Perfis:** papéis de aplicação do Entra mapeados para administrador do agente, operador da orquestradora, cliente agente e usuário de análise (Api.Agente), e administrador e serviço agente (Api.RAG). Papéis desconhecidos não concedem acesso.
6. **Organização:** a identidade vem do token; a organização vem do cabeçalho `X-Organizacao-Id` e só é aceita se a identidade estiver autorizada para ela. Com uma única organização autorizada, o cabeçalho é dispensável.
7. **Autorizações por organização:** em configuração validada na inicialização enquanto os clientes forem poucos; migração para banco quando houver gestão de clientes.
8. **Segurança por padrão:** política de fallback exige autenticação; somente as verificações de saúde são anônimas.

## Alternativas consideradas

| Alternativa | Vantagens | Desvantagens | Motivo da rejeição |
| --- | --- | --- | --- |
| Chaves de API por cliente | Simples de emitir e usar. | Sem expiração, perfis ou identidade padronizada; rotação manual; segredo fixo compartilhado. | Contraria SQ-03 e dificulta auditar quem chamou. |
| `Microsoft.Identity.Web` | Integração pronta com o Entra. | Amarra o código ao fornecedor e acrescenta dependências. | O JwtBearer padrão atende sem acoplamento. |
| Organizações como grupos ou papéis no Entra | Gestão centralizada no provedor. | Um papel por organização e por cliente; tokens maiores; gestão fora do agente. | Mais custo agora; pode ser adotado depois sem mudar o cabeçalho. |
| Um token por organização (troca de token) | Organização assinada pelo provedor. | Exige fluxo de troca de token na Sofia e na Maia. | Complexidade desproporcional para o início. |

## Consequências

- As próximas mudanças apenas declaram perfis e escopo de organização em cada endpoint.
- O Entra ID passa a ser dependência operacional: os registros de aplicação e as variáveis do Vercel precisam existir antes do deploy desta mudança.
- Incluir uma organização ou um cliente exige alterar a configuração e publicar, até a migração para banco.
- A Sofia e a Maia precisam de registros de aplicação próprios e do papel de cliente agente concedido pelo administrador do tenant.

## Verificação

- Testes automatizados de REQ-AUTH-001 a REQ-AUTH-008 com tokens de teste.
- `GET /identidade` em produção com token real, depois do merge.
- Revisão de PR rejeita endpoint funcional sem política declarada ou com dados de identidade vindos do corpo da requisição.
