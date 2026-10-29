---
name: testes-agentes-ia
description: Projetar e executar os testes automatizados do Agente-ECF (.NET 10) com xUnit e Testcontainers, cobrindo validadores, contratos estruturados, REST, MCP, PgVector, MongoDB, Semantic Kernel e testes adversariais. Usar ao escrever cenários de aceite, testes e evidências das mudanças OpenSpec, no plano técnico, nas tarefas e no /opsx:apply.
---

# Testes do Agente-ECF

Adaptada da skill de mesmo nome do pacote INFIS para Spec Kit. Em conflito, `openspec/constitution.md` prevalece. Stack decidida pelo responsável em 2026-10-29: xUnit e Testcontainers (DP-17).

**Mudanças do roteiro** (`docs/roteiro.md`): todas as mudanças (01 a 13); em 07 a 09, as fixtures seguem as localizações extraídas do Manual da ECF. O `tasks.md` indica os grupos de tarefas que usam esta skill.

## Regras transversais

- Ler `openspec/constitution.md`, `openspec/config.yaml` e os artefatos da mudança em `openspec/changes/<id>/` antes de alterar arquivos. Se não existirem, informar e não inventar requisitos.
- Atuar somente nas tarefas aprovadas e citar os `REQ-<CAPACIDADE>-<NNN>` atendidos.
- .NET 10 e ASP.NET Core; separação Api.Agente/Api.RAG (P15); nenhuma biblioteca sem justificativa (P16).
- Nenhum segredo em código, logs, testes ou Git (SQ-03).
- Não inventar leiautes, regras fiscais ou documentos oficiais; uma fixture sintética nunca prova conformidade fiscal (P01).
- Executar `dotnet test` e `openspec validate --strict`; informar o que não foi executado.
- Commits e PRs seguem a skill `github-versionamento`.

## Fluxo de execução

1. Mapear cada requisito e cenário (`#### Scenario`) da mudança para pelo menos um teste; listar requisitos sem teste (DoD-09).
2. Priorizar testes determinísticos: validadores estruturais e de negócio com caso válido e inválido (P02), XSD só para XML com schema oficial, cálculo do status (P05), revisão humana (P09), autorização, limites e erros.
3. Testes unitários usam dublês para OpenAI, MCP e ferramentas. Nenhum assert depende de resposta variável do modelo; o CI não chama a OpenAI (SQ-09).
4. Testes de integração REST e MCP com instâncias reais em contêiner (Testcontainers) para PostgreSQL com PgVector e MongoDB; bancos não são simulados em integração (SQ-09).
5. Fixtures sintéticas ou anonimizadas (SQ-10), cada uma ligada ao requisito e à fonte que representa: arquivo válido, inválido, versão de leiaute incorreta, manual ausente, falha técnica e injeção de prompt.
6. Testes adversariais (P10) sempre que a mudança tocar LLM, RAG, prompts ou ferramentas: instruções embutidas em campos do arquivo, chunks e resultados de ferramentas.
7. Testes de contrato do JSON do LLM e do envelope (P12, P13): resposta válida, malformada e semanticamente inválida, referência fabricada (P07), categoria proibida para a origem (P04), limite de retentativas e metadados inventados pelo modelo.
8. Verificar rastreabilidade das referências RAG (P06, P11), isolamento por usuário e organização (P18), negação de ferramenta sem autorização (P14), limitações declaradas (P08) e consumo desconhecido nunca registrado como zero (P17).
9. Testes de arquitetura (P16, RA-08): o domínio não depende de ASP.NET Core, Semantic Kernel, SDKs de IA, MCP ou drivers de banco; a Api.Agente não referencia dados da Api.RAG (P15).
10. Avaliações de qualidade com o modelo real usam o conjunto aprovado (DP-24), rodam fora do CI e registram custo e variabilidade; não são certificação fiscal.
11. Executar `dotnet test` e relatar testes aprovados, falhos, ignorados e bloqueios, sem falsificar execução.

## Critérios de entrega

- Matriz requisito → teste → evidência no `tasks.md` ou no PR, com comandos reproduzíveis no PowerShell.
- Nenhuma entrega com teste obrigatório falho ou não executado sem ressalva registrada.
