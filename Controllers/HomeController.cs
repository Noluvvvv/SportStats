using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SportStats.Models;
using SportStats.Models.ViewModels;
using SportStats.Services;

namespace SportStats.Controllers;

public class HomeController : Controller
{
    private readonly RankingService _rankingService;

    public HomeController(RankingService rankingService)
    {
        _rankingService = rankingService;
    }

    public IActionResult Index()
    {
        var model = new HomeViewModel();

        _rankingService.CarregarRankings(model);

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
