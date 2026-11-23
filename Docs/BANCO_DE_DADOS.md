# Banco de dados do SportStats

## 1. Visão geral

O banco `sportstats` armazena o catálogo de modalidades e os registros esportivos locais usados pela aplicação MVC. O mesmo schema inclui contas, preferências, favoritos, notificações e histórico de visualização. O BACK-06 foi validado em MySQL 8.4.13. A aplicação executa SQL com Dapper sobre conexões do MySqlConnector, conforme as dependências em [SportStats.csproj](../SportStats.csproj).

As partidas recebidas de ESPN e PandaScore são convertidas em `PartidaExterna` e mantidas em memória por `CachePartidasExternas`, com chave `Fonte:IdExterno`. O serviço de tempo real atualiza o cache e publica placares via SignalR. Esse fluxo não insere partidas, times ou atletas dos provedores no MySQL. As referências textuais nas tabelas de atividade podem identificar um registro externo ou um registro local com `Fonte = 'SportStats'`. [CachePartidasExternas.cs](../Integracao/CachePartidasExternas.cs) · [ServicoTempoReal.cs](../Integracao/ServicoTempoReal.cs)

## 2. Dicionário de dados

O dicionário segue as 16 tabelas de [Database/script.sql](../Database/script.sql). Consulte o [diagrama entidade-relacionamento](MODELAGEM.md) para visualizar as cardinalidades. A coluna **Nulo** indica se o MySQL aceita `NULL`. PK e FK indicam chave primária e chave estrangeira, respectivamente. Quando uma PK contém várias colunas, a unicidade se aplica ao conjunto.

### Modalidade

Mantém o catálogo de modalidades com nome, grupo e slug. Uma modalidade pode ter vários campeonatos e atletas.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `Nome` | `VARCHAR(50)` | Não | `UNIQUE` |
| `Grupo` | `VARCHAR(10)` | Não | — |
| `Slug` | `VARCHAR(20)` | Não | `UNIQUE` |

`CHECK` `ck_modalidade_grupo` exige `CAST(Grupo AS BINARY) IN ('esportes', 'esports')`.

### Campeonato

Registra uma competição local por temporada. Cada campeonato pertence a uma modalidade e pode ter várias partidas e inscrições de times.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `Nome` | `VARCHAR(100)` | Não | — |
| `Temporada` | `INT` | Não | — |
| `ModalidadeId` | `INT` | Não | FK → `Modalidade.Id` |

### Time

Armazena a identificação de um time local. Um time pode ter várias inscrições, transferências e partidas, como mandante ou visitante.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `Nome` | `VARCHAR(80)` | Não | — |
| `Sigla` | `VARCHAR(4)` | Não | — |
| `Cidade` | `VARCHAR(80)` | Não | — |

### CampeonatoTime

Registra a relação N:N entre campeonatos e times. Cada inscrição pertence a um campeonato e a um time.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `CampeonatoId` | `INT` | Não | PK composta · FK → `Campeonato.Id` |
| `TimeId` | `INT` | Não | PK composta · FK → `Time.Id` |

### Atleta

Armazena a identificação esportiva e a modalidade de um atleta. O atleta pode ter várias transferências e até uma linha em cada tabela de estatística.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `Nome` | `VARCHAR(100)` | Não | — |
| `Nickname` | `VARCHAR(50)` | Sim | — |
| `Nacionalidade` | `VARCHAR(50)` | Não | — |
| `ModalidadeId` | `INT` | Não | FK → `Modalidade.Id` |

### Transferencia

Registra o histórico N:N entre atletas e times. Cada linha vincula um atleta a um time em um período. Data final nula indica um vínculo em aberto.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `AtletaId` | `INT` | Não | FK → `Atleta.Id` |
| `TimeId` | `INT` | Não | FK → `Time.Id` |
| `DataInicio` | `DATE` | Não | — |
| `DataFim` | `DATE` | Sim | — |

### Partida

Persiste uma partida local, seus participantes, horário, placar, estado, minuto e formato de série. Cada partida tem um campeonato, um mandante e um visitante.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `CampeonatoId` | `INT` | Não | FK → `Campeonato.Id` |
| `TimeCasaId` | `INT` | Não | FK → `Time.Id` |
| `TimeForaId` | `INT` | Não | FK → `Time.Id` |
| `Tipo` | `VARCHAR(20)` | Não | — |
| `DataHora` | `DATETIME` | Não | — |
| `PlacarCasa` | `INT` | Não | Padrão `0` |
| `PlacarFora` | `INT` | Não | Padrão `0` |
| `Status` | `VARCHAR(9)` | Não | Padrão `'Agendada'` |
| `Minuto` | `INT` | Sim | — |
| `MelhorDe` | `INT` | Não | Padrão `0` |

