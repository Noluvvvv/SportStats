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
// 4. Estrutura de estatisticas tipadas para modalidades esportivas.
// 5. Exibir o ranking na coluna da direita de Views/Home/Index.cshtml
//    (ha um comentario BACKEND marcando o lugar).





using SportStats.Exceptions;

namespace SportStats.Models;

public abstract class Estatistica
{
    public int Id { get; private set; }
    public int AtletaId { get; private set; }

    protected Estatistica(int atletaId, int id = 0)
    {
        if (atletaId <= 0)
            throw new DominioException("O ID do atleta ├® inv├ílido.");

        if (id < 0)
            throw new DominioException("O ID da estat├¡stica ├® inv├ílido.");

        AtletaId = atletaId;
        Id = id;
    }

    public abstract double CalcularRating();

    internal void DefinirId(int id)
    {
        if (id <= 0)
            throw new DominioException("O ID da estat├¡stica ├® inv├ílido.");

        if (Id > 0)
            throw new DominioException("A estat├¡stica j├í possui um ID.");

        Id = id;
    }
}

