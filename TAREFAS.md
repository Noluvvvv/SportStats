# Tarefas do grupo

## Como procurar TODOs no código

**Windows (PowerShell):**
```powershell
Get-ChildItem -Recurse -Include *.cs,*.cshtml,*.js,*.sql,*.md | Select-String "TODO \[BACK-"
```

**Linux:**
```bash
grep -RnE "TODO \[BACK-" --include="*.cs" --include="*.cshtml" --include="*.js" --include="*.sql" --include="*.md" .
```

## Tarefas principais

| ID | Tarefa | Arquivos | Manual | Responsável |
|---|---|---|---|---|
| BACK-01 | CRUD de Time e Campeonato | Controllers/TimesController.cs | R5, R6, R7 | Ângelo |
| BACK-02 | CRUD de Partida, placar e detalhes | Controllers/PartidasController.cs | R1, R3, R4 | Ângelo |
| BACK-03 | Atleta e Transferência | Controllers/AtletasController.cs | R1, R2, R4 | Maxwell |
| BACK-04 | Estatísticas e ranking | Models/Estatistica.cs | R3, R5 | |
| BACK-05 | Tempo real (SignalR) | Hubs/PlacarHub.cs | extra | |
| BACK-06 | Colunas novas no banco | Database/script.sql | R6 | |
| BACK-07 | README (nomes, RA e diagrama) | README.md | entrega | todos |

## Regras do grupo

- Uma branch por tarefa (`feature/back-01-...`) e Pull Request revisado por outro integrante.
- Cada integrante deve realizar os commits com seu próprio usuário do Git, pois o professor confere o histórico.
- Cada integrante precisa saber explicar o código da própria tarefa durante a arguição.
- Ao terminar uma tarefa, apagar o TODO correspondente e atualizar seu andamento neste documento.

## Andamento das tarefas

| ID | Situação |
|---|---|
| BACK-03 | Implementação realizada e testada. Dois commits enviados para a branch `feature/back-03-atleta-transferencia`. Pull Request pendente de criação e revisão. R4 parcialmente atendido: falta ajustar o feedback da exclusão de atleta com histórico. |
| BACK-06 | Implementação concluída na branch `feature/back-06-database`. Build e validação em MySQL 8.4.13 descartável aprovados para o catálogo, as restrições, as chaves estrangeiras, a persistência Dapper, o retrospecto, a concorrência e os formulários MVC. Bases existentes exigem uma migração não destrutiva antes da atualização. |

## Atualização (integração, usuários e pendências)

| ID | Tarefa | Arquivos | Responsável |
|---|---|---|---|
| BACK-05 | Provedores ESPN/PandaScore, cache, SignalR e /Esports: feito. Falta confirmar ligas e pausar polling sem clientes. | Integracao/, Hubs/ | |
| BACK-08 | Usuário, login e preferências (hash de senha, transação Usuario + Preferencia). | Database/script.sql, Controllers/ | |
| BACK-09 | Favoritos, histórico e notificações (por Tipo + Fonte + IdExterno). | Repositories/, Services/, Views/ | |
| BACK-10 | (Futuro) Iframe do Flashscore em /Esports. | Views/Esports | |
| BACK-11 | (Futuro) Rever o uso da PandaScore sem token / 401. | Integracao/PandaScore | |