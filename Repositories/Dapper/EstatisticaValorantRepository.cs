using Dapper;
using SportStats.Models;
using SportStats.Repositories.Interfaces;

namespace SportStats.Repositories.Dapper;

public class EstatisticaValorantRepository : IRepository<EstatisticaValorant>
{
    private readonly IDbConnectionFactory _factory;

    public EstatisticaValorantRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IEnumerable<EstatisticaValorant> ObterTodos()
    {
        using var con = _factory.Criar();

        return con.Query<EstatisticaValorant>(
            @"SELECT AtletaId, Abates, Assistencias, Agente, Id
          FROM EstatisticaValorant
          ORDER BY Abates DESC"
        ).ToList();
    }

    public EstatisticaValorant? ObterPorId(int id)
    {
        using var con = _factory.Criar();

        return con.QuerySingleOrDefault<EstatisticaValorant>(
            @"SELECT AtletaId, Abates, Assistencias, Agente, Id
          FROM EstatisticaValorant
          WHERE Id = @id",
            new { id }
        );
    }

    public int Inserir(EstatisticaValorant entidade)
    {
        using var con = _factory.Criar();

        var id = con.ExecuteScalar<int>(
            @"INSERT INTO EstatisticaValorant
                (AtletaId, Abates, Assistencias, Agente)
              VALUES
                (@AtletaId, @Abates, @Assistencias, @Agente);
              SELECT LAST_INSERT_ID();",
            entidade
        );

        entidade.DefinirId(id);

        return id;
    }

    public void Atualizar(EstatisticaValorant entidade)
    {
        using var con = _factory.Criar();

        con.Execute(
            @"UPDATE EstatisticaValorant
              SET Abates = @Abates,
                  Assistencias = @Assistencias,
                  Agente = @Agente
              WHERE Id = @Id",
            entidade
        );
    }

    public void Remover(int id)
    {
        using var con = _factory.Criar();

        con.Execute(
            "DELETE FROM EstatisticaValorant WHERE Id = @id",
            new { id }
        );
    }
}
