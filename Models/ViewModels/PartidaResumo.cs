namespace SportStats.Models.ViewModels;

// Modelo de leitura (telas). Preenchido pelo Dapper com JOIN; nao e entidade de dominio.
public class PartidaResumo
{
    public static readonly TimeSpan DuracaoAoVivo = TimeSpan.FromHours(3);

    public int Id { get; set; }
    public int CampeonatoId { get; set; }
    public string Campeonato { get; set; } = "";
    public string Modalidade { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string TimeCasa { get; set; } = "";
    public string SiglaCasa { get; set; } = "";
    public string TimeFora { get; set; } = "";
    public string SiglaFora { get; set; } = "";
    public DateTime DataHora { get; set; }
    public int PlacarCasa { get; set; }
    public int PlacarFora { get; set; }
    public bool Encerrada { get; set; }
    public int MelhorDe { get; set; }

    // BACKEND: regra provisoria baseada em horario. Trocar por status real
    // (Agendada / AoVivo / Encerrada + minuto) vindo do banco ou do tempo real. (BACK-05, BACK-06)
    public bool AoVivo => !Encerrada && DataHora <= DateTime.Now && DataHora > DateTime.Now - DuracaoAoVivo;
    public bool Agendada => !Encerrada && DataHora > DateTime.Now;

    public string? Formato => Tipo == "Esport" && MelhorDe > 0 ? $"MD{MelhorDe}" : null;
}

public class CompeticaoItem
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public int Temporada { get; set; }
    public string Modalidade { get; set; } = "";

    public string ModalidadeSlug => CatalogoModalidades.PorNome(Modalidade)?.Slug ?? "";
}