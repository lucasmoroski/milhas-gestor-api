using System.ComponentModel.DataAnnotations;

namespace MilhasApi.Api.DTOs;

public record ProgramaRequest(
    [MaxLength(30)] string? Codigo,
    [Required, MaxLength(100)] string Nome,
    [MaxLength(100)] string CompanhiaAerea,
    [MaxLength(20)] string CorHex,
    bool Ativo
);

public record ProgramaResponse(int Id, string Codigo, string Nome, string CompanhiaAerea, string CorHex, bool Ativo);
