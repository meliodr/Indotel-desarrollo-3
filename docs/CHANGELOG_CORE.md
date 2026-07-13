# Changelog del Core INDOTEL

## Bloque 1: base inicial

Se creo la base inicial del Core.

Archivos importantes:

- Solucion del Core.
- Proyecto ASP.NET Core Web API.
- Program.cs.
- appsettings.json.
- HealthController.
- README del Core.
- Guia de instalacion.

Funciona:

- Swagger.
- GET /health.
- Estructura de carpetas.

Verificacion local:

- dotnet restore funciona.
- dotnet build funciona.
- dotnet run funciona.
- Swagger abre en localhost:5085.
- GET /health responde 200 OK.
- El warning HTTPS de desarrollo fue corregido.

## Bloque 2: modelos y DbContext

Se agregaron los modelos principales del MVP y el DbContext.

Modelos agregados:

- Rol.
- Usuario.
- Ciudadano.
- Prestadora.
- ServicioTelecom.
- Reclamacion.
- DocumentoReclamacion.
- RespuestaPrestadora.
- HistorialReclamacion.
- Auditoria.

Archivos importantes:

- core-indotel/Indotel.Core/Data/IndotelDbContext.cs
- docs/DATABASE_MODEL.md

Pendiente del Bloque 2:

- Probar dotnet build localmente.
- Crear migracion inicial.
- Crear base de datos.
- Crear datos semilla.

Pendiente general:

- Login.
- JWT.
- Reclamaciones.
