namespace MilhasApi.Api.Models;

public class LancamentoFinanceiro
{
    public int Id { get; set; }
    public TipoLancamento Tipo { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public StatusLancamento Status { get; set; } = StatusLancamento.Pendente;

    public int? VendaEmissaoId { get; set; }
    public VendaEmissao? VendaEmissao { get; set; }

    public int? CompraMilhasId { get; set; }
    public CompraMilhas? CompraMilhas { get; set; }
}
