# Design: Recuperação de conhecimento por MCP

> **Estado:** aprovado por Kaio Macedo em 2026-11-07, com o ADR 0005. A chave da OpenAI em produção ficou em aberto (E04): a busca foi para produção desativada.
>
> **Atualizado em 2026-11-12:** E04 decidida em 2026-11-10 (a chave que o projeto já usa) e chave cadastrada no Vercel em 2026-11-12; a busca está ligada em produção, usada pela etapa 5 e pelas perguntas da mudança 17. Duas notas da verificação: a consulta da etapa 5 leva o código `REGRA_...` do manual quando existe, senão o do registro, porque o identificador `EST-...` só agrupa e a ferramenta recusa hífen (ver `docs/avaliacao/recuperacao.md`); e a relevância é 1 menos a distância de cosseno, como no contrato da ferramenta.

## Context

A base de conhecimento está populada em produção: seis manuais da ECF (leiautes 7 a 12), 7.064 trechos, busca exata por cosseno filtrada por tipo e versão, avaliada com 30 perguntas (96,7% de acerto nos 5 primeiros trechos, `docs/avaliacao/recuperacao.md`). Hoje a busca só existe no comando `--avaliar`, dentro da Api.RAG.

A etapa 5 do fluxo (P03) precisa dessa busca na Api.Agente, por MCP (P15, RA-03). A skill `mcp-client-server` exige, como pré-condição, um ADR que avalie o transporte MCP no Vercel (DP-18; ADR 0002). Esse é o ADR 0005, apoiado na prova de conceito de E01.

## Goals / Non-Goals

**Goals:**
- Ferramenta MCP `buscar_no_manual` na Api.RAG, com contrato versionado, filtro obrigatório por tipo e versão e busca por código de regra.
- Cliente MCP na Api.Agente, com timeout, retentativas e registro dos trechos na execução.
- Etapa 5 com consultas montadas pelo sistema a partir dos achados, sem LLM.
- Limite de uso e desligamento da ferramenta pública (SQ-12).

**Non-Goals:**
- Uso dos trechos pelo LLM (mudança 10).
- Outras ferramentas MCP.
- Autenticação entre os serviços (SQ-12; `docs/roteiro.md`).
- Índice vetorial aproximado (ADR 0004).

## Decisions

### D1. Transporte MCP sem estado (E01; ADR 0005)

Streamable HTTP do SDK oficial do MCP para .NET, em modo sem estado (`HttpServerSessionMode.Stateless`), no caminho `/mcp` da Api.RAG. Cada requisição é independente: não há sessão em memória, então qualquer instância do Vercel atende qualquer chamada, como já acontece com o A2A.

- **Pacotes:** `ModelContextProtocol.AspNetCore` 2.4.0 na Api.RAG e `ModelContextProtocol.Core` 2.4.0 na Api.Agente, onde ficam `McpClient` e `HttpClientTransport`. São os únicos pacotes novos (P16, SQ-11).
- **Protocolo:** o SDK negocia a revisão 2026-07-28 da especificação do MCP, que não tem `initialize`, e aceita as anteriores.
- **Resposta:** `text/event-stream` com um único evento; o cliente aceita os dois tipos de conteúdo.
- **Prova de conceito, versões e alternativa** em caso de falha no Vercel: ADR 0005.

### D2. Contrato `buscar_no_manual` 1.0 (REQ-RECUP-001, 003, 005; GD-01)

Entrada:

| Campo | Tipo | Regra |
| --- | --- | --- |
| `consulta` | texto | Obrigatório, de 1 a 500 caracteres. |
| `tipoDocumento` | texto | Obrigatório. Tipo na base, hoje `manual-ecf`. |
| `versao` | texto | Obrigatório. Versão na base, como `leiaute-12`. |
| `quantidade` | inteiro | De 1 a 10; padrão 5 (E02). |
| `codigo` | texto | Opcional. Letras maiúsculas, dígitos e sublinhado, até 60 caracteres (REQ-RECUP-005). |
| `idExecucao` | UUID | Opcional. Execução da Api.Agente, para o consumo (D9). |

Saída: `versaoContrato` (`1.0`), `motivo` (`versao_indisponivel` quando não há documento vigente) e `trechos`, cada um com `idTrecho`, `idDocumento`, `titulo`, `versao`, `pagina`, `secao`, `relevancia` (de 0 a 1), `contemCodigo` e `texto`.

