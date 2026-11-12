# Verificação da implantação pela skill `sofia-devops-free-cloud`

Em 2026-11-10, a skill `sofia-devops-free-cloud`, da INFIS, foi aplicada ao Agente-ECF, que já estava publicado no Vercel. A skill só orienta, sem scripts, e manda não criar contas nem fazer deploy sem autorização. Nada foi publicado nem alterado nos provedores. Arquivo recebido: `sofia-devops-free-cloud.zip`, SHA-256 `e6393b403c9ddb26f056f77d1093d8275ec3ac9131acc5dda5bad6c82669f7a2`.

## Resultado

Os dois serviços atendem ao roteiro da skill, com quatro ressalvas: o plano Hobby do Vercel, a autenticação adiada pela SQ-12, o MongoDB Atlas ainda não provisionado e a falta de um `.env.example`.

| Item da skill | Situação | Evidência |
| --- | --- | --- |
| .NET 10 em contêiner Linux, com build em estágios | OK | `src/Api.Agente/Dockerfile` e `src/Api.RAG/Dockerfile` |
| Contêiner no Vercel | OK, com outro nome de arquivo | `vercel.json` com `"framework": "container"` e o `Dockerfile` de cada serviço (ADR 0002). A skill sugere `Dockerfile.vercel`; o nome atual funciona e foi mantido |
| Porta dada pela plataforma | OK | `EnderecoDeEscuta.Resolver(builder.Configuration["PORT"])` |
| Sem processo residente nem estado em disco | OK | MCP sem estado (ADR 0005); A2A sem streaming |
| `.dockerignore` | OK | Na pasta de cada serviço |
| `.env.example` sem segredos | Falta | As variáveis estão em `docs/ci-cd.md`; o exemplo entra com a mudança 12, que traz as do MongoDB |
| Saúde pública | OK | Em 2026-11-10, `/health/live` e `/health/ready` deram 200 nos dois serviços, de 1,3 a 2,2 s |
| Agent Card público | OK | Versão 1.2.0, interface JSON-RPC A2A 1.0 em `https://agente-ecf-agente.vercel.app/a2a`, skill `analisar_documentos` |
| PostgreSQL com `vector` no Neon | OK | Base do manual com os embeddings carregados (mudança 05) |
| MongoDB Atlas para a memória | Pendente | Entra com a mudança 12 |
| Segredos fora do Git | OK | `git grep` só acha valores falsos de teste, como `senha-secreta@banco.exemplo` |
| Autenticação A2A e REST (regra 6 da skill) | Adaptação decidida | SQ-12: primeira versão sem token, por decisão de Kaio Macedo em 2026-11-05; só dados sintéticos |
| Plano Hobby (regra 8 da skill) | A confirmar | O time `agent-ecf` está no plano Hobby (`docs/ci-cd.md`), que o Vercel destina a uso pessoal e não comercial |
| Tempo e partida a frio | OK | ECF de 4 MB em até 8,2 s (mudança 07); análise de 0,7 a 2,5 s (mudança 08) |
| Custos da OpenAI | OK | Tabela `consumo_ia` (mudança 04) |
| Rollback | OK | `docs/ci-cd.md`, seção Rollback |

## O que fica com Kaio Macedo

- **Plano do Vercel:** confirmar se a INFIS pode usar o plano Hobby, que o Vercel destina a uso pessoal e não comercial, ou passar o time para um plano pago.
- **MongoDB Atlas (mudança 12):** criar o cluster gratuito, o usuário de banco com permissão mínima e o acesso de rede, e cadastrar a conexão no Vercel. O Vercel não tem IP de saída fixo, então liberar o acesso de qualquer IP exige aprovação explícita, com senha forte e TLS. A skill pede isso, e o assistente não cria contas nem manuseia senhas.
- **Chave da OpenAI em produção:** feito em 2026-11-12. Kaio Macedo cadastrou a variável sensível `OpenAI__ApiKey` nos dois projetos, e a busca no manual (mudança 06) e as perguntas (mudança 17) foram ligadas. A análise com IA (mudança 10) está adiada pelo congelamento de 2026-11-12.
