using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportStats.Integracao.Config;
using SportStats.Integracao.Modelos;
using SportStats.Integracao.Opcoes;
using SportStats.Integracao.Services;

namespace SportStats.Integracao.PandaScore;

// REST: Bearer (token fora da URL e dos logs). WebSocket: so aceita ?token= (URL nunca e registrada em log).
// Cota: 429 pausa o provedor pelo Retry-After; 401/403 desliga ate reiniciar o app.
public sealed class ProvedorPandaScore : IProvedorTempoReal
{
    private static readonly TimeSpan ValidadeCache = TimeSpan.FromSeconds(15);
    private const int MaxFalhasSeguidas = 5;
    private const string RotaAoVivo = "matches/running";
    private const string RotaProximas = "matches/upcoming?sort=begin_at&page%5Bsize%5D=50";
    private const string RotaPassadas = "matches/past?filter%5Bstatus%5D=finished&sort=-begin_at&page%5Bsize%5D=50";

    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly IOptions<IntegracaoOptions> _opcoes;
    private readonly IOptions<EsportsOptions> _esports;
    private readonly ProviderStatusService _status;
    private readonly ILogger<ProvedorPandaScore> _log;
    private volatile bool _autenticacaoFalhou;

    public ProvedorPandaScore(IHttpClientFactory http, IMemoryCache cache, IOptions<IntegracaoOptions> opcoes,
                              IOptions<EsportsOptions> esports, ProviderStatusService status, ILogger<ProvedorPandaScore> log)
    {
        _http = http; _cache = cache; _opcoes = opcoes; _esports = esports; _status = status; _log = log;
    }

    private PandaScoreOptions Opt => _opcoes.Value.PandaScore;
    private string Token => (Opt.Token ?? "").Trim().Trim('"', '\'');

    public string Nome => "PandaScore";
    public bool Configurado => !string.IsNullOrWhiteSpace(Token);
    public bool AutenticacaoFalhou => _autenticacaoFalhou;
    public bool Habilitado => Configurado && !_autenticacaoFalhou && _status.PandaScoreDisponivel;
    public bool AgendaBarata => false;
    public bool Atende(string modalidadeSlug) => Habilitado && modalidadeSlug is "lol" or "valorant" or "cs";

    public Task<IReadOnlyList<PartidaExterna>> ObterAoVivoAsync(CancellationToken ct = default) =>
        BuscarAsync(RotaAoVivo, ct);

    public async Task<IReadOnlyList<PartidaExterna>> ObterAgendaAsync(DateOnly data, CancellationToken ct = default)
    {
        // A API ja separa partidas futuras e passadas.
        // Nao limitar a consulta ao dia atual: isso escondia confrontos futuros.
        _ = data;

        var proximas = await BuscarAsync(RotaProximas, ct);
        var passadas = await BuscarAsync(RotaPassadas, ct);

        return proximas
            .Concat(passadas)
            .GroupBy(p => p.Chave)
            .Select(g => g.First())
            .OrderBy(p => p.IniciaEmUtc)
            .ToList();
    }

    private async Task<IReadOnlyList<PartidaExterna>> BuscarAsync(string rota, CancellationToken ct)
    {
        if (!Habilitado)
            throw new InvalidOperationException("PandaScore indisponivel para consulta.");

        try
        {
            var json = await _cache.GetOrCreateAsync($"panda:{rota}", async entrada =>
            {
                entrada.AbsoluteExpirationRelativeToNow = ValidadeCache;
                return await BaixarAsync(rota, ct);
            });
            if (json is null)
                throw new HttpRequestException("PandaScore nao retornou uma resposta valida.");

            return Mapear(json);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            _log.LogWarning("PandaScore {Rota}: {Mensagem}", rota, ex.Message);
            throw;
        }
    }

    private async Task<string?> BaixarAsync(string rota, CancellationToken ct)
    {
        using var resposta = await _http.CreateClient("pandascore").GetAsync(rota, ct);
        LerSaldo(resposta);

        switch (resposta.StatusCode)
        {
            case HttpStatusCode.TooManyRequests:
                var espera = CalcularEspera(resposta);
                _status.MarcarLimiteAtingido(espera);
                _log.LogWarning("PandaScore: limite de requisicoes atingido. Pausando por {Segundos} s.", (int)espera.TotalSeconds);
                return null;

            case HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden:
                _autenticacaoFalhou = true;
                _log.LogError("PandaScore recusou o token ({Codigo}). Provedor desligado ate reiniciar o app. Confira o token no painel e no user-secrets.", (int)resposta.StatusCode);
                return null;
        }

        resposta.EnsureSuccessStatusCode();
        return await resposta.Content.ReadAsStringAsync(ct);
    }

    private TimeSpan CalcularEspera(HttpResponseMessage resposta)
    {
        var ra = resposta.Headers.RetryAfter;
        if (ra?.Delta is TimeSpan delta) return delta;
        if (ra?.Date is DateTimeOffset data) return data - DateTimeOffset.UtcNow;
        return TimeSpan.FromMinutes(Math.Max(1, _esports.Value.PandaScoreCooldownMinutes));
    }

    private void LerSaldo(HttpResponseMessage resposta)
    {
        if (resposta.Headers.TryGetValues("X-Rate-Limit-Remaining", out var valores) &&
            int.TryParse(valores.FirstOrDefault(), out var restantes))
        {
            _status.AtualizarRestantes(restantes);
            if (restantes < 50) _log.LogWarning("PandaScore: restam {Restantes} requisicoes nesta hora.", restantes);
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
            var status = JsonUtil.Texto(m, "status");

            // A API pode fornecer a data original mesmo quando begin_at
            // ou scheduled_at não estão preenchidos.
            var inicio = JsonUtil.Data(m, "begin_at")
                ?? JsonUtil.Data(m, "scheduled_at")
                ?? JsonUtil.Data(m, "original_scheduled_at");

            // Sem data, não inventamos uma data atual para partidas
            // agendadas ou finalizadas. Partidas ao vivo são preservadas.
            if (inicio is null &&
                !string.Equals(status, "running", StringComparison.OrdinalIgnoreCase))
                continue;

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
                Status = MapearStatus(status),
                Detalhe = detalhe,
                PlacarCasa = Placar(m, JsonUtil.Inteiro(o1.Value, "id")),
                PlacarFora = Placar(m, JsonUtil.Inteiro(o2.Value, "id")),
                MelhorDe = JsonUtil.Inteiro(m, "number_of_games"),
                IniciaEmUtc = inicio ?? DateTime.MinValue,
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
    private static int? Placar(JsonElement partida, int? idTime)
    {
        if (idTime is null ||
            !partida.TryGetProperty("results", out var resultados) ||
            resultados.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var resultado in resultados.EnumerateArray())
        {
            if (JsonUtil.Inteiro(resultado, "team_id") == idTime)
                return JsonUtil.Inteiro(resultado, "score");
        }

        return null;
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

    // ---- WebSocket (frames: foto do jogo a cada ~2 s). Limite: 3 conexoes por partida por endpoint. ----
    public async Task AssinarAsync(PartidaExterna partida, Func<string, Task> aoReceber, CancellationToken ct)
    {
        if (!Habilitado || string.IsNullOrWhiteSpace(partida.IdExterno)) return;

        var uri = new Uri($"{Opt.LiveBaseUrl.TrimEnd('/')}/matches/{partida.IdExterno}?token={Uri.EscapeDataString(Token)}");
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