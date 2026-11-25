# Modelagem do SportStats

## 1. Estrutura implementada

O diagrama representa as tabelas do [schema MySQL](../Database/script.sql) após o BACK-06, validado em MySQL 8.4.13. O banco persiste os dados esportivos locais e os dados de usuário. O acesso ao banco usa Dapper.

`PK` indica chave primária, `FK` indica chave estrangeira e `UK` indica unicidade. Quando várias colunas da mesma tabela têm a marca `PK`, elas compõem uma única chave. As linhas contínuas indicam que a FK integra a PK da tabela dependente. As linhas tracejadas indicam que a FK fica fora da PK.

```mermaid
erDiagram
    Modalidade ||..o{ Campeonato : ModalidadeId
    Modalidade ||..o{ Atleta : ModalidadeId
    Campeonato ||--o{ CampeonatoTime : CampeonatoId
    Time ||--o{ CampeonatoTime : TimeId
    Atleta ||..o{ Transferencia : AtletaId
    Time ||..o{ Transferencia : TimeId
    Campeonato ||..o{ Partida : CampeonatoId
    Time ||..o{ Partida : TimeCasaId
    Time ||..o{ Partida : TimeForaId
    Atleta ||..o| EstatisticaFutebol : AtletaId
    Atleta ||..o| EstatisticaBasquete : AtletaId
    Atleta ||..o| EstatisticaValorant : AtletaId
    Atleta ||..o| EstatisticaCsGo : AtletaId
    Usuario ||--o| PreferenciaUsuario : UsuarioId
    Usuario ||--o{ Favorito : UsuarioId
    Usuario ||..o{ Notificacao : UsuarioId
    Usuario ||..o{ HistoricoVisualizacao : UsuarioId

    Modalidade {
        int Id PK
        varchar(50) Nome UK
        varchar(10) Grupo
        varchar(20) Slug UK
    }
    Campeonato {
        int Id PK
        varchar(100) Nome
        int Temporada
        int ModalidadeId FK
    }
    Time {
        int Id PK
        varchar(80) Nome
        varchar(4) Sigla
        varchar(80) Cidade
    }
    CampeonatoTime {
        int CampeonatoId PK, FK
        int TimeId PK, FK
    }
    Atleta {
        int Id PK
        varchar(100) Nome
        varchar(50) Nickname "opcional"
        varchar(50) Nacionalidade
        int ModalidadeId FK
    }
    Transferencia {
        int Id PK
        int AtletaId FK
        int TimeId FK
        date DataInicio
        date DataFim "opcional"
    }
    Partida {
        int Id PK
        int CampeonatoId FK
        int TimeCasaId FK
        int TimeForaId FK
        varchar(20) Tipo
        datetime DataHora
        int PlacarCasa
        int PlacarFora
        varchar(9) Status
        int Minuto "opcional"
        int MelhorDe
    }
    EstatisticaFutebol {
        int Id PK
        int AtletaId FK, UK
        int Gols
        int ChutesAGol
        int Escanteios
        int Cartoes
        int Finalizacoes
    }
    EstatisticaBasquete {
        int Id PK
        int AtletaId FK, UK
        int Pontos
        int Assistencias
        decimal(5,2) MediaPontos
        int Faltas
        int Rebotes
    }
    EstatisticaValorant {
        int Id PK
        int AtletaId FK, UK
        int Abates
        int Assistencias
        varchar(50) Agente
    }
    EstatisticaCsGo {
        int Id PK
        int AtletaId FK, UK
        int Abates
        int Assistencias
        int Headshots
    }
    Usuario {
        int Id PK
        varchar(100) Nome
        varchar(150) Email UK
        varchar(255) SenhaHash
        datetime CriadoEm
        datetime UltimoAcesso "opcional"
        tinyint(1) Ativo
    }
    PreferenciaUsuario {
        int UsuarioId PK, FK
        varchar(10) GrupoPadrao
        varchar(20) ModalidadePadrao "opcional"
        varchar(40) FusoHorario
        varchar(10) Idioma
        tinyint(1) NotificacoesAtivas
    }
    Favorito {
        int UsuarioId PK, FK
        varchar(12) Tipo PK
        varchar(20) Fonte PK
        varchar(60) IdExterno PK
        varchar(120) Nome "opcional"
        datetime CriadoEm
    }
    Notificacao {
        int Id PK
        int UsuarioId FK
        varchar(20) Tipo
        varchar(20) Fonte "opcional"
        varchar(60) PartidaIdExterno "opcional"
        varchar(200) Mensagem
        tinyint(1) Lida
        datetime CriadaEm
    }
    HistoricoVisualizacao {
        int Id PK
        int UsuarioId FK
        varchar(12) Tipo
        varchar(20) Fonte
        varchar(60) IdExterno
        varchar(120) Nome "opcional"
        datetime VisualizadoEm
    }
```

