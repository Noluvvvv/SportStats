using SportStats.Models;
using SportStats.Repositories.Interfaces;
using global::Dapper;

namespace SportStats.Repositories.Dapper;

public class FavoritoRepository : IFavoritoRepository
{
    private readonly IDbConnectionFactory _factory;

    public FavoritoRepository(IDbConnectionFactory factory) => _factory = factory;

    public IEnumerable<Favorito> ListarPorUsuario(int usuarioId)
    {
        using var con = _factory.Criar();
        return con.Query<Favorito>(
            """
            SELECT UsuarioId, Tipo, Fonte, IdExterno, Nome, CriadoEm
            FROM Favorito
            WHERE UsuarioId = @usuarioId
            ORDER BY CriadoEm DESC, Tipo, Fonte, IdExterno
            """,
            new { usuarioId }).ToList();
    }

    public bool Existe(int usuarioId, string tipo, string fonte, string idExterno)
    {
        using var con = _factory.Criar();
        return con.ExecuteScalar<int>(
            """
            SELECT EXISTS (
                SELECT 1
                FROM Favorito
                WHERE UsuarioId = @usuarioId
                  AND Tipo = @tipo
                  AND Fonte = @fonte
                  AND IdExterno = @idExterno
            )
            """,
            new { usuarioId, tipo, fonte, idExterno }) == 1;
    }
}
