# Pendencias de back end: Controllers

<!-- integracao-2026-11 -->
## Atualizacao (integracao e usuarios)
- Novos, somente leitura: IntegracaoController (/api/partidas/*) e EsportsController (/Esports).
- Antes de editar HomeController, _Layout ou Program.cs: git pull e avisar o grupo (arquivos compartilhados).
- [ ] UsuariosController: cadastro, login e preferencias (hash de senha; ver Database/BACKEND.md).
- [ ] FavoritosController: seguir e deixar de seguir por Tipo + Fonte + IdExterno (dados esportivos nao ficam no MySQL).
- [ ] HistoricoController e NotificacoesController (listar, marcar como lida).
- [ ] Decidir se o link E-sports do topo vai para /Esports (hoje vai para a Home).
- [ ] Capturar DominioException nos POSTs e mostrar alerta amigavel (R4).
