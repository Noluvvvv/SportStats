using SportStats.Models;

namespace SportStats.Repositories.Interfaces;

public interface IFavoritoRepository
{
    IEnumerable<Favorito> ListarPorUsuario(int usuarioId);
    bool Existe(int usuarioId, string tipo, string fonte, string idExterno);

    void Inserir(Favorito favorito);
    void Remover(int usuarioId, string tipo, string fonte, string idExterno);
}
