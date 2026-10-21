using System.ComponentModel.DataAnnotations;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.DTOs;

public record LancamentoRequest(
    TipoLancamento Tipo,
    [Required, MaxLength(100)] string Categoria,
    [Required, MaxLength(300)] string Descricao,
    [Range(0, double.MaxValue)] decimal Valor,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    StatusLancamento Status,
    int? VendaEmissaoId,
    int? CompraMilhasId
);

public record LancamentoResponse(
    int Id, TipoLancamento Tipo, string Categoria, string Descricao, decimal Valor,
    DateTime DataVencimento, DateTime? DataPagamento, StatusLancamento Status,
    int? VendaEmissaoId, int? CompraMilhasId
);
