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
        public StatusPartida Status { get; set; }
        public int? Minuto { get; set; }
        public int MelhorDe { get; set; }
    }

    private const string Colunas =
        "Id, CampeonatoId, TimeCasaId, TimeForaId, Tipo, DataHora, PlacarCasa, PlacarFora, Status, Minuto, MelhorDe";

    private static Partida Montar(Linha l) => Enum.Parse<TipoPartida>(l.Tipo) switch
    {
        TipoPartida.Futebol => new PartidaFutebol(l.CampeonatoId, l.TimeCasaId, l.TimeForaId, l.DataHora,
                                   l.Id, l.PlacarCasa, l.PlacarFora, l.Status, l.Minuto),
        TipoPartida.Basquete => new PartidaBasquete(l.CampeonatoId, l.TimeCasaId, l.TimeForaId, l.DataHora,
                                   l.Id, l.PlacarCasa, l.PlacarFora, l.Status, l.Minuto),
        TipoPartida.Volei => new PartidaVolei(l.CampeonatoId, l.TimeCasaId, l.TimeForaId, l.DataHora,
                                   l.Id, l.PlacarCasa, l.PlacarFora, l.Status, l.Minuto),
        TipoPartida.Esport => new PartidaEsport(l.CampeonatoId, l.TimeCasaId, l.TimeForaId, l.DataHora,
                                   l.MelhorDe, l.Id, l.PlacarCasa, l.PlacarFora, l.Status, l.Minuto),
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
                                       PlacarCasa, PlacarFora, Status, Minuto, MelhorDe)
                  VALUES (@CampeonatoId, @TimeCasaId, @TimeForaId, @Tipo, @DataHora,
                          @PlacarCasa, @PlacarFora, @Status, @Minuto, @MelhorDe);
                  SELECT LAST_INSERT_ID();",
                new
                {
                    p.CampeonatoId, p.TimeCasaId, p.TimeForaId,
                    Tipo = p.Tipo.ToString(), p.DataHora,
                    p.PlacarCasa, p.PlacarFora, Status = p.Status.ToString(), p.Minuto, p.MelhorDe
                });
        }
        catch (MySqlException ex) when (ex.ErrorCode is MySqlErrorCode.NoReferencedRow2 or MySqlErrorCode.NoReferencedRow)
        {
            throw new DominioException("Campeonato ou time informado não existe.");
        }
    }

    public void Atualizar(Partida p)
    {
        var anterior = p.EstadoPersistido;
        if (p.EstadoAtual == anterior)
            return;

        using var con = _factory.Criar();
        var atualizadas = con.Execute(
            @"UPDATE Partida SET DataHora = @DataHora, PlacarCasa = @PlacarCasa,
                                 PlacarFora = @PlacarFora, Status = @Status, Minuto = @Minuto
              WHERE Id = @Id AND Status = @StatusAnterior AND Status <> @StatusEncerrada
                AND PlacarCasa = @PlacarCasaAnterior AND PlacarFora = @PlacarForaAnterior
                AND Minuto <=> @MinutoAnterior",
            new
            {
                p.Id, p.DataHora, p.PlacarCasa, p.PlacarFora, p.Minuto,
                Status = p.Status.ToString(),
                StatusAnterior = anterior.Status.ToString(),
                StatusEncerrada = nameof(StatusPartida.Encerrada),
                PlacarCasaAnterior = anterior.PlacarCasa,
                PlacarForaAnterior = anterior.PlacarFora,
                MinutoAnterior = anterior.Minuto
            });

        if (atualizadas == 0)
            throw new DominioException("A partida foi alterada ou excluída em outra requisição. Recarregue os detalhes e tente novamente.");

        p.RegistrarEstadoPersistido();
    }

    public void Remover(int id)
    {
        using var con = _factory.Criar();
        con.Execute("DELETE FROM Partida WHERE Id = @id", new { id });
    }
}
