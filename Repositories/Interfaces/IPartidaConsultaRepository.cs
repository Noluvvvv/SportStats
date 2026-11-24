using SportStats.Models.ViewModels;

namespace SportStats.Repositories.Interfaces;

// Consultas de leitura para as telas (JOINs). Escrita continua em IPartidaRepository.
public interface IPartidaConsultaRepository
{
    IEnumerable<PartidaResumo> Listar(IEnumerable<string> modalidades, int? campeonatoId);
    IEnumerable<CompeticaoItem> ListarCompeticoes(IEnumerable<string> modalidades);
}