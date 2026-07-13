using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize(Roles = "Administrador")]
[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IndotelDbContext _db;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public UsuariosController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await CrearConsultaUsuarios()
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await CrearConsultaUsuarios().FirstOrDefaultAsync(x => x.Id == id);
        return usuario is null ? NotFound(new { mensaje = "Usuario no encontrado" }) : Ok(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UsuarioCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCompleto) || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Clave))
        {
            return BadRequest(new { mensaje = "Nombre, correo y clave son obligatorios" });
        }

        if (request.Clave.Length < 6)
        {
            return BadRequest(new { mensaje = "La clave debe tener al menos 6 caracteres" });
        }

        var rolExiste = await _db.Roles.AnyAsync(x => x.Id == request.RolId && x.Activo);
        if (!rolExiste)
        {
            return BadRequest(new { mensaje = "Rol no valido" });
        }

        if (await _db.Usuarios.AnyAsync(x => x.Correo == request.Correo))
        {
            return Conflict(new { mensaje = "Ya existe un usuario con este correo" });
        }

        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto,
            Correo = request.Correo,
            RolId = request.RolId,
            Activo = request.Activo,
            FechaCreacion = DateTime.UtcNow
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Clave);

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        var response = await CrearConsultaUsuarios().FirstAsync(x => x.Id == usuario.Id);
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UsuarioUpdateDto request)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado" });

        if (string.IsNullOrWhiteSpace(request.NombreCompleto) || string.IsNullOrWhiteSpace(request.Correo))
        {
            return BadRequest(new { mensaje = "Nombre y correo son obligatorios" });
        }

        var rolExiste = await _db.Roles.AnyAsync(x => x.Id == request.RolId && x.Activo);
        if (!rolExiste)
        {
            return BadRequest(new { mensaje = "Rol no valido" });
        }

        var correoEnUso = await _db.Usuarios.AnyAsync(x => x.Correo == request.Correo && x.Id != id);
        if (correoEnUso)
        {
            return Conflict(new { mensaje = "Ya existe otro usuario con este correo" });
        }

        usuario.NombreCompleto = request.NombreCompleto;
        usuario.Correo = request.Correo;
        usuario.RolId = request.RolId;
        usuario.Activo = request.Activo;

        await _db.SaveChangesAsync();

        var response = await CrearConsultaUsuarios().FirstAsync(x => x.Id == usuario.Id);
        return Ok(response);
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, UsuarioEstadoDto request)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado" });

        usuario.Activo = request.Activo;
        await _db.SaveChangesAsync();

        var response = await CrearConsultaUsuarios().FirstAsync(x => x.Id == usuario.Id);
        return Ok(response);
    }

    [HttpPut("{id:int}/clave")]
    public async Task<IActionResult> CambiarClave(int id, UsuarioClaveDto request)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado" });

        if (string.IsNullOrWhiteSpace(request.NuevaClave) || request.NuevaClave.Length < 6)
        {
            return BadRequest(new { mensaje = "La nueva clave debe tener al menos 6 caracteres" });
        }

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.NuevaClave);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Clave actualizada correctamente" });
    }

    private IQueryable<UsuarioResponseDto> CrearConsultaUsuarios()
    {
        return from usuario in _db.Usuarios
               join rol in _db.Roles on usuario.RolId equals rol.Id into roles
               from rol in roles.DefaultIfEmpty()
               select new UsuarioResponseDto
               {
                   Id = usuario.Id,
                   NombreCompleto = usuario.NombreCompleto,
                   Correo = usuario.Correo,
                   RolId = usuario.RolId,
                   Rol = rol != null ? rol.Nombre : "SinRol",
                   Activo = usuario.Activo,
                   FechaCreacion = usuario.FechaCreacion
               };
    }
}
