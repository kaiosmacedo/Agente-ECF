# Integração com a Sofia INFIS por A2A

Como a Sofia aciona o Agente-ECF. Requisitos em `openspec/changes/servidor-a2a/` e regras em RA-07, SQ-12 e SQ-13 da constitution. O contrato de negócio é compartilhado entre os agentes da turma e está em [`.claude/skills/sofia-a2a-contrato/references/contrato-sofia-v1.md`](../../.claude/skills/sofia-a2a-contrato/references/contrato-sofia-v1.md).

**Situação:** esqueleto da integração, em produção desde 2026-11-05. Protocolo e contrato são reais, mas a análise ainda não existe: toda solicitação válida vira uma execução registrada e responde com status `parcial`, sem achados, declarando as validações não realizadas. Sem autenticação nem token (SQ-12): use somente dados sintéticos ou anonimizados.

## Identificação

| Item | Valor |
| --- | --- |
| Cartão | `GET /.well-known/agent-card.json` (cópia versionada em [`agent-card.json`](agent-card.json)) |
| Endpoint A2A | `supportedInterfaces[0].url` do cartão; em produção, `https://agente-ecf-agente.vercel.app/a2a` |
| Protocolo | A2A `1.0`, JSON-RPC 2.0, com o cabeçalho `A2A-Version: 1.0` |
| SDK do servidor | `A2A.AspNetCore` 1.0.0-preview4 |
| `agente.idAgente` | `agente-ecf` |
| Skill | `analisar_documentos` |
| Streaming e notificações | Não |

As skills da turma fixavam v0.3.x "até decisão explícita de migração". Kaio Macedo padronizou v1.0 em 2026-11-05, a versão que o pacote `Microsoft.Agents.AI.A2A` do projeto `SofiaInfis` traz. O contrato de negócio (`versaoContrato: "1.0"`) não mudou.

## O que é de A2A e o que é do contrato

| Camada | Campos | Quem define |
| --- | --- | --- |
| JSON-RPC | `jsonrpc`, `id`, `method`, `params`, `result`, `error` | Protocolo |
| Mensagem A2A | `message.role`, `message.messageId`, `message.parts[].text` | Protocolo |
| Tarefa A2A | `task.id`, `task.contextId`, `task.status.state`, `task.artifacts[].artifactId`, `.name`, `.parts[].text` | Protocolo |
| `SolicitacaoAnaliseSofia` | JSON dentro de `message.parts[0].text` | Contrato SOFIA v1.0 |
| `ResultadoAnaliseSofia` | JSON dentro de `task.artifacts[0].parts[0].text` | Contrato SOFIA v1.0 |

## Exemplo

