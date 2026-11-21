using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SportStats.Models.ViewModels;

public class AtletaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do atleta.")]
    [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = "";

    [StringLength(50, ErrorMessage = "O nickname pode ter no máximo 50 caracteres.")]
    public string? Nickname { get; set; }

    [Required(ErrorMessage = "Informe a nacionalidade.")]
    [StringLength(50, ErrorMessage = "A nacionalidade pode ter no máximo 50 caracteres.")]
    public string Nacionalidade { get; set; } = "";

    [Display(Name = "Modalidade")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha uma modalidade.")]
    public int ModalidadeId { get; set; }

    [BindNever, ValidateNever]
    public List<SelectListItem> Modalidades { get; set; } = new();
}