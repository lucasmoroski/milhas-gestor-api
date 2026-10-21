using System.ComponentModel.DataAnnotations;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.DTOs;

public record CompraRequest(
    [Required] int ContaMilhasId,
    [Required, MaxLength(150)] string FornecedorNome,
    [Range(1, long.MaxValue)] long QuantidadeMilhas,
    [Range(0, double.MaxValue)] decimal ValorTotal,
    string? FormaPagamento,
    DateTime DataCompra,
    StatusCompra Status,
    string? Observacoes
);

public record CompraResponse(
    int Id, int ContaMilhasId, string ContaTitular, string ProgramaNome,
    string FornecedorNome, long QuantidadeMilhas, decimal ValorTotal,
    decimal CustoPorMilheiro, string? FormaPagamento, DateTime DataCompra,
    StatusCompra Status, string? Observacoes
);
