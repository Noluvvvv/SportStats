using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Repositories.Interfaces;
using global::Dapper;
using MySqlConnector;

namespace SportStats.Repositories.Dapper;

public class CampeonatoRepository : IRepository<Campeonato>
{
    private readonly IDbConnectionFactory _factory;
    public CampeonatoRepository(IDbConnectionFactory factory) => _factory = factory;

    public IEnumerable<Campeonato> ObterTodos()
    {
        using var con = _factory.Criar();
        return con.Query<Campeonato>(
            @"SELECT c.Id, c.Nome, c.Temporada, c.ModalidadeId, m.Nome AS ModalidadeNome
              FROM Campeonato c
              JOIN Modalidade m ON m.Id = c.ModalidadeId
              ORDER BY c.Temporada DESC, c.Nome").ToList();
    }

    public Campeonato? ObterPorId(int id)
    {
        using var con = _factory.Criar();
        return con.QuerySingleOrDefault<Campeonato>(
            @"SELECT c.Id, c.Nome, c.Temporada, c.ModalidadeId, m.Nome AS ModalidadeNome
              FROM Campeonato c
              JOIN Modalidade m ON m.Id = c.ModalidadeId
              WHERE c.Id = @id", new { id });
    }

    public int Inserir(Campeonato c)
    {
        try
        {
            using var con = _factory.Criar();
            return con.ExecuteScalar<int>(
                @"INSERT INTO Campeonato (Nome, Temporada, ModalidadeId) VALUES (@Nome, @Temporada, @ModalidadeId);
                  SELECT LAST_INSERT_ID();",
                new { c.Nome, c.Temporada, c.ModalidadeId });
        }
        catch (MySqlException ex) when (ex.ErrorCode is MySqlErrorCode.NoReferencedRow2 or MySqlErrorCode.NoReferencedRow)
        {
            // A modalidade escolhida não existe no banco.
            throw new DominioException("A modalidade escolhida não existe.");
        }
    }

    public void Atualizar(Campeonato c)
    {
        try
        {
            using var con = _factory.Criar();
            con.Execute(
                "UPDATE Campeonato SET Nome = @Nome, Temporada = @Temporada, ModalidadeId = @ModalidadeId WHERE Id = @Id",
                new { c.Nome, c.Temporada, c.ModalidadeId, c.Id });
        }
        catch (MySqlException ex) when (ex.ErrorCode is MySqlErrorCode.NoReferencedRow2 or MySqlErrorCode.NoReferencedRow)
        {
            throw new DominioException("A modalidade escolhida não existe.");
        }
    }

    public void Remover(int id)
    {
        try
        {
            using var con = _factory.Criar();
            con.Execute("DELETE FROM Campeonato WHERE Id = @id", new { id });
        }
        catch (MySqlException ex) when (ex.ErrorCode is MySqlErrorCode.RowIsReferenced2 or MySqlErrorCode.RowIsReferenced)
        {
            throw new DominioException("Não é possível excluir um campeonato que possui partidas ou times vinculados.");
        }
    }
}
