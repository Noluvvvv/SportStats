// TODO [BACK-04] Estatisticas polimorficas e rankings  (manual: R3, R5)
//
// 1. Criar a classe abstrata Estatistica (AtletaId) com o metodo abstrato CalcularRating().
// 2. Subclasses (tabelas ja existem no script.sql):
//      EstatisticaFutebol  (Gols, ChutesAGol, Escanteios, Cartoes, Finalizacoes)
//      EstatisticaBasquete (Pontos, Assistencias, MediaPontos, Faltas, Rebotes)
//      EstatisticaValorant (Abates, Assistencias, Agente)
//      EstatisticaCsGo     (Abates, Assistencias, Headshots)
//    Cada uma sobrescreve CalcularRating() com a sua formula.
// 3. Repositorio generico: IRepository<T> where T : Estatistica, uma implementacao Dapper por tabela.
// 4. Servico de artilharia/ranking (Services/RankingService.cs) usando List<T> tipadas.
// 5. Exibir o ranking na coluna da direita de Views/Home/Index.cshtml
//    (ha um comentario BACKEND marcando o lugar).