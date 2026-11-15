using System.Text.Json.Serialization;

namespace SportStats.Integracao.Modelos;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusPartidaExterna { Agendada, AoVivo, Intervalo, Encerrada, Adiada, Cancelada }

public class TimeExterno
{
    public string IdExterno { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Sigla { get; set; } = "";
    public string? EscudoUrl { get; set; }
}

public class EventoExterno
{
    public string Tipo { get; set; } = "";
    public int Minuto { get; set; }
    public string? AtletaIdExterno { get; set; }
    public string? Descricao { get; set; }
}

// Formato unico do SportStats: cada provedor converte a resposta da sua API para esta classe.
public class PartidaExterna
{
    public string Fonte { get; set; } = "";                 // "ESPN" ou "PandaScore"
    public string IdExterno { get; set; } = "";
    public string ModalidadeSlug { get; set; } = "";        // futebol, basquete, volei, lol, valorant, cs
    public string Campeonato { get; set; } = "";
    public string? CampeonatoIdExterno { get; set; }
    public TimeExterno Casa { get; set; } = new();
    public TimeExterno Fora { get; set; } = new();
    public StatusPartidaExterna Status { get; set; }
    public string? Detalhe { get; set; }                    // "67'", "Q3 5:23", "Mapa 2"
    public int? Minuto { get; set; }
    public int PlacarCasa { get; set; }
    public int PlacarFora { get; set; }
    public int? MelhorDe { get; set; }
    public DateTime IniciaEmUtc { get; set; }
    public bool TempoRealDisponivel { get; set; }
    public DateTime AtualizadaEmUtc { get; set; }
    public List<EventoExterno> Eventos { get; set; } = new();

    public string Chave => $"{Fonte}:{IdExterno}";

    [JsonIgnore]
    public string? UrlTempoReal { get; set; }
}