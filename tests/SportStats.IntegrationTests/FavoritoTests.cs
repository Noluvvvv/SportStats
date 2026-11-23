using Dapper;
using MySqlConnector;
using SportStats.Exceptions;
using SportStats.Repositories.Dapper;
using SportStats.Services;

namespace SportStats.IntegrationTests;

[Collection(nameof(MySqlFixture))]
public sealed class FavoritoTests(MySqlFixture banco)
{
    private readonly FavoritoRepository _repositorio = new(banco.Factory);

    [Fact]
    public async Task SeguirERemoverSaoIdempotentes()
    {
        var usuario = await banco.CriarUsuario();
        var servico = new FavoritoService(_repositorio);

        servico.Seguir(usuario, "partida", "ESPN", "00042", "Original");
        var original = Assert.Single(_repositorio.ListarPorUsuario(usuario));
        Assert.Equal("Original", original.Nome);
        Assert.NotEqual(default, original.CriadoEm);
        Assert.True(_repositorio.Existe(usuario, "partida", "ESPN", "00042"));

        // Evita depender da granularidade de segundos do relógio durante duas inclusões rápidas.
        var criadaEm = new DateTime(2025, 1, 2);
        await using var conexao = banco.CriarConexao();
        await conexao.ExecuteAsync("UPDATE Favorito SET CriadoEm = @criadaEm WHERE UsuarioId = @usuario",
            new { criadaEm, usuario });

        servico.Seguir(usuario, "partida", "ESPN", "00042", "Outro nome");
        var repetido = Assert.Single(_repositorio.ListarPorUsuario(usuario));
        Assert.Equal(criadaEm, repetido.CriadoEm);
        Assert.Equal("Original", repetido.Nome);

        servico.DeixarDeSeguir(usuario, "partida", "ESPN", "00042");
        servico.DeixarDeSeguir(usuario, "partida", "ESPN", "00042");
        Assert.False(_repositorio.Existe(usuario, "partida", "ESPN", "00042"));
        Assert.Empty(_repositorio.ListarPorUsuario(usuario));
    }

    [Fact]
    public async Task ChaveCompostaPreservaTipoFonteEUsuario()
    {
        var usuario = await banco.CriarUsuario();
        var outroUsuario = await banco.CriarUsuario();
        var servico = new FavoritoService(_repositorio);

        servico.Seguir(usuario, "partida", "ESPN", "42");
        servico.Seguir(usuario, "partida", "PandaScore", "42");
        servico.Seguir(usuario, "time", "ESPN", "42");
        servico.Seguir(outroUsuario, "partida", "ESPN", "42", "Outro usuário");

        Assert.Equal(3, _repositorio.ListarPorUsuario(usuario).Count());
        Assert.Equal("Outro usuário", Assert.Single(_repositorio.ListarPorUsuario(outroUsuario)).Nome);
        Assert.False(_repositorio.Existe(outroUsuario, "time", "ESPN", "42"));

        servico.DeixarDeSeguir(usuario, "partida", "ESPN", "42");
        Assert.False(_repositorio.Existe(usuario, "partida", "ESPN", "42"));
        Assert.True(_repositorio.Existe(usuario, "partida", "PandaScore", "42"));
        Assert.True(_repositorio.Existe(usuario, "time", "ESPN", "42"));
        Assert.True(_repositorio.Existe(outroUsuario, "partida", "ESPN", "42"));
        Assert.All(_repositorio.ListarPorUsuario(usuario), favorito => Assert.Null(favorito.Nome));
    }

    [Fact]
    public async Task DuasInclusoesConcorrentesGravamUmFavorito()
    {
        var usuario = await banco.CriarUsuario();
        var servico = new FavoritoService(_repositorio);
        using var barreira = new Barrier(2);

        Task SeguirEmParalelo() => Task.Factory.StartNew(() =>
        {
            if (!barreira.SignalAndWait(TimeSpan.FromSeconds(10)))
                throw new TimeoutException("As duas inclusões não chegaram à barreira.");

            servico.Seguir(usuario, "partida", "ESPN", "concorrente");
        }, TestContext.Current.CancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        await Task.WhenAll(SeguirEmParalelo(), SeguirEmParalelo());
        Assert.Equal("concorrente", Assert.Single(_repositorio.ListarPorUsuario(usuario)).IdExterno);
    }

    [Theory]
    [InlineData("jogador", "ESPN", "42")]
    [InlineData("time", "espn", "42")]
    [InlineData("time", "ESPN", " ")]
    [InlineData("time", "ESPN", null)]
    public async Task RejeitaChavesInvalidas(string tipo, string fonte, string? id)
    {
        var usuario = await banco.CriarUsuario();
        var servico = new FavoritoService(_repositorio);

        Assert.Throws<DominioException>(() => servico.Seguir(usuario, tipo, fonte, id!));
        Assert.Throws<DominioException>(() => servico.DeixarDeSeguir(usuario, tipo, fonte, id!));
        Assert.Empty(_repositorio.ListarPorUsuario(usuario));
    }

    [Fact]
    public async Task RejeitaUsuarioInvalidoETextosAcimaDoLimite()
    {
        var usuario = await banco.CriarUsuario();
        var servico = new FavoritoService(_repositorio);

        Assert.Throws<DominioException>(() => servico.Seguir(0, "time", "ESPN", "42"));
        Assert.Throws<DominioException>(() => servico.DeixarDeSeguir(0, "time", "ESPN", "42"));
        Assert.Throws<DominioException>(() => servico.Seguir(usuario, "time", "ESPN", new string('X', 61)));
        Assert.Throws<DominioException>(() => servico.Seguir(usuario, "time", "ESPN", "42", new string('X', 121)));
        Assert.Empty(_repositorio.ListarPorUsuario(usuario));
    }

    [Fact]
    public async Task ViolacaoDeChaveEstrangeiraEPropagada()
    {
        var usuario = await banco.CriarUsuario();
        await banco.ExcluirUsuario(usuario);
        var servico = new FavoritoService(_repositorio);

        var erro = Assert.Throws<MySqlException>(() => servico.Seguir(usuario, "time", "ESPN", "42"));
        Assert.Equal(MySqlErrorCode.NoReferencedRow2, erro.ErrorCode);
        Assert.Empty(_repositorio.ListarPorUsuario(usuario));
    }
}
