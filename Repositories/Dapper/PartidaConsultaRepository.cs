using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;
using global::Dapper;

namespace SportStats.Repositories.Dapper;

public class PartidaConsultaRepository : IPartidaConsultaRepository
{
    private readonly IDbConnectionFactory _factory;
    public PartidaConsultaRepository(IDbConnectionFactory factory) => _factory = factory;

    private const string SelectPartidas = @"
        SELECT p.Id, p.CampeonatoId, c.Nome AS Campeonato, m.Nome AS Modalidade, p.Tipo,
               tc.Nome AS TimeCasa, tc.Sigla AS SiglaCasa,
               tf.Nome AS TimeFora, tf.Sigla AS SiglaFora,
               p.DataHora, p.PlacarCasa, p.PlacarFora, p.Encerrada, p.MelhorDe
        FROM Partida p
        JOIN Campeonato c ON c.Id = p.CampeonatoId
        JOIN Modalidade m ON m.Id = c.ModalidadeId
        JOIN `Time` tc ON tc.Id = p.TimeCasaId
        JOIN `Time` tf ON tf.Id = p.TimeForaId
        WHERE m.Nome IN @modalidades";

    public IEnumerable<PartidaResumo> Listar(IEnumerable<string> modalidades, int? campeonatoId)
    {
        // Apenas trechos fixos de SQL sao concatenados; os valores vao como parametros.
        var sql = SelectPartidas
                  + (campeonatoId.HasValue ? " AND p.CampeonatoId = @campeonatoId" : "")
                  + " ORDER BY p.DataHora DESC";

        using var con = _factory.Criar();
        return con.Query<PartidaResumo>(sql, new { modalidades = modalidades.ToArray(), campeonatoId }).ToList();
    }

    public IEnumerable<CompeticaoItem> ListarCompeticoes(IEnumerable<string> modalidades)
    {
        const string sql = @"
            SELECT c.Id, c.Nome, c.Temporada, m.Nome AS Modalidade
            FROM Campeonato c
            JOIN Modalidade m ON m.Id = c.ModalidadeId
            WHERE m.Nome IN @modalidades
            ORDER BY m.Nome, c.Nome";

        using var con = _factory.Criar();
        return con.Query<CompeticaoItem>(sql, new { modalidades = modalidades.ToArray() }).ToList();
    }
}