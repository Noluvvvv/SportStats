using SportStats.Integracao.Modelos;

namespace SportStats.Integracao;

public interface IProvedorDados
{
    string Nome { get; }
    bool Habilitado { get; }

    // true quando a agenda do dia vem na mesma chamada do "ao vivo" (caso da ESPN)
    bool AgendaBarata { get; }

    bool Atende(string modalidadeSlug);
    Task<IReadOnlyList<PartidaExterna>> ObterAoVivoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PartidaExterna>> ObterAgendaAsync(DateOnly data, CancellationToken ct = default);
}

// So quem tem WebSocket implementa (PandaScore). A ESPN nao tem WebSocket documentado.
public interface IProvedorTempoReal : IProvedorDados
{
    Task AssinarAsync(PartidaExterna partida, Func<string, Task> aoReceber, CancellationToken ct);
}