using SportStats.Exceptions;

namespace SportStats.Models;

public class EstatisticaValorant : Estatistica
{
    public int Abates { get; private set; }
    public int Assistencias { get; private set; }
    public string Agente { get; private set; }

    public EstatisticaValorant(
        int atletaId,
        int abates,
        int assistencias,
        string agente,
        int id = 0) : base(atletaId, id)
    {
        if (abates < 0 || assistencias < 0)
            throw new DominioException(
                "As estatísticas do Valorant não podem ser negativas.");

        if (string.IsNullOrWhiteSpace(agente))
            throw new DominioException("O agente deve ser informado.");

        Abates = abates;
        Assistencias = assistencias;
        Agente = agente;
    }

    public override double CalcularRating()
    {
        return (Abates * 1.5) + Assistencias;
    }
}