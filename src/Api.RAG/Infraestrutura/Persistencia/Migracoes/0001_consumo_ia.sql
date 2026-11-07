-- Consumo de provedores de IA (mudança 04, REQ-CONSUMO-001 a 003; GD-10).
-- Valor desconhecido fica nulo, nunca zero. Sem texto, prompt ou credencial.
CREATE TABLE consumo_ia (
    id                     uuid PRIMARY KEY,
    id_operacao            uuid,
    correlacao             text,
    provedor               text NOT NULL,
    modelo                 text NOT NULL,
    tipo                   text NOT NULL CHECK (tipo IN ('geracao', 'embedding', 'retentativa', 'auxiliar')),
    status                 text NOT NULL CHECK (status IN ('sucesso', 'falha')),
    criada_em              timestamptz NOT NULL,
    duracao_ms             double precision NOT NULL,
    tentativa              integer NOT NULL,
    tokens_entrada         integer,
    tokens_entrada_origem  text NOT NULL,
    tokens_saida           integer,
    tokens_saida_origem    text NOT NULL,
    custo_usd              numeric(18, 10),
    custo_origem           text NOT NULL,
    tabela_precos_versao   text,
    CONSTRAINT consumo_ia_custo_com_tabela CHECK (custo_usd IS NULL OR tabela_precos_versao IS NOT NULL)
);

CREATE INDEX consumo_ia_criada_em ON consumo_ia (criada_em);
CREATE INDEX consumo_ia_id_operacao ON consumo_ia (id_operacao);
