using SportStats.Models.ViewModels;

namespace SportStats.Repositories.Interfaces;

// Repositorio de leitura para consultas utilizadas pelas telas.
public interface IPartidaConsultaRepository
{
    // Consultas da Home.
    IEnumerable<PartidaResumo> Listar(
        IEnumerable<string> modalidades,
        int? campeonatoId);

    IEnumerable<CompeticaoItem> ListarCompeticoes(
        IEnumerable<string> modalidades);

    // Consultas gerais.
    IEnumerable<PartidaResumo> ListarResumos();

    PartidaResumo? ObterResumoPorId(int id);

    IEnumerable<CompeticaoItem> ListarCompeticoes();

    // Historico de confrontos diretos.
    RetrospectoConfronto ObterRetrospecto(
        int partidaId,
        int timeCasaId,
        int timeForaId);
}
