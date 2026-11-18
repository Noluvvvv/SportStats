using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportStats.Hubs;
using SportStats.Integracao.Modelos;
using SportStats.Integracao.Opcoes;

namespace SportStats.Integracao;

// Atualiza o cache em ciclos e avisa o navegador pelo SignalR.
// Provedor pausado (sem token, 401 ou cota) simplesmente pula o ciclo e volta quando liberar.
public sealed class ServicoTempoReal : BackgroundService
{
    private readonly RoteadorDeProvedores _roteador;
    private readonly CachePartidasExternas _cache;
    private readonly IOptions<IntegracaoOptions> _opcoes;
    private readonly IHubContext<PlacarHub> _hub;
    private readonly ILogger<ServicoTempoReal> _log;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _assinaturas = new();

    public ServicoTempoReal(RoteadorDeProvedores roteador, CachePartidasExternas cache,
                            IOptions<IntegracaoOptions> opcoes, IHubContext<PlacarHub> hub, ILogger<ServicoTempoReal> log)
    {
        _roteador = roteador; _cache = cache; _opcoes = opcoes; _hub = hub; _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var todos = _roteador.Todos();
        _log.LogInformation("Provedores: {Estado}", string.Join(", ", todos.Select(p => $"{p.Nome}={(p.Habilitado ? "ativo" : "inativo")}")));
        await Task.WhenAll(todos.Select(p => LoopAsync(p, ct)));
    }

    private async Task LoopAsync(IProvedorDados provedor, CancellationToken ct)
    {
        var o = _opcoes.Value;
        var intervalo = Math.Max(10, o.IntervaloAoVivoSegundos);
        var proximaAgenda = DateTime.MinValue;

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalo));
        do
        {
            try
            {
                if (provedor.Habilitado)
                {
                    var agora = DateTime.UtcNow;
                    if (provedor.AgendaBarata || agora >= proximaAgenda)
                    {
                        _cache.Mesclar(provedor.Nome, await provedor.ObterAgendaAsync(DateOnly.FromDateTime(DateTime.Now), ct));
                        proximaAgenda = agora.AddSeconds(Math.Max(60, o.IntervaloAgendaSegundos));
                    }

                    var aoVivo = await provedor.ObterAoVivoAsync(ct);
                    _cache.Mesclar(provedor.Nome, aoVivo);
                    await PublicarAsync(aoVivo, ct);

                    if (provedor is IProvedorTempoReal tempoReal && o.TempoRealWebSocket)
                        GerenciarAssinaturas(tempoReal, aoVivo, o.MaxPartidasWebSocket, ct);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Falha ao atualizar {Fonte}", provedor.Nome);
            }
        } while (await AguardarAsync(timer, ct));
    }

    // Envia o placar das partidas ao vivo para os grupos do SignalR: grupo (esportes/esports) e modalidade (lol, cs...).
    private async Task PublicarAsync(IReadOnlyList<PartidaExterna> aoVivo, CancellationToken ct)
    {
        if (aoVivo.Count == 0) return;

        foreach (var g in aoVivo.GroupBy(p => Modalidades.Esports.Contains(p.ModalidadeSlug) ? "esports" : "esportes"))
            await _hub.Clients.Group(g.Key).SendAsync("placarAtualizado", g.Select(Resumo).ToList(), ct);

        foreach (var g in aoVivo.GroupBy(p => p.ModalidadeSlug))
            await _hub.Clients.Group(g.Key).SendAsync("placarAtualizado", g.Select(Resumo).ToList(), ct);
    }

    private static object Resumo(PartidaExterna p) => new
    {
        chave = p.Chave,
        placarCasa = p.PlacarCasa,
        placarFora = p.PlacarFora,
        status = p.Status.ToString(),
        detalhe = p.Detalhe,
        minuto = p.Minuto
    };

    private void GerenciarAssinaturas(IProvedorTempoReal provedor, IReadOnlyList<PartidaExterna> aoVivo, int maximo, CancellationToken ct)
    {
        foreach (var p in aoVivo.Where(x => x.TempoRealDisponivel && x.Status == StatusPartidaExterna.AoVivo))
        {
            if (_assinaturas.Count >= maximo) break;

            var chave = p.Chave;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (!_assinaturas.TryAdd(chave, cts)) { cts.Dispose(); continue; }

            _ = Task.Run(async () =>
            {
                try
                {
                    await provedor.AssinarAsync(p, json => { _cache.GuardarFrame(chave, json); return Task.CompletedTask; }, cts.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { _log.LogWarning("WebSocket {Chave}: {Mensagem}", chave, ex.Message); }
                finally
                {
                    _assinaturas.TryRemove(chave, out _);
                    cts.Dispose();
                }
            }, CancellationToken.None);
        }

        var vivas = aoVivo.Select(x => x.Chave).ToHashSet();
        foreach (var kv in _assinaturas)
        {
            if (!kv.Key.StartsWith(provedor.Nome + ":") || vivas.Contains(kv.Key)) continue;
            try { kv.Value.Cancel(); } catch (ObjectDisposedException) { }
        }
    }

    private static async Task<bool> AguardarAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try { return await timer.WaitForNextTickAsync(ct); }
        catch (OperationCanceledException) { return false; }
    }
}