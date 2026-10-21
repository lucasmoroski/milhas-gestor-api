using System.ComponentModel.DataAnnotations;

namespace MilhasApi.Api.DTOs;

public record TarefaRequest(
    [Required, MaxLength(300)] string Descricao,
    DateTime DataPrazo,
    bool Concluida,
    int? ClienteId
);

public record TarefaResponse(
    int Id, string Descricao, DateTime DataPrazo, bool Concluida,
    int? ClienteId, string? ClienteNome
);