Erros da ferramenta, com código e mensagem sem dado sensível: `argumento_invalido`, `busca_desativada`, `limite_de_uso_atingido`, `falha_do_provedor` e `falha_tecnica`. O schema fica em `docs/contratos/mcp-buscar-no-manual.md`. Mudança incompatível vira 2.0, com convivência.

### D3. Consulta ao banco com busca por código (REQ-RECUP-002, 005)

Uma consulta SQL, sempre com `tipo`, `versao` e `vigente` como filtros:

1. Sem `codigo`: os `quantidade` trechos mais próximos por cosseno, como no `--avaliar`.
2. Com `codigo`: primeiro os trechos cujo texto contém o código (`strpos(texto, @codigo) > 0`), ordenados por cosseno; depois, os mais próximos que não o contêm, até completar a quantidade. Nenhum trecho repete.

O código é parâmetro do Npgsql, nunca concatenado (SQ-05). Sem documento vigente para a versão, a resposta vem vazia com `versao_indisponivel`, sem gerar embedding: a conferência da versão vem antes.

### D4. Avaliação antes de adotar valores (E02, E05; P11, GD-08)

Com o conjunto de `docs/avaliacao/perguntas.json`, no Leiaute 12:

- **Busca por código:** as perguntas que citam regra ou registro ganham o campo `codigo`. Compara-se o acerto com e sem a busca por código. A P15 é o caso conhecido.
- **Quantidade:** 3, 5 e 8 trechos.
- **Limiar:** entram 5 perguntas que o manual não responde, rascunhadas pelo assistente. O limiar é a menor relevância que separa as perguntas respondidas das não respondidas sem perder acerto. Se nenhum valor separar, fica sem limiar, e a etapa 5 não declara ausência de evidência pela relevância.

O resultado e o custo vão para `docs/avaliacao/recuperacao.md`. Até lá, valem 5 trechos, busca por código ligada e nenhum limiar. O custo previsto é de menos de US$ 0,01.

### D5. Limite de uso e desligamento (E06; REQ-RECUP-006; SQ-12)

| Opção | Padrão | Uso |
| --- | --- | --- |
| `Busca:Habilitada` | `false` | Sem a chave da OpenAI em produção, a ferramenta responde `busca_desativada`. |
| `Busca:LimiteDeConsultasPorDia` | `2000` | Contado no `consumo_ia`: embeddings de consulta desde 00:00 UTC, pela coluna `finalidade` da migração `0003_finalidade_do_consumo`. Ao atingir, `limite_de_uso_atingido`. |

As duas verificações vêm antes de gerar o embedding. Cada consulta tem cerca de 20 tokens, ou US$ 0,0000004 a US$ 0,02 por milhão; 2.000 por dia custam menos de US$ 0,001. O limite protege contra abuso do endpoint público, não contra o custo. A contagem por dia aceita uma pequena folga com chamadas simultâneas, registrada como risco.

### D6. Timeouts e retentativas (E03; DP-14; SQ-06)

| Chamada | Timeout | Retentativas |
| --- | --- | --- |
| Api.Agente para Api.RAG (MCP) | 10 s por tentativa (`Conhecimento:TimeoutEmSegundos`) | Até 2, com espera de 0,5 e 1 s, só para timeout, falha de conexão e HTTP 5xx |
| Api.RAG para OpenAI (embedding) | O de `ClienteDeEmbeddingsComConsumo`, da mudança 05 | As da mudança 05, uma linha de consumo por tentativa |

A busca não altera dados, exceto o registro de consumo, então repetir é seguro. Erros da ferramenta (`argumento_invalido` e os de D5) não são repetidos.

### D7. Etapa 5 na Api.Agente (E07; REQ-CONSULTA-005, 007)

O serviço de análise monta as consultas depois da etapa 4:

1. Agrupa os achados determinísticos pela regra. Cada grupo vira uma consulta com o código da regra, o registro do primeiro achado no texto da consulta e o tipo e a versão identificados na etapa 3.
2. Até `Conhecimento:MaximoDeConsultasPorExecucao` (padrão 10). As regras que passarem disso vão para a limitação.
3. Sem achado: etapa não aplicável, com a justificativa prevista "Nenhum achado a fundamentar no manual." (REQ-EXEC-003).

