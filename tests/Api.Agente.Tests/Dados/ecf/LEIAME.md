# ECF sintéticas da recepção e da identificação

Arquivos **sintéticos**, sem dado de contribuinte real (SQ-10). O CNPJ `11111111000191` e o nome são os do exemplo de preenchimento dos próprios manuais. Os arquivos ficam em ISO 8859-1 e com CR LF, como pede a seção 2.2 do manual (leiaute 12, p. 32), por isso o `.gitattributes` não converte as quebras de linha.

**Os `ecf-leiaute-NN.txt` não são ECF completas:** têm só o Registro 0000 e registros de abertura e encerramento, o bastante para a recepção (mudança 07) e a identificação do leiaute; pela validação estrutural (mudança 08), eles têm erros de estrutura.

**`ecf-leiaute-12-estrutural.txt`:** ECF do leiaute 12 estruturalmente válida pelo catálogo, com os 13 registros obrigatórios na importação e na transmissão, gerada por `.github/scripts/catalogo/gerar-ecf-sintetica.py` (REQ-ESTRUT-001 a 015). O 0010, o 0020 e o 0030 seguem os exemplos de preenchimento do manual (p. 79, 94 e 102), com os ajustes que o programa da ECF 12.2.8 exigiu, e ele a importa. As regras de conteúdo (mudança 09) não foram consideradas: na validação, o programa aponta erros de negócio nela.

**`gabarito/`:** a ECF válida e uma variante por erro de estrutura plantado, para o gabarito no programa da ECF (mudança 08, D10), com a regra esperada de cada arquivo e o que o programa apontou em `gabarito/erros-esperados.md`. Gerados por `.github/scripts/catalogo/gerar-gabarito.py`, que preserva a coluna do programa.

| Arquivo | Leiaute | `COD_VER` | `DT_INI` e `DT_FIN` | Requisito | Fonte |
| --- | --- | --- | --- | --- | --- |
| `ecf-leiaute-07.txt` | 7 | `0007` | 2020 | REQ-LEIAUTE-001 | Manual do leiaute 7, p. 55 |
| `ecf-leiaute-08.txt` | 8 | `0008` | 2021 | REQ-LEIAUTE-001 | Manual do leiaute 8, p. 56 |
| `ecf-leiaute-09.txt` | 9 | `0009` | 2022 | REQ-LEIAUTE-001 | Manual do leiaute 9, p. 55 |
| `ecf-leiaute-10.txt` | 10 | `0010` | 2023 | REQ-LEIAUTE-001 | Manual do leiaute 10, p. 60 e 65 |
| `ecf-leiaute-11.txt` | 11 | `0011` | 2024 | REQ-LEIAUTE-001 | Manual do leiaute 11, p. 59 |
| `ecf-leiaute-12.txt` | 12 | `0012` | 2025 | REQ-LEIAUTE-001, REQ-RECEPCAO-005 a 008 | Manual do leiaute 12, p. 32 e 63 |

As variações dos testes (sem `COD_VER`, `COD_VER` fora dos leiautes, período de outro ano, situação especial, marcador de corte, linha longa) são feitas em código a partir do `ecf-leiaute-12.txt`, em `Infra/EcfSintetica.cs`.