`CHECK` `ck_partida_status` exige `CAST(Status AS BINARY) IN ('Agendada', 'AoVivo', 'Encerrada')`.

`CHECK` `ck_partida_minuto` exige `Minuto IS NULL OR Minuto >= 0`.

### EstatisticaFutebol

Armazena os indicadores de futebol de um atleta. Cada atleta pode ter zero ou uma linha nesta tabela.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `AtletaId` | `INT` | Não | FK → `Atleta.Id` · `UNIQUE` |
| `Gols` | `INT` | Não | — |
| `ChutesAGol` | `INT` | Não | — |
| `Escanteios` | `INT` | Não | — |
| `Cartoes` | `INT` | Não | — |
| `Finalizacoes` | `INT` | Não | — |

### EstatisticaBasquete

Armazena os indicadores de basquete de um atleta. Cada atleta pode ter zero ou uma linha nesta tabela.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `AtletaId` | `INT` | Não | FK → `Atleta.Id` · `UNIQUE` |
| `Pontos` | `INT` | Não | — |
| `Assistencias` | `INT` | Não | — |
| `MediaPontos` | `DECIMAL(5,2)` | Não | — |
| `Faltas` | `INT` | Não | — |
| `Rebotes` | `INT` | Não | — |

### EstatisticaValorant

Armazena abates, assistências e agente de um atleta no Valorant. Cada atleta pode ter zero ou uma linha nesta tabela.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `AtletaId` | `INT` | Não | FK → `Atleta.Id` · `UNIQUE` |
| `Abates` | `INT` | Não | — |
| `Assistencias` | `INT` | Não | — |
| `Agente` | `VARCHAR(50)` | Não | — |

### EstatisticaCsGo

Armazena abates, assistências e headshots de um atleta no Counter-Strike. Cada atleta pode ter zero ou uma linha nesta tabela.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `AtletaId` | `INT` | Não | FK → `Atleta.Id` · `UNIQUE` |
| `Abates` | `INT` | Não | — |
| `Assistencias` | `INT` | Não | — |
| `Headshots` | `INT` | Não | — |

### Usuario

Armazena a identificação da conta e o campo destinado ao hash da senha. Um usuário pode ter zero ou uma preferência e várias linhas em cada tabela de atividade.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `Nome` | `VARCHAR(100)` | Não | — |
| `Email` | `VARCHAR(150)` | Não | `UNIQUE` |
| `SenhaHash` | `VARCHAR(255)` | Não | — |
| `CriadoEm` | `DATETIME` | Não | Padrão `CURRENT_TIMESTAMP` |
| `UltimoAcesso` | `DATETIME` | Sim | — |
| `Ativo` | `TINYINT(1)` | Não | Padrão `1` |

### PreferenciaUsuario

Armazena as preferências de um usuário. O slug em `ModalidadePadrao` é texto sem FK. Cada linha pertence a um usuário.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `UsuarioId` | `INT` | Não | PK · FK → `Usuario.Id`, `ON DELETE CASCADE` |
| `GrupoPadrao` | `VARCHAR(10)` | Não | Padrão `'esportes'` |
| `ModalidadePadrao` | `VARCHAR(20)` | Sim | — |
| `FusoHorario` | `VARCHAR(40)` | Não | Padrão `'America/Sao_Paulo'` |
| `Idioma` | `VARCHAR(10)` | Não | Padrão `'pt-BR'` |
| `NotificacoesAtivas` | `TINYINT(1)` | Não | Padrão `1` |

`CHECK` `ck_pref_grupo` exige `GrupoPadrao IN ('esportes', 'esports')`.

### Favorito

Armazena os alvos favoritos de um usuário. `Nome` é uma cópia opcional para exibição. A PK permite um favorito por usuário, tipo, fonte e ID.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `UsuarioId` | `INT` | Não | PK composta · FK → `Usuario.Id`, `ON DELETE CASCADE` |
| `Tipo` | `VARCHAR(12)` | Não | PK composta |
| `Fonte` | `VARCHAR(20)` | Não | PK composta |
| `IdExterno` | `VARCHAR(60)` | Não | PK composta |
| `Nome` | `VARCHAR(120)` | Sim | — |
| `CriadoEm` | `DATETIME` | Não | Padrão `CURRENT_TIMESTAMP` |

