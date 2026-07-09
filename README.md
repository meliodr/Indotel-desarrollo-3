# Sistema Digital INDOTEL - Core Backend

Rama principal de trabajo: `core`

## Estado actual

El Core Backend del Sistema Digital INDOTEL queda como un prototipo institucional avanzado, funcional, probado y documentado para fines academicos.

```text
Core principal de reclamaciones: VALIDADO
Fase 2A - Resoluciones institucionales: VALIDADA
Fase 2B - Autorizaciones y certificaciones: VALIDADA
Fase 2C - Espectro radioelectrico y licencias tecnicas: VALIDADA
Fase 2D - Reportes regulatorios ampliados: VALIDADA
Fase 3 - Hardening de autenticacion: VALIDADA
```

No se declara como produccion gubernamental real certificada. Se declara como Core academico/prototipo avanzado validado funcionalmente dentro del alcance definido.

## Tecnologia

```text
ASP.NET Core Web API
C# / .NET
Entity Framework Core
SQL Server 2022
JWT Bearer Authentication
Swagger / OpenAPI
GitHub Actions basico
Scripts bash de prueba funcional
```

## Modulos implementados

### Core principal

```text
Autenticacion JWT
Registro ciudadano
Roles
Ciudadanos
Prestadoras
Servicios telecom
Reclamaciones
Clasificacion de reclamaciones
SLA regulatorio
Respuesta de prestadora
Resolucion y cierre
Documentos seguros
Auditoria institucional
Busqueda paginada
Reportes regulatorios base
Notificaciones internas
Health checks
Manejo global de errores
CI basico
```

### Fase 2A

```text
Resoluciones institucionales
Estados: BORRADOR, APROBADA, PUBLICADA, ARCHIVADA
Documento oficial de resolucion
Reporte de resoluciones
Auditoria de creacion, aprobacion, publicacion y adjunto documental
```

### Fase 2B

```text
Autorizaciones institucionales
Certificaciones institucionales
Estados de solicitud: RECIBIDA, EN_REVISION, DOCUMENTACION_INCOMPLETA, APROBADA, RECHAZADA, VENCIDA, RENOVADA
Renovacion de autorizaciones y certificaciones
Reportes de autorizaciones y certificaciones
Auditoria de creacion, cambio de estado y renovacion
```

### Fase 2C

```text
Frecuencias radioelectricas
Asignaciones de frecuencia
Licencias tecnicas
Estados de frecuencia: DISPONIBLE, ASIGNADA, RESERVADA, SUSPENDIDA
Estados de licencia: SOLICITADA, EN_EVALUACION_TECNICA, APROBADA, ACTIVA, POR_VENCER, VENCIDA, CANCELADA
Reportes de espectro y licencias tecnicas
Auditoria tecnica
```

### Fase 2D

```text
Reportes regulatorios ampliados
Ranking de prestadoras
Ranking SLA
Reclamaciones mensuales
Tiempo promedio de respuesta
Servicios mas reclamados
Resoluciones por periodo
Autorizaciones por estado
Certificaciones por estado
Licencias por vencimiento
```

### Fase 3

```text
Refresh token
Logout real
Revocacion de refresh token
Bloqueo por intentos fallidos
Rate limiting basico en autenticacion
Campos de seguridad en Usuario
Tabla RefreshTokens
```

## Pruebas funcionales

Scripts principales:

```bash
bash /tmp/probar_final_100_indotel.sh
bash scripts/probar_fase2a_resoluciones.sh
bash scripts/probar_fase2b_autorizaciones_certificaciones.sh
ADMIN_PASSWORD='***' bash scripts/probar_fase2c_espectro_licencias.sh
ADMIN_PASSWORD='***' bash scripts/probar_fase2d_reportes_ampliados.sh
ADMIN_PASSWORD='***' bash scripts/probar_fase3_hardening_auth.sh
```

Resultados validados:

```text
Core principal: OK
Fase 2A: OK
Fase 2B: OK
Fase 2C: OK
Fase 2D: OK
Fase 3: OK
```

## Documentacion importante

```text
docs/CORE_FINAL_STATUS.md
docs/CORE_FINAL_100_TEST_RESULTS.md
docs/CORE_ARCHITECTURE.md
docs/DEFENSA_CORE_INDOTEL.md
docs/CORE_PHASE_IMPLEMENTATION_PLAN.md
docs/CORE_ROADMAP_PLAN_CHECKLIST.md
docs/CORE_FASE2A_RESOLUCIONES_TEST_RESULTS.md
docs/CORE_FASE2B_2C_TEST_RESULTS.md
docs/CORE_FASE2D_3_TEST_RESULTS.md
```

## Frase de defensa

> El proyecto no intenta copiar todo INDOTEL. Construimos un Core regulatorio funcional que maneja reclamaciones, SLA, documentos, auditoria, resoluciones, autorizaciones, certificaciones, espectro, licencias tecnicas, reportes regulatorios y controles basicos de seguridad. Esta validado dentro del alcance academico definido.
