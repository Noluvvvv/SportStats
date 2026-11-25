using SportStats.Exceptions;

namespace SportStats.Models;

public class EstatisticaBasquete : Estatistica
{
    public int Pontos { get; private set; }
    public int Assistencias { get; private set; }
    public double MediaPontos { get; private set; }
    public int Faltas { get; private set; }
    public int Rebotes { get; private set; }

    public EstatisticaBasquete(
        int atletaId,
        int pontos,
        int assistencias,
        double mediaPontos,
        int faltas,
        int rebotes,
        int id = 0) : base(atletaId, id)
    {
        if (pontos < 0 || assistencias < 0 || mediaPontos < 0 ||
            faltas < 0 || rebotes < 0)
        {
            throw new DominioException(
                "As estat├¡sticas do basquete n├úo podem ser negativas.");
        }

        Pontos = pontos;
        Assistencias = assistencias;
        MediaPontos = mediaPontos;
        Faltas = faltas;
        Rebotes = rebotes;
    }

    public override double CalcularRating()
    {
        return (Pontos * 0.5) +
               (Assistencias * 1.5) +
               (MediaPontos * 2.0) +
               Rebotes -
               (Faltas * 0.5);
    }
}
