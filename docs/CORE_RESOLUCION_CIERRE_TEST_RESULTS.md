# Resultados de prueba de resolución y cierre estructurado - Core INDOTEL

Fecha de prueba: 09/07/2026
Rama probada: `core`
Entorno: Desarrollo local
API: `http://localhost:5085`

## Objetivo

Validar la Fase 7 del Core INDOTEL: resolución formal y cierre estructurado de reclamaciones.

## Migración aplicada

Migración generada localmente y aplicada contra SQL Server Docker:

```text
Fase7ResolucionCierre
```

Resultado:

```text
Migración aplicada correctamente.
Base de datos actualizada.
API inició correctamente en http://localhost:5085.
```

## Datos creados durante la prueba

```text
CIUDADANO_ID=7
RECLAMACION_ID=12
EXPEDIENTE=IND-20260709074934086-394
```

## Resultados

| Prueba | Resultado esperado | Resultado real |
|---|---:|---:|
| Login admin | Token válido | OK |
| Registrar ciudadano | 200 | OK |
| Crear reclamación | 201 | OK |
| Cerrar sin resolver | 409 | OK |
| Cambiar a `VALIDADA` | 200 | OK |
| Cambiar a `ENVIADA_A_PRESTADORA` | 200 | OK |
| Registrar respuesta de prestadora | 200 | OK |
| Cambiar a `EN_REVISION` | 200 | OK |
| Resolver reclamación con endpoint estructurado | 200 | OK |
| Estado luego de resolver | `RESUELTA` | OK |
| Guardar fecha de resolución | No vacío | OK |
| Guardar resultado de resolución | `PROCEDE` | OK |
| Guardar comentario de resolución | No vacío | OK |
| Guardar fundamento de resolución | No vacío | OK |
| Guardar acción ordenada | No vacío | OK |
| Guardar monto de ajuste | 350.75 | OK |
| Cerrar reclamación con endpoint estructurado | 200 | OK |
| Estado luego de cerrar | `CERRADA` | OK |
| Guardar fecha de cierre | No vacío | OK |
| Guardar motivo de cierre | No vacío | OK |
| Guardar comentario de cierre | No vacío | OK |
| Guardar conformidad ciudadana | true | OK |
| Consultar historial | 200 | OK |

## Endpoints validados

```text
POST /api/reclamaciones
PATCH /api/reclamaciones/{id}/estado
POST /api/reclamaciones/{id}/respuesta-prestadora
POST /api/reclamaciones/{id}/resolver
POST /api/reclamaciones/{id}/cerrar
GET /api/reclamaciones/{id}/historial
```

## Resultado general

```text
FASE 7 - RESOLUCION Y CIERRE ESTRUCTURADO FUNCIONANDO
```

## Alcance implementado

El Core ya permite:

```text
Resolver reclamaciones con campos formales.
Guardar resultado de resolución.
Guardar comentario de resolución.
Guardar fundamento de resolución.
Guardar acción ordenada.
Guardar monto de ajuste.
Guardar usuario que resolvió.
Cerrar reclamaciones con campos formales.
Guardar motivo de cierre.
Guardar comentario de cierre.
Guardar conformidad del ciudadano.
Guardar usuario que cerró.
Bloquear cierre directo si la reclamación no está resuelta.
Registrar resolución y cierre en historial.
Diferenciar FechaResolucion de FechaCierre.
```

## Nota técnica

Esta fase requiere migración porque agrega campos de resolución y cierre a `Reclamacion`:

```text
FechaResolucion
ResultadoResolucion
ComentarioResolucion
FundamentoResolucion
AccionOrdenada
MontoAjuste
UsuarioResolucionId
MotivoCierre
ComentarioCierre
ConformidadCiudadano
UsuarioCierreId
```

La migración fue creada localmente por EF Core y debe subirse al repositorio junto al snapshot actualizado.
