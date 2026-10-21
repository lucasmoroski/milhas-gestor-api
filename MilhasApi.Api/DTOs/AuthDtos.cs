using System.ComponentModel.DataAnnotations;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.DTOs;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Senha
);

public record LoginResponse(string Token, DateTime ExpiraEm, UsuarioResponse Usuario);

public record UsuarioResponse(int Id, string Nome, string Email, PerfilUsuario Perfil, bool Ativo, DateTime CriadoEm);

public record CriarUsuarioRequest(
    [Required, MaxLength(150)] string Nome,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Senha,
    PerfilUsuario Perfil
);

public record AtualizarUsuarioRequest(
    [Required, MaxLength(150)] string Nome,
    PerfilUsuario Perfil,
    bool Ativo,
    string? NovaSenha
);
