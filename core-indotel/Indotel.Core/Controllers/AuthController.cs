using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Indotel.Core.Controllers;

[EnableRateLimiting("AuthPolicy")]
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshTokenDuration = TimeSpan.FromDays(7);

    private readonly IndotelDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AuthController(IndotelDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var correo = request.Correo.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Correo == correo && x.Activo);

        if (usuario is null)
        {
            return Unauthorized(new { mensaje = "Credenciales invalidas" });
        }

        if (usuario.LockoutEnd is not null && usuario.LockoutEnd > DateTime.UtcNow)
        {
            return StatusCode(StatusCodes.Status423Locked, new
            {
                mensaje = "Usuario bloqueado temporalmente por intentos fallidos",
                bloqueadoHasta = usuario.LockoutEnd
            });
        }

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);

        if (resultado == PasswordVerificationResult.Failed)
        {
            usuario.AccessFailedCount += 1;
            usuario.LastFailedLoginAt = DateTime.UtcNow;

            if (usuario.AccessFailedCount >= MaxFailedAttempts)
            {
                usuario.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                await _db.SaveChangesAsync();

                return StatusCode(StatusCodes.Status423Locked, new
                {
                    mensaje = "Usuario bloqueado temporalmente por intentos fallidos",
                    bloqueadoHasta = usuario.LockoutEnd
                });
            }

            await _db.SaveChangesAsync();
            return Unauthorized(new { mensaje = "Credenciales invalidas" });
        }

        usuario.AccessFailedCount = 0;
        usuario.LockoutEnd = null;
        usuario.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await CrearRespuestaLogin(usuario);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken(RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { mensaje = "Refresh token obligatorio" });
        }

        var tokenHash = CalcularHashToken(request.RefreshToken);
        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (refreshToken is null || refreshToken.FechaRevocacion is not null || refreshToken.ExpiraEn <= DateTime.UtcNow)
        {
            return Unauthorized(new { mensaje = "Refresh token invalido o expirado" });
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == refreshToken.UsuarioId && x.Activo);
        if (usuario is null)
        {
            return Unauthorized(new { mensaje = "Usuario no encontrado o inactivo" });
        }

        var nuevoRefreshToken = CrearRefreshToken();
        var nuevoRefreshExpira = DateTime.UtcNow.Add(RefreshTokenDuration);
        var nuevoRefreshHash = CalcularHashToken(nuevoRefreshToken);

        refreshToken.FechaRevocacion = DateTime.UtcNow;
        refreshToken.RevocadoPorIp = ObtenerIp();
        refreshToken.ReemplazadoPorTokenHash = nuevoRefreshHash;

        _db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuario.Id,
            TokenHash = nuevoRefreshHash,
            ExpiraEn = nuevoRefreshExpira,
            CreadoPorIp = ObtenerIp(),
            FechaCreacion = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return await CrearRespuestaLogin(usuario, nuevoRefreshToken, nuevoRefreshExpira);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { mensaje = "Refresh token obligatorio" });
        }

        var tokenHash = CalcularHashToken(request.RefreshToken);
        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (refreshToken is not null && refreshToken.FechaRevocacion is null)
        {
            refreshToken.FechaRevocacion = DateTime.UtcNow;
            refreshToken.RevocadoPorIp = ObtenerIp();
            await _db.SaveChangesAsync();
        }

        return Ok(new { mensaje = "Sesion cerrada correctamente" });
    }

    [HttpPost("register-ciudadano")]
    public async Task<ActionResult<LoginResponseDto>> RegisterCiudadano(RegisterCiudadanoDto request)
    {
        var errorClave = ValidarPassword(request.Password);
        if (errorClave is not null)
        {
            return BadRequest(new { mensaje = errorClave });
        }

        if (string.IsNullOrWhiteSpace(request.Cedula) || string.IsNullOrWhiteSpace(request.Nombres) || string.IsNullOrWhiteSpace(request.Apellidos) || string.IsNullOrWhiteSpace(request.Correo))
        {
            return BadRequest(new { mensaje = "Cedula, nombres, apellidos y correo son obligatorios" });
        }

        var correo = request.Correo.Trim().ToLowerInvariant();
        var cedula = request.Cedula.Trim();

        var existeUsuario = await _db.Usuarios.AnyAsync(x => x.Correo == correo);
        if (existeUsuario)
        {
            return Conflict(new { mensaje = "Ya existe un usuario registrado con este correo" });
        }

        var existeCiudadano = await _db.Ciudadanos.AnyAsync(x => x.Cedula == cedula);
        if (existeCiudadano)
        {
            return Conflict(new { mensaje = "Ya existe un ciudadano registrado con esta cedula" });
        }

        var rolCiudadano = await _db.Roles.FirstOrDefaultAsync(x => x.Nombre == "Ciudadano");
        if (rolCiudadano is null)
        {
            return BadRequest(new { mensaje = "No existe el rol Ciudadano en el sistema" });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var ciudadano = new Ciudadano
        {
            Cedula = cedula,
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Telefono = request.Telefono.Trim(),
            Correo = correo,
            Direccion = request.Direccion.Trim(),
            Activo = true
        };

        _db.Ciudadanos.Add(ciudadano);

        var usuario = new Usuario
        {
            NombreCompleto = $"{ciudadano.Nombres} {ciudadano.Apellidos}".Trim(),
            Correo = correo,
            RolId = rolCiudadano.Id,
            Activo = true
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Password);
        _db.Usuarios.Add(usuario);

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await CrearRespuestaLogin(usuario);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdText, out var userId))
        {
            return Unauthorized(new { mensaje = "Sesion invalida" });
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == userId && x.Activo);
        if (usuario is null)
        {
            return Unauthorized(new { mensaje = "Usuario no encontrado o inactivo" });
        }

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.PasswordActual);
        if (resultado == PasswordVerificationResult.Failed)
        {
            return BadRequest(new { mensaje = "La clave actual no es correcta" });
        }

        var errorClave = ValidarPassword(request.PasswordNueva);
        if (errorClave is not null)
        {
            return BadRequest(new { mensaje = errorClave });
        }

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.PasswordNueva);
        usuario.AccessFailedCount = 0;
        usuario.LockoutEnd = null;
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Clave actualizada correctamente" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
    {
        var correo = request.Correo.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Correo == correo && x.Activo);

        if (usuario is null)
        {
            return Ok(new { mensaje = "Si el correo existe, se generara un enlace de recuperacion." });
        }

        var expiraEn = DateTime.UtcNow.AddMinutes(30);
        var resetToken = CrearResetPasswordToken(usuario, expiraEn);

        return Ok(new
        {
            mensaje = "Token de recuperacion generado. En produccion este token debe enviarse por correo.",
            resetToken,
            expiraEn
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
    {
        var errorClave = ValidarPassword(request.PasswordNueva);
        if (errorClave is not null)
        {
            return BadRequest(new { mensaje = errorClave });
        }

        var principal = ValidarResetPasswordToken(request.Token);
        if (principal is null)
        {
            return BadRequest(new { mensaje = "Token de recuperacion invalido o expirado" });
        }

        var purpose = principal.FindFirstValue("purpose");
        var correo = principal.FindFirstValue(ClaimTypes.Email);

        if (purpose != "password_reset" || string.IsNullOrWhiteSpace(correo))
        {
            return BadRequest(new { mensaje = "Token de recuperacion invalido" });
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Correo == correo && x.Activo);
        if (usuario is null)
        {
            return BadRequest(new { mensaje = "Usuario no encontrado o inactivo" });
        }

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.PasswordNueva);
        usuario.AccessFailedCount = 0;
        usuario.LockoutEnd = null;
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Clave restablecida correctamente" });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            nombre = User.FindFirstValue(ClaimTypes.Name),
            correo = User.FindFirstValue(ClaimTypes.Email),
            rol = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    private async Task<LoginResponseDto> CrearRespuestaLogin(Usuario usuario, string? refreshTokenExistente = null, DateTime? refreshExpiraExistente = null)
    {
        var rol = await _db.Roles.FirstOrDefaultAsync(x => x.Id == usuario.RolId);
        var expiraEn = DateTime.UtcNow.AddHours(4);
        var token = CrearToken(usuario, rol?.Nombre ?? "SinRol", expiraEn);

        var refreshToken = refreshTokenExistente ?? CrearRefreshToken();
        var refreshExpira = refreshExpiraExistente ?? DateTime.UtcNow.Add(RefreshTokenDuration);

        if (refreshTokenExistente is null)
        {
            _db.RefreshTokens.Add(new RefreshToken
            {
                UsuarioId = usuario.Id,
                TokenHash = CalcularHashToken(refreshToken),
                ExpiraEn = refreshExpira.Value,
                CreadoPorIp = ObtenerIp(),
                FechaCreacion = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiraEn = expiraEn,
            RefreshTokenExpiraEn = refreshExpira.Value,
            Usuario = new UsuarioSesionDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                Rol = rol?.Nombre ?? "SinRol"
            }
        };
    }

    private string CrearToken(Usuario usuario, string rol, DateTime expiraEn)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, rol)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiraEn,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CrearRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string CalcularHashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private string ObtenerIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    private string CrearResetPasswordToken(Usuario usuario, DateTime expiraEn)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Correo),
            new("purpose", "password_reset")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiraEn,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidarResetPasswordToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            return tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out _);
        }
        catch
        {
            return null;
        }
    }

    private static string? ValidarPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return "La clave debe tener al menos 8 caracteres";
        }

        return null;
    }
}
