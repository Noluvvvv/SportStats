// TODO [BACK-01] CRUD de Time e Campeonato 
//
// 1. Criar TimesController : Controller, injetando IRepository<Time> no construtor. OK!
// 2. Actions: Index, Criar (GET/POST), Editar (GET/POST), Excluir (POST). OK!
// 3. No POST: validar ModelState e capturar DominioException (R4), OK!
//    mostrando a mensagem na tela (TempData["Erro"]) em vez de erro 500.
// 4. Views em Views/Times/ (Index, Criar, Editar), reaproveitando o layout e o site.css. OK!
// 5. Sem SQL no controller: usar so o repositorio (TimeRepository ja existe). OK!
// 6. Fazer o mesmo para Campeonato: OK!
//      - Models/Campeonato.cs (propriedades privadas, construtor valido)
//      - Repositories/Dapper/CampeonatoRepository.cs : IRepository<Campeonato>
//      - Controllers/CampeonatosController.cs e Views/Campeonatos/
//      - Registrar no Program.cs: AddScoped<IRepository<Campeonato>, CampeonatoRepository>()
// 7. Adicionar "Times" e "Campeonatos" no menu de Views/Shared/_Layout.cshtml. OK!



using Microsoft.AspNetCore.Mvc;
using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;

namespace SportStats.Controllers
{
    public class TimesController : Controller
    {
        private readonly IRepository<Time> _timeRepository;

        public TimesController(IRepository<Time> timeRepository)
        {
            _timeRepository = timeRepository;
        }

        public IActionResult Index()
        {
            var times = _timeRepository.ObterTodos();
            return View(times);
        }

        [HttpGet]
        public IActionResult Criar()
        {
            return View(new TimeFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Criar(TimeFormViewModel form)
        {
            if (!ModelState.IsValid)
                return View(form);

            try
            {
                var time = new Time(form.Nome, form.Sigla, form.Cidade);
                _timeRepository.Inserir(time);

                TempData["Sucesso"] = "Time cadastrado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
                return View(form);
            }
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var time = _timeRepository.ObterPorId(id);
            if (time is null)
            {
                TempData["Erro"] = "Time não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var form = new TimeFormViewModel
            {
                Id = time.Id,
                Nome = time.Nome,
                Sigla = time.Sigla,
                Cidade = time.Cidade
            };
            return View(form);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(int id, TimeFormViewModel form)
        {
            form.Id = id; 
            if (!ModelState.IsValid)
                return View(form);

            try
            {
                var time = _timeRepository.ObterPorId(id);
                if (time is null)
                {
                    TempData["Erro"] = "Time não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                time.Atualizar(form.Nome, form.Sigla, form.Cidade); 
                _timeRepository.Atualizar(time);

                TempData["Sucesso"] = "Time atualizado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
                return View(form);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            try
            {
                _timeRepository.Remover(id);
                TempData["Sucesso"] = "Time excluído com sucesso.";
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
