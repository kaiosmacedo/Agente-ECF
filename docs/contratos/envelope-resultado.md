# Envelope do resultado

Formato do resultado de uma execução, devolvido por `GET /analises/{id}`. Requisitos em `openspec/changes/execucao-e-resultado/` (REQ-RESULT-001 a 006, REQ-EXEC-001 a 006) e regras em P03 a P09, GD-03, GD-04 e GD-05 da constitution.

**Situação:** a análise da ECF ainda não existe. Toda solicitação válida termina em `parcial`, com a revisão humana indicada e sem achados. Sem autenticação (SQ-12): a consulta não exige credencial nem restringe por organização, então use somente dados sintéticos ou anonimizados.

## Exemplo

Saída real do serviço para a solicitação de `tests/Api.Agente.Tests/Fixtures/Sofia/solicitacao-exemplo.json`. Um teste confere que este exemplo continua igual ao contrato.

```json
{
  "versaoContrato": "1.0",
  "idExecucao": "0c2b1690-69c3-478e-89c1-be2026274300",
  "idExecucaoSofia": "8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e",
  "idSolicitacao": "e1b3a4cb-d729-4bd4-b796-b9c94be07b46",
  "idCorrelacao": "exemplo-doc-1",
  "status": "parcial",
  "revisaoHumana": true,
  "sintese": "2 documento(s) recebido(s). Esta versão ainda não executa as validações da ECF: nenhum achado foi produzido e nenhuma conformidade é declarada.",
  "etapas": [
    {
      "numero": 1,
      "nome": "Recepção",
      "estado": "concluida",
      "justificativa": null
    },
    {
      "numero": 2,
      "nome": "Validação da entrada",
      "estado": "concluida",
      "justificativa": null
    },
    {
      "numero": 3,
      "nome": "Identificação do leiaute",
      "estado": "nao-executada",
      "justificativa": null
    },
    {
      "numero": 4,
      "nome": "Validação estrutural",
      "estado": "nao-executada",
      "justificativa": null
    },
    {
      "numero": 5,
      "nome": "Consulta ao manual",
      "estado": "nao-executada",
      "justificativa": null
    },
    {
      "numero": 6,
      "nome": "Ferramentas externas",
      "estado": "nao-executada",
      "justificativa": null
    },
    {
      "numero": 7,
      "nome": "Análise com LLM",
      "estado": "nao-executada",
      "justificativa": null
    },
    {
      "numero": 8,
      "nome": "Validação da saída",
      "estado": "nao-executada",
      "justificativa": null
    },
    {
      "numero": 9,
      "nome": "Retorno do resultado",
      "estado": "concluida",
      "justificativa": null
    }
  ],
  "achados": [],
  "limitacoes": [
    "Etapa não realizada: Identificação do leiaute. Motivo: ainda não implementada nesta versão.",
    "Etapa não realizada: Validação estrutural. Motivo: ainda não implementada nesta versão.",
    "Etapa não realizada: Consulta ao manual. Motivo: ainda não implementada nesta versão.",
    "Etapa não realizada: Ferramentas externas. Motivo: ainda não implementada nesta versão.",
    "Etapa não realizada: Análise com LLM. Motivo: ainda não implementada nesta versão.",
    "Etapa não realizada: Validação da saída. Motivo: ainda não implementada nesta versão."
  ],
  "documentos": [
    {
      "idDocumento": "doc-001",
      "hash": "8d5e75d4a95a7e77b3e5f11e5d1a45aa6c0063b6708948a28ffc89f6657cbccf",
      "caracteres": 50
    },
    {
      "idDocumento": "doc-002",
      "hash": "c0e8a6303b06ec5cfb98b65c05b9c551036a4d4101a23c7cea64031fe8fc7a73",
      "caracteres": 30
    }
  ],
  "identidade": {
    "usuarioPseudonimo": "afb7e6dc7a5af17641062578df2598374f7436ba6359315f7285092caa1d904e",
    "organizacao": "infis",
    "verificada": false
  },
  "versoes": {
    "contrato": "1.0",
    "servico": "0.1.0+local",
    "validadores": "nenhum"
  },
  "criadaEm": "2026-11-05T16:02:45.1793594+00:00",
  "concluidaEm": "2026-11-05T16:02:45.1841929+00:00",
  "duracaoMs": 4.8335,
  "erro": null
}
```

## Campos

Todos são preenchidos pelo sistema, nunca pelo modelo (P13).

