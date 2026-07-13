# Resultados de prueba RBAC por dueño real - Core INDOTEL

Fecha de prueba: 09/07/2026
Rama probada: `core`
Entorno: Desarrollo local
API: `http://localhost:5085`

## Objetivo

Validar que el Core aplica control de acceso por dueño real para ciudadanos en reclamaciones y documentos.

## Escenario probado

Se crearon dos ciudadanos reales desde el endpoint público de registro:

```text
Ciudadano A: ciudadano.a.1783580967789699266@indotel.test
Ciudadano B: ciudadano.b.1783580967789699266@indotel.test
```

IDs creados:

```text
CIUDADANO_A_ID=3
CIUDADANO_B_ID=4
```

El ciudadano A creó una reclamación propia:

```text
RECLAMACION_A_ID=9
```

## Resultados

| Prueba | Resultado esperado | Resultado real |
|---|---:|---:|
| Login admin | Token válido | OK |
| Registrar ciudadano A | 200 | OK |
| Registrar ciudadano B | 200 | OK |
| Buscar ciudadanos por cédula con admin | 200 | OK |
| Ciudadano A crea reclamación propia | 201/200 | OK |
| Ciudadano A consulta su reclamación | 200 | OK |
| Ciudadano B intenta ver reclamación de A | 403 | OK |
| Ciudadano B intenta crear reclamación usando ID de A | 403 | OK |
| Ciudadano A lista reclamaciones | 200 | OK |
| Lista de A solo contiene sus casos | cantidad=1 | OK |
| Ciudadano B intenta ver documentos de reclamación A | 403 | OK |

## Endpoints validados

```text
POST /api/auth/register-ciudadano
GET /api/ciudadanos/cedula/{cedula}
POST /api/reclamaciones
GET /api/reclamaciones/{id}
GET /api/reclamaciones
GET /api/reclamaciones/{id}/documentos
```

## Resultado general

```text
RBAC POR DUEÑO REAL BASICO FUNCIONANDO
```

## Alcance implementado

El Core ya protege:

```text
Ciudadano solo ve sus propias reclamaciones.
Ciudadano no puede consultar reclamaciones ajenas.
Ciudadano no puede crear reclamaciones a nombre de otro ciudadano.
Ciudadano no puede ver documentos de reclamaciones ajenas.
Administrador mantiene acceso global.
```

## Nota técnica

Esta fase básica se implementó sin migración nueva. El dueño real se resuelve usando el correo del usuario autenticado y buscando el ciudadano o prestadora relacionada por correo.

Para producción estricta, sigue pendiente agregar relación directa en `Usuario`:

```text
Usuario.CiudadanoId
Usuario.PrestadoraId
```

Eso permitirá un RBAC más sólido y explícito en base de datos.
