namespace MilhasApi.Api.DTOs;

public record DashboardResumo(
    int TotalClientes,
    long MilhasTotais,
    decimal EconomiaTotal,
    int ViagensAtivas,
    int ViagensTotais,
    int ViagensFinalizadas,
    int TaxaConclusao,
    decimal OrcamentoTotal,
    decimal EconomiaMedia,
    List<RenovacaoCliente> Renovacoes,
    List<ProximaViagem> ProximasViagens,
    List<TarefaResponse> TarefasProximas
);

public record RenovacaoCliente(int ClienteId, string Nome, string? Email, DateTime DataRenovacao, int DiasRestantes);

public record ProximaViagem(
    int ViagemId, string Titulo, string ClienteNome, DateTime? DataInicio,
    string? Localizador, string? Hotel, string Status
);
