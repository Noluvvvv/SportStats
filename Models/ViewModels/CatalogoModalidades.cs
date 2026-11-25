namespace SportStats.Models.ViewModels;

public record ModalidadeInfo(string Slug, string Nome, string Grupo);

public static class CatalogoModalidades
{
    public static readonly IReadOnlyList<ModalidadeInfo> Todas =
        new List<ModalidadeInfo>
        {
            new("futebol", "Futebol", "esportes"),
            new("basquete", "Basquete", "esportes"),
            new("volei", "Vôlei", "esportes"),
            new("lol", "League of Legends", "esports"),
            new("valorant", "Valorant", "esports"),
            new("cs", "Counter-Strike", "esports")
        };

    public static IEnumerable<ModalidadeInfo> DoGrupo(string grupo) =>
        Todas.Where(m => m.Grupo == grupo);

    public static ModalidadeInfo? PorSlug(string? slug) =>
        Todas.FirstOrDefault(m =>
            string.Equals(m.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public static ModalidadeInfo? PorNome(string? nome) =>
        Todas.FirstOrDefault(m =>
            string.Equals(m.Nome, nome, StringComparison.OrdinalIgnoreCase));
}
