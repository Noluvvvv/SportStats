
using SportStats.Models;
using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;

namespace SportStats.Services;

public class RankingService
{
    private readonly IRepository<Atleta> _atletaRepository;
    private readonly IRepository<EstatisticaFutebol> _futebolRepository;
    private readonly IRepository<EstatisticaBasquete> _basqueteRepository;
    private readonly IRepository<EstatisticaValorant> _valorantRepository;
    private readonly IRepository<EstatisticaCsGo> _csGoRepository;

    public RankingService(
        IRepository<Atleta> atletaRepository,
        IRepository<EstatisticaFutebol> futebolRepository,
        IRepository<EstatisticaBasquete> basqueteRepository,
        IRepository<EstatisticaValorant> valorantRepository,
        IRepository<EstatisticaCsGo> csGoRepository)
    {
        _atletaRepository = atletaRepository;
        _futebolRepository = futebolRepository;
        _basqueteRepository = basqueteRepository;
        _valorantRepository = valorantRepository;
        _csGoRepository = csGoRepository;
    }

    public void CarregarRankings(HomeViewModel model)
    {
        var atletas = _atletaRepository.ObterTodos()
            .ToDictionary(a => a.Id);

        List<EstatisticaFutebol> futebol =
            _futebolRepository.ObterTodos().ToList();

        List<EstatisticaBasquete> basquete =
            _basqueteRepository.ObterTodos().ToList();

        List<EstatisticaValorant> valorant =
            _valorantRepository.ObterTodos().ToList();

        List<EstatisticaCsGo> csGo =
            _csGoRepository.ObterTodos().ToList();

        model.RankingFutebol = MontarRanking(futebol, atletas);
        model.RankingBasquete = MontarRanking(basquete, atletas);
        model.RankingValorant = MontarRanking(valorant, atletas);
        model.RankingCsGo = MontarRanking(csGo, atletas);
    }

    private static List<LinhaRanking> MontarRanking<T>(
        List<T> estatisticas,
        Dictionary<int, Atleta> atletas)
        where T : Estatistica
    {
        var ranking = new List<LinhaRanking>();

        foreach (var estatistica in estatisticas)
        {
            if (atletas.TryGetValue(estatistica.AtletaId, out var atleta))
            {
                ranking.Add(new LinhaRanking
                {
                    Nome = atleta.Nome,
                    Nickname = atleta.Nickname,
                    Rating = estatistica.CalcularRating()
                });
            }
        }

        ranking = ranking
            .OrderByDescending(item => item.Rating)
            .ThenBy(item => item.Nome)
            .ToList();

        for (int i = 0; i < ranking.Count; i++)
        {
            ranking[i].Posicao = i + 1;
        }

        return ranking;
    }
}
