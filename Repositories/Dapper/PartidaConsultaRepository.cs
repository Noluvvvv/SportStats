using SportStats.Models.Enums;
using SportStats.Models.ViewModels;
using SportStats.Repositories.Interfaces;
using global::Dapper;

namespace SportStats.Repositories.Dapper;

public class PartidaConsultaRepository : IPartidaConsultaRepository
{
    private readonly IDbConnectionFactory _factory;
    public PartidaConsultaRepository(IDbConnectionFactory factory) => _factory = factory;

    // JOIN = "juntar tabelas": traz o nome do campeonato, da modalidade e dos dois times
    // em vez de só os números (Ids) que existem na tabela Partida.
    private const string SelectResumo = @"
        SELECT p.Id, p.CampeonatoId, c.Nome AS Campeonato, m.Nome AS Modalidade, p.Tipo,
               m.Grupo AS ModalidadeGrupo, m.Slug AS ModalidadeSlug,
               tc.Nome AS TimeCasa, tc.Sigla AS SiglaCasa,
               tf.Nome AS TimeFora, tf.Sigla AS SiglaFora,
               p.DataHora, p.PlacarCasa, p.PlacarFora, p.Status, p.Minuto, p.MelhorDe
        FROM Partida p
        JOIN Campeonato c ON c.Id = p.CampeonatoId
        JOIN Modalidade m ON m.Id = c.ModalidadeId
        JOIN `Time` tc ON tc.Id = p.TimeCasaId
        JOIN `Time` tf ON tf.Id = p.TimeForaId";

    // Condição "jogos encerrados entre os dois times, em qualquer mando, menos a partida atual".
    // O texto do SQL é fixo; os VALORES entram sempre por parâmetro (@timeCasaId...),
    // nunca colados no texto: isso evita SQL Injection.
    private const string FiltroConfronto = @"
        p.Status = @statusEncerrada AND p.Id <> @partidaId
        AND ((p.TimeCasaId = @timeCasaId AND p.TimeForaId = @timeForaId)
          OR (p.TimeCasaId = @timeForaId AND p.TimeForaId = @timeCasaId))";

    public IEnumerable<PartidaResumo> ListarResumos()
    {
        using var con = _factory.Criar();
        return con.Query<PartidaResumo>(SelectResumo + " ORDER BY p.DataHora DESC").ToList();
    }

    public PartidaResumo? ObterResumoPorId(int id)
    {
        using var con = _factory.Criar();
        return con.QuerySingleOrDefault<PartidaResumo>(SelectResumo + " WHERE p.Id = @id", new { id });
    }

    public IEnumerable<CompeticaoItem> ListarCompeticoes()
    {
        using var con = _factory.Criar();
        return con.Query<CompeticaoItem>(
            @"SELECT c.Id, c.Nome, c.Temporada, m.Nome AS Modalidade,
                     m.Grupo AS ModalidadeGrupo, m.Slug AS ModalidadeSlug
              FROM Campeonato c
              JOIN Modalidade m ON m.Id = c.ModalidadeId
              ORDER BY m.Nome, c.Nome").ToList();
    }

    // Linha com os totais calculados pelo banco (só existe para o Dapper preencher).
    private sealed class Totais
    {
        public int Jogos { get; set; }
        public int VitoriasCasa { get; set; }
        public int VitoriasFora { get; set; }
        public int Empates { get; set; }
    }

    public RetrospectoConfronto ObterRetrospecto(int partidaId, int timeCasaId, int timeForaId)
    {
        using var con = _factory.Criar();
        var parametros = new { partidaId, timeCasaId, timeForaId, statusEncerrada = nameof(StatusPartida.Encerrada) };

        // 1) Contagem: o banco soma vitórias de cada time e empates.
        //    Um time vence quando joga em casa com placar maior OU fora com placar maior.
        var totais = con.QuerySingle<Totais>(@"
            SELECT COUNT(*) AS Jogos,
                   COALESCE(SUM(CASE WHEN (p.TimeCasaId = @timeCasaId AND p.PlacarCasa > p.PlacarFora)
                                       OR (p.TimeForaId = @timeCasaId AND p.PlacarFora > p.PlacarCasa)
                                     THEN 1 ELSE 0 END), 0) AS VitoriasCasa,
                   COALESCE(SUM(CASE WHEN (p.TimeCasaId = @timeForaId AND p.PlacarCasa > p.PlacarFora)
                                       OR (p.TimeForaId = @timeForaId AND p.PlacarFora > p.PlacarCasa)
                                     THEN 1 ELSE 0 END), 0) AS VitoriasFora,
                   COALESCE(SUM(CASE WHEN p.PlacarCasa = p.PlacarFora THEN 1 ELSE 0 END), 0) AS Empates
            FROM Partida p
            WHERE " + FiltroConfronto, parametros);

        // 2) Lista dos 5 últimos confrontos.
        var ultimos = con.Query<PartidaResumo>(
            SelectResumo + " WHERE " + FiltroConfronto + " ORDER BY p.DataHora DESC LIMIT 5",
            parametros).ToList();

        return new RetrospectoConfronto
        {
            Jogos = totais.Jogos,
            VitoriasCasa = totais.VitoriasCasa,
            VitoriasFora = totais.VitoriasFora,
            Empates = totais.Empates,
            Ultimos = ultimos
        };
    }
}
