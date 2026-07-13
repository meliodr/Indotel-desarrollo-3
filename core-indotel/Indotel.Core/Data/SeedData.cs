using Indotel.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IndotelDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var passwordHasher = new PasswordHasher<Usuario>();

        await db.Database.MigrateAsync();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Rol { Nombre = "Administrador", Descripcion = "Acceso total al sistema" },
                new Rol { Nombre = "AnalistaDAU", Descripcion = "Gestiona reclamaciones ciudadanas" },
                new Rol { Nombre = "Prestadora", Descripcion = "Consulta y responde casos asignados" },
                new Rol { Nombre = "Ciudadano", Descripcion = "Registra y consulta sus reclamaciones" },
                new Rol { Nombre = "Auditor", Descripcion = "Consulta reportes y auditoria" }
            );
        }

        if (!await db.ServiciosTelecom.AnyAsync())
        {
            db.ServiciosTelecom.AddRange(
                new ServicioTelecom { Nombre = "Internet", Descripcion = "Servicio de internet fijo o movil" },
                new ServicioTelecom { Nombre = "Telefonia movil", Descripcion = "Servicio de llamadas y datos moviles" },
                new ServicioTelecom { Nombre = "Telefonia fija", Descripcion = "Servicio telefonico residencial o empresarial" },
                new ServicioTelecom { Nombre = "Telecable", Descripcion = "Servicio de television por cable" }
            );
        }

        if (!await db.Prestadoras.AnyAsync())
        {
            db.Prestadoras.AddRange(
                new Prestadora { Rnc = "101001001", NombreComercial = "Claro Dominicana", RazonSocial = "Compania Dominicana de Telefonos", Representante = "Representante Claro", Telefono = "8090000001", Correo = "claro@test.local" },
                new Prestadora { Rnc = "101001002", NombreComercial = "Altice Dominicana", RazonSocial = "Altice Dominicana", Representante = "Representante Altice", Telefono = "8090000002", Correo = "altice@test.local" },
                new Prestadora { Rnc = "101001003", NombreComercial = "Viva", RazonSocial = "Trilogy Dominicana", Representante = "Representante Viva", Telefono = "8090000003", Correo = "viva@test.local" },
                new Prestadora { Rnc = "101001004", NombreComercial = "Wind Telecom", RazonSocial = "Wind Telecom", Representante = "Representante Wind", Telefono = "8090000004", Correo = "wind@test.local" }
            );
        }

        if (!await db.TiposResolucion.AnyAsync())
        {
            db.TiposResolucion.AddRange(
                new TipoResolucion { Nombre = "Reclamacion", Descripcion = "Resolucion institucional vinculada a un expediente de reclamacion" },
                new TipoResolucion { Nombre = "Prestadora", Descripcion = "Resolucion institucional vinculada a una prestadora regulada" },
                new TipoResolucion { Nombre = "Autorizacion", Descripcion = "Resolucion de aprobacion, rechazo o condicionamiento de una autorizacion" },
                new TipoResolucion { Nombre = "Licencia Tecnica", Descripcion = "Resolucion vinculada a licencias tecnicas o uso de espectro" }
            );
        }

        await db.SaveChangesAsync();

        var adminEmail = configuration["SeedData:AdminEmail"]?.Trim().ToLowerInvariant()
                         ?? "admin@indotel.test";
        var adminPassword = configuration["SeedData:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "SeedData:AdminPassword es obligatorio cuando el seed se ejecuta fuera de Development.");
            }

            adminPassword = "Admin123*";
        }

        var adminRole = await db.Roles.FirstAsync(x => x.Nombre == "Administrador");
        var admin = await db.Usuarios.FirstOrDefaultAsync(x => x.Correo == adminEmail);

        if (admin is null)
        {
            admin = new Usuario
            {
                NombreCompleto = "Administrador INDOTEL",
                Correo = adminEmail,
                RolId = adminRole.Id,
                Activo = true
            };

            admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);
            db.Usuarios.Add(admin);
        }
        else if (admin.PasswordHash == "PendienteConfigurarHashEnBloqueJWT")
        {
            admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);
        }

        await db.SaveChangesAsync();
    }
}
