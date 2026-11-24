using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SportStats.Models;
using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;
using SportStats.Services;

namespace SportStats.Controllers;

public class HomeController : Controller
{
    private readonly IPartidaConsultaRepository _consulta;
    private readonly ClassificacaoService _classificacao;

    public HomeController(IPartidaConsultaRepository consulta, ClassificacaoService classificacao)
    {
        _consulta = consulta;
        _classificacao = classificacao;
    }

    public IActionResult Index(string? grupo, string? modalidade, int? campeonato)
    {
        var (grupoAtivo, info, nomes) = Resolver(grupo, modalidade);

        var partidas = _consulta.Listar(nomes, campeonato).ToList();
        var competicoes = _consulta.ListarCompeticoes(nomes).ToList();

        var vm = new HomeViewModel
        {
            Grupo = grupoAtivo,
            ModalidadeSlug = info?.Slug,
            CampeonatoId = campeonato,
            Modalidades = CatalogoModalidades.DoGrupo(grupoAtivo).ToList(),
            Competicoes = competicoes,
            AoVivo = partidas.Where(p => p.AoVivo).OrderBy(p => p.DataHora).ToList(),
            Proximas = partidas.Where(p => p.Agendada).OrderBy(p => p.DataHora).Take(10).ToList(),
            Recentes = partidas.Where(p => p.Encerrada).OrderByDescending(p => p.DataHora).Take(15).ToList()
        };

        var idClassificacao = campeonato ?? competicoes.FirstOrDefault()?.Id;
        if (idClassificacao is int id)
        {
            vm.TituloClassificacao = competicoes.FirstOrDefault(c => c.Id == id)?.Nome;
            vm.Classificacao = _classificacao.Calcular(id);
        }

        return View(vm);
    }

    public IActionResult Privacy() => View();

    [HttpGet]
    public IActionResult AoVivo(string? grupo, string? modalidade)
    {
        var (_, _, nomes) = Resolver(grupo, modalidade);
        var placares = _consulta.Listar(nomes, null)
            .Where(p => p.AoVivo)
            .Select(p => new { p.Id, p.PlacarCasa, p.PlacarFora });
        return Json(placares);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    private static (string Grupo, ModalidadeInfo? Info, string[] Nomes) Resolver(string? grupo, string? modalidade)
    {
        var info = CatalogoModalidades.PorSlug(modalidade);
        var grupoAtivo = info?.Grupo ?? (grupo == "esports" ? "esports" : "esportes");
        var nomes = info is not null
            ? new[] { info.Nome }
            : CatalogoModalidades.DoGrupo(grupoAtivo).Select(m => m.Nome).ToArray();
        return (grupoAtivo, info, nomes);
    }
}