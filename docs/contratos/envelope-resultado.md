# Envelope do resultado

Formato do resultado de uma execução, devolvido por `GET /analises/{id}`. Requisitos em `openspec/changes/execucao-e-resultado/` (REQ-RESULT-001 a 006, REQ-EXEC-001 a 006) e regras em P03 a P09, GD-03, GD-04 e GD-05 da constitution.

**Situação:** o agente recebe a ECF e identifica o leiaute (mudança 07), mas ainda não valida o conteúdo. Toda ECF aceita termina com status e veredito `parcial`, com a revisão humana indicada e sem achados. Sem autenticação (SQ-12): a consulta não exige credencial nem restringe por organização, então use somente dados sintéticos ou anonimizados.

## Exemplo

Saída real do serviço para a solicitação de `tests/Api.Agente.Tests/Fixtures/Sofia/solicitacao-exemplo.json`, com o primeiro documento trocado pela ECF sintética do leiaute 12 (`tests/Api.Agente.Tests/Dados/ecf/`), como nos testes. Um teste confere que este exemplo continua igual ao contrato.

```json
{
  "versaoContrato": "1.2",
  "idExecucao": "0b198f54-5004-4357-a3bb-2847f3eaad56",
  "idExecucaoSofia": "8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e",
  "idSolicitacao": "e1b3a4cb-d729-4bd4-b796-b9c94be07b46",
  "idCorrelacao": "exemplo-doc-1",
  "status": "parcial",
  "veredito": "parcial",
  "revisaoHumana": true,
  "resumo": {
    "impeditivas": 0,
    "graves": 0,
    "observacoes": 0
  },
  "sintese": "Avaliação parcial: nem todas as validações foram realizadas. ECF do leiaute 12 recebida. Esta versão ainda não executa as validações de conteúdo: nenhum achado foi produzido e nenhuma conformidade é declarada.",
  "etapas": [
    {
      "numero": 1,
      "nome": "Recepção",
      "estado": "concluida",
      "justificativa": null,
      "causa": null
    },
    {
      "numero": 2,
      "nome": "Validação da entrada",
      "estado": "concluida",
      "justificativa": null,
      "causa": null
    },
    {
      "numero": 3,
      "nome": "Identificação do leiaute",
      "estado": "concluida",
      "justificativa": null,
      "causa": null
    },
    {
      "numero": 4,
      "nome": "Validação estrutural",
      "estado": "nao-executada",
      "justificativa": null,
      "causa": null
    },
    {
      "numero": 5,
      "nome": "Consulta ao manual",
      "estado": "nao-executada",
      "justificativa": null,
      "causa": null
    },
    {
      "numero": 6,
      "nome": "Ferramentas externas",
      "estado": "nao-executada",
      "justificativa": null,
      "causa": null
    },
    {
      "numero": 7,
      "nome": "Análise com LLM",
      "estado": "nao-executada",
      "justificativa": null,
      "causa": null
    },
    {
      "numero": 8,
      "nome": "Validação da saída",
      "estado": "nao-executada",
      "justificativa": null,
      "causa": null
    },
    {
      "numero": 9,
      "nome": "Retorno do resultado",
      "estado": "concluida",
      "justificativa": null,
      "causa": null
    }
  ],
  "achados": [],
  "limitacoes": [
    "Etapa não realizada: Validação estrutural. Motivo: ainda não implementada nesta versão.",
    "Etapa não realizada: Consulta ao manual. Motivo: ainda não implementada nesta versão.",
    "Etapa não realizada: Ferramentas externas. Motivo: ainda não implementada nesta versão.",
    "Etapa não realizada: Análise com LLM. Motivo: ainda não implementada nesta versão.",
    "Etapa não realizada: Validação da saída. Motivo: ainda não implementada nesta versão."
  ],
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
  ],
  "documentos": [
    {
      "idDocumento": "doc-001",
      "hash": "4fcc568d741837390469a2019c1eee5367d3bba78fdfa3ada09720e8e92f6701",
      "caracteres": 140
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
    "contrato": "1.2",
    "servico": "0.1.0+local",
    "validadores": "nenhum"
  },
  "criadaEm": "2026-11-07T23:22:55.0812409+00:00",
  "concluidaEm": "2026-11-07T23:22:55.0882484+00:00",
  "duracaoMs": 7.0075,
  "erro": null,
  "canal": "a2a",
  "leiaute": {
    "situacao": "identificado",
    "tipo": "ECF",
    "leiaute": 12,
    "codVer": "0012",
    "anoDoPeriodo": 2025,
    "motivo": null
  },
  "observacoes": [
    "Documento doc-002 não analisado por este agente: não é uma ECF.",
    "Leiaute identificado no documento doc-001: 12 (COD_VER 0012, DT_FIN em 2025)."
  ]
}
```

