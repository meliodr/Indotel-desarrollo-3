using Indotel.Core.Constants;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Helpers;
using Indotel.Core.Models;
using Indotel.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/ciudadanos")]
public sealed class CiudadanosController : ControllerBase
{
    private readonly IndotelDbContext _db;
    private readonly CurrentUserService _currentUser;

    public CiudadanosController(IndotelDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [Authorize(Policy = AuthorizationPolicies.CajaConsulta)]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var data = await _db.Ciudadanos
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
        return Ok(data);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Ciudadano")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if (!await _currentUser.PuedeAccederCiudadanoAsync(id, cancellationToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status403Forbidden,
                "CIUDADANO_SIN_ACCESO",
                "No tiene permiso para consultar este ciudadano");
        }

        var ciudadano = await _db.Ciudadanos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return ciudadano is null
            ? ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status404NotFound,
                "CIUDADANO_NO_ENCONTRADO",
                "Ciudadano no encontrado")
            : Ok(ciudadano);
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

        var ciudadano = await _db.Ciudadanos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Cedula == valor, cancellationToken);

        return ciudadano is null
            ? ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status404NotFound,
                "CIUDADANO_NO_ENCONTRADO",
                "No existe un ciudadano con esta cedula")
            : Ok(ciudadano);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Ciudadano")]
    [HttpGet("{id:int}/reclamaciones")]
    public async Task<IActionResult> GetReclamacionesByCiudadano(
        int id,
        CancellationToken cancellationToken)
    {
        if (!await _currentUser.PuedeAccederCiudadanoAsync(id, cancellationToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status403Forbidden,
                "CIUDADANO_SIN_ACCESO",
                "No tiene permiso para consultar las reclamaciones de este ciudadano");
        }

        var ciudadanoExiste = await _db.Ciudadanos
            .AsNoTracking()
            .AnyAsync(x => x.Id == id, cancellationToken);

        if (!ciudadanoExiste)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status404NotFound,
                "CIUDADANO_NO_ENCONTRADO",
                "Ciudadano no encontrado");
        }

        var data = await _db.Reclamaciones
            .AsNoTracking()
            .Where(x => x.CiudadanoId == id)
            .OrderByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    [Authorize(Policy = AuthorizationPolicies.CajaOperador)]
    [HttpPost]
    public async Task<IActionResult> Create(
        CiudadanoCreateDto request,
        CancellationToken cancellationToken)
    {
        var error = ValidarDatos(
            request.Cedula,
            request.Nombres,
            request.Apellidos,
            request.Correo);

        if (error is not null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "DATOS_CIUDADANO_NO_VALIDOS",
                error);
        }

        var cedula = request.Cedula.Trim();
        var correo = request.Correo.Trim().ToLowerInvariant();

        if (await _db.Ciudadanos.AnyAsync(x => x.Cedula == cedula, cancellationToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status409Conflict,
                "CEDULA_DUPLICADA",
                "Ya existe un ciudadano con esta cedula");
        }

        if (await _db.Ciudadanos.AnyAsync(x => x.Correo == correo, cancellationToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status409Conflict,
                "CORREO_DUPLICADO",
                "Ya existe un ciudadano con este correo");
        }

        var ciudadano = new Ciudadano
        {
            Cedula = cedula,
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Telefono = (request.Telefono ?? string.Empty).Trim(),
            Correo = correo,
            Direccion = (request.Direccion ?? string.Empty).Trim(),
            Activo = true
        };

        _db.Ciudadanos.Add(ciudadano);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = ciudadano.Id }, ciudadano);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Ciudadano")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        CiudadanoUpdateDto request,
        CancellationToken cancellationToken)
    {
        if (!await _currentUser.PuedeAccederCiudadanoAsync(id, cancellationToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status403Forbidden,
                "CIUDADANO_SIN_ACCESO",
                "No tiene permiso para modificar este ciudadano");
        }

        var ciudadano = await _db.Ciudadanos
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ciudadano is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status404NotFound,
                "CIUDADANO_NO_ENCONTRADO",
                "Ciudadano no encontrado");
        }

        var error = ValidarDatos(
            ciudadano.Cedula,
            request.Nombres,
            request.Apellidos,
            request.Correo);

        if (error is not null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "DATOS_CIUDADANO_NO_VALIDOS",
                error);
        }

        var correoNuevo = request.Correo.Trim().ToLowerInvariant();
        var correoAnterior = ciudadano.Correo;

        if (_currentUser.EsCiudadano &&
            !correoNuevo.Equals(correoAnterior, StringComparison.OrdinalIgnoreCase))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status409Conflict,
                "CAMBIO_CORREO_NO_PERMITIDO",
                "El ciudadano no puede cambiar su correo desde este recurso");
        }

        if (await _db.Ciudadanos.AnyAsync(
                x => x.Id != id && x.Correo == correoNuevo,
                cancellationToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status409Conflict,
                "CORREO_DUPLICADO",
                "Ya existe otro ciudadano con este correo");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        ciudadano.Nombres = request.Nombres.Trim();
        ciudadano.Apellidos = request.Apellidos.Trim();
        ciudadano.Telefono = (request.Telefono ?? string.Empty).Trim();
        ciudadano.Correo = correoNuevo;
        ciudadano.Direccion = (request.Direccion ?? string.Empty).Trim();

        if (!_currentUser.EsCiudadano &&
            !correoNuevo.Equals(correoAnterior, StringComparison.OrdinalIgnoreCase))
        {
            if (await _db.Usuarios.AnyAsync(
                    x => x.Correo == correoNuevo,
                    cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return ApiProblemFactory.Create(
                    HttpContext,
                    StatusCodes.Status409Conflict,
                    "CORREO_USUARIO_DUPLICADO",
                    "Ya existe un usuario con este correo");
            }

            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(x => x.Correo == correoAnterior, cancellationToken);

            if (usuario is not null)
            {
                usuario.Correo = correoNuevo;
                usuario.NombreCompleto = $"{ciudadano.Nombres} {ciudadano.Apellidos}".Trim();
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(ciudadano);
    }

    private static string? ValidarDatos(
        string cedula,
        string nombres,
        string apellidos,
        string correo)
    {
        var cedulaNormalizada = (cedula ?? string.Empty).Replace("-", string.Empty).Trim();
        if (cedulaNormalizada.Length != 11 || !cedulaNormalizada.All(char.IsDigit))
            return "La cedula debe contener 11 digitos";
        if (string.IsNullOrWhiteSpace(nombres)) return "Los nombres son obligatorios";
        if (string.IsNullOrWhiteSpace(apellidos)) return "Los apellidos son obligatorios";
        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
            return "El correo no tiene un formato valido";

        return null;
    }
}
