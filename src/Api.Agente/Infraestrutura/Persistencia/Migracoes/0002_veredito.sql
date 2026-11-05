-- Veredito da execução (mudança 15, REQ-RESULT-010). As execuções anteriores não têm achados, então o veredito sai do status.
ALTER TABLE execucoes ADD COLUMN veredito text;

UPDATE execucoes
SET veredito = CASE WHEN status IN ('entrada-rejeitada', 'falha-tecnica') THEN 'nao-avaliado' ELSE 'parcial' END;

UPDATE execucoes
SET conteudo = conteudo || jsonb_build_object(
    'veredito', veredito,
    'resumo', jsonb_build_object('impeditivas', 0, 'graves', 0, 'observacoes', 0));

ALTER TABLE execucoes ALTER COLUMN veredito SET NOT NULL;
