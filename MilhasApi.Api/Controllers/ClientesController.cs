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
public class ClientesController : ControllerBase
{
    private const string StatusAtivo = "Ativo";
    private const string StatusViagemPendente = "Viagem Pendente";
    private const string StatusRenovacaoProxima = "Renovação Próxima";
    private const int DiasAlertaRenovacao = 30;

    private readonly MilhasDbContext _db;
    private readonly IAccessScope _scope;

    public ClientesController(MilhasDbContext db, IAccessScope scope)
    {
        _db = db;
        _scope = scope;
    }

    private async Task<List<ClienteResponse>> MontarAsync(IQueryable<Cliente> query)
    {
        var hoje = DateTime.UtcNow.Date;

        var clientes = await query
            .OrderBy(c => c.Nome)
            .Select(c => new
            {
                c.Id, c.Nome, c.Email, c.Telefone, c.Cpf, c.Observacoes, c.CriadoEm, c.DataRenovacao, c.ValorGestao,
                TotalMilhas = c.Milhas.Sum(m => (long?)m.Quantidade) ?? 0,
                Economia = c.Economias.Sum(e => (decimal?)e.Valor) ?? 0
            })
            .ToListAsync();

        var ids = clientes.Select(c => c.Id).ToList();
        var pendentes = await _db.Viagens
            .Where(v => ids.Contains(v.ClienteId) && v.Status != StatusViagem.Finalizada)
            .Select(v => new { v.ClienteId, v.Nome, v.Status, v.Destino, v.DataInicio })
            .ToListAsync();

        return clientes.Select(c =>
        {
            var viagens = pendentes.Where(v => v.ClienteId == c.Id).ToList();
            var proxima = viagens
                .Where(v => !string.IsNullOrWhiteSpace(v.Destino))
                .OrderBy(v => v.DataInicio is null ? 2 : v.DataInicio >= hoje ? 0 : 1)
                .ThenBy(v => v.DataInicio)
                .FirstOrDefault();

            var status = viagens.Count > 0 ? StatusViagemPendente
                : c.DataRenovacao is not null && (c.DataRenovacao.Value.Date - hoje).Days <= DiasAlertaRenovacao ? StatusRenovacaoProxima
                : StatusAtivo;

            return new ClienteResponse(
                c.Id, c.Nome, c.Email, c.Telefone, c.Cpf, c.Observacoes, c.CriadoEm, c.DataRenovacao, c.ValorGestao,
                c.TotalMilhas, c.Economia,
                proxima?.Nome.Trim(), proxima?.Status.ToString().ToLowerInvariant(), status);
        }).ToList();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteResponse>>> GetAll() =>
        Ok(await MontarAsync(_scope.Clientes));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> GetById(int id)
    {
        var lista = await MontarAsync(_scope.Clientes.Where(c => c.Id == id));
        return lista.Count == 0 ? NotFound() : Ok(lista[0]);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Create(ClienteRequest request)
    {
        var cliente = new Cliente
        {
            Nome = request.Nome.Trim(),
            Email = request.Email,
            Telefone = request.Telefone,
            Cpf = request.Cpf,
            Observacoes = request.Observacoes,
            DataRenovacao = request.DataRenovacao,
            ValorGestao = request.ValorGestao,
            GestorId = _scope.GestorId
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        var criado = await MontarAsync(_db.Clientes.Where(c => c.Id == cliente.Id));
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, criado[0]);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> Update(int id, ClienteRequest request)
    {
        var cliente = await _scope.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound();

        cliente.Nome = request.Nome.Trim();
        cliente.Email = request.Email;
        cliente.Telefone = request.Telefone;
        cliente.Cpf = request.Cpf;
        cliente.Observacoes = request.Observacoes;
        cliente.DataRenovacao = request.DataRenovacao;
        cliente.ValorGestao = request.ValorGestao;

        await _db.SaveChangesAsync();
        var atualizado = await MontarAsync(_db.Clientes.Where(c => c.Id == id));
        return Ok(atualizado[0]);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _scope.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound();

        if (await _db.VendasEmissoes.AnyAsync(v => v.ClienteId == id))
            return Conflict(new { message = "Nao e possivel excluir um cliente com vendas vinculadas." });

        _db.Clientes.Remove(cliente);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
