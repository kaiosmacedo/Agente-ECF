# Integração com a Sofia INFIS por A2A

Como a Sofia aciona o Agente-ECF. Requisitos em `openspec/changes/servidor-a2a/` e regras em RA-07, SQ-12 e SQ-13 da constitution. O contrato de negócio é compartilhado entre os agentes da turma e está em [`.claude/skills/sofia-a2a-contrato/references/contrato-sofia-v1.md`](../../.claude/skills/sofia-a2a-contrato/references/contrato-sofia-v1.md).

**Situação:** em produção desde 2026-11-05. O agente reconhece a ECF e identifica o leiaute (mudança 07) e valida a estrutura do leiaute 12: registros, campos, hierarquia, ocorrências e contagens (mudança 08). Com erro de estrutura impeditivo, o veredito é `recusado`, e cada achado diz o que corrigir; sem ele, é `parcial`, porque as regras de conteúdo (09) e a análise (10) ainda não existem. Sem autenticação nem token (SQ-12): use somente dados sintéticos ou anonimizados.

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

Chamada real à produção, refeita em 2026-11-08, com a ECF sintética `01-valor-fora-da-lista.txt` do gabarito (`tests/Api.Agente.Tests/Dados/ecf/gabarito/`), que tem `IND_SIT_INI_PER` igual a 9 no Registro 0000. O veredito é `recusado`, com o que corrigir, e a consulta ao manual não roda porque a busca está desligada em produção.

