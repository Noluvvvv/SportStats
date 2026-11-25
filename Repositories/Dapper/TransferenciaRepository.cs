using SportStats.Models;
using SportStats.Repositories.Interfaces;
using Dapper;
using SportStats.Exceptions;
using MySqlConnector;

namespace SportStats.Repositories.Dapper;

public class TransferenciaRepository : ITransferenciaRepository
{
    private readonly IDbConnectionFactory _factory;

    public TransferenciaRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public void SalvarHistorico(Atleta atleta)
    {
        if (atleta == null)
            throw new ArgumentNullException(nameof(atleta));

        if (atleta.Id <= 0)
            throw new DominioException("Salve o atleta antes de registrar transferências.");

        using var con = _factory.Criar();

        con.Open();

        using var transacao = con.BeginTransaction();
        var novasTransferencias = new List<(Transferencia Transferencia, int Id)>();

        try
        {
            foreach (var transferencia in atleta.Transferencias)
            {
                if (transferencia.Id > 0 && transferencia.DataFim.HasValue)
                {
                    con.Execute(
                        @"UPDATE Transferencia
                      SET DataFim = @DataFim
                      WHERE Id = @Id AND AtletaId = @AtletaId",
                        new
                        {
                            transferencia.DataFim,
                            transferencia.Id,
                            AtletaId = atleta.Id
                        },
                        transacao);
                }
            }

            foreach (var transferencia in atleta.Transferencias)
            {
                if (transferencia.Id == 0)
                {
                    var id = con.ExecuteScalar<int>(
                        @"INSERT INTO Transferencia
                        (AtletaId, TimeId, DataInicio, DataFim)
                        VALUES
                        (@AtletaId, @TimeId, @DataInicio, @DataFim);
                        SELECT LAST_INSERT_ID();",
                        new
                        {
                            transferencia.AtletaId,
                            transferencia.TimeId,
                            transferencia.DataInicio,
                            transferencia.DataFim
                        },
                        transacao);

                    novasTransferencias.Add((transferencia, id));
                }
            }

            transacao.Commit();

            foreach (var nova in novasTransferencias)
            {
                nova.Transferencia.DefinirId(nova.Id);
            }
        }
        catch (MySqlException ex) when (
            ex.ErrorCode is MySqlErrorCode.NoReferencedRow2
                or MySqlErrorCode.NoReferencedRow)
        {
            transacao.Rollback();

            throw new DominioException(
                "Não foi possível registrar a transferência. Verifique se o atleta e o time existem.");
        }
    }
}