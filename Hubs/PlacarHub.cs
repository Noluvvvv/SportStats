using Microsoft.AspNetCore.SignalR;

namespace SportStats.Hubs;

// O hub so gerencia grupos. Quem envia os placares e o ServicoTempoReal (via IHubContext).
public class PlacarHub : Hub
{
    private static readonly HashSet<string> GruposValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "esportes", "esports", "futebol", "basquete", "volei", "lol", "valorant", "cs"
    };

    public Task EntrarNoGrupo(string grupo) =>
        GruposValidos.Contains(grupo)
            ? Groups.AddToGroupAsync(Context.ConnectionId, grupo.ToLowerInvariant())
            : Task.CompletedTask;

    public Task SairDoGrupo(string grupo) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, (grupo ?? "").ToLowerInvariant());
}