namespace SportStats.Exceptions;

public class PlacarInvalidoException : DominioException
{
    public PlacarInvalidoException(string mensagem) : base(mensagem) { }
}