`CHECK` `ck_fav_tipo` exige `Tipo IN ('time', 'atleta', 'competicao', 'partida')`.

### Notificacao

Armazena mensagens e o estado de leitura por usuário. A referência para a partida é opcional. `InicioPartida`, `Gol` e `FimPartida` aparecem nos exemplos, sem restrição de enumeração no DDL.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `UsuarioId` | `INT` | Não | FK → `Usuario.Id`, `ON DELETE CASCADE` |
| `Tipo` | `VARCHAR(20)` | Não | — |
| `Fonte` | `VARCHAR(20)` | Sim | — |
| `PartidaIdExterno` | `VARCHAR(60)` | Sim | — |
| `Mensagem` | `VARCHAR(200)` | Não | — |
| `Lida` | `TINYINT(1)` | Não | Padrão `0` |
| `CriadaEm` | `DATETIME` | Não | Padrão `CURRENT_TIMESTAMP` |

Índice `idx_notif_usuario` em `(UsuarioId, Lida)`.

### HistoricoVisualizacao

Registra cada visualização de um usuário. O mesmo alvo pode aparecer várias vezes, pois cada visita tem seu próprio ID.

| Coluna | Tipo SQL | Nulo | Chaves e padrão |
| --- | --- | --- | --- |
| `Id` | `INT` | Não | PK · `AUTO_INCREMENT` |
| `UsuarioId` | `INT` | Não | FK → `Usuario.Id`, `ON DELETE CASCADE` |
| `Tipo` | `VARCHAR(12)` | Não | — |
| `Fonte` | `VARCHAR(20)` | Não | — |
| `IdExterno` | `VARCHAR(60)` | Não | — |
| `Nome` | `VARCHAR(120)` | Sim | — |
| `VisualizadoEm` | `DATETIME` | Não | Padrão `CURRENT_TIMESTAMP` |

Índice `idx_hist_usuario` em `(UsuarioId, VisualizadoEm)`.

## 3. Regras de integridade

### Garantias do MySQL

- As 17 FKs são obrigatórias e apontam para a PK da tabela referenciada. As relações esportivas sem cláusula de cascata bloqueiam a exclusão de registros referenciados. Por exemplo, transferências ou estatísticas impedem a exclusão de um atleta. A exclusão de `Usuario` remove as linhas dependentes de `PreferenciaUsuario`, `Favorito`, `Notificacao` e `HistoricoVisualizacao` por `ON DELETE CASCADE`.
- `UNIQUE` garante a unicidade de `Modalidade.Nome`, `Modalidade.Slug`, `Usuario.Email` e `AtletaId` em cada tabela de estatísticas. A PK de `PreferenciaUsuario` limita a preferência a uma por usuário. As PKs compostas de `CampeonatoTime` e `Favorito` impedem a duplicação da inscrição ou do favorito, respectivamente.
- Os cinco `CHECK` restringem o grupo da modalidade, o grupo da preferência, o tipo do favorito, o status da partida e o minuto. Os testes sobre `Modalidade.Grupo` e `Partida.Status` comparam valores em binário. `Status` aceita `Agendada`, `AoVivo` ou `Encerrada`. `Minuto` aceita `NULL` ou um inteiro maior ou igual a zero, independentemente do status.
- O banco exige que os dois times da partida existam e que o campeonato exista. As FKs apontam diretamente para `Time` e `Campeonato`. O DDL não verifica a participação dos times no campeonato nem a correspondência entre `Partida.Tipo` e a modalidade do campeonato. A FK de cada estatística garante a existência do atleta, mas não valida a modalidade dele.
- As referências textuais de favoritos, notificações e visualizações não participam da integridade referencial esportiva. Excluir um registro esportivo local não apaga essas referências.

### Regras aplicadas no C#

