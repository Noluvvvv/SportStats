using SportStats.Models;

namespace SportStats.Repositories.Interfaces;

public interface ITransferenciaRepository
{
    void SalvarHistorico(Atleta atleta);
}