using SportStats.Exceptions;
using SportStats.Models.Enums;

namespace SportStats.Models;

public class PartidaVolei : Partida
{
    public PartidaVolei(int campeonatoId, int timeCasaId, int timeForaId, DateTime dataHora,
                        int id = 0, int placarCasa = 0, int placarFora = 0, bool encerrada = false)
        : base(campeonatoId, timeCasaId, timeForaId, dataHora, id, placarCasa, placarFora, encerrada) { }

    public override TipoPartida Tipo => TipoPartida.Volei;
    public override int MelhorDe => 5;

    protected override void ValidarPlacarFinal()
    {
        var maior = Math.Max(PlacarCasa, PlacarFora);
        var menor = Math.Min(PlacarCasa, PlacarFora);
        if (maior != 3 || menor > 2)
            throw new PlacarInvalidoException("No vôlei o vencedor precisa de 3 sets e o outro no máximo 2.");
    }

    public override int CalcularPontos(bool mandante)
    {
        if (!Encerrada) return 0;
        var (meu, adv) = Placares(mandante);
        if (meu > adv) return adv <= 1 ? 3 : 2;
        return meu == 2 ? 1 : 0;
    }
}