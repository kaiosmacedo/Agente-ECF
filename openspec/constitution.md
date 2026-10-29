# Constitution do Agente-ECF

| Campo | Valor |
| --- | --- |
| Versão | 1.3.0 |
| Estado | Vigente (ratificada na v1.1.0; emendas v1.2.0 e v1.3.0 aprovadas) |
| Criada em | 2026-10-29 |
| Última alteração | 2026-10-29 |
| Ratificada em | 2026-10-29, por Kaio Macedo (versão 1.1.0) |
| Responsáveis pela aprovação | Kaio Macedo, responsável pelo projeto, com aprovação única (DP-19) |
| Fonte normativa | `openspec/constitution.md` neste repositório |

Este documento é a fonte normativa única dos princípios, restrições e critérios obrigatórios do Agente-ECF. Sínteses em outros locais, como o cofre Obsidian da INFIS, MUST indicar a versão resumida e MUST NOT ser usadas como referência em revisões.

## Convenções normativas

| Palavra-chave | Significado |
| --- | --- |
| **MUST** (ou **SHALL**) | Obrigação. O descumprimento bloqueia a aprovação, salvo desvio registrado (seção 6.7). |
| **MUST NOT** (ou **SHALL NOT**) | Proibição. Mesma consequência. |
| **MAY** | Opção permitida, sem obrigação. |

- As palavras-chave seguem o sentido da RFC 2119 e ficam em inglês, como o validador do OpenSpec exige nas especificações. Esta constitution usa MUST, MUST NOT e MAY; nas especificações, SHALL e MUST são equivalentes. O texto restante é escrito em português.
- Termos em português como "deve" ou "pode", em minúsculas, não têm efeito normativo.
- Identificadores citáveis em revisões: `P` (princípios), `RA` (restrições arquiteturais), `GD` (governança de dados), `SQ` (segurança e qualidade), `DoD` (Definition of Done) e `DP` (decisões pendentes). Um comentário de revisão MUST citar o identificador violado, por exemplo: "viola P07".
- Uma regra marcada "quando aplicável" exige que a especificação declare se ela se aplica e por quê.

---

## 1. Propósito e escopo

### 1.1 Propósito

O Agente-ECF é um agente de Inteligência Artificial que analisa arquivos da Escrituração Contábil Fiscal (ECF), valida sua estrutura e a consistência das informações e identifica inconsistências, divergências e possíveis erros de preenchimento ou de estruturação dos registros.

A solução aplica regras de negócio e critérios técnicos derivados do leiaute e das exigências do Sistema Público de Escrituração Digital (SPED). Ela interpreta os registros, verifica a integridade das informações, cruza registros relacionados e analisa a coerência dos dados fiscais e contábeis. Isso inclui a apuração do IRPJ e da CSLL e a compatibilidade entre informações contábeis, fiscais e de períodos anteriores, quando aplicável.

Ao final, o agente gera um relatório com as inconsistências, os registros envolvidos, a descrição dos problemas e, quando possível, orientações de correção. O objetivo é reduzir erros, aumentar a confiabilidade das informações fiscais e tornar a conferência da ECF mais eficiente.

O Agente-ECF é um dos agentes do ecossistema da **Sofia INFIS**, a orquestradora da INFIS. A Sofia MAY acioná-lo por A2A, e o agente MAY também ser usado diretamente pelos endpoints REST da Api.Agente. Nos dois casos, o objetivo é validar arquivos da ECF.

### 1.2 Escopo

A solução combina:

- validação determinística de arquivos e registros;
- consulta a manuais oficiais por RAG;
- uso controlado de ferramentas externas;
- análise com LLM;
- respostas estruturadas e rastreáveis;
- integração com outros agentes e orquestradores por A2A, a começar pela Sofia INFIS.

A análise é executada sob demanda, a cada envio de arquivo pelo usuário ou pela orquestradora. Não há execução agendada.

### 1.3 Fora do escopo

Nesta fase, o agente apenas valida o arquivo recebido. Os itens abaixo estão fora do escopo, e a inclusão de qualquer um deles exige emenda a esta constitution (seção 8).

- Transmitir, assinar ou entregar a ECF ao SPED.
- Alterar, corrigir ou regenerar o arquivo analisado. Orientações de correção são permitidas (P06).
- Substituir a responsabilidade do profissional contábil ou fiscal. O resultado é apoio à conferência.
- Produzir interpretações tributárias sem fundamento em fonte oficial registrada.

### 1.4 Aplicabilidade e precedência

Esta constitution se aplica a especificações, esclarecimentos, planos técnicos, ADRs, tarefas, código, testes, prompts, configurações, contratos e revisões de pull request.

Regras de domínio fiscal e contábil derivam exclusivamente dos documentos oficiais registrados nas especificações. Esta constitution não define regras fiscais. Nos demais temas, em caso de conflito, prevalece nesta ordem:

1. esta constitution;
2. ADRs aceitos;
3. especificações vigentes (`openspec/specs/`);
4. mudanças em andamento (`openspec/changes/`);
5. código.

Um conflito MUST ser registrado e resolvido por esclarecimento, ADR ou emenda. Ele MUST NOT ser resolvido silenciosamente no código.

### 1.5 Glossário

| Termo | Definição |
| --- | --- |
| Execução | Processamento de uma solicitação de análise, do recebimento ao retorno do resultado, com identificador único gerado pelo sistema. |
| Documento analisado | Arquivo recebido para validação, por exemplo um arquivo da ECF. |
| Fonte oficial | Documento publicado por órgão competente (manual de leiaute, schema, tabela, norma) identificado por emissor, título e versão. |
| Base de conhecimento | Conjunto de fontes oficiais ingeridas e indexadas pela Api.RAG. |
| Chunk | Trecho de uma fonte oficial indexado com embedding e metadados de origem. |
| Validador | Componente determinístico que verifica estrutura ou regra de negócio e produz o mesmo resultado para a mesma entrada e versão das regras. |
| Ferramenta | Função invocável pelo agente, por Function Calling ou MCP, com contrato próprio. |
| Achado | Item do resultado associado a uma das cinco categorias de GD-04. |
| Evidência | Dado verificável que sustenta um achado: localização no documento, resultado de validador ou ferramenta, ou chunk recuperado. |
| Referência | Identificador recuperável de uma fonte oficial: documento, versão e localização. |
| Contrato do LLM | Schema JSON versionado da resposta que o modelo produz. |
| Envelope da API | Estrutura retornada ao cliente, preenchida pelo sistema, que contém metadados e o resultado da análise. |

---

## 2. Princípios obrigatórios

### P01 — Fontes oficiais como única base do domínio

**Regra**

- Todo leiaute, registro, campo, tabela de domínio, schema, regra de validação e critério de conformidade MUST ter origem em fonte oficial registrada na especificação. O registro inclui emissor, título, versão ou data de publicação, vigência aplicável e localização (seção, página, registro ou campo, quando disponível).
- Esses elementos MUST NOT ser inferidos de conhecimento prévio do modelo, de arquivos de exemplo, de código de terceiros ou de suposições da equipe.
- Uma regra sem fonte identificada MUST ser registrada como esclarecimento aberto ou decisão pendente e MUST NOT ser implementada como requisito aprovado.
- Divergências entre fontes ou entre versões MUST ser registradas na especificação, com a decisão de precedência e seu responsável.

**Justificativa.** Um leiaute ou regra fiscal incorreta compromete o resultado e pode induzir o contribuinte a erro. A ligação com a fonte permite auditar cada regra e atualizá-la quando o leiaute mudar.

**Verificação**

- Revisão de especificação: cada requisito de domínio cita uma fonte cadastrada (documento, versão e localização).
- Revisão de PR: toda regra implementada referencia o identificador do requisito correspondente. Não existem constantes de leiaute ou de regra sem requisito.
- Os testes de cada regra citam o identificador do requisito.

### P02 — Determinismo antes da IA

**Regra**

