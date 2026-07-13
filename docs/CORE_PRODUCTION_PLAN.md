# Plan de producción real - Core INDOTEL

Rama de trabajo: `core`
Estado actual: Core académico probado y funcional.
Objetivo nuevo: convertir el Core en una base seria para un sistema institucional real.

## 1. Diagnóstico actual

El Core ya tiene los módulos principales para una demostración académica:

```text
Autenticación JWT
Usuarios base
Roles
Ciudadanos
Prestadoras como catálogo
Servicios telecom como catálogo
Reclamaciones
Máquina de estados
Historial
Respuesta de prestadora
Documentos/evidencias
Reportes básicos
Consulta por expediente
Script de pruebas
Evidencia de pruebas
```

Porcentaje actual estimado:

```text
Core académico/demo: 100%
Core funcional probado: 98%
Core producción real: 70%
```

## 2. Principio del plan

No se debe crecer el Core solo con CRUD. Para producción real se necesita:

```text
Seguridad
Auditoría
Trazabilidad
SLA regulatorio
Permisos por dueño real
Reportes regulatorios
Gestión completa de entidades del negocio
Documentos seguros
Notificaciones
Observabilidad
Pruebas automáticas
```

El objetivo es llevar el Core de 70% producción a 90%-95%.

## 3. Fases de implementación

## Fase 1 - Autenticación pública y seguridad de usuario

Prioridad: crítica.

Objetivo: permitir que ciudadanos y usuarios reales operen sin depender del administrador.

Endpoints propuestos:

```text
POST /api/auth/register-ciudadano
POST /api/auth/change-password
POST /api/auth/forgot-password
POST /api/auth/reset-password
POST /api/auth/refresh-token
POST /api/auth/logout
```

Cambios de modelo sugeridos:

```text
Usuario.UltimoAcceso
Usuario.IntentosFallidos
Usuario.BloqueadoHasta
Usuario.DebeCambiarClave
Usuario.RefreshTokenHash
Usuario.RefreshTokenExpiraEn
Usuario.TokenRecuperacionHash
Usuario.TokenRecuperacionExpiraEn
```

Reglas:

```text
La contraseña nunca se guarda en texto plano.
El token de recuperación se guarda hasheado.
El login bloquea por intentos fallidos.
El refresh token permite renovar sesión sin exponer la contraseña.
El logout invalida refresh token.
```

Resultado esperado:

```text
Autenticación lista para usuarios reales.
```

## Fase 2 - RBAC fase 2 y dueño real de datos

Prioridad: crítica.

Objetivo: que cada usuario solo vea lo que le corresponde.

Cambios de modelo sugeridos:

```text
Usuario.CiudadanoId nullable
Usuario.PrestadoraId nullable
```

Reglas:

```text
Ciudadano solo ve sus propias reclamaciones.
Prestadora solo ve reclamaciones asignadas a su empresa.
Auditor solo lee.
AnalistaDAU gestiona reclamaciones.
Administrador gestiona todo.
```

Endpoints afectados:

```text
GET /api/reclamaciones
GET /api/reclamaciones/{id}
GET /api/reclamaciones/expediente/{numero}
GET /api/ciudadanos/{id}
GET /api/ciudadanos/{id}/reclamaciones
POST /api/reclamaciones/{id}/respuesta-prestadora
GET /api/reportes/*
```

Resultado esperado:

```text
Control de acceso por rol y por entidad real.
```

## Fase 3 - Gestión completa de prestadoras

Prioridad: alta.

Objetivo: convertir Prestadoras de catálogo a módulo de negocio completo.

Endpoints propuestos:

```text
GET /api/prestadoras
GET /api/prestadoras/{id}
POST /api/prestadoras
PUT /api/prestadoras/{id}
PATCH /api/prestadoras/{id}/estado
GET /api/prestadoras/{id}/reclamaciones
GET /api/prestadoras/{id}/usuarios
POST /api/prestadoras/{id}/usuarios
```

Campos relevantes:

```text
RNC
Razón social
Nombre comercial
Representante legal
Correo institucional
Teléfono
Dirección
Estado
FechaRegistro
```

Reglas:

```text
No permitir RNC duplicado.
No eliminar prestadoras con reclamaciones; solo desactivar.
Auditar cambios.
```

Resultado esperado:

```text
Prestadoras administrables y relacionadas con usuarios y reclamaciones.
```

## Fase 4 - Gestión completa de servicios telecom

Prioridad: alta.

Objetivo: convertir ServiciosTelecom de catálogo a módulo regulatorio.

Endpoints propuestos:

```text
GET /api/servicios
GET /api/servicios/{id}
POST /api/servicios
PUT /api/servicios/{id}
PATCH /api/servicios/{id}/estado
GET /api/servicios/{id}/reclamaciones
```

Reglas:

```text
No permitir nombre duplicado activo.
No eliminar servicios con reclamaciones; solo desactivar.
Auditar cambios.
```

