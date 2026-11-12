## 1. Correção e regressão

Skill: testes-agentes-ia

- [x] 1.1 Validar este change em modo estrito antes de alterar código (P20; REQ-DOCAPI-005). OpenSpec 1.15.0, válido em 12/11/2026.
- [x] 1.2 Reproduzir a falta do objeto/propriedade no documento atual com um teste que falhe; corrigir os metadados do upload (REQ-DOCAPI-005, cenário Documento de upload). Três casos novos falharam com o schema antigo; depois da correção, 9/9 testes da documentação passaram.
- [x] 1.3 Construir multipart pelo nome publicado, enviar a ECF sintética de erro e conferir HTTP 201 e achado; confirmar HTTP 400 sem anexo (REQ-DOCAPI-005, cenários Envio e Arquivo não anexado). Cenários executados em `DocumentacaoApiTests`.
- [x] 1.4 Executar formatação, build, testes .NET e validação OpenSpec estrita; conferir compatibilidade dos demais endpoints (REQ-DOCAPI-001 a 005, REQ-RECEPCAO-001). Formatação sem diferenças, build Release sem aviso/erro, 542 testes aprovados e 22 artefatos OpenSpec válidos em 12/11/2026.

## 2. Publicação e fechamento

Skill: github-versionamento

- [x] 2.1 Atualizar guia e roteiro, publicar PR, revisar pela delegação vigente e mesclar somente com os checks obrigatórios verdes (REQ-DOCAPI-005; P20). PR #47 revisado pela delegação, CI #121 aprovado; merge `1bca6fc` em 12/11/2026. CI de push também aprovado.
- [x] 2.2 Acompanhar implantação e conferir em produção o schema e o envio dos arquivos sintéticos com e sem erro estrutural, registrando as respostas (REQ-DOCAPI-005; REQ-RECEPCAO-001). CD aprovado; Swagger com curl `-F arquivo=@...`, HTTP 201 e recusado; controle HTTP 201, parcial e zero achados. IDs e versões no design.
- [x] 2.3 Atualizar Obsidian, conferir a Definition of Done aplicável e validar o encerramento técnico para a consolidação da spec vigente (REQ-DOCAPI-005; DoD-01 a 20). Notas 018, projeto e apresentação atualizadas com causa, testes, PR e evidências; DoD conferida no design.

## Workflow follow-up

Após todas as tarefas verificadas, executar `openspec archive corrigir-upload-ecf-swagger --yes` e publicar o fechamento por PR com checks verdes (DoD-21). A publicação do arquivamento será conferida antes da entrega final.
