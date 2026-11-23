using SportStats.Exceptions;
using SportStats.Models;
using SportStats.Repositories.Interfaces;

namespace SportStats.Services;

public class FavoritoService
{
    private readonly IFavoritoRepository _repository;

    public FavoritoService(IFavoritoRepository repository) => _repository = repository;

    public void Seguir(int usuarioId, string tipo, string fonte, string idExterno, string? nome = null)
    {
        ValidarChave(usuarioId, tipo, fonte, idExterno);

        if (nome is not null && nome.EnumerateRunes().Count() > 120)
            throw new DominioException("O nome pode ter no máximo 120 caracteres.");

        _repository.Inserir(new Favorito
        {
            UsuarioId = usuarioId,
            Tipo = tipo,
            Fonte = fonte,
            IdExterno = idExterno,
            Nome = nome
        });
    }

    public void DeixarDeSeguir(int usuarioId, string tipo, string fonte, string idExterno)
    {
        ValidarChave(usuarioId, tipo, fonte, idExterno);
        _repository.Remover(usuarioId, tipo, fonte, idExterno);
    }

    private static void ValidarChave(int usuarioId, string tipo, string fonte, string idExterno)
    {
        if (usuarioId <= 0)
            throw new DominioException("O ID do usuário deve ser positivo.");

        if (tipo is not ("time" or "atleta" or "competicao" or "partida"))
            throw new DominioException("O tipo de favorito é inválido.");

        if (fonte is not ("SportStats" or "ESPN" or "PandaScore"))
            throw new DominioException("A fonte do favorito é inválida.");

        if (string.IsNullOrWhiteSpace(idExterno) || idExterno.EnumerateRunes().Count() > 60)
            throw new DominioException("O ID externo é obrigatório e pode ter no máximo 60 caracteres.");
    }
}
