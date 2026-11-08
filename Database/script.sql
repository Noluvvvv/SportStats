DROP DATABASE IF EXISTS sportstats;
CREATE DATABASE sportstats CHARACTER SET utf8mb4;
USE sportstats;

CREATE TABLE Modalidade (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Campeonato (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Temporada INT NOT NULL,
    ModalidadeId INT NOT NULL,
    FOREIGN KEY (ModalidadeId) REFERENCES Modalidade(Id)
);

CREATE TABLE `Time` (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(80) NOT NULL,
    Sigla VARCHAR(4) NOT NULL,
    Cidade VARCHAR(80) NOT NULL
);

CREATE TABLE CampeonatoTime (
    CampeonatoId INT NOT NULL,
    TimeId INT NOT NULL,
    PRIMARY KEY (CampeonatoId, TimeId),
    FOREIGN KEY (CampeonatoId) REFERENCES Campeonato(Id),
    FOREIGN KEY (TimeId) REFERENCES `Time`(Id)
);

CREATE TABLE Atleta (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Nickname VARCHAR(50) NULL,
    Nacionalidade VARCHAR(50) NOT NULL,
    ModalidadeId INT NOT NULL,
    FOREIGN KEY (ModalidadeId) REFERENCES Modalidade(Id)
);

CREATE TABLE Transferencia (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AtletaId INT NOT NULL,
    TimeId INT NOT NULL,
    DataInicio DATE NOT NULL,
    DataFim DATE NULL,
    FOREIGN KEY (AtletaId) REFERENCES Atleta(Id),
    FOREIGN KEY (TimeId) REFERENCES `Time`(Id)
);

CREATE TABLE Partida (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CampeonatoId INT NOT NULL,
    TimeCasaId INT NOT NULL,
    TimeForaId INT NOT NULL,
    Tipo VARCHAR(20) NOT NULL,
    DataHora DATETIME NOT NULL,
    PlacarCasa INT NOT NULL DEFAULT 0,
    PlacarFora INT NOT NULL DEFAULT 0,
    Encerrada TINYINT(1) NOT NULL DEFAULT 0,
    MelhorDe INT NOT NULL DEFAULT 0,
    FOREIGN KEY (CampeonatoId) REFERENCES Campeonato(Id),
    FOREIGN KEY (TimeCasaId) REFERENCES `Time`(Id),
    FOREIGN KEY (TimeForaId) REFERENCES `Time`(Id)
);

CREATE TABLE EstatisticaFutebol (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AtletaId INT NOT NULL UNIQUE,
    Gols INT NOT NULL,
    ChutesAGol INT NOT NULL,
    Escanteios INT NOT NULL,
    Cartoes INT NOT NULL,
    Finalizacoes INT NOT NULL,
    FOREIGN KEY (AtletaId) REFERENCES Atleta(Id)
);

CREATE TABLE EstatisticaBasquete (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AtletaId INT NOT NULL UNIQUE,
    Pontos INT NOT NULL,
    Assistencias INT NOT NULL,
    MediaPontos DECIMAL(5,2) NOT NULL,
    Faltas INT NOT NULL,
    Rebotes INT NOT NULL,
    FOREIGN KEY (AtletaId) REFERENCES Atleta(Id)
);

CREATE TABLE EstatisticaValorant (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AtletaId INT NOT NULL UNIQUE,
    Abates INT NOT NULL,
    Assistencias INT NOT NULL,
    Agente VARCHAR(50) NOT NULL,
    FOREIGN KEY (AtletaId) REFERENCES Atleta(Id)
);

CREATE TABLE EstatisticaCsGo (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AtletaId INT NOT NULL UNIQUE,
    Abates INT NOT NULL,
    Assistencias INT NOT NULL,
    Headshots INT NOT NULL,
    FOREIGN KEY (AtletaId) REFERENCES Atleta(Id)
);

INSERT INTO Modalidade (Nome) VALUES
('Futebol'), ('Basquete'), ('Vôlei'), ('Valorant'), ('Counter-Strike');

INSERT INTO Campeonato (Nome, Temporada, ModalidadeId) VALUES
('Liga Nacional', 2026, 1),
('Copa Regional', 2026, 1),
('Liga de Basquete', 2026, 2),
('Superliga de Vôlei', 2026, 3),
('Valorant Champions Cup', 2026, 4),
('CS Pro Series', 2026, 5);

INSERT INTO `Time` (Nome, Sigla, Cidade) VALUES
('Atlético Norte', 'ATN', 'Ribeirão Preto'),
('Real Cidade', 'RCI', 'São Paulo'),
('União FC', 'UFC', 'Campinas'),
('Sport Leste', 'SPL', 'Santos'),
('Tigres Basquete', 'TIG', 'Franca'),
('Águias', 'AGU', 'Bauru'),
('Vulcan Esports', 'VUL', 'Curitiba'),
('Nova Legião', 'NLG', 'Porto Alegre');

INSERT INTO CampeonatoTime (CampeonatoId, TimeId) VALUES
(1,1), (1,2), (1,3), (1,4), (2,2), (2,4),
(3,5), (3,6), (4,3), (4,4), (5,7), (5,8), (6,7), (6,8);

INSERT INTO Atleta (Nome, Nickname, Nacionalidade, ModalidadeId) VALUES
('Lucas Almeida', NULL, 'Brasil', 1),
('Pedro Santos', NULL, 'Brasil', 1),
('Rafael Costa', NULL, 'Brasil', 1),
('Diego Fernández', NULL, 'Argentina', 1),
('Mateus Oliveira', NULL, 'Brasil', 1),
('João Barros', NULL, 'Brasil', 2),
('Marcos Lima', NULL, 'Brasil', 2),
('Caio Ribeiro', NULL, 'Brasil', 2),
('Derrick Hall', NULL, 'Estados Unidos', 2),
('André Souza', NULL, 'Brasil', 2),
('Felipe Rocha', 'Nox', 'Brasil', 4),
('Gustavo Mendes', 'Blaze', 'Brasil', 4),
('Thiago Nunes', 'Kira', 'Brasil', 4),
('Bruno Martins', 'Zeta', 'Brasil', 4),
('Leonardo Pinto', 'Ghost', 'Portugal', 4),
('Rodrigo Lopes', 'Hawk', 'Brasil', 5),
('Vinícius Ramos', 'Bolt', 'Brasil', 5),
('Eduardo Dias', 'Frag', 'Brasil', 5),
('Henrique Castro', 'Viper', 'Chile', 5),
('Samuel Cardoso', 'Echo', 'Brasil', 5);

INSERT INTO Transferencia (AtletaId, TimeId, DataInicio, DataFim) VALUES
(1, 1, '2025-01-10', NULL),
(2, 1, '2025-01-10', NULL),
(3, 2, '2024-07-01', NULL),
(4, 3, '2025-03-15', NULL),
(6, 5, '2024-02-01', NULL),
(7, 5, '2024-01-01', '2025-12-31'),
(7, 6, '2026-01-01', NULL),
(11, 7, '2025-05-20', NULL),
(12, 7, '2025-05-20', NULL),
(13, 8, '2025-08-01', NULL),
(16, 8, '2025-09-10', NULL);

INSERT INTO Partida (CampeonatoId, TimeCasaId, TimeForaId, Tipo, DataHora, PlacarCasa, PlacarFora, Encerrada, MelhorDe) VALUES
(1, 1, 2, 'Futebol',  '2026-10-20 16:00:00', 2, 1, 1, 0),
(1, 3, 4, 'Futebol',  '2026-10-20 18:30:00', 0, 0, 1, 0),
(1, 1, 3, 'Futebol',  '2026-10-27 16:00:00', 1, 0, 1, 0),
(2, 2, 4, 'Futebol',  '2026-11-05 19:00:00', 0, 0, 0, 0),
(3, 5, 6, 'Basquete', '2026-10-25 20:00:00', 98, 91, 1, 0),
(4, 3, 4, 'Volei',    '2026-10-26 21:00:00', 3, 1, 1, 0),
(5, 7, 8, 'Esport',   '2026-10-28 19:30:00', 2, 1, 1, 3),
(6, 8, 7, 'Esport',   '2026-11-06 20:00:00', 0, 0, 0, 1);

INSERT INTO EstatisticaFutebol (AtletaId, Gols, ChutesAGol, Escanteios, Cartoes, Finalizacoes) VALUES
(1, 12, 40, 5, 3, 55),
(2, 8, 30, 2, 5, 41),
(3, 3, 12, 20, 4, 22),
(4, 5, 18, 1, 6, 30),
(5, 9, 33, 4, 2, 47);

INSERT INTO EstatisticaBasquete (AtletaId, Pontos, Assistencias, MediaPontos, Faltas, Rebotes) VALUES
(6, 420, 95, 21.00, 48, 150),
(7, 380, 120, 19.00, 40, 90),
(8, 310, 60, 15.50, 55, 210),
(9, 520, 70, 26.00, 38, 180),
(10, 270, 140, 13.50, 44, 70);

INSERT INTO EstatisticaValorant (AtletaId, Abates, Assistencias, Agente) VALUES
(11, 210, 65, 'Jett'),
(12, 185, 90, 'Sova'),
(13, 160, 110, 'Omen'),
(14, 175, 70, 'Reyna'),
(15, 140, 120, 'Sage');

INSERT INTO EstatisticaCsGo (AtletaId, Abates, Assistencias, Headshots) VALUES
(16, 320, 80, 150),
(17, 290, 95, 130),
(18, 305, 70, 160),
(19, 260, 110, 100),
(20, 275, 85, 120);
-- TODO [BACK-06] Ajustes de banco (editar este arquivo e manter DROP/CREATE/INSERT completos):
--   1. Partida: adicionar colunas Status (Agendada/AoVivo/Encerrada) e Minuto.
--   2. Modalidade: adicionar colunas Grupo ('esportes'/'esports') e Slug.
--   3. Depois atualizar PartidaConsultaRepository, PartidaRepository.Montar e CatalogoModalidades.
--   4. Antes da entrega, trocar os NOW() das partidas de teste por datas fixas.