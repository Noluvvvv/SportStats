using SportStats.Models.Enums;

namespace SportStats.Models.ViewModels;

// Modelo de leitura (telas). Preenchido pelo Dapper com JOIN; nao e entidade de dominio.
public class PartidaResumo
{
    public int Id { get; set; }
    public int CampeonatoId { get; set; }
    public string Campeonato { get; set; } = "";
    public string Modalidade { get; set; } = "";
    public string ModalidadeGrupo { get; set; } = "";
    public string ModalidadeSlug { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string TimeCasa { get; set; } = "";
    public string SiglaCasa { get; set; } = "";
    public string TimeFora { get; set; } = "";
    public string SiglaFora { get; set; } = "";
    public DateTime DataHora { get; set; }
    public int PlacarCasa { get; set; }
    public int PlacarFora { get; set; }
    public StatusPartida Status { get; set; }
    public int? Minuto { get; set; }
    public int MelhorDe { get; set; }

    public bool AoVivo => Status == StatusPartida.AoVivo;
    public bool Agendada => Status == StatusPartida.Agendada;
    public bool Encerrada => Status == StatusPartida.Encerrada;

    public string? Formato => Tipo == "Esport" && MelhorDe > 0 ? $"MD{MelhorDe}" : null;
}

public class CompeticaoItem
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public int Temporada { get; set; }
    public string Modalidade { get; set; } = "";
    public string ModalidadeGrupo { get; set; } = "";
    public string ModalidadeSlug { get; set; } = "";
}
