using SportStats.Models;

namespace SportStats.Repositories.Interfaces;

public interface IHistoricoVisualizacaoRepository
{
    int Registrar(HistoricoVisualizacao visualizacao);
    IEnumerable<HistoricoVisualizacao> ListarRecentesPorUsuario(int usuarioId, int limite);
}
