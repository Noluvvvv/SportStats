// TODO [BACK-03] Atleta e Transferencia  (manual: R1, R2, R4)
//
// 1. Models/Atleta.cs: Nome, Nickname (opcional), Nacionalidade, ModalidadeId.
//    Propriedades privadas, construtor validando, metodo de dominio para transferir.
// 2. Models/Transferencia.cs: AtletaId, TimeId, DataInicio, DataFim (nula = time atual).
// 3. Composicao (R2): Atleta guarda List<Transferencia> e controla o ciclo de vida.
// 4. Regra: um atleta so pode ter um time ativo (DataFim nula).
//    Violacao lanca AtletaEmDoisTimesException (criar em Exceptions/, herdando de DominioException).
// 5. Repositories/Dapper/AtletaRepository.cs : IRepository<Atleta> e
//    TransferenciaRepository (SQL parametrizado, sem SQL no controller).
// 6. AtletasController: Index, Criar, Editar, Excluir e Transferir (GET/POST).
// 7. Views em Views/Atletas/. Mostrar o historico de times do atleta.
// 8. Registrar os repositorios no Program.cs (AddScoped).




using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;

namespace SportStats.Controllers;

public class AtletasController : Controller
{
    private readonly IRepository<Atleta> _atletaRepository;
    private readonly IModalidadeRepository _modalidadeRepository;
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly IRepository<Time> _timeRepository;

    public AtletasController(
        IRepository<Atleta> atletaRepository,
        IModalidadeRepository modalidadeRepository,
        ITransferenciaRepository transferenciaRepository,
        IRepository<Time> timeRepository)
    {
        _atletaRepository = atletaRepository;
        _modalidadeRepository = modalidadeRepository;
        _transferenciaRepository = transferenciaRepository;
        _timeRepository = timeRepository;
    }





    public IActionResult Index()
    {
        var atletas = _atletaRepository.ObterTodos().ToList();
        var times = _timeRepository.ObterTodos().ToDictionary(t => t.Id, t => t.Nome);
        var modalidades = _modalidadeRepository.ObterTodas()
            .ToDictionary(m => m.Id, m => m.Nome);

        var viewModel = new AtletaIndexViewModel();

        foreach (var atleta in atletas)
        {
            var item = new AtletaListaItemViewModel
            {
                Id = atleta.Id,
                Nome = atleta.Nome,
                Nickname = atleta.Nickname,
                Nacionalidade = atleta.Nacionalidade,
                ModalidadeNome = modalidades.GetValueOrDefault(atleta.ModalidadeId, "Não identificada")
            };

            foreach (var transferencia in atleta.Transferencias)
            {
                item.Historico.Add(new TransferenciaHistoricoItemViewModel
                {
                    NomeTime = times.GetValueOrDefault(transferencia.TimeId, "Time não identificado"),
                    DataInicio = transferencia.DataInicio,
                    DataFim = transferencia.DataFim
                });
            }

            viewModel.Atletas.Add(item);
        }

        return View(viewModel);
    }





    [HttpGet]
    public IActionResult Criar()
    {
        return View(NovoFormulario(new AtletaFormViewModel()));
    }



    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Criar(AtletaFormViewModel form)
    {
        if (!ModelState.IsValid)
            return View(NovoFormulario(form));

        try
        {
            var atleta = new Atleta(
                form.Nome,
                form.Nickname,
                form.Nacionalidade,
                form.ModalidadeId);

            _atletaRepository.Inserir(atleta);

            TempData["Sucesso"] = "Atleta cadastrado com sucesso.";
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
        var atleta = _atletaRepository.ObterPorId(id);

        if (atleta is null)
        {
            TempData["Erro"] = "Atleta não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var form = new AtletaFormViewModel
        {
            Id = atleta.Id,
            Nome = atleta.Nome,
            Nickname = atleta.Nickname,
            Nacionalidade = atleta.Nacionalidade,
            ModalidadeId = atleta.ModalidadeId
        };

        return View(NovoFormulario(form));
    }





    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Editar(int id, AtletaFormViewModel form)
    {
        form.Id = id;

        if (!ModelState.IsValid)
            return View(NovoFormulario(form));

        try
        {
            var atleta = _atletaRepository.ObterPorId(id);

            if (atleta is null)
            {
                TempData["Erro"] = "Atleta não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            atleta.Atualizar(
                form.Nome,
                form.Nickname,
                form.Nacionalidade,
                form.ModalidadeId);

            _atletaRepository.Atualizar(atleta);

            TempData["Sucesso"] = "Atleta atualizado com sucesso.";
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
            _atletaRepository.Remover(id);

            TempData["Sucesso"] = "Atleta excluído com sucesso.";
        }
        catch (DominioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }




    [HttpGet]
    public IActionResult Transferir(int id)
    {
        var atleta = _atletaRepository.ObterPorId(id);

        if (atleta is null)
        {
            TempData["Erro"] = "Atleta não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var form = new TransferenciaFormViewModel
        {
            AtletaId = atleta.Id,
            DataInicio = DateTime.Today
        };

        return View(NovoFormularioTransferencia(form));
    }





    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Transferir(int id, TransferenciaFormViewModel form)
    {
        form.AtletaId = id;

        if (!ModelState.IsValid)
            return View(NovoFormularioTransferencia(form));

        try
        {
            var atleta = _atletaRepository.ObterPorId(id);

            if (atleta is null)
            {
                TempData["Erro"] = "Atleta não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            atleta.Transferir(form.TimeId, form.DataInicio);

            _transferenciaRepository.SalvarHistorico(atleta);

            TempData["Sucesso"] = "Transferência registrada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DominioException ex)
        {
            TempData["Erro"] = ex.Message;
            return View(NovoFormularioTransferencia(form));
        }
    }




    private AtletaFormViewModel NovoFormulario(AtletaFormViewModel form)
    {
        form.Modalidades = _modalidadeRepository.ObterTodas()
            .Select(m => new SelectListItem(m.Nome, m.Id.ToString()))
            .ToList();

        return form;
    }


    private TransferenciaFormViewModel NovoFormularioTransferencia(
        TransferenciaFormViewModel form)
    {
        form.Times = _timeRepository.ObterTodos()
            .Select(t => new SelectListItem(t.Nome, t.Id.ToString()))
            .ToList();

        return form;
    }
}
