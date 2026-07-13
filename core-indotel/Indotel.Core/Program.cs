using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Indotel.Core.Constants;
using Indotel.Core.Data;
using Indotel.Core.Filters;
using Indotel.Core.Helpers;
using Indotel.Core.Middleware;
using Indotel.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión DefaultConnection no está configurada.");
}

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key debe contener al menos 32 bytes.");
}

if (!builder.Environment.IsDevelopment() &&
    jwtKey.Contains("CAMBIAR_ESTA_CLAVE", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("La clave JWT de ejemplo no puede utilizarse fuera de Development.");
}

builder.Services.AddProblemDetails();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiProblemResultFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddDbContext<IndotelDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                if (context.Response.HasStarted) return;

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                var problem = ApiProblemFactory.Build(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    "AUTENTICACION_REQUERIDA",
                    "La sesión no es válida o ha expirado");
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            },
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted) return;

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";
                var problem = ApiProblemFactory.Build(
                    context.HttpContext,
                    StatusCodes.Status403Forbidden,
                    "ACCESO_DENEGADO",
                    "No tiene permiso para ejecutar esta operación");
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.CajaOperador,
        policy => policy.RequireRole("Administrador", "AnalistaDAU"));
    options.AddPolicy(AuthorizationPolicies.CajaConsulta,
        policy => policy.RequireRole("Administrador", "AnalistaDAU", "Auditor"));
    options.AddPolicy(AuthorizationPolicies.AuditoriaLectura,
        policy => policy.RequireRole("Administrador", "Auditor"));
    options.AddPolicy(AuthorizationPolicies.Administracion,
        policy => policy.RequireRole("Administrador"));
    options.AddPolicy(AuthorizationPolicies.Prestadora,
        policy => policy.RequireRole("Prestadora"));
    options.AddPolicy(AuthorizationPolicies.Ciudadano,
        policy => policy.RequireRole("Ciudadano"));
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "INDOTEL Core API",
        Version = "v1",
        Description = "Backend principal del Sistema Digital INDOTEL. Proyecto academico."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite el token JWT. Ejemplo: Bearer eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCors", policy =>
    {
        if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.SetIsOriginAllowed(_ => false);
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

var seedEnabled = app.Environment.IsDevelopment() ||
                  app.Configuration.GetValue<bool>("SeedData:Enabled");

if (seedEnabled)
{
    await SeedData.InitializeAsync(app.Services);
}
else if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<IndotelDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors("ApiCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TransactionalRequestMiddleware>();

app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;
    if (response.HasStarted || response.ContentLength > 0) return;

    var status = response.StatusCode;
    var code = status switch
    {
        StatusCodes.Status404NotFound => "RUTA_NO_ENCONTRADA",
        StatusCodes.Status405MethodNotAllowed => "METODO_NO_PERMITIDO",
        StatusCodes.Status429TooManyRequests => "LIMITE_SOLICITUDES_EXCEDIDO",
        _ => $"HTTP_{status}"
    };

    var message = status switch
    {
        StatusCodes.Status404NotFound => "La ruta o el recurso solicitado no existe",
        StatusCodes.Status405MethodNotAllowed => "El método HTTP no está permitido para esta ruta",
        StatusCodes.Status429TooManyRequests => "Se excedió el límite temporal de solicitudes",
        _ => "La solicitud no pudo completarse"
    };

    response.ContentType = "application/problem+json";
    var problem = ApiProblemFactory.Build(statusContext.HttpContext, status, code, message);
    await response.WriteAsync(JsonSerializer.Serialize(problem));
});

app.MapControllers();
app.Run();

public partial class Program
{
}
