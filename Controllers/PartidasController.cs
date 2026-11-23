// TODO [BACK-02] CRUD de Partida, placar e detalhes  (manual: R1, R3, R4)
//
// 1. Criar PartidasController injetando IPartidaRepository e IRepository<Time>. Ok!
// 2. Criar partida: formulario com Tipo (Futebol, Basquete, Volei, Esport). Ok!
//    Montar a subclasse certa (PartidaFutebol, PartidaBasquete, PartidaVolei, PartidaEsport).
//    Esport exige MelhorDe = 1, 3 ou 5.
// 3. RegistrarPlacar(id, casa, fora) e Encerrar(id): usar os metodos de dominio Ok!
//    Partida.RegistrarPlacar() e Partida.Encerrar(); depois IPartidaRepository.Atualizar().
// 4. Capturar PlacarInvalidoException, PartidaEncerradaException e EscalacaoInvalidaException Ok!
//    (todas herdam de DominioException) e exibir alerta amigavel na view (R4).
// 5. Action Detalhes(id): pagina da partida. Depois trocar href="#" em Ok!
//    Views/Shared/_PartidaCard.cshtml pelo link de Detalhes.
// 6. Views em Views/Partidas/ (Index, Criar, Detalhes). Ok!
// 7. Na Detalhes, mostrar retrospecto entre os dois times (consulta nova em Ok!
//    IPartidaConsultaRepository, com SQL parametrizado).

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Models.Enums;
using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;

namespace SportStats.Controllers
{
    public class PartidasController : Controller
    {
        private readonly IPartidaRepository _partidaRepository;         
        private readonly IRepository<Time> _timeRepository;               
        private readonly IPartidaConsultaRepository _consultaRepository;  

        public PartidasController(IPartidaRepository partidaRepository,
                                  IRepository<Time> timeRepository,
                                  IPartidaConsultaRepository consultaRepository)
        {
            _partidaRepository = partidaRepository;
            _timeRepository = timeRepository;
            _consultaRepository = consultaRepository;
        }

        public IActionResult Index()
        {
            return View(_consultaRepository.ListarResumos().ToList());
        }

        [HttpGet]
        public IActionResult Criar()
        {
            return View(PreencherListas(new PartidaFormViewModel()));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Criar(PartidaFormViewModel form)
        {
            if (!ModelState.IsValid)
                return View(PreencherListas(form));

            try
            {
                var partida = MontarPartida(form);
                var id = _partidaRepository.Inserir(partida);

                TempData["Sucesso"] = "Partida cadastrada com sucesso.";
                return RedirectToAction(nameof(Detalhes), new { id });
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
                return View(PreencherListas(form));
            }
        }

        public IActionResult Detalhes(int id)
        {
            var resumo = _consultaRepository.ObterResumoPorId(id);
            var partida = _partidaRepository.ObterPorId(id);
            if (resumo is null || partida is null)
            {
                TempData["Erro"] = "Partida não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var modelo = new PartidaDetalhesViewModel
            {
                Partida = resumo,
                Retrospecto = _consultaRepository.ObterRetrospecto(id, partida.TimeCasaId, partida.TimeForaId)
            };
            return View(modelo);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Iniciar(int id) =>
            AtualizarPartida(id, partida => partida.Iniciar(), "Partida iniciada.");

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RegistrarPlacar(int id, [BindRequired] int casa, [BindRequired] int fora, int? minuto = null)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Informe um placar válido e, se preenchido, um minuto inteiro.";
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            // Campo ausente preserva o minuto. Campo enviado em branco limpa o valor.
            var minutoInformado = minuto.HasValue || ModelState.ContainsKey(nameof(minuto));
            return AtualizarPartida(id, partida =>
            {
                if (minutoInformado)
                    partida.AtualizarMinuto(minuto);
                partida.RegistrarPlacar(casa, fora);
            }, "Placar atualizado.");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Encerrar(int id) =>
            AtualizarPartida(id, partida => partida.Encerrar(), "Partida encerrada.");

        private IActionResult AtualizarPartida(int id, Action<Partida> alterar, string mensagemSucesso)
        {
            try
            {
                var partida = _partidaRepository.ObterPorId(id);
                if (partida is null)
                {
                    TempData["Erro"] = "Partida não encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                alterar(partida);
                _partidaRepository.Atualizar(partida);

                TempData["Sucesso"] = mensagemSucesso;
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
            }
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            try
            {
                _partidaRepository.Remover(id);
                TempData["Sucesso"] = "Partida excluída.";
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Detalhes), new { id });
            }
            return RedirectToAction(nameof(Index));
        }

        private static Partida MontarPartida(PartidaFormViewModel f)
        {
            var dataHora = f.DataHora!.Value; 

            return f.Tipo switch
            {
                TipoPartida.Futebol  => new PartidaFutebol(f.CampeonatoId, f.TimeCasaId, f.TimeForaId, dataHora),
                TipoPartida.Basquete => new PartidaBasquete(f.CampeonatoId, f.TimeCasaId, f.TimeForaId, dataHora),
                TipoPartida.Volei    => new PartidaVolei(f.CampeonatoId, f.TimeCasaId, f.TimeForaId, dataHora),
                TipoPartida.Esport   => new PartidaEsport(f.CampeonatoId, f.TimeCasaId, f.TimeForaId, dataHora, f.MelhorDe),
                _ => throw new DominioException("Tipo de partida inválido.")
            };
        }

        private PartidaFormViewModel PreencherListas(PartidaFormViewModel form)
        {
            form.Tipos = new List<SelectListItem>
            {
                new("Futebol", nameof(TipoPartida.Futebol)),
                new("Basquete", nameof(TipoPartida.Basquete)),
                new("Vôlei", nameof(TipoPartida.Volei)),
                new("E-sport", nameof(TipoPartida.Esport))
            };

            form.Campeonatos = _consultaRepository.ListarCompeticoes()
                .Select(c => new SelectListItem($"{c.Nome} {c.Temporada} - {c.Modalidade}", c.Id.ToString()))
                .ToList();

            form.Times = _timeRepository.ObterTodos()
                .Select(t => new SelectListItem($"{t.Nome} ({t.Sigla})", t.Id.ToString()))
                .ToList();

            return form;
        }
    }
}
