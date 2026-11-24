using SportStats.Exceptions;

namespace SportStats.Models;

public class EstatisticaFutebol : Estatistica
{
    public int Gols { get; private set; }
    public int ChutesAGol { get; private set; }
    public int Escanteios { get; private set; }
    public int Cartoes { get; private set; }
    public int Finalizacoes { get; private set; }

    public EstatisticaFutebol(
        int atletaId,
        int gols,
        int chutesAGol,
        int escanteios,
        int cartoes,
        int finalizacoes,
        int id = 0) : base(atletaId, id)
    {
        if (gols < 0 || chutesAGol < 0 || escanteios < 0 ||
            cartoes < 0 || finalizacoes < 0)
        {
            throw new DominioException(
                "As estatísticas do futebol não podem ser negativas.");
        }

        Gols = gols;
        ChutesAGol = chutesAGol;
        Escanteios = escanteios;
        Cartoes = cartoes;
        Finalizacoes = finalizacoes;
    }

    public override double CalcularRating()
    {
        return (Gols * 5.0) +
               (ChutesAGol * 1.5) +
               (Escanteios * 0.2) +
               (Finalizacoes * 0.5) -
               Cartoes;
    }
}