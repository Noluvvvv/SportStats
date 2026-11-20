# Pendencias de back end: Database

<!-- usuarios-2026-11b -->
## Usuario, preferencias e afins (instrucoes)
- Tudo fica em um unico arquivo: Database/script.sql (parte 2). Nao ha segundo script.
- ATENCAO: o script.sql recria o banco (DROP DATABASE) e apaga os usuarios. Usar so em desenvolvimento.
- Criar usuario (no repositorio, em uma transacao): 1) validar e-mail unico; 2) gerar o hash da senha no C# (PasswordHasher ou BCrypt), nunca gravar senha pura; 3) INSERT em Usuario; 4) INSERT em PreferenciaUsuario com LAST_INSERT_ID() (padroes: grupo esportes, fuso America/Sao_Paulo, idioma pt-BR, notificacoes ativas); 5) COMMIT.
- Preferencias: UPDATE em PreferenciaUsuario por UsuarioId. GrupoPadrao so aceita esportes ou esports.
- Favoritos: INSERT em Favorito com Tipo (time, atleta, competicao, partida), Fonte (ESPN, PandaScore ou SportStats para id interno), IdExterno e Nome. A chave primaria evita duplicar.
- Excluir usuario apaga preferencias, favoritos, notificacoes e historico (ON DELETE CASCADE).
- Consultas sempre com parametros (Dapper). Nunca concatenar strings.
- Os hashes dos usuarios de teste sao falsos: criar usuarios reais pelo cadastro.
- O projeto usa MySQL. Se algum dia usar SQLite: INTEGER PRIMARY KEY AUTOINCREMENT, TEXT no lugar de VARCHAR, PRAGMA foreign_keys = ON e last_insert_rowid() no lugar de LAST_INSERT_ID().