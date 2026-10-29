---
name: github-versionamento
description: Padronizar o versionamento Git e GitHub do Agente-ECF, conduzido com OpenSpec e SDD. Usar ao solicitar commit, push, criação de branch, abertura de pull request, publicação de uma mudança OpenSpec ou sincronização de código e artefatos de especificação.
---

# GitHub: versionamento, commit e push no Agente-ECF

Adaptada da skill de mesmo nome usada com o GitHub Spec Kit. As regras vêm da seção 6.8 (versionamento), 6.6 (revisão de PR) e 7 (Definition of Done) de `openspec/constitution.md`; em conflito, a constitution prevalece.

## Contrato de entrada e saída

- **Entradas:** repositório local, mudança OpenSpec (`openspec/changes/<id>/`) ou motivo da alteração, branch de destino e autorização para publicar.
- **Saídas:** alterações verificadas, commit rastreável, push confirmado, hash do commit, branch e URL do PR quando houver. Bloqueios são informados como bloqueios, sem simular sucesso.

## Convenções do repositório

| Item | Regra |
| --- | --- |
| Remoto | `origin` → `https://github.com/kaiosmacedo/Agente-ECF` (privado). |
| Linha principal | `main`, alterada somente por PR. Exceção única: o commit inicial de criação do repositório. |
| Branch de mudança OpenSpec | `feat/<id-da-mudança>`, igual ao nome da pasta em `openspec/changes/`. Exemplo: `feat/fundacao-solucao`. |
| Correção sem nova mudança (P20) | `fix/<descrição-curta>`, referenciando o requisito afetado. |
| Documentação e governança | `docs/<descrição-curta>`. Emendas à constitution: `docs/constitution-v<versão>`. |
| Commits | Conventional Commits com escopo: `agente`, `rag`, `spec`, `constitution`, `adr`, `infra`, `docs`, `tests`. Exemplos: `feat(rag): ingerir manual com metadados de versão`, `docs(spec): registrar esclarecimentos da recepcao-arquivos`. |

## Regras inegociáveis

1. Antes de qualquer alteração, inspecionar `git status --short --branch`, `git remote -v`, `git branch --show-current`, `git diff --stat` e `git diff`.
2. Não forçar push, não reescrever histórico compartilhado e não executar reset destrutivo sem aprovação explícita.
3. Não adicionar segredos, `.env`, chaves, credenciais, `bin/`, `obj/`, arquivos ECF reais de clientes ou dados pessoais (SQ-03, SQ-10). Conferir `git diff --cached` e `.gitignore`; interromper se houver risco.
4. Preservar mudanças locais de outras tarefas. Usar `git add` com caminhos explícitos, nunca `git add .`.
5. Relacionar o trabalho à mudança OpenSpec: `proposal.md`, `specs/`, `design.md`, `tasks.md`, contratos e testes existentes. Não inventar arquivos ausentes.
6. Antes do commit de código, executar as validações do projeto (`dotnet build`, `dotnet test`, analisadores) e `openspec validate --strict`. Registrar falhas; nunca afirmar aprovação sem executar.
7. Marcar tarefas como concluídas em `tasks.md` somente com evidência real.
8. Se faltar acesso ou autenticação, entregar ao usuário os comandos exatos, sem simular o resultado.

## Fluxo operacional

### 1. Identificação

```powershell
git status --short --branch
git remote -v
git branch --show-current
git fetch origin
git log -5 --oneline
git diff --check
git diff --stat
```

Confirmar que `origin` aponta para `kaiosmacedo/Agente-ECF` e que a branch local não diverge da remota.

### 2. Branch da mudança

```powershell
git switch main
git pull --ff-only origin main
git switch -c feat/<id-da-mudança>
```

Uma branch por mudança OpenSpec. Não acumular mudanças diferentes na mesma branch.

### 3. Validação

```powershell
openspec validate --strict
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

Se ainda não houver solução .NET, executar apenas o que existe e informar o que não foi executado.

### 4. Seleção de arquivos e commit

```powershell
git add openspec/changes/<id-da-mudança>/ src/<projeto>/ tests/<projeto>/
git diff --cached --check
git diff --cached --stat
git diff --cached
git commit -m "feat(agente): <descrição>"
```

Os caminhos são exemplos; adicionar somente os que existem e pertencem à mudança.

### 5. Publicação e PR

```powershell
git fetch origin
git status -sb
git push -u origin HEAD
gh auth status
gh pr create --base main --title "<tipo>(<escopo>): <descrição>" --body-file <arquivo-com-o-corpo>
```

O corpo do PR MUST conter (constitution, 6.6):

```markdown
## Mudança OpenSpec
`openspec/changes/<id-da-mudança>/`

## Requisitos atendidos
- REQ-<CAPACIDADE>-<NNN>

## Princípios impactados
- Pxx: como a mudança atende

## Evidências de teste
- Comandos executados e resultado

## Desvios
- Nenhum | DEV-NNN (docs/governanca/desvios.md)

## Definition of Done
- Itens não aplicáveis, com justificativa
```

O PR é aprovado por pessoa diferente do autor. Se houver divergência com `main`, interromper e avaliar merge ou rebase com o usuário.

### 6. Após o merge

Arquivar a mudança com `/opsx:archive` em uma branch `docs/arquivar-<id-da-mudança>` (ou na própria branch, antes do merge, quando o aceite já tiver ocorrido) e atualizar a nota `Projetos/Agente-ECF` no cofre Obsidian.

### 7. Evidências

Informar: resumo das alterações, testes efetivamente executados, branch local e remota, hash (`git rev-parse --short HEAD`), estado final (`git status -sb`) e link do PR. Se o push falhar, distinguir commit local de publicação remota.
