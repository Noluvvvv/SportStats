using Microsoft.AspNetCore.Mvc;
using SportStats.Integracao;
using SportStats.Integracao.Modelos;
using SportStats.Models.ViewModels;

namespace SportStats.Controllers;

public class HomeController : Controller
{
    private readonly CachePartidasExternas _cache;

    public HomeController(CachePartidasExternas cache)
    {
        _cache = cache;
    }

    public IActionResult Index(
        string? grupo,
        string? modalidade,
        string? campeonato)
    {
        var info = CatalogoModalidades.PorSlug(modalidade);

        var grupoAtivo = info?.Grupo ??
            (grupo?.Equals("esports", StringComparison.OrdinalIgnoreCase) == true
                ? "esports"
                : "esportes");

        var modalidades = CatalogoModalidades.DoGrupo(grupoAtivo).ToList();

        var partidas = _cache.AoVivo(grupoAtivo)
            .Concat(_cache.Agenda(grupoAtivo))
            .Where(p => info == null || p.ModalidadeSlug == info.Slug)
            .ToList();

        var competicoes = partidas
            .GroupBy(p => new
            {
                p.Fonte,
                Id = string.IsNullOrWhiteSpace(p.CampeonatoIdExterno)
                    ? p.Campeonato
                    : p.CampeonatoIdExterno,
                p.Campeonato,
                p.ModalidadeSlug
            })
            .Select(g => new CompeticaoExternaItem
            {
                Chave = $"{g.Key.Fonte}:{g.Key.Id}",
                Nome = g.Key.Campeonato,
                Fonte = g.Key.Fonte,
                ModalidadeSlug = g.Key.ModalidadeSlug,
                ModalidadeNome = CatalogoModalidades.PorSlug(g.Key.ModalidadeSlug)?.Nome
                    ?? g.Key.ModalidadeSlug
            })
            .OrderBy(c => c.Nome)
            .ToList();

        if (!string.IsNullOrWhiteSpace(campeonato))
        {
            partidas = partidas
                .Where(p =>
                {
                    var id = string.IsNullOrWhiteSpace(p.CampeonatoIdExterno)
                        ? p.Campeonato
                        : p.CampeonatoIdExterno;

                    return $"{p.Fonte}:{id}" == campeonato;
                })
                .ToList();
        }

        var vm = new HomeViewModel
        {
            Grupo = grupoAtivo,
            ModalidadeSlug = info?.Slug,
            CampeonatoId = campeonato,
            Modalidades = modalidades,
            Competicoes = competicoes,

            AoVivo = partidas
                .Where(p => p.Status is StatusPartidaExterna.AoVivo
                    or StatusPartidaExterna.Intervalo)
                .OrderBy(p => p.IniciaEmUtc)
                .ToList(),

            Proximas = partidas
                .Where(p => p.Status == StatusPartidaExterna.Agendada)
                .OrderBy(p => p.IniciaEmUtc)
                .Take(15)
                .ToList(),

            Recentes = partidas
                .Where(p => p.Status == StatusPartidaExterna.Encerrada)
                .OrderByDescending(p => p.IniciaEmUtc)
                .Take(15)
                .ToList(),

            AtualizacaoEspn = _cache.UltimaAtualizacao("ESPN"),
            AtualizacaoPandaScore = _cache.UltimaAtualizacao("PandaScore"),

            TituloClassificacao = null,
            Classificacao = new List<LinhaClassificacao>()
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult AoVivo(string? grupo, string? modalidade)
    {
        var info = CatalogoModalidades.PorSlug(modalidade);

        var grupoAtivo = info?.Grupo ??
            (grupo?.Equals("esports", StringComparison.OrdinalIgnoreCase) == true
                ? "esports"
                : "esportes");

        var placares = _cache.AoVivo(grupoAtivo)
            .Where(p => info == null || p.ModalidadeSlug == info.Slug)
            .Select(p => new
            {
                chave = p.Chave,
                placarCasa = p.PlacarCasa,
                placarFora = p.PlacarFora,
                status = p.Status.ToString(),
                detalhe = p.Detalhe,
                minuto = p.Minuto
            })
            .ToList();

        return Json(placares);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
