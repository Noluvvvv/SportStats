using SportStats.Models;

namespace SportStats.Repositories.Interfaces;

public interface IPartidaRepository : IRepository<Partida>
{
    IEnumerable<Partida> ObterPorCampeonato(int campeonatoId);
}