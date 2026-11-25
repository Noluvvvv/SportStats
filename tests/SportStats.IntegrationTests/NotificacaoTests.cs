using Dapper;
using MySqlConnector;
using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Repositories.Dapper;

namespace SportStats.IntegrationTests;

[Collection(nameof(MySqlFixture))]
public sealed class NotificacaoTests(MySqlFixture banco)
{
    private readonly NotificacaoRepository _repositorio = new(banco.Factory);

    [Fact]
    public async Task CriacaoUsaPadroesDoBancoEPreservaReferenciasOpcionais()
    {
        var usuario = await banco.CriarUsuario();
        var semReferencia = Aviso(usuario);
        semReferencia.Lida = true;
        semReferencia.CriadaEm = new DateTime(2000, 1, 1);
        var semReferenciaId = _repositorio.Criar(semReferencia);
        var externa = Aviso(usuario);
        externa.Fonte = "PandaScore";
        externa.PartidaIdExterno = "00042";
        var externaId = _repositorio.Criar(externa);

        var listadas = _repositorio.ListarPorUsuario(usuario, 100).ToDictionary(item => item.Id);
        Assert.Equal(2, listadas.Count);
        Assert.All(listadas.Values, item =>
        {
            Assert.Equal(usuario, item.UsuarioId);
            Assert.False(item.Lida);
            Assert.Equal("Sua partida vai começar.", item.Mensagem);
            Assert.True(item.CriadaEm > semReferencia.CriadaEm);
        });
        Assert.Null(listadas[semReferenciaId].Fonte);
        Assert.Null(listadas[semReferenciaId].PartidaIdExterno);
        Assert.Equal("PandaScore", listadas[externaId].Fonte);
        Assert.Equal("00042", listadas[externaId].PartidaIdExterno);
    }

    [Fact]
    public async Task MarcaLeituraDeFormaIdempotenteApenasParaOTitular()
    {
        var usuario = await banco.CriarUsuario();
        var outroUsuario = await banco.CriarUsuario();
        var id = _repositorio.Criar(Aviso(usuario));
        var outroId = _repositorio.Criar(Aviso(outroUsuario));
        var original = Assert.Single(_repositorio.ListarNaoLidasPorUsuario(usuario, 100));

        _repositorio.MarcarComoLida(outroUsuario, id);
        Assert.Equal(id, Assert.Single(_repositorio.ListarNaoLidasPorUsuario(usuario, 100)).Id);

        _repositorio.MarcarComoLida(usuario, id);
        _repositorio.MarcarComoLida(usuario, id);
        Assert.Empty(_repositorio.ListarNaoLidasPorUsuario(usuario, 100));
        Assert.Equal(outroId, Assert.Single(_repositorio.ListarNaoLidasPorUsuario(outroUsuario, 100)).Id);

        var lida = Assert.Single(_repositorio.ListarPorUsuario(usuario, 100));
        Assert.Equal(id, lida.Id);
        Assert.True(lida.Lida);
        Assert.Equal(original.CriadaEm, lida.CriadaEm);
    }

    [Fact]
    public async Task OrdenaEAplicaFiltroAntesDoLimite()
    {
        var usuario = await banco.CriarUsuario();
        var aviso = Aviso(usuario);
        var primeiro = _repositorio.Criar(aviso);
        var segundo = _repositorio.Criar(aviso);
        var terceiro = _repositorio.Criar(aviso);
        var empate = new DateTime(2025, 1, 2);
        var maisRecente = empate.AddSeconds(1);
        await using var conexao = banco.CriarConexao();
        await conexao.ExecuteAsync("UPDATE Notificacao SET CriadaEm = @empate WHERE UsuarioId = @usuario",
            new { empate, usuario });
        await conexao.ExecuteAsync(
            "UPDATE Notificacao SET CriadaEm = @maisRecente WHERE UsuarioId = @usuario AND Id = @primeiro",
            new { maisRecente, usuario, primeiro });
        _repositorio.MarcarComoLida(usuario, primeiro);

        Assert.Equal(new[] { primeiro, terceiro, segundo }, _repositorio.ListarPorUsuario(usuario, 100).Select(item => item.Id));
        Assert.Equal(terceiro, Assert.Single(_repositorio.ListarNaoLidasPorUsuario(usuario, 1)).Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task RejeitaLimitesInvalidos(int limite)
    {
        var usuario = await banco.CriarUsuario();
        Assert.Throws<DominioException>(() => _repositorio.ListarPorUsuario(usuario, limite));
        Assert.Throws<DominioException>(() => _repositorio.ListarNaoLidasPorUsuario(usuario, limite));
    }

    [Fact]
    public async Task RejeitaUsuarioInexistente()
    {
        var usuario = await banco.CriarUsuario();
        await banco.ExcluirUsuario(usuario);

        var erro = Assert.Throws<MySqlException>(() => _repositorio.Criar(Aviso(usuario)));
        Assert.Equal(MySqlErrorCode.NoReferencedRow2, erro.ErrorCode);
        Assert.Empty(_repositorio.ListarPorUsuario(usuario, 100));
    }

    internal static Notificacao Aviso(int usuario) =>
        new() { UsuarioId = usuario, Tipo = "InicioPartida", Mensagem = "Sua partida vai começar." };
}
