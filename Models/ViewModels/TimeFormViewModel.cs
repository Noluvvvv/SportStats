using System.ComponentModel.DataAnnotations;

namespace SportStats.Models.ViewModels;

public class TimeFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do time.")]
    [StringLength(80, ErrorMessage = "O nome pode ter no máximo 80 caracteres.")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Informe a sigla do time.")]
    [StringLength(4, MinimumLength = 2, ErrorMessage = "A sigla deve ter de 2 a 4 caracteres.")]
    public string Sigla { get; set; } = "";

    [Required(ErrorMessage = "Informe a cidade.")]
    [StringLength(80, ErrorMessage = "A cidade pode ter no máximo 80 caracteres.")]
    public string Cidade { get; set; } = "";
}
