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
public class DashboardController : ControllerBase
{
    private const int JanelaRenovacaoDias = 60;

    private readonly MilhasDbContext _db;
    private readonly IAccessScope _scope;

    public DashboardController(MilhasDbContext db, IAccessScope scope)
    {
        _db = db;
        _scope = scope;
    }

    [HttpGet("resumo")]
    public async Task<ActionResult<DashboardResumo>> Resumo()
    {
        var hoje = DateTime.UtcNow.Date;
        var clientes = _scope.Clientes;
        var ids = clientes.Select(c => c.Id);

        var listaClientes = await clientes
            .Select(c => new { c.Id, c.Nome, c.Email, c.DataRenovacao })
            .ToListAsync();

        var totalClientes = listaClientes.Count;
        var milhasTotais = await _db.ClientesMilhas.Where(m => ids.Contains(m.ClienteId)).SumAsync(m => (long?)m.Quantidade) ?? 0;
        var economiaTotal = await _db.Economias.Where(e => ids.Contains(e.ClienteId)).SumAsync(e => (decimal?)e.Valor) ?? 0;

        var viagens = await _db.Viagens
            .Where(v => ids.Contains(v.ClienteId))
            .Select(v => new
            {
                v.Id, v.Nome, v.Status, v.Orcamento, v.DataInicio, v.DataFim, v.Localizador, v.Hotel,
                ClienteNome = v.Cliente!.Nome
            })
            .ToListAsync();

        var viagensTotais = viagens.Count;
        var finalizadas = viagens.Count(v => v.Status == StatusViagem.Finalizada);
        var ativas = viagens.Count(v => v.Status != StatusViagem.Finalizada && (v.DataFim ?? v.DataInicio) >= hoje);
        var taxaConclusao = viagensTotais > 0 ? (int)Math.Round(finalizadas * 100m / viagensTotais) : 0;
        var orcamentoTotal = viagens.Sum(v => v.Orcamento ?? 0);
        var economiaMedia = totalClientes > 0 ? Math.Round(economiaTotal / totalClientes, 2) : 0;

        var renovacoes = listaClientes
            .Where(c => c.DataRenovacao is not null && (c.DataRenovacao.Value.Date - hoje).Days <= JanelaRenovacaoDias)
            .OrderBy(c => c.DataRenovacao)
            .Take(5)
            .Select(c => new RenovacaoCliente(c.Id, c.Nome, c.Email, c.DataRenovacao!.Value, (c.DataRenovacao.Value.Date - hoje).Days))
            .ToList();

        var proximasViagens = viagens
            .Where(v => v.DataInicio is not null && v.DataInicio >= hoje)
            .OrderBy(v => v.DataInicio)
            .Take(4)
            .Select(v => new ProximaViagem(
                v.Id, v.Nome.Trim(), v.ClienteNome.Trim(), v.DataInicio, v.Localizador,
                v.Hotel?.Trim(), v.Status.ToString().ToLowerInvariant()))
            .ToList();

        var tarefas = await _db.Tarefas
            .Include(t => t.Cliente)
            .Where(t => !t.Concluida && (t.ClienteId == null || ids.Contains(t.ClienteId.Value)))
            .OrderBy(t => t.DataPrazo)
            .Take(5)
            .ToListAsync();

        var tarefasResponse = tarefas
            .Select(t => new TarefaResponse(t.Id, t.Descricao, t.DataPrazo, t.Concluida, t.ClienteId, t.Cliente?.Nome))
            .ToList();

        return Ok(new DashboardResumo(
            totalClientes, milhasTotais, economiaTotal, ativas,
            viagensTotais, finalizadas, taxaConclusao, orcamentoTotal, economiaMedia,
            renovacoes, proximasViagens, tarefasResponse));
    }
}
