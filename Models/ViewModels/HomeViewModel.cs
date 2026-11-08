namespace SportStats.Models.ViewModels;

public class HomeViewModel
{
    public string Grupo { get; set; } = "esportes";
    public string? ModalidadeSlug { get; set; }
    public int? CampeonatoId { get; set; }

    public List<ModalidadeInfo> Modalidades { get; set; } = new();
    public List<CompeticaoItem> Competicoes { get; set; } = new();

    public List<PartidaResumo> AoVivo { get; set; } = new();
    public List<PartidaResumo> Proximas { get; set; } = new();
    public List<PartidaResumo> Recentes { get; set; } = new();

    public string? TituloClassificacao { get; set; }
    public List<LinhaClassificacao> Classificacao { get; set; } = new();
}

public record BlocoPartidas(string Titulo, IReadOnlyList<PartidaResumo> Partidas, string Vazio, bool Destaque = false);

public class LinhaClassificacao
{
    public string Nome { get; set; } = "";
    public string Sigla { get; set; } = "";
    public int Jogos { get; set; }
    public int Vitorias { get; set; }
    public int Saldo { get; set; }
    public int Pontos { get; set; }
}