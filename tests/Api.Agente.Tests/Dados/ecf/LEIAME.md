# ECF sintéticas da recepção e da identificação

Arquivos **sintéticos**, sem dado de contribuinte real (SQ-10). O CNPJ `11111111000191` e o nome são os do exemplo de preenchimento dos próprios manuais. Os arquivos ficam em ISO 8859-1 e com CR LF, como pede a seção 2.2 do manual (leiaute 12, p. 32), por isso o `.gitattributes` não converte as quebras de linha.

**Não são ECF completas:** têm só o Registro 0000 e registros de abertura e encerramento, o bastante para a recepção (mudança 07) e a identificação do leiaute. A validação estrutural (mudança 08) traz uma ECF estruturalmente válida.

| Arquivo | Leiaute | `COD_VER` | `DT_INI` e `DT_FIN` | Requisito | Fonte |
| --- | --- | --- | --- | --- | --- |
| `ecf-leiaute-07.txt` | 7 | `0007` | 2020 | REQ-LEIAUTE-001 | Manual do leiaute 7, p. 55 |
| `ecf-leiaute-08.txt` | 8 | `0008` | 2021 | REQ-LEIAUTE-001 | Manual do leiaute 8, p. 56 |
| `ecf-leiaute-09.txt` | 9 | `0009` | 2022 | REQ-LEIAUTE-001 | Manual do leiaute 9, p. 55 |
| `ecf-leiaute-10.txt` | 10 | `0010` | 2023 | REQ-LEIAUTE-001 | Manual do leiaute 10, p. 60 e 65 |
| `ecf-leiaute-11.txt` | 11 | `0011` | 2024 | REQ-LEIAUTE-001 | Manual do leiaute 11, p. 59 |
| `ecf-leiaute-12.txt` | 12 | `0012` | 2025 | REQ-LEIAUTE-001, REQ-RECEPCAO-005 a 008 | Manual do leiaute 12, p. 32 e 63 |

As variações dos testes (sem `COD_VER`, `COD_VER` fora dos leiautes, período de outro ano, situação especial, marcador de corte, linha longa) são feitas em código a partir do `ecf-leiaute-12.txt`, em `Infra/EcfSintetica.cs`.
