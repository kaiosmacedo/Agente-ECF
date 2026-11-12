## 1. Governança

- [x] 1.1 Registrar a aprovação de Kaio Macedo à proposta, às specs e aos esclarecimentos, e a emenda v2.2.0 das seções 1.2 e 1.3 da constitution (P20).
- [x] 1.2 Escrever o plano técnico (`design.md`) e estas tarefas.

## 2. Domínio e aplicação

- [x] 2.1 Resolver a versão do manual pela pergunta, com a tabela de vigência testada contra `docs/fontes/manifesto-rag.json` (REQ-PERGUNTA-002; D3).
- [x] 2.2 Extrair o código literal para a busca, sem tomar ano por código (REQ-PERGUNTA-002; D4).
- [x] 2.3 Criar a porta `IRedatorDeRespostas`, o prompt `pergunta-v1` e a validação da saída, com citações, tamanho e frases proibidas (REQ-PERGUNTA-003, 004, 006; D5, D6).
- [x] 2.4 Criar a resposta de reserva e o texto fixo de assunto fora dos manuais (REQ-PERGUNTA-004, 005, 007; D6).
- [x] 2.5 Criar o `ServicoDePerguntas`, com validação, limite, busca, redação, status e resultado `resposta-pergunta` 1.0 (REQ-PERGUNTA-001, 005, 007, 009; D1, D4, D7).

## 3. Infraestrutura

- [x] 3.1 Adicionar `Microsoft.SemanticKernel` 1.82.0 e o `RedatorSemanticKernel`, com saída estruturada estrita, timeout e retentativas só para falha transitória; não subir com o LLM ligado e sem chave (REQ-PERGUNTA-003, 007; D5).
- [x] 3.2 Copiar da Api.RAG o registro de consumo e a tabela de preços, com a migração `0004_consumo_ia.sql` e o preço do `gpt-4.1-mini` conferido na página oficial (D10).
- [x] 3.3 Criar a migração `0005_perguntas.sql` e o repositório de perguntas, sem texto da pergunta nem da resposta e com a contagem diária (REQ-PERGUNTA-009; D9).
- [x] 3.4 Configurar `Perguntas` e `Llm`, desligados em produção (D5, D9).

## 4. Canais

- [x] 4.1 Encaminhar ao modo pergunta o pedido A2A que só traz a pergunta, e projetar o resultado no contrato SOFIA v1.0 (REQ-PERGUNTA-001, 008; REQ-RECEPCAO-006; D2, D7).
- [x] 4.2 Criar o `POST /perguntas` e publicá-lo no OpenAPI (REQ-PERGUNTA-008; REQ-DOCAPI-004; D2, D7).
- [x] 4.3 Cartão 1.4.0 com a função ligada, e o 1.3.0 de hoje com ela desligada (REQ-A2A-007; D8).

## 5. Testes

- [x] 5.1 Testes de unidade: versão, código, validação da saída, reserva, status e frases proibidas (REQ-PERGUNTA-002 a 007).
- [x] 5.2 Testes HTTP em memória do A2A e do REST com busca e redator falsos: pergunta da Sofia, limite de tamanho, função desligada, busca desligada, modelo indisponível, citação falsa, resposta longa, mesmo resultado nos dois canais e pergunta maliciosa (REQ-PERGUNTA-001 a 008).
- [x] 5.3 Testes com PostgreSQL em contêiner: registro sem texto, limite diário e retentativa com o mesmo `idSolicitacao` (REQ-PERGUNTA-009).
- [x] 5.4 Teste do redator com resposta HTTP simulada da OpenAI: tokens, schema e retentativa (D5, D10).
- [x] 5.5 Rodar a suíte inteira, o `dotnet format` e o `openspec validate --all --strict`.

## 6. Avaliação e entrega

- [x] 6.1 Criar o conjunto `docs/avaliacao/perguntas-respostas.json` e o script `.github/scripts/avaliar-respostas.sh` (REQ-PERGUNTA-010; D11).
- [x] 6.2 Rodar a avaliação local com a chave do RAG e registrar o resultado em `docs/avaliacao/respostas.md` (E11; D11).
- [x] 6.3 Atualizar `docs/contratos/sofia-a2a.md`, `docs/ci-cd.md`, `docs/roteiro.md` e o cofre Obsidian.
- [x] 6.4 **(feito por Kaio Macedo em 2026-11-12)** Cadastrar `OpenAI__ApiKey` em `agente-ecf-rag` e `agente-ecf-agente` (E11).
- [x] 6.5 Ligar a busca, o LLM e as perguntas em produção, atualizar as cópias do cartão e verificar pela Sofia (Migration Plan, passo 4). **Feito em 2026-11-12:** as variáveis de configuração estão no Vercel, e 10 de 10 perguntas do roteiro saíram como esperado. O A2A 0.3.0 com o pseudodocumento saiu `concluido`, e o cartão está na versão 1.4.0. A conversa da Sofia central chamou o agente, com "Concluído". Uma tentativa logo depois do deploy saiu sem especialista (`docs/contratos/sofia-a2a.md`). As cópias do cartão em `docs/contratos/` seguem com a configuração padrão (1.3.0), a usada nos testes de contrato.
- [x] 6.7 Corrigir as citações recusadas em produção (perguntas do Registro 0000). O prompt passou a `pergunta-v2`, o id da fonte vem com o prefixo `trecho:`, o escape é mínimo e a citação sem prefixo é aceita. A avaliação foi refeita, com a R01 (D5).
- [ ] 6.6 Executar `/opsx:verify` e arquivar a mudança.
