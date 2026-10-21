using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilhasApi.Api.Data;
using MilhasApi.Api.DTOs;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgramasController : ControllerBase
{
    private readonly MilhasDbContext _db;

    public ProgramasController(MilhasDbContext db)
    {
        _db = db;
    }

    private static ProgramaResponse ToResponse(ProgramaFidelidade p) =>
        new(p.Id, p.Codigo, p.Nome, p.CompanhiaAerea, p.CorHex, p.Ativo);

    private static string GerarCodigo(string nome)
    {
        var sb = new StringBuilder();
        foreach (var ch in nome.ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (char.IsLetterOrDigit(ch) && ch < 128) sb.Append(ch);
        }
        return sb.Length == 0 ? "programa" : sb.ToString();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProgramaResponse>>> GetAll()
    {
        var programas = await _db.ProgramasFidelidade.OrderBy(p => p.Nome).ToListAsync();
        return Ok(programas.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProgramaResponse>> GetById(int id)
    {
        var programa = await _db.ProgramasFidelidade.FindAsync(id);
        return programa is null ? NotFound() : Ok(ToResponse(programa));
    }

    [HttpPost]
    public async Task<ActionResult<ProgramaResponse>> Create(ProgramaRequest request)
    {
        var codigo = string.IsNullOrWhiteSpace(request.Codigo) ? GerarCodigo(request.Nome) : request.Codigo.Trim().ToLowerInvariant();
        if (await _db.ProgramasFidelidade.AnyAsync(p => p.Codigo == codigo))
            return Conflict(new { message = "Ja existe um programa com esse codigo." });

        var programa = new ProgramaFidelidade
        {
            Codigo = codigo,
            Nome = request.Nome,
            CompanhiaAerea = request.CompanhiaAerea,
            CorHex = request.CorHex,
            Ativo = request.Ativo
        };

        _db.ProgramasFidelidade.Add(programa);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = programa.Id }, ToResponse(programa));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProgramaResponse>> Update(int id, ProgramaRequest request)
    {
        var programa = await _db.ProgramasFidelidade.FindAsync(id);
        if (programa is null) return NotFound();

        programa.Nome = request.Nome;
        programa.CompanhiaAerea = request.CompanhiaAerea;
        programa.CorHex = request.CorHex;
        programa.Ativo = request.Ativo;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(programa));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var programa = await _db.ProgramasFidelidade.FindAsync(id);
        if (programa is null) return NotFound();

        var emUso = await _db.ContasMilhas.AnyAsync(c => c.ProgramaFidelidadeId == id)
            || await _db.ClientesMilhas.AnyAsync(c => c.ProgramaFidelidadeId == id);
        if (emUso)
            return Conflict(new { message = "Nao e possivel excluir um programa em uso por contas ou clientes." });

        _db.ProgramasFidelidade.Remove(programa);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