| Resposta | Etapa 5 | Efeito |
| --- | --- | --- |
| Trechos recebidos em todas as consultas | Concluída | Trechos ligados aos achados (D8) |
| `busca_desativada` ou `limite_de_uso_atingido` | Não executada | Limitação "consulta ao manual desativada" ou "limite diário de consultas ao manual atingido" |
| Timeout, conexão ou `falha_do_provedor`, depois das retentativas | Com falha, causa técnica | Achado de falha técnica e limitação "manual não consultado" (REQ-CONSULTA-003) |
| `versao_indisponivel` | Não executada | Limitação "manual da versão identificada não está na base" (P08) |

A causa da falha da etapa vem da mudança 07 (D7 dela). Se a 06 for implementada antes, esta mudança acrescenta a causa com o mesmo desenho.

### D8. Trechos na execução e no resultado (REQ-CONSULTA-002, 004, 006; P07, P19)

- Os trechos ficam no envelope guardado da execução (coluna `conteudo`, em JSON), em cada achado: `idTrecho`, `idDocumento`, versão, página e relevância. O texto do trecho não é guardado: ele pode ser lido de novo na Api.RAG pelo identificador.
- Cada achado ganha `trechosDoManual`, com as referências da sua regra. No contrato SOFIA, entra como campo opcional do achado; no envelope, a versão sobe para 1.1, compatível.
- Para a mudança 10, o texto recebido fica disponível só em memória, delimitado como `<fonte id="..." documento="..." versao="..." pagina="...">`, e nunca concatenado às instruções (P10).

### D9. Correlação e consumo (REQ-RECUP-004; SQ-07, P17)

O cliente envia `X-Correlation-Id` em cada requisição HTTP do MCP, e o `idExecucao` vai como argumento da ferramenta. A Api.RAG grava os dois no `consumo_ia` de cada embedding de consulta. O consumo da consulta fica na Api.RAG, sem consolidação (DP-26).

### D10. Configuração e chave (E04; RA-09, SQ-03)

| Serviço | Opção | Produção |
| --- | --- | --- |
| Api.Agente | `Conhecimento:UrlDoServidorMcp` | `https://agente-ecf-rag.vercel.app/mcp`, validada na inicialização |
| Api.RAG | `OpenAI:ApiKey` (`OpenAI__ApiKey`) | Segredo cadastrado por Kaio Macedo no Vercel, só se `Busca:Habilitada` for `true` |
| Api.RAG | `Busca:Habilitada` | `false` até o cadastro da chave |

A validação de inicialização exige a chave só quando a busca está habilitada, para o serviço subir sem ela. **Recomendação:** uma chave própria do Agente-ECF, com limite de gasto no painel da OpenAI, em vez da chave do `SofiaInfis`, que está em texto aberto no `appsettings.json` daquele projeto. O assistente não cadastra nem lê a chave em produção (SQ-03).

### D11. Testes (SQ-09)

- **Contrato:** schema de entrada e saída da ferramenta; envelope 1.1 e contrato SOFIA com `trechosDoManual`.
- **Integração:** Testcontainers com `pgvector/pgvector:pg16` e embeddings simulados; os dois serviços ponta a ponta pelo transporte HTTP real.
- **Falhas:** timeout, conexão recusada, `busca_desativada`, `limite_de_uso_atingido`, `versao_indisponivel`.
- **Adversarial:** trecho com instrução embutida; código com aspas e ponto e vírgula.
- **Arquitetura:** a Api.Agente não referencia Npgsql para a Api.RAG, nem a API administrativa dela.
- O CI não chama a OpenAI: embeddings simulados ou gravados. A avaliação de D4 é execução separada.

## Dimensionamento

- **Custo:** menos de US$ 0,001 por dia no limite de D5; a avaliação de D4, menos de US$ 0,01.
- **Tempo:** a busca exata em cerca de 1.200 trechos por versão é de milissegundos. O que pesa é o embedding da consulta, uma chamada à OpenAI por regra, até 10 por execução, feitas em paralelo, com até 4 ao mesmo tempo.

## Risks / Trade-offs

- **MCP no Vercel:** a prova de conceito foi local, com duas instâncias. A confirmação final é em produção (tarefa 6.2), com a alternativa do ADR 0005.
- **Endpoint público:** sem autenticação (SQ-12), qualquer um pode consultar os manuais, que são públicos. O limite diário protege o custo.
- **Contagem diária com folga:** chamadas simultâneas podem passar um pouco do limite.
- **Antes da mudança 10, a etapa 5 rende pouco:** ela só liga trechos aos achados. O valor maior vem com a análise.

