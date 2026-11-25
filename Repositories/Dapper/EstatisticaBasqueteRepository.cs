using Dapper;
using SportStats.Models;
using SportStats.Repositories.Interfaces;

namespace SportStats.Repositories.Dapper;

public class EstatisticaBasqueteRepository : IRepository<EstatisticaBasquete>
{
    private readonly IDbConnectionFactory _factory;

    public EstatisticaBasqueteRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IEnumerable<EstatisticaBasquete> ObterTodos()
    {
        using var con = _factory.Criar();

        return con.Query<EstatisticaBasquete>(
            @"SELECT AtletaId, Pontos, Assistencias,
                 CAST(MediaPontos AS DOUBLE) AS MediaPontos,
                 Faltas, Rebotes, Id
          FROM EstatisticaBasquete
          ORDER BY Pontos DESC"
        ).ToList();
    }

    public EstatisticaBasquete? ObterPorId(int id)
    {
        using var con = _factory.Criar();

        return con.QuerySingleOrDefault<EstatisticaBasquete>(
            @"SELECT AtletaId, Pontos, Assistencias,
                 CAST(MediaPontos AS DOUBLE) AS MediaPontos,
                 Faltas, Rebotes, Id
          FROM EstatisticaBasquete
          WHERE Id = @id",
            new { id }
        );
    }

    public int Inserir(EstatisticaBasquete entidade)
    {
        using var con = _factory.Criar();

        var id = con.ExecuteScalar<int>(
            @"INSERT INTO EstatisticaBasquete
                (AtletaId, Pontos, Assistencias, MediaPontos, Faltas, Rebotes)
              VALUES
                (@AtletaId, @Pontos, @Assistencias, @MediaPontos, @Faltas, @Rebotes);
              SELECT LAST_INSERT_ID();",
            entidade
        );

        entidade.DefinirId(id);

        return id;
    }

    public void Atualizar(EstatisticaBasquete entidade)
    {
        using var con = _factory.Criar();

        con.Execute(
            @"UPDATE EstatisticaBasquete
              SET Pontos = @Pontos,
                  Assistencias = @Assistencias,
                  MediaPontos = @MediaPontos,
                  Faltas = @Faltas,
                  Rebotes = @Rebotes
              WHERE Id = @Id",
            entidade
        );
    }

    public void Remover(int id)
    {
        using var con = _factory.Criar();

        con.Execute(
            "DELETE FROM EstatisticaBasquete WHERE Id = @id",
            new { id }
        );
    }
}
