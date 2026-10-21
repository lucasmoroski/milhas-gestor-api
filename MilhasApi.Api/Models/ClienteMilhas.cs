namespace MilhasApi.Api.Models;

public class ClienteMilhas : IComOrigem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int ProgramaFidelidadeId { get; set; }
    public ProgramaFidelidade? Programa { get; set; }

    public long Quantidade { get; set; }
    public DateTime? DataExpiracao { get; set; }
    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;

    public string? Login { get; set; }
    public string? SenhaCriptografada { get; set; }
    public string? OrigemId { get; set; }
}
