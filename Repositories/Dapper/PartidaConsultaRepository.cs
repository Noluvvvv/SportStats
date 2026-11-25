using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;
using global::Dapper;

namespace SportStats.Repositories.Dapper;

public class PartidaConsultaRepository : IPartidaConsultaRepository
{
    private readonly IDbConnectionFactory _factory;

    public PartidaConsultaRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    private const string SelectResumo = @"
        SELECT
            p.Id,
            p.CampeonatoId,
            c.Nome AS Campeonato,
            m.Nome AS Modalidade,
            p.Tipo,
            tc.Nome AS TimeCasa,
            tc.Sigla AS SiglaCasa,
            tf.Nome AS TimeFora,
            tf.Sigla AS SiglaFora,
            p.DataHora,
            p.PlacarCasa,
            p.PlacarFora,
            p.Encerrada,
            p.MelhorDe
        FROM Partida p
        JOIN Campeonato c ON c.Id = p.CampeonatoId
        JOIN Modalidade m ON m.Id = c.ModalidadeId
        JOIN `Time` tc ON tc.Id = p.TimeCasaId
        JOIN `Time` tf ON tf.Id = p.TimeForaId";

    private const string FiltroConfronto = @"
        p.Encerrada = 1
        AND p.Id <> @partidaId
        AND (
            (p.TimeCasaId = @timeCasaId AND p.TimeForaId = @timeForaId)
            OR
            (p.TimeCasaId = @timeForaId AND p.TimeForaId = @timeCasaId)
        )";

    public IEnumerable<PartidaResumo> Listar(
        IEnumerable<string> modalidades,
        int? campeonatoId)
    {
        var sql = SelectResumo
            + " WHERE m.Nome IN @modalidades"
            + (campeonatoId.HasValue
                ? " AND p.CampeonatoId = @campeonatoId"
                : "")
            + " ORDER BY p.DataHora DESC";

        using var con = _factory.Criar();

        return con.Query<PartidaResumo>(
            sql,
            new
            {
                modalidades = modalidades.ToArray(),
                campeonatoId
            }).ToList();
    }

    public IEnumerable<CompeticaoItem> ListarCompeticoes(
        IEnumerable<string> modalidades)
    {
        const string sql = @"
            SELECT
                c.Id,
                c.Nome,
                c.Temporada,
                m.Nome AS Modalidade
            FROM Campeonato c
            JOIN Modalidade m ON m.Id = c.ModalidadeId
            WHERE m.Nome IN @modalidades
            ORDER BY m.Nome, c.Nome";

        using var con = _factory.Criar();

        return con.Query<CompeticaoItem>(
            sql,
            new { modalidades = modalidades.ToArray() }).ToList();
    }

    public IEnumerable<PartidaResumo> ListarResumos()
    {
        using var con = _factory.Criar();

        return con.Query<PartidaResumo>(
            SelectResumo + " ORDER BY p.DataHora DESC").ToList();
    }

    public PartidaResumo? ObterResumoPorId(int id)
    {
        using var con = _factory.Criar();

        return con.QuerySingleOrDefault<PartidaResumo>(
            SelectResumo + " WHERE p.Id = @id",
            new { id });
    }

    public IEnumerable<CompeticaoItem> ListarCompeticoes()
    {
        const string sql = @"
            SELECT
                c.Id,
                c.Nome,
                c.Temporada,
                m.Nome AS Modalidade
            FROM Campeonato c
            JOIN Modalidade m ON m.Id = c.ModalidadeId
            ORDER BY m.Nome, c.Nome";

        using var con = _factory.Criar();

        return con.Query<CompeticaoItem>(sql).ToList();
    }

    private sealed class Totais
    {
        public int Jogos { get; set; }
        public int VitoriasCasa { get; set; }
        public int VitoriasFora { get; set; }
        public int Empates { get; set; }
    }

    public RetrospectoConfronto ObterRetrospecto(
        int partidaId,
        int timeCasaId,
        int timeForaId)
    {
        using var con = _factory.Criar();

        var parametros = new
        {
            partidaId,
            timeCasaId,
            timeForaId
        };

        var totais = con.QuerySingle<Totais>(
            @"
            SELECT
                COUNT(*) AS Jogos,

                COALESCE(SUM(
                    CASE
                        WHEN (p.TimeCasaId = @timeCasaId
                              AND p.PlacarCasa > p.PlacarFora)
                          OR (p.TimeForaId = @timeCasaId
                              AND p.PlacarFora > p.PlacarCasa)
                        THEN 1 ELSE 0
                    END
                ), 0) AS VitoriasCasa,

                COALESCE(SUM(
                    CASE
                        WHEN (p.TimeCasaId = @timeForaId
                              AND p.PlacarCasa > p.PlacarFora)
                          OR (p.TimeForaId = @timeForaId
                              AND p.PlacarFora > p.PlacarCasa)
                        THEN 1 ELSE 0
                    END
                ), 0) AS VitoriasFora,

                COALESCE(SUM(
                    CASE
                        WHEN p.PlacarCasa = p.PlacarFora
                        THEN 1 ELSE 0
                    END
                ), 0) AS Empates

            FROM Partida p
            WHERE " + FiltroConfronto,
            parametros);

        var ultimos = con.Query<PartidaResumo>(
            SelectResumo
            + " WHERE " + FiltroConfronto
            + " ORDER BY p.DataHora DESC LIMIT 5",
            parametros).ToList();

        return new RetrospectoConfronto
        {
            Jogos = totais.Jogos,
            VitoriasCasa = totais.VitoriasCasa,
            VitoriasFora = totais.VitoriasFora,
            Empates = totais.Empates,
            Ultimos = ultimos
        };
    }
}
