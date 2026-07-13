using INDOTEL.WEB.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace INDOTEL.WEB.Services;

public enum PortalRefreshResult
{
    Success,
    InvalidSession,
    TemporaryFailure
}

public sealed class PortalSessionService
{
    public const string SessionIdClaim = "PortalSessionId";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> RefreshLocks = new();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PortalTokenStore _tokenStore;
    private readonly ILogger<PortalSessionService> _logger;

    public PortalSessionService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        PortalTokenStore tokenStore,
        ILogger<PortalSessionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _tokenStore = tokenStore;
        _logger = logger;
    }

    private HttpContext HttpContext =>
        _httpContextAccessor.HttpContext
        ?? throw new InvalidOperationException("No existe un contexto HTTP activo.");

    public async Task<HttpClient> CrearClienteAutorizadoAsync(
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("IndotelGateway");
        var sessionId = ObtenerSessionId();
        if (string.IsNullOrWhiteSpace(sessionId)) return client;

        var state = await _tokenStore.GetAsync(sessionId, cancellationToken);
        if (state is null || state.RefreshTokenExpiraEn <= DateTimeOffset.UtcNow)
        {
            await CerrarSesionLocalAsync(cancellationToken);
            return client;
        }

        if (DebeRenovarToken(state))
        {
            var result = await RenovarSesionAsync(cancellationToken);
            if (result == PortalRefreshResult.TemporaryFailure)
            {
                throw new GatewayUnavailableException(
                    "No fue posible renovar la sesión porque el servicio central no está disponible.",
                    "RENOVACION_TEMPORALMENTE_NO_DISPONIBLE",
                    timeout: false);
            }

            if (result == PortalRefreshResult.InvalidSession)
            {
                return client;
            }

            state = await _tokenStore.GetAsync(sessionId, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(state?.AccessToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", state.AccessToken);
        }

        return client;
    }

    public async Task IniciarSesionAsync(
        LoginResponse loginResult,
        CancellationToken cancellationToken = default)
    {
        var oldSessionId = ObtenerSessionId();
        if (!string.IsNullOrWhiteSpace(oldSessionId))
        {
            await _tokenStore.RemoveAsync(oldSessionId, cancellationToken);
        }

        var sessionId = Guid.NewGuid().ToString("N");
        var state = CrearTokenState(loginResult);
        await _tokenStore.SaveAsync(sessionId, state, cancellationToken);
        await GuardarCookieAsync(loginResult, sessionId, cancellationToken);
    }

    public async Task<PortalRefreshResult> RenovarSesionAsync(
        CancellationToken cancellationToken = default)
    {
        var sessionId = ObtenerSessionId();
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            await CerrarSesionLocalAsync(cancellationToken);
            return PortalRefreshResult.InvalidSession;
        }

        var refreshLock = RefreshLocks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
        await refreshLock.WaitAsync(cancellationToken);

        try
        {
            var state = await _tokenStore.GetAsync(sessionId, cancellationToken);
            if (state is null ||
                string.IsNullOrWhiteSpace(state.RefreshToken) ||
                state.RefreshTokenExpiraEn <= DateTimeOffset.UtcNow)
            {
                await CerrarSesionLocalAsync(cancellationToken);
                return PortalRefreshResult.InvalidSession;
            }

            if (!DebeRenovarToken(state))
            {
                return PortalRefreshResult.Success;
            }

            var client = _httpClientFactory.CreateClient("IndotelGateway");
            using var contenido = new StringContent(
                JsonSerializer.Serialize(new { RefreshToken = state.RefreshToken }),
                Encoding.UTF8,
                "application/json");

            using var response = await client.PostAsync(
                "/api/auth/refresh-token",
                contenido,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.BadRequest or
                HttpStatusCode.Unauthorized or
                HttpStatusCode.Forbidden)
            {
                await CerrarSesionLocalAsync(cancellationToken);
                return PortalRefreshResult.InvalidSession;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Renovación temporalmente rechazada. StatusCode={StatusCode} CorrelationId={CorrelationId}",
                    (int)response.StatusCode,
                    HttpContext.TraceIdentifier);
                return PortalRefreshResult.TemporaryFailure;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var loginResult = JsonSerializer.Deserialize<LoginResponse>(json, JsonOptions);

            if (!EsRespuestaLoginValida(loginResult) ||
                !loginResult!.Usuario.Rol.Equals("Ciudadano", StringComparison.OrdinalIgnoreCase))
            {
                await CerrarSesionLocalAsync(cancellationToken);
                return PortalRefreshResult.InvalidSession;
            }

            await _tokenStore.SaveAsync(
                sessionId,
                CrearTokenState(loginResult),
                cancellationToken);
            await GuardarCookieAsync(loginResult, sessionId, cancellationToken);

            return PortalRefreshResult.Success;
        }
        catch (GatewayUnavailableException ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudo renovar la sesión por indisponibilidad temporal. CorrelationId={CorrelationId}",
                HttpContext.TraceIdentifier);
            return PortalRefreshResult.TemporaryFailure;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Respuesta inválida durante renovación. CorrelationId={CorrelationId}",
                HttpContext.TraceIdentifier);
            return PortalRefreshResult.TemporaryFailure;
        }
        finally
        {
            refreshLock.Release();
        }
    }

    public async Task CerrarSesionAsync(
        CancellationToken cancellationToken = default)
    {
        var sessionId = ObtenerSessionId();
        var state = string.IsNullOrWhiteSpace(sessionId)
            ? null
            : await _tokenStore.GetAsync(sessionId, cancellationToken);

        try
        {
            if (state is not null &&
                !string.IsNullOrWhiteSpace(state.AccessToken) &&
                !string.IsNullOrWhiteSpace(state.RefreshToken))
            {
                var client = _httpClientFactory.CreateClient("IndotelGateway");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", state.AccessToken);

                using var contenido = new StringContent(
                    JsonSerializer.Serialize(new LogoutRequest
                    {
                        RefreshToken = state.RefreshToken
                    }),
                    Encoding.UTF8,
                    "application/json");

                await client.PostAsync(
                    "/api/auth/logout",
                    contenido,
                    cancellationToken);
            }
        }
        catch (GatewayUnavailableException)
        {
            // La revocación remota puede completarse después; la sesión local se elimina siempre.
        }
        finally
        {
            await CerrarSesionLocalAsync(cancellationToken);
        }
    }

    public async Task RevocarRespuestaLoginAsync(
        LoginResponse loginResult,
        CancellationToken cancellationToken = default)
    {
        if (!EsRespuestaLoginValida(loginResult)) return;

        try
        {
            var client = _httpClientFactory.CreateClient("IndotelGateway");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResult.Token);

            using var contenido = new StringContent(
                JsonSerializer.Serialize(new LogoutRequest
                {
                    RefreshToken = loginResult.RefreshToken
                }),
                Encoding.UTF8,
                "application/json");

            await client.PostAsync(
                "/api/auth/logout",
                contenido,
                cancellationToken);
        }
        catch (GatewayUnavailableException)
        {
            // No se interrumpe el portal si la revocación remota falla.
        }
    }

    public static bool EsRespuestaLoginValida(LoginResponse? loginResult)
    {
        return loginResult is not null &&
               loginResult.Usuario is not null &&
               !string.IsNullOrWhiteSpace(loginResult.Token) &&
               !string.IsNullOrWhiteSpace(loginResult.RefreshToken) &&
               loginResult.Usuario.Id > 0 &&
               !string.IsNullOrWhiteSpace(loginResult.Usuario.Correo) &&
               !string.IsNullOrWhiteSpace(loginResult.Usuario.Rol);
    }

    private async Task GuardarCookieAsync(
        LoginResponse loginResult,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var principal = CrearPrincipal(loginResult, sessionId);
        var expiraCookie = ConvertirAFechaUtc(loginResult.RefreshTokenExpiraEn);

        if (expiraCookie <= DateTimeOffset.UtcNow)
        {
            expiraCookie = DateTimeOffset.UtcNow.AddMinutes(30);
        }

        var propiedades = new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true,
            ExpiresUtc = expiraCookie
        };

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            propiedades: null);
        HttpContext.User = principal;
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            propiedades);

        cancellationToken.ThrowIfCancellationRequested();
    }

    private static ClaimsPrincipal CrearPrincipal(
        LoginResponse loginResult,
        string sessionId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, loginResult.Usuario.Id.ToString()),
            new(ClaimTypes.Name, loginResult.Usuario.NombreCompleto),
            new(ClaimTypes.Email, loginResult.Usuario.Correo),
            new(ClaimTypes.Role, loginResult.Usuario.Rol),
            new(SessionIdClaim, sessionId)
        };

        var identidad = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identidad);
    }

    private static PortalTokenState CrearTokenState(LoginResponse loginResult) => new()
    {
        AccessToken = loginResult.Token,
        RefreshToken = loginResult.RefreshToken,
        AccessTokenExpiraEn = ConvertirAFechaUtc(loginResult.ExpiraEn),
        RefreshTokenExpiraEn = ConvertirAFechaUtc(loginResult.RefreshTokenExpiraEn)
    };

    private static bool DebeRenovarToken(PortalTokenState state) =>
        state.AccessTokenExpiraEn <= DateTimeOffset.UtcNow.AddMinutes(2);

    private string? ObtenerSessionId() =>
        HttpContext.User.FindFirstValue(SessionIdClaim);

    private static DateTimeOffset ConvertirAFechaUtc(DateTime fecha)
    {
        var fechaUtc = fecha.Kind == DateTimeKind.Utc
            ? fecha
            : fecha.ToUniversalTime();

        return new DateTimeOffset(fechaUtc);
    }

    private async Task CerrarSesionLocalAsync(
        CancellationToken cancellationToken = default)
    {
        var sessionId = ObtenerSessionId();
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            await _tokenStore.RemoveAsync(sessionId, cancellationToken);
            RefreshLocks.TryRemove(sessionId, out _);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
    }
}