- Validações estruturais (leiaute, registros, campos, tipos, tamanhos, obrigatoriedade, hierarquia, schema) e regras de negócio determinísticas MUST ser implementadas por validadores ou ferramentas verificáveis, com resultado reproduzível para a mesma entrada e a mesma versão das regras.
- O LLM MUST NOT executar, substituir, reavaliar ou anular essas validações. A análise do modelo MUST NOT alterar o resultado de um validador.
- Uma regra classificada como determinística na especificação MUST NOT existir apenas em prompt.
- A validação por XSD MUST ser aplicada somente a documentos XML para os quais exista schema oficial correspondente. Os demais formatos MUST usar validadores compatíveis com seus leiautes.
- O LLM MAY explicar, contextualizar e priorizar os resultados determinísticos, e MAY apontar indícios adicionais (P04).

**Justificativa.** A conferência fiscal exige resultados repetíveis e auditáveis. Modelos de linguagem são probabilísticos e não garantem a mesma resposta.

**Verificação**

- Cada regra determinística possui validador com testes de caso válido e de caso inválido.
- Nenhum prompt contém regra classificada como determinística sem o validador correspondente.
- Um teste com resposta simulada do LLM que contradiz um validador confirma que o achado e o status não mudam.

### P03 — Fluxo de validação obrigatório

**Regra**

Toda execução de análise MUST percorrer as etapas abaixo, respeitando suas dependências:

1. Receber o arquivo e identificar usuário, contexto (incluindo organização, quando aplicável) e execução.
2. Validar tamanho, formato e condições de processamento.
3. Identificar o tipo de documento e a versão do leiaute.
4. Validar estrutura, registros e schema aplicável.
5. Consultar por MCP os chunks do manual correspondente ao tipo e à versão identificados.
6. Executar ferramentas externas quando necessário e autorizado.
7. Produzir a análise com o LLM.
8. Validar a saída estruturada.
9. Retornar o resultado com evidências e rastreabilidade.

- Cada etapa MUST registrar seu estado: concluída, não aplicável (com justificativa prevista na especificação), com falha ou não executada.
- A falha de uma etapa MUST interromper as etapas que dependem dela ou marcá-las como não executadas. O resultado MUST NOT tratá-las como concluídas.
- Se a etapa 3 não identificar o tipo e a versão do leiaute de forma inequívoca, as validações dependentes de versão MUST NOT ser executadas, e a limitação MUST constar do resultado.
- A especificação MAY paralelizar etapas independentes, desde que preserve as dependências e o registro de estados.

**Justificativa.** O fluxo garante que a IA atue sobre um documento já validado deterministicamente e com o conhecimento da versão correta.

**Verificação**

- O registro de cada execução contém as nove etapas com seus estados.
- Testes de integração simulam a falha de cada etapa e verificam a interrupção das dependentes e o status resultante.
- Um PR que altere a ordem ou elimine etapas exige emenda a esta constitution.

### P04 — Classificação inequívoca dos resultados

**Regra**

- Cada item do resultado MUST pertencer a exatamente uma destas categorias, definidas em GD-04:
  1. erro estrutural do documento;
  2. inconsistência de negócio;
  3. indício identificado pela IA;
  4. ausência de evidências;
  5. falha técnica ou dependência indisponível.
- Erros estruturais e inconsistências de negócio MUST ser produzidos exclusivamente por validadores ou ferramentas determinísticas.
- Itens produzidos pelo LLM MUST ser classificados como indício ou como ausência de evidências. O LLM MUST NOT criar itens das demais categorias.
- Falhas técnicas MUST ser registradas pelo sistema e MUST NOT ser apresentadas como problema do documento.

**Justificativa.** O usuário precisa distinguir violação comprovada, suspeita, falta de base e problema do sistema, porque cada caso exige uma ação diferente.

**Verificação**

- O contrato define a categoria como enumeração fechada.
- A validação semântica rejeita categoria incompatível com a origem do item.
- Existem testes para cada categoria e para cada combinação proibida de origem e categoria.

### P05 — Conformidade somente com validações concluídas

**Regra**

- O sistema MUST NOT declarar o documento conforme, válido ou sem inconsistências quando alguma validação obrigatória não tiver sido concluída, quando uma dependência necessária estiver indisponível ou quando a saída do LLM não tiver sido aceita.
- A ausência de achados MUST NOT ser apresentada como conformidade enquanto houver validação obrigatória com falha, não executada ou marcada como não aplicável sem justificativa.
- O status do processamento MUST ser calculado por regra determinística do sistema, a partir dos estados das etapas e dos achados. O LLM MUST NOT definir esse status.
- A especificação MUST definir o conjunto de validações obrigatórias para cada tipo e versão de documento.

**Justificativa.** Declarar conformidade indevidamente é o erro mais grave que uma ferramenta de conferência fiscal pode cometer.

**Verificação**

- Para cada validação obrigatória, um teste simula sua não conclusão e confirma que o status não indica conformidade.
- Revisão de código: o cálculo do status fica fora do componente que integra o LLM.

### P06 — Evidência e rastreabilidade

**Regra**

- Cada achado MUST identificar:
  - os registros envolvidos, com identificador do registro, linha ou posição no arquivo e campo, quando disponíveis;
  - a regra, o requisito ou a fonte que o fundamenta;
  - a origem: validador, ferramenta ou LLM;
  - a execução.
- Toda referência a manual MUST ser recuperável: identificador do documento, versão e localização do trecho.
- Orientações de correção MUST se basear em fonte recuperável ou em regra determinística. Elas MUST ser apresentadas como orientação, nunca como correção aplicada.

**Justificativa.** Sem evidência localizável, o usuário não consegue conferir nem corrigir o apontamento, e o resultado não pode ser auditado.

**Verificação**

- O schema rejeita achados sem os campos obrigatórios.
- Um teste confirma que cada referência retornada aponta para documento e versão existentes na base de conhecimento.

### P07 — Proibição de fabricação

**Regra**

- O LLM MUST NOT fabricar evidências, resultados de ferramentas, referências, trechos de manuais, identificadores de registros ou valores do documento.
- O sistema MUST verificar que cada referência, chunk, resultado de ferramenta e localização citados pelo modelo existem entre os itens efetivamente recuperados, executados ou presentes no documento naquela execução.
- Um item com referência não verificável MUST ser rejeitado ou reclassificado conforme a política definida na especificação (DP-23), e a ocorrência MUST ser registrada. Esse item MUST NOT ser entregue como evidência válida.

**Justificativa.** Para o usuário, uma referência inventada é indistinguível de uma verdadeira. Uma única ocorrência compromete a confiança em todo o resultado.

**Verificação**

- A validação semântica cruza os identificadores citados com o registro da execução.
- Testes usam respostas simuladas com referência, chunk, resultado de ferramenta ou linha inexistentes e confirmam a rejeição.

### P08 — Limitações declaradas

**Regra**

- O resultado MUST declarar a limitação e listar as validações não realizadas, com o motivo, quando faltarem evidências suficientes, quando uma etapa não for executada ou quando a versão aplicável do manual não estiver disponível.
- O agente MUST NOT preencher lacunas com suposições ou com conhecimento prévio do modelo.

**Justificativa.** Declarar o que não foi verificado é tão importante quanto apontar erros, porque evita uma falsa sensação de segurança.

**Verificação**

- Testes executados sem a versão aplicável do manual e com ferramenta indisponível confirmam o preenchimento das limitações e das validações não realizadas.

### P09 — Revisão humana determinada pelo sistema

**Regra**

- O resultado MUST indicar se é necessária revisão humana.
- A indicação MUST ser calculada por regras determinísticas do sistema, definidas na especificação. No mínimo, ela MUST ser positiva quando houver falha técnica, validação obrigatória não concluída, indício identificado pela IA ou ausência de evidências.
- O LLM MAY recomendar revisão humana adicional, mas MUST NOT dispensá-la quando as regras do sistema a exigirem.

**Justificativa.** O agente apoia a conferência. Resultados incertos precisam chegar a um profissional responsável.

**Verificação**

- Existe um teste para cada condição mínima.
- Um teste com o LLM recomendando a dispensa confirma que a indicação não muda.

### P10 — Conteúdo externo é dado não confiável

**Regra**

