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
            throw new DominioException("Atleta inv├ílido.");

        if (timeId <= 0)
            throw new DominioException("Time inv├ílido.");

        if (dataInicio == default)
            throw new DominioException("A data de in├¡cio ├® obrigat├│ria.");

        AtletaId = atletaId;
        TimeId = timeId;
        DataInicio = dataInicio;
    }

    public void Encerrar(DateTime dataFim)
    {
        if (DataFim.HasValue)
            throw new DominioException("Esta transfer├¬ncia j├í foi encerrada.");

        if (dataFim == default)
            throw new DominioException("A data de encerramento ├® obrigat├│ria.");

        if (dataFim.Date < DataInicio.Date)
            throw new DominioException("A data de encerramento n├úo pode ser anterior ├á data de in├¡cio.");

        DataFim = dataFim;
    }

    internal void DefinirId(int id)
    {
        if (id <= 0)
            throw new DominioException("O ID da transfer├¬ncia ├® inv├ílido.");

        if (Id > 0)
            throw new DominioException("A transfer├¬ncia j├í possui um ID.");

        Id = id;
    }

    internal Transferencia(
        int id,
        int atletaId,
        int timeId,
        DateTime dataInicio,
        DateTime? dataFim)
    {
        if (id <= 0)
            throw new DominioException("O ID da transfer├¬ncia ├® inv├ílido.");

        if (atletaId <= 0)
            throw new DominioException("Atleta inv├ílido.");

        if (timeId <= 0)
            throw new DominioException("Time inv├ílido.");

        if (dataInicio == default)
            throw new DominioException("A data de in├¡cio ├® obrigat├│ria.");

        if (dataFim.HasValue && dataFim.Value.Date < dataInicio.Date)
            throw new DominioException("A data de encerramento n├úo pode ser anterior ├á data de in├¡cio.");

        Id = id;
        AtletaId = atletaId;
        TimeId = timeId;
        DataInicio = dataInicio;
        DataFim = dataFim;
    }
}