## Migration Plan

1. Deploy da Api.RAG com `Busca:Habilitada` `false`, depois da Api.Agente, com a migração `0006`.
2. Quando Kaio Macedo decidir a chave (E04, em aberto), ele a cadastra, `Busca:Habilitada` passa a `true`, e o assistente redeploya e verifica (tarefas 6.1 e 6.2).
3. Volta: `Busca:Habilitada` `false` desliga a busca sem deploy de código; a etapa 5 vira limitação.

## Ajustes na implementação (2026-11-07)

Mudanças em relação ao plano aprovado, sem efeito nos requisitos:

- **Identificadores da base (D2):** `tipoDocumento` e `versao` usam os valores da base (`manual-ecf`, `leiaute-12`), em vez de `ECF` e `12`. Assim a Api.RAG não esconde uma conversão de nomes; quem converte o leiaute identificado é a Api.Agente (`ManualDaVersao.DaEcf`).
- **Trechos no envelope (D8):** o envelope da execução já é guardado inteiro em JSON, então a tabela `trechos_consultados` ficou desnecessária (P16). Nenhuma migração na Api.Agente.
- **Código `falha_tecnica` (D2):** separa a falha do banco da Api.RAG da falha do provedor.
- **Teste ponta a ponta (tarefa 4.1):** o teste dos contêineres chama a ferramenta MCP na imagem real da Api.RAG. A chamada da Api.Agente à Api.RAG em contêineres entra na mudança 07, que liga a etapa 5 ao fluxo.

## Open Questions

- Que chave usar em produção (E04): em aberto, por decisão de Kaio Macedo em 2026-11-07. Não bloqueia a implementação.

## Skills aplicáveis

- `mcp-client-server`: D1, D2, D6 a D9. Pré-condições: SQ-12 vigente, que permite o servidor sem autenticação, e o ADR 0005, que avalia o transporte no Vercel.
- `rag-pgvector`: D3 e D4. Pré-condição: ADR 0004, aceito.
- `vercel-dotnet-pgvector-atlas`: tarefa 6.2. Pré-condição: ADR 0002, aceito.
- `controle-custos-llm`: D5 e D9.
- `testes-agentes-ia`: D11.
- `github-versionamento`: branch, commits e PR.

## Verificação da constitution

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Não se aplica | Nenhuma regra fiscal nova; a busca devolve trechos das fontes registradas. |
| P02 | Atende | As consultas são montadas por regra do sistema, sem LLM (D7). |
| P03 | Atende | Etapa 5 com os quatro estados e as dependências da etapa 3 (D7). |
| P04 | Atende | Indisponibilidade é falha técnica; desligamento e limite são limitação (D7). |
| P05 | Atende | Etapa 5 não concluída impede `concluida`. |
| P06 | Atende | Cada trecho com documento, versão, página e relevância (D2, D8). |
| P07 | Atende | Trechos registrados por identificador, para conferir o que o LLM citar na mudança 10 (D8). |
| P08 | Atende | Limitações para desligamento, limite, versão ausente e consultas acima do máximo (D7). |
| P09 | Atende | Falha técnica e validação não concluída exigem revisão humana, pela regra existente. |
| P10 | Atende | Trechos delimitados como fonte; teste adversarial (D8, D11). |
| P11 | Atende | Filtro por tipo e versão; quantidade, busca por código e limiar avaliados antes de adotar (D3, D4). |
| P12 | Não se aplica | Sem LLM. |
| P13 | Atende | Correlação, execução e consumo preenchidos pelo sistema (D9). |
| P14 | Atende | Ferramenta com nome, contrato, validação, timeout, retentativa, testes e registro (D2, D5, D6). Autorização adiada pela SQ-12. |
| P15 | Atende | Só MCP entre os serviços; teste de arquitetura (D11). |
| P16 | Atende | Dois pacotes do SDK oficial, justificados no ADR 0005. |
| P17 | Atende | Uma linha de consumo por embedding de consulta, com tentativas (D9). |
| P18 | Não se aplica | Sem memória. |
| P19 | Atende | Trechos consultados registrados por execução (D8). |
| P20 | Atende | Esclarecimentos, ADR e tarefas ligadas a requisitos antes do código. |
