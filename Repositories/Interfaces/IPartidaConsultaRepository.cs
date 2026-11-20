using SportStats.Models.ViewModels;

namespace SportStats.Repositories.Interfaces;

// Repositório só de LEITURA para as telas: traz dados já "juntados" (nomes dos times,
// do campeonato...) sem precisar montar a entidade de domínio.
public interface IPartidaConsultaRepository
{
    IEnumerable<PartidaResumo> ListarResumos();
    PartidaResumo? ObterResumoPorId(int id);
    IEnumerable<CompeticaoItem> ListarCompeticoes();
    RetrospectoConfronto ObterRetrospecto(int partidaId, int timeCasaId, int timeForaId);
}
