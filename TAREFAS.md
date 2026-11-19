# Tarefas do grupo

Procurar no codigo: `Get-ChildItem -Recurse -Include *.cs,*.cshtml,*.js,*.sql,*.md | Select-String "TODO \[BACK-"`

| ID | Tarefa | Arquivos | Manual | Responsavel |
|---|---|---|---|---|
| BACK-01 | CRUD de Time e Campeonato | Controllers/TimesController.cs | R5, R6, R7 | |
| BACK-02 | CRUD de Partida, placar, detalhes | Controllers/PartidasController.cs | R1, R3, R4 | |
| BACK-03 | Atleta e Transferencia | Controllers/AtletasController.cs | R1, R2, R4 | |
| BACK-04 | Estatisticas e ranking | Models/Estatistica.cs | R3, R5 | |
| BACK-05 | Tempo real (SignalR) | Hubs/PlacarHub.cs | extra | |
| BACK-06 | Colunas novas no banco | Database/script.sql | R6 | |
| BACK-07 | README (nomes, RA, diagrama) | README.md | entrega | todos |

Regras do grupo:
- Uma branch por tarefa (`feature/back-01-...`) e Pull Request revisado por outro integrante.
- Cada um commita com o proprio usuario do git (o professor confere o historico).
- Cada integrante precisa saber explicar o codigo da sua tarefa na arguicao.
- Ao terminar uma tarefa, apagar o TODO correspondente e marcar aqui.
## Atualizacao (integracao, usuarios e pendencias)

| ID | Tarefa | Arquivos | Responsavel |
|---|---|---|---|
| BACK-05 | Provedores ESPN/PandaScore, cache, SignalR e /Esports: feito. Falta confirmar ligas e pausar polling sem clientes | Integracao/, Hubs/ | |
| BACK-08 | Usuario, login e preferencias (hash de senha, transacao Usuario + Preferencia) | Database/script.sql, Controllers/ | |
| BACK-09 | Favoritos, historico e notificacoes (por Tipo + Fonte + IdExterno) | Repositories/, Services/, Views/ | |
| BACK-10 | (futuro) Iframe do Flashscore em /Esports | Views/Esports | |
| BACK-11 | (futuro) Rever o uso da PandaScore sem token / 401 | Integracao/PandaScore | |