// TODO [BACK-01] CRUD de Time e Campeonato  (manual: R5, R6, R7)
//
// 1. Criar TimesController : Controller, injetando IRepository<Time> no construtor.
// 2. Actions: Index, Criar (GET/POST), Editar (GET/POST), Excluir (POST).
// 3. No POST: validar ModelState e capturar DominioException (R4),
//    mostrando a mensagem na tela (TempData["Erro"]) em vez de erro 500.
// 4. Views em Views/Times/ (Index, Criar, Editar), reaproveitando o layout e o site.css.
// 5. Sem SQL no controller: usar so o repositorio (TimeRepository ja existe).
// 6. Fazer o mesmo para Campeonato:
//      - Models/Campeonato.cs (propriedades privadas, construtor valido)
//      - Repositories/Dapper/CampeonatoRepository.cs : IRepository<Campeonato>
//      - Controllers/CampeonatosController.cs e Views/Campeonatos/
//      - Registrar no Program.cs: AddScoped<IRepository<Campeonato>, CampeonatoRepository>()
// 7. Adicionar "Times" e "Campeonatos" no menu de Views/Shared/_Layout.cshtml.