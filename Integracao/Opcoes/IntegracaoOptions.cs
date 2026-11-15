namespace SportStats.Integracao.Opcoes;

public class IntegracaoOptions
{
    public EspnOptions Espn { get; set; } = new();
    public PandaScoreOptions PandaScore { get; set; } = new();

    public int IntervaloAoVivoSegundos { get; set; } = 30;
    public int IntervaloAgendaSegundos { get; set; } = 300;

    // Exige plano com Live API na PandaScore. Deixe false ate confirmar o plano.
    public bool TempoRealWebSocket { get; set; } = false;
    public int MaxPartidasWebSocket { get; set; } = 3;

    // Aplicado so quando nenhuma liga foi configurada (evita duplicar a lista ao usar appsettings).
    public void AplicarPadroes()
    {
        if (Espn.Ligas.Count > 0) return;
        Espn.Ligas.AddRange(new[]
        {
            new EspnLiga { Esporte = "soccer", Liga = "eng.1", Modalidade = "futebol", Nome = "Premier League" },
            new EspnLiga { Esporte = "soccer", Liga = "uefa.champions", Modalidade = "futebol", Nome = "UEFA Champions League" },
            new EspnLiga { Esporte = "soccer", Liga = "bra.1", Modalidade = "futebol", Nome = "Brasileirão Série A" }, // conferir o slug
            new EspnLiga { Esporte = "basketball", Liga = "nba", Modalidade = "basquete", Nome = "NBA" },
            // Volei: adicionar quando confirmar o slug da ESPN. Sem ele, cadastrar partidas pelo CRUD.
        });
    }
}

public class EspnOptions
{
    public string BaseUrl { get; set; } = "https://site.api.espn.com/apis/site/v2/sports";
    public List<EspnLiga> Ligas { get; set; } = new();
}

public class EspnLiga
{
    public string Esporte { get; set; } = "";      // soccer, basketball...
    public string Liga { get; set; } = "";         // eng.1, nba...
    public string Modalidade { get; set; } = "";   // slug do SportStats
    public string? Nome { get; set; }
}

public class PandaScoreOptions
{
    public string BaseUrl { get; set; } = "https://api.pandascore.co";
    public string LiveBaseUrl { get; set; } = "wss://live.pandascore.co";

    // Nunca no repositorio: use user-secrets ou a variavel Integracao__PandaScore__Token.
    public string Token { get; set; } = "";
}