- O conteúdo dos documentos analisados, dos chunks, das fontes ingeridas, dos resultados de ferramentas e das mensagens recebidas por A2A MUST ser tratado como dado, nunca como instrução.
- Esse conteúdo MUST NOT alterar instruções do sistema, regras de validação, autorização de ferramentas, contrato de saída ou classificação dos achados.
- Mensagens do usuário MAY orientar a análise dentro do escopo permitido, mas MUST NOT alterar regras do sistema, autorizações ou o contrato.
- O conteúdo não confiável MUST ser delimitado e identificado nas mensagens enviadas ao modelo.

**Justificativa.** Arquivos e documentos podem conter texto, malicioso ou acidental, que tenta manipular o agente (injeção de prompt).

**Verificação**

- Uma suíte de testes adversariais executada em CI embute instruções em campos do arquivo, em chunks e em resultados de ferramentas e confirma que o comportamento não muda.
- Alterações de prompt passam por revisão de PR.

### P11 — Conhecimento versionado e aplicável

**Regra**

- Cada documento da base de conhecimento MUST possuir origem, identificação, versão, vigência (quando disponível) e hash do conteúdo.
- Cada chunk MUST referenciar seu documento e sua versão, e a localização no documento (página, seção, registro ou campo), quando disponível.
- A recuperação MUST filtrar pelo tipo de documento e pela versão do manual identificados na etapa 3. A recuperação de outra versão MUST NOT ocorrer, salvo regra explícita da especificação, e nesse caso MUST ser sinalizada no resultado.
- A ingestão MUST seguir critérios documentados de segmentação, deduplicação, reprocessamento e atualização.
- Modelo de embeddings, dimensão dos vetores e estratégia de busca MUST ser compatíveis entre si e documentados. A troca do modelo de embeddings MUST implicar o reprocessamento dos vetores afetados.
- Tamanho de chunk, sobreposição, topK, limiares de similaridade e demais parâmetros de recuperação MUST ser definidos na especificação ou no plano técnico e avaliados com exemplos representativos, com o resultado registrado. Valores MUST NOT ser adotados sem essa avaliação.

**Justificativa.** Os manuais mudam entre versões. Aplicar a versão errada produz apontamentos incorretos, e parâmetros escolhidos sem avaliação degradam a recuperação sem que ninguém perceba.

**Verificação**

- Restrições de banco impedem documento ou chunk sem os metadados obrigatórios.
- Testes de recuperação com duas versões do mesmo manual confirmam o filtro de versão.
- O relatório de avaliação dos parâmetros está anexado ao plano técnico.

### P12 — Saída estruturada, versionada e validada

**Regra**

- A resposta do LLM MUST seguir um contrato JSON explícito, versionado e mantido no repositório.
- O modelo configurado MUST suportar o mecanismo de saída estruturada adotado. Um modelo sem esse suporte MUST NOT ser selecionado sem ADR.
- Antes de aceitar a resposta, o sistema MUST validar a sintaxe, a aderência ao schema e as regras semânticas: P04, P07, P09 e as demais definidas na especificação.
- Respostas inválidas MUST seguir uma política de retentativa limitada e configurável. Esgotadas as tentativas, a etapa de análise MUST ser registrada como falha técnica. O resultado MUST manter os achados determinísticos já obtidos e MUST NOT incluir conteúdo da resposta rejeitada.
- Toda tentativa, válida ou inválida, MUST ser registrada para fins de consumo (P17) e auditoria (P19).

**Justificativa.** Integrações e relatórios dependem de estrutura previsível. Validar antes de aceitar impede que respostas malformadas ou incoerentes cheguem ao usuário.

**Verificação**

- Testes de contrato cobrem respostas válidas, malformadas e semanticamente inválidas.
- Um teste confirma o limite de retentativas e o resultado após esgotá-las.
- Cada resultado informa a versão do contrato.

### P13 — Metadados operacionais pertencem ao sistema

**Regra**

- O JSON produzido pelo LLM e o envelope retornado pela API MUST ser contratos distintos.
- O sistema MUST preencher os identificadores de execução e correlação, a versão do contrato, o status do processamento, o consumo de tokens, os custos, os tempos, as datas e as versões de componentes.
- O contrato do LLM MUST NOT conter campos para esses metadados. Valores dessa natureza produzidos pelo modelo MUST ser descartados.

**Justificativa.** O modelo não conhece esses valores e, se solicitado, os inventaria.

**Verificação**

- Revisão do schema do contrato do LLM.
- Um teste insere metadados na resposta simulada e confirma que o envelope usa apenas os valores do sistema.

### P14 — Ferramentas sob controle explícito

**Regra**

- Cada ferramenta exposta ao LLM por Function Calling, ou consumida por MCP, MUST possuir:
  - nome único e responsabilidade clara;
  - contratos de entrada e saída;
  - validação de argumentos;
  - regras de autenticação e autorização;
  - timeout e tratamento de falhas;
  - política de retentativa e idempotência, quando aplicável;
  - testes;
  - registro de execução.
- A seleção de uma ferramenta pelo LLM MUST NOT conceder autorização para executá-la. O sistema MUST verificar a autorização a cada chamada, considerando usuário, organização, execução e a política da ferramenta.
- Argumentos gerados pelo LLM MUST ser validados como entrada não confiável antes da execução.
- Ferramentas com efeitos externos (escrita, envio de dados a terceiros ou custo) MUST ser identificadas como tal na especificação e exigir a autorização explícita definida nela.
- Os resultados das ferramentas MUST ser registrados para que possam ser citados e verificados (P07).

**Justificativa.** Function Calling transforma texto do modelo em ações. Sem controle, uma injeção ou uma alucinação vira uma execução indevida.

**Verificação**

- O catálogo de ferramentas contém os campos obrigatórios para cada ferramenta.
- Cada ferramenta tem testes de autorização negada, argumento inválido e timeout.

### P15 — Fronteiras de serviço e propriedade dos dados

**Regra**

- A comunicação funcional entre Api.Agente e Api.RAG MUST ocorrer por MCP, com a Api.Agente como cliente e a Api.RAG como servidor.
- Cada serviço MUST ser proprietário dos seus dados. A Api.Agente MUST NOT acessar diretamente bancos, tabelas, vetores ou arquivos internos da Api.RAG, e a Api.RAG MUST NOT acessar os dados internos da Api.Agente.
- A Api.Agente MUST NOT usar a API REST administrativa da Api.RAG para recuperar conhecimento.

**Justificativa.** Fronteiras explícitas permitem evoluir, escalar e proteger cada serviço de forma independente, e permitem reutilizar o RAG com outros agentes.

**Verificação**

- A Api.Agente não possui string de conexão, credencial, migração ou referência de projeto que acesse dados da Api.RAG.
- Testes de arquitetura automatizados verificam as referências entre projetos.
- A configuração de cada ambiente é revisada quanto a credenciais cruzadas.

### P16 — Camadas e simplicidade justificada

**Regra**

- Cada serviço MUST separar endpoints, aplicação, domínio e infraestrutura, com dependências direcionadas ao domínio.
- O domínio MUST NOT depender de ASP.NET Core, Semantic Kernel, SDKs de provedores de IA, SDK MCP, drivers de banco ou outras bibliotecas de infraestrutura.
- Regras de validação e cálculo de status MUST residir no domínio ou na aplicação, nunca em endpoints ou prompts.
- Projetos, bibliotecas, serviços ou padrões adicionais MUST ser justificados no plano técnico ou em ADR, com o problema, as alternativas consideradas e o custo de manutenção. Abstrações sem uso concreto atual MUST NOT ser introduzidas.

**Justificativa.** Separar regras de negócio de detalhes técnicos permite testá-las isoladamente e trocar provedores. Limitar estruturas evita complexidade sem retorno.

**Verificação**

- Testes de arquitetura automatizados verificam as dependências proibidas.
- O checklist de PR exige justificativa para cada novo projeto ou pacote.

### P17 — Consumo e custos registrados com fidelidade

**Regra**

