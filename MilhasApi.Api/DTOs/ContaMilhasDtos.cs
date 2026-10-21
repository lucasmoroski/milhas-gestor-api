using System.ComponentModel.DataAnnotations;

namespace MilhasApi.Api.DTOs;

public record ContaMilhasRequest(
    [Required] int ProgramaFidelidadeId,
    [Required, MaxLength(150)] string NomeTitular,
    [Required, MaxLength(20)] string CpfTitular,
    string? LoginAcesso,
    bool Ativo
);

public record ContaMilhasResponse(
    int Id,
    int ProgramaFidelidadeId,
    string ProgramaNome,
    string ProgramaCorHex,
    string NomeTitular,
    string CpfTitular,
    string? LoginAcesso,
    long SaldoMilhas,
    decimal CustoMedioMilheiro,
    bool Ativo,
    DateTime CriadoEm
);
