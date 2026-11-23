using Dapper;
using SportStats.Repositories.Dapper;
using SportStats.Services;

namespace SportStats.IntegrationTests;

[Collection(nameof(MySqlFixture))]
public sealed class IntegridadeTests(MySqlFixture banco)
{
    [Fact]
    public async Task ExclusaoDeUsuarioRemoveDadosDependentesApenasDoTitular()
    {
        var usuario = await banco.CriarUsuario();
        var outroUsuario = await banco.CriarUsuario();
        var favoritos = new FavoritoRepository(banco.Factory);
        var servico = new FavoritoService(favoritos);
        var historico = new HistoricoVisualizacaoRepository(banco.Factory);
        var notificacoes = new NotificacaoRepository(banco.Factory);
        await using var conexao = banco.CriarConexao();

        foreach (var titular in new[] { usuario, outroUsuario })
        {
            await conexao.ExecuteAsync("INSERT INTO PreferenciaUsuario (UsuarioId) VALUES (@titular)", new { titular });
            servico.Seguir(titular, "partida", "ESPN", "00042");
            historico.Registrar(HistoricoTests.Visita(titular));
            notificacoes.Criar(NotificacaoTests.Aviso(titular));
        }

        await banco.ExcluirUsuario(usuario);

        Assert.Empty(favoritos.ListarPorUsuario(usuario));
        Assert.Empty(historico.ListarRecentesPorUsuario(usuario, 100));
        Assert.Empty(notificacoes.ListarPorUsuario(usuario, 100));
        Assert.Single(favoritos.ListarPorUsuario(outroUsuario));
        Assert.Single(historico.ListarRecentesPorUsuario(outroUsuario, 100));
        Assert.Single(notificacoes.ListarPorUsuario(outroUsuario, 100));
        Assert.Equal(new[] { outroUsuario }, await conexao.QueryAsync<int>(
            "SELECT UsuarioId FROM PreferenciaUsuario WHERE UsuarioId IN (@usuario, @outroUsuario)",
            new { usuario, outroUsuario }));
    }
}
