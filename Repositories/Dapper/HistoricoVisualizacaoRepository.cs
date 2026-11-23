using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Repositories.Interfaces;
using global::Dapper;

namespace SportStats.Repositories.Dapper;

public class HistoricoVisualizacaoRepository : IHistoricoVisualizacaoRepository
{
    private const int LimiteMaximo = 100;
    private readonly IDbConnectionFactory _factory;

    public HistoricoVisualizacaoRepository(IDbConnectionFactory factory) => _factory = factory;

    public int Registrar(HistoricoVisualizacao visualizacao)
    {
        ArgumentNullException.ThrowIfNull(visualizacao);
        ValidarUsuario(visualizacao.UsuarioId);

        if (visualizacao.Tipo is not ("time" or "atleta" or "competicao" or "partida"))
            throw new DominioException("O tipo de visualização é inválido.");

        if (visualizacao.Fonte is not ("SportStats" or "ESPN" or "PandaScore"))
            throw new DominioException("A fonte da visualização é inválida.");

        if (string.IsNullOrWhiteSpace(visualizacao.IdExterno) || visualizacao.IdExterno.EnumerateRunes().Count() > 60)
            throw new DominioException("O ID externo é obrigatório e pode ter no máximo 60 caracteres.");

        if (visualizacao.Nome is not null && visualizacao.Nome.EnumerateRunes().Count() > 120)
            throw new DominioException("O nome pode ter no máximo 120 caracteres.");

        using var con = _factory.Criar();
        return con.ExecuteScalar<int>(
            """
            INSERT INTO HistoricoVisualizacao (UsuarioId, Tipo, Fonte, IdExterno, Nome)
            VALUES (@UsuarioId, @Tipo, @Fonte, @IdExterno, @Nome);
            SELECT LAST_INSERT_ID();
            """,
            new { visualizacao.UsuarioId, visualizacao.Tipo, visualizacao.Fonte, visualizacao.IdExterno, visualizacao.Nome });
    }

    public IEnumerable<HistoricoVisualizacao> ListarRecentesPorUsuario(int usuarioId, int limite)
    {
        ValidarUsuario(usuarioId);

        if (limite is < 1 or > LimiteMaximo)
            throw new DominioException($"O limite do histórico deve ser de 1 a {LimiteMaximo} registros.");

        using var con = _factory.Criar();
        return con.Query<HistoricoVisualizacao>(
            """
            SELECT Id, UsuarioId, Tipo, Fonte, IdExterno, Nome, VisualizadoEm
            FROM HistoricoVisualizacao
            WHERE UsuarioId = @usuarioId
            ORDER BY VisualizadoEm DESC, Id DESC
            LIMIT @limite
            """,
            new { usuarioId, limite }).ToList();
    }

    private static void ValidarUsuario(int usuarioId)
    {
        if (usuarioId <= 0)
            throw new DominioException("O ID do usuário deve ser positivo.");
    }
}
