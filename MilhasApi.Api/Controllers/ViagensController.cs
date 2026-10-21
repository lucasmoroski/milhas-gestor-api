using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilhasApi.Api.Data;
using MilhasApi.Api.DTOs;
using MilhasApi.Api.Models;
using MilhasApi.Api.Services;

namespace MilhasApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ViagensController : ControllerBase
{
    private readonly MilhasDbContext _db;
    private readonly IAccessScope _scope;

    public ViagensController(MilhasDbContext db, IAccessScope scope)
    {
        _db = db;
        _scope = scope;
    }

    private static IQueryable<ViagemResponse> Projetar(IQueryable<Viagem> query) =>
        query.Select(v => new ViagemResponse(
            v.Id, v.ClienteId, v.Cliente!.Nome, v.Nome, v.Status,
            v.Localizador, v.Companhia, v.Hotel, v.Origem, v.Destino,
            v.DataInicio, v.DataFim, v.SomenteIda, v.TipoPagamento,
            v.ProgramaFidelidadeId, v.Programa != null ? v.Programa.Nome : null, v.QuantidadeMilhas,
            v.ValorPago, v.TaxaEmbarque, v.Orcamento, v.Observacoes,
            v.CriadoEm, v.AtualizadoEm));

    private static void Aplicar(Viagem v, ViagemRequest r)
    {
        v.ClienteId = r.ClienteId;
        v.Nome = r.Nome.Trim();
        v.Status = r.Status;
        v.Localizador = r.Localizador;
        v.Companhia = r.Companhia;
        v.Hotel = r.Hotel;
        v.Origem = r.Origem;
        v.Destino = r.Destino;
        v.DataInicio = r.DataInicio;
        v.DataFim = r.SomenteIda ? null : r.DataFim;
        v.SomenteIda = r.SomenteIda;
        v.TipoPagamento = r.TipoPagamento;
        v.ProgramaFidelidadeId = r.TipoPagamento == TipoPagamentoViagem.Milhas ? r.ProgramaFidelidadeId : null;
        v.QuantidadeMilhas = r.TipoPagamento == TipoPagamentoViagem.Milhas ? r.QuantidadeMilhas : null;
        v.ValorPago = r.ValorPago;
        v.TaxaEmbarque = r.TaxaEmbarque;
        v.Orcamento = r.Orcamento;
        v.Observacoes = r.Observacoes;
        v.AtualizadoEm = DateTime.UtcNow;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ViagemResponse>>> GetAll([FromQuery] int? clienteId)
    {
        var ids = _scope.Clientes.Select(c => c.Id);
        var query = _db.Viagens.Where(v => ids.Contains(v.ClienteId));
        if (clienteId.HasValue) query = query.Where(v => v.ClienteId == clienteId.Value);

        var viagens = await Projetar(query.OrderBy(v => v.DataInicio == null).ThenByDescending(v => v.DataInicio)).ToListAsync();
        return Ok(viagens);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ViagemResponse>> GetById(int id)
    {
        var ids = _scope.Clientes.Select(c => c.Id);
        var viagem = await Projetar(_db.Viagens.Where(v => v.Id == id && ids.Contains(v.ClienteId))).FirstOrDefaultAsync();
        return viagem is null ? NotFound() : Ok(viagem);
    }

    [HttpPost]
    public async Task<ActionResult<ViagemResponse>> Create(ViagemRequest request)
    {
        if (!await _scope.PodeAcessarClienteAsync(request.ClienteId))
            return BadRequest(new { message = "Cliente invalido." });
        if (request.ProgramaFidelidadeId is not null && !await _db.ProgramasFidelidade.AnyAsync(p => p.Id == request.ProgramaFidelidadeId))
            return BadRequest(new { message = "Programa de milhas invalido." });

        var viagem = new Viagem();
        Aplicar(viagem, request);
        _db.Viagens.Add(viagem);
        await _db.SaveChangesAsync();

        var criada = await Projetar(_db.Viagens.Where(v => v.Id == viagem.Id)).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = viagem.Id }, criada);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ViagemResponse>> Update(int id, ViagemRequest request)
    {
        var ids = _scope.Clientes.Select(c => c.Id);
        var viagem = await _db.Viagens.FirstOrDefaultAsync(v => v.Id == id && ids.Contains(v.ClienteId));
        if (viagem is null) return NotFound();

        if (!await _scope.PodeAcessarClienteAsync(request.ClienteId))
            return BadRequest(new { message = "Cliente invalido." });
        if (request.ProgramaFidelidadeId is not null && !await _db.ProgramasFidelidade.AnyAsync(p => p.Id == request.ProgramaFidelidadeId))
            return BadRequest(new { message = "Programa de milhas invalido." });

        Aplicar(viagem, request);
        await _db.SaveChangesAsync();

        return Ok(await Projetar(_db.Viagens.Where(v => v.Id == id)).FirstAsync());
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<ViagemResponse>> AlterarStatus(int id, ViagemStatusRequest request)
    {
        var ids = _scope.Clientes.Select(c => c.Id);
        var viagem = await _db.Viagens.FirstOrDefaultAsync(v => v.Id == id && ids.Contains(v.ClienteId));
        if (viagem is null) return NotFound();

        viagem.Status = request.Status;
        viagem.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(await Projetar(_db.Viagens.Where(v => v.Id == id)).FirstAsync());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ids = _scope.Clientes.Select(c => c.Id);
        var viagem = await _db.Viagens.FirstOrDefaultAsync(v => v.Id == id && ids.Contains(v.ClienteId));
        if (viagem is null) return NotFound();

        var economias = await _db.Economias.Where(e => e.ViagemId == id).ToListAsync();
        foreach (var e in economias) e.ViagemId = null;
        var notas = await _db.Notas.Where(n => n.ViagemId == id).ToListAsync();
        foreach (var n in notas) n.ViagemId = null;

        _db.Viagens.Remove(viagem);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
