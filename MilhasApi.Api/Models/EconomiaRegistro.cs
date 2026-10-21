namespace MilhasApi.Api.Models;

public class EconomiaRegistro : IComOrigem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? ViagemId { get; set; }
    public Viagem? Viagem { get; set; }

    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public string? OrigemId { get; set; }
}
