# Tasks: Recepção do arquivo e identificação do leiaute

> Plano aprovado por Kaio Macedo em 2026-11-07 (P20). Skills: `sofia-a2a-contrato` e `sofia-a2a-implementacao` (grupos 4 e 6), `testes-agentes-ia` (todos), `github-versionamento` (grupo 7).

## 1. Dados de teste

- [ ] 1.1 Criar as ECF sintéticas mínimas dos leiautes 7 a 12 e as variações (sem `COD_VER`, `COD_VER` fora dos leiautes, marcador de corte, linha longa, NF-e sintética), com `LEIAME.md` que liga cada arquivo ao requisito e à página do manual (D11; SQ-10); verificar que nenhum dado vem de arquivo real.

## 2. Etapa 2: recepção

- [ ] 2.1 Normalizar o pedido dos dois canais no serviço de análise (D1); verificar que o adaptador A2A não tem regra de análise (REQ-A2A-002, REQ-RECEPCAO-008).
- [ ] 2.2 Reconhecer a ECF pela primeira linha (D2); verificar os cenários de ECF e de documento que não é ECF (REQ-RECEPCAO-005).
- [ ] 2.3 Aplicar a regra de uma ECF por pedido e as observações dos demais documentos (D2); verificar os cenários de ECF com NF-e, nenhuma ECF e duas ECF (REQ-RECEPCAO-006).
- [ ] 2.4 Trocar `A2A:TamanhoMaximoDosDocumentosEmBytes` pelas opções de `Recepcao` (D4), validadas na inicialização; verificar o limite de tamanho e os limites de linha (REQ-RECEPCAO-002, REQ-RECEPCAO-004, REQ-SOFIA-004).
- [ ] 2.5 Detectar o marcador de corte (D6); verificar o cenário de ECF cortada (REQ-RECEPCAO-007).

## 3. Etapa 3: identificação

- [ ] 3.1 Ler o Registro 0000 e mapear `COD_VER` para o leiaute pela tabela de D5; verificar um arquivo de cada leiaute de 7 a 12 (REQ-LEIAUTE-001).
- [ ] 3.2 Registrar a versão fora dos leiautes 7 a 12 como não suportada (D7); verificar que as etapas 4 e 5 ficam não executadas com a limitação (REQ-LEIAUTE-002).
- [ ] 3.3 Acrescentar a causa da falha à etapa e ajustar o status (D7), com a migração `0005_causa_da_falha`; verificar `COD_VER` ausente como `parcial` e exceção no identificador como `falha-tecnica` (REQ-LEIAUTE-003, REQ-EXEC-004).
- [ ] 3.4 Conferir o leiaute com o ano de `DT_FIN` e o `SIT_ESPECIAL` (D5); verificar os cenários de situação especial no ano seguinte e de versão de outro ano (REQ-LEIAUTE-004).

## 4. Canais

- [ ] 4.1 Criar `POST /analises` (D3), com a migração `0004_canal`; verificar o envio válido, o arquivo acima do limite, a leitura em ISO 8859-1 e o nome `../../etc/passwd` (REQ-RECEPCAO-001, REQ-RECEPCAO-002, REQ-RECEPCAO-004).
- [ ] 4.2 Verificar com Testcontainers que a mesma ECF sintética pelos dois canais produz o mesmo status, veredito e achados (REQ-RECEPCAO-008).
- [ ] 4.3 Atualizar o cartão para a versão 1.1.0 (D8) e a cópia em `docs/contratos/agent-card.json`; verificar os cenários de REQ-A2A-007 e REQ-A2A-008 com teste de contrato.
- [ ] 4.4 Ligar a etapa 5 ao fluxo depois da identificação, com `EtapaDeConsultaAoManual` da mudança 06 (D7 dela), e verificar em contêineres a chamada da Api.Agente à Api.RAG, com a busca desligada: etapa 5 não executada e limitação "consulta ao manual desativada" (REQ-CONSULTA-007).

## 5. Segurança e qualidade

- [ ] 5.1 Verificar com o Registro 0000 sintético que uma instrução no campo `NOME` não muda etapas, status nem veredito (P10; D11).
- [ ] 5.2 Verificar que os logs não trazem o texto, o nome do arquivo nem o nome do usuário (REQ-SOFIA-005; SQ-04).

## 6. Documentação

- [ ] 6.1 Escrever o guia para a Sofia em `docs/contratos/sofia-a2a.md` (D9) e atualizar `docs/contratos/envelope-resultado.md` com os códigos de D2.
- [ ] 6.2 Registrar em `docs/roteiro.md` a medição de ECF reais antes de dados reais (D4) e atualizar a seção de autenticação adiada para o `POST /analises`.

## 7. Entrega e verificação

- [ ] 7.1 Depois do merge, verificar em produção, só com dados sintéticos: cartão 1.1.0; `SendMessage` com ECF do leiaute 12, com NF-e, com duas ECF e com o marcador de corte; e `POST /analises`.
- [ ] 7.2 Medir em produção o tempo de um `SendMessage` com ECF sintética perto de 4 MiB e registrar em `docs/contratos/sofia-a2a.md`, para a mudança 10 (E03).
- [ ] 7.3 Atualizar o cofre Obsidian; mover DP-05 e DP-22 para as decisões resolvidas; executar `/opsx:verify` e conferir a Definition of Done (seção 7 da constitution).
