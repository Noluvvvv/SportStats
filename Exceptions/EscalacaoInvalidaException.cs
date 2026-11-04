namespace SportStats.Exceptions;

public class EscalacaoInvalidaException : DominioException
{
    public EscalacaoInvalidaException(string mensagem) : base(mensagem) { }
}