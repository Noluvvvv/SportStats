using SportStats.Exceptions;

namespace SportStats.Models;

public class Atleta
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = "";
    public string? Nickname { get; private set; }
    public string Nacionalidade { get; private set; } = "";
    public int ModalidadeId { get; private set; }

    private readonly List<Transferencia> _transferencias = new();

    public IReadOnlyCollection<Transferencia> Transferencias => _transferencias.AsReadOnly();

    private Atleta() { }



    internal void DefinirId(int id)
    {
        if (id <= 0)
            throw new DominioException("O ID do atleta é inválido.");

        if (Id > 0)
            throw new DominioException("O atleta já possui um ID.");

        Id = id;
    }



    public Atleta(string nome, string? nickname, string nacionalidade, int modalidadeId)
    {
        Atualizar(nome, nickname, nacionalidade, modalidadeId);
    }



    public void Atualizar(string nome, string? nickname, string nacionalidade, int modalidadeId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do atleta é obrigatório.");

        if (nome.Trim().Length > 100)
            throw new DominioException("O nome do atleta pode ter no máximo 100 caracteres.");

        if (nickname?.Trim().Length > 50)
            throw new DominioException("O nickname pode ter no máximo 50 caracteres.");

        if (string.IsNullOrWhiteSpace(nacionalidade))
            throw new DominioException("A nacionalidade do atleta é obrigatória.");

        if (nacionalidade.Trim().Length > 50)
            throw new DominioException("A nacionalidade pode ter no máximo 50 caracteres.");

        if (modalidadeId <= 0)
            throw new DominioException("Escolha uma modalidade.");

        Nome = nome.Trim();
        Nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim();
        Nacionalidade = nacionalidade.Trim();
        ModalidadeId = modalidadeId;
    }




    public void Transferir(int timeId, DateTime dataInicio)
    {
        if (Id <= 0)
            throw new DominioException("Salve o atleta antes de registrar uma transferência.");

        var transferenciasAtivas = _transferencias
            .Where(t => t.DataFim == null)
            .ToList();

        if (transferenciasAtivas.Count > 1)
            throw new AtletaEmDoisTimesException(
                "O atleta possui mais de um vínculo ativo com times.");

        var transferenciaAtiva = transferenciasAtivas.SingleOrDefault();

        if (transferenciaAtiva != null && transferenciaAtiva.TimeId == timeId)
            throw new DominioException("O atleta já está vinculado a este time.");

        var novaTransferencia = new Transferencia(Id, timeId, dataInicio);

        if (transferenciaAtiva != null)
            transferenciaAtiva.Encerrar(dataInicio);

        _transferencias.Add(novaTransferencia);
    }





    internal void CarregarTransferencia(Transferencia transferencia)
    {
        if (Id <= 0)
            throw new DominioException("O atleta precisa estar salvo para carregar seu histórico.");

        if (transferencia.AtletaId != Id)
            throw new DominioException("A transferência não pertence a este atleta.");

        if (_transferencias.Any(t => t.Id == transferencia.Id))
            throw new DominioException("Esta transferência já foi carregada.");

        if (transferencia.DataFim == null &&
            _transferencias.Any(t => t.DataFim == null))
        {
            throw new AtletaEmDoisTimesException(
                "O histórico possui mais de um vínculo ativo com times.");
        }

        _transferencias.Add(transferencia);
    }
}

