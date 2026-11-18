using Microsoft.Extensions.Options;
using SportStats.Integracao.Config;

namespace SportStats.Integracao.Providers;

// Nao ha scraping nem chamada de API: so entrega a URL oficial do widget, se estiver configurada.
public class EsportsResultsProvider
{
    private readonly IOptions<EsportsOptions> _opcoes;

    public EsportsResultsProvider(IOptions<EsportsOptions> opcoes) => _opcoes = opcoes;

    public string? ObterUrlDoWidget()
    {
        var url = _opcoes.Value.EsportsResultsWidgetUrl?.Trim();
        if (string.IsNullOrWhiteSpace(url)) return null;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            ? uri.ToString()
            : null;
    }
}