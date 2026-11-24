namespace SportStats.Integracao.Config;

public class FlashscoreOptions
{
    // Endereco de incorporacao (src do iframe) fornecido oficialmente pelo Flashscore.
    public string EmbedUrl { get; set; } = "";
    public string Titulo { get; set; } = "Resultados ao vivo (Flashscore)";
    public int Altura { get; set; } = 650;

    public string? UrlSegura()
    {
        var url = EmbedUrl?.Trim();
        if (string.IsNullOrWhiteSpace(url)) return null;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            ? uri.ToString()
            : null;
    }
}