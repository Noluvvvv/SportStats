using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Models.Enums;
using SportStats.Repositories.Interfaces;
using global::Dapper;
using MySqlConnector;

namespace SportStats.Repositories.Dapper;

public class PartidaRepository : IPartidaRepository
{
    private readonly IDbConnectionFactory _factory;
    public PartidaRepository(IDbConnectionFactory factory) => _factory = factory;

    private sealed class Linha
    {
        public int Id { get; set; }
        public int CampeonatoId { get; set; }
        public int TimeCasaId { get; set; }
        public int TimeForaId { get; set; }
        public string Tipo { get; set; } = "";
        public DateTime DataHora { get; set; }
        public int PlacarCasa { get; set; }
        public int PlacarFora { get; set; }
        public bool Encerrada { get; set; }
        public int MelhorDe { get; set; }
    }

    private const string Colunas =
        "Id, CampeonatoId, TimeCasaId, TimeForaId, Tipo, DataHora, PlacarCasa, PlacarFora, Encerrada, MelhorDe";

    private static Partida Montar(Linha l) => Enum.Parse<TipoPartida>(l.Tipo) switch
    {
        TipoPartida.Futebol => new PartidaFutebol(l.CampeonatoId, l.TimeCasaId, l.TimeForaId, l.DataHora,
                                   l.Id, l.PlacarCasa, l.PlacarFora, l.Encerrada),
        TipoPartida.Basquete => new PartidaBasquete(l.CampeonatoId, l.TimeCasaId, l.TimeForaId, l.DataHora,
                                   l.Id, l.PlacarCasa, l.PlacarFora, l.Encerrada),
        TipoPartida.Volei => new PartidaVolei(l.CampeonatoId, l.TimeCasaId, l.TimeForaId, l.DataHora,
                                   l.Id, l.PlacarCasa, l.PlacarFora, l.Encerrada),
        TipoPartida.Esport => new PartidaEsport(l.CampeonatoId, l.TimeCasaId, l.TimeForaId, l.DataHora,
                                   l.MelhorDe, l.Id, l.PlacarCasa, l.PlacarFora, l.Encerrada),
        _ => throw new InvalidOperationException($"Tipo de partida desconhecido: {l.Tipo}")
    };

    public IEnumerable<Partida> ObterTodos()
    {
        using var con = _factory.Criar();
        return con.Query<Linha>("SELECT " + Colunas + " FROM Partida ORDER BY DataHora DESC")
                  .Select(Montar).ToList();
    }

    public IEnumerable<Partida> ObterPorCampeonato(int campeonatoId)
    {
        using var con = _factory.Criar();
        return con.Query<Linha>(
                "SELECT " + Colunas + " FROM Partida WHERE CampeonatoId = @campeonatoId ORDER BY DataHora",
                new { campeonatoId })
            .Select(Montar).ToList();
    }

    public Partida? ObterPorId(int id)
    {
        using var con = _factory.Criar();
        var linha = con.QuerySingleOrDefault<Linha>(
            "SELECT " + Colunas + " FROM Partida WHERE Id = @id", new { id });
        return linha is null ? null : Montar(linha);
    }

    public int Inserir(Partida p)
    {
        try
        {
            using var con = _factory.Criar();
            return con.ExecuteScalar<int>(
                @"INSERT INTO Partida (CampeonatoId, TimeCasaId, TimeForaId, Tipo, DataHora,
                                       PlacarCasa, PlacarFora, Encerrada, MelhorDe)
                  VALUES (@CampeonatoId, @TimeCasaId, @TimeForaId, @Tipo, @DataHora,
                          @PlacarCasa, @PlacarFora, @Encerrada, @MelhorDe);
                  SELECT LAST_INSERT_ID();",
                new
                {
                    p.CampeonatoId, p.TimeCasaId, p.TimeForaId,
                    Tipo = p.Tipo.ToString(), p.DataHora,
                    p.PlacarCasa, p.PlacarFora, p.Encerrada, p.MelhorDe
                });
        }
        catch (MySqlException ex) when (ex.ErrorCode is MySqlErrorCode.NoReferencedRow2 or MySqlErrorCode.NoReferencedRow)
        {
            throw new DominioException("Campeonato ou time informado não existe.");
        }
    }

    public void Atualizar(Partida p)
    {
        using var con = _factory.Criar();
        con.Execute(
            @"UPDATE Partida SET DataHora = @DataHora, PlacarCasa = @PlacarCasa,
                                 PlacarFora = @PlacarFora, Encerrada = @Encerrada
              WHERE Id = @Id",
            new { p.DataHora, p.PlacarCasa, p.PlacarFora, p.Encerrada, p.Id });
    }

    public void Remover(int id)
    {
        using var con = _factory.Criar();
        con.Execute("DELETE FROM Partida WHERE Id = @id", new { id });
    }
}