## Campos

Todos são preenchidos pelo sistema, nunca pelo modelo (P13).

| Campo | Conteúdo |
| --- | --- |
| `versaoContrato` | Versão deste envelope, hoje `1.2`. A 1.1 acrescentou `trechosDoManual` aos achados (mudança 06); a 1.2, `canal`, `leiaute`, `observacoes` e a `causa` de cada etapa (mudança 07). Todos opcionais. |
| `idExecucao` | Identificador da execução, gerado pelo agente. É o `{id}` da consulta. |
| `idExecucaoSofia` | `idExecucao` recebido da Sofia. |
| `idSolicitacao` | `idSolicitacao` recebido da Sofia. É único: uma retentativa devolve a execução já registrada. |
| `idCorrelacao` | `X-Correlation-Id` da requisição que criou a execução. |
| `status` | Se a análise rodou até o fim: status interno, na tabela abaixo. |
| `veredito` | Como a ECF está: veredito, na tabela abaixo. |
| `revisaoHumana` | Calculada pelo sistema; nenhuma recomendação do modelo a dispensa (P09). |
| `canal` | `a2a` ou `rest`: por onde o pedido chegou (mudança 07, D1). |
| `leiaute` | O que a etapa 3 identificou: `situacao` (`identificado`, `nao-suportado` ou `ambiguo`), `tipo` (`ECF`), `leiaute`, `codVer`, `anoDoPeriodo` (ano de `DT_FIN`) e `motivo`, quando não identificou (REQ-LEIAUTE-001 a 004). Vazio quando a entrada foi rejeitada antes. |
| `observacoes` | Informações que não mudam status nem veredito, como o leiaute identificado e cada documento que não é ECF (REQ-RECEPCAO-006). |
| `resumo` | Quantos achados há em cada classe: `impeditivas`, `graves` e `observacoes`. |
| `sintese` | Texto curto sobre o resultado, a partir do veredito. |
| `etapas` | As nove etapas de P03, sempre todas, cada uma com `numero`, `nome`, `estado`, `justificativa` e `causa`. A `causa` só vem em etapa com falha: `tecnica` torna o status `falha-tecnica`; `documento`, como `COD_VER` ausente, deixa a análise `parcial` (mudança 07, D7). |
| `achados` | Lista de achados, descritos abaixo. |
| `limitacoes` | Cada validação obrigatória não realizada, com o motivo, e as demais limitações da análise. |
| `naoVerificados` | O que o arquivo não prova, com `item` e `motivo`. Não muda o status nem o veredito, e fica vazio em `nao-avaliado`. |
| `documentos` | Para cada documento: `idDocumento`, `hash` (SHA-256 do texto) e `caracteres`. O texto nunca é guardado. |
| `identidade` | `usuarioPseudonimo` (SHA-256 de `usuario.nome`), `organizacao` (valor configurado) e `verificada`, sempre `false` nesta versão. |
| `versoes` | Versões do `contrato`, do `servico` e dos `validadores`. |
| `criadaEm`, `concluidaEm`, `duracaoMs` | Datas em UTC e duração. |
| `erro` | `codigo` e `mensagem`, quando a entrada é rejeitada; senão, `null`. Códigos: `solicitacao_invalida`, `versao_contrato_nao_suportada`, `documentos_acima_do_limite`, `nenhuma_ecf`, `mais_de_uma_ecf`, `linha_acima_do_limite` e `linhas_acima_do_limite` (mudança 07, D2). |

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

