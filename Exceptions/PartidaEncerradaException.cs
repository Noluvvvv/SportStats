namespace SportStats.Exceptions;

public class PartidaEncerradaException : DominioException
{
    public PartidaEncerradaException()
        : base("A partida já foi encerrada. O placar, o minuto e o estado não podem mais ser alterados.") { }
}
