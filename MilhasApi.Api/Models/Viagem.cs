namespace MilhasApi.Api.Models;

public class Viagem : IComOrigem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public string Nome { get; set; } = string.Empty;
    public StatusViagem Status { get; set; } = StatusViagem.Planejada;
    public string? Localizador { get; set; }
    public string? Companhia { get; set; }
    public string? Hotel { get; set; }

    public string? Origem { get; set; }
    public string? Destino { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool SomenteIda { get; set; }

    public TipoPagamentoViagem TipoPagamento { get; set; } = TipoPagamentoViagem.Pagante;
    public int? ProgramaFidelidadeId { get; set; }
    public ProgramaFidelidade? Programa { get; set; }
    public long? QuantidadeMilhas { get; set; }
    public decimal? ValorPago { get; set; }
    public decimal? TaxaEmbarque { get; set; }
    public decimal? Orcamento { get; set; }

    public string? Observacoes { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    public string? OrigemId { get; set; }
}
