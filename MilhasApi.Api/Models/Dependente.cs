namespace MilhasApi.Api.Models;

public class Dependente : IComOrigem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public string Nome { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public string? Parentesco { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public string? OrigemId { get; set; }
}
