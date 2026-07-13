# Sprint 1 — Core seguro y estable

## Estado

**Implementación y validación automática completadas en la rama `core`.**

Commit validado: `6704e0a`.

La aceptación funcional definitiva conserva pendientes las pruebas manuales de integración con SQL Server, autenticación, propiedad de recursos y concurrencia indicadas al final de este documento.

## Evidencia de validación automática

Validación ejecutada en Ubuntu 24.04 mediante `scripts/validar_sprint1_core.sh`.

Resultados:

- Restauración: correcta.
- Compilación Release: correcta, sin errores.
- Pruebas automáticas: 20 ejecutadas, 20 correctas, 0 errores, 0 omitidas.
- Publicación de comprobación: correcta.
- Proyecto principal generado: `Indotel.Core.dll` para `net8.0`.
- Proyecto de pruebas generado: `Indotel.Core.Tests.dll` para `net8.0`.
- Cobertura Cobertura generada localmente por el runner.

Mensaje final obtenido:

```text
Sprint 1 validado: restauración, compilación, pruebas y publicación completadas.
```

## Correcciones implementadas

### Seguridad y propiedad

- Se agregó `CurrentUserService` para centralizar identidad, rol, ciudadano y prestadora asociados a la sesión.
- Se corrigió el acceso a ciudadanos por ID.
- Un ciudadano solo puede consultar sus propios datos y reclamaciones.
- Un ciudadano no puede crear registros arbitrarios mediante `POST /api/ciudadanos`.
- Un ciudadano no puede cambiar su correo desde el recurso general de ciudadanos.
- Las operaciones internas de ciudadanos usan políticas de Caja.
- Documentos se listan, descargan y cargan únicamente si el usuario puede acceder a la reclamación.

### Autenticación

- Access token configurable; valor predeterminado: 30 minutos.
- Refresh token configurable; valor predeterminado: 7 días.
- Rotación del refresh token dentro de una transacción serializable.
- Detección de reutilización de refresh token rotado.
- Revocación de todas las sesiones activas cuando se detecta reutilización.
- Revocación de refresh tokens al cambiar o restablecer contraseña.
- Logout solo revoca tokens pertenecientes al usuario autenticado.
- Política de contraseña con mayúscula, minúscula, número y carácter especial.
- El token de recuperación solo puede exponerse en Development mediante configuración explícita.
- El seed exige una contraseña configurada cuando se ejecuta fuera de Development.

### Disponibilidad

- `/health` y `/health/live`: comprueban que el proceso está vivo.
- `/health/ready`, `/api/health/ready` y `/api/health/db`: comprueban SQL Server.
- Readiness devuelve HTTP 503 cuando la base de datos no está disponible.
- SQL Server utiliza resiliencia de conexión configurable y timeout de comando.
- No se reintentan automáticamente comandos de escritura, porque podrían duplicar operaciones.
- Seed y migraciones de inicio son configurables.

### Errores y observabilidad

- Respuestas de error en formato ProblemDetails.
- Compatibilidad conservada mediante la propiedad `mensaje`.
- Todos los errores normalizados incluyen `codigo`, `correlationId` y `fecha`.
- Middleware para excepciones de concurrencia, persistencia, timeout y errores no controlados.
- Las caídas de SQL Server se diferencian de los conflictos de integridad de datos.
- Propagación de `X-Correlation-ID` desde Gateway o cliente.
- Rate limiting de autenticación particionado por IP.
- Respuesta 429 estructurada con `Retry-After` cuando corresponde.

### Reclamaciones

- Filtro transaccional para operaciones de escritura de reclamaciones.
- La transacción se confirma antes de generar la respuesta HTTP.
- Aislamiento serializable para evitar cambios simultáneos basados en un estado obsoleto.
- Endpoint `GET /api/reclamaciones/{id}/transiciones`.
- Las transiciones se calculan desde la máquina de estados del Core y se filtran por rol.
- DTO de creación, cambio de estado, resolución, cierre y respuesta de prestadora con validaciones y protección ante valores nulos.

### Documentos

