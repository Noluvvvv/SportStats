namespace SportStats.Models.ViewModels;

public class AtletaIndexViewModel
{
    public List<AtletaListaItemViewModel> Atletas { get; set; } = new();
}

public class AtletaListaItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string? Nickname { get; set; }
    public string Nacionalidade { get; set; } = "";
    public string ModalidadeNome { get; set; } = "";

    public List<TransferenciaHistoricoItemViewModel> Historico { get; set; } = new();
}

public class TransferenciaHistoricoItemViewModel
{
    public string NomeTime { get; set; } = "";
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}