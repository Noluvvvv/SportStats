using SportStats.Exceptions;

namespace SportStats.Models;

public class Time
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = "";
    public string Sigla { get; private set; } = "";
    public string Cidade { get; private set; } = "";

    private Time() { } // usado pelo Dapper

    public Time(string nome, string sigla, string cidade) => Atualizar(nome, sigla, cidade);

    public void Atualizar(string nome, string sigla, string cidade)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do time é obrigatório.");
        if (string.IsNullOrWhiteSpace(sigla) || sigla.Trim().Length is < 2 or > 4)
            throw new DominioException("A sigla deve ter de 2 a 4 caracteres.");
        if (string.IsNullOrWhiteSpace(cidade))
            throw new DominioException("A cidade é obrigatória.");

        Nome = nome.Trim();
        Sigla = sigla.Trim().ToUpperInvariant();
        Cidade = cidade.Trim();
    }
}