namespace MilhasApi.Api.Models;

public class ProgramaFidelidade
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string CompanhiaAerea { get; set; } = string.Empty;
    public string CorHex { get; set; } = "#6366f1";
    public bool Ativo { get; set; } = true;

    public ICollection<ContaMilhas> Contas { get; set; } = new List<ContaMilhas>();
}
