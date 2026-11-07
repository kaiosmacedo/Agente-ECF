-- Base de conhecimento (mudança 05, REQ-INGEST-001 a 005; GD-07).
-- Sem índice vetorial: a busca sempre filtra por tipo e versão (P11), e cada manual tem cerca de 2 mil trechos.
-- A busca exata é rápida nesse tamanho e não perde resultados, como um índice aproximado perde com filtro.
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE documentos (
    id                    uuid PRIMARY KEY,
    emissor               text NOT NULL,
    titulo                text NOT NULL,
    tipo                  text NOT NULL,
    versao                text NOT NULL,
    ano_calendario        integer,
    versao_da_publicacao  text,
    vigencia              text,
    origem                text,
    hash                  text NOT NULL,
    tamanho_bytes         bigint NOT NULL,
    paginas               integer,
    estado                text NOT NULL CHECK (estado IN ('processando', 'vigente', 'substituido', 'desativado', 'falhou')),
    ingerido_em           timestamptz NOT NULL,
    modelo                text,
    dimensao              integer,
    CONSTRAINT documentos_hash_unico UNIQUE (hash)
);

-- Só uma versão vigente de cada documento.
CREATE UNIQUE INDEX documentos_um_vigente ON documentos (tipo, versao) WHERE estado = 'vigente';

CREATE TABLE chunks (
    id            uuid PRIMARY KEY,
    documento_id  uuid NOT NULL REFERENCES documentos (id) ON DELETE CASCADE,
    ordem         integer NOT NULL,
    pagina        integer NOT NULL,
    secao         text,
    texto         text NOT NULL,
    hash          text NOT NULL,
    modelo        text NOT NULL,
    dimensao      integer NOT NULL,
    embedding     vector(1536) NOT NULL,
    CONSTRAINT chunks_ordem_unica UNIQUE (documento_id, ordem)
);