Requisição (`POST` no endereço do cartão, com `Content-Type: application/json` e `A2A-Version: 1.0`):

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "method": "SendMessage",
  "params": {
    "message": {
      "role": "ROLE_USER",
      "messageId": "8c74437a-3bb3-4e2d-86f1-5074bf30e6eb",
      "parts": [
        {
          "text": "{\"versaoContrato\":\"1.0\",\"idExecucao\":\"1bf40689-5619-4f5e-ac8a-330e4136d9e4\",\"idSolicitacao\":\"a5c6f634-2553-4c86-b62f-3cb5b209f714\",\"usuario\":{\"nome\":\"Maria Oliveira\"},\"pergunta\":\"Valide esta ECF e diga o que precisa ser corrigido.\",\"documentos\":[{\"idDocumento\":\"doc-001\",\"nome\":\"ecf-2025.txt\",\"tipo\":\"text/plain\",\"texto\":\"|0000|LECF|0012|11111111000191|EMPRESA SINTETICA DE TESTE|9|0|||01012025|31122025|N||0||\\r\\n|0001|0|\\r\\n|0010||N|1|T|01|RRRR|||||||\\r\\n|0020|1|0|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N||\\r\\n|0030|2046|6204000|RUA TESTE|1234|BLOCO Z SALA 301|BAIRRO ECF|DF|5300108|71000000|6133333333|testeecf@ecf.gov.br|\\r\\n|0990|6|\\r\\n|C001|1|\\r\\n|C990|2|\\r\\n|E001|1|\\r\\n|9001|0|\\r\\n|9900|0000|1|||\\r\\n|9900|0001|1|||\\r\\n|9900|0010|1|||\\r\\n|9900|0020|1|||\\r\\n|9900|0030|1|||\\r\\n|9900|0990|1|||\\r\\n|9900|C001|1|||\\r\\n|9900|C990|1|||\\r\\n|9900|E001|1|||\\r\\n|9900|9001|1|||\\r\\n|9900|9900|13|||\\r\\n|9900|9990|1|||\\r\\n|9900|9999|1|||\\r\\n|9990|16|\\r\\n|9999|25|\\r\\n\"}]}"
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
      "id": "f044450ab4884526a1b59feadb6fe3d6",
      "contextId": "dac0c992d7b8427794a7b19316b7ca66",
      "status": {
        "state": "TASK_STATE_COMPLETED",
        "timestamp": "2026-11-08T00:52:30.5112806Z"
      },
      "artifacts": [
        {
          "artifactId": "6b34264a-e417-4f68-9c03-4b9931b32df3",
          "name": "analise-sofia-v1",
          "parts": [
            {
              "text": {
                "versaoContrato": "1.0",
                "idExecucao": "1bf40689-5619-4f5e-ac8a-330e4136d9e4",
                "idSolicitacao": "a5c6f634-2553-4c86-b62f-3cb5b209f714",
                "agente": {
                  "idAgente": "agente-ecf",
                  "nome": "Agente ECF"
                },
                "status": "parcial",
                "analise": "Recusada na avaliação: 1 regra(s) impeditiva(s) violada(s). O resultado vale para as validações realizadas e é apoio à conferência, sem substituir o programa oficial da ECF. ECF do leiaute 12 recebida e validada na estrutura: 1 achado(s) estrutural(is). As regras de conteúdo e a análise ainda não existem, então nenhuma conformidade é declarada.",
                "achados": [
                  {
                    "idAchado": "est-0001",
                    "descricao": "O campo IND_SIT_INI_PER do Registro 0000 tem um valor fora dos valores válidos, na linha 1.",
                    "idDocumentos": [
                      "doc-001"
                    ],
                    "evidencias": [
                      "Linha 1"
                    ],
                    "severidade": "critica",
                    "categoria": "erro-estrutural",
                    "origem": "validador",
                    "regra": "EST-CAMPO-006 0000.IND_SIT_INI_PER",
                    "classe": "impeditiva",
                    "registros": [
                      {
                        "idDocumento": "doc-001",
                        "registro": "0000",
                        "linha": 1,
                        "campo": "IND_SIT_INI_PER",
                        "valorEncontrado": "9"
                      }
                    ],
                    "orientacao": "Preencha IND_SIT_INI_PER com um dos valores válidos: [0; 1; 2; 3; 4;5;6;7;8].",
                    "fonte": {
                      "documento": "Manual de Orientação do Leiaute 12 da ECF",
                      "versao": "leiaute-12",
                      "localizacao": "p. 63, Registro 0000, campo 6 IND_SIT_INI_PER"
                    }
                  }
                ],
                "observacoes": [
                  "Documento doc-001 recebido: 607 caracteres, SHA-256 3a73c964e82514d32f38cc7e6632929f02c5ed463c83a471baed23816635772f.",
                  "Leiaute identificado no documento doc-001: 12 (COD_VER 0012, DT_FIN em 2025).",
                  "Etapa não realizada: Consulta ao manual. Motivo: a busca no manual está desligada na base de conhecimento.",
                  "Etapa não realizada: Ferramentas externas. Motivo: ainda não implementada nesta versão.",
                  "Etapa não realizada: Análise com LLM. Motivo: ainda não implementada nesta versão.",
                  "Etapa não realizada: Validação da saída. Motivo: ainda não implementada nesta versão.",
                  "consulta ao manual desativada"
                ],
                "erro": null,
                "idExecucaoAgente": "89aba8c8-6e28-49c9-8413-3f9e9fad9e12",
                "revisaoHumana": true,
                "veredito": "recusado",
                "resumoAchados": {
                  "impeditivas": 1,
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
                  },
                  {
                    "item": "Regras REGRA_ de conteúdo do manual: valores, cruzamentos entre registros, cálculos, preenchimento e existência condicionados",
                    "motivo": "Ficam para a mudança 09, de regras de negócio."
                  },
                  {
                    "item": "Dígito verificador de CNPJ, CPF e recibo (REGRA_VALIDA_CNPJ, REGRA_VALIDA_CPF_CNPJ e outras)",
                    "motivo": "O algoritmo não está no manual; adotar um sem fonte violaria P01."
                  },
                  {
                    "item": "REGRA_LEIAUTE_VIGENTE e REGRA_SEM_LEIAUTE",
                    "motivo": "Dependem de tabelas que não estão no manual."
                  },
                  {
                    "item": "Valores de campos que remetem a outro registro: M305, M310, M355, M360, M500 e M510",
                    "motivo": "A coluna de valores válidos cita o campo de outro registro, como [M010.COD_CTA_B] (p. 251). É cruzamento e fica para a mudança 09."
                  },
                  {
                    "item": "Valores de campos que dependem de tabela externa ou dinâmica",
                    "motivo": "As tabelas ficam fora do manual, como a planilha de tabelas dinâmicas (p. 620)."
                  },
                  {
                    "item": "Tamanho exato dos campos",
                    "motivo": "O manual fala em tamanho exato no caso geral (p. 59), e o programa da ECF só o exige em alguns campos, sem lista publicada. Foram conferidos o máximo e o tamanho exato dos campos preenchidos por posição."
                  },
                  {
                    "item": "Ocorrência mínima e máxima de 43 registros: C051, C150, C155, C157, E155, J051, K156, K356, K915, K935, L200, M300, M350, N500, N610, N615, N650, N660, P030, P200, P300, P400, P500, S100, S150, S200, S300, S500, S700, U030, U182, V010, W250, X350, X351, X352, X353, X354, X355, X356, Y681, 9100, 9900",
                    "motivo": "O cabeçalho de cada registro (página em docs/catalogo/ecf-leiaute-12.md) e a Tabela de Registros (p. 43 a 58) divergem (REQ-ESTRUT-011)."
                  },
                  {
                    "item": "Hierarquia dos registros X451 e Y650",
                    "motivo": "O nível do cabeçalho e o da Tabela de Registros divergem (REQ-ESTRUT-011)."
                  },
                  {
                    "item": "Ordem entre registros irmãos",
                    "motivo": "O manual só diz que a ordem é sequencial (p. 58); foram conferidas a ordem dos blocos e a hierarquia."
                  },
                  {
                    "item": "Fim de linha CR LF",
                    "motivo": "O texto que chega pela Sofia pode ter as quebras de linha trocadas."
                  },
                  {
                    "item": "Campos do tipo D (X280.VIG_INI e X280.VIG_FIM)",
                    "motivo": "O tipo não está definido no manual (p. 33)."
                  },
                  {
                    "item": "Duplicidade no Registro V100",
                    "motivo": "A chave do registro está vazia no cabeçalho (p. 424)."
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
| Solicitação válida com uma ECF | `TASK_STATE_COMPLETED` | `parcial` | — |
| Nenhum documento é ECF | `TASK_STATE_COMPLETED` | `erro` | `nenhuma_ecf` |
| Mais de uma ECF no pedido | `TASK_STATE_COMPLETED` | `erro` | `mais_de_uma_ecf` |
| Linha da ECF acima do limite, ou linhas demais | `TASK_STATE_COMPLETED` | `erro` | `linha_acima_do_limite` ou `linhas_acima_do_limite` |
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

## Guia para a Sofia

Para quem mantém a Sofia e para o agente validador que recebe o resultado depois dela (mudança 07, D9 e D10).

**Como reconhecer uma ECF:** é um texto cuja primeira linha começa com `|0000|LECF|` (Manual da ECF, leiaute 12, p. 32 e 63). O campo seguinte é o `COD_VER`, de `0007` a `0012` para os leiautes 7 a 12 (anos-calendário 2020 a 2025). O cartão do agente diz o mesmo, na descrição da skill `analisar_documentos`.

**O que enviar:**

- Uma ECF por pedido, com o texto inteiro em `documentos[].texto` e `tipo` `text/plain`.
- Sem o corte de 20.000 caracteres: com o marcador `[DOCUMENTO TRUNCADO]`, o agente só identifica o leiaute e declara a limitação "documento cortado antes de chegar ao agente". Retirar o corte para os agentes é com quem mantém a Sofia.
- Até 4 MiB de texto. Acima de cerca de 4,5 MB de corpo, o Vercel responde HTTP 413 sem JSON-RPC, e a Sofia deve tratar como falha técnica.
- Outros documentos no mesmo pedido não atrapalham: ficam nas observações como "não analisado por este agente: não é uma ECF".

**Como ler o resultado:**

| Campo | Significado |
| --- | --- |
| `veredito` | Como a ECF está: `conforme`, `conforme-com-ressalvas`, `recusado`, `parcial` ou `nao-avaliado`. |
| `achados` | O que corrigir, com registro, linha, campo, valor encontrado, orientação e fonte com página. Só a classe `impeditiva` recusa o arquivo; `observacao` aponta, sem recusar, e o que o programa da ECF aceita não é recusado, mesmo quando o manual diverge (REQ-ESTRUT-016). O campo `regra` começa pelo identificador da regra, do manual (como `REGRA_OCORRENCIA_UNITARIA_ARQ`) ou estrutural (como `EST-CAMPO-006`; lista no design da mudança 08, D2), seguido do registro e do campo: `EST-CAMPO-006 0000.IND_SIT_INI_PER`. No envelope de `GET /analises/{id}`, o mesmo texto está em `fundamento`. |
| `naoVerificados` e `observacoes` | O que ficou de fora, como o leiaute identificado, documentos que não são ECF e etapas não realizadas, cada uma com o motivo real: a consulta ao manual, por exemplo, diz que a busca está desligada. |
| `idExecucaoAgente` | Identificador para consultar `GET /analises/{id}`, com o envelope completo. |
| `erro.codigo` | Por que nada foi analisado, na tabela de desfechos. |

**Agente validador:** o Agente-ECF não chama outros agentes (RA-07): a Sofia leva o resultado adiante. O resultado mantém os mesmos `idExecucao` e `idSolicitacao`, e cada achado traz a evidência necessária para um terceiro conferir.

## Limite de tamanho

| Onde | Limite | O que acontece acima dele |
| --- | --- | --- |
| Vercel (produção) | Corpo da requisição entre 4,4 MB e 4,6 MB, medido em 2026-11-05 | HTTP 413 `FUNCTION_PAYLOAD_TOO_LARGE`, sem resposta JSON-RPC. A Sofia deve tratar como falha técnica. |
| Serviço | `Recepcao:TamanhoMaximoDosDocumentosEmBytes` (4 MiB), o mesmo no `POST /analises` | Resultado com status `erro` e código `documentos_acima_do_limite`. Em produção, o limite do Vercel vem antes para corpos acima de cerca de 4,5 MB. |
| Kestrel (Docker) | 30 MB no corpo, padrão do ASP.NET Core | Erro HTTP 413. |

O corpo inclui o envelope e o escape do JSON, então o texto útil é um pouco menor. A mudança 07 fixou 4 MiB (DP-05); arquivos reais maiores dependem de medir ECF reais antes de dados reais (`docs/roteiro.md`).

**Medição de 2026-11-07, em produção:** uma ECF sintética de 3.989.743 bytes, num corpo de 3.991.891 bytes, foi aceita, e o `SendMessage` levou de 6,5 a 8,2 segundos em três rodadas, a maior parte no envio do corpo. Com a recepção e a identificação, a resposta na mesma requisição cabe (DP-22); a mudança 10 reavalia com o LLM.

## Configuração

`ConnectionStrings__Execucoes` é segredo: fica no Vercel, nunca no repositório (ver `docs/ci-cd.md`). O serviço não inicia sem ela. Os demais valores não são segredo.

| Opção (variável de ambiente) | Padrão | Uso |
| --- | --- | --- |
| `A2A:UrlPublica` (`A2A__UrlPublica`) | Em produção, a URL do Vercel; fora dela, `http://localhost:8080/a2a` | Endereço anunciado no cartão |
| `Recepcao:TamanhoMaximoDosDocumentosEmBytes` (`Recepcao__TamanhoMaximoDosDocumentosEmBytes`) | `4194304` | Limite do texto somado dos documentos, nos dois canais (mudança 07, D4) |
| `Recepcao:TamanhoMaximoDaLinhaEmCaracteres` | Igual ao tamanho máximo | Limite de cada linha; o manual não limita o `ARQ_RTF` do Y800 |
| `Recepcao:QuantidadeMaximaDeLinhas` | `1000000` | Limite de linhas da ECF |
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
