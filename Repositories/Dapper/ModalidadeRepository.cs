using SportStats.Models;
using SportStats.Repositories.Interfaces;
using global::Dapper;

namespace SportStats.Repositories.Dapper;

public class ModalidadeRepository : IModalidadeRepository
{
    private readonly IDbConnectionFactory _factory;
    public ModalidadeRepository(IDbConnectionFactory factory) => _factory = factory;

    public IEnumerable<ModalidadeItem> ObterTodas()
    {
        using var con = _factory.Criar();
        return con.Query<ModalidadeItem>("SELECT Id, Nome FROM Modalidade ORDER BY Nome").ToList();
    }
}
