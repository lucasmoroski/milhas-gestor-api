using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MilhasApi.Api.Data;
using MilhasApi.Api.DTOs;
using MilhasApi.Api.Models;
using MilhasApi.Api.Services;

namespace MilhasApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MilhasDbContext _db;
    private readonly IJwtService _jwt;

    public AuthController(MilhasDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (usuario is null || !usuario.Ativo || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            return Unauthorized(new { message = "Email ou senha invalidos." });
        }

        var (token, expiraEm) = _jwt.GerarToken(usuario);

        return Ok(new LoginResponse(
            token,
            expiraEm,
            new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil, usuario.Ativo, usuario.CriadoEm)
        ));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioResponse>> Me()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(sub, out var id)) return Unauthorized();

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.Ativo);
        if (usuario is null) return Unauthorized();

        return Ok(new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil, usuario.Ativo, usuario.CriadoEm));
    }
}
