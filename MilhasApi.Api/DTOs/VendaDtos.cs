using System.ComponentModel.DataAnnotations;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.DTOs;

public record VendaRequest(
    [Required] int ClienteId,
    [Required] int ContaMilhasId,
    [Required, MaxLength(100)] string CompanhiaAerea,
    string? Localizador,
    [Required, MaxLength(60)] string Origem,
    [Required, MaxLength(60)] string Destino,
    DateTime? DataViagem,
    DateTime DataEmissao,
    [Range(1, long.MaxValue)] long QuantidadeMilhasUtilizadas,
    [Range(0, double.MaxValue)] decimal TaxaEmbarque,
    [Range(0, double.MaxValue)] decimal ValorVenda,
    [Range(0, double.MaxValue)] decimal ValorMercado,
    string? Hotel,
    StatusVenda Status,
    string? Observacoes
);

public record VendaStatusRequest(StatusVenda Status);

public record VendaResponse(
    int Id, int ClienteId, string ClienteNome, int ContaMilhasId, string ContaTitular,
    string ProgramaNome, string CompanhiaAerea, string? Localizador, string Origem, string Destino,
    DateTime? DataViagem, DateTime DataEmissao, long QuantidadeMilhasUtilizadas,
    decimal TaxaEmbarque, decimal ValorVenda, decimal CustoMilhasNaVenda, decimal LucroEstimado,
    decimal ValorMercado, decimal Economia, string? Hotel,
    StatusVenda Status, string? Observacoes
);
