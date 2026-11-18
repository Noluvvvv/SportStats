using Microsoft.AspNetCore.Mvc;
using SportStats.Integracao;
using SportStats.Integracao.Services;

namespace SportStats.Controllers;

// Endpoints JSON de diagnostico e consumo. Os dados vem das APIs via cache em memoria.
[ApiController]
[Route("api/partidas")]
public class IntegracaoController : ControllerBase
{
    private readonly CachePartidasExternas _cache;
    private readonly RoteadorDeProvedores _roteador;
    private readonly ProviderStatusService _status;

    public IntegracaoController(CachePartidasExternas cache, RoteadorDeProvedores roteador, ProviderStatusService status)
    {
        _cache = cache; _roteador = roteador; _status = status;
    }

    // grupo: esportes | esports | (vazio = todos)
    [HttpGet("ao-vivo")]
    public IActionResult AoVivo([FromQuery] string? grupo) => Ok(_cache.AoVivo(grupo));

    [HttpGet("agenda")]
    public IActionResult Agenda([FromQuery] string? grupo) => Ok(_cache.Agenda(grupo));

    [HttpGet("fontes")]
    public IActionResult Fontes() => Ok(new
    {
        provedores = _roteador.Todos().Select(p => new
        {
            p.Nome,
            p.Habilitado,
            UltimaAtualizacaoUtc = _cache.UltimaAtualizacao(p.Nome)
        }),
        pandaScore = new
        {
            disponivel = _status.PandaScoreDisponivel,
            bloqueadoAteUtc = _status.BloqueadoAte,
            requisicoesRestantes = _status.RequisicoesRestantes
        }
    });

    [HttpGet("{fonte}/{id}/frame")]
    public IActionResult Frame(string fonte, string id)
    {
        var json = _cache.Frame($"{fonte}:{id}");
        return json is null ? NotFound() : Content(json, "application/json");
    }
}