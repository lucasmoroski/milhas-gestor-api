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
public class ContasMilhasController : ControllerBase
{
    private readonly MilhasDbContext _db;

    public ContasMilhasController(MilhasDbContext db)
    {
        _db = db;
    }

    private static ContaMilhasResponse ToResponse(ContaMilhas c) => new(
        c.Id, c.ProgramaFidelidadeId, c.Programa?.Nome ?? "", c.Programa?.CorHex ?? "#999999",
        c.NomeTitular, c.CpfTitular, c.LoginAcesso, c.SaldoMilhas, c.CustoMedioMilheiro, c.Ativo, c.CriadoEm
    );

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContaMilhasResponse>>> GetAll()
    {
        var contas = await _db.ContasMilhas.Include(c => c.Programa).OrderBy(c => c.NomeTitular).ToListAsync();
        return Ok(contas.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ContaMilhasResponse>> GetById(int id)
    {
        var conta = await _db.ContasMilhas.Include(c => c.Programa).FirstOrDefaultAsync(c => c.Id == id);
        return conta is null ? NotFound() : Ok(ToResponse(conta));
    }

    [HttpPost]
    public async Task<ActionResult<ContaMilhasResponse>> Create(ContaMilhasRequest request)
    {
        if (!await _db.ProgramasFidelidade.AnyAsync(p => p.Id == request.ProgramaFidelidadeId))
            return BadRequest(new { message = "Programa de fidelidade invalido." });

        var conta = new ContaMilhas
        {
            ProgramaFidelidadeId = request.ProgramaFidelidadeId,
            NomeTitular = request.NomeTitular,
            CpfTitular = request.CpfTitular,
            LoginAcesso = request.LoginAcesso,
            Ativo = request.Ativo
        };

        _db.ContasMilhas.Add(conta);
        await _db.SaveChangesAsync();
        await _db.Entry(conta).Reference(c => c.Programa).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = conta.Id }, ToResponse(conta));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ContaMilhasResponse>> Update(int id, ContaMilhasRequest request)
    {
        var conta = await _db.ContasMilhas.Include(c => c.Programa).FirstOrDefaultAsync(c => c.Id == id);
        if (conta is null) return NotFound();

        if (!await _db.ProgramasFidelidade.AnyAsync(p => p.Id == request.ProgramaFidelidadeId))
            return BadRequest(new { message = "Programa de fidelidade invalido." });

        conta.ProgramaFidelidadeId = request.ProgramaFidelidadeId;
        conta.NomeTitular = request.NomeTitular;
        conta.CpfTitular = request.CpfTitular;
        conta.LoginAcesso = request.LoginAcesso;
        conta.Ativo = request.Ativo;

        await _db.SaveChangesAsync();
        await _db.Entry(conta).Reference(c => c.Programa).LoadAsync();

        return Ok(ToResponse(conta));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var conta = await _db.ContasMilhas.FindAsync(id);
        if (conta is null) return NotFound();

        var possuiMovimentacoes = await _db.ComprasMilhas.AnyAsync(c => c.ContaMilhasId == id)
            || await _db.VendasEmissoes.AnyAsync(v => v.ContaMilhasId == id);

        if (possuiMovimentacoes)
            return Conflict(new { message = "Nao e possivel excluir uma conta com compras ou vendas vinculadas." });

        _db.ContasMilhas.Remove(conta);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
