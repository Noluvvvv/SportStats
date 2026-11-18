using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;

namespace SportStats.Controllers
{
    public class CampeonatosController : Controller
    {
        private readonly IRepository<Campeonato> _campeonatoRepository;
        private readonly IModalidadeRepository _modalidadeRepository;

        public CampeonatosController(IRepository<Campeonato> campeonatoRepository,
                                     IModalidadeRepository modalidadeRepository)
        {
            _campeonatoRepository = campeonatoRepository;
            _modalidadeRepository = modalidadeRepository;
        }

        public IActionResult Index()
        {
            return View(_campeonatoRepository.ObterTodos());
        }

        [HttpGet]
        public IActionResult Criar()
        {
            return View(NovoFormulario(new CampeonatoFormViewModel()));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Criar(CampeonatoFormViewModel form)
        {
            if (!ModelState.IsValid)
                return View(NovoFormulario(form));

            try
            {
                var campeonato = new Campeonato(form.Nome, form.Temporada, form.ModalidadeId);
                _campeonatoRepository.Inserir(campeonato);

                TempData["Sucesso"] = "Campeonato cadastrado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
                return View(NovoFormulario(form));
            }
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var campeonato = _campeonatoRepository.ObterPorId(id);
            if (campeonato is null)
            {
                TempData["Erro"] = "Campeonato não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var form = new CampeonatoFormViewModel
            {
                Id = campeonato.Id,
                Nome = campeonato.Nome,
                Temporada = campeonato.Temporada,
                ModalidadeId = campeonato.ModalidadeId
            };
            return View(NovoFormulario(form));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(int id, CampeonatoFormViewModel form)
        {
            form.Id = id;

            if (!ModelState.IsValid)
                return View(NovoFormulario(form));

            try
            {
                var campeonato = _campeonatoRepository.ObterPorId(id);
                if (campeonato is null)
                {
                    TempData["Erro"] = "Campeonato não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                campeonato.Atualizar(form.Nome, form.Temporada, form.ModalidadeId);
                _campeonatoRepository.Atualizar(campeonato);

                TempData["Sucesso"] = "Campeonato atualizado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
                return View(NovoFormulario(form));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            try
            {
                _campeonatoRepository.Remover(id);
                TempData["Sucesso"] = "Campeonato excluído com sucesso.";
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        private CampeonatoFormViewModel NovoFormulario(CampeonatoFormViewModel form)
        {
            form.Modalidades = _modalidadeRepository.ObterTodas()
                .Select(m => new SelectListItem(m.Nome, m.Id.ToString()))
                .ToList();
            return form;
        }
    }
}
