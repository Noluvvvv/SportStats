namespace SportStats.Integracao;

// Escolhe o provedor pela modalidade: Esportes -> ESPN, E-sports -> PandaScore.
public sealed class RoteadorDeProvedores
{
    private readonly IReadOnlyList<IProvedorDados> _provedores;

    public RoteadorDeProvedores(IEnumerable<IProvedorDados> provedores) => _provedores = provedores.ToList();

    public IProvedorDados? Para(string modalidadeSlug) =>
        _provedores.FirstOrDefault(p => p.Habilitado && p.Atende(modalidadeSlug));

    public IReadOnlyList<IProvedorDados> Todos() => _provedores;
}