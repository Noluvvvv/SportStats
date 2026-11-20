using SportStats.Integracao.Modelos;
using SportStats.Integracao.PandaScore;
using SportStats.Integracao.Providers;
using SportStats.Models.Esports;

namespace SportStats.Integracao.Services;

public class EsportsService
{
    private static readonly string[] Jogos = { "lol", "valorant", "cs" };

    private readonly ProvedorPandaScore _pandaScore;
    private readonly ProviderStatusService _status;
    private readonly CachePartidasExternas _cache;
    private readonly EsportsResultsProvider _widget;

    public EsportsService(ProvedorPandaScore pandaScore, ProviderStatusService status,
                          CachePartidasExternas cache, EsportsResultsProvider widget)
    {
        _pandaScore = pandaScore; _status = status; _cache = cache; _widget = widget;
    }

    public EsportsPageViewModel Obter(string? jogo)
    {
        var slug = Normalizar(jogo);

        if (_pandaScore.Habilitado)
        {
            bool Filtro(PartidaExterna p) => slug == "all" || p.ModalidadeSlug == slug;
            var agenda = _cache.Agenda("esports").Where(Filtro).ToList();

            return new EsportsPageViewModel
            {
                Game = slug,
                DisplayMode = "api",
                Provider = "PandaScore",
                AtualizadoEmUtc = _cache.UltimaAtualizacao("PandaScore"),
                AoVivo = _cache.AoVivo("esports").Where(Filtro).ToList(),
                Proximas = agenda.Where(p => p.Status == StatusPartidaExterna.Agendada).Take(10).ToList(),
                Recentes = agenda.Where(p => p.Status == StatusPartidaExterna.Encerrada)
                                 .OrderByDescending(p => p.IniciaEmUtc).Take(10).ToList()
            };
        }

        // Sem dados estruturados: widget oficial (se configurado) ou aviso. Nunca placar velho como se fosse ao vivo.
        var url = _widget.ObterUrlDoWidget();
        return new EsportsPageViewModel
        {
            Game = slug,
            DisplayMode = url is null ? "indisponivel" : "widget",
            Provider = url is null ? "" : "EsportsResults",
            WidgetUrl = url,
            Motivo = MotivoDaIndisponibilidade()
        };
    }

    private string MotivoDaIndisponibilidade()
    {
        if (_status.BloqueadoAte is { } ate)
            return $"Limite de consultas da fonte principal atingido. Nova tentativa automática às {ate.ToLocalTime():HH:mm}.";
        return "A fonte principal de dados está indisponível no momento.";
    }

    private static string Normalizar(string? jogo)
    {
        var j = (jogo ?? "all").ToLowerInvariant();
        if (j is "csgo" or "cs2") j = "cs";
        return Jogos.Contains(j) ? j : "all";
    }
}