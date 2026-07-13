using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Helpers;
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
public sealed class AuthController : ControllerBase
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IndotelDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AuthController(
        IndotelDbContext db,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _db = db;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Correo) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "CREDENCIALES_OBLIGATORIAS",
                "Correo y clave son obligatorios");
        }

        var correo = request.Correo.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(x => x.Correo == correo && x.Activo, cancellationToken);

        if (usuario is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "CREDENCIALES_INVALIDAS",
                "Credenciales invalidas");
        }

        if (usuario.LockoutEnd is not null && usuario.LockoutEnd > DateTime.UtcNow)
        {
            var result = ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status423Locked,
                "USUARIO_BLOQUEADO",
                "Usuario bloqueado temporalmente por intentos fallidos");
            ((ProblemDetails)result.Value!).Extensions["bloqueadoHasta"] = usuario.LockoutEnd;
            return result;
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            request.Password);

        if (resultado == PasswordVerificationResult.Failed)
        {
            usuario.AccessFailedCount += 1;
            usuario.LastFailedLoginAt = DateTime.UtcNow;

            if (usuario.AccessFailedCount >= MaxFailedAttempts)
            {
                usuario.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                await _db.SaveChangesAsync(cancellationToken);

                var result = ApiProblemFactory.Create(
                    HttpContext,
                    StatusCodes.Status423Locked,
                    "USUARIO_BLOQUEADO",
                    "Usuario bloqueado temporalmente por intentos fallidos");
                ((ProblemDetails)result.Value!).Extensions["bloqueadoHasta"] = usuario.LockoutEnd;
                return result;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "CREDENCIALES_INVALIDAS",
                "Credenciales invalidas");
        }

        usuario.AccessFailedCount = 0;
        usuario.LockoutEnd = null;
        usuario.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return await CrearRespuestaLogin(usuario, cancellationToken: cancellationToken);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "REFRESH_TOKEN_OBLIGATORIO",
                "Refresh token obligatorio");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var tokenHash = CalcularHashToken(request.RefreshToken);
        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "REFRESH_TOKEN_INVALIDO",
                "Refresh token invalido o expirado");
        }

        if (refreshToken.FechaRevocacion is not null)
        {
            if (!string.IsNullOrWhiteSpace(refreshToken.ReemplazadoPorTokenHash))
            {
                await RevocarTokensActivosAsync(refreshToken.UsuarioId, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ApiProblemFactory.Create(
                    HttpContext,
                    StatusCodes.Status401Unauthorized,
                    "REFRESH_TOKEN_REUTILIZADO",
                    "Se detecto reutilizacion de un refresh token. Todas las sesiones fueron revocadas.");
            }

            await transaction.RollbackAsync(cancellationToken);
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "REFRESH_TOKEN_REVOCADO",
                "Refresh token invalido o expirado");
        }

        if (refreshToken.ExpiraEn <= DateTime.UtcNow)
        {
            refreshToken.FechaRevocacion = DateTime.UtcNow;
            refreshToken.RevocadoPorIp = ObtenerIp();
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "REFRESH_TOKEN_EXPIRADO",
                "Refresh token invalido o expirado");
        }

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(
                x => x.Id == refreshToken.UsuarioId && x.Activo,
                cancellationToken);

        if (usuario is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "USUARIO_INACTIVO",
                "Usuario no encontrado o inactivo");
        }

        var nuevoRefreshToken = CrearRefreshToken();
        var nuevoRefreshExpira = DateTime.UtcNow.Add(ObtenerDuracionRefreshToken());
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

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await CrearRespuestaLogin(
            usuario,
            nuevoRefreshToken,
            nuevoRefreshExpira,
            cancellationToken);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "REFRESH_TOKEN_OBLIGATORIO",
                "Refresh token obligatorio");
        }

        var userId = ObtenerUsuarioIdActual();
        if (userId is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "SESION_INVALIDA",
                "Sesion invalida");
        }

        var tokenHash = CalcularHashToken(request.RefreshToken);
        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(
            x => x.TokenHash == tokenHash && x.UsuarioId == userId.Value,
            cancellationToken);

        if (refreshToken is not null && refreshToken.FechaRevocacion is null)
        {
            refreshToken.FechaRevocacion = DateTime.UtcNow;
            refreshToken.RevocadoPorIp = ObtenerIp();
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Ok(new
        {
            mensaje = "Sesion cerrada correctamente",
            correlationId = HttpContext.TraceIdentifier
        });
    }

    [HttpPost("register-ciudadano")]
    public async Task<ActionResult<LoginResponseDto>> RegisterCiudadano(
        RegisterCiudadanoDto request,
        CancellationToken cancellationToken)
    {
        var errorClave = ValidarPassword(request.Password);
        if (errorClave is not null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "CLAVE_NO_VALIDA",
                errorClave);
        }

        if (string.IsNullOrWhiteSpace(request.Cedula) ||
            string.IsNullOrWhiteSpace(request.Nombres) ||
            string.IsNullOrWhiteSpace(request.Apellidos) ||
            string.IsNullOrWhiteSpace(request.Correo))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "DATOS_CIUDADANO_INCOMPLETOS",
                "Cedula, nombres, apellidos y correo son obligatorios");
        }

        var correo = request.Correo.Trim().ToLowerInvariant();
        var cedula = request.Cedula.Trim();

        if (!correo.Contains('@'))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "CORREO_NO_VALIDO",
                "El correo no tiene un formato valido");
        }

        if (await _db.Usuarios.AnyAsync(x => x.Correo == correo, cancellationToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status409Conflict,
                "CORREO_DUPLICADO",
                "Ya existe un usuario registrado con este correo");
        }

        if (await _db.Ciudadanos.AnyAsync(x => x.Cedula == cedula, cancellationToken))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status409Conflict,
                "CEDULA_DUPLICADA",
                "Ya existe un ciudadano registrado con esta cedula");
        }

        var rolCiudadano = await _db.Roles
            .FirstOrDefaultAsync(x => x.Nombre == "Ciudadano", cancellationToken);

        if (rolCiudadano is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status500InternalServerError,
                "ROL_CIUDADANO_NO_CONFIGURADO",
                "No existe el rol Ciudadano en el sistema");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

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

        var usuario = new Usuario
        {
            NombreCompleto = $"{ciudadano.Nombres} {ciudadano.Apellidos}".Trim(),
            Correo = correo,
            RolId = rolCiudadano.Id,
            Activo = true
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Password);
        _db.Usuarios.Add(usuario);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await CrearRespuestaLogin(usuario, cancellationToken: cancellationToken);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordDto request,
        CancellationToken cancellationToken)
    {
        var userId = ObtenerUsuarioIdActual();
        if (userId is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "SESION_INVALIDA",
                "Sesion invalida");
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(
            x => x.Id == userId.Value && x.Activo,
            cancellationToken);

        if (usuario is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "USUARIO_INACTIVO",
                "Usuario no encontrado o inactivo");
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            request.PasswordActual);

        if (resultado == PasswordVerificationResult.Failed)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "CLAVE_ACTUAL_INCORRECTA",
                "La clave actual no es correcta");
        }

        var errorClave = ValidarPassword(request.PasswordNueva);
        if (errorClave is not null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "CLAVE_NO_VALIDA",
                errorClave);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.PasswordNueva);
        usuario.AccessFailedCount = 0;
        usuario.LockoutEnd = null;
        await RevocarTokensActivosAsync(usuario.Id, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new
        {
            mensaje = "Clave actualizada correctamente. Las sesiones anteriores fueron revocadas.",
            correlationId = HttpContext.TraceIdentifier
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordDto request,
        CancellationToken cancellationToken)
    {
        const string mensajeGenerico =
            "Si el correo existe, se generara un enlace de recuperacion.";

        if (string.IsNullOrWhiteSpace(request.Correo))
        {
            return Ok(new { mensaje = mensajeGenerico });
        }

        var correo = request.Correo.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(
            x => x.Correo == correo && x.Activo,
            cancellationToken);

        if (usuario is null)
        {
            return Ok(new { mensaje = mensajeGenerico });
        }

        var expiraEn = DateTime.UtcNow.AddMinutes(30);
        var resetToken = CrearResetPasswordToken(usuario, expiraEn);
        var exponerToken = _environment.IsDevelopment() &&
                           _configuration.GetValue<bool>("Security:ExposeResetTokenInResponse");

        if (exponerToken)
        {
            return Ok(new
            {
                mensaje = mensajeGenerico,
                resetToken,
                expiraEn,
                soloDesarrollo = true
            });
        }

        // En producción el token debe enviarse mediante un proveedor de correo.
        return Ok(new { mensaje = mensajeGenerico });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordDto request,
        CancellationToken cancellationToken)
    {
        var errorClave = ValidarPassword(request.PasswordNueva);
        if (errorClave is not null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "CLAVE_NO_VALIDA",
                errorClave);
        }

        var principal = ValidarResetPasswordToken(request.Token);
        if (principal is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "TOKEN_RECUPERACION_INVALIDO",
                "Token de recuperacion invalido o expirado");
        }

        var purpose = principal.FindFirstValue("purpose");
        var correo = principal.FindFirstValue(ClaimTypes.Email);

        if (purpose != "password_reset" || string.IsNullOrWhiteSpace(correo))
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "TOKEN_RECUPERACION_INVALIDO",
                "Token de recuperacion invalido");
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(
            x => x.Correo == correo && x.Activo,
            cancellationToken);

        if (usuario is null)
        {
            return ApiProblemFactory.Create(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "USUARIO_INACTIVO",
                "Usuario no encontrado o inactivo");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.PasswordNueva);
        usuario.AccessFailedCount = 0;
        usuario.LockoutEnd = null;
        await RevocarTokensActivosAsync(usuario.Id, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new
        {
            mensaje = "Clave restablecida correctamente. Las sesiones anteriores fueron revocadas.",
            correlationId = HttpContext.TraceIdentifier
        });
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

    private async Task<LoginResponseDto> CrearRespuestaLogin(
        Usuario usuario,
        string? refreshTokenExistente = null,
        DateTime? refreshExpiraExistente = null,
        CancellationToken cancellationToken = default)
    {
        var rol = await _db.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == usuario.RolId, cancellationToken);

        var expiraEn = DateTime.UtcNow.Add(ObtenerDuracionAccessToken());
        var token = CrearToken(usuario, rol?.Nombre ?? "SinRol", expiraEn);

        var refreshToken = refreshTokenExistente ?? CrearRefreshToken();
        var refreshExpira = refreshExpiraExistente ?? DateTime.UtcNow.Add(ObtenerDuracionRefreshToken());

        if (refreshTokenExistente is null)
        {
            _db.RefreshTokens.Add(new RefreshToken
            {
                UsuarioId = usuario.Id,
                TokenHash = CalcularHashToken(refreshToken),
                ExpiraEn = refreshExpira,
                CreadoPorIp = ObtenerIp(),
                FechaCreacion = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(cancellationToken);
        }

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiraEn = expiraEn,
            RefreshTokenExpiraEn = refreshExpira,
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

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiraEn,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task RevocarTokensActivosAsync(
        int usuarioId,
        CancellationToken cancellationToken)
    {
        var ahora = DateTime.UtcNow;
        var tokens = await _db.RefreshTokens
            .Where(x => x.UsuarioId == usuarioId && x.FechaRevocacion == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.FechaRevocacion = ahora;
            token.RevocadoPorIp = ObtenerIp();
        }
    }

    private int? ObtenerUsuarioIdActual()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    private TimeSpan ObtenerDuracionAccessToken()
    {
        var minutes = Math.Clamp(
            _configuration.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 30,
            5,
            240);
        return TimeSpan.FromMinutes(minutes);
    }

    private TimeSpan ObtenerDuracionRefreshToken()
    {
        var days = Math.Clamp(
            _configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 7,
            1,
            30);
        return TimeSpan.FromDays(days);
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

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiraEn,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidarResetPasswordToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

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
            return "La clave debe tener al menos 8 caracteres";
        if (!password.Any(char.IsUpper))
            return "La clave debe incluir al menos una letra mayuscula";
        if (!password.Any(char.IsLower))
            return "La clave debe incluir al menos una letra minuscula";
        if (!password.Any(char.IsDigit))
            return "La clave debe incluir al menos un numero";
        if (!password.Any(x => !char.IsLetterOrDigit(x)))
            return "La clave debe incluir al menos un caracter especial";

        return null;
    }
}
