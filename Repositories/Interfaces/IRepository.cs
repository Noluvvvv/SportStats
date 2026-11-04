namespace SportStats.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    IEnumerable<T> ObterTodos();
    T? ObterPorId(int id);
    int Inserir(T entidade);
    void Atualizar(T entidade);
    void Remover(int id);
}