- O construtor de `Partida` exige times distintos e valida o enum de status e o minuto inicial. Novas partidas do fluxo MVC começam em `Agendada`. `Iniciar()` muda de `Agendada` para `AoVivo`. `Encerrar()` aceita uma partida agendada ou ao vivo se o placar final passar pela validação da subclasse. O estado persistido não é deduzido de `DataHora`.
- `RegistrarPlacar()` rejeita valores negativos e permite alterar o placar enquanto a partida não estiver encerrada. `AtualizarMinuto()` só aceita alterações enquanto a partida estiver ao vivo. Um valor nulo limpa o minuto. O formulário preserva o minuto quando o campo está ausente. Iniciar, alterar placar ou minuto e encerrar falham quando a partida já está encerrada. A exclusão usa um caminho separado por ID. [Partida.cs](../Models/Partida.cs) · [PartidasController.cs](../Controllers/PartidasController.cs)
- Ao encerrar, futebol permite empate, basquete rejeita empate, vôlei exige três sets do vencedor e no máximo dois do adversário, e e-sports exige o número de mapas necessário para vencer uma série melhor de 1, 3 ou 5. Essas validações de placar, transições e elegibilidade do minuto ficam na aplicação. [PartidaFutebol.cs](../Models/PartidaFutebol.cs) · [PartidaBasquete.cs](../Models/PartidaBasquete.cs) · [PartidaVolei.cs](../Models/PartidaVolei.cs) · [PartidaEsport.cs](../Models/PartidaEsport.cs)
- O domínio considera ativo um vínculo com `DataFim IS NULL`. Ao registrar uma transferência, `Atleta.Transferir()` rejeita a repetição do time ativo e encerra o vínculo anterior na data de início do novo. O modelo rejeita histórico com mais de um vínculo em aberto e data final anterior à inicial. O schema só garante as referências a atleta e time. [Atleta.cs](../Models/Atleta.cs) · [Transferencia.cs](../Models/Transferencia.cs)
- O domínio também valida, por exemplo, a sigla de time com 2 a 4 caracteres após remover espaços externos e a temporada do campeonato entre 1900 e 2100. O banco armazena esses campos como `VARCHAR(4)` e `INT`. [Time.cs](../Models/Time.cs) · [Campeonato.cs](../Models/Campeonato.cs)

## 4. Persistência com Dapper

### Conexões e consultas

`IDbConnectionFactory.Criar()` retorna uma `IDbConnection`. A implementação `MySqlConnectionFactory` lê `ConnectionStrings:Padrao` e cria uma nova `MySqlConnection` por chamada. A fábrica é registrada como singleton e os repositórios têm escopo de requisição. Cada operação descarta a própria conexão com `using`. [IDbConnectionFactory.cs](../Repositories/Dapper/IDbConnectionFactory.cs) · [Program.cs](../Program.cs)

Os repositórios usam `Query` e `QuerySingleOrDefault` para ler, `Execute` para atualizar ou excluir e `ExecuteScalar<int>` para obter o ID gerado com `LAST_INSERT_ID()` na mesma conexão do `INSERT`. Consultas com valores variáveis usam parâmetros como `@id`, preenchidos com objetos passados ao Dapper. Os trechos concatenados nas consultas de partida são constantes SQL.

| Repositório | Operações implementadas |
| --- | --- |
| [ModalidadeRepository](../Repositories/Dapper/ModalidadeRepository.cs) | Lista o catálogo por nome com ID, grupo e slug. |
| [TimeRepository](../Repositories/Dapper/TimeRepository.cs) | Lista, busca por ID, insere, atualiza e exclui times. |
| [CampeonatoRepository](../Repositories/Dapper/CampeonatoRepository.cs) | CRUD de campeonatos. As leituras juntam o nome da modalidade. |
| [AtletaRepository](../Repositories/Dapper/AtletaRepository.cs) | CRUD de atletas. As leituras carregam e reconstroem o histórico de transferências. |
| [TransferenciaRepository](../Repositories/Dapper/TransferenciaRepository.cs) | Atualiza as datas finais dos vínculos existentes encerrados e insere os novos vínculos do atleta. |
| [PartidaRepository](../Repositories/Dapper/PartidaRepository.cs) | Lista, filtra por campeonato, busca por ID, insere e exclui. A leitura monta a subclasse pelo tipo gravado. A atualização persiste alterações de estado e placar no mesmo comando. |
| [PartidaConsultaRepository](../Repositories/Dapper/PartidaConsultaRepository.cs) | Consulta resumos com campeonato, modalidade e times, lista competições e calcula retrospectos. O retrospecto conta partidas encerradas entre os times em qualquer mando, exclui a própria partida e lista até cinco confrontos mais recentes por `DataHora`. |

Os repositórios de time, campeonato e atleta convertem erros de FK na exclusão em mensagens de domínio. A mensagem de exclusão de atleta cita transferências mesmo quando o bloqueio decorre de uma estatística. A escrita de campeonatos e a inserção de partidas também traduzem erros de referência inexistente.

### Transações e concorrência

