using SportStats.Hubs;
using SportStats.Integracao;
using SportStats.Models;
using SportStats.Repositories.Dapper;
using SportStats.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<IPartidaConsultaRepository, PartidaConsultaRepository>();
builder.Services.AddScoped<IRepository<Time>, TimeRepository>();
builder.Services.AddScoped<IRepository<Campeonato>, CampeonatoRepository>();
builder.Services.AddScoped<IModalidadeRepository, ModalidadeRepository>();
builder.Services.AddScoped<IPartidaRepository, PartidaRepository>();
builder.Services.AddScoped<IRepository<Partida>>(sp => sp.GetRequiredService<IPartidaRepository>());
builder.Services.AddScoped<IRepository<Atleta>, AtletaRepository>();
builder.Services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();

builder.Services.AddIntegracaoEsportiva(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

app.MapHub<PlacarHub>("/hubs/placar");

app.Run();