Resultado esperado:

```text
Servicios telecom administrables para reportes regulatorios.
```

## Fase 5 - Tipos, motivos y clasificación de reclamaciones

Prioridad: alta.

Objetivo: que la reclamación no sea solo título y descripción.

Modelos propuestos:

```text
TipoReclamacion
MotivoReclamacion
CanalRecepcion
Provincia
Municipio
```

Campos sugeridos en Reclamacion:

```text
TipoReclamacionId
MotivoReclamacionId
CanalRecepcion
Provincia
Municipio
Prioridad
```

Ejemplos de motivos:

```text
Facturación
Avería
Cobro indebido
Mala calidad del servicio
Instalación pendiente
Cancelación no procesada
Portabilidad
```

Resultado esperado:

```text
Reclamaciones clasificadas para análisis y reportes reales.
```

## Fase 6 - SLA regulatorio

Prioridad: crítica para negocio INDOTEL.

Objetivo: controlar vencimientos y plazos de respuesta.

Campos sugeridos en Reclamacion:

```text
FechaEnvioPrestadora
FechaLimiteRespuesta
FechaRespuestaPrestadora
DiasHabilesSla
EstaVencida
FechaMarcadaVencida
```

Endpoints propuestos:

```text
GET /api/reclamaciones/vencidas
GET /api/reclamaciones/proximas-vencer
POST /api/reclamaciones/{id}/marcar-vencida
GET /api/reportes/sla
```

Reglas:

```text
Al pasar a ENVIADA_A_PRESTADORA se calcula FechaLimiteRespuesta.
Si no hay respuesta antes de la fecha límite, el caso puede marcarse VENCIDA.
Si la prestadora responde, se guarda FechaRespuestaPrestadora.
Las reclamaciones vencidas deben aparecer en reportes.
```

Resultado esperado:

```text
Control regulatorio de plazos.
```

## Fase 7 - Resolución, cierre y motivos estructurados

Prioridad: alta.

Objetivo: cerrar casos con decisión institucional clara.

Campos sugeridos:

```text
ResolucionFinal
MotivoCierreId
MotivoRechazoId
ResultadoCiudadano
MontoCompensado
RequiereSeguimiento
FechaResolucion
```

Endpoints propuestos:

```text
POST /api/reclamaciones/{id}/resolver
POST /api/reclamaciones/{id}/cerrar
POST /api/reclamaciones/{id}/rechazar
POST /api/reclamaciones/{id}/archivar
```

Reglas:

```text
No cerrar sin resolución final.
No rechazar sin motivo.
No resolver sin comentario institucional.
Auditar cada decisión.
```

Resultado esperado:

```text
Cierre regulatorio defendible.
```

## Fase 8 - Auditoría institucional completa

Prioridad: crítica.

Objetivo: registrar todo evento importante.

Modelo sugerido:

```text
Auditoria
- Id
- UsuarioId
- Accion
- Modulo
- Entidad
- EntidadId
- ValorAnterior
- ValorNuevo
- Ip
- UserAgent
- Fecha
- Resultado
- CorrelationId
```

Eventos mínimos:

```text
Login exitoso
Login fallido
Logout
Crear usuario
Desactivar usuario
Cambiar clave
Crear ciudadano
Editar ciudadano
Crear reclamación
Cambiar estado
Subir documento
Eliminar documento
Descargar documento
Registrar respuesta prestadora
Cerrar caso
```

Endpoints:

```text
GET /api/auditoria
GET /api/auditoria/reclamacion/{id}
GET /api/auditoria/usuario/{id}
GET /api/auditoria/entidad/{entidad}/{id}
```

Resultado esperado:

```text
Trazabilidad institucional completa.
```

## Fase 9 - Documentos seguros

Prioridad: alta.

Objetivo: manejar evidencias como datos sensibles.

Mejoras:

```text
Descarga por endpoint autorizado.
Validación MIME real.
Hash del archivo.
Tamaño máximo configurable.
Auditoría de descarga.
Almacenamiento externo preparado.
No servir documentos públicamente.
```

Endpoints propuestos:

```text
GET /api/documentos/{id}/metadata
GET /api/documentos/{id}/descargar
DELETE /api/documentos/{id}
```

Resultado esperado:

```text
Evidencias protegidas y auditables.
```

## Fase 10 - Filtros, búsqueda y paginación

Prioridad: alta.

Objetivo: que el Core soporte volumen real.

Endpoints a mejorar:

```text
GET /api/reclamaciones?page=1&pageSize=20
GET /api/reclamaciones?estado=VALIDADA
GET /api/reclamaciones?prestadoraId=1
GET /api/reclamaciones?servicioId=1
GET /api/reclamaciones?ciudadanoId=1
GET /api/reclamaciones?desde=2026-01-01&hasta=2026-12-31
GET /api/reclamaciones?buscar=IND-2026
```

Respuesta estándar sugerida:

