using System.Security.Claims;
using Indotel.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/reclamaciones")]
public class ReclamacionesBusquedaController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public ReclamacionesBusquedaController(IndotelDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? numeroExpediente,
        [FromQuery] string? estado,
        [FromQuery] int? ciudadanoId,
        [FromQuery] int? prestadoraId,
        [FromQuery] int? servicioTelecomId,
        [FromQuery] int? tipoReclamacionId,
        [FromQuery] int? motivoReclamacionId,
        [FromQuery] string? prioridad,
        [FromQuery] string? canalRecepcion,
        [FromQuery] string? provincia,
        [FromQuery] string? municipio,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] bool? vencida,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var query = _db.Reclamaciones
            .Include(x => x.TipoReclamacion)
            .Include(x => x.MotivoReclamacion)
            .AsQueryable();

        if (User.IsInRole("Ciudadano"))
        {
            var ciudadanoActual = await ObtenerCiudadanoIdActual();
            if (ciudadanoActual is null) return Forbid();
            query = query.Where(x => x.CiudadanoId == ciudadanoActual.Value);
        }
        else if (User.IsInRole("Prestadora"))
        {
            var prestadoraActual = await ObtenerPrestadoraIdActual();
            if (prestadoraActual is null) return Forbid();
            query = query.Where(x => x.PrestadoraId == prestadoraActual.Value);
        }

        if (!string.IsNullOrWhiteSpace(numeroExpediente))
        {
            var filtro = numeroExpediente.Trim();
            query = query.Where(x => x.NumeroExpediente.Contains(filtro));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var filtro = estado.Trim().ToUpperInvariant();
            query = query.Where(x => x.Estado == filtro);
        }

        if (ciudadanoId is not null)
        {
            query = query.Where(x => x.CiudadanoId == ciudadanoId.Value);
        }

        if (prestadoraId is not null)
        {
            query = query.Where(x => x.PrestadoraId == prestadoraId.Value);
        }

        if (servicioTelecomId is not null)
        {
            query = query.Where(x => x.ServicioTelecomId == servicioTelecomId.Value);
        }

        if (tipoReclamacionId is not null)
        {
            query = query.Where(x => x.TipoReclamacionId == tipoReclamacionId.Value);
        }

        if (motivoReclamacionId is not null)
        {
            query = query.Where(x => x.MotivoReclamacionId == motivoReclamacionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(prioridad))
        {
            var filtro = prioridad.Trim().ToUpperInvariant();
            query = query.Where(x => x.Prioridad == filtro);
        }

        if (!string.IsNullOrWhiteSpace(canalRecepcion))
        {
            var filtro = canalRecepcion.Trim().ToUpperInvariant();
            query = query.Where(x => x.CanalRecepcion == filtro);
        }

        if (!string.IsNullOrWhiteSpace(provincia))
        {
            var filtro = provincia.Trim();
            query = query.Where(x => x.Provincia.Contains(filtro));
        }

        if (!string.IsNullOrWhiteSpace(municipio))
        {
            var filtro = municipio.Trim();
            query = query.Where(x => x.Municipio.Contains(filtro));
        }

        if (desde is not null)
        {
            query = query.Where(x => x.FechaCreacion >= desde.Value);
        }

        if (hasta is not null)
        {
            query = query.Where(x => x.FechaCreacion <= hasta.Value);
        }

        if (vencida is not null)
        {
            query = query.Where(x => x.EstaVencida == vencida.Value);
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            total,
            page,
            pageSize,
            data
        });
    }

    private async Task<int?> ObtenerCiudadanoIdActual()
    {
        var correo = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(correo)) return null;

        return await _db.Ciudadanos
            .Where(x => x.Correo == correo && x.Activo)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<int?> ObtenerPrestadoraIdActual()
    {
        var correo = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(correo)) return null;

        return await _db.Prestadoras
            .Where(x => x.Correo == correo && x.Activa)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }
}
