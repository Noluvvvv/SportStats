using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using SportStats.Integracao.Config;
using SportStats.Integracao.Espn;
using SportStats.Integracao.Opcoes;
using SportStats.Integracao.PandaScore;
using SportStats.Integracao.Providers;
using SportStats.Integracao.Services;

namespace SportStats.Integracao;

public static class IntegracaoServiceCollectionExtensions
{
    public static IServiceCollection AddIntegracaoEsportiva(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<IntegracaoOptions>(config.GetSection("Integracao"));
        services.PostConfigure<IntegracaoOptions>(o => o.AplicarPadroes());
        services.Configure<EsportsOptions>(config.GetSection("Esports"));
        services.AddMemoryCache();
        services.AddSignalR();

        services.AddHttpClient("espn", (sp, c) =>
        {
            var o = sp.GetRequiredService<IOptions<IntegracaoOptions>>().Value.Espn;
            c.BaseAddress = new Uri(o.BaseUrl.TrimEnd('/') + "/");
            c.Timeout = TimeSpan.FromSeconds(10);
            c.DefaultRequestHeaders.UserAgent.ParseAdd("SportStats/1.0 (projeto academico)");
        });

        services.AddHttpClient("pandascore", (sp, c) =>
        {
            var o = sp.GetRequiredService<IOptions<IntegracaoOptions>>().Value.PandaScore;
            c.BaseAddress = new Uri(o.BaseUrl.TrimEnd('/') + "/");
            c.Timeout = TimeSpan.FromSeconds(10);
            var token = (o.Token ?? "").Trim().Trim('"', '\'');
            if (token.Length > 0)
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });

        services.AddSingleton<ProviderStatusService>();
        services.AddSingleton<EsportsResultsProvider>();
        services.AddSingleton<ProvedorEspn>();
        services.AddSingleton<ProvedorPandaScore>();
        services.AddSingleton<IProvedorDados>(sp => sp.GetRequiredService<ProvedorEspn>());
        services.AddSingleton<IProvedorDados>(sp => sp.GetRequiredService<ProvedorPandaScore>());
        services.AddSingleton<RoteadorDeProvedores>();
        services.AddSingleton<CachePartidasExternas>();
        services.AddScoped<EsportsService>();
        services.AddHostedService<ServicoTempoReal>();
        return services;
    }
}