-- Consumo de provedores de IA na Api.Agente (mudança 17, D10; mudança 10, D8), copiado do serviço de conhecimento (RA-10, P15).
-- Valor desconhecido fica nulo, nunca zero. Sem texto, prompt ou credencial.
CREATE TABLE consumo_ia (
    id                     uuid PRIMARY KEY,
    id_operacao            uuid,
    correlacao             text,
    provedor               text NOT NULL,
    modelo                 text NOT NULL,
    tipo                   text NOT NULL CHECK (tipo IN ('geracao', 'retentativa')),
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
    finalidade             text NOT NULL CHECK (finalidade IN ('pergunta', 'analise')),
    CONSTRAINT consumo_ia_custo_com_tabela CHECK (custo_usd IS NULL OR tabela_precos_versao IS NOT NULL)
);

CREATE INDEX consumo_ia_criada_em ON consumo_ia (criada_em);
CREATE INDEX consumo_ia_id_operacao ON consumo_ia (id_operacao);
