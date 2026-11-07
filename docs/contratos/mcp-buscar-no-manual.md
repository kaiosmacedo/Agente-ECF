# Ferramenta MCP `buscar_no_manual` 1.0

Contrato da busca nos manuais oficiais que a Api.RAG expõe por MCP e que a Api.Agente usa na etapa 5 do fluxo (P15, RA-03). Requisitos em `openspec/changes/recuperacao-conhecimento-mcp/` (REQ-RECUP-001 a 006, REQ-CONSULTA-001 a 007) e transporte no [ADR 0005](../adr/0005-mcp-sem-estado-no-vercel.md).

**Situação:** em produção desde a mudança 06, mas **desligada** (`Busca:Habilitada` é `false`): a chave da OpenAI de produção está em aberto, por decisão de Kaio Macedo em 2026-11-07. Desligada, a ferramenta responde `busca_desativada`, e a etapa 5 registra a limitação, sem falha técnica.

## Transporte

| Item | Valor |
| --- | --- |
| Endereço | `POST /mcp` da Api.RAG; em produção, `https://agente-ecf-rag.vercel.app/mcp` |
| Transporte | Streamable HTTP, sem estado: nenhuma sessão, nenhum `Mcp-Session-Id` |
| SDK | `ModelContextProtocol.AspNetCore` 2.4.0 (servidor) e `ModelContextProtocol.Core` 2.4.0 (cliente) |
| Protocolo | O SDK negocia a revisão 2026-07-28 do MCP e aceita as anteriores, desde a 2024-11-05 |
| Resposta | `text/event-stream` com um único evento; o cliente aceita `application/json` e `text/event-stream` |
| Correlação | Cabeçalho `X-Correlation-Id`, gravado no consumo da consulta |
| Autenticação | Nenhuma na primeira versão (SQ-12; `docs/roteiro.md`, "Autenticação adiada") |

## Entrada

Todos os argumentos são opcionais no schema, para que a falta de um vire erro tipado do contrato, e não do protocolo (REQ-RECUP-003).

| Argumento | Tipo | Regra |
| --- | --- | --- |
| `consulta` | texto | Obrigatório, de 1 a 500 caracteres. |
| `tipoDocumento` | texto | Obrigatório. Tipo do documento na base, como `manual-ecf` (`docs/fontes/manifesto-rag.json`). |
| `versao` | texto | Obrigatório. Versão na base, como `leiaute-12`. |
| `quantidade` | inteiro | De 1 a 10; padrão 5. |
| `codigo` | texto | Opcional. Código literal a procurar primeiro, como `REGRA_NATUREZA_PERMITIDA_PATRIMONIAL` ou `K155`: letras maiúsculas, dígitos e sublinhado, até 60 caracteres. |
| `idExecucao` | UUID | Opcional. Execução da Api.Agente que pediu a busca, gravada no consumo. |

## Saída

O texto do resultado é um JSON:

```json
{
  "versaoContrato": "1.0",
  "motivo": null,
  "trechos": [
    {
      "idTrecho": "4f8c3a3e-1b5e-4c0a-9a43-6d2a6d0f2b11",
      "idDocumento": "a0d1f3c2-7e44-4b8f-8a1d-2c9f0b6e5d33",
      "titulo": "Manual de Orientação do Leiaute 12 da ECF",
      "versao": "leiaute-12",
      "pagina": 147,
      "secao": null,
      "relevancia": 0.8123,
      "contemCodigo": true,
      "texto": "REGRA_NATUREZA_PERMITIDA_PATRIMONIAL: ..."
    }
  ]
}
```

- `motivo` é `versao_indisponivel` quando não há documento vigente do tipo e da versão; nesse caso, `trechos` vem vazio e nenhum embedding é gerado.
- `relevancia` é a similaridade de cosseno, arredondada em 4 casas.
- Com `codigo`, os trechos que contêm o código vêm primeiro (`contemCodigo` verdadeiro), ordenados por relevância, e os demais completam a quantidade, sem repetir.
- O texto do trecho é dado não confiável (P10): quem o usa com um modelo o delimita como fonte.

## Erros

Um erro chega como resultado com `isError` verdadeiro, e o texto é um JSON:

```json
{ "versaoContrato": "1.0", "erro": { "codigo": "busca_desativada", "mensagem": "A busca está desativada neste ambiente." } }
```

| Código | Quando | O cliente repete? | Na etapa 5 |
| --- | --- | --- | --- |
| `argumento_invalido` | Argumento ausente ou fora da regra | Não | Falha técnica: é defeito de quem chama |
| `busca_desativada` | `Busca:Habilitada` é `false` | Não | Limitação "consulta ao manual desativada" |
| `limite_de_uso_atingido` | Consultas do dia (UTC) atingiram `Busca:LimiteDeConsultasPorDia` | Não | Limitação "limite diário de consultas ao manual atingido" |
| `falha_do_provedor` | A OpenAI não respondeu depois das tentativas da Api.RAG | Não | Falha técnica |
| `falha_tecnica` | O banco da Api.RAG não respondeu | Não | Falha técnica |

O cliente repete só timeout, falha de conexão e HTTP 5xx, até 2 vezes, com 0,5 e 1 s de espera, com timeout de 10 s por tentativa (`Conhecimento:TimeoutEmSegundos`).

## Versionamento

Campo novo e opcional mantém a versão 1.x. Mudança incompatível vira 2.0, com convivência das duas versões (GD-01). O cliente recusa resposta com outra `versaoContrato` e registra falha técnica.

## Como testar

```bash
curl -s -X POST https://agente-ecf-rag.vercel.app/mcp -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -d '{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}'
```
