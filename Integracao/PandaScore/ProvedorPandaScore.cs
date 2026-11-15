using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportStats.Integracao.Modelos;
using SportStats.Integracao.Opcoes;

namespace SportStats.Integracao.PandaScore;

// REST: autenticacao por Bearer (o token nao aparece na URL nem nos logs).
// WebSocket: so aceita o token na URL (?token=); por isso a URL nunca e registrada em log.
public sealed class ProvedorPandaScore : IProvedorTempoReal
{
    private static readonly TimeSpan ValidadeCache = TimeSpan.FromSeconds(15);
    private const int MaxFalhasSeguidas = 5;

    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly IOptions<IntegracaoOptions> _opcoes;
    private readonly ILogger<ProvedorPandaScore> _log;

    public ProvedorPandaScore(IHttpClientFactory http, IMemoryCache cache, IOptions<IntegracaoOptions> opcoes, ILogger<ProvedorPandaScore> log)
    {
        _http = http; _cache = cache; _opcoes = opcoes; _log = log;
    }

    private PandaScoreOptions Opt => _opcoes.Value.PandaScore;

    public string Nome => "PandaScore";
    public bool Habilitado => !string.IsNullOrWhiteSpace(Opt.Token);
    public bool AgendaBarata => false;
    public bool Atende(string modalidadeSlug) => Habilitado && modalidadeSlug is "lol" or "valorant" or "cs";

    public Task<IReadOnlyList<PartidaExterna>> ObterAoVivoAsync(CancellationToken ct = default) =>
        BuscarAsync("matches/running", ct);

    public async Task<IReadOnlyList<PartidaExterna>> ObterAgendaAsync(DateOnly data, CancellationToken ct = default)
    {
        var proximas = await BuscarAsync("matches/upcoming", ct);
        var passadas = await BuscarAsync("matches/past", ct);
        return proximas.Concat(passadas)
            .Where(p => DateOnly.FromDateTime(p.IniciaEmUtc.ToLocalTime()) == data)
            .OrderBy(p => p.IniciaEmUtc)
            .ToList();
    }

    private async Task<IReadOnlyList<PartidaExterna>> BuscarAsync(string rota, CancellationToken ct)
    {
        if (!Habilitado) return Array.Empty<PartidaExterna>();
        try
        {
            var json = await _cache.GetOrCreateAsync($"panda:{rota}", async entrada =>
            {
                entrada.AbsoluteExpirationRelativeToNow = ValidadeCache;
                return await _http.CreateClient("pandascore").GetStringAsync(rota, ct);
            });
            return json is null ? Array.Empty<PartidaExterna>() : Mapear(json);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            _log.LogWarning("PandaScore {Rota}: {Mensagem}", rota, ex.Message);
            return Array.Empty<PartidaExterna>();
        }
    }

    private static List<PartidaExterna> Mapear(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var lista = new List<PartidaExterna>();
        if (doc.RootElement.ValueKind != JsonValueKind.Array) return lista;

        foreach (var m in doc.RootElement.EnumerateArray())
        {
            var modalidade = ModalidadeDe(JsonUtil.Texto(m, "videogame", "slug") ?? JsonUtil.Texto(m, "videogame", "name"));
            if (modalidade is null) continue;

            if (!m.TryGetProperty("opponents", out var ops) || ops.ValueKind != JsonValueKind.Array || ops.GetArrayLength() < 2) continue;
            var o1 = JsonUtil.Obter(ops[0], "opponent");
            var o2 = JsonUtil.Obter(ops[1], "opponent");
            if (o1 is null || o2 is null) continue;   // confronto ainda indefinido (TBD)

            string? detalhe = null;
            if (m.TryGetProperty("games", out var jogos) && jogos.ValueKind == JsonValueKind.Array)
                foreach (var g in jogos.EnumerateArray())
                    if (JsonUtil.Texto(g, "status") == "running" && JsonUtil.Inteiro(g, "position") is int pos)
                        detalhe = $"Mapa {pos}";

            var urlLive = JsonUtil.Texto(m, "live", "url");

            lista.Add(new PartidaExterna
            {
                Fonte = "PandaScore",
                IdExterno = JsonUtil.Texto(m, "id") ?? "",
                ModalidadeSlug = modalidade,
                Campeonato = JsonUtil.Texto(m, "league", "name") ?? "PandaScore",
                CampeonatoIdExterno = JsonUtil.Texto(m, "league", "id"),
                // A PandaScore nao tem mandante: o primeiro adversario e tratado como "casa".
                Casa = MapearTime(o1.Value),
                Fora = MapearTime(o2.Value),
                Status = MapearStatus(JsonUtil.Texto(m, "status")),
                Detalhe = detalhe,
                PlacarCasa = Placar(m, JsonUtil.Inteiro(o1.Value, "id")),
                PlacarFora = Placar(m, JsonUtil.Inteiro(o2.Value, "id")),
                MelhorDe = JsonUtil.Inteiro(m, "number_of_games"),
                IniciaEmUtc = JsonUtil.Data(m, "begin_at") ?? JsonUtil.Data(m, "scheduled_at") ?? DateTime.UtcNow,
                TempoRealDisponivel = JsonUtil.Booleano(m, "live", "supported") && !string.IsNullOrEmpty(urlLive),
                UrlTempoReal = urlLive
            });
        }
        return lista;
    }

