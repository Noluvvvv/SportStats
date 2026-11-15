using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportStats.Integracao.Modelos;
using SportStats.Integracao.Opcoes;

namespace SportStats.Integracao;

// Atualiza o cache em ciclos. ESPN: polling. PandaScore: polling (+ WebSocket se habilitado).
public sealed class ServicoTempoReal : BackgroundService
{
    private readonly RoteadorDeProvedores _roteador;
    private readonly CachePartidasExternas _cache;
    private readonly IOptions<IntegracaoOptions> _opcoes;
    private readonly ILogger<ServicoTempoReal> _log;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _assinaturas = new();

    public ServicoTempoReal(RoteadorDeProvedores roteador, CachePartidasExternas cache,
                            IOptions<IntegracaoOptions> opcoes, ILogger<ServicoTempoReal> log)
    {
        _roteador = roteador; _cache = cache; _opcoes = opcoes; _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var ativos = _roteador.Todos().Where(p => p.Habilitado).ToList();
        if (ativos.Count == 0)
        {
            _log.LogWarning("Nenhum provedor habilitado: configure ligas da ESPN e/ou o token da PandaScore.");
            return;
        }
        _log.LogInformation("Provedores ativos: {Nomes}", string.Join(", ", ativos.Select(p => p.Nome)));
        await Task.WhenAll(ativos.Select(p => LoopAsync(p, ct)));
    }

    private async Task LoopAsync(IProvedorDados provedor, CancellationToken ct)
    {
        var o = _opcoes.Value;
        var intervalo = Math.Max(10, o.IntervaloAoVivoSegundos);
        var ciclosPorAgenda = provedor.AgendaBarata ? 1 : Math.Max(1, o.IntervaloAgendaSegundos / intervalo);
        var ciclo = 0;

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalo));
        do
        {
            try
            {
                if (ciclo % ciclosPorAgenda == 0)
                    _cache.Mesclar(provedor.Nome, await provedor.ObterAgendaAsync(DateOnly.FromDateTime(DateTime.Now), ct));

                var aoVivo = await provedor.ObterAoVivoAsync(ct);
                _cache.Mesclar(provedor.Nome, aoVivo);

                if (provedor is IProvedorTempoReal tempoReal && o.TempoRealWebSocket)
                    GerenciarAssinaturas(tempoReal, aoVivo, o.MaxPartidasWebSocket, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Falha ao atualizar {Fonte}", provedor.Nome);
            }
            ciclo++;
        } while (await AguardarAsync(timer, ct));
    }

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

        // Encerra a assinatura de quem saiu do ao vivo.
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