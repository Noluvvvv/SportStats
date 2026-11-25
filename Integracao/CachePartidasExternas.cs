using System.Collections.Concurrent;
using SportStats.Integracao.Modelos;

namespace SportStats.Integracao;

public sealed class CachePartidasExternas
{
    private static readonly TimeSpan ValidadeAoVivo = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ValidadeItem = TimeSpan.FromHours(12);
    private static readonly TimeSpan ValidadeFrame = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, PartidaExterna> _itens = new();

    private readonly ConcurrentDictionary<string, FrameArmazenado> _frames = new();

    private readonly ConcurrentDictionary<string, DateTime> _atualizacoes = new();

    private sealed record FrameArmazenado(string Json, DateTime ExpiraEmUtc);

    public void Mesclar(string fonte, IEnumerable<PartidaExterna> partidas)
    {
        var agora = DateTime.UtcNow;

        foreach (var partida in partidas)
        {
            partida.AtualizadaEmUtc = agora;
            _itens[partida.Chave] = partida;
        }

        _atualizacoes[fonte] = agora;

        Podar(agora);
    }

    // Substitui o conjunto de partidas ao vivo de uma fonte.
    // Deve ser chamado somente depois de uma consulta concluida com sucesso.
    public void SubstituirAoVivo(string fonte, IEnumerable<PartidaExterna> partidas)
    {
        var snapshot = partidas.ToList();

        var chavesAtuais = snapshot
            .Select(p => p.Chave)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var item in _itens.ToArray())
        {
            var partida = item.Value;

            if (!string.Equals(partida.Fonte, fonte, StringComparison.Ordinal))
                continue;

            var estavaAoVivo =
                partida.Status is StatusPartidaExterna.AoVivo
                    or StatusPartidaExterna.Intervalo;

            if (estavaAoVivo && !chavesAtuais.Contains(item.Key))
                _itens.TryRemove(item.Key, out _);
        }

        Mesclar(fonte, snapshot);
    }

    public IReadOnlyList<PartidaExterna> AoVivo(string? grupo)
    {
        var agora = DateTime.UtcNow;

        Podar(agora);

        var limite = agora - ValidadeAoVivo;

        return _itens.Values
            .Where(p =>
                p.Status is StatusPartidaExterna.AoVivo
                    or StatusPartidaExterna.Intervalo)
            .Where(p => p.AtualizadaEmUtc >= limite)
            .Where(p => Modalidades.PertenceAoGrupo(p.ModalidadeSlug, grupo))
            .OrderBy(p => p.IniciaEmUtc)
            .ToList();
    }

    public IReadOnlyList<PartidaExterna> Agenda(string? grupo)
    {
        var agora = DateTime.UtcNow;

        Podar(agora);

        var limite = agora - ValidadeItem;

        return _itens.Values
            .Where(p =>
                p.Status != StatusPartidaExterna.AoVivo
                && p.Status != StatusPartidaExterna.Intervalo)
            .Where(p => p.AtualizadaEmUtc >= limite)
            .Where(p => Modalidades.PertenceAoGrupo(p.ModalidadeSlug, grupo))
            .OrderBy(p => p.IniciaEmUtc)
            .ToList();
    }

    public DateTime? UltimaAtualizacao(string fonte) =>
        _atualizacoes.TryGetValue(fonte, out var data) ? data : null;

    public void GuardarFrame(string chave, string json)
    {
        var agora = DateTime.UtcNow;

        _frames[chave] = new FrameArmazenado(
            json,
            agora.Add(ValidadeFrame)
        );

        if (_frames.Count > 300)
            Podar(agora);
    }

    public string? Frame(string chave)
    {
        if (!_frames.TryGetValue(chave, out var frame))
            return null;

        if (frame.ExpiraEmUtc <= DateTime.UtcNow)
        {
            _frames.TryRemove(chave, out _);
            return null;
        }

        return frame.Json;
    }

    private void Podar(DateTime agora)
    {
        foreach (var item in _itens)
        {
            if (agora - item.Value.AtualizadaEmUtc > ValidadeItem)
                _itens.TryRemove(item.Key, out _);
        }

        foreach (var frame in _frames)
        {
            if (frame.Value.ExpiraEmUtc <= agora)
                _frames.TryRemove(frame.Key, out _);
        }
    }
}
