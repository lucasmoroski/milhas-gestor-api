namespace MilhasApi.Api.Models;

public class ContaMilhas
{
    public int Id { get; set; }

    public int ProgramaFidelidadeId { get; set; }
    public ProgramaFidelidade? Programa { get; set; }

    public string NomeTitular { get; set; } = string.Empty;
    public string CpfTitular { get; set; } = string.Empty;
    public string? LoginAcesso { get; set; }

    public long SaldoMilhas { get; set; }
    public decimal CustoMedioMilheiro { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<CompraMilhas> Compras { get; set; } = new List<CompraMilhas>();
    public ICollection<VendaEmissao> Vendas { get; set; } = new List<VendaEmissao>();
}
