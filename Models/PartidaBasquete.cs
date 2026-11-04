using SportStats.Exceptions;
using SportStats.Models.Enums;

namespace SportStats.Models;

public class PartidaBasquete : Partida
{
    public PartidaBasquete(int campeonatoId, int timeCasaId, int timeForaId, DateTime dataHora,
                           int id = 0, int placarCasa = 0, int placarFora = 0, bool encerrada = false)
        : base(campeonatoId, timeCasaId, timeForaId, dataHora, id, placarCasa, placarFora, encerrada) { }

    public override TipoPartida Tipo => TipoPartida.Basquete;

    protected override void ValidarPlacarFinal()
    {
        if (PlacarCasa == PlacarFora)
            throw new PlacarInvalidoException("Basquete não admite empate: registre a prorrogação.");
    }

    public override int CalcularPontos(bool mandante)
    {
        if (!Encerrada) return 0;
        var (meu, adv) = Placares(mandante);
        return meu > adv ? 2 : 1;
    }
}