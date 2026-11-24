using SportStats.Exceptions;

namespace SportStats.Models;

public class EstatisticaCsGo : Estatistica
{
    public int Abates { get; private set; }
    public int Assistencias { get; private set; }
    public int Headshots { get; private set; }

    public EstatisticaCsGo(
        int atletaId,
        int abates,
        int assistencias,
        int headshots,
        int id = 0) : base(atletaId, id)
    {
        if (abates < 0 || assistencias < 0 || headshots < 0)
        {
            throw new DominioException(
                "As estatísticas do CS:GO não podem ser negativas.");
        }

        Abates = abates;
        Assistencias = assistencias;
        Headshots = headshots;
    }

    public override double CalcularRating()
    {
        return (Abates * 1.2) +
               (Assistencias * 0.8) +
               (Headshots * 1.5);
    }
}