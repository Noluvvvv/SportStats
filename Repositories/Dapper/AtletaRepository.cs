using SportStats.Models;
using SportStats.Repositories.Interfaces;
using Dapper;
using SportStats.Exceptions;
using MySqlConnector;


namespace SportStats.Repositories.Dapper;

public class AtletaRepository : IRepository<Atleta>
{

    private sealed class DadosAtleta
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string? Nickname { get; set; }
        public string Nacionalidade { get; set; } = "";
        public int ModalidadeId { get; set; }
    }


    private sealed class DadosTransferencia
    {
        public int Id { get; set; }
        public int AtletaId { get; set; }
        public int TimeId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }




    private static Atleta Montar(
        DadosAtleta dados,
        IEnumerable<DadosTransferencia> transferencias)
    {
        var atleta = new Atleta(
            dados.Nome,
            dados.Nickname,
            dados.Nacionalidade,
            dados.ModalidadeId);

        atleta.DefinirId(dados.Id);

        foreach (var dadosTransferencia in transferencias)
        {
            var transferencia = new Transferencia(
                dadosTransferencia.Id,
                dadosTransferencia.AtletaId,
                dadosTransferencia.TimeId,
                dadosTransferencia.DataInicio,
                dadosTransferencia.DataFim);

            atleta.CarregarTransferencia(transferencia);
        }

        return atleta;
    }




    private readonly IDbConnectionFactory _factory;

    public AtletaRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }





    public IEnumerable<Atleta> ObterTodos()
    {
        using var con = _factory.Criar();

        var atletas = con.Query<DadosAtleta>(
                @"SELECT Id, Nome, Nickname, Nacionalidade, ModalidadeId
          FROM Atleta
          ORDER BY Nome")
            .ToList();

        var transferencias = con.Query<DadosTransferencia>(
                @"SELECT Id, AtletaId, TimeId, DataInicio, DataFim
          FROM Transferencia
          ORDER BY DataInicio, Id")
            .ToList();

        return atletas.Select(atleta =>
            Montar(
                atleta,
                transferencias.Where(t => t.AtletaId == atleta.Id)
            )).ToList();
    }





    public Atleta? ObterPorId(int id)
    {
        using var con = _factory.Criar();

        var dados = con.QuerySingleOrDefault<DadosAtleta>(
            @"SELECT Id, Nome, Nickname, Nacionalidade, ModalidadeId
          FROM Atleta
          WHERE Id = @id",
            new { id });

        if (dados == null)
            return null;

        var transferencias = con.Query<DadosTransferencia>(
            @"SELECT Id, AtletaId, TimeId, DataInicio, DataFim
          FROM Transferencia
          WHERE AtletaId = @id
          ORDER BY DataInicio, Id",
            new { id });

        return Montar(dados, transferencias);
    }






    public int Inserir(Atleta entidade)
    {
        using var con = _factory.Criar();

        var id = con.ExecuteScalar<int>(
            @"INSERT INTO Atleta (Nome, Nickname, Nacionalidade, ModalidadeId)
          VALUES (@Nome, @Nickname, @Nacionalidade, @ModalidadeId);
          SELECT LAST_INSERT_ID();",
            new
            {
                entidade.Nome,
                entidade.Nickname,
                entidade.Nacionalidade,
                entidade.ModalidadeId
            });

        entidade.DefinirId(id);

        return id;
    }






    public void Atualizar(Atleta entidade)
    {
        using var con = _factory.Criar();

        con.Execute(
            @"UPDATE Atleta
          SET Nome = @Nome,
              Nickname = @Nickname,
              Nacionalidade = @Nacionalidade,
              ModalidadeId = @ModalidadeId
          WHERE Id = @Id",
            new
            {
                entidade.Nome,
                entidade.Nickname,
                entidade.Nacionalidade,
                entidade.ModalidadeId,
                entidade.Id
            });
    }





    public void Remover(int id)
    {
        try
        {
            using var con = _factory.Criar();

            con.Execute(
                "DELETE FROM Atleta WHERE Id = @id",
                new { id });
        }
        catch (MySqlException ex) when (
            ex.ErrorCode is MySqlErrorCode.RowIsReferenced2
                or MySqlErrorCode.RowIsReferenced)
        {
            throw new DominioException(
                "Não é possível excluir um atleta que possui transferências registradas.");
        }
    }
}