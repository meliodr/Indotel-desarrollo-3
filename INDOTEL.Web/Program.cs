using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddControllersWithViews();

// 1. Configurar el HttpClient para consumir el Core/Middleware
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl");
builder.Services.AddHttpClient("IndotelCore", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl ?? throw new InvalidOperationException("API Base URL no configurada."));
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// 2. Configurar Autenticación por Cookies para mantener la sesión en la Web
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Pantalla a redirigir si no está autenticado
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Error 403
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // Tiempo de expiración de la sesión
    });

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// El orden de estos dos es estrictamente necesario
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();