namespace MilhasApi.Api.Models;

public class Cliente : IComOrigem
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public DateTime? DataRenovacao { get; set; }
    public decimal? ValorGestao { get; set; }

    public int? GestorId { get; set; }
    public Usuario? Gestor { get; set; }
    public string? OrigemId { get; set; }

    public ICollection<VendaEmissao> Vendas { get; set; } = new List<VendaEmissao>();
    public ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
    public ICollection<Viagem> Viagens { get; set; } = new List<Viagem>();
    public ICollection<ClienteMilhas> Milhas { get; set; } = new List<ClienteMilhas>();
    public ICollection<Dependente> Dependentes { get; set; } = new List<Dependente>();
    public ICollection<EconomiaRegistro> Economias { get; set; } = new List<EconomiaRegistro>();
    public ICollection<Nota> Notas { get; set; } = new List<Nota>();
}
