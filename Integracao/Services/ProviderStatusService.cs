namespace SportStats.Integracao.Services;

// Estado em memoria da PandaScore (limite de cota). Com varias instancias do app, mover para Redis.
public sealed class ProviderStatusService
{
    private readonly object _trava = new();
    private DateTimeOffset? _bloqueadoAte;
    private int? _restantes;

    public bool PandaScoreDisponivel
    {
        get { lock (_trava) return _bloqueadoAte is null || DateTimeOffset.UtcNow >= _bloqueadoAte; }
    }

    public DateTimeOffset? BloqueadoAte
    {
        get
        {
            lock (_trava)
            {
                if (_bloqueadoAte is { } ate && ate > DateTimeOffset.UtcNow) return ate;
                return null;
            }
        }
    }

    public int? RequisicoesRestantes
    {
        get { lock (_trava) return _restantes; }
    }

    public void MarcarLimiteAtingido(TimeSpan espera)
    {
        var limitada = TimeSpan.FromSeconds(Math.Clamp(espera.TotalSeconds, 5, 6 * 3600));
        lock (_trava) _bloqueadoAte = DateTimeOffset.UtcNow + limitada;
    }

    public void AtualizarRestantes(int restantes)
    {
        lock (_trava) _restantes = restantes;
    }

    public void Resetar()
    {
        lock (_trava) _bloqueadoAte = null;
    }
}