- `TransferenciaRepository.SalvarHistorico()` abre uma transação explícita, atualiza vínculos existentes encerrados e insere os novos. Os IDs são atribuídos aos objetos após o `Commit`. Em erro de FK por referência inexistente, o repositório chama `Rollback` e lança uma exceção de domínio. O histórico é lido antes dessa transação. As atualizações não conferem o estado anterior e o schema não impõe unicidade de vínculo aberto, por isso requisições simultâneas podem gravar mais de um vínculo ativo.
- `PartidaRepository.Atualizar()` usa controle otimista. O `UPDATE` exige o mesmo ID, status, placares e minuto lidos pelo repositório e exclui linhas já encerradas. O operador `<=>` permite comparar minutos nulos. Se o estado não mudou no objeto, o método termina sem escrita. Se uma escrita afeta zero linhas, o repositório lança uma exceção de domínio para solicitar recarga. Quando a escrita tem sucesso, o objeto registra o novo estado persistido.
- A proteção de partidas cobre os campos comparados por esse `UPDATE` entre a leitura no servidor e a gravação. `DataHora` é escrita pelo comando, mas fica fora da comparação e da detecção de mudanças. A exclusão por ID e as atualizações de times, campeonatos e atletas também ficam fora desse controle.

## 5. Dados de teste

O script carrega seis modalidades para o catálogo e um conjunto fictício de campeonatos, times, inscrições, atletas, transferências, partidas e estatísticas. Os dados permitem demonstrar consultas e relacionamentos logo após a criação da base. Há histórico de troca de time e partidas nos três estados, com datas fixas e status explícitos para que a demonstração não dependa do dia da execução.

A segunda parte carrega cinco usuários fictícios e exemplos de preferências, favoritos, notificações e visualizações. `HASH_DE_EXEMPLO_1` e os demais valores dessa série são marcadores sem hash de senha válido. As contas de exemplo servem para testar o armazenamento e os relacionamentos, não para autenticação. A referência de exemplo a uma partida PandaScore ilustra o formato do identificador externo. Os dados reais dos provedores chegam separadamente pelo cache.

## 6. Execução e segurança

**Use [Database/script.sql](../Database/script.sql) exclusivamente em uma instância descartável de desenvolvimento ou teste.** A primeira instrução, `DROP DATABASE IF EXISTS sportstats`, elimina a base com esse nome e todo o seu conteúdo. Em seguida, o script cria `sportstats` com `utf8mb4`, define as tabelas e carrega os dados de exemplo. Verifique a instância de destino antes de executar o script e mantenha as credenciais de conexão fora do versionamento.

Bases existentes exigem uma migração não destrutiva, testada em uma cópia. A migração do BACK-06 precisa preservar registros e FKs, preencher `Modalidade.Grupo` e `Slug` e definir `Partida.Status` para os dados anteriores antes de remover a coluna antiga `Encerrada`. A correspondência entre a coluna antiga e os novos estados precisa ser definida para os registros existentes, pois o booleano anterior não distinguia partidas agendadas de partidas ao vivo. Este documento descreve o requisito, sem fornecer ou executar uma migração.

## 7. Limitações e evolução

- **BACK-08** prevê cadastro, login, geração de hash de senha e criação de `Usuario` e `PreferenciaUsuario` na mesma transação. O schema já oferece as tabelas. A FK só exige que a preferência aponte para um usuário existente e a PK só limita a cardinalidade. O fluxo de cadastro, a autenticação e a transação conjunta seguem pendentes. [TAREFAS.md](../TAREFAS.md) · [Database/BACKEND.md](../Database/BACKEND.md)
- **BACK-09** prevê os fluxos de favoritos, histórico e notificações. As tabelas e os exemplos já existem, enquanto os repositórios, serviços e telas desses fluxos seguem pendentes. As notificações por usuário dependem do login do BACK-08. [TAREFAS.md](../TAREFAS.md) · [Hubs/BACKEND.md](../Hubs/BACKEND.md)
- `CampeonatoTime` tem registros iniciais e FKs, mas ainda não tem repositório de inscrição. As quatro tabelas de estatísticas também não têm repositórios e o ranking permanece no BACK-04. [Program.cs](../Program.cs) · [Estatistica.cs](../Models/Estatistica.cs)
- As partidas de provedores ficam no cache em memória. A persistência sugerida em comentário no cache ainda não faz parte do fluxo. [CachePartidasExternas.cs](../Integracao/CachePartidasExternas.cs)
