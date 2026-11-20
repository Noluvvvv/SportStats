using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportStats.Models.Enums;

namespace SportStats.Models.ViewModels;

// "Formulário" de criação de partida: guarda o que a pessoa escolheu na tela.
// Os atributos ([Range], [Required]) são a 1ª barreira de validação (ModelState).
public class PartidaFormViewModel
{
    [Display(Name = "Tipo")]
    public TipoPartida Tipo { get; set; } = TipoPartida.Futebol;

    [Display(Name = "Campeonato")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha um campeonato.")]
    public int CampeonatoId { get; set; }

    [Display(Name = "Time da casa")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha o time da casa.")]
    public int TimeCasaId { get; set; }

    [Display(Name = "Time visitante")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha o time visitante.")]
    public int TimeForaId { get; set; }

    // DateTime? (aceita vazio) para o [Required] poder mostrar a mensagem em português.
    [Display(Name = "Data e hora")]
    [Required(ErrorMessage = "Informe a data e a hora da partida.")]
    public DateTime? DataHora { get; set; } = DateTime.Today.AddDays(1).AddHours(19);

    // Só vale para e-sports (melhor de 1, 3 ou 5 mapas). Para os outros tipos é ignorado.
    [Display(Name = "Formato (e-sports)")]
    public int MelhorDe { get; set; } = 3;

    // Listas dos <select>. Quem monta é o controller; o formulário NÃO envia isso de volta.
    [BindNever, ValidateNever] public List<SelectListItem> Tipos { get; set; } = new();
    [BindNever, ValidateNever] public List<SelectListItem> Campeonatos { get; set; } = new();
    [BindNever, ValidateNever] public List<SelectListItem> Times { get; set; } = new();
}

// Tudo o que a tela de Detalhes precisa mostrar.
public class PartidaDetalhesViewModel
{
    public PartidaResumo Partida { get; set; } = new();
    public RetrospectoConfronto Retrospecto { get; set; } = new();
}

// Retrospecto = histórico de jogos já encerrados entre os dois times.
// "Casa" e "Fora" se referem aos times DESTA partida (não ao mando de jogos antigos).
public class RetrospectoConfronto
{
    public int Jogos { get; set; }
    public int VitoriasCasa { get; set; }
    public int VitoriasFora { get; set; }
    public int Empates { get; set; }
    public List<PartidaResumo> Ultimos { get; set; } = new();
}