- Cada chamada a provedor de IA, incluindo geração, embeddings, retentativas e chamadas auxiliares, MUST ser registrada em PostgreSQL pelo serviço que a realiza, com os campos de GD-10 que o provedor disponibilizar.
- Valores informados pelo provedor e valores estimados pelo sistema MUST ser distinguidos.
- Um consumo desconhecido MUST ser registrado como desconhecido e MUST NOT ser registrado como zero.
- As tabelas de preços MUST ser versionadas, com vigência. Cada estimativa MUST referenciar a versão utilizada.
- Uma falha no registro de consumo MUST NOT ser silenciosa: ela MUST gerar registro de erro e métrica observável.

**Justificativa.** Custos de IA variam por modelo e por volume. Dados fiéis permitem controlar o orçamento e identificar desperdícios, como retentativas excessivas.

**Verificação**

- Testes confirmam um registro por tentativa, inclusive as que falharam.
- O esquema do banco permite valor nulo apenas acompanhado do indicador de desconhecido.
- A revisão confirma a referência à tabela de preços em cada estimativa.

### P18 — Memória isolada e separada do conhecimento oficial

**Regra**

- A memória conversacional MUST ser isolada por usuário e conversa, e por organização quando aplicável. Consultas MUST NOT retornar dados de outro escopo.
- A política de retenção, expiração e exclusão MUST estar documentada antes do uso com dados reais.
- O histórico enviado ao modelo MUST ser limitado por parâmetro configurável definido no plano técnico.
- Dados sensíveis MUST ser protegidos por minimização, mascaramento ou criptografia, conforme ADR.
- A memória MUST NOT ser usada como fonte de regra, evidência ou referência. O conhecimento oficial provém exclusivamente da base gerida pela Api.RAG.
- A tecnologia de armazenamento (MongoDB ou Redis) MUST ser decidida em ADR antes da implementação (DP-01).

**Justificativa.** Conversas podem conter dados fiscais sigilosos de clientes. Misturar memória e conhecimento oficial contaminaria as evidências.

**Verificação**

- Testes de isolamento entre usuários, conversas e organizações.
- Teste de expiração conforme a política.
- A validação semântica rejeita referências que apontem para a memória.

### P19 — Execução auditável

**Regra**

- Cada execução MUST registrar o suficiente para explicar seu resultado:
  - hash do arquivo recebido;
  - tipo e versão de leiaute identificados;
  - versões dos validadores e das regras, do contrato, do template de prompt e da configuração do modelo;
  - identificadores dos chunks recuperados e das versões dos documentos;
  - chamadas de ferramentas, com argumentos e resultados, respeitando SQ-04;
  - tentativas do LLM;
  - estados das etapas (P03).
- Prompts e templates MUST ser versionados no repositório e revisados como código.

**Justificativa.** Respostas de LLM não são determinísticas. Sem esses registros, não é possível investigar um resultado contestado.

**Verificação**

- Um teste de integração confirma a presença desses registros em uma execução completa.
- A revisão de PR exige nova versão para cada alteração de prompt ou template.

### P20 — Especificação antes da implementação

**Regra**

- Uma funcionalidade MUST NOT ser implementada sem uma mudança OpenSpec com especificação aprovada, esclarecimentos resolvidos ou registrados como não bloqueantes, plano técnico e tarefas (seção 6).
- Cada requisito MUST ter identificador estável e critérios de aceite verificáveis na forma de cenários.
- Tarefas, testes e PRs MUST referenciar os requisitos que atendem.
- Uma alteração de comportamento MUST atualizar a especificação no mesmo ciclo.
- Refatorações sem mudança de comportamento e correções de defeito contra requisito existente MAY dispensar nova mudança OpenSpec, desde que o PR referencie o requisito afetado.

**Justificativa.** A rastreabilidade entre requisito, tarefa e teste torna as revisões objetivas e evita funcionalidades sem fundamento.

**Verificação**

- `openspec validate --strict` é executado sem erros.
- O `tasks.md` liga cada tarefa e cada teste a um requisito.
- Um PR sem referência a requisito é rejeitado.

---

## 3. Restrições arquiteturais

**RA-01 — Plataforma.** A solução MUST usar .NET 10 com ASP.NET Core. Versões de SDKs e de pacotes MUST ser definidas no plano técnico. A troca de plataforma exige emenda MAJOR.

**RA-02 — Serviços principais.** A solução MUST ser composta pelos serviços abaixo. Uma responsabilidade MUST NOT migrar entre serviços sem ADR.

| Serviço | Responsabilidades |
| --- | --- |
| **Api.Agente** | API REST para execução do agente; recepção de arquivos e solicitações de análise; orquestração do fluxo de validação (P03); integração com a OpenAI; consulta à Api.RAG como cliente MCP; Semantic Kernel para integração e execução de ferramentas por Function Calling; memória conversacional; registro de consumo de tokens e custos; validação da resposta estruturada do LLM; servidor A2A. |
| **Api.RAG** | API REST para administração da base de conhecimento; endpoint POST de ingestão; processamento de documentos, geração de chunks e embeddings; armazenamento e consulta de vetores em PostgreSQL com PgVector; servidor MCP com ferramentas de recuperação de conhecimento. |

**RA-03 — MCP.** A Api.RAG MUST expor a recuperação de conhecimento como ferramentas MCP com contratos versionados (GD-01). Essas ferramentas MUST aceitar filtros de tipo de documento e de versão (P11) e retornar os metadados de referência (P06). A chamada entre serviços MUST ser autenticada (SQ-01).

**RA-04 — API REST da Api.RAG.** A API REST da Api.RAG destina-se à administração da base de conhecimento e à ingestão. Seu acesso MUST ser restrito a perfis administrativos (SQ-02).

**RA-05 — Persistência.**

| Serviço | Dados | Armazenamento |
| --- | --- | --- |
| Api.RAG | Documentos, chunks, embeddings e metadados de ingestão. | PostgreSQL com PgVector. |
| Api.Agente | Consumo e custos. | PostgreSQL. |
| Api.Agente | Memória conversacional. | MongoDB ou Redis, conforme ADR (DP-01). |
| Api.Agente | Execuções, achados e resultados. | Definido no plano técnico (DP-25). |

Os serviços MAY compartilhar infraestrutura física de banco somente com bancos ou esquemas distintos, credenciais distintas e nenhuma permissão cruzada.

**RA-06 — Integração com LLM.** A integração MUST ser feita com a OpenAI, por adaptadores na camada de infraestrutura, usando Semantic Kernel para Function Calling. Modelo, parâmetros de execução, endpoints e chaves MUST vir de configuração externa e MUST NOT ser fixados no código. Tipos do Semantic Kernel e do SDK do provedor MUST NOT aparecer no domínio. A adoção de outro provedor exige ADR e emenda MINOR.

**RA-07 — A2A.** O servidor A2A da Api.Agente MUST reutilizar o mesmo fluxo de aplicação (P03), as mesmas regras de autenticação e autorização e o mesmo contrato de resultado da API REST. Ele MUST NOT expor capacidade que não esteja especificada. A consumidora prevista é a Sofia INFIS; o contrato A2A MUST ser especificado considerando os requisitos de integração dela. O escopo e a priorização do A2A dependem de DP-16.

**RA-08 — Camadas.** As dependências MUST seguir a direção `Endpoints → Aplicação → Domínio`. A Infraestrutura implementa as abstrações definidas pela Aplicação ou pelo Domínio. A organização física, em projetos ou em pastas, é definida no plano técnico da fundação (DP-17), e testes de arquitetura MUST verificá-la.

**RA-09 — Configuração.** As configurações MUST ser externas ao código e validadas na inicialização. Uma configuração obrigatória ausente ou inválida MUST impedir a inicialização do serviço. O repositório MUST NOT conter segredos (SQ-03).

**RA-10 — Compartilhamento entre serviços.** Api.Agente e Api.RAG MUST NOT compartilhar biblioteca com lógica de domínio ou acesso a dados. Contratos MAY ser compartilhados por schemas ou artefatos gerados, com justificativa em ADR.

