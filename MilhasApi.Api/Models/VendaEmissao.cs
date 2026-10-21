namespace MilhasApi.Api.Models;

public class VendaEmissao
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int ContaMilhasId { get; set; }
    public ContaMilhas? Conta { get; set; }

    public string CompanhiaAerea { get; set; } = string.Empty;
    public string? Localizador { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public DateTime? DataViagem { get; set; }
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;

    public long QuantidadeMilhasUtilizadas { get; set; }
    public decimal TaxaEmbarque { get; set; }
    public decimal ValorVenda { get; set; }
    public decimal CustoMilhasNaVenda { get; set; }
    public decimal ValorMercado { get; set; }
    public string? Hotel { get; set; }

    public StatusVenda Status { get; set; } = StatusVenda.Pendente;
    public string? Observacoes { get; set; }

    public decimal LucroEstimado => ValorVenda - CustoMilhasNaVenda;
    public decimal Economia => Math.Max(0, ValorMercado - ValorVenda - TaxaEmbarque);

    public static bool StatusDebitaSaldo(StatusVenda status) => status is StatusVenda.Emitida or StatusVenda.Finalizada;
}