### Dados esportivos

- O catálogo de modalidades vem da tabela `Modalidade`, consultada por `ModalidadeRepository`. `Grupo` aceita `esportes` ou `esports`. Cada campeonato e cada atleta pertence a uma modalidade.
- `CampeonatoTime` representa a relação N:N entre campeonato e time. A chave composta `(CampeonatoId, TimeId)` permite uma única inscrição por par.
- `Transferencia` registra a relação N:N histórica entre atleta e time. Cada vínculo tem `DataInicio` e pode ter `DataFim`. Uma data final nula indica um vínculo em aberto.
- Cada partida pertence a um campeonato e referencia `Time` em dois papéis, mandante e visitante. As FKs dos times apontam diretamente para `Time.Id`. A inscrição no campeonato é registrada separadamente em `CampeonatoTime`.
- `Partida.Status` tem padrão `Agendada` e aceita `Agendada`, `AoVivo` ou `Encerrada`. `Minuto` aceita valor nulo ou inteiro não negativo.
- Futebol, basquete, Valorant e Counter-Strike têm tabelas de estatística próprias. Cada registro pertence a um atleta e a restrição `UNIQUE` em `AtletaId` limita cada atleta a zero ou um registro por tabela. A FK aponta para o atleta, que carrega a referência à modalidade.

### Usuários e atividade

- `PreferenciaUsuario.UsuarioId` é PK e FK. Cada preferência pertence a um usuário e cada usuário pode ter zero ou uma preferência no banco. `ModalidadePadrao` guarda o slug como texto, sem FK para `Modalidade`. `GrupoPadrao` aceita `esportes` ou `esports`.
- Cada usuário pode ter vários favoritos, notificações e registros de visualização. As quatro FKs para `Usuario` usam `ON DELETE CASCADE`.
- `Favorito` identifica o alvo por `Tipo`, `Fonte` e `IdExterno`. O tipo aceita `time`, `atleta`, `competicao` ou `partida`. `HistoricoVisualizacao` usa os mesmos campos para registrar cada visualização. `Notificacao` pode guardar a referência da partida em `Fonte` e `PartidaIdExterno`.
- As referências a alvos esportivos são textuais, sem FK. A fonte `SportStats` identifica um ID interno armazenado como texto. As demais fontes identificam registros externos.
- `Notificacao` tem índice em `(UsuarioId, Lida)`. `HistoricoVisualizacao` tem índice em `(UsuarioId, VisualizadoEm)`. `Usuario.SenhaHash` guarda o hash da senha.

## 2. Funcionalidades previstas

- A integração de favoritos, notificações e histórico com a aplicação está prevista no BACK-09. As tabelas correspondentes já fazem parte do schema.
- Parciais de partida (tempos, quartos, sets ou mapas), eventos (gols, cartões e substituições) e escalações constam da proposta anterior de modelagem. A proposta também incluía fase da partida, posição e data de nascimento do atleta.
- A proposta de estatísticas incluía tabelas para vôlei (pontos, aces, bloqueios e ataques) e League of Legends (mortes, campeão e função), além de arremessos tentados e convertidos no basquete e função do jogador no Valorant.
- Entre as regras de integridade propostas estão exigir times distintos em uma partida, exigir `DataFim >= DataInicio`, limitar o atleta a um vínculo em aberto e garantir que arremessos convertidos sejam no máximo iguais aos tentados.

### Proposta histórica de atualização em tempo real

A proposta previa receber dados esportivos por WebSocket, identificar os registros do provedor por IDs textuais e manter os dados recebidos em memória. O exemplo registra o formato discutido para alinhamento com SignalR no BACK-05.

```json
{
  "tipo": "placar",
  "partidaId": "ext-12345",
  "modalidade": "futebol",
  "status": "AoVivo",
  "minuto": 67,
  "casa": { "id": "ext-t1", "nome": "Atlético Norte", "placar": 2 },
  "fora": { "id": "ext-t2", "nome": "Real Cidade", "placar": 1 },
  "parciais": [ { "numero": 1, "descricao": "Primeiro tempo", "casa": 1, "fora": 0 } ],
  "eventos": [ { "tipo": "Gol", "minuto": 12, "atletaId": "ext-a1", "timeId": "ext-t1" } ]
}
```

Na proposta, ficaram em aberto a escolha da fonte para as atualizações em tempo real, o eventual cache de partidas encerradas no MySQL, a adoção de login social e os eventos que gerariam notificações.
