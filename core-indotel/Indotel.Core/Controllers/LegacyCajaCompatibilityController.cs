using Indotel.Core.Constants;
using Indotel.Core.Data;
using Indotel.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

/// <summary>
/// Mantiene los contratos utilizados por la Caja original de .NET Framework 4.8.1.
/// La Caja no necesita cambios: esta capa adapta el Core actual a su ruta histórica.
/// </summary>
[Authorize]
[ApiController]
[Route("api/reclamaciones")]
public sealed class LegacyCajaCompatibilityController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public LegacyCajaCompatibilityController(IndotelDbContext db)
    {
        _db = db;
    }

    [Authorize(Policy = AuthorizationPolicies.CajaConsulta)]
    [HttpGet("cedula/{cedula}")]
    public async Task<IActionResult> GetByCedula(
        string cedula,
        CancellationToken cancellationToken)
    {
        var valor = (cedula ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(valor))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "CEDULA_OBLIGATORIA",
                "La cedula es obligatoria");
        }

        var ciudadanoId = await _db.Ciudadanos
            .AsNoTracking()
            .Where(x => x.Cedula == valor && x.Activo)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (ciudadanoId is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status404NotFound,
                "CIUDADANO_NO_ENCONTRADO",
                "No existe un ciudadano con esta cedula");
        }

        var data = await _db.Reclamaciones
            .AsNoTracking()
            .Where(x => x.CiudadanoId == ciudadanoId.Value)
            .OrderByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return Ok(data);
    }
}
