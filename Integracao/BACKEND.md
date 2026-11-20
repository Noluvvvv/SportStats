# Pendencias de back end: Integracao

<!-- integracao-2026-11 -->
## Estado atual e pendencias
- Pronto: ESPN (REST, sem token), PandaScore (REST + WebSocket opcional), cache em memoria, ServicoTempoReal, endpoints /api/partidas/* e pagina /Esports com SignalR.
- [ ] PandaScore devolvendo 401: o provedor fica desligado ate reiniciar o app. Sem token valido o app segue com ESPN; /Esports mostra widget (se configurado) ou aviso.
- [ ] (futuro, BACK-11) Rever o uso da PandaScore sem depender do token: outro plano ou provedor, ou cadastro manual pelo CRUD.
- [ ] (futuro, BACK-10) Iframe do Flashscore como fallback visual em /Esports. Antes: conferir se existe incorporacao oficial e os termos de uso. Nao verificado.
- [ ] Confirmar o slug da ESPN para o Brasileirao (bra.1) e se ha cobertura de volei.
- [ ] Pausar o polling quando nao houver clientes conectados (cota da PandaScore: cerca de 144 req/h em uso normal; conferir o limite do plano no painel).
- [ ] Implementar ObterDetalhes (ESPN summary, PandaScore match) para a pagina de detalhes da partida.
- [ ] Converter os frames do WebSocket da PandaScore em eventos (exige plano com Live API; TempoRealWebSocket esta desligado).
- [ ] Testes dos parsers de ESPN e PandaScore com JSON de exemplo salvo em arquivo.
- Regra: tokens so em user-secrets ou variavel de ambiente. Nunca em appsettings.json.
