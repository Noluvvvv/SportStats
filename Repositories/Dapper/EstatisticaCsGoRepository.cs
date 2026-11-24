using Dapper;
using SportStats.Models;
using SportStats.Repositories.Interfaces;

namespace SportStats.Repositories.Dapper;

public class EstatisticaCsGoRepository : IRepository<EstatisticaCsGo>
{
    private readonly IDbConnectionFactory _factory;

    public EstatisticaCsGoRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IEnumerable<EstatisticaCsGo> ObterTodos()
    {
        using var con = _factory.Criar();

        return con.Query<EstatisticaCsGo>(
            @"SELECT Id, AtletaId, Abates, Assistencias, Headshots
              FROM EstatisticaCsGo
              ORDER BY Abates DESC"
        ).ToList();
    }

    public EstatisticaCsGo? ObterPorId(int id)
    {
        using var con = _factory.Criar();

        return con.QuerySingleOrDefault<EstatisticaCsGo>(
            @"SELECT Id, AtletaId, Abates, Assistencias, Headshots
              FROM EstatisticaCsGo
              WHERE Id = @id",
            new { id }
        );
    }

    public int Inserir(EstatisticaCsGo entidade)
    {
        using var con = _factory.Criar();

        var id = con.ExecuteScalar<int>(
            @"INSERT INTO EstatisticaCsGo
                (AtletaId, Abates, Assistencias, Headshots)
              VALUES
                (@AtletaId, @Abates, @Assistencias, @Headshots);
              SELECT LAST_INSERT_ID();",
            entidade
        );

        entidade.DefinirId(id);

        return id;
    }

    public void Atualizar(EstatisticaCsGo entidade)
    {
        using var con = _factory.Criar();

        con.Execute(
            @"UPDATE EstatisticaCsGo
              SET Abates = @Abates,
                  Assistencias = @Assistencias,
                  Headshots = @Headshots
              WHERE Id = @Id",
            entidade
        );
    }

    public void Remover(int id)
    {
        using var con = _factory.Criar();

        con.Execute(
            "DELETE FROM EstatisticaCsGo WHERE Id = @id",
            new { id }
        );
    }
}