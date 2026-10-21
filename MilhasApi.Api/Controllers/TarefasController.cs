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
public class TarefasController : ControllerBase
{
    private readonly MilhasDbContext _db;

    public TarefasController(MilhasDbContext db)
    {
        _db = db;
    }

    private static TarefaResponse ToResponse(Tarefa t) =>
        new(t.Id, t.Descricao, t.DataPrazo, t.Concluida, t.ClienteId, t.Cliente?.Nome);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TarefaResponse>>> GetAll()
    {
        var tarefas = await _db.Tarefas.Include(t => t.Cliente).OrderBy(t => t.DataPrazo).ToListAsync();
        return Ok(tarefas.Select(ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<TarefaResponse>> Create(TarefaRequest request)
    {
        var tarefa = new Tarefa
        {
            Descricao = request.Descricao,
            DataPrazo = request.DataPrazo,
            Concluida = request.Concluida,
            ClienteId = request.ClienteId
        };

        _db.Tarefas.Add(tarefa);
        await _db.SaveChangesAsync();
        await _db.Entry(tarefa).Reference(t => t.Cliente).LoadAsync();

        return Ok(ToResponse(tarefa));
    }

    [HttpPut("{id:int}/concluir")]
    public async Task<ActionResult<TarefaResponse>> Concluir(int id)
    {
        var tarefa = await _db.Tarefas.Include(t => t.Cliente).FirstOrDefaultAsync(t => t.Id == id);
        if (tarefa is null) return NotFound();

        tarefa.Concluida = true;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(tarefa));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tarefa = await _db.Tarefas.FindAsync(id);
        if (tarefa is null) return NotFound();

        _db.Tarefas.Remove(tarefa);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