**RA-11 — Arquivos grandes.** Limites de tamanho e o modelo de processamento, síncrono ou assíncrono, MUST ser definidos na especificação de recepção (DP-05 e DP-22) antes da implementação da etapa 2 do fluxo.

---

## 4. Contratos e governança dos dados

**GD-01 — Contratos versionados.** Os contratos abaixo MUST ser explícitos, versionados e mantidos no repositório: API REST (OpenAPI), ferramentas MCP, ferramentas de Function Calling, contrato do LLM, envelope da API e cartão do agente A2A.

- O versionamento MUST seguir SemVer. Uma mudança incompatível MUST gerar nova versão MAJOR, com plano de convivência ou de migração.
- A alteração de um contrato MUST estar prevista na especificação da mudança.
- Cada resultado MUST informar as versões do contrato do LLM e do envelope.

**GD-02 — Envelope da API e contrato do LLM.** A especificação do contrato MUST contemplar os elementos abaixo, respeitando a origem indicada.

| Elemento | Produzido por | Observação |
| --- | --- | --- |
| Identificador da execução e da correlação | Sistema | P13. |
| Versões dos contratos e dos componentes | Sistema | P13, P19. |
| Status do processamento | Sistema | Cálculo determinístico (P05, GD-03). |
| Erros estruturais e inconsistências de negócio | Sistema (validadores e ferramentas) | P02, P04. |
| Indícios identificados pela IA | LLM | Sempre com evidência citada (P06, P07). |
| Síntese do resultado da análise | LLM | MUST NOT contradizer os achados determinísticos nem o status. |
| Severidade de cada achado | Regra do sistema ou LLM dentro da escala | GD-05. |
| Evidências e referências | LLM cita; sistema valida e resolve | P07. |
| Limitações e validações não realizadas | Sistema (estados das etapas) e LLM (limitações analíticas) | P08. |
| Necessidade de revisão humana | Sistema; o LLM MAY apenas recomendar | P09. |
| Consumo, custos, tempos e datas | Sistema | P13, P17. |

**GD-03 — Status do processamento.** A especificação MUST definir a enumeração de status e sua regra de cálculo (DP-08). A enumeração MUST distinguir, no mínimo:

- entrada rejeitada na etapa 2;
- análise concluída com todas as validações obrigatórias;
- análise parcial, com validações obrigatórias não concluídas ou limitações;
- falha técnica que impediu a análise.

**GD-04 — Categorias de resultado.**

| Categoria | Definição | Origem permitida | Efeito mínimo |
| --- | --- | --- | --- |
| Erro estrutural do documento | Violação de leiaute, schema ou estrutura definidos em fonte oficial. | Validador estrutural. | Impede declarar conformidade. |
| Inconsistência de negócio | Violação de regra de negócio determinística registrada em especificação, incluindo cruzamentos entre registros. | Validador ou ferramenta determinística. | Impede declarar conformidade. |
| Indício identificado pela IA | Possível problema apontado pelo LLM, sem comprovação por regra determinística. | LLM, com evidência verificada. | Exige revisão humana (P09). |
| Ausência de evidências | Verificação que não pôde ser concluída por falta de dados ou de conhecimento aplicável. | Sistema ou LLM. | Exige revisão humana e entra nas limitações (P08). |
| Falha técnica ou dependência indisponível | Erro de processamento ou indisponibilidade de serviço, ferramenta ou provedor. | Sistema. | Impede declarar conformidade e exige revisão humana. |

**GD-05 — Severidade.** A escala e seus critérios MUST ser definidos na especificação (DP-07). Para achados determinísticos, a severidade MUST estar definida na regra correspondente. Para indícios, o LLM MAY propor uma severidade dentro da escala, e o sistema MUST validá-la. A severidade MUST NOT substituir a categoria.

**GD-06 — Documentos recebidos.**

- O sistema MUST registrar o hash de cada arquivo recebido.
- Limites de tamanho, formatos aceitos e condições de processamento MUST estar na especificação (DP-05).
- O armazenamento e a retenção dos arquivos MUST seguir a política de DP-11.
- O sistema MUST enviar ao LLM somente os trechos necessários à análise, conforme definido na especificação. O envio de dados reais ao provedor depende de DP-12.
- Arquivos reais de clientes MUST NOT ser versionados no repositório nem usados em testes sem anonimização (SQ-10).

**GD-07 — Base de conhecimento.** Os metadados mínimos são:

| Entidade | Metadados obrigatórios |
| --- | --- |
| Documento | Identificador; emissor ou origem; título; tipo de documento a que se aplica; versão; vigência, quando disponível; hash do conteúdo; data de ingestão; estado (vigente ou substituído). |
| Chunk | Identificador; documento e versão; localização (página, seção, registro ou campo), quando disponível; hash do conteúdo; modelo e dimensão do embedding. |

- A ingestão MUST ser idempotente: o mesmo conteúdo, identificado por hash, MUST NOT gerar duplicatas.
- Uma versão substituída MUST continuar recuperável para auditoria de execuções anteriores, enquanto a política de retenção permitir, e MUST NOT ser retornada como vigente.
- Os critérios de segmentação, deduplicação, reprocessamento e atualização MUST estar documentados na especificação de ingestão (DP-03).

**GD-08 — Avaliação da recuperação.** Os parâmetros de P11 MUST ser avaliados com um conjunto representativo de consultas e respostas esperadas (DP-24), usando métricas definidas no plano técnico. O resultado MUST ser registrado e reavaliado a cada mudança de modelo de embeddings, de segmentação ou de estratégia de busca.

**GD-09 — Memória conversacional.** Aplicam-se as regras de P18. O histórico conversacional e o conhecimento oficial MUST ficar em armazenamentos e contratos distintos.

**GD-10 — Registro de consumo.** Por chamada, quando disponibilizado pelo provedor:

| Campo | Regra |
| --- | --- |
| Execução e correlação | Obrigatório. |
| Provedor e modelo | Obrigatório. |
| Tipo de chamada (geração, embedding, retentativa, auxiliar) | Obrigatório. |
| Tokens de entrada e de saída | Valor informado pelo provedor ou desconhecido; nunca zero por omissão. |
| Consumo de embeddings | Quando aplicável. |
| Data, duração e status | Obrigatório. |
| Custo estimado e referência da tabela de preços | Obrigatório quando houver tabela vigente para o modelo; caso contrário, desconhecido. |
| Origem de cada valor (informado ou estimado) | Obrigatório. |

A consolidação do consumo da Api.RAG, com embeddings de ingestão e de consulta, é decidida em DP-26, respeitando P15.

**GD-11 — Retenção e exclusão.** Cada armazenamento (arquivos, resultados, memória, logs, consumo, base de conhecimento) MUST ter política de retenção e exclusão aprovada antes do uso em produção (DP-11).

---

## 5. Segurança e qualidade

**SQ-01 — Autenticação.** Todos os pontos de entrada (REST, MCP e A2A) MUST exigir autenticação. Somente verificações de saúde que não exponham dados MAY ser anônimas. O mecanismo é definido em ADR (DP-09).

**SQ-02 — Autorização.** Cada operação e cada chamada de ferramenta MUST verificar a autorização por usuário e organização, segundo o princípio do menor privilégio. Funções administrativas da Api.RAG MUST exigir perfil administrativo.

**SQ-03 — Segredos.** Segredos MUST ser obtidos de armazenamento seguro de segredos. Eles MUST NOT aparecer no repositório, em logs, em prompts ou em respostas. A rotação de um segredo MUST NOT exigir alteração de código.

**SQ-04 — Dados sensíveis.** Os dados tratados MUST ser classificados quanto à sensibilidade. Logs MUST NOT conter o conteúdo integral de arquivos, prompts com dados fiscais completos ou segredos. Identificadores sensíveis MUST ser mascarados nos logs. A base legal e a política de tratamento dependem de DP-11 e DP-12.

**SQ-05 — Entrada segura.**

- Tamanho e formato MUST ser validados antes do processamento completo.
- O processamento MUST impor limites contra esgotamento de recursos.
- Parsers XML MUST ter a resolução de DTD e de entidades externas desabilitada.
- Arquivos compactados, se aceitos, MUST ter limites de expansão.
- Nomes e conteúdos de arquivos MUST NOT ser usados para compor caminhos, comandos ou consultas sem validação e parametrização.

