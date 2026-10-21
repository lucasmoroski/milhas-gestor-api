using System.ComponentModel.DataAnnotations;

namespace MilhasApi.Api.DTOs;

public record ClienteRequest(
    [Required, MaxLength(150)] string Nome,
    [EmailAddress] string? Email,
    string? Telefone,
    string? Cpf,
    string? Observacoes,
    DateTime? DataRenovacao,
    decimal? ValorGestao
);

public record ClienteResponse(
    int Id, string Nome, string? Email, string? Telefone, string? Cpf, string? Observacoes,
    DateTime CriadoEm, DateTime? DataRenovacao, decimal? ValorGestao,
    long TotalMilhas, decimal EconomiaTotal,
    string? ProximaViagem, string? ProximaViagemStatus, string Status
);
