using SportStats.Models.Enums;

namespace SportStats.Models;

public class PartidaFutebol : Partida
{
    public PartidaFutebol(int campeonatoId, int timeCasaId, int timeForaId, DateTime dataHora,
                          int id = 0, int placarCasa = 0, int placarFora = 0, bool encerrada = false)
        : base(campeonatoId, timeCasaId, timeForaId, dataHora, id, placarCasa, placarFora, encerrada) { }

    public override TipoPartida Tipo => TipoPartida.Futebol;

    protected override void ValidarPlacarFinal() { } // empate permitido

    public override int CalcularPontos(bool mandante)
    {
        if (!Encerrada) return 0;
        var (meu, adv) = Placares(mandante);
        return meu > adv ? 3 : meu == adv ? 1 : 0;
    }
}