**SQ-06 — Resiliência.** Toda chamada externa (OpenAI, MCP, ferramentas e bancos) MUST ter timeout. Retentativas MUST ser limitadas, com espera progressiva, e aplicadas somente a erros transitórios em operações idempotentes. Uma falha MUST resultar em achado de falha técnica (P04).

**SQ-07 — Observabilidade.** Os logs MUST ser estruturados. O identificador de correlação MUST ser propagado entre REST, MCP, A2A e chamadas ao provedor. Os serviços MUST expor métricas de latência, erros, tokens e custos, sem dados sensíveis.

**SQ-08 — Qualidade de código.** O CI MUST executar build, testes e analisadores. A configuração de analisadores, formatação e tratamento de avisos MUST ser definida no plano técnico da fundação (DP-17). Avisos configurados como bloqueantes MUST NOT ser suprimidos sem justificativa no PR.

**SQ-09 — Estratégia de testes.**

| Tipo | Obrigatoriedade |
| --- | --- |
| Unitário de validadores | Cada regra determinística com pelo menos um caso válido e um inválido. |
| Contrato | Contrato do LLM, envelope, OpenAPI, ferramentas MCP e de Function Calling e cartão A2A, sempre que o contrato mudar. |
| Integração | PostgreSQL/PgVector, armazenamento de memória e MCP entre os serviços, com instâncias reais em ambiente isolado. Bancos MUST NOT ser simulados em testes de integração. |
| Adversarial | Injeção de prompt, referência fabricada e argumento inválido, sempre que a mudança tocar LLM, RAG, prompts ou ferramentas. |
| Avaliação da recuperação | GD-08, sempre que mudarem parâmetros de RAG. |
| Avaliação da análise | Conjunto de exemplos com resultados esperados (DP-24), sempre que mudarem modelo, prompt ou contrato do LLM, com o custo da avaliação registrado. |

O CI MUST NOT depender de chamadas reais à OpenAI. Os testes automatizados MUST usar respostas simuladas ou gravadas. As avaliações com o modelo real são execuções separadas.

**SQ-10 — Dados de teste.** Os dados de teste MUST ser sintéticos ou anonimizados e MUST indicar o requisito e a fonte que representam. Arquivos reais de clientes MUST NOT ser versionados.

**SQ-11 — Dependências.** Os pacotes MUST vir de fontes confiáveis, com versões fixadas, e passar por verificação de vulnerabilidades no CI. Cada nova dependência MUST ser justificada (P16).

---

## 6. Fluxo de desenvolvimento SDD

### 6.1 Etapas, artefatos e critérios de saída

Fluxo obrigatório: **constitution → especificação → esclarecimentos → plano técnico → tarefas → implementação → validação**.

| Etapa | Artefato e comando (OpenSpec) | Conteúdo mínimo | Critério para avançar |
| --- | --- | --- | --- |
| 1. Constitution | `openspec/constitution.md` | Este documento. | Ratificada (seção 8.1). |
| 2. Especificação | `proposal.md` e `specs/<capacidade>/spec.md`, criados com `/opsx:new` e `/opsx:continue` | Intenção, escopo e fora de escopo; fontes oficiais; requisitos com identificador; cenários. | Todo requisito de domínio tem fonte; todo requisito tem cenário verificável. |
| 3. Esclarecimentos | Seção "Esclarecimentos" do `proposal.md` | Pergunta, resposta, fonte ou responsável, data e indicação de bloqueio. | Nenhum esclarecimento bloqueante em aberto; especificação aprovada. |
| 4. Plano técnico | `design.md` (`/opsx:continue`) e ADRs | Abordagem, decisões, contratos, dados, parâmetros com plano de avaliação, estratégia de testes e verificação da constitution (6.3). | Verificação da constitution aprovada; ADRs necessários aceitos. |
| 5. Tarefas | `tasks.md` (`/opsx:continue`) | Tarefas pequenas com identificadores de requisito; tarefas de teste por cenário; atualização de contratos e documentação. | Toda tarefa ligada a requisito; todo cenário com teste previsto. |
| 6. Implementação | Código e PR (`/opsx:apply`) | Código, testes e contratos. | Definition of Done (seção 7). |
| 7. Validação | `/opsx:verify`, testes, `openspec validate --strict`, revisão, aceite e arquivamento (`/opsx:archive`) | Evidências de testes e aceite. | Specs vigentes atualizadas; mudança arquivada. |

A ferramenta de apoio adotada é o OpenSpec. As convenções concretas de cada artefato ficam em `openspec/config.yaml`, que MUST permanecer coerente com esta seção.

O fluxo padrão é passo a passo, com `/opsx:new` e `/opsx:continue`, para que os esclarecimentos sejam resolvidos antes do `design.md`. Os comandos `/opsx:propose` e `/opsx:ff` geram todos os artefatos de uma vez e MAY ser usados somente em mudanças sem regra de domínio nova. Se surgir esclarecimento bloqueante nesses casos, `design.md` e `tasks.md` MUST ser refeitos depois da resposta.

A ordem das mudanças, suas dependências e os prefixos de requisito por capacidade ficam em `docs/roteiro.md`. As skills do projeto (`.claude/skills/`) MUST ser usadas nas mudanças e nos grupos de tarefas indicados no roteiro e no `tasks.md`. O plano técnico MUST confirmar as pré-condições que cada skill declara; uma skill nunca prevalece sobre esta constitution.

### 6.2 Convenções das especificações

- Especificações MUST ser escritas em português, mantendo os cabeçalhos estruturais do OpenSpec e as palavras-chave `SHALL`/`MUST` em inglês.
- Cada requisito MUST ter identificador estável e único, no formato definido em `openspec/config.yaml`. Identificadores MUST NOT ser reutilizados.
- Cada requisito de domínio MUST citar sua fonte oficial (P01).
- Cada requisito MUST ter ao menos um cenário verificável, incluindo os casos de falha e de ausência de evidência pertinentes.

### 6.3 Verificação da constitution no plano técnico

Todo `design.md` MUST conter a seção "Verificação da constitution", com uma linha por princípio (P01 a P20), no formato:

| Princípio | Situação | Evidência ou justificativa |
| --- | --- | --- |
| P01 | Atende / Não se aplica / Desvio | Onde e como a mudança atende; por que não se aplica; ou o identificador do desvio registrado. |

### 6.4 Esclarecimentos

- Uma dúvida que afete escopo, comportamento observável, compatibilidade ou critério de aceite MUST ser resolvida antes do plano técnico.
- Cada resposta MUST registrar seu responsável e a fonte, quando houver.
- Uma dúvida de domínio sem resposta MUST virar decisão pendente ou esclarecimento aberto. Ela MUST NOT ser substituída por suposição.

### 6.5 ADRs

- Os ADRs ficam em `docs/adr/NNNN-titulo.md`, conforme o modelo de `docs/adr/README.md`.
- Conteúdo mínimo: contexto, decisão, alternativas consideradas, consequências, princípios relacionados, estado, data e responsáveis.
- Estados possíveis: Proposto, Aceito, Rejeitado e Substituído.
- Um ADR aceito MUST NOT ser reescrito. A mudança de decisão MUST gerar novo ADR que substitui o anterior.
- Um ADR é obrigatório para:
  - tecnologia de armazenamento;
  - mecanismo de autenticação;
  - novo projeto, biblioteca compartilhada ou serviço;
  - escolha ou troca de modelo de embeddings;
  - modelo de LLM sem saída estruturada;
  - troca de provedor;
  - versão MAJOR de contrato;
  - política de dados com o provedor de IA;
  - hospedagem;
  - qualquer decisão pendente da seção 9 marcada com o instrumento ADR.

### 6.6 Revisão de pull requests

