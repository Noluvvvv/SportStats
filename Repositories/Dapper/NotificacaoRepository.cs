using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Repositories.Interfaces;
using global::Dapper;

namespace SportStats.Repositories.Dapper;

public class NotificacaoRepository : INotificacaoRepository
{
    private const int LimiteMaximo = 100;
    private readonly IDbConnectionFactory _factory;

    public NotificacaoRepository(IDbConnectionFactory factory) => _factory = factory;

    public int Criar(Notificacao notificacao)
    {
        ArgumentNullException.ThrowIfNull(notificacao);
        ValidarUsuario(notificacao.UsuarioId);

        if (string.IsNullOrWhiteSpace(notificacao.Tipo) || notificacao.Tipo.EnumerateRunes().Count() > 20)
            throw new DominioException("O tipo é obrigatório e pode ter no máximo 20 caracteres.");

        if (notificacao.Fonte is not null && notificacao.Fonte.EnumerateRunes().Count() > 20)
            throw new DominioException("A fonte pode ter no máximo 20 caracteres.");

        if (notificacao.PartidaIdExterno is not null && notificacao.PartidaIdExterno.EnumerateRunes().Count() > 60)
            throw new DominioException("O ID externo da partida pode ter no máximo 60 caracteres.");

        if (string.IsNullOrWhiteSpace(notificacao.Mensagem) || notificacao.Mensagem.EnumerateRunes().Count() > 200)
            throw new DominioException("A mensagem é obrigatória e pode ter no máximo 200 caracteres.");

        using var con = _factory.Criar();
        return con.ExecuteScalar<int>(
            """
            INSERT INTO Notificacao (UsuarioId, Tipo, Fonte, PartidaIdExterno, Mensagem)
            VALUES (@UsuarioId, @Tipo, @Fonte, @PartidaIdExterno, @Mensagem);
            SELECT LAST_INSERT_ID();
            """,
            new
            {
                notificacao.UsuarioId,
                notificacao.Tipo,
                notificacao.Fonte,
                notificacao.PartidaIdExterno,
                notificacao.Mensagem
            });
    }

    public IEnumerable<Notificacao> ListarPorUsuario(int usuarioId, int limite) =>
        Listar(usuarioId, limite, somenteNaoLidas: false);

    public IEnumerable<Notificacao> ListarNaoLidasPorUsuario(int usuarioId, int limite) =>
        Listar(usuarioId, limite, somenteNaoLidas: true);

    public void MarcarComoLida(int usuarioId, int id)
    {
        ValidarUsuario(usuarioId);

        if (id <= 0)
            throw new DominioException("O ID da notificação deve ser positivo.");

        using var con = _factory.Criar();
        con.Execute(
            """
            UPDATE Notificacao
            SET Lida = 1
            WHERE UsuarioId = @usuarioId AND Id = @id AND Lida = 0
            """,
            new { usuarioId, id });
    }

    private IEnumerable<Notificacao> Listar(int usuarioId, int limite, bool somenteNaoLidas)
    {
        ValidarUsuario(usuarioId);

        if (limite is < 1 or > LimiteMaximo)
            throw new DominioException($"O limite de notificações deve ser de 1 a {LimiteMaximo} registros.");

        using var con = _factory.Criar();
        return con.Query<Notificacao>(
            """
            SELECT Id, UsuarioId, Tipo, Fonte, PartidaIdExterno, Mensagem, Lida, CriadaEm
            FROM Notificacao
            WHERE UsuarioId = @usuarioId AND (@somenteNaoLidas = 0 OR Lida = 0)
            ORDER BY CriadaEm DESC, Id DESC
            LIMIT @limite
            """,
            new { usuarioId, limite, somenteNaoLidas }).ToList();
    }

    private static void ValidarUsuario(int usuarioId)
    {
        if (usuarioId <= 0)
            throw new DominioException("O ID do usuário deve ser positivo.");
    }
}
