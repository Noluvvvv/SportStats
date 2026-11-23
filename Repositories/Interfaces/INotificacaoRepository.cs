using SportStats.Models;

namespace SportStats.Repositories.Interfaces;

public interface INotificacaoRepository
{
    int Criar(Notificacao notificacao);
    IEnumerable<Notificacao> ListarPorUsuario(int usuarioId, int limite);
    IEnumerable<Notificacao> ListarNaoLidasPorUsuario(int usuarioId, int limite);
    void MarcarComoLida(int usuarioId, int id);
}
