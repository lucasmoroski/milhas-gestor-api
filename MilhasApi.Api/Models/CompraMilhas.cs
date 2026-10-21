namespace MilhasApi.Api.Models;

public class CompraMilhas
{
    public int Id { get; set; }

    public int ContaMilhasId { get; set; }
    public ContaMilhas? Conta { get; set; }

    public string FornecedorNome { get; set; } = string.Empty;
    public long QuantidadeMilhas { get; set; }
    public decimal ValorTotal { get; set; }
    public string? FormaPagamento { get; set; }
    public DateTime DataCompra { get; set; } = DateTime.UtcNow;
    public StatusCompra Status { get; set; } = StatusCompra.Confirmada;
    public string? Observacoes { get; set; }

    public decimal CustoPorMilheiro => QuantidadeMilhas > 0
        ? Math.Round(ValorTotal / (QuantidadeMilhas / 1000m), 2)
        : 0;
}