- Todo PR MUST referenciar a mudança OpenSpec e os requisitos atendidos.
- Todo PR MUST declarar os princípios impactados e apresentar as evidências de teste.
- Todo PR MUST indicar os desvios registrados, se houver.
- O PR MUST ser aprovado por pessoa diferente do autor. Quando o autor for o assistente de IA, o responsável pelo projeto MAY ser o revisor. PRs escritos pelo responsável MUST ser revisados por outra pessoa.
- Como os PRs são abertos com a conta GitHub do responsável, a aprovação MUST ser registrada por comentário de revisão no PR, seguido do merge.
- Em 2026-10-29, o responsável delegou ao assistente de IA a validação e o merge de todos os PRs. Ao usar a delegação, o assistente MUST registrar no comentário de revisão a delegação, as verificações executadas e o resultado, e MUST NOT mesclar com check obrigatório falho ou pendente. O responsável MAY revogar ou restringir a delegação a qualquer momento.
- Uma violação desta constitution sem desvio registrado MUST bloquear o merge.

### 6.7 Desvios

- Todo desvio MUST ser registrado em `docs/governanca/desvios.md` antes do merge, com: identificador, princípio ou regra afetada, descrição, justificativa, impacto, responsável, aprovador e data da aprovação, tipo (temporário ou permanente), prazo de revisão (obrigatório se temporário), plano de remoção e estado.
- O autor da mudança MUST NOT aprovar o próprio desvio.
- Um desvio temporário vencido MUST ser renovado formalmente ou eliminado. Enquanto estiver vencido, PRs que dependam dele MUST NOT ser aprovados.
- Um desvio permanente MUST motivar a avaliação de emenda a esta constitution.

### 6.8 Versionamento

- O repositório oficial é privado, no GitHub: `kaiosmacedo/Agente-ECF`.
- A branch `main` é a linha principal. Ela MUST receber alterações somente por PR, exceto o commit inicial de criação do repositório.
- Cada mudança OpenSpec MUST ser desenvolvida em branch própria, nomeada `feat/<id-da-mudança>`. Correções sem nova mudança (P20) usam `fix/<descrição>`, alterações de documentação ou governança usam `docs/<descrição>` e alterações de pipeline usam `ci/<descrição>`. Branches abertas pelo Dependabot (`dependabot/...`) são aceitas.
- A especificação inicial de várias mudanças MAY ser proposta em conjunto numa branch `docs/roteiro-<descrição>`, contendo apenas a proposta e as specs de cada mudança. Esclarecimentos, plano técnico, tarefas e implementação de cada mudança MUST seguir em sua própria branch `feat/<id-da-mudança>`.
- Os checks `OpenSpec` e `.NET` do workflow `CI` MUST passar antes de qualquer merge na `main`. O pipeline está descrito em `docs/ci-cd.md`.
- Mensagens de commit MUST seguir Conventional Commits, com escopo de componente, por exemplo `feat(rag): ...` ou `docs(constitution): ...`.
- O procedimento operacional fica na skill `github-versionamento` do projeto.

---

## 7. Definition of Done

Uma mudança só é considerada concluída quando todos os itens aplicáveis abaixo forem atendidos. Itens não aplicáveis MUST ser justificados no PR.

**Especificação e plano**

- [ ] **DoD-01** Especificação aprovada, com requisitos identificados, fontes oficiais citadas e cenários verificáveis.
- [ ] **DoD-02** Nenhum esclarecimento bloqueante em aberto; decisões pendentes afetadas atualizadas.
- [ ] **DoD-03** `design.md` com a verificação da constitution aprovada e ADRs necessários aceitos.
- [ ] **DoD-04** `tasks.md` com todas as tarefas concluídas e ligadas a requisitos.

**Implementação**

- [ ] **DoD-05** Código nas camadas corretas (P16, RA-08), com testes de arquitetura aprovados.
- [ ] **DoD-06** Regras determinísticas em validadores, nenhuma regra apenas em prompt (P02).
- [ ] **DoD-07** Contratos alterados versionados e publicados (GD-01).
- [ ] **DoD-08** Configurações externas, sem segredos ou modelos fixados no código (RA-06, RA-09, SQ-03).

**Testes**

- [ ] **DoD-09** Cada cenário com teste automatizado correspondente, aprovado em CI.
- [ ] **DoD-10** Testes unitários, de contrato e de integração exigidos por SQ-09 aprovados.
- [ ] **DoD-11** Testes adversariais aprovados quando a mudança tocar LLM, RAG, prompts ou ferramentas.
- [ ] **DoD-12** Avaliação de recuperação ou de análise registrada quando houver mudança de parâmetros, modelo, prompt ou contrato.
- [ ] **DoD-13** `openspec validate --strict` sem erros.

**Segurança, dados e operação**

- [ ] **DoD-14** Autenticação e autorização testadas para os novos pontos de entrada e ferramentas (SQ-01, SQ-02, P14).
- [ ] **DoD-15** Logs sem dados sensíveis e correlação propagada (SQ-04, SQ-07).
- [ ] **DoD-16** Consumo registrado para toda chamada a provedor, inclusive retentativas (P17).
- [ ] **DoD-17** Dados de teste sintéticos ou anonimizados (SQ-10).

**Documentação e encerramento**

- [ ] **DoD-18** Documentação e nota do projeto no cofre Obsidian atualizadas com o estado entregue.
- [ ] **DoD-19** Desvios registrados e aprovados, quando houver (6.7).
- [ ] **DoD-20** PR aprovado por revisor diferente do autor.
- [ ] **DoD-21** Mudança arquivada no OpenSpec após o aceite, com as specs vigentes atualizadas.

---

## 8. Governança e alterações

### 8.1 Ratificação

- A constitution MUST ser ratificada pelos responsáveis definidos em DP-19, com registro de data e aprovadores no histórico de versões.
- Especificações MUST NOT ser aprovadas antes da ratificação.

### 8.2 Versionamento

A constitution segue versionamento semântico:

| Tipo | Quando usar |
| --- | --- |
| MAJOR | Remoção ou redefinição incompatível de princípio, restrição ou critério obrigatório. |
| MINOR | Inclusão de princípio, restrição ou seção, ou ampliação material de uma regra existente. |
| PATCH | Esclarecimento de redação, correção ou ajuste sem efeito normativo. |

### 8.3 Processo de emenda

Toda alteração MUST ser feita por PR que contenha:

- nova versão e data;
- descrição da alteração e justificativa;
- avaliação de impacto, listando especificações vigentes, mudanças em andamento, ADRs e desvios afetados, com a ação prevista para cada um;
- aprovação dos responsáveis (DP-19).

Uma emenda MAJOR MUST incluir plano de adequação das especificações afetadas, com prazo. A aprovação de uma emenda MUST atualizar o histórico de versões.

### 8.4 Revisão periódica

A constitution MUST ser revisada sempre que o projeto adotar nova versão do leiaute da ECF e sempre que uma emenda MAJOR for proposta. Cada revisão MUST verificar os desvios a vencer, as decisões pendentes e a coerência com `openspec/config.yaml`.

### 8.5 Sínteses e cópias

Sínteses desta constitution, como a nota do cofre Obsidian, MUST indicar a versão resumida e o link para este arquivo. Elas MUST NOT introduzir regras que não estejam aqui.

### 8.6 Histórico de versões

