-- Para que serviu cada chamada (mudança 06, D5): o limite diário da busca conta só as consultas.
-- As linhas anteriores ficam sem finalidade: eram da ingestão e da avaliação, sem como separar.
ALTER TABLE consumo_ia
    ADD COLUMN finalidade text CHECK (finalidade IN ('ingestao', 'consulta', 'avaliacao'));

CREATE INDEX consumo_ia_finalidade_criada_em ON consumo_ia (finalidade, criada_em);
