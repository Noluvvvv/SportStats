using System.Collections.Concurrent;
using SportStats.Integracao.Modelos;

namespace SportStats.Integracao;

// Armazena em memoria o que chega das APIs.
// Os dados esportivos permanecem somente em memoria; nao sao persistidos no MySQL.
public sealed class CachePartidasExternas
{
    private static readonly TimeSpan ValidadeAoVivo = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ValidadeItem = TimeSpan.FromHours(12);

    private readonly ConcurrentDictionary<string, PartidaExterna> _itens = new();
    private readonly ConcurrentDictionary<string, string> _frames = new();
    private readonly ConcurrentDictionary<string, DateTime> _atualizacoes = new();

    public void Mesclar(string fonte, IEnumerable<PartidaExterna> partidas)
    {
        var agora = DateTime.UtcNow;
        foreach (var p in partidas)
        {
            p.AtualizadaEmUtc = agora;
            _itens[p.Chave] = p;
        }
        _atualizacoes[fonte] = agora;
        Podar(agora);
    }

    public IReadOnlyList<PartidaExterna> AoVivo(string? grupo)
    {
        var limite = DateTime.UtcNow - ValidadeAoVivo;
        return _itens.Values
            .Where(p => p.Status is StatusPartidaExterna.AoVivo or StatusPartidaExterna.Intervalo)
            .Where(p => p.AtualizadaEmUtc >= limite)
            .Where(p => Modalidades.PertenceAoGrupo(p.ModalidadeSlug, grupo))
            .OrderBy(p => p.IniciaEmUtc)
            .ToList();
    }

    public IReadOnlyList<PartidaExterna> Agenda(string? grupo) =>
        _itens.Values
            .Where(p => p.Status != StatusPartidaExterna.AoVivo && p.Status != StatusPartidaExterna.Intervalo)
            .Where(p => Modalidades.PertenceAoGrupo(p.ModalidadeSlug, grupo))
            .OrderBy(p => p.IniciaEmUtc)
            .ToList();

    public DateTime? UltimaAtualizacao(string fonte) =>
        _atualizacoes.TryGetValue(fonte, out var d) ? d : null;

    public void GuardarFrame(string chave, string json) => _frames[chave] = json;
    public string? Frame(string chave) => _frames.TryGetValue(chave, out var json) ? json : null;

    private void Podar(DateTime agora)
    {
        foreach (var kv in _itens)
            if (agora - kv.Value.AtualizadaEmUtc > ValidadeItem) _itens.TryRemove(kv.Key, out _);
    }
}
