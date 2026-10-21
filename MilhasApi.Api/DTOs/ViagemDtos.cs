using System.ComponentModel.DataAnnotations;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.DTOs;

public record ViagemRequest(
    [Required] int ClienteId,
    [Required, MaxLength(200)] string Nome,
    StatusViagem Status,
    [MaxLength(30)] string? Localizador,
    [MaxLength(100)] string? Companhia,
    [MaxLength(200)] string? Hotel,
    [MaxLength(200)] string? Origem,
    [MaxLength(200)] string? Destino,
    DateTime? DataInicio,
    DateTime? DataFim,
    bool SomenteIda,
    TipoPagamentoViagem TipoPagamento,
    int? ProgramaFidelidadeId,
    [Range(0, long.MaxValue)] long? QuantidadeMilhas,
    [Range(0, double.MaxValue)] decimal? ValorPago,
    [Range(0, double.MaxValue)] decimal? TaxaEmbarque,
    [Range(0, double.MaxValue)] decimal? Orcamento,
    string? Observacoes
);

public record ViagemStatusRequest(StatusViagem Status);

public record ViagemResponse(
    int Id, int ClienteId, string ClienteNome, string Nome, StatusViagem Status,
    string? Localizador, string? Companhia, string? Hotel, string? Origem, string? Destino,
    DateTime? DataInicio, DateTime? DataFim, bool SomenteIda, TipoPagamentoViagem TipoPagamento,
    int? ProgramaFidelidadeId, string? ProgramaNome, long? QuantidadeMilhas,
    decimal? ValorPago, decimal? TaxaEmbarque, decimal? Orcamento, string? Observacoes,
    DateTime CriadoEm, DateTime AtualizadoEm
);
