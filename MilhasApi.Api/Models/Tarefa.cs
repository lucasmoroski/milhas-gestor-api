namespace MilhasApi.Api.Models;

public class Tarefa
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataPrazo { get; set; }
    public bool Concluida { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
