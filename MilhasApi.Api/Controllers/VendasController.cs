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
public class VendasController : ControllerBase
{
    private readonly MilhasDbContext _db;

    public VendasController(MilhasDbContext db)
    {
        _db = db;
    }

    private static VendaResponse ToResponse(VendaEmissao v) => new(
        v.Id, v.ClienteId, v.Cliente?.Nome ?? "", v.ContaMilhasId, v.Conta?.NomeTitular ?? "",
        v.Conta?.Programa?.Nome ?? "", v.CompanhiaAerea, v.Localizador, v.Origem, v.Destino,
        v.DataViagem, v.DataEmissao, v.QuantidadeMilhasUtilizadas, v.TaxaEmbarque, v.ValorVenda,
        v.CustoMilhasNaVenda, v.LucroEstimado, v.ValorMercado, v.Economia, v.Hotel, v.Status, v.Observacoes
    );

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendaResponse>>> GetAll()
    {
        var vendas = await _db.VendasEmissoes
            .Include(v => v.Cliente)
            .Include(v => v.Conta).ThenInclude(c => c!.Programa)
            .OrderByDescending(v => v.DataEmissao)
            .ToListAsync();
        return Ok(vendas.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VendaResponse>> GetById(int id)
    {
        var venda = await _db.VendasEmissoes
            .Include(v => v.Cliente)
            .Include(v => v.Conta).ThenInclude(c => c!.Programa)
            .FirstOrDefaultAsync(v => v.Id == id);
        return venda is null ? NotFound() : Ok(ToResponse(venda));
    }

    [HttpPost]
    public async Task<ActionResult<VendaResponse>> Create(VendaRequest request)
    {
        var cliente = await _db.Clientes.FindAsync(request.ClienteId);
        if (cliente is null) return BadRequest(new { message = "Cliente invalido." });

        var conta = await _db.ContasMilhas.Include(c => c.Programa).FirstOrDefaultAsync(c => c.Id == request.ContaMilhasId);
        if (conta is null) return BadRequest(new { message = "Conta de milhas invalida." });

        var debitaSaldo = VendaEmissao.StatusDebitaSaldo(request.Status);

        if (debitaSaldo && conta.SaldoMilhas < request.QuantidadeMilhasUtilizadas)
            return BadRequest(new { message = $"Saldo insuficiente. Disponivel: {conta.SaldoMilhas:N0} milhas." });

        var custoMilhasNaVenda = Math.Round((request.QuantidadeMilhasUtilizadas / 1000m) * conta.CustoMedioMilheiro, 2);

        var venda = new VendaEmissao
        {
            ClienteId = request.ClienteId,
            ContaMilhasId = request.ContaMilhasId,
            CompanhiaAerea = request.CompanhiaAerea,
            Localizador = request.Localizador,
            Origem = request.Origem.ToUpperInvariant(),
            Destino = request.Destino.ToUpperInvariant(),
            DataViagem = request.DataViagem,
            DataEmissao = request.DataEmissao,
            QuantidadeMilhasUtilizadas = request.QuantidadeMilhasUtilizadas,
            TaxaEmbarque = request.TaxaEmbarque,
            ValorVenda = request.ValorVenda,
            ValorMercado = request.ValorMercado,
            Hotel = request.Hotel,
            CustoMilhasNaVenda = custoMilhasNaVenda,
            Status = request.Status,
            Observacoes = request.Observacoes
        };

        if (debitaSaldo)
            conta.SaldoMilhas -= venda.QuantidadeMilhasUtilizadas;

        _db.VendasEmissoes.Add(venda);
        await _db.SaveChangesAsync();

        venda.Cliente = cliente;
        venda.Conta = conta;
        return CreatedAtAction(nameof(GetById), new { id = venda.Id }, ToResponse(venda));
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<VendaResponse>> AlterarStatus(int id, VendaStatusRequest request)
    {
        var venda = await _db.VendasEmissoes
            .Include(v => v.Cliente)
            .Include(v => v.Conta).ThenInclude(c => c!.Programa)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();

        var estavaDebitada = VendaEmissao.StatusDebitaSaldo(venda.Status);
        var vaiDebitar = VendaEmissao.StatusDebitaSaldo(request.Status);

        if (!estavaDebitada && vaiDebitar)
        {
            if (venda.Conta!.SaldoMilhas < venda.QuantidadeMilhasUtilizadas)
                return BadRequest(new { message = $"Saldo insuficiente. Disponivel: {venda.Conta.SaldoMilhas:N0} milhas." });
            venda.Conta.SaldoMilhas -= venda.QuantidadeMilhasUtilizadas;
        }
        else if (estavaDebitada && !vaiDebitar)
        {
            venda.Conta!.SaldoMilhas += venda.QuantidadeMilhasUtilizadas;
        }

        venda.Status = request.Status;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(venda));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var venda = await _db.VendasEmissoes.Include(v => v.Conta).FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();

        if (VendaEmissao.StatusDebitaSaldo(venda.Status) && venda.Conta is not null)
            venda.Conta.SaldoMilhas += venda.QuantidadeMilhasUtilizadas;

        _db.VendasEmissoes.Remove(venda);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
