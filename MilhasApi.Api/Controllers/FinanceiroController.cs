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
public class FinanceiroController : ControllerBase
{
    private readonly MilhasDbContext _db;

    public FinanceiroController(MilhasDbContext db)
    {
        _db = db;
    }

    private static LancamentoResponse ToResponse(LancamentoFinanceiro l) => new(
        l.Id, l.Tipo, l.Categoria, l.Descricao, l.Valor, l.DataVencimento, l.DataPagamento,
        l.Status, l.VendaEmissaoId, l.CompraMilhasId
    );

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LancamentoResponse>>> GetAll([FromQuery] TipoLancamento? tipo, [FromQuery] StatusLancamento? status)
    {
        var query = _db.LancamentosFinanceiros.AsQueryable();
        if (tipo.HasValue) query = query.Where(l => l.Tipo == tipo.Value);
        if (status.HasValue) query = query.Where(l => l.Status == status.Value);

        var lancamentos = await query.OrderByDescending(l => l.DataVencimento).ToListAsync();
        return Ok(lancamentos.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LancamentoResponse>> GetById(int id)
    {
        var lancamento = await _db.LancamentosFinanceiros.FindAsync(id);
        return lancamento is null ? NotFound() : Ok(ToResponse(lancamento));
    }

    [HttpPost]
    public async Task<ActionResult<LancamentoResponse>> Create(LancamentoRequest request)
    {
        var lancamento = new LancamentoFinanceiro
        {
            Tipo = request.Tipo,
            Categoria = request.Categoria,
            Descricao = request.Descricao,
            Valor = request.Valor,
            DataVencimento = request.DataVencimento,
            DataPagamento = request.DataPagamento,
            Status = request.Status,
            VendaEmissaoId = request.VendaEmissaoId,
            CompraMilhasId = request.CompraMilhasId
        };

        _db.LancamentosFinanceiros.Add(lancamento);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = lancamento.Id }, ToResponse(lancamento));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LancamentoResponse>> Update(int id, LancamentoRequest request)
    {
        var lancamento = await _db.LancamentosFinanceiros.FindAsync(id);
        if (lancamento is null) return NotFound();

        lancamento.Tipo = request.Tipo;
        lancamento.Categoria = request.Categoria;
        lancamento.Descricao = request.Descricao;
        lancamento.Valor = request.Valor;
        lancamento.DataVencimento = request.DataVencimento;
        lancamento.DataPagamento = request.DataPagamento;
        lancamento.Status = request.Status;
        lancamento.VendaEmissaoId = request.VendaEmissaoId;
        lancamento.CompraMilhasId = request.CompraMilhasId;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(lancamento));
    }

    [HttpPut("{id:int}/quitar")]
    public async Task<ActionResult<LancamentoResponse>> Quitar(int id)
    {
        var lancamento = await _db.LancamentosFinanceiros.FindAsync(id);
        if (lancamento is null) return NotFound();

        lancamento.Status = StatusLancamento.Pago;
        lancamento.DataPagamento = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(lancamento));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var lancamento = await _db.LancamentosFinanceiros.FindAsync(id);
        if (lancamento is null) return NotFound();

        _db.LancamentosFinanceiros.Remove(lancamento);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
