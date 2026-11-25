using SportStats.Integracao.Modelos;

namespace SportStats.Models.ViewModels;

public class HomeViewModel
{
    public string Grupo { get; set; } = "esportes";
    public string? ModalidadeSlug { get; set; }
    public string? CampeonatoId { get; set; }

    public List<ModalidadeInfo> Modalidades { get; set; } = new();
    public List<CompeticaoExternaItem> Competicoes { get; set; } = new();

    public List<PartidaExterna> AoVivo { get; set; } = new();
    public List<PartidaExterna> Proximas { get; set; } = new();
    public List<PartidaExterna> Recentes { get; set; } = new();

    public string? TituloClassificacao { get; set; }
    public List<LinhaClassificacao> Classificacao { get; set; } = new();

    public DateTime? AtualizacaoEspn { get; set; }
    public DateTime? AtualizacaoPandaScore { get; set; }
}

public record BlocoPartidas(
    string Titulo,
    IReadOnlyList<PartidaExterna> Partidas,
    string Vazio,
    bool Destaque = false
);

public class CompeticaoExternaItem
{
    public string Chave { get; set; } = "";
    public string Nome { get; set; } = "";
    public string ModalidadeNome { get; set; } = "";
    public string ModalidadeSlug { get; set; } = "";
    public string Fonte { get; set; } = "";
}

public class LinhaClassificacao
{
    public string Nome { get; set; } = "";
    public string Sigla { get; set; } = "";
    public int Jogos { get; set; }
    public int Vitorias { get; set; }
    public int Saldo { get; set; }
    public int Pontos { get; set; }
}
