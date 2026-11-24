using Dapper;
using SportStats.Models;
using SportStats.Repositories.Interfaces;

namespace SportStats.Repositories.Dapper;

public class EstatisticaFutebolRepository : IRepository<EstatisticaFutebol>
{
    private readonly IDbConnectionFactory _factory;

    public EstatisticaFutebolRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IEnumerable<EstatisticaFutebol> ObterTodos()
    {
        using var con = _factory.Criar();

        return con.Query<EstatisticaFutebol>(
            @"SELECT AtletaId, Gols, ChutesAGol, Escanteios,
                 Cartoes, Finalizacoes, Id
          FROM EstatisticaFutebol
          ORDER BY Gols DESC"
        ).ToList();
    }

    public EstatisticaFutebol? ObterPorId(int id)
    {
        using var con = _factory.Criar();

        return con.QuerySingleOrDefault<EstatisticaFutebol>(
            @"SELECT AtletaId, Gols, ChutesAGol, Escanteios,
                 Cartoes, Finalizacoes, Id
          FROM EstatisticaFutebol
          WHERE Id = @id",
            new { id }
        );
    }

    public int Inserir(EstatisticaFutebol entidade)
    {
        using var con = _factory.Criar();

        var id = con.ExecuteScalar<int>(
            @"INSERT INTO EstatisticaFutebol
                (AtletaId, Gols, ChutesAGol, Escanteios, Cartoes, Finalizacoes)
              VALUES
                (@AtletaId, @Gols, @ChutesAGol, @Escanteios, @Cartoes, @Finalizacoes);
              SELECT LAST_INSERT_ID();",
            entidade
        );

        entidade.DefinirId(id);

        return id;
    }

    public void Atualizar(EstatisticaFutebol entidade)
    {
        using var con = _factory.Criar();

        con.Execute(
            @"UPDATE EstatisticaFutebol
              SET Gols = @Gols,
                  ChutesAGol = @ChutesAGol,
                  Escanteios = @Escanteios,
                  Cartoes = @Cartoes,
                  Finalizacoes = @Finalizacoes
              WHERE Id = @Id",
            entidade
        );
    }

    public void Remover(int id)
    {
        using var con = _factory.Criar();

        con.Execute(
            "DELETE FROM EstatisticaFutebol WHERE Id = @id",
            new { id }
        );
    }
}