| Campo | Conteúdo |
| --- | --- |
| `versaoContrato` | Versão deste envelope, hoje `1.0`. |
| `idExecucao` | Identificador da execução, gerado pelo agente. É o `{id}` da consulta. |
| `idExecucaoSofia` | `idExecucao` recebido da Sofia. |
| `idSolicitacao` | `idSolicitacao` recebido da Sofia. É único: uma retentativa devolve a execução já registrada. |
| `idCorrelacao` | `X-Correlation-Id` da requisição que criou a execução. |
| `status` | Status interno, na tabela abaixo. |
| `revisaoHumana` | Calculada pelo sistema; nenhuma recomendação do modelo a dispensa (P09). |
| `sintese` | Texto curto sobre o resultado. |
| `etapas` | As nove etapas de P03, sempre todas, cada uma com `numero`, `nome`, `estado` e `justificativa`. |
| `achados` | Lista de achados, descritos abaixo. |
| `limitacoes` | Cada validação obrigatória não realizada, com o motivo, e as demais limitações da análise. |
| `documentos` | Para cada documento: `idDocumento`, `hash` (SHA-256 do texto) e `caracteres`. O texto nunca é guardado. |
| `identidade` | `usuarioPseudonimo` (SHA-256 de `usuario.nome`), `organizacao` (valor configurado) e `verificada`, sempre `false` nesta versão. |
| `versoes` | Versões do `contrato`, do `servico` e dos `validadores`. |
| `criadaEm`, `concluidaEm`, `duracaoMs` | Datas em UTC e duração. |
| `erro` | `codigo` e `mensagem`, quando a entrada é rejeitada; senão, `null`. |

Texto dos documentos, nome do usuário e pergunta não são guardados (GD-06, SQ-04). O pseudônimo é só minimização: um SHA-256 sem sal de um nome conhecido pode ser recalculado. Por isso, dados reais continuam dependendo de DP-11 e da retomada da autenticação (mudança 02).

## Estado das etapas

`concluida`, `nao-aplicavel` (com `justificativa` prevista na especificação), `com-falha` ou `nao-executada`. Quando uma etapa falha, as que dependem dela ficam como `nao-executada`.

## Status

A regra é aplicada nesta ordem, e `status` no contrato SOFIA é o da última coluna (E01):

| Condição | `status` | Contrato SOFIA |
| --- | --- | --- |
| Etapa 2 com falha | `entrada-rejeitada` | `erro` |
| Etapa com falha ou achado de falha técnica | `falha-tecnica` | `erro` |
| Validação obrigatória não concluída, ou limitações | `parcial` | `parcial` |
| Todas as validações obrigatórias concluídas | `concluida` | `concluido` |

`concluida` não significa "conforme": erros e inconsistências continuam nos achados.

## Revisão humana

`revisaoHumana` é `true` com falha técnica, validação obrigatória não concluída, indício da IA ou ausência de evidências (REQ-RESULT-005).

## Achados

| Campo | Conteúdo |
| --- | --- |
| `idAchado` | Identificador do achado. |
| `categoria` | Uma das cinco, na tabela abaixo (GD-04). |
| `origem` | `validador`, `ferramenta`, `llm` ou `sistema`. |
| `fundamento` | Regra, requisito ou fonte que fundamenta o achado. Obrigatório. |
| `descricao` | O problema apontado. |
| `registros` | `idDocumento`, `registro` e, quando disponíveis, `linha` e `campo`. |
| `evidencias` | Trechos que sustentam o achado. |
| `severidade` | `baixa`, `media`, `alta` ou `critica`. Obrigatória. |

| Categoria | Origens permitidas | Exige `registros` |
| --- | --- | --- |
| `erro-estrutural` | `validador` | Sim |
| `inconsistencia-de-negocio` | `validador`, `ferramenta` | Sim |
| `indicio-da-ia` | `llm` | Sim |
| `ausencia-de-evidencias` | `sistema`, `llm` | Não: não houve como verificar |
| `falha-tecnica` | `sistema` | Não: não houve como verificar |

## Severidade

É uma escala do produto, a mesma do contrato SOFIA v1.0, e não uma classificação da Receita Federal. Cada regra determinística declara a sua (GD-05), e a severidade não substitui a categoria.

| Severidade | Critério |
| --- | --- |
| `critica` | O arquivo não pode ser analisado ou entregue como está, segundo a regra citada. |
| `alta` | Precisa de correção. |
| `media` | Uma pessoa precisa verificar. |
| `baixa` | Risco pequeno. |

## Consulta

| Resposta | Quando |
| --- | --- |
| 200, com o envelope | A execução existe. |
| 404 | O identificador não existe. |

A API está descrita em `/openapi/v1.json`.
