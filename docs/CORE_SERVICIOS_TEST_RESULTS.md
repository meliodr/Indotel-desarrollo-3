# Resultados de prueba de gestión de servicios telecom - Core INDOTEL

Fecha de prueba: 09/07/2026
Rama probada: `core`
Entorno: Desarrollo local
API: `http://localhost:5085`

## Objetivo

Validar la gestión completa básica de servicios telecom como módulo de negocio del Core INDOTEL.

## Servicio creado durante la prueba

```text
SERVICIO_ID=5
NOMBRE=Servicio Prueba 1783581790255944003
```

## Resultados

| Prueba | Resultado esperado | Resultado real |
|---|---:|---:|
| Login admin | Token válido | OK |
| Listar servicios | 200 | OK |
| Crear servicio | 201 | OK |
| Consultar servicio por ID | 200 | OK |
| Validar nombre duplicado | 409 | OK |
| Actualizar servicio | 200 | OK |
| Desactivar servicio | 200 | OK |
| Reactivar servicio | 200 | OK |
| Ver reclamaciones del servicio | 200 | OK |
| Mantener catálogo antiguo `/api/catalogos/servicios` | 200 | OK |

## Endpoints validados

```text
GET /api/servicios
GET /api/servicios/{id}
POST /api/servicios
PUT /api/servicios/{id}
PATCH /api/servicios/{id}/estado
GET /api/servicios/{id}/reclamaciones
GET /api/catalogos/servicios
```

## Resultado general

```text
GESTION COMPLETA BASICA DE SERVICIOS TELECOM FUNCIONANDO
```

## Alcance implementado

El Core ya permite:

```text
Listar servicios activos.
Consultar un servicio por ID.
Crear servicios telecom.
Validar nombre duplicado.
Actualizar datos del servicio.
Activar o desactivar servicio.
Consultar reclamaciones asociadas a un servicio.
Mantener catálogo de servicios para consumo de Web/Caja.
```

## Nota técnica

La ruta `/api/servicios` ahora pertenece al controlador de gestión completa de servicios.

La ruta `/api/catalogos/servicios` se conserva como catálogo simple para pantallas Web/Caja y flujos existentes.
