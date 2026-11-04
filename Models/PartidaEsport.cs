using SportStats.Exceptions;
using SportStats.Models.Enums;

namespace SportStats.Models;

public class PartidaEsport : Partida
{
    private readonly int _melhorDe;

    public PartidaEsport(int campeonatoId, int timeCasaId, int timeForaId, DateTime dataHora, int melhorDe,
                         int id = 0, int placarCasa = 0, int placarFora = 0, bool encerrada = false)
        : base(campeonatoId, timeCasaId, timeForaId, dataHora, id, placarCasa, placarFora, encerrada)
    {
        if (melhorDe is not (1 or 3 or 5))
            throw new PlacarInvalidoException("O formato deve ser melhor de 1, 3 ou 5 mapas.");
        _melhorDe = melhorDe;
    }

    public override TipoPartida Tipo => TipoPartida.Esport;
    public override int MelhorDe => _melhorDe;

    protected override void ValidarPlacarFinal()
    {
        var necessarios = _melhorDe / 2 + 1;
        var maior = Math.Max(PlacarCasa, PlacarFora);
        var menor = Math.Min(PlacarCasa, PlacarFora);
        if (maior != necessarios || menor >= necessarios)
            throw new PlacarInvalidoException($"Em melhor de {_melhorDe}, o vencedor precisa de {necessarios} mapa(s).");
    }

    public override int CalcularPontos(bool mandante)
    {
        if (!Encerrada) return 0;
        var (meu, adv) = Placares(mandante);
        return meu > adv ? 3 : 0;
    }
}