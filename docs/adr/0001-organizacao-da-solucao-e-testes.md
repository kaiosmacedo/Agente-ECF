# 0001 — Organização da solução e estratégia de testes

| Campo | Valor |
| --- | --- |
| Estado | Aceito (Kaio Macedo, 2026-10-29) |
| Data | 2026-10-29 |
| Responsáveis | Kaio Macedo (decisão), assistente de IA (redação) |
| Decisões pendentes resolvidas | DP-17 |
| Princípios relacionados | P15, P16, RA-01, RA-02, RA-08, RA-10, SQ-08, SQ-09 |
| Substitui / Substituído por | — |

## Contexto

A constitution exige separação entre endpoints, aplicação, domínio e infraestrutura (P16, RA-08), serviços independentes (P15, RA-10) e justificativa para cada projeto ou biblioteca adicional (P16). A organização física e o framework de testes estavam pendentes (DP-17). A mudança `fundacao-solucao` cria a primeira estrutura de código.

## Decisão

1. Solução `Agente-ECF.slnx` com **um projeto por serviço**: `src/Api.Agente` e `src/Api.RAG`, cada um com as pastas e namespaces `Endpoints`, `Aplicacao`, `Dominio` e `Infraestrutura`.
2. Um projeto de testes por serviço: `tests/Api.Agente.Tests` e `tests/Api.RAG.Tests`.
3. **xUnit** como framework de testes, `Microsoft.AspNetCore.Mvc.Testing` para testes HTTP em memória e **Testcontainers** para integração com bancos quando as mudanças os introduzirem.
4. Regras de camadas e de isolamento entre serviços verificadas por testes de arquitetura (`NetArchTest.Rules`), executados no check obrigatório `.NET`.
5. Nenhuma biblioteca compartilhada entre os serviços; versões de pacotes declaradas em cada `.csproj`.

## Alternativas consideradas

| Alternativa | Vantagens | Desvantagens | Motivo da rejeição |
| --- | --- | --- | --- |
| Projetos por camada (`Dominio`, `Aplicacao`, `Infraestrutura`, `Api`) | O compilador impede dependências proibidas. | Oito projetos de produção e mais referências a manter. | Escolha do responsável por simplicidade (P16). |
| Biblioteca comum para correlação e saúde | Sem duplicação de código. | Acopla os serviços e o ciclo de versões. | Pouco código duplicado, coberto por testes nos dois serviços. |
| NUnit | Equivalente ao xUnit. | Diferente do padrão das skills da INFIS. | Escolha do responsável por xUnit. |
| Gerenciamento central de pacotes | Versões num só arquivo. | O contêiner precisaria da raiz do repositório no contexto de build. | Quebraria a paridade com o Root Directory do Vercel (ADR 0002). |

## Consequências

- A estrutura é simples e igual nos dois serviços; as próximas mudanças apenas preenchem as pastas.
- O isolamento do domínio depende dos testes de arquitetura, não do compilador. Um teste de arquitetura falho bloqueia o merge.
- Pacotes comuns aos dois serviços precisam ter a versão atualizada nos dois `.csproj`; o Dependabot ajuda a manter o alinhamento.

## Verificação

- Testes de arquitetura de REQ-QUALIDADE-003 e REQ-QUALIDADE-004 passam no CI.
- Revisão de PR rejeita novo projeto ou biblioteca sem justificativa em design ou ADR.
