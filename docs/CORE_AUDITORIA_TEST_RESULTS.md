# Resultados de prueba de auditoría institucional - Core INDOTEL

Fecha de prueba: 09/07/2026
Rama probada: `core`
Entorno: Desarrollo local
API: `http://localhost:5085`

## Objetivo

Validar la Fase 8 del Core INDOTEL: auditoría institucional manual, automática, filtrable y paginada.

## Migración aplicada

Migración generada localmente y aplicada contra SQL Server Docker:

```text
Fase8AuditoriaInstitucional
```

Resultado:

```text
Migración aplicada correctamente.
Base de datos actualizada.
API inició correctamente en http://localhost:5085.
```

## Datos creados durante la prueba

```text
AUDITORIA_ID=1
CIUDADANO_ID=8
RECLAMACION_ID=13
EXPEDIENTE=IND-20260709075849849-803
```

## Resultados

| Prueba | Resultado esperado | Resultado real |
|---|---:|---:|
| Login admin | Token válido | OK |
| Crear auditoría manual | 201 | OK |
| Consultar auditoría manual por ID | 200 | OK |
| Guardar usuarioCorreo | No vacío | OK |
| Guardar usuarioRol | No vacío | OK |
| Guardar entidad | Correcto | OK |
| Guardar acción | Correcto | OK |
| Guardar nivel | INFO | OK |
| Guardar método HTTP | POST | OK |
| Guardar ruta | `/api/auditorias` | OK |
| Guardar correlationId | No vacío | OK |
| Guardar fecha | No vacío | OK |
| Registrar ciudadano | 200 | OK |
| Crear reclamación | 201 | OK |
| Auditoría automática `CREAR_RECLAMACION` | Existe | OK |
| Cambio de estado `RECIBIDA -> VALIDADA` | 200 | OK |
| Auditoría automática `CAMBIO_ESTADO` | Existe | OK |
| Respuesta de prestadora | 200 | OK |
| Auditoría automática `RESPUESTA_PRESTADORA` | Existe | OK |
| Resolver reclamación | 200 | OK |
| Auditoría automática `RESOLVER_RECLAMACION` | Existe | OK |
| Cerrar reclamación | 200 | OK |
| Auditoría automática `CERRAR_RECLAMACION` | Existe | OK |
| Listado paginado de auditoría | 200 | OK |
| Estructura paginada `total/page/pageSize/data` | Correcta | OK |

## Endpoints validados

```text
POST /api/auditorias
GET /api/auditorias/{id}
GET /api/auditorias
POST /api/reclamaciones
PATCH /api/reclamaciones/{id}/estado
POST /api/reclamaciones/{id}/respuesta-prestadora
POST /api/reclamaciones/{id}/resolver
POST /api/reclamaciones/{id}/cerrar
```

## Acciones auditadas automáticamente

```text
CREAR_RECLAMACION
CAMBIO_ESTADO
RESPUESTA_PRESTADORA
RESOLVER_RECLAMACION
CERRAR_RECLAMACION
SLA_VENCIDA
```

## Resultado general

```text
FASE 8 - AUDITORIA INSTITUCIONAL FUNCIONANDO
```

## Alcance implementado

El Core ya permite:

```text
Crear auditorías manuales.
Consultar auditorías por ID.
Listar auditorías con filtros.
Paginar auditorías.
Filtrar por entidad.
Filtrar por acción.
Filtrar por usuario.
Filtrar por rango de fechas.
Guardar usuario, correo y rol.
Guardar método HTTP.
Guardar ruta.
Guardar IP.
Guardar User-Agent.
Guardar correlationId.
Auditar creación de reclamaciones.
Auditar cambios de estado.
Auditar respuestas de prestadoras.
Auditar resolución de reclamaciones.
Auditar cierre de reclamaciones.
Auditar marcado de vencidas por SLA.
```