Requisição (`POST` no endereço do cartão, com `Content-Type: application/json` e `A2A-Version: 1.0`):

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "method": "SendMessage",
  "params": {
    "message": {
      "role": "ROLE_USER",
      "messageId": "5f92c2ba-6870-4862-87ba-1ae8669a77f4",
      "parts": [
        {
          "text": "{\"versaoContrato\":\"1.0\",\"idExecucao\":\"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e\",\"idSolicitacao\":\"e1b3a4cb-d729-4bd4-b796-b9c94be07b46\",\"usuario\":{\"nome\":\"Maria Oliveira\"},\"pergunta\":\"Analise o documento\",\"documentos\":[{\"idDocumento\":\"doc-001\",\"nome\":\"documento-sintetico.txt\",\"texto\":\"Documento sintético de teste.\"}]}"
        }
      ]
    }
  }
}
```

Resposta (o texto do Artifact foi formatado aqui para leitura; ele chega como string JSON):

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "result": {
    "task": {
      "id": "5cd591b8-6cd1-4dd2-87d2-ede0a99ac066",
      "contextId": "4441e6c2-5d89-4715-9a9f-f9e4885d029e",
      "status": { "state": "TASK_STATE_COMPLETED", "timestamp": "2026-11-05T13:58:45.7168463+00:00" },
      "artifacts": [
        {
          "artifactId": "a147e2de-93ac-4c60-a764-efeae072d3df",
          "name": "analise-sofia-v1",
          "parts": [
            {
              "text": {
                "versaoContrato": "1.0",
                "idExecucao": "8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e",
                "idSolicitacao": "e1b3a4cb-d729-4bd4-b796-b9c94be07b46",
                "agente": {
                  "idAgente": "agente-ecf",
                  "nome": "Agente ECF"
                },
                "status": "parcial",
                "analise": "Avaliação parcial: nem todas as validações foram realizadas. 2 documento(s) recebido(s). Esta versão ainda não executa as validações da ECF: nenhum achado foi produzido e nenhuma conformidade é declarada.",
                "achados": [],
                "observacoes": [
                  "Documento doc-001 recebido: 50 caracteres, SHA-256 8d5e75d4a95a7e77b3e5f11e5d1a45aa6c0063b6708948a28ffc89f6657cbccf.",
                  "Documento doc-002 recebido: 30 caracteres, SHA-256 c0e8a6303b06ec5cfb98b65c05b9c551036a4d4101a23c7cea64031fe8fc7a73.",
                  "Etapa não realizada: Identificação do leiaute. Motivo: ainda não implementada nesta versão.",
                  "Etapa não realizada: Validação estrutural. Motivo: ainda não implementada nesta versão.",
                  "Etapa não realizada: Consulta ao manual. Motivo: ainda não implementada nesta versão.",
                  "Etapa não realizada: Ferramentas externas. Motivo: ainda não implementada nesta versão.",
                  "Etapa não realizada: Análise com LLM. Motivo: ainda não implementada nesta versão.",
                  "Etapa não realizada: Validação da saída. Motivo: ainda não implementada nesta versão."
                ],
                "erro": null,
                "idExecucaoAgente": "14fa6878-4e5e-4189-8ac0-2897fd0aa555",
                "revisaoHumana": true,
                "veredito": "parcial",
                "resumoAchados": {
                  "impeditivas": 0,
                  "graves": 0,
                  "observacoes": 0
                },
                "naoVerificados": [
                  {
                    "item": "Confronto do IRPJ e da CSLL com DCTF, DCOMP e pagamentos",
                    "motivo": "Depende de dados da Receita Federal ou de relatórios que não vêm no arquivo."
                  },
                  {
                    "item": "Certificado e procuração, ECD e ECF anterior na base, hash na base, cadastro do CNPJ, versão do programa e recepção",
                    "motivo": "São conferidos na transmissão ou na base da Receita Federal."
                  },
                  {
                    "item": "Pagamento efetivo, PGFN, parcelamento, intimação, aviso de malha, autos de infração e Sintonia",
                    "motivo": "Estão em sistemas da Receita Federal, com acesso autenticado."
                  },
                  {
                    "item": "e-Financeira, EFD-Reinf e eSocial de terceiros, DIRF de fontes pagadoras e Siscomex",
                    "motivo": "São dados prestados por terceiros."
                  },
                  {
                    "item": "Documentos que comprovam prejuízo fiscal e base negativa, e guarda de documentos",
                    "motivo": "Estão fora da ECF."
                  },
                  {
                    "item": "REGRA_DV_RECIBO, REGRA_VALIDA_HASH_ARQUIVO, regras das tabelas dinâmicas fora do manual e fórmulas com funções sem definição",
                    "motivo": "Não há algoritmo ou fonte acessível, e adotar um violaria P01."
                  }
                ]
              }
            }
          ]
        }
      ]
    }
  }
}
```

## Desfechos

| Situação | `task.status.state` | `status` do contrato | `erro.codigo` |
| --- | --- | --- | --- |
| Solicitação válida | `TASK_STATE_COMPLETED` | `parcial` | — |
| Campo obrigatório ausente ou inválido, `idDocumento` repetido | `TASK_STATE_COMPLETED` | `erro` | `solicitacao_invalida` |
| `versaoContrato` diferente de `1.0` | `TASK_STATE_COMPLETED` | `erro` | `versao_contrato_nao_suportada` |
| Texto dos documentos acima do limite | `TASK_STATE_COMPLETED` | `erro` | `documentos_acima_do_limite` |
| Banco indisponível: nada foi registrado, e a Sofia pode repetir a chamada. O veredito é `nao-avaliado`. | `TASK_STATE_COMPLETED` | `erro` | `falha_tecnica` |
| Mensagem sem parte de texto, texto fora da estrutura JSON do contrato, `idExecucao` ou `idSolicitacao` que não são UUID | `TASK_STATE_FAILED` | sem resultado | motivo em `status.message` |

`concluido` fica reservado para quando todas as validações obrigatórias existirem (P05).

**Campos opcionais:** além dos da v1.0, o resultado traz `idExecucaoAgente`, o identificador para consultar `GET /analises/{id}` (ver `envelope-resultado.md`), `revisaoHumana`, `veredito`, `resumoAchados` e `naoVerificados`, a lista do que o arquivo não prova. Com achados, cada um traz também `categoria`, `origem`, `regra`, `classe`, `registros`, `orientacao` e `fonte`. Quem não os conhece pode ignorá-los.

**Veredito:** é o campo que diz como a ECF está: `conforme`, `conforme-com-ressalvas`, `recusado`, `parcial` ou `nao-avaliado` (ver `envelope-resultado.md`). O `status` só diz se a análise rodou: `recusado` pode vir com `status` `parcial`.

**Retentativas:** repetir o mesmo `idSolicitacao` devolve o resultado já registrado, sem criar outra execução.

