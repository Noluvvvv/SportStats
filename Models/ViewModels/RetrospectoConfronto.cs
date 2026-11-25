namespace SportStats.Models.ViewModels;

public class RetrospectoConfronto
{
    public int Jogos { get; set; }
    public int VitoriasCasa { get; set; }
    public int VitoriasFora { get; set; }
    public int Empates { get; set; }

    public List<PartidaResumo> Ultimos { get; set; } = new();
}
