using System.Security.Claims;
using Indotel.Core.Data;
using Indotel.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Services;

public sealed class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IndotelDbContext _db;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IndotelDbContext db)
    {
        _httpContextAccessor = httpContextAccessor;
        _db = db;
    }

    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? new ClaimsPrincipal(new ClaimsIdentity());

    public int? UsuarioId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string Correo => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    public string Rol => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public bool EsAdministrador => User.IsInRole("Administrador");
    public bool EsAnalista => User.IsInRole("AnalistaDAU");
    public bool EsAuditor => User.IsInRole("Auditor");
    public bool EsCiudadano => User.IsInRole("Ciudadano");
    public bool EsPrestadora => User.IsInRole("Prestadora");
    public bool EsInternoConsulta => EsAdministrador || EsAnalista || EsAuditor;
    public bool EsOperadorCaja => EsAdministrador || EsAnalista;

    public async Task<int?> ObtenerCiudadanoIdAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(Correo)) return null;

        return await _db.Ciudadanos
            .AsNoTracking()
            .Where(x => x.Correo == Correo && x.Activo)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int?> ObtenerPrestadoraIdAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(Correo)) return null;

        return await _db.Prestadoras
            .AsNoTracking()
            .Where(x => x.Correo == Correo && x.Activa)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> PuedeAccederCiudadanoAsync(
        int ciudadanoId,
        CancellationToken cancellationToken = default)
    {
        if (EsInternoConsulta) return true;
        if (!EsCiudadano) return false;

        var actual = await ObtenerCiudadanoIdAsync(cancellationToken);
        return actual == ciudadanoId;
    }

    public async Task<bool> PuedeVerReclamacionAsync(
        Reclamacion reclamacion,
        CancellationToken cancellationToken = default)
    {
        if (EsInternoConsulta) return true;

        if (EsCiudadano)
        {
            var ciudadanoId = await ObtenerCiudadanoIdAsync(cancellationToken);
            return ciudadanoId == reclamacion.CiudadanoId;
        }

        if (EsPrestadora)
        {
            var prestadoraId = await ObtenerPrestadoraIdAsync(cancellationToken);
            return prestadoraId == reclamacion.PrestadoraId;
        }

        return false;
    }
}