```json
{
  "page": 1,
  "pageSize": 20,
  "total": 150,
  "items": []
}
```

Resultado esperado:

```text
API preparada para miles de registros.
```

## Fase 11 - Reportes regulatorios avanzados

Prioridad: alta.

Objetivo: aportar valor institucional.

Endpoints propuestos:

```text
GET /api/reportes/reclamaciones-por-prestadora
GET /api/reportes/reclamaciones-por-servicio
GET /api/reportes/reclamaciones-por-estado
GET /api/reportes/reclamaciones-por-mes
GET /api/reportes/sla-vencidas
GET /api/reportes/tiempo-promedio-respuesta
GET /api/reportes/prestadoras-con-mas-reclamaciones
GET /api/reportes/exportar-excel
GET /api/reportes/exportar-pdf
```

Resultado esperado:

```text
Reportes útiles para gestión y supervisión.
```

## Fase 12 - Notificaciones

Prioridad: media-alta.

Objetivo: informar cambios importantes.

Modelo sugerido:

```text
Notificacion
- Id
- UsuarioId
- Titulo
- Mensaje
- Tipo
- Leida
- FechaCreacion
- FechaLectura
- Entidad
- EntidadId
```

Eventos:

```text
Caso creado
Caso enviado a prestadora
Prestadora respondió
Caso vencido
Caso resuelto
Caso cerrado
Documento recibido
```

Endpoints:

```text
GET /api/notificaciones
PATCH /api/notificaciones/{id}/leida
PATCH /api/notificaciones/marcar-todas-leidas
```

Resultado esperado:

```text
Usuarios informados dentro del sistema.
```

## Fase 13 - Manejo global de errores y logs

Prioridad: alta.

Objetivo: responder errores de forma estándar y registrar fallos.

Componentes:

```text
ExceptionHandlingMiddleware
CorrelationIdMiddleware
Serilog o logging estructurado
Respuesta estándar de error
```

Respuesta sugerida:

```json
{
  "codigo": "ERROR_VALIDACION",
  "mensaje": "La solicitud no es válida.",
  "traceId": "abc-123",
  "fecha": "2026-07-09T00:00:00Z"
}
```

Resultado esperado:

```text
Errores controlados y diagnosticables.
```

## Fase 14 - Health checks, configuración y despliegue

Prioridad: media-alta.

Objetivo: preparar operación real.

Endpoints:

```text
GET /health
GET /health/live
GET /health/ready
GET /health/db
GET /health/storage
```

Configuración:

```text
CORS restringido por ambiente.
HTTPS obligatorio en producción.
Secretos fuera del repositorio.
Variables de entorno.
appsettings.Production.json seguro.
```

Resultado esperado:

```text
API operable en ambiente real.
```

## Fase 15 - CI/CD y pruebas automáticas

Prioridad: alta.

Objetivo: evitar romper el Core.

GitHub Actions sugerido:

```text
dotnet restore
dotnet build
dotnet test
```

Pruebas mínimas:

```text
Auth login correcto
Auth login incorrecto
Registro ciudadano
Crear reclamación
Transición válida
Transición inválida
Respuesta prestadora
Subir documento
Bloquear documento en caso cerrado
RBAC ciudadano
RBAC prestadora
Reportes básicos
```

Resultado esperado:

```text
Cada cambio se valida automáticamente.
```

## 4. Orden recomendado de ejecución

```text
1. Auth pública completa
2. RBAC fase 2
3. Gestión completa de prestadoras
4. Gestión completa de servicios telecom
5. Tipos y motivos de reclamación
6. SLA regulatorio
7. Resolución/cierre estructurado
8. Auditoría institucional
9. Documentos seguros
10. Filtros y paginación
11. Reportes regulatorios avanzados
12. Notificaciones
13. Manejo global de errores y logs
14. Health checks y configuración producción
15. CI/CD y pruebas automáticas
```

## 5. Meta porcentual

Estado actual:

```text
Producción real: 70%
```

Meta por fases:

```text
Fase 1 completada: 75%
Fase 2 completada: 80%
Fase 3-4 completadas: 84%
Fase 5-7 completadas: 88%
Fase 8-11 completadas: 92%
Fase 12-15 completadas: 95%
```

## 6. Criterio de listo para producción

El Core se considerará listo para producción cuando cumpla:

```text
Autenticación pública completa
Permisos por rol y dueño real
SLA regulatorio funcionando
Auditoría completa
Documentos seguros
Filtros y paginación
Reportes regulatorios
Errores globales controlados
Health checks
Pruebas automáticas
Despliegue reproducible
```

## 7. Decisión estratégica

El Core no debe convertirse en un sistema genérico. Debe crecer como un sistema regulatorio INDOTEL:

```text
menos CRUD genérico
más trazabilidad
más SLA
más auditoría
más reportes regulatorios
más control de prestadoras
más protección de datos ciudadanos
```
