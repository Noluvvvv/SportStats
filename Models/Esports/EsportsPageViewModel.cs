using SportStats.Integracao.Modelos;

namespace SportStats.Models.Esports;

public class EsportsPageViewModel
{
    public string Game { get; set; } = "all";            // all | lol | valorant | cs
    public string DisplayMode { get; set; } = "api";     // api | widget | indisponivel
    public string Provider { get; set; } = "";
    public string? Motivo { get; set; }
    public string? WidgetUrl { get; set; }
    public DateTime? AtualizadoEmUtc { get; set; }

    public IReadOnlyList<PartidaExterna> AoVivo { get; set; } = Array.Empty<PartidaExterna>();
    public IReadOnlyList<PartidaExterna> Proximas { get; set; } = Array.Empty<PartidaExterna>();
    public IReadOnlyList<PartidaExterna> Recentes { get; set; } = Array.Empty<PartidaExterna>();
}

public record EsportsBloco(string Titulo, IReadOnlyList<PartidaExterna> Partidas, string Vazio, bool Destaque = false);