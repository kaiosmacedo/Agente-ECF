# 0004 — Modelo de embeddings

| Campo | Valor |
| --- | --- |
| Estado | Aceito |
| Data | 2026-11-05 |
| Responsável pela decisão | Kaio Macedo |
| Relacionado a | DP-02, P11, mudança `base-conhecimento-ingestao` |

## Contexto

A Api.RAG precisa de embeddings para indexar os manuais da ECF (leiautes 7 a 12), que somam cerca de 3 milhões de tokens. O projeto `SofiaInfis` já usa o `text-embedding-3-small`, com 1536 dimensões, no mesmo tipo de busca.

## Decisão

Usar o `text-embedding-3-small`, da OpenAI, com 1536 dimensões e distância de cosseno. Modelo e dimensão ficam em cada trecho.

A busca é exata, filtrada por tipo de documento e versão (P11), sem índice vetorial aproximado. Cada manual tem cerca de 2 mil trechos, tamanho em que a busca exata é rápida. Um índice aproximado pode perder resultados quando há filtro, porque filtra depois de escolher os candidatos.

## Alternativas

- **`text-embedding-3-large`:** mais caro e com vetores maiores. Fica como opção se a avaliação (GD-08) mostrar recuperação insuficiente.
- **Índice HNSW:** entra se a base crescer a ponto de a busca exata ficar lenta, com avaliação da perda de resultados.

## Consequências

- Custo estimado de cerca de US$ 0,06 por carga completa, a US$ 0,02 por milhão de tokens em 2026-11-05 (`docs/ci-cd.md`).
- Trocar de modelo exige reprocessar os vetores (P11).
- A busca da mudança 06 precisa da chave da OpenAI também em produção, decisão que fica para aquele plano.
