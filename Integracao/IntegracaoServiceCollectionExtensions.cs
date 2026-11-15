using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using SportStats.Integracao.Espn;
using SportStats.Integracao.Opcoes;
using SportStats.Integracao.PandaScore;

namespace SportStats.Integracao;

public static class IntegracaoServiceCollectionExtensions
{
    public static IServiceCollection AddIntegracaoEsportiva(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<IntegracaoOptions>(config.GetSection("Integracao"));
        services.PostConfigure<IntegracaoOptions>(o => o.AplicarPadroes());
        services.AddMemoryCache();

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
            if (!string.IsNullOrWhiteSpace(o.Token))
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", o.Token);
        });

        services.AddSingleton<ProvedorEspn>();
        services.AddSingleton<ProvedorPandaScore>();
        services.AddSingleton<IProvedorDados>(sp => sp.GetRequiredService<ProvedorEspn>());
        services.AddSingleton<IProvedorDados>(sp => sp.GetRequiredService<ProvedorPandaScore>());
        services.AddSingleton<RoteadorDeProvedores>();
        services.AddSingleton<CachePartidasExternas>();
        services.AddHostedService<ServicoTempoReal>();
        return services;
    }
}