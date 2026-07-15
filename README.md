# Sistema Digital INDOTEL

Prototipo académico avanzado para gestión de reclamaciones ciudadanas, compuesto por Core, GUI operativa del Core, API Gateway, portal Web, aplicación interna Caja, SQL Server y un release reproducible con Docker.

> No se presenta como plataforma gubernamental real certificada. Antes de uso institucional se requieren infraestructura oficial, auditoría independiente, pruebas de penetración, gobierno de datos, monitoreo y aprobación operativa.

## Arquitectura

```text
Core GUI ───────────> Core ASP.NET ───> SQL Server

Portal Web ──────┐
                 ├──> API Gateway ───> Core ASP.NET ───> SQL Server
Caja WinForms ───┘
```

En release actual:

```text
Navegador / Caja -> HTTPS Caddy -> Web o Gateway -> Core -> SQL
```

La GUI del Core se agregará como proyecto independiente de la misma solución y consumirá la API del Core mediante HTTP. No accederá directamente a SQL Server ni al `IndotelDbContext`.

Web y Caja no acceden directamente a SQL Server. El Core conserva autorización, propiedad, reglas de estados y persistencia.

## Componentes

| Componente | Tecnología | Rama o estado |
|---|---|---|
| Core API | ASP.NET Core 8, EF Core, JWT, SQL Server | base validada |
| Core GUI | ASP.NET Core 8 MVC, Razor, Bootstrap/Fluent | plan obligatorio en `proyecto-revisado-funcionando` |
| API Gateway | ASP.NET Core 8, proxy, timeout, circuit breaker | `api-gateway` |
| Portal ciudadano | ASP.NET Core MVC 8 | `web` |
| Caja interna | WinForms `net8.0-windows` | `caja` |
| Integración y release | Bash, Docker Compose, Caddy | `integracion` |

## Estado de validación

| Sprint | Resultado confirmado |
|---|---|
| 1 — Core | compilación y publicación; 20/20 pruebas |
| 2 — Gateway | compilación y publicación; 14/14 pruebas |
| 3 — Web | compilación y publicación; 12/12 pruebas |
| 4 — Caja | restauración Linux, configuración WinForms y 15/15 pruebas; build Windows pendiente de evidencia CI |
| 5 — Integración | scripts, E2E, IDOR, refresh y fallas preparados; ejecución consolidada pendiente |
| 6 — Release | Docker, HTTPS, backup, restore, rollback y manuales preparados; smoke final pendiente |
| Evolución Core GUI | arquitectura y fases documentadas; implementación pendiente |

Total base confirmado hasta Caja: **61 pruebas automáticas**.

## Preparar rama integrada

```bash
cd /home/jarry/Indotel-desarrollo-3
git fetch origin

git worktree add -B integracion \
  /home/jarry/indotel-prueba-integracion \
  origin/integracion

cd /home/jarry/indotel-prueba-integracion
bash scripts/preparar_sprint5_integracion.sh
git push origin integracion
```

## Validar Sprint 5

Validación de compilación, pruebas, arquitectura y secretos:

```bash
bash scripts/validar_sprint5_integracion.sh
```

Incluyendo ejecución Web → Gateway → Core → SQL y fallas:

```bash
RUN_RUNTIME_TESTS=1 \
INDOTEL_SQL_PASSWORD='CLAVE_SQL_LOCAL' \
bash scripts/validar_sprint5_integracion.sh
```

## Preparar release

```bash
cp deploy/.env.release.example deploy/.env.release
chmod 600 deploy/.env.release
```

Reemplazar todos los valores `CAMBIAR_...` sin subir el archivo real a Git.

Validación estructural:

```bash
bash scripts/validar_sprint6_despliegue.sh
```

Build y smoke:

```bash
BUILD_IMAGES=1 RUN_RELEASE_SMOKE=1 \
bash scripts/validar_sprint6_despliegue.sh
```

Despliegue:

```bash
bash scripts/desplegar_release.sh deploy/.env.release
```

Direcciones predeterminadas:

```text
Web:     https://localhost:8443/
API:     https://localhost:8443/api/...
Health:  https://localhost:8443/health
```

Caja debe configurar `ApiBaseUrl=https://localhost:8443/`.

## Seguridad implementada

- JWT y refresh token rotativo.
- Detección de reutilización y revocación de sesiones.
- Bloqueo temporal por intentos fallidos.
- Autorización central por roles.
- Propiedad de ciudadanos, reclamaciones y documentos.
- Validación de firma real de PDF, PNG y JPEG.
- Transacciones en operaciones críticas.
- Errores estructurados con `codigo`, `mensaje` y `correlationId`.
- Rate limiting en Core y Gateway.
- Reintentos del Gateway solo para consultas seguras.
- Tokens Web almacenados del lado servidor.
- Tokens Caja únicamente en memoria.
- SQL en red Docker interna.
- Contenedores .NET con usuario no root.
- Secretos y respaldos excluidos de Git.

