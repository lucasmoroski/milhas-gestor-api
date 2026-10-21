using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilhasApi.Api.Data;
using MilhasApi.Api.DTOs;
using MilhasApi.Api.Models;
using MilhasApi.Api.Services;

namespace MilhasApi.Api.Controllers;

[ApiController]
[Route("api/clientes/{clienteId:int}/milhas")]
[Authorize]
public class ClienteMilhasController : ControllerBase
{
    private const int DiasParaVencer = 90;

    private readonly MilhasDbContext _db;
    private readonly IAccessScope _scope;
    private readonly ISecretProtector _protector;

    public ClienteMilhasController(MilhasDbContext db, IAccessScope scope, ISecretProtector protector)
    {
        _db = db;
        _scope = scope;
        _protector = protector;
    }

    private ClienteMilhasResponse ToResponse(ClienteMilhas m)
    {
        var limite = DateTime.UtcNow.Date.AddDays(DiasParaVencer);
        var aVencer = m.DataExpiracao is not null && m.DataExpiracao.Value.Date <= limite ? m.Quantidade : 0;
        return new ClienteMilhasResponse(
            m.Id, m.ClienteId, m.ProgramaFidelidadeId, m.Programa?.Nome ?? "", m.Programa?.Codigo ?? "",
            m.Programa?.CorHex ?? "#999999", m.Quantidade, aVencer, m.DataExpiracao,
            m.Login, _protector.Unprotect(m.SenhaCriptografada), m.UltimaAtualizacao);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteMilhasResponse>>> GetAll(int clienteId)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();

        var itens = await _db.ClientesMilhas.Include(m => m.Programa)
            .Where(m => m.ClienteId == clienteId)
            .OrderBy(m => m.Programa!.Nome)
            .ToListAsync();
        return Ok(itens.Select(ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteMilhasResponse>> Create(int clienteId, ClienteMilhasRequest request)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        if (!await _db.ProgramasFidelidade.AnyAsync(p => p.Id == request.ProgramaFidelidadeId))
            return BadRequest(new { message = "Programa de milhas invalido." });

        var item = new ClienteMilhas
        {
            ClienteId = clienteId,
            ProgramaFidelidadeId = request.ProgramaFidelidadeId,
            Quantidade = request.Quantidade,
            DataExpiracao = request.DataExpiracao,
            Login = string.IsNullOrWhiteSpace(request.Login) ? null : request.Login.Trim(),
            SenhaCriptografada = _protector.Protect(request.Senha),
            UltimaAtualizacao = DateTime.UtcNow
        };

        _db.ClientesMilhas.Add(item);
        await _db.SaveChangesAsync();
        await _db.Entry(item).Reference(m => m.Programa).LoadAsync();
        return Ok(ToResponse(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteMilhasResponse>> Update(int clienteId, int id, ClienteMilhasRequest request)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        var item = await _db.ClientesMilhas.Include(m => m.Programa).FirstOrDefaultAsync(m => m.Id == id && m.ClienteId == clienteId);
        if (item is null) return NotFound();
        if (!await _db.ProgramasFidelidade.AnyAsync(p => p.Id == request.ProgramaFidelidadeId))
            return BadRequest(new { message = "Programa de milhas invalido." });

        item.ProgramaFidelidadeId = request.ProgramaFidelidadeId;
        item.Quantidade = request.Quantidade;
        item.DataExpiracao = request.DataExpiracao;
        item.Login = string.IsNullOrWhiteSpace(request.Login) ? null : request.Login.Trim();
        if (request.Senha is not null) item.SenhaCriptografada = _protector.Protect(request.Senha);
        item.UltimaAtualizacao = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(item).Reference(m => m.Programa).LoadAsync();
        return Ok(ToResponse(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int clienteId, int id)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        var item = await _db.ClientesMilhas.FirstOrDefaultAsync(m => m.Id == id && m.ClienteId == clienteId);
        if (item is null) return NotFound();

        _db.ClientesMilhas.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/clientes/{clienteId:int}/dependentes")]
[Authorize]
public class DependentesController : ControllerBase
{
    private readonly MilhasDbContext _db;
    private readonly IAccessScope _scope;

    public DependentesController(MilhasDbContext db, IAccessScope scope)
    {
        _db = db;
        _scope = scope;
    }

    private static DependenteResponse ToResponse(Dependente d) =>
        new(d.Id, d.ClienteId, d.Nome, d.DataNascimento, d.Parentesco);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DependenteResponse>>> GetAll(int clienteId)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        var itens = await _db.Dependentes.Where(d => d.ClienteId == clienteId).OrderBy(d => d.Nome).ToListAsync();
        return Ok(itens.Select(ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<DependenteResponse>> Create(int clienteId, DependenteRequest request)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();

        var item = new Dependente
        {
            ClienteId = clienteId,
            Nome = request.Nome.Trim(),
            DataNascimento = request.DataNascimento,
            Parentesco = request.Parentesco
        };
        _db.Dependentes.Add(item);
        await _db.SaveChangesAsync();
        return Ok(ToResponse(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DependenteResponse>> Update(int clienteId, int id, DependenteRequest request)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        var item = await _db.Dependentes.FirstOrDefaultAsync(d => d.Id == id && d.ClienteId == clienteId);
        if (item is null) return NotFound();

        item.Nome = request.Nome.Trim();
        item.DataNascimento = request.DataNascimento;
        item.Parentesco = request.Parentesco;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int clienteId, int id)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        var item = await _db.Dependentes.FirstOrDefaultAsync(d => d.Id == id && d.ClienteId == clienteId);
        if (item is null) return NotFound();

        _db.Dependentes.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/clientes/{clienteId:int}/economia")]
[Authorize]
public class EconomiaController : ControllerBase
{
    private readonly MilhasDbContext _db;
    private readonly IAccessScope _scope;

    public EconomiaController(MilhasDbContext db, IAccessScope scope)
    {
        _db = db;
        _scope = scope;
    }

    private static EconomiaResponse ToResponse(EconomiaRegistro e) =>
        new(e.Id, e.ClienteId, e.ViagemId, e.Viagem?.Nome.Trim(), e.Valor, e.Descricao, e.CriadoEm);

    private async Task<bool> ViagemDoClienteAsync(int clienteId, int? viagemId) =>
        viagemId is null || await _db.Viagens.AnyAsync(v => v.Id == viagemId && v.ClienteId == clienteId);

    [HttpGet]
    public async Task<ActionResult<EconomiaListaResponse>> GetAll(int clienteId)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();

        var itens = await _db.Economias.Include(e => e.Viagem)
            .Where(e => e.ClienteId == clienteId)
            .OrderByDescending(e => e.CriadoEm)
            .ToListAsync();
        return Ok(new EconomiaListaResponse(itens.Sum(i => i.Valor), itens.Select(ToResponse).ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<EconomiaResponse>> Create(int clienteId, EconomiaRequest request)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        if (!await ViagemDoClienteAsync(clienteId, request.ViagemId))
            return BadRequest(new { message = "Viagem invalida para este cliente." });

        var item = new EconomiaRegistro
        {
            ClienteId = clienteId,
            ViagemId = request.ViagemId,
            Valor = request.Valor,
            Descricao = request.Descricao.Trim()
        };
        _db.Economias.Add(item);
        await _db.SaveChangesAsync();
        await _db.Entry(item).Reference(e => e.Viagem).LoadAsync();
        return Ok(ToResponse(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EconomiaResponse>> Update(int clienteId, int id, EconomiaRequest request)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        var item = await _db.Economias.FirstOrDefaultAsync(e => e.Id == id && e.ClienteId == clienteId);
        if (item is null) return NotFound();
        if (!await ViagemDoClienteAsync(clienteId, request.ViagemId))
            return BadRequest(new { message = "Viagem invalida para este cliente." });

        item.Valor = request.Valor;
        item.Descricao = request.Descricao.Trim();
        item.ViagemId = request.ViagemId;
        await _db.SaveChangesAsync();
        await _db.Entry(item).Reference(e => e.Viagem).LoadAsync();
        return Ok(ToResponse(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int clienteId, int id)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        var item = await _db.Economias.FirstOrDefaultAsync(e => e.Id == id && e.ClienteId == clienteId);
        if (item is null) return NotFound();

        _db.Economias.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/clientes/{clienteId:int}/notas")]
[Authorize]
public class NotasController : ControllerBase
{
    private readonly MilhasDbContext _db;
    private readonly IAccessScope _scope;

    public NotasController(MilhasDbContext db, IAccessScope scope)
    {
        _db = db;
        _scope = scope;
    }

    private static NotaResponse ToResponse(Nota n) =>
        new(n.Id, n.ClienteId, n.ViagemId, n.Viagem?.Nome.Trim(), n.Gestor?.Nome, n.Conteudo, n.VisivelAoCliente, n.CriadoEm);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotaResponse>>> GetAll(int clienteId)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();

        var itens = await _db.Notas.Include(n => n.Viagem).Include(n => n.Gestor)
            .Where(n => n.ClienteId == clienteId)
            .OrderByDescending(n => n.CriadoEm)
            .ToListAsync();
        return Ok(itens.Select(ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<NotaResponse>> Create(int clienteId, NotaRequest request)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        if (request.ViagemId is not null && !await _db.Viagens.AnyAsync(v => v.Id == request.ViagemId && v.ClienteId == clienteId))
            return BadRequest(new { message = "Viagem invalida para este cliente." });

        var item = new Nota
        {
            ClienteId = clienteId,
            ViagemId = request.ViagemId,
            GestorId = _scope.UsuarioId,
            Conteudo = request.Conteudo.Trim(),
            VisivelAoCliente = request.VisivelAoCliente
        };
        _db.Notas.Add(item);
        await _db.SaveChangesAsync();
        await _db.Entry(item).Reference(n => n.Viagem).LoadAsync();
        await _db.Entry(item).Reference(n => n.Gestor).LoadAsync();
        return Ok(ToResponse(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int clienteId, int id)
    {
        if (!await _scope.PodeAcessarClienteAsync(clienteId)) return NotFound();
        var item = await _db.Notas.FirstOrDefaultAsync(n => n.Id == id && n.ClienteId == clienteId);
        if (item is null) return NotFound();

        _db.Notas.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
