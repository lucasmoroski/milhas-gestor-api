using System.ComponentModel.DataAnnotations;

namespace MilhasApi.Api.DTOs;

public record ClienteMilhasRequest(
    [Required] int ProgramaFidelidadeId,
    [Range(0, long.MaxValue)] long Quantidade,
    DateTime? DataExpiracao,
    [MaxLength(200)] string? Login,
    [MaxLength(200)] string? Senha
);

public record ClienteMilhasResponse(
    int Id, int ClienteId, int ProgramaFidelidadeId, string ProgramaNome, string ProgramaCodigo,
    string ProgramaCorHex, long Quantidade, long ProximasAVencer, DateTime? DataExpiracao,
    string? Login, string? Senha, DateTime UltimaAtualizacao
);

public record DependenteRequest(
    [Required, MaxLength(150)] string Nome,
    DateTime? DataNascimento,
    [MaxLength(60)] string? Parentesco
);

public record DependenteResponse(int Id, int ClienteId, string Nome, DateTime? DataNascimento, string? Parentesco);

public record EconomiaRequest(
    [Range(0.01, double.MaxValue)] decimal Valor,
    [Required, MaxLength(300)] string Descricao,
    int? ViagemId
);

public record EconomiaResponse(
    int Id, int ClienteId, int? ViagemId, string? ViagemNome, decimal Valor, string Descricao, DateTime CriadoEm
);

public record EconomiaListaResponse(decimal Total, List<EconomiaResponse> Itens);

public record NotaRequest([Required] string Conteudo, bool VisivelAoCliente, int? ViagemId);

public record NotaResponse(
    int Id, int ClienteId, int? ViagemId, string? ViagemNome, string? GestorNome,
    string Conteudo, bool VisivelAoCliente, DateTime CriadoEm
);