## Veredito

Calculado pelo sistema, nesta ordem (REQ-RESULT-010). O LLM não define nem altera o veredito.

| Ordem | Condição | `veredito` | Significado |
| --- | --- | --- | --- |
| 1 | Há achado impeditivo | `recusado` | Recusada na avaliação. Vale mesmo com validações pendentes: um erro impeditivo já prova que a ECF não está correta. |
| 2 | Status `entrada-rejeitada` ou `falha-tecnica` | `nao-avaliado` | Não foi possível avaliar. |
| 3 | Status `parcial` | `parcial` | Nada impeditivo até aqui, mas nem tudo foi validado. |
| 4 | Status `concluida`, com achados | `conforme-com-ressalvas` | Nada impeditivo; há pontos a verificar. |
| 5 | Status `concluida`, sem achados | `conforme` | De acordo e verificada nas validações realizadas. |

O resultado é apoio à conferência (seção 1.3 da constitution) e não substitui o programa oficial da ECF. O veredito é do produto, não uma classificação da Receita Federal.

## Classes

Calculadas pela severidade, sem criar outra escala (REQ-RESULT-007).

| `classe` | `severidade` | Critério |
| --- | --- | --- |
| `impeditiva` | `critica` | Regra do tipo `Erro` no manual: "impede a conclusão com sucesso da funcionalidade" (Manual do Leiaute 12, capítulo 3, página 38). Só erro estrutural e inconsistência de negócio podem ser impeditivos. |
| `grave` | `alta` | Não impede, mas a checagem é determinística com fonte oficial, o valor afeta tributo, saldo do ano seguinte ou gatilho oficial, e há consequência oficial nomeada. Critério do produto, em `docs/classificacao-das-regras.md`. |
| `observacao` | `media`, `baixa` | Regra do tipo `Aviso` no manual ("alerta apenas informativo") e demais pontos a verificar. |

## Revisão humana

`revisaoHumana` é `true` com falha técnica, validação obrigatória não concluída, indício da IA, ausência de evidências ou achado da classe `grave` (REQ-RESULT-005).

## Achados

| Campo | Conteúdo |
| --- | --- |
| `idAchado` | Identificador do achado. |
| `categoria` | Uma das cinco, na tabela abaixo (GD-04). |
| `origem` | `validador`, `ferramenta`, `llm` ou `sistema`. |
| `fundamento` | Regra, requisito ou fonte que fundamenta o achado. Obrigatório. |
| `descricao` | O problema apontado. |
| `registros` | `idDocumento`, `registro` e, quando disponíveis, `linha`, `campo` e `valorEncontrado`, este com até 200 caracteres. |
| `evidencias` | Trechos que sustentam o achado. |
| `severidade` | `baixa`, `media`, `alta` ou `critica`. Obrigatória. |
| `classe` | `impeditiva`, `grave` ou `observacao`, calculada pela severidade. |
| `regra` | `codigo`, `tipo` (`Erro`, `Aviso` ou `Não declarado`) e `nivel` (1.1, 1.2, 2 ou 3), quando o achado vem de regra do manual. O nível só vem quando o manual o declara. |
| `fonte` | `documento`, `versao` e `localizacao`, obrigatória em achado de regra do manual. |
| `orientacao` | Como corrigir, obrigatória em achado de regra do manual. O agente orienta, mas não altera o arquivo. |
| `trechosDoManual` | Trechos do manual recebidos na etapa 5 para a regra do achado: `idTrecho`, `idDocumento`, `versao`, `pagina` e `relevancia`, sem o texto. Opcional: só vem em achado de validador ou ferramenta cuja regra foi consultada (REQ-CONSULTA-006). |

O valor encontrado é dado do arquivo, então só é usado com dados sintéticos até DP-11 decidir retenção e mascaramento.

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
