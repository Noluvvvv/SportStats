using System.Data;
using MySqlConnector;

namespace SportStats.Repositories.Dapper;

public interface IDbConnectionFactory
{
    IDbConnection Criar();
}

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration config) =>
        _connectionString = config.GetConnectionString("Padrao")
            ?? throw new InvalidOperationException("ConnectionString 'Padrao' não configurada.");

    public IDbConnection Criar() => new MySqlConnection(_connectionString);
}