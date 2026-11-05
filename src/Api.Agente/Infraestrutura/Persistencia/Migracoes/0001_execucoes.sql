-- Execuções da análise (mudança 03, REQ-EXEC-001 e REQ-EXEC-006).
-- Sem texto de documentos, nome do usuário ou pergunta (GD-06, SQ-04): só hash, pseudônimo e o envelope.
CREATE TABLE execucoes (
    id                 uuid PRIMARY KEY,
    id_execucao_sofia  uuid NOT NULL,
    id_solicitacao     uuid NOT NULL,
    organizacao        text NOT NULL,
    usuario_pseudonimo text NOT NULL,
    status             text NOT NULL,
    criada_em          timestamptz NOT NULL,
    concluida_em       timestamptz,
    conteudo           jsonb NOT NULL,
    CONSTRAINT execucoes_id_solicitacao_unico UNIQUE (id_solicitacao)
);

CREATE INDEX execucoes_id_execucao_sofia ON execucoes (id_execucao_sofia);
