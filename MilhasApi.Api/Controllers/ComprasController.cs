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
public class ComprasController : ControllerBase
{
    private readonly MilhasDbContext _db;

    public ComprasController(MilhasDbContext db)
    {
        _db = db;
    }

    private static CompraResponse ToResponse(CompraMilhas c) => new(
        c.Id, c.ContaMilhasId, c.Conta?.NomeTitular ?? "", c.Conta?.Programa?.Nome ?? "",
        c.FornecedorNome, c.QuantidadeMilhas, c.ValorTotal, c.CustoPorMilheiro,
        c.FormaPagamento, c.DataCompra, c.Status, c.Observacoes
    );

    private static void AplicarCreditoNaConta(ContaMilhas conta, long quantidade, decimal custoPorMilheiro)
    {
        var milheirosAtuais = conta.SaldoMilhas / 1000m;
        var milheirosNovos = quantidade / 1000m;
        var totalMilheiros = milheirosAtuais + milheirosNovos;

        conta.CustoMedioMilheiro = totalMilheiros > 0
            ? Math.Round(((milheirosAtuais * conta.CustoMedioMilheiro) + (milheirosNovos * custoPorMilheiro)) / totalMilheiros, 2)
            : 0;

        conta.SaldoMilhas += quantidade;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompraResponse>>> GetAll()
    {
        var compras = await _db.ComprasMilhas
            .Include(c => c.Conta).ThenInclude(c => c!.Programa)
            .OrderByDescending(c => c.DataCompra)
            .ToListAsync();
        return Ok(compras.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompraResponse>> GetById(int id)
    {
        var compra = await _db.ComprasMilhas
            .Include(c => c.Conta).ThenInclude(c => c!.Programa)
            .FirstOrDefaultAsync(c => c.Id == id);
        return compra is null ? NotFound() : Ok(ToResponse(compra));
    }

    [HttpPost]
    public async Task<ActionResult<CompraResponse>> Create(CompraRequest request)
    {
        var conta = await _db.ContasMilhas.Include(c => c.Programa).FirstOrDefaultAsync(c => c.Id == request.ContaMilhasId);
        if (conta is null) return BadRequest(new { message = "Conta de milhas invalida." });

        var compra = new CompraMilhas
        {
            ContaMilhasId = request.ContaMilhasId,
            FornecedorNome = request.FornecedorNome,
            QuantidadeMilhas = request.QuantidadeMilhas,
            ValorTotal = request.ValorTotal,
            FormaPagamento = request.FormaPagamento,
            DataCompra = request.DataCompra,
            Status = request.Status,
            Observacoes = request.Observacoes
        };

        if (compra.Status == StatusCompra.Confirmada)
            AplicarCreditoNaConta(conta, compra.QuantidadeMilhas, compra.CustoPorMilheiro);

        _db.ComprasMilhas.Add(compra);
        await _db.SaveChangesAsync();

        compra.Conta = conta;
        return CreatedAtAction(nameof(GetById), new { id = compra.Id }, ToResponse(compra));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var compra = await _db.ComprasMilhas.FindAsync(id);
        if (compra is null) return NotFound();

        if (compra.Status == StatusCompra.Confirmada)
            return Conflict(new { message = "Cancele a compra antes de exclui-la, para nao distorcer o saldo da conta." });

        _db.ComprasMilhas.Remove(compra);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
