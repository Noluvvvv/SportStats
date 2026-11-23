using SportStats.Exceptions;
using SportStats.Models.Enums;

namespace SportStats.Models;

public abstract class Partida
{
    public int Id { get; private set; }
    public int CampeonatoId { get; private set; }
    public int TimeCasaId { get; private set; }
    public int TimeForaId { get; private set; }
    public DateTime DataHora { get; private set; }
    public int PlacarCasa { get; private set; }
    public int PlacarFora { get; private set; }
    public StatusPartida Status { get; private set; }
    public int? Minuto { get; }
    public bool Encerrada => Status == StatusPartida.Encerrada;

    public abstract TipoPartida Tipo { get; }
    public virtual int MelhorDe => 0;

    protected Partida(int campeonatoId, int timeCasaId, int timeForaId, DateTime dataHora,
                      int id, int placarCasa, int placarFora, StatusPartida status, int? minuto)
    {
        if (timeCasaId == timeForaId)
            throw new EscalacaoInvalidaException("Um time não pode jogar contra ele mesmo.");

        if (!Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(status));
        if (minuto is int minutoInformado)
            ArgumentOutOfRangeException.ThrowIfNegative(minutoInformado, nameof(minuto));

        Id = id;
        CampeonatoId = campeonatoId;
        TimeCasaId = timeCasaId;
        TimeForaId = timeForaId;
        DataHora = dataHora;
        PlacarCasa = placarCasa;
        PlacarFora = placarFora;
        Status = status;
        Minuto = minuto;
    }

    public void RegistrarPlacar(int casa, int fora)
    {
        if (Encerrada) throw new PartidaEncerradaException();
        if (casa < 0 || fora < 0)
            throw new PlacarInvalidoException("O placar não pode ser negativo.");

        PlacarCasa = casa;
        PlacarFora = fora;
    }

    public void Encerrar()
    {
        if (Encerrada) throw new PartidaEncerradaException();
        ValidarPlacarFinal();
        Status = StatusPartida.Encerrada;
    }

    public int? DefinirVencedorId() =>
        PlacarCasa > PlacarFora ? TimeCasaId :
        PlacarFora > PlacarCasa ? TimeForaId : null;

    protected abstract void ValidarPlacarFinal();
    public abstract int CalcularPontos(bool mandante);

    protected (int Meu, int Adv) Placares(bool mandante) =>
        mandante ? (PlacarCasa, PlacarFora) : (PlacarFora, PlacarCasa);
}