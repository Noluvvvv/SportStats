// TODO [BACK-05] Tempo real com SignalR
//
// 1. Criar PlacarHub : Hub e mapear no Program.cs: app.MapHub<PlacarHub>("/hubs/placar").
// 2. Criar um BackgroundService que busca placares (API esportiva ou painel de operador),
//    grava no banco e publica pelo hub.
// 3. Mensagem sugerida:
//      { "id": 1, "placarCasa": 2, "placarFora": 1, "minuto": 67, "status": "AoVivo" }
// 4. No front, em wwwroot/js/site.js, trocar o polling pelo cliente SignalR.
//    O script atualiza os elementos [data-placar-casa="id"] e [data-placar-fora="id"].
// 5. Remover a action Home/AoVivo quando o hub estiver funcionando.