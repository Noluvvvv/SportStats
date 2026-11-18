namespace SportStats.Integracao.Config;

public class EsportsOptions
{
    // URL oficial de incorporacao (iframe). Vazia = sem fallback visual.
    public string EsportsResultsWidgetUrl { get; set; } = "";

    // Usado so quando a resposta 429 vier sem o cabecalho Retry-After.
    public int PandaScoreCooldownMinutes { get; set; } = 60;
}