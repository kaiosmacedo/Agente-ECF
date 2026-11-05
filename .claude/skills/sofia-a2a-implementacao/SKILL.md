---
name: sofia-a2a-implementacao
description: Implementar e integrar o Agente-ECF ao orquestrador SOFIA da INFIS pelo protocolo Agent2Agent (A2A) em .NET 10. Usar ao criar ou alterar o servidor A2A, o Agent Card, os fluxos de tarefas, os testes de interoperabilidade e os planos ou tarefas de integração.
---

# Implementação A2A para a Sofia

Adaptada da skill de mesmo nome do pacote da turma SOFIA/INFIS, escrita para Spec Kit. Em conflito com o fluxo do projeto, `openspec/constitution.md` prevalece.

**Mudanças do roteiro** (`docs/roteiro.md`): 13 `servidor-a2a`. O Agente-ECF é **servidor** A2A; o cliente A2A, o registry, o despacho e a agregação são da Sofia e ficam fora deste repositório.

## Objetivo

Implementar uma integração **A2A real**, em que a Sofia é cliente e orquestradora e o Agente-ECF é servidor A2A. Não substituir o protocolo por um POST REST arbitrário. Adotar **A2A v0.3.x JSON-RPC** como perfil fixado para a turma, até decisão explícita de migração; seguir a [especificação oficial](https://a2a-protocol.org/v0.3.0/specification/) e confirmar o SDK .NET disponível antes de escrever código.

## Ler primeiro

- `references/arquitetura-e-criterios.md`: requisitos e critérios.
- A skill complementar `sofia-a2a-contrato` e o contrato compartilhado `../sofia-a2a-contrato/references/contrato-sofia-v1.md`.

## Fluxo no OpenSpec

1. Inspecionar `openspec/`, a constitution, as specs, o código e os testes. Não sobrescrever especificações nem convenções existentes.
2. Manter na constitution (RA-07) interoperabilidade, segurança de documentos, evolução versionada, rastreabilidade e testes de contrato.
3. Especificar: Agent Card público ou autenticado conforme a arquitetura, `message/send`, transporte JSON-RPC, análise de documento em texto, retorno da análise, propagação de IDs, tratamento de erros e timeout.
4. No `design.md`, descrever camadas (`Endpoints`, `Aplicacao`, `Dominio`, `Infraestrutura`), adaptador A2A, autenticação, logs e observabilidade.
5. Decompor em tarefas verificáveis e implementar por etapas: publicação do cartão, servidor, DTOs de negócio, mapeamento A2A, testes e documentação.
6. Executar testes unitários, de contrato e ponta a ponta com um cliente A2A real. Entregar evidências: requisições, respostas e logs sanitizados.

## Regras de protocolo e domínio

- O agente é descoberto em `/.well-known/agent-card.json`, que declara `url`, skills, versão e transporte. Nunca tratar o endereço do cartão como endpoint RPC.
- No perfil v0.3.x: JSON-RPC 2.0 `message/send`, `params.message.role=user`, `messageId` e `parts` com `kind=text`. A resposta é uma `Task` com a análise em `artifacts`, não em `status.message`.
- `SolicitacaoAnaliseSofia` chega como **texto JSON UTF-8 em um TextPart**; campos de negócio não são atributos do protocolo. `ResultadoAnaliseSofia` sai como JSON em TextPart de Artifact.
- `idExecucao`, `idSolicitacao`, `usuario.nome`, `pergunta`, `documentos[].idDocumento`, `documentos[].nome` e `documentos[].texto` são obrigatórios. Preservar os IDs e a relação documento-análise.
- Propagar `traceId` e request ID em metadata ou cabeçalhos, sem expor segredo. Registrar duração, status e agente; remover conteúdo sensível dos logs.
- O agente MUST NOT chamar outros agentes sem permissão explícita: a Sofia centraliza roteamento e síntese.
- Autenticar por HTTPS e mecanismo pactuado (por exemplo, Bearer), prevenir SSRF e prompt injection. Enquanto valer a SQ-12, a credencial fica adiada.
- Não inventar APIs de Microsoft Agent Framework nem de SDK A2A: inspecionar versões, pacotes e documentação antes de programar.

## Saída exigida

Atualizar constitution, spec, `design.md`, `tasks.md`, código .NET, testes, README e exemplos JSON, com comandos de execução e teste. Trabalhando só em especificação, não afirmar que a integração está executando.
