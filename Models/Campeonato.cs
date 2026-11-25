using SportStats.Exceptions;

namespace SportStats.Models;

public class Campeonato
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = "";
    public int Temporada { get; private set; }
    public int ModalidadeId { get; private set; }

    public string ModalidadeNome { get; private set; } = "";

    private Campeonato() { } 

    public Campeonato(string nome, int temporada, int modalidadeId) => Atualizar(nome, temporada, modalidadeId);

    public void Atualizar(string nome, int temporada, int modalidadeId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do campeonato ├® obrigat├│rio.");
        if (nome.Trim().Length > 100)
            throw new DominioException("O nome do campeonato pode ter no m├íximo 100 caracteres.");
        if (temporada is < 1900 or > 2100)
            throw new DominioException("A temporada deve estar entre 1900 e 2100.");
        if (modalidadeId <= 0)
            throw new DominioException("Escolha uma modalidade.");

        Nome = nome.Trim();
        Temporada = temporada;
        ModalidadeId = modalidadeId;
    }
}
