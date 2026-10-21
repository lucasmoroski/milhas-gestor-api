using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MilhasApi.Api.Data;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.Services;

/// <summary>
/// Define o que o usuario logado enxerga: gestores veem apenas seus proprios clientes;
/// Admin/Operador veem todos.
/// </summary>
public interface IAccessScope
{
    int? GestorId { get; }
    int? UsuarioId { get; }
    IQueryable<Cliente> Clientes { get; }
    Task<bool> PodeAcessarClienteAsync(int clienteId);
}

public class AccessScope : IAccessScope
{
    private readonly MilhasDbContext _db;
    private readonly ClaimsPrincipal _user;

    public AccessScope(MilhasDbContext db, IHttpContextAccessor accessor)
    {
        _db = db;
        _user = accessor.HttpContext?.User ?? new ClaimsPrincipal();
    }

    public int? UsuarioId
    {
        get
        {
            var raw = _user.FindFirst("sub")?.Value ?? _user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public int? GestorId => _user.IsInRole(nameof(PerfilUsuario.Gestor)) ? UsuarioId : null;

    public IQueryable<Cliente> Clientes
    {
        get
        {
            var gestorId = GestorId;
            return gestorId is null
                ? _db.Clientes
                : _db.Clientes.Where(c => c.GestorId == gestorId);
        }
    }

    public Task<bool> PodeAcessarClienteAsync(int clienteId) => Clientes.AnyAsync(c => c.Id == clienteId);
}
