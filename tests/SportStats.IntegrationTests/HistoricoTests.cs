using Dapper;
using MySqlConnector;
using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Repositories.Dapper;

namespace SportStats.IntegrationTests;

[Collection(nameof(MySqlFixture))]
public sealed class HistoricoTests(MySqlFixture banco)
{
    private readonly HistoricoVisualizacaoRepository _repositorio = new(banco.Factory);

    [Fact]
    public async Task RegistraVisitasRepetidasComIdsPropriosEIsolaUsuarios()
    {
        var usuario = await banco.CriarUsuario();
        var outroUsuario = await banco.CriarUsuario();
        var visita = Visita(usuario);

        var primeiro = _repositorio.Registrar(visita);
        var segundo = _repositorio.Registrar(visita);
        var deOutroUsuario = _repositorio.Registrar(Visita(outroUsuario));

        Assert.NotEqual(primeiro, segundo);
        var listadas = _repositorio.ListarRecentesPorUsuario(usuario, 100).ToArray();
        Assert.Equal(new[] { segundo, primeiro }, listadas.Select(item => item.Id));
        Assert.All(listadas, item =>
        {
            Assert.Equal(usuario, item.UsuarioId);
            Assert.Equal("00042", item.IdExterno);
            Assert.Null(item.Nome);
            Assert.NotEqual(default, item.VisualizadoEm);
        });
        Assert.Equal(deOutroUsuario, Assert.Single(_repositorio.ListarRecentesPorUsuario(outroUsuario, 100)).Id);
    }

    [Fact]
    public async Task OrdenaPorDataEIdELimitaSemExcluir()
    {
        var usuario = await banco.CriarUsuario();
        var visita = Visita(usuario);
        var ids = Enumerable.Range(0, 102).Select(_ => _repositorio.Registrar(visita)).ToArray();
        var empate = new DateTime(2025, 1, 2);
        var maisRecente = empate.AddSeconds(1);
        var primeiro = ids[0];
        await using var conexao = banco.CriarConexao();
        await conexao.ExecuteAsync("UPDATE HistoricoVisualizacao SET VisualizadoEm = @empate WHERE UsuarioId = @usuario",
            new { empate, usuario });
        await conexao.ExecuteAsync(
            "UPDATE HistoricoVisualizacao SET VisualizadoEm = @maisRecente WHERE UsuarioId = @usuario AND Id = @primeiro",
            new { maisRecente, usuario, primeiro });

        Assert.Equal(primeiro, Assert.Single(_repositorio.ListarRecentesPorUsuario(usuario, 1)).Id);
        Assert.Equal(new[] { primeiro, ids[^1], ids[^2] },
            _repositorio.ListarRecentesPorUsuario(usuario, 3).Select(item => item.Id));
        Assert.Equal(100, _repositorio.ListarRecentesPorUsuario(usuario, 100).Count());
        Assert.Equal(102, await conexao.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM HistoricoVisualizacao WHERE UsuarioId = @usuario", new { usuario }));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(101)]
    public async Task RejeitaLimitesInvalidos(int limite)
    {
        var usuario = await banco.CriarUsuario();
        Assert.Throws<DominioException>(() => _repositorio.ListarRecentesPorUsuario(usuario, limite));
    }

    [Fact]
    public async Task RejeitaUsuarioInexistente()
    {
        var usuario = await banco.CriarUsuario();
        await banco.ExcluirUsuario(usuario);

        var erro = Assert.Throws<MySqlException>(() => _repositorio.Registrar(Visita(usuario)));
        Assert.Equal(MySqlErrorCode.NoReferencedRow2, erro.ErrorCode);
        Assert.Empty(_repositorio.ListarRecentesPorUsuario(usuario, 1));
    }

    internal static HistoricoVisualizacao Visita(int usuario) =>
        new() { UsuarioId = usuario, Tipo = "partida", Fonte = "ESPN", IdExterno = "00042" };
}
