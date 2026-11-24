using SportStats.Models;
using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;

namespace SportStats.Services;

public class ClassificacaoService
{
    private readonly IPartidaRepository _partidas;
    private readonly IRepository<Time> _times;

    public ClassificacaoService(IPartidaRepository partidas, IRepository<Time> times)
    {
        _partidas = partidas;
        _times = times;
    }

    // Os pontos vem de Partida.CalcularPontos(): cada modalidade tem sua regra (polimorfismo).
    // BACKEND: incluir times sem jogos (CampeonatoTime), desempate por confronto direto e colunas por modalidade.
    public List<LinhaClassificacao> Calcular(int campeonatoId)
    {
        var times = _times.ObterTodos().ToDictionary(t => t.Id);
        var linhas = new Dictionary<int, LinhaClassificacao>();

        LinhaClassificacao ObterOuCriar(int timeId)
        {
            if (!linhas.TryGetValue(timeId, out var linha))
            {
                var time = times[timeId];
                linha = new LinhaClassificacao { Nome = time.Nome, Sigla = time.Sigla };
                linhas[timeId] = linha;
            }
            return linha;
        }

        foreach (var p in _partidas.ObterPorCampeonato(campeonatoId).Where(p => p.Encerrada))
        {
            var casa = ObterOuCriar(p.TimeCasaId);
            var fora = ObterOuCriar(p.TimeForaId);

            casa.Jogos++;
            fora.Jogos++;
            casa.Pontos += p.CalcularPontos(true);
            fora.Pontos += p.CalcularPontos(false);
            casa.Saldo += p.PlacarCasa - p.PlacarFora;
            fora.Saldo += p.PlacarFora - p.PlacarCasa;

            var vencedor = p.DefinirVencedorId();
            if (vencedor == p.TimeCasaId) casa.Vitorias++;
            else if (vencedor == p.TimeForaId) fora.Vitorias++;
        }

        return linhas.Values
            .OrderByDescending(l => l.Pontos)
            .ThenByDescending(l => l.Saldo)
            .ThenBy(l => l.Nome)
            .ToList();
    }
}