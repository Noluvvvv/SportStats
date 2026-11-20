namespace SportStats.Integracao;

public static class Modalidades
{
    public static readonly string[] Esportes = { "futebol", "basquete", "volei" };
    public static readonly string[] Esports = { "lol", "valorant", "cs" };

    public static bool PertenceAoGrupo(string slug, string? grupo) => grupo?.ToLowerInvariant() switch
    {
        "esportes" => Esportes.Contains(slug),
        "esports" => Esports.Contains(slug),
        _ => true
    };
}