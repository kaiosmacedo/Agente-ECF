# Autenticação e autorização

Como a Api.Agente e a Api.RAG autenticam chamadores, aplicam perfis e isolam organizações, e como configurar o Microsoft Entra ID e o Vercel. Decisões em [ADR 0003](adr/0003-autenticacao-e-autorizacao.md); requisitos REQ-AUTH-001 a REQ-AUTH-008 na mudança `autenticacao-autorizacao`.

## Visão geral

- Todo endpoint exige um token JWT v2 do Entra ID (política de fallback). Somente `/health/live` e `/health/ready` são anônimos.
- O serviço valida emissor, audiência, assinatura e validade a partir das opções `Autenticacao`. Token ausente ou inválido: HTTP 401.
- A identidade vem só do token: claim `oid` (identificador), `idtyp` (`app` indica aplicação; ausente, pessoa) e `roles` (papéis de aplicação, convertidos em perfis). Nada do corpo, da URL ou de outros cabeçalhos altera a identidade.
- Papel desconhecido não concede acesso. Identidade sem o perfil exigido pelo endpoint: HTTP 403, sem executar a operação.
- A organização da requisição vem do cabeçalho `X-Organizacao-Id` e precisa constar das autorizações da identidade, configuradas na Api.Agente.

## Papéis e perfis

Os nomes abaixo são os valores (`Value`) dos papéis de aplicação no Entra e coincidem com as constantes `PapeisDoAgente` e `PapeisDoRag` do código.

### Api.Agente

| Papel (`roles`) | Perfil | Política | Tipo de membro | Quem recebe |
| --- | --- | --- | --- | --- |
| `Agente.Administrador` | administrador-agente | `AdministradorAgente` | Usuários/Grupos | Kaio Macedo |
| `Agente.OperadorOrquestrador` | operador-orquestrador | `OperadorOrquestrador` | Usuários/Grupos | Equipe que cuida da Sofia |
| `Agente.ClienteAgente` | cliente-agente | `ClienteAgente` | Aplicativos | Aplicações da Sofia e, no futuro, da Maia |
| `Agente.UsuarioAnalise` | usuario-analise | `UsuarioAnalise` | Usuários/Grupos | Pessoas, no futuro |

A política `QualquerPerfil` aceita qualquer um dos quatro papéis; é a de `GET /identidade`.

### Api.RAG

| Papel (`roles`) | Política | Tipo de membro | Quem recebe |
| --- | --- | --- | --- |
| `Rag.Administrador` | `AdministradorRag` | Usuários/Grupos | Kaio Macedo (administração da base, mudança 05) |
| `Rag.ServicoAgente` | `ServicoAgente` (exige também `idtyp = app`) | Aplicativos | Aplicação da Api.Agente (MCP, mudança 06) |

## Configuração por serviço

Nenhum destes valores é segredo. Em produção, ficam nas variáveis de ambiente de cada projeto do Vercel; o separador de seção é `__`.

| Variável | Api.Agente | Api.RAG | Valor |
| --- | --- | --- | --- |
| `Autenticacao__Autoridade` | obrigatória | obrigatória | `https://login.microsoftonline.com/<ID do locatário>/v2.0` |
| `Autenticacao__Audiencia` | obrigatória | obrigatória | ID do aplicativo (client ID) do registro da própria API |
| `Autenticacao__EmissoresValidos__0` | opcional | opcional | Emissor aceito além do anunciado pela autoridade |
| `Autorizacoes__Identidades__<n>__Id` | sim | — | `oid` da identidade autorizada |
| `Autorizacoes__Identidades__<n>__Nome` | sim | — | Nome para leitura humana, por exemplo `Sofia INFIS` |
| `Autorizacoes__Identidades__<n>__Organizacoes__<m>` | sim | — | Identificador de organização autorizada |

- **Audiência:** em tokens v2 do Entra, a claim `aud` é sempre o ID do aplicativo (client ID) da API, e não o Application ID URI. Por isso `Autenticacao__Audiencia` recebe o GUID do registro. O Application ID URI (`api://<ID do aplicativo>`) continua sendo usado pelos clientes para pedir o token (escopo `api://<ID do aplicativo>/.default`).
- **`oid` de uma aplicação:** é o ID de objeto da aplicação empresarial (entidade de serviço) no locatário, em **Entra ID > Aplicativos empresariais**. Não é o ID do aplicativo nem o ID de objeto do registro de aplicativo. Para uma pessoa, é o ID de objeto do usuário.
- As opções são validadas na inicialização: sem `Autoridade` ou `Audiencia`, ou com autorização sem `Id` ou com organização em formato inválido, o serviço não sobe e o erro cita a opção, nunca o valor.

