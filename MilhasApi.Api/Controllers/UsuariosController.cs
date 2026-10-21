using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilhasApi.Api.Data;
using MilhasApi.Api.DTOs;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly MilhasDbContext _db;

    public UsuariosController(MilhasDbContext db)
    {
        _db = db;
    }

    private static UsuarioResponse ToResponse(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.Perfil, u.Ativo, u.CriadoEm);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioResponse>>> GetAll()
    {
        var usuarios = await _db.Usuarios.OrderBy(u => u.Nome).ToListAsync();
        return Ok(usuarios.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioResponse>> GetById(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        return usuario is null ? NotFound() : Ok(ToResponse(usuario));
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioResponse>> Create(CriarUsuarioRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Usuarios.AnyAsync(u => u.Email.ToLower() == email))
            return Conflict(new { message = "Ja existe um usuario com esse email." });

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Perfil = request.Perfil,
            Ativo = true
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, ToResponse(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioResponse>> Update(int id, AtualizarUsuarioRequest request)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        usuario.Nome = request.Nome;
        usuario.Perfil = request.Perfil;
        usuario.Ativo = request.Ativo;

        if (!string.IsNullOrWhiteSpace(request.NovaSenha))
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

        await _db.SaveChangesAsync();
        return Ok(ToResponse(usuario));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
