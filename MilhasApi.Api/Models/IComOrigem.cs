namespace MilhasApi.Api.Models;

/// <summary>Entidades que podem ser importadas de uma fonte externa e reimportadas sem duplicar.</summary>
public interface IComOrigem
{
    string? OrigemId { get; set; }
}
