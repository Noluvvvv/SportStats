using Microsoft.AspNetCore.Mvc;
using SportStats.Integracao;

namespace SportStats.Controllers;

// Endpoints JSON para o front. Os dados vem das APIs (ESPN e PandaScore) via cache em memoria.
// BACKEND: quando o Hub SignalR existir, o front deixa de consultar estes endpoints a cada ciclo.
[ApiController]
[Route("api/partidas")]
public class IntegracaoController : ControllerBase
{
    private readonly CachePartidasExternas _cache;
    private readonly RoteadorDeProvedores _roteador;

    public IntegracaoController(CachePartidasExternas cache, RoteadorDeProvedores roteador)
    {
        _cache = cache;
        _roteador = roteador;
    }

    // grupo: esportes | esports | (vazio = todos)
    [HttpGet("ao-vivo")]
    public IActionResult AoVivo([FromQuery] string? grupo) => Ok(_cache.AoVivo(grupo));

    [HttpGet("agenda")]
    public IActionResult Agenda([FromQuery] string? grupo) => Ok(_cache.Agenda(grupo));

    [HttpGet("fontes")]
    public IActionResult Fontes() => Ok(_roteador.Todos().Select(p => new
    {
        p.Nome,
        p.Habilitado,
        UltimaAtualizacaoUtc = _cache.UltimaAtualizacao(p.Nome)
    }));

    // Ultimo frame bruto recebido pelo WebSocket da PandaScore (so se TempoRealWebSocket = true).
    [HttpGet("{fonte}/{id}/frame")]
    public IActionResult Frame(string fonte, string id)
    {
        var json = _cache.Frame($"{fonte}:{id}");
        return json is null ? NotFound() : Content(json, "application/json");
    }
}