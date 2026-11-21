using SportStats.Exceptions;

namespace SportStats.Models;

public class Transferencia
{
    public int Id { get; private set; }
    public int AtletaId { get; private set; }
    public int TimeId { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }

    private Transferencia() { }

    public Transferencia(int atletaId, int timeId, DateTime dataInicio)
    {
        if (atletaId <= 0)
            throw new DominioException("Atleta inválido.");

        if (timeId <= 0)
            throw new DominioException("Time inválido.");

        if (dataInicio == default)
            throw new DominioException("A data de início é obrigatória.");

        AtletaId = atletaId;
        TimeId = timeId;
        DataInicio = dataInicio;
    }

    public void Encerrar(DateTime dataFim)
    {
        if (DataFim.HasValue)
            throw new DominioException("Esta transferência já foi encerrada.");

        if (dataFim == default)
            throw new DominioException("A data de encerramento é obrigatória.");

        if (dataFim.Date < DataInicio.Date)
            throw new DominioException("A data de encerramento não pode ser anterior à data de início.");

        DataFim = dataFim;
    }

    internal Transferencia(
        int id,
        int atletaId,
        int timeId,
        DateTime dataInicio,
        DateTime? dataFim)
    {
        if (id <= 0)
            throw new DominioException("O ID da transferência é inválido.");

        if (atletaId <= 0)
            throw new DominioException("Atleta inválido.");

        if (timeId <= 0)
            throw new DominioException("Time inválido.");

        if (dataInicio == default)
            throw new DominioException("A data de início é obrigatória.");

        if (dataFim.HasValue && dataFim.Value.Date < dataInicio.Date)
            throw new DominioException("A data de encerramento não pode ser anterior à data de início.");

        Id = id;
        AtletaId = atletaId;
        TimeId = timeId;
        DataInicio = dataInicio;
        DataFim = dataFim;
    }
}


