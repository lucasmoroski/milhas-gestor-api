namespace MilhasApi.Api.Models;

public class Nota : IComOrigem
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? ViagemId { get; set; }
    public Viagem? Viagem { get; set; }

    public int? GestorId { get; set; }
    public Usuario? Gestor { get; set; }

    public string Conteudo { get; set; } = string.Empty;
    public bool VisivelAoCliente { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public string? OrigemId { get; set; }
}
