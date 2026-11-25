using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportStats.Integracao.Modelos;
using SportStats.Integracao.Opcoes;

namespace SportStats.Integracao.Espn;

// API publica e NAO oficial da ESPN (sem token, sem documentacao). Pode mudar sem aviso.
// Sem WebSocket: o tempo real e feito por polling do scoreboard.
public sealed class ProvedorEspn : IProvedorDados
{
    private static readonly TimeSpan ValidadeCache = TimeSpan.FromSeconds(20);

    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly IOptions<IntegracaoOptions> _opcoes;
    private readonly ILogger<ProvedorEspn> _log;

    public ProvedorEspn(IHttpClientFactory http, IMemoryCache cache, IOptions<IntegracaoOptions> opcoes, ILogger<ProvedorEspn> log)
    {
        _http = http; _cache = cache; _opcoes = opcoes; _log = log;
    }

    private EspnOptions Opt => _opcoes.Value.Espn;

    public string Nome => "ESPN";
    public bool Habilitado => Opt.Ligas.Count > 0;
    public bool AgendaBarata => true;
    public bool Atende(string modalidadeSlug) => Opt.Ligas.Any(l => l.Modalidade == modalidadeSlug);

    public async Task<IReadOnlyList<PartidaExterna>> ObterAoVivoAsync(CancellationToken ct = default)
    {
        var hoje = await ObterAgendaAsync(DateOnly.FromDateTime(DateTime.Now), ct);
        return hoje.Where(p => p.Status is StatusPartidaExterna.AoVivo or StatusPartidaExterna.Intervalo).ToList();
    }

    public async Task<IReadOnlyList<PartidaExterna>> ObterAgendaAsync(DateOnly data, CancellationToken ct = default)
    {
        // Sem o parametro dates, a ESPN devolve o "dia atual" dela; assim o ao vivo e a agenda dividem a mesma chamada.
        DateOnly? parametro = data == DateOnly.FromDateTime(DateTime.Now) ? null : data;
        var listas = await Task.WhenAll(Opt.Ligas.Select(l => ObterLigaAsync(l, parametro, ct)));
        return listas.SelectMany(x => x).OrderBy(p => p.IniciaEmUtc).ToList();
    }

    private async Task<List<PartidaExterna>> ObterLigaAsync(EspnLiga liga, DateOnly? data, CancellationToken ct)
    {
        var rota = $"{liga.Esporte}/{liga.Liga}/scoreboard" + (data is null ? "" : $"?dates={data.Value:yyyyMMdd}");
        try
        {
            var json = await _cache.GetOrCreateAsync($"espn:{rota}", async entrada =>
            {
                entrada.AbsoluteExpirationRelativeToNow = ValidadeCache;
                return await _http.CreateClient("espn").GetStringAsync(rota, ct);
            });
            if (json is null)
                throw new HttpRequestException("A ESPN nao retornou uma resposta valida.");

            return Mapear(json, liga);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            _log.LogWarning("ESPN {Liga}: {Mensagem}", liga.Liga, ex.Message);
            throw;
        }
    }

    private static List<PartidaExterna> Mapear(string json, EspnLiga liga)
    {
        using var doc = JsonDocument.Parse(json);
        var raiz = doc.RootElement;
        var resultado = new List<PartidaExterna>();
        if (!raiz.TryGetProperty("events", out var eventos) || eventos.ValueKind != JsonValueKind.Array) return resultado;

        var nomeLiga = liga.Nome ?? NomeDaLiga(raiz) ?? liga.Liga;

        foreach (var ev in eventos.EnumerateArray())
        {
            if (!ev.TryGetProperty("competitions", out var competicoes) ||
                competicoes.ValueKind != JsonValueKind.Array || competicoes.GetArrayLength() == 0) continue;
            if (!competicoes[0].TryGetProperty("competitors", out var competidores) ||
                competidores.ValueKind != JsonValueKind.Array) continue;

            JsonElement? casa = null, fora = null;
            foreach (var c in competidores.EnumerateArray())
            {
                var lado = JsonUtil.Texto(c, "homeAway");
                if (lado == "home") casa = c; else if (lado == "away") fora = c;
            }
            if (casa is null || fora is null) continue;

            var status = MapearStatus(JsonUtil.Texto(ev, "status", "type", "state"), JsonUtil.Texto(ev, "status", "type", "name"));
            int? minuto = null;
            if (liga.Modalidade == "futebol" && status == StatusPartidaExterna.AoVivo)
                minuto = ExtrairMinuto(JsonUtil.Texto(ev, "status", "displayClock"));

            var inicio = JsonUtil.Data(ev, "date");

            // Evento sem data valida nao deve receber DateTime.UtcNow artificial.
            if (inicio is null) continue;

            resultado.Add(new PartidaExterna
            {
                Fonte = "ESPN",
                IdExterno = JsonUtil.Texto(ev, "id") ?? "",
                ModalidadeSlug = liga.Modalidade,
                Campeonato = nomeLiga,
                CampeonatoIdExterno = liga.Liga,
                Casa = MapearTime(casa.Value),
                Fora = MapearTime(fora.Value),
                Status = status,
                Detalhe = JsonUtil.Texto(ev, "status", "type", "shortDetail"),
                Minuto = minuto,
                PlacarCasa = JsonUtil.Inteiro(casa.Value, "score"),
                PlacarFora = JsonUtil.Inteiro(fora.Value, "score"),
                IniciaEmUtc = inicio.Value
            });
        }
        return resultado;
    }

    private static string? NomeDaLiga(JsonElement raiz) =>
        raiz.TryGetProperty("leagues", out var ls) && ls.ValueKind == JsonValueKind.Array && ls.GetArrayLength() > 0
            ? JsonUtil.Texto(ls[0], "name")
            : null;

    private static TimeExterno MapearTime(JsonElement competidor) => new()
    {
        IdExterno = JsonUtil.Texto(competidor, "team", "id") ?? "",
        Nome = JsonUtil.Texto(competidor, "team", "displayName") ?? JsonUtil.Texto(competidor, "team", "name") ?? "?",
        Sigla = JsonUtil.Texto(competidor, "team", "abbreviation") ?? "",
        EscudoUrl = JsonUtil.Texto(competidor, "team", "logo")
    };

    private static StatusPartidaExterna MapearStatus(string? estado, string? nome)
    {
        var n = (nome ?? "").ToUpperInvariant();
        if (n.Contains("POSTPONED")) return StatusPartidaExterna.Adiada;
        if (n.Contains("CANCEL")) return StatusPartidaExterna.Cancelada;
        if (n.Contains("HALFTIME")) return StatusPartidaExterna.Intervalo;
        return estado switch
        {
            "in" => StatusPartidaExterna.AoVivo,
            "post" => StatusPartidaExterna.Encerrada,
            _ => StatusPartidaExterna.Agendada
        };
    }

    private static int? ExtrairMinuto(string? relogio)
    {
        var m = Regex.Match(relogio ?? "", @"\d+");
        return m.Success && int.TryParse(m.Value, out var n) ? n : null;
    }
}