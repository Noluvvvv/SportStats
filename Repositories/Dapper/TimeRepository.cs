using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Repositories.Interfaces;
using global::Dapper;
using MySqlConnector;

namespace SportStats.Repositories.Dapper;

public class TimeRepository : IRepository<Time>
{
    private readonly IDbConnectionFactory _factory;
    public TimeRepository(IDbConnectionFactory factory) => _factory = factory;

    public IEnumerable<Time> ObterTodos()
    {
        using var con = _factory.Criar();
        return con.Query<Time>("SELECT Id, Nome, Sigla, Cidade FROM `Time` ORDER BY Nome").ToList();
    }

    public Time? ObterPorId(int id)
    {
        using var con = _factory.Criar();
        return con.QuerySingleOrDefault<Time>(
            "SELECT Id, Nome, Sigla, Cidade FROM `Time` WHERE Id = @id", new { id });
    }

    public int Inserir(Time t)
    {
        using var con = _factory.Criar();
        return con.ExecuteScalar<int>(
            @"INSERT INTO `Time` (Nome, Sigla, Cidade) VALUES (@Nome, @Sigla, @Cidade);
              SELECT LAST_INSERT_ID();",
            new { t.Nome, t.Sigla, t.Cidade });
    }

    public void Atualizar(Time t)
    {
        using var con = _factory.Criar();
        con.Execute("UPDATE `Time` SET Nome = @Nome, Sigla = @Sigla, Cidade = @Cidade WHERE Id = @Id",
            new { t.Nome, t.Sigla, t.Cidade, t.Id });
    }

    public void Remover(int id)
    {
        try
        {
            using var con = _factory.Criar();
            con.Execute("DELETE FROM `Time` WHERE Id = @id", new { id });
        }
        catch (MySqlException ex) when (ex.ErrorCode is MySqlErrorCode.RowIsReferenced2 or MySqlErrorCode.RowIsReferenced)
        {
            throw new DominioException("Não é possível excluir um time que possui partidas, campeonatos ou atletas vinculados.");
        }
    }
}