using System.ComponentModel.DataAnnotations;

namespace SportStats.Models;

public class Notificacao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }

    [StringLength(20)]
    public string Tipo { get; set; } = "";

    [StringLength(20)]
    public string? Fonte { get; set; }

    [StringLength(60)]
    public string? PartidaIdExterno { get; set; }

    [StringLength(200)]
    public string Mensagem { get; set; } = "";

    public bool Lida { get; set; }
    public DateTime CriadaEm { get; set; }
}
