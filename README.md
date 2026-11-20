::: {align="center"}
# SportStats

### Acompanhe partidas, placares e competições de esportes e e-sports.

Um portal web acadêmico para consultar informações esportivas,
acompanhar partidas e gerenciar dados de times, campeonatos e jogos.

------------------------------------------------------------------------

## 📌 Sobre o projeto

O **SportStats** é um portal de estatísticas esportivas inspirado em
plataformas de acompanhamento de resultados, como o SofaScore. O sistema
reúne funcionalidades para esportes tradicionais e e-sports, combinando
dados cadastrados no próprio sistema com informações obtidas de
provedores externos.

A aplicação foi desenvolvida com **ASP.NET Core MVC**, persistência em
**MySQL** utilizando **Dapper** e atualização de informações em tempo
real por meio de **SignalR**.

##  Diagrama de Clase do Projeto

<img width="1047" height="450" alt="image" src="https://github.com/user-attachments/assets/12222eed-983e-40ac-a8ee-9ac0ca16b1df" />


## 🧰 Tecnologias utilizadas

  Técnologia                          Utilização
  ----------------------------------- -------------------------------------
  C# / .NET 10                        Linguagem e plataforma da aplicação
  ASP.NET Core MVC                    Aplicação web, controllers e views
  Razor                               Renderização das páginas
  MySQL                               Banco de dados relacional
  Dapper                              Acesso e persistência de dados
  MySqlConnector                      Conexão com o MySQL
  SignalR                             Comunicação em tempo real
  ESPN API                            Fonte externa de dados esportivos
  PandaScore API                      Fonte externa de dados de e-sports
  HTML, CSS, JavaScript e Bootstrap   Interface da aplicação

## 🏗️ Organização do projeto

``` text
SportStats/
├── Controllers/        # Controllers MVC e endpoints de integração
├── Database/           # Script de criação e carga do banco MySQL
├── Docs/               # Documentação e modelagem
├── Exceptions/         # Exceções e regras do domínio
├── Hubs/               # Hub SignalR
├── Integracao/         # Provedores externos, cache e atualização
│   ├── Espn/
│   ├── PandaScore/
│   ├── Providers/
│   ├── Services/
│   └── Modelos/
├── Models/             # Entidades, enums e ViewModels
├── Repositories/       # Interfaces e repositórios Dapper
├── Services/           # Serviços da aplicação
├── Views/              # Páginas Razor
├── wwwroot/            # CSS, JavaScript e arquivos estáticos
├── Program.cs          # Configuração e inicialização
└── SportStats.csproj   # Dependências e configuração do projeto
```

### Visão geral da arquitetura

A aplicação está organizada em camadas para separar responsabilidades:

-   **Controllers:** recebem as requisições e coordenam o fluxo da
    aplicação.
-   **Models:** representam entidades, regras de domínio e modelos de
    tela.
-   **Repositories:** concentram o acesso ao banco de dados com Dapper.
-   **Integracao:** conecta a aplicação aos provedores externos e mantém
    os dados recebidos em cache.
-   **Hubs:** disponibilizam comunicação em tempo real por SignalR.
-   **Views:** apresentam as informações ao usuário por meio de páginas
    Razor.

## 🚀 Como executar localmente

### 1. Pré-requisitos

Antes de iniciar, instale:

-   [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
-   [MySQL Server](https://dev.mysql.com/downloads/mysql/)
-   [MySQL Workbench](https://dev.mysql.com/downloads/workbench/)
-   Git

### 2. Clone o repositório

``` bash
git clone https://github.com/Noluvvvv/SportStats.git
cd SportStats
```

### 3. Configure o banco de dados

O projeto utiliza o banco `sportstats`. O script de criação está em:

``` text
Database/script.sql
```

Com o MySQL instalado e em execução, execute o script a partir da pasta
raiz do projeto:

**Windows (Prompt de Comando):**

``` cmd
mysql -u root -p < Database\script.sql
```

O comando solicitará a senha do usuário MySQL.

> ⚠️ **Atenção:** o script contém `DROP DATABASE IF EXISTS sportstats` e
> recria o banco. Isso apaga os dados existentes desse banco. Utilize
> somente em ambiente de desenvolvimento ou depois de fazer um backup.


### 6. Restaure as dependências e execute

Na pasta que contém `SportStats.csproj`, rode:

``` bash
dotnet restore
dotnet run
```

O terminal informará o endereço local em que a aplicação foi iniciada.
Abra esse endereço no navegador.

## 🗄️ Banco de dados

O banco é criado pelo script `Database/script.sql` e contém estruturas
para dados esportivos e dados de usuários.

Entre as tabelas definidas estão:

-   `Modalidade`
-   `Campeonato`
-   `Time`
-   `CampeonatoTime`
-   `Atleta`
-   `Transferencia`
-   `Partida`
-   Tabelas de estatísticas por modalidade
-   `Usuario`
-   `PreferenciaUsuario`
-   `Favorito`
-   `Notificacao`
-   `HistoricoVisualizacao`

Os dados obtidos pelas APIs externas são mantidos em cache de memória; o
script também define estruturas próprias para preferências, favoritos,
notificações e histórico.

"ANEXAR A MODELAGEM DO BANCO"


## 👥 Equipe

  Integrante                        Registro Acadêmico
  -------------------------------- --------------------
  Ângelo Avezum Marques da Silva      2840482511017
  Gabriel Henrique dos Santos Lopes   xxxxxxxxxxxxx
  Maxwell Carlos Silverio             xxxxxxxxxxxxx
  Saulo da Silva Stuque Menegucci     xxxxxxxxxxxxx

------------------------------------------------------------------------

::: {align="center"}
**SportStats** · Projeto acadêmico de Programação Orientada a Objetos
:::
