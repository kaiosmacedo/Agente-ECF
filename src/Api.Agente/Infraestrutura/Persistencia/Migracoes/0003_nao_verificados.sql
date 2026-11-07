-- Lista do que o arquivo não prova (mudança 15, REQ-RESULT-013). As execuções anteriores não a tinham.
UPDATE execucoes SET conteudo = conteudo || jsonb_build_object('naoVerificados', '[]'::jsonb) WHERE NOT conteudo ? 'naoVerificados';
