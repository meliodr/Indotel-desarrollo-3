using Indotel.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/catalogos")]
public class CatalogosController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public CatalogosController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var data = await _db.Roles.OrderBy(x => x.Nombre).ToListAsync();
        return Ok(data);
    }

    [HttpGet("servicios")]
    [HttpGet("~/api/servicios")]
    public async Task<IActionResult> GetServicios()
    {
        var data = await _db.ServiciosTelecom.Where(x => x.Activo).OrderBy(x => x.Nombre).ToListAsync();
        return Ok(data);
    }

    [HttpGet("prestadoras")]
    public async Task<IActionResult> GetPrestadoras()
    {
        var data = await _db.Prestadoras.Where(x => x.Activa).OrderBy(x => x.NombreComercial).ToListAsync();
        return Ok(data);
    }
}
