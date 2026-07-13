using INDOTEL.WEB.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<PortalTokenStore>();
builder.Services.AddScoped<PortalSessionService>();
builder.Services.AddTransient<GatewayTransportHandler>();

var gatewayBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:GatewayBaseUrl");
if (!Uri.TryCreate(gatewayBaseUrl, UriKind.Absolute, out var gatewayUri) ||
    gatewayUri.Scheme is not ("http" or "https"))
{
    throw new InvalidOperationException("ApiSettings:GatewayBaseUrl no está configurada correctamente.");
}

void ConfigureGatewayClient(HttpClient client)
{
    client.BaseAddress = gatewayUri;
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(20);
}

builder.Services.AddHttpClient("IndotelGateway", ConfigureGatewayClient)
    .AddHttpMessageHandler<GatewayTransportHandler>();

// Alias temporal para controladores existentes. Ambos nombres consumen exclusivamente el Gateway.
builder.Services.AddHttpClient("IndotelCore", ConfigureGatewayClient)
    .AddHttpMessageHandler<GatewayTransportHandler>();

var configuredKeysPath = builder.Configuration.GetValue<string>("Security:DataProtectionKeysPath");
var keysPath = string.IsNullOrWhiteSpace(configuredKeysPath)
    ? Path.Combine(builder.Environment.ContentRootPath, ".data-protection-keys")
    : Path.IsPathRooted(configuredKeysPath)
        ? configuredKeysPath
        : Path.Combine(builder.Environment.ContentRootPath, configuredKeysPath);
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("INDOTEL.PortalCiudadano");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.Name = "INDOTEL.Portal.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program
{
}
