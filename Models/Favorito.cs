using System.ComponentModel.DataAnnotations;

namespace SportStats.Models;

public class Favorito
{
    public int UsuarioId { get; set; }

    [StringLength(12)]
    public string Tipo { get; set; } = "";

    [StringLength(20)]
    public string Fonte { get; set; } = "";

    [StringLength(60)]
    public string IdExterno { get; set; } = "";

    [StringLength(120)]
    public string? Nome { get; set; }

    public DateTime CriadoEm { get; set; }
}
