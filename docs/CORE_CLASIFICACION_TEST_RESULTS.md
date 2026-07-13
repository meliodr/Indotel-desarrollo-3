# Resultados de prueba de clasificación de reclamaciones - Core INDOTEL

Fecha de prueba: 09/07/2026
Rama probada: `core`
Entorno: Desarrollo local
API: `http://localhost:5085`

## Objetivo

Validar la Fase 5 del Core INDOTEL: tipos, motivos, canales, prioridades y creación de reclamaciones clasificadas.

## Migración aplicada

Migración generada localmente y aplicada contra SQL Server Docker:

```text
Fase5ClasificacionReclamaciones
```

Resultado:

```text
Migración aplicada correctamente.
Base de datos actualizada.
API inició correctamente en http://localhost:5085.
```

## Datos creados durante la prueba

```text
TIPO_ID=1
MOTIVO_ID=1
CIUDADANO_ID=5
RECLAMACION_ID=10
EXPEDIENTE=IND-20260709073334350-218
```

## Resultados

| Prueba | Resultado esperado | Resultado real |
|---|---:|---:|
| Login admin | Token válido | OK |
| Crear tipo de reclamación | 201 | OK |
| Validar tipo duplicado | 409 | OK |
| Crear motivo de reclamación | 201 | OK |
| Validar motivo duplicado | 409 | OK |
| Listar tipos | 200 | OK |
| Listar motivos por tipo | 200 | OK |
| Listar canales | 200 | OK |
| Listar prioridades | 200 | OK |
| Registrar ciudadano | 200 | OK |
| Rechazar canal inválido | 400 | OK |
| Rechazar prioridad inválida | 400 | OK |
| Crear reclamación clasificada | 201 | OK |
| Consultar reclamación clasificada | 200 | OK |

## Endpoints validados

```text
POST /api/catalogos/reclamaciones/tipos
GET /api/catalogos/reclamaciones/tipos
PATCH /api/catalogos/reclamaciones/tipos/{id}/estado
POST /api/catalogos/reclamaciones/motivos
GET /api/catalogos/reclamaciones/motivos
PATCH /api/catalogos/reclamaciones/motivos/{id}/estado
GET /api/catalogos/reclamaciones/canales
GET /api/catalogos/reclamaciones/prioridades
POST /api/reclamaciones
GET /api/reclamaciones/{id}
```

## Resultado general

```text
FASE 5 - CLASIFICACION DE RECLAMACIONES FUNCIONANDO
```

## Alcance implementado

El Core ya permite:

```text
Crear tipos de reclamación.
Crear motivos asociados a tipos.
Validar duplicados.
Listar tipos activos.
Listar motivos activos.
Filtrar motivos por tipo.
Listar canales de recepción permitidos.
Listar prioridades permitidas.
Crear reclamaciones con tipo, motivo, canal, prioridad, provincia y municipio.
Validar canal inválido.
Validar prioridad inválida.
Validar tipo inválido.
Validar motivo inválido.
Validar que el motivo corresponda al tipo.
Consultar reclamaciones con su clasificación.
```

## Nota técnica

Esta fase requiere migración porque agrega tablas y columnas:

```text
TiposReclamacion
MotivosReclamacion
Reclamaciones.TipoReclamacionId
Reclamaciones.MotivoReclamacionId
Reclamaciones.CanalRecepcion
Reclamaciones.Prioridad
Reclamaciones.Provincia
Reclamaciones.Municipio
```

La migración fue creada localmente por EF Core y debe subirse al repositorio junto al snapshot actualizado.
