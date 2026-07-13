using System.Security.Claims;
using Indotel.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize(Roles = "Ciudadano")]
[ApiController]
[Route("api/ciudadanos")]
public class MiPerfilCiudadanoController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public MiPerfilCiudadanoController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var correo = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(correo))
        {
            return Unauthorized(new { mensaje = "La sesión no contiene un correo válido" });
        }

        var ciudadano = await _db.Ciudadanos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Correo == correo && x.Activo);

        if (ciudadano is null)
        {
            return NotFound(new { mensaje = "No se encontró el perfil ciudadano asociado a la sesión" });
        }

        return Ok(new
        {
            ciudadano.Id,
            ciudadano.Cedula,
            ciudadano.Nombres,
            ciudadano.Apellidos,
            ciudadano.Correo,
            ciudadano.Telefono,
            ciudadano.Direccion
        });
    }
}