| Versão | Data | Alteração | Aprovação |
| --- | --- | --- | --- |
| 1.0.0 | 2026-10-29 | Versão inicial proposta para validação. | Pendente (DP-19). |
| 1.0.1 | 2026-10-29 | PATCH: palavras-chave normativas passam de DEVE/NÃO DEVE/PODE para MUST/MUST NOT/MAY, alinhadas às especificações OpenSpec. Sem efeito normativo; nenhuma especificação existente afetada. | Texto aprovado por Kaio Macedo em 2026-10-29. |
| 1.1.0 | 2026-10-29 | MINOR: Sofia INFIS como orquestradora e consumidora A2A (1.1, 1.2, RA-07); análise sob demanda (1.2); só validação nesta fase (1.3); fluxo passo a passo no OpenSpec (6.1); revisão de PR pelo responsável quando o autor for o assistente de IA (6.6); versionamento no GitHub (6.8); revisão da constitution a cada novo leiaute (8.4); DP-19, DP-20 e DP-21 resolvidas; fonte inicial em DP-04; referências para DP-01, DP-16 e DP-18. Nenhuma especificação existente afetada. | Ratificada por Kaio Macedo em 2026-10-29. |
| 1.2.0 | 2026-10-29 | MINOR: delegação ao assistente de IA da validação e do merge de PRs (6.6); branches `ci/` e do Dependabot e checks obrigatórios do CI (6.8); decisões do responsável para DP-17 e DP-18, a formalizar em ADR. Nenhuma especificação existente afetada. | Decisões aprovadas por Kaio Macedo em 2026-10-29; merge por delegação (6.6). |
| 1.3.0 | 2026-10-29 | MINOR: roteiro em `docs/roteiro.md` e uso obrigatório das skills do projeto indicadas no roteiro e no `tasks.md` (6.1); especificação conjunta de várias mudanças numa branch `docs/roteiro-*` (6.8). Nenhuma especificação vigente afetada. | Aprovada por Kaio Macedo em 2026-10-29 (PR #4). |

---

## 9. Decisões pendentes

As decisões abaixo não estão definidas. Elas MUST NOT ser tratadas como requisitos aprovados até serem resolvidas no instrumento e na etapa indicados.

| ID | Decisão | Etapa de resolução | Instrumento | Bloqueia |
| --- | --- | --- | --- | --- |
| DP-01 | Armazenamento da memória conversacional: MongoDB ou Redis. Referência a avaliar: o projeto AgenteAnaliseDIMP usa MongoDB Atlas. | Plano técnico da primeira mudança de memória. | ADR | Implementação da memória. |
| DP-02 | Modelos OpenAI de geração e de embeddings, parâmetros de execução e dimensão dos vetores. | Plano técnico das mudanças de análise e de RAG. | ADR (embeddings) e design | Implementação da análise e da ingestão. |
| DP-03 | Estratégia de segmentação, sobreposição, deduplicação, reprocessamento, busca (vetorial, híbrida, reordenação), topK e limiares. | Especificação e plano técnico da ingestão e recuperação, com avaliação (GD-08). | Especificação e design | Ingestão em uso real. |
| DP-04 | Fontes oficiais da ECF a ingerir. **Fonte inicial definida em 2026-10-29:** Manual da ECF, leiaute 12, publicado em 20/05/2026, para o ano-calendário 2025 e situações especiais de 2026 (dados conforme o nome do arquivo, a confirmar na ingestão), disponível na [página da ECF no portal SPED](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf) e no [arquivo do manual](https://www.gov.br/sped/pt-br/assuntos/escrituracoes-digitais/ecf/manuais-e-documentos-tecnicos/manual_ecf_leiaute_12_20_05_2026_ac_2025_sit_esp_2026.pdf/@@display-file/file). **Pendentes:** demais documentos (tabelas, regras de validação, schemas), se a cobertura inicial se limita ao leiaute 12, versões anteriores para comparação entre períodos e relação com as regras do programa validador oficial da ECF. | Esclarecimentos da primeira mudança de validação. | Especificação | Qualquer regra de domínio. |
| DP-05 | Formatos de entrada aceitos, limites de tamanho e condições de processamento. | Especificação da recepção de arquivos. | Especificação | Etapa 2 do fluxo. |
| DP-06 | Escopo das validações de IRPJ e CSLL, dos cruzamentos entre registros e da comparação com períodos anteriores e com outras escriturações, incluindo a origem desses dados. | Esclarecimentos das mudanças de regras de negócio. | Especificação | Regras de negócio correspondentes. |
| DP-07 | Escala de severidade e seus critérios. | Especificação do contrato de resultado. | Especificação | Contrato de resultado. |
| DP-08 | Enumeração dos status de processamento e regra de cálculo. | Especificação do contrato de resultado. | Especificação | Contrato de resultado. |
| DP-09 | Mecanismo de autenticação e autorização (provedor de identidade, autenticação entre serviços, MCP e A2A) e modelo de organizações. | Antes da primeira mudança que exponha endpoints. | ADR | Exposição de qualquer endpoint. |
| DP-10 | Ferramentas externas necessárias, suas fontes e suas políticas de autorização. | Especificação de cada ferramenta. | Especificação | Etapa 6 do fluxo. |
| DP-11 | Política de retenção e exclusão de arquivos, resultados, memória, logs e consumo, com a base legal (LGPD). | Antes do uso com dados reais. | ADR ou política aprovada | Uso com dados reais. |
| DP-12 | Política de dados com a OpenAI: retenção no provedor, região, uso para treinamento, minimização e anonimização. | Antes do envio de dados reais ao provedor. | ADR | Envio de dados reais ao LLM. |
| DP-13 | Fonte e moeda da tabela de preços e forma de atualização. | Plano técnico da mudança de consumo. | Design | Estimativa de custos. |
| DP-14 | Valores da política de retentativas e dos timeouts do LLM e das ferramentas. | Plano técnico das mudanças de análise e ferramentas. | Design | Configuração de produção. |
| DP-15 | Formato do relatório entregue ao usuário (apenas JSON ou também documento legível) e seu público. | Especificação do resultado. | Especificação | Etapa 9 do fluxo. |
| DP-16 | Escopo, capacidades expostas, autenticação e priorização do servidor A2A. Consumidora prevista: Sofia INFIS (1.1). | Especificação dedicada, quando priorizada. | Especificação | Implementação do A2A. |
| DP-17 | Organização física da solução (camadas em projetos ou pastas), frameworks de teste, testes de arquitetura e regras de analisadores. **Decisão do responsável em 2026-10-29:** um projeto por serviço, com camadas em pastas verificadas por testes de arquitetura; xUnit e Testcontainers. Formalização em ADR na mudança `fundacao-solucao`. | Plano técnico da mudança de fundação. | Design ou ADR | Início da implementação. |
| DP-18 | Hospedagem, ambientes, contêineres e pipeline de CI/CD. **Decisões do responsável em 2026-10-29:** CI no GitHub Actions (ativo, seção 6.8); hospedagem no Vercel com contêineres, como no projeto AgenteAnaliseDIMP. Pendentes: ambientes, viabilidade do MCP e do A2A no Vercel e provedores de banco, a formalizar em ADR na mudança `fundacao-solucao`. | Plano técnico da fundação. | ADR | Primeira implantação. |
| DP-22 | Processamento síncrono ou assíncrono das análises e tratamento de arquivos grandes. | Especificação e plano técnico da recepção. | Especificação e design | Etapa 2 do fluxo. |
| DP-23 | Política de rejeição ou reclassificação de itens com referência não verificável (P07). | Especificação do contrato de resultado. | Especificação | Validação semântica. |
| DP-24 | Conjunto representativo de exemplos para avaliar a recuperação e a análise: origem, anonimização e resultados esperados. | Especificação de avaliação, antes da definição de parâmetros. | Especificação | Aprovação de parâmetros (P11) e de modelos. |
| DP-25 | Armazenamento das execuções, dos achados e dos resultados na Api.Agente. | Plano técnico da fundação. | Design ou ADR | Persistência de resultados. |
| DP-26 | Consolidação do consumo da Api.RAG (embeddings de ingestão e de consulta) sem violar a propriedade dos dados (P15). | Plano técnico da mudança de consumo. | Design | Relatórios consolidados de custo. |

### 9.1 Decisões resolvidas

| ID | Decisão | Data | Registro |
| --- | --- | --- | --- |
| DP-19 | O responsável pelo projeto ratifica a constitution e aprova especificações, planos, ADRs e desvios. O assistente de IA escreve os PRs e o responsável os revisa. Responsável: Kaio Macedo. | 2026-10-29 | Cabeçalho e seção 6.6. |
| DP-20 | GitHub privado `kaiosmacedo/Agente-ECF`; `main` alterada só por PR; uma branch por mudança; Conventional Commits. | 2026-10-29 | Seção 6.8. |
| DP-21 | Revisão da constitution a cada nova versão do leiaute da ECF adotada e a cada emenda MAJOR. | 2026-10-29 | Seção 8.4. |
