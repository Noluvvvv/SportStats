using SportStats.Models;

namespace SportStats.Repositories.Interfaces;

public interface IModalidadeRepository
{
    IEnumerable<ModalidadeItem> ObterTodas();
}