- Validación de firma real para PDF, PNG y JPEG.
- Rechazo de archivos renombrados con una extensión permitida.
- Nombre físico aleatorio.
- Nombre original sanitizado.
- Ruta física validada dentro del directorio de cargas.
- Descarga por streaming con soporte de rangos.
- Eliminación de archivo parcial cuando falla la persistencia.

### Configuración

- CORS abierto solo en Development cuando no existe lista configurada.
- En otros ambientes se requiere `Cors:AllowedOrigins`.
- El Core rechaza la clave JWT de ejemplo fuera de Development.
- La cadena de conexión y la clave JWT se validan durante el inicio.
- La resiliencia SQL se configura con `ConnectRetryCount`, `ConnectRetryIntervalSeconds` y `CommandTimeoutSeconds`.

### Pruebas y CI

- Proyecto `Indotel.Core.Tests` agregado a la solución.
- Pruebas de transiciones válidas, inválidas y estados finales.
- Pruebas de firmas PDF, PNG y JPEG.
- Workflow de GitHub Actions actualizado para restaurar, compilar y ejecutar pruebas.
- Script reproducible `scripts/validar_sprint1_core.sh` para limpiar, restaurar, compilar, probar y publicar.

## Archivos principales agregados

- `Constants/AuthorizationPolicies.cs`
- `Filters/ApiProblemResultFilter.cs`
- `Filters/TransactionalActionFilter.cs`
- `Helpers/ApiProblemFactory.cs`
- `Middleware/CorrelationIdMiddleware.cs`
- `Services/CurrentUserService.cs`
- `Services/FileSignatureValidator.cs`
- `Controllers/ReclamacionTransicionesController.cs`
- `Indotel.Core.Tests/Indotel.Core.Tests.csproj`
- `Indotel.Core.Tests/ReclamacionEstadoServiceTests.cs`
- `Indotel.Core.Tests/FileSignatureValidatorTests.cs`
- `scripts/validar_sprint1_core.sh`

## Configuración local esperada

Crear `appsettings.Development.json` desde el ejemplo y configurar:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key`
- `Cors:AllowedOrigins`
- `SeedData:Enabled`
- `Database:ConnectRetryCount`
- `Database:ConnectRetryIntervalSeconds`
- `Database:CommandTimeoutSeconds`
- `Security:ExposeResetTokenInResponse`

No se debe usar la clave de ejemplo fuera de Development.

## Comandos de validación

```bash
cd /home/jarry/indotel-prueba-core
git fetch origin
git reset --hard origin/core
bash scripts/validar_sprint1_core.sh
```

Alternativa manual:

```bash
dotnet restore core-indotel/Indotel.Core.sln
dotnet build core-indotel/Indotel.Core.sln --configuration Release
dotnet test core-indotel/Indotel.Core.Tests/Indotel.Core.Tests.csproj --configuration Release
```

## Pruebas manuales obligatorias pendientes

1. `GET /health` con SQL encendido y apagado.
2. `GET /health/ready` con SQL encendido: 200.
3. `GET /health/ready` con SQL apagado: 503.
4. Login correcto e incorrecto.
5. Bloqueo después de intentos fallidos.
6. Rotación de refresh token.
7. Reutilización del refresh token anterior.
8. Cambio de contraseña y revocación de sesiones.
9. Ciudadano A intentando consultar ciudadano B: 403.
10. Ciudadano A intentando consultar reclamaciones de B: 403.
11. Documento PDF real: aceptado.
12. Archivo de texto renombrado a PDF: rechazado.
13. Consulta de transiciones por Administrador, Auditor, Prestadora y Ciudadano.
14. Dos cambios simultáneos de estado sobre el mismo expediente.

## Riesgos pendientes de validar

- Compatibilidad exacta de los clientes Web y Caja con ProblemDetails.
- Migraciones y seed sobre una base existente.
- Comportamiento de transacciones serializables bajo carga.
- Resultado del pipeline CI después del último commit.
- Pruebas E2E con Gateway, Web y Caja; corresponden a sprints posteriores.
