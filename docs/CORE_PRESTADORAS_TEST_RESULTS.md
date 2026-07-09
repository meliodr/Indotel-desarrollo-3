# Resultados de prueba de gestión de prestadoras - Core INDOTEL

Fecha de prueba: 09/07/2026
Rama probada: `core`
Entorno: Desarrollo local
API: `http://localhost:5085`

## Objetivo

Validar la gestión completa básica de prestadoras como módulo de negocio del Core INDOTEL.

## Prestadora creada durante la prueba

```text
PRESTADORA_ID=5
RNC=2021783581378579989142
CORREO=prestadora.1783581378579989142@indotel.test
```

## Resultados

| Prueba | Resultado esperado | Resultado real |
|---|---:|---:|
| Login admin | Token válido | OK |
| Listar prestadoras | 200 | OK |
| Crear prestadora | 201 | OK |
| Consultar prestadora por ID | 200 | OK |
| Validar RNC duplicado | 409 | OK |
| Actualizar prestadora | 200 | OK |
| Desactivar prestadora | 200 | OK |
| Reactivar prestadora | 200 | OK |
| Ver reclamaciones de la prestadora | 200 | OK |
| Mantener catálogo antiguo `/api/catalogos/prestadoras` | 200 | OK |

## Endpoints validados

```text
GET /api/prestadoras
GET /api/prestadoras/{id}
POST /api/prestadoras
PUT /api/prestadoras/{id}
PATCH /api/prestadoras/{id}/estado
GET /api/prestadoras/{id}/reclamaciones
GET /api/catalogos/prestadoras
```

## Resultado general

```text
GESTION COMPLETA BASICA DE PRESTADORAS FUNCIONANDO
```

## Alcance implementado

El Core ya permite:

```text
Listar prestadoras activas.
Consultar una prestadora por ID.
Crear prestadoras.
Validar RNC duplicado.
Validar correo duplicado.
Actualizar datos de prestadora.
Activar o desactivar prestadora.
Consultar reclamaciones asociadas a una prestadora.
Mantener catálogo de prestadoras para consumo de Web/Caja.
```

## Nota técnica

La ruta `/api/prestadoras` ahora pertenece al controlador de gestión completa de prestadoras.

La ruta `/api/catalogos/prestadoras` se conserva como catálogo simple para pantallas Web/Caja y flujos existentes.
