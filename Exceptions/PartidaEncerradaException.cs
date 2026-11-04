namespace SportStats.Exceptions;

public class PartidaEncerradaException : DominioException
{
    public PartidaEncerradaException()
        : base("Partida já encerrada: o placar não pode mais ser alterado.") { }
}