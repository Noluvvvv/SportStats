using Microsoft.AspNetCore.Mvc;
using SportStats.Integracao.Services;

namespace SportStats.Controllers;

public class EsportsController : Controller
{
    private readonly EsportsService _service;

    public EsportsController(EsportsService service) => _service = service;

    // /Esports?game=all|lol|valorant|cs
    public IActionResult Index(string game = "all") => View(_service.Obter(game));
}