## GUI operativa obligatoria del Core

El docente confirmó que el Core debe tener una interfaz gráfica propia, similar conceptualmente a una aplicación bancaria interna. Swagger continuará como consola técnica, pero no se considerará la GUI final.

La GUI se construirá como:

```text
core-indotel/Indotel.Core.Gui
```

Principios:

- forma parte de `Indotel.Core.sln` y del entregable del Core;
- usa ASP.NET Core 8 MVC y Razor Views;
- se ejecuta en puerto independiente;
- inicia sesión contra `/api/auth/login`;
- almacena JWT y refresh token del lado servidor;
- consume la API mediante `HttpClient`;
- no accede al `DbContext` ni a SQL Server;
- aplica permisos por rol;
- incluye dashboard, usuarios, perfiles, auditoría y salud;
- crecerá hacia clientes, productos, servicios cobrables, cotizaciones, facturas, cuentas por cobrar, pagos, cobros y recibos;
- usará doble autorización operador/aprobador para acciones sensibles;
- no requiere modificar Caja, Web o Integración.

Primera demostración prevista:

```text
Login -> Dashboard -> Usuarios -> Crear/editar/desactivar -> Auditoría -> Health -> Logout
```

Documentación:

```text
docs/PLAN_IMPLEMENTACION_GUI_CORE_OBLIGATORIA.md
docs/ADR_GUI_ADMINISTRATIVA_CORE.md
```

## Evolución planificada del Core

La rama `proyecto-revisado-funcionando` documenta una ampliación exclusivamente aditiva del Core para incorporar perfiles administrativos, clientes comerciales, productos, servicios cobrables, cotizaciones, facturas, cuentas por cobrar, pagos y cobros.

Principios obligatorios:

- no cambiar rutas ni respuestas utilizadas por Web, Caja o Gateway;
- mantener `ServicioTelecom` separado del nuevo catálogo de servicios cobrables;
- usar migraciones aditivas y feature flags;
- aplicar idempotencia, transacciones explícitas y concurrencia en operaciones monetarias;
- construir la GUI como proyecto de la solución del Core que consume la API y nunca accede al `DbContext`.

Documentación:

```text
docs/PLAN_EVOLUCION_CORE_COMERCIAL_Y_GUI.md
docs/PLAN_IMPLEMENTACION_GUI_CORE_OBLIGATORIA.md
docs/ADR_GUI_ADMINISTRATIVA_CORE.md
```

## Operación

Backup:

```bash
bash scripts/backup_sqlserver.sh deploy/.env.release
```

Restauración:

```bash
CONFIRM_RESTORE=YES \
bash scripts/restore_sqlserver.sh backups/IndotelCoreDb-FECHA.bak deploy/.env.release
```

Rollback:

```bash
CONFIRM_ROLLBACK=YES \
bash scripts/rollback_release.sh TAG_ANTERIOR backups/IndotelCoreDb-ANTES.bak deploy/.env.release
```

## Documentación principal

```text
docs/PLAN_MAESTRO_6_SPRINTS.md
docs/SPRINT_1_CORE_HARDENING.md
docs/SPRINT_2_API_GATEWAY.md
docs/SPRINT_3_WEB_CIUDADANO.md
docs/SPRINT_4_CAJA.md
docs/SPRINT_5_INTEGRACION_SEGURIDAD.md
docs/SPRINT_6_DESPLIEGUE_ENTREGA.md
docs/ARQUITECTURA_FINAL.md
docs/MANUAL_TECNICO.md
docs/MANUAL_USUARIO.md
docs/GUIA_DEMO_DEFENSA.md
docs/PLAN_EVOLUCION_CORE_COMERCIAL_Y_GUI.md
docs/PLAN_IMPLEMENTACION_GUI_CORE_OBLIGATORIA.md
docs/ADR_GUI_ADMINISTRATIVA_CORE.md
```

## Responsabilidad ante fallos

- Core: reglas, seguridad, health y códigos HTTP.
- Core GUI: presentación operativa, sesión y mensajes controlados.
- Gateway: timeout, circuit breaker, 503 y correlación.
- Web/Caja: mensaje controlado y continuidad de interfaz.

La caída de la GUI del Core no debe detener la API. La caída de un servicio no debe cerrar Caja ni producir una pantalla técnica en Web.