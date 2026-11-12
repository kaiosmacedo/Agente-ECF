-- Perguntas sobre a ECF (mudança 17, D9; REQ-PERGUNTA-009).
-- Sem o texto da pergunta nem o da resposta (DP-25): só hash, tamanho, versão, trechos e desfecho.
CREATE TABLE perguntas (
    id                  uuid PRIMARY KEY,
    id_solicitacao      uuid,
    canal               text NOT NULL CHECK (canal IN ('a2a', 'rest')),
    status              text NOT NULL CHECK (status IN ('concluido', 'parcial', 'erro')),
    codigo_erro         text,
    pergunta_sha256     text NOT NULL,
    pergunta_caracteres integer NOT NULL,
    versao_manual       text,
    trechos             jsonb NOT NULL,
    citados             jsonb NOT NULL,
    redacao             text NOT NULL CHECK (redacao IN ('aceita', 'reserva', 'nao-chamada')),
    modelo              text,
    criada_em           timestamptz NOT NULL,
    concluida_em        timestamptz NOT NULL,
    CONSTRAINT perguntas_id_solicitacao_unico UNIQUE (id_solicitacao)
);

CREATE INDEX perguntas_criada_em ON perguntas (criada_em);