    private static TimeExterno MapearTime(JsonElement oponente)
    {
        var nome = JsonUtil.Texto(oponente, "name") ?? "?";
        return new TimeExterno
        {
            IdExterno = JsonUtil.Texto(oponente, "id") ?? "",
            Nome = nome,
            Sigla = JsonUtil.Texto(oponente, "acronym") ?? nome[..Math.Min(3, nome.Length)].ToUpperInvariant(),
            EscudoUrl = JsonUtil.Texto(oponente, "image_url")
        };
    }

    // Placar = mapas ganhos, vindo de results[{ team_id, score }].
    private static int Placar(JsonElement partida, int? idTime)
    {
        if (idTime is null || !partida.TryGetProperty("results", out var rs) || rs.ValueKind != JsonValueKind.Array) return 0;
        foreach (var r in rs.EnumerateArray())
            if (JsonUtil.Inteiro(r, "team_id") == idTime) return JsonUtil.Inteiro(r, "score") ?? 0;
        return 0;
    }

    private static StatusPartidaExterna MapearStatus(string? status) => status switch
    {
        "running" => StatusPartidaExterna.AoVivo,
        "finished" => StatusPartidaExterna.Encerrada,
        "canceled" => StatusPartidaExterna.Cancelada,
        "postponed" => StatusPartidaExterna.Adiada,
        _ => StatusPartidaExterna.Agendada
    };

    private static string? ModalidadeDe(string? videogame)
    {
        var s = (videogame ?? "").ToLowerInvariant();
        if (s.Contains("league")) return "lol";
        if (s.Contains("valorant")) return "valorant";
        if (s.Contains("cs") || s.Contains("counter")) return "cs";
        return null;
    }

    // ---- WebSocket (feed "frames": foto do jogo a cada ~2 s) ----
    // Limite da PandaScore: 3 conexoes simultaneas por partida por endpoint.
    // Os frames chegam como JSON bruto; converter em placar/eventos e a proxima etapa (BACKEND).
    public async Task AssinarAsync(PartidaExterna partida, Func<string, Task> aoReceber, CancellationToken ct)
    {
        if (!Habilitado || string.IsNullOrWhiteSpace(partida.IdExterno)) return;

        var uri = new Uri($"{Opt.LiveBaseUrl.TrimEnd('/')}/matches/{partida.IdExterno}?token={Uri.EscapeDataString(Opt.Token)}");
        var espera = TimeSpan.FromSeconds(2);
        var falhas = 0;

        while (!ct.IsCancellationRequested)
        {
            using var ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(uri, ct);
                falhas = 0;
                espera = TimeSpan.FromSeconds(2);
                await ReceberAsync(ws, aoReceber, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                falhas++;
                _log.LogWarning("PandaScore WebSocket da partida {Id}: {Mensagem}", partida.IdExterno, ex.Message);
                if (falhas >= MaxFalhasSeguidas)
                {
                    _log.LogWarning("PandaScore WebSocket da partida {Id}: desistindo apos {N} falhas (plano sem Live API?)", partida.IdExterno, falhas);
                    return;
                }
            }

            try { await Task.Delay(espera, ct); }
            catch (OperationCanceledException) { break; }
            espera = TimeSpan.FromSeconds(Math.Min(espera.TotalSeconds * 2, 30));
        }
    }

    private static async Task ReceberAsync(ClientWebSocket ws, Func<string, Task> aoReceber, CancellationToken ct)
    {
        var buffer = new byte[16 * 1024];
        using var mensagem = new MemoryStream();

        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            mensagem.SetLength(0);
            WebSocketReceiveResult resultado;
            do
            {
                resultado = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (resultado.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    return;
                }
                mensagem.Write(buffer, 0, resultado.Count);
            } while (!resultado.EndOfMessage);

            if (resultado.MessageType == WebSocketMessageType.Text)
                await aoReceber(Encoding.UTF8.GetString(mensagem.GetBuffer(), 0, (int)mensagem.Length));
        }
    }
}