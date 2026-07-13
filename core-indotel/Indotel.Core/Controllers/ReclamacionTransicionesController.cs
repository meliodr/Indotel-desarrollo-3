using Indotel.Core.Data;
using Indotel.Core.Helpers;
using Indotel.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/reclamaciones")]
public sealed class ReclamacionTransicionesController : ControllerBase
{
    private readonly IndotelDbContext _db;
    private readonly CurrentUserService _currentUser;

    public ReclamacionTransicionesController(
        IndotelDbContext db,
        CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("{id:int}/transiciones")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var reclamacion = await _db.Reclamaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (reclamacion is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status404NotFound,
                "RECLAMACION_NO_ENCONTRADA",
                "Reclamacion no encontrada");
        }

        if (!await _currentUser.PuedeVerReclamacionAsync(reclamacion, cancellationToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status403Forbidden,
                "RECLAMACION_SIN_ACCESO",
                "No tiene permiso para consultar esta reclamacion");
        }

        var permitidas = ReclamacionEstadoService
            .ObtenerTransicionesPermitidas(reclamacion.Estado)
            .ToList();

        if (!_currentUser.EsOperadorCaja)
        {
            permitidas = _currentUser.EsPrestadora
                ? permitidas.Where(x => x == "RESPONDIDA_POR_PRESTADORA").ToList()
                : new List<string>();
        }

        return Ok(new
        {
            reclamacionId = reclamacion.Id,
            estadoActual = ReclamacionEstadoService.NormalizarEstado(reclamacion.Estado),
            rol = _currentUser.Rol,
            transicionesPermitidas = permitidas,
            puedeCambiarEstado = _currentUser.EsOperadorCaja && permitidas.Count > 0,
            respuestaPrestadoraRequerida = _currentUser.EsPrestadora && permitidas.Contains("RESPONDIDA_POR_PRESTADORA")
        });
    }
}