O mesmo formato em JSON, na Api.Agente:

```json
"Autorizacoes": {
  "Identidades": [
    { "Id": "<oid da aplicação da Sofia>", "Nome": "Sofia INFIS", "Organizacoes": ["org-a", "org-b"] }
  ]
}
```

### Execução local

Defina as opções antes de `dotnet run` (os mesmos valores de produção servem, porque não são segredos):

```bash
export Autenticacao__Autoridade="https://login.microsoftonline.com/<ID do locatário>/v2.0" Autenticacao__Audiencia="<ID do aplicativo da Api.Agente>"
```

```bash
dotnet run --project src/Api.Agente
```

Os testes automatizados não usam o Entra: cada projeto de testes assina tokens com uma chave criada na execução (`Infra/GeradorDeTokens.cs`).

## Registros no Entra ID

Passo a passo das tarefas 8.1 e 8.2, no [centro de administração do Microsoft Entra](https://entra.microsoft.com), com uma conta que seja pelo menos Administrador de Aplicativos de Nuvem.

### 1. Registro de cada API

Repita para `agente-ecf-agente` (Api.Agente) e `agente-ecf-rag` (Api.RAG):

1. **Entra ID > Registros de aplicativo > Novo registro.** Nome conforme acima; tipos de conta: somente este diretório organizacional (locatário único); sem URI de redirecionamento. Anote o **ID do aplicativo (cliente)** e o **ID do diretório (locatário)**.
2. **Expor uma API > Application ID URI > Adicionar:** aceite o sugerido, `api://<ID do aplicativo>` (formato recomendado pela Microsoft).
3. **Manifesto:** defina `"requestedAccessTokenVersion": 2` dentro do objeto `api` (no formato antigo do manifesto, o mesmo campo se chama `accessTokenAcceptedVersion`). Sem isso, o Entra emite tokens v1, com outro emissor.
4. **Funções de aplicativo > Criar função de aplicativo:** crie os papéis da tabela do serviço, com o `Value` exatamente igual ao da tabela e o tipo de membro indicado.
5. **Configuração de token > Adicionar declaração opcional > Acesso > `idtyp`.** Ela identifica tokens de aplicação (`app`), exigidos pela política `ServicoAgente`.
6. **Proprietários:** inclua Kaio Macedo. Sem proprietário, a API não aparece para quem pede permissão a ela.

### 2. Papéis de Kaio Macedo

Em **Entra ID > Aplicativos empresariais**, abra `agente-ecf-agente` > **Usuários e grupos > Adicionar usuário/grupo**, escolha Kaio Macedo e o papel `Agente.Administrador`. Faça o mesmo em `agente-ecf-rag` com `Rag.Administrador`.

### 3. Aplicação da Sofia

1. No registro de aplicativo da Sofia: **Permissões de API > Adicionar uma permissão > Minhas APIs** (ou APIs que minha organização usa) > `agente-ecf-agente` > **Permissões de aplicativo** > `Agente.ClienteAgente` > **Adicionar permissões**.
2. **Conceder consentimento do administrador** para o locatário. A coluna Status deve indicar consentimento concedido.
3. Em **Aplicativos empresariais**, abra a aplicação da Sofia e copie o **ID de objeto**. Esse é o `oid` que vai em `Autorizacoes__Identidades__0__Id`, com as organizações iniciais dela.

A Maia segue os mesmos passos quando for integrada. A aplicação da própria Api.Agente recebe `Rag.ServicoAgente` na mudança 06, junto com o segredo que o responsável cadastra no Vercel.

### 4. Token de pessoa para verificação (tarefa 8.4)

Para Kaio Macedo consultar `GET /identidade` com o próprio usuário, a API precisa de um escopo delegado e de um cliente público autorizado. Com a CLI do Azure:

1. Em `agente-ecf-agente` > **Expor uma API > Adicionar um escopo**: `Agente.Acesso`, consentimento de administradores e usuários.
2. **Adicionar um aplicativo cliente:** ID `04b07795-8ddb-461a-bbee-02f9e1bf7b46` (CLI do Azure), marcando o escopo `Agente.Acesso`.
3. Obter o token:

```bash
az login --tenant "<ID do locatário>"
```

```bash
TOKEN=$(az account get-access-token --scope "api://<ID do aplicativo da Api.Agente>/Agente.Acesso" --query accessToken -o tsv)
```

O token de usuário traz em `roles` os papéis atribuídos a Kaio Macedo; o escopo não altera a autorização, que usa apenas os papéis.

## Token por client credentials

A Sofia e a Maia obtêm o token como aplicação. O segredo (ou certificado) é da aplicação cliente, fica no cofre de segredos dela e nunca em código, log ou repositório:

```bash
curl -s -X POST "https://login.microsoftonline.com/$ID_LOCATARIO/oauth2/v2.0/token" -d grant_type=client_credentials -d client_id="$ID_CLIENTE" --data-urlencode client_secret="$SEGREDO_CLIENTE" -d scope="api://$ID_APLICATIVO_AGENTE/.default"
```

A resposta traz `access_token`, enviado em `Authorization: Bearer <token>`.

## Organização por requisição

Cabeçalho `X-Organizacao-Id`, com identificador opaco de 1 a 64 caracteres entre letras, dígitos, `.`, `_` e `-`.

| Situação | Endpoints com escopo de organização |
| --- | --- |
| Cabeçalho com formato inválido | 400 |
| Organização informada e autorizada | Aceita |
| Organização informada e não autorizada | 403 |
| Sem cabeçalho e uma única organização autorizada | Usa essa |
| Sem cabeçalho e várias autorizadas | 400, organização obrigatória |
| Identidade sem organizações autorizadas | 403 |

Recurso de outra organização responde 404, sem revelar que existe (REQ-AUTH-004; o primeiro recurso com organização chega na mudança 03).

## Consulta da identidade

`GET /identidade` (Api.Agente) aceita qualquer perfil reconhecido e serve para o cliente conferir a própria configuração. Não exige organização: devolve a resolvida quando o cabeçalho é válido e autorizado, e `null` quando ele falta e há várias autorizadas.

```bash
curl -s -H "Authorization: Bearer $TOKEN" -H "X-Organizacao-Id: org-a" https://agente-ecf-agente.vercel.app/identidade
```

Resposta para a Sofia autorizada em `org-a` e `org-b`, como no teste `IdentidadeTests` (em produção, `id` é o `oid`, um GUID):

```json
{
  "id": "sofia-infis",
  "tipo": "aplicacao",
  "perfis": ["cliente-agente"],
  "organizacoes": ["org-a", "org-b"],
  "organizacaoResolvida": "org-a"
}
```

A Maia usa a mesma chamada com o próprio token; a resposta lista as organizações autorizadas para ela.

| Status | Quando |
| --- | --- |
| 200 | Token válido com perfil reconhecido e organização válida ou ausente |
| 400 | `X-Organizacao-Id` com formato inválido |
| 401 | Token ausente, expirado ou inválido |
| 403 | Nenhum perfil reconhecido, ou organização não autorizada |

A resposta nunca contém o token.

## Registro de falhas

Falhas ficam no log em JSON, no escopo do `CorrelationId`, com motivo e rota, sem token, chave ou senha:

| EventId | Mensagem | Serviço |
| --- | --- | --- |
| 100 | Falha de autenticação: {Motivo} em {Rota} | Ambos |
| 101 | Acesso negado por perfil em {Rota} | Ambos |
| 102 | Organização recusada: {Situacao} em {Rota} | Api.Agente |

O motivo é apenas o tipo da falha (por exemplo, `CredencialAusente` ou o nome da exceção de validação), nunca a mensagem da biblioteca.

## Referências oficiais

- [Access token claims reference](https://learn.microsoft.com/en-us/entra/identity-platform/access-token-claims-reference): `aud`, `iss`, `oid` e `roles` em tokens v2.
- [Optional claims reference](https://learn.microsoft.com/en-us/entra/identity-platform/optional-claims-reference): declaração `idtyp`.
- [Add app roles and get them from a token](https://learn.microsoft.com/en-us/entra/identity-platform/howto-add-app-roles-in-apps): papéis, atribuição a aplicativos e usuários e consentimento do administrador.
- [App manifest (Microsoft Graph format)](https://learn.microsoft.com/en-us/entra/identity-platform/reference-microsoft-graph-app-manifest): `requestedAccessTokenVersion` e formatos de Application ID URI.
- [Client credentials flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-client-creds-grant-flow).
