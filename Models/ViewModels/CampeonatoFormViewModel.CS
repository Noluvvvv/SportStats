using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SportStats.Models.ViewModels;

// "Formulário" do Campeonato (o que a pessoa digita/escolhe na tela).
public class CampeonatoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do campeonato.")]
    [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = "";

    [Range(1900, 2100, ErrorMessage = "A temporada deve estar entre 1900 e 2100.")]
    public int Temporada { get; set; } = DateTime.Now.Year;

    [Display(Name = "Modalidade")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha uma modalidade.")]
    public int ModalidadeId { get; set; }

    [BindNever, ValidateNever]
    public List<SelectListItem> Modalidades { get; set; } = new();
}