| Erro JSON-RPC | Quando |
| --- | --- |
| `-32700` | O corpo não é JSON. |
| `-32601` | Método desconhecido, inclusive os da v0.3, como `message/send`. |
| `-32602` | Parâmetros inválidos. |
| `-32001` | Tarefa não encontrada. |
| `-32002` | `CancelTask` em tarefa já terminada. |
| `-32003` | Configuração de notificações. |
| `-32004` | `SendStreamingMessage` e `SubscribeToTask` em tarefa terminada. |
| `-32009` | Versão desconhecida em `A2A-Version`. |

A resposta de `SendMessage` já é final, mesmo com `returnImmediately`. No Vercel, as instâncias não duram, então `GetTask` não é garantido. `ListTasks` responde sempre com lista vazia: sem identidade verificada, nenhum chamador vê as tarefas dos outros.

## Limite de tamanho

| Onde | Limite | O que acontece acima dele |
| --- | --- | --- |
| Vercel (produção) | Corpo da requisição entre 4,4 MB e 4,6 MB, medido em 2026-11-05 | HTTP 413 `FUNCTION_PAYLOAD_TOO_LARGE`, sem resposta JSON-RPC. A Sofia deve tratar como falha técnica. |
| Serviço | `A2A:TamanhoMaximoDosDocumentosEmBytes` (10 MiB) | Resultado com status `erro` e código `documentos_acima_do_limite`. Em produção, o limite do Vercel vem antes. |
| Kestrel (Docker) | 30 MB no corpo, padrão do ASP.NET Core | Erro HTTP 413. |

O corpo inclui o envelope e o escape do JSON, então o texto útil é um pouco menor. A decisão para arquivos reais fica na mudança 07 (DP-05).

## Configuração

`ConnectionStrings__Execucoes` é segredo: fica no Vercel, nunca no repositório (ver `docs/ci-cd.md`). O serviço não inicia sem ela. Os demais valores não são segredo.

| Opção (variável de ambiente) | Padrão | Uso |
| --- | --- | --- |
| `A2A:UrlPublica` (`A2A__UrlPublica`) | Em produção, a URL do Vercel; fora dela, `http://localhost:8080/a2a` | Endereço anunciado no cartão |
| `A2A:TamanhoMaximoDosDocumentosEmBytes` | `10485760` (provisório até DP-05, na mudança 07) | Limite do texto somado dos documentos |
| `Cors:AllowedOrigins:<n>` (`Cors__AllowedOrigins__<n>`) | Vazio | Origens de navegador, só se houver frontend |
| `Identidade:OrganizacaoPadrao` (`Identidade__OrganizacaoPadrao`) | `infis` | Organização registrada nas execuções |
| `ConnectionStrings:Execucoes` (`ConnectionStrings__Execucoes`) | Nenhum: obrigatória | Banco das execuções, no formato `Host=...;Database=...;Username=...;Password=...` |

**CORS:** a chamada da Sofia é entre servidores e não depende de CORS. Preencha `Cors:AllowedOrigins` só se um frontend no navegador chamar o agente, com a origem desse frontend (esquema, host e porta, sem barra final). CORS não substitui autenticação.

**Autenticação:** a integração não usa token, por decisão de Kaio Macedo (SQ-12). O que voltaria se isso mudar está na seção "Autenticação adiada" de [`docs/roteiro.md`](../roteiro.md).

**Organização:** o contrato v1.0 não tem esse campo, então o agente registra um valor padrão configurado, marcado como não verificado.

**Privacidade:** `usuario.nome`, `pergunta` e o texto dos documentos não vão para logs, para a resposta, para o armazenamento das tarefas nem para o banco. Do nome, só o SHA-256 é guardado, como pseudônimo; dos documentos, só hash e tamanho. Os logs trazem `idExecucao`, `idSolicitacao`, status e o `X-Correlation-Id`.

## Como executar

Testes:

```bash
dotnet test --solution Agente-ECF.slnx -c Release
```

Na rede da Sofia, com Docker. A rede é a `sofia_network` do compose de `SofiaInfis`, cujo nome real é `sofia-infis_sofia_network`. Sem aquele compose no ar, crie a rede uma vez:

```bash
docker network create sofia-infis_sofia_network
```

O compose sobe também o banco local das execuções e aplica as migrações:

```bash
docker compose up --build agente-ecf
```

```bash
curl http://localhost:8085/.well-known/agent-card.json
```

De outro contêiner na mesma rede, o endereço é o nome do serviço, não `localhost`:

```bash
docker run --rm --network sofia-infis_sofia_network curlimages/curl:8.10.1 http://agente-ecf:8080/.well-known/agent-card.json
```

A API da Sofia hoje roda fora do compose dela. Nesse caso, informe o endereço que ela enxerga:

```bash
A2A_URL_PUBLICA=http://localhost:8085/a2a docker compose up --build agente-ecf
```
