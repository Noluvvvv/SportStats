using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SportStats.Models.ViewModels;

public class TransferenciaFormViewModel
{
    public int AtletaId { get; set; }

    [Display(Name = "Novo time")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha um time.")]
    public int TimeId { get; set; }

    [Display(Name = "Data da transferência")]
    [DataType(DataType.Date)]
    public DateTime DataInicio { get; set; } = DateTime.Today;

    [BindNever, ValidateNever]
    public List<SelectListItem> Times { get; set; } = new();
}