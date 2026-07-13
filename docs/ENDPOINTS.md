# Catálogo de endpoints y errores

## Direcciones

### Release

```text
Base pública: https://localhost:8443
Web:          https://localhost:8443/
API:          https://localhost:8443/api/...
Health:       https://localhost:8443/health
```

### Desarrollo

```text
Core interno: http://localhost:5085
Gateway:      http://localhost:5185
Web:          http://localhost:5234
```

Web y Caja deben utilizar el Gateway, no el puerto 5085.

## Autenticación

| Método | Ruta | Uso |
|---|---|---|
| POST | `/api/auth/login` | Iniciar sesión |
| POST | `/api/auth/register-ciudadano` | Registrar ciudadano y crear sesión |
| POST | `/api/auth/refresh-token` | Rotar access y refresh token |
| POST | `/api/auth/logout` | Revocar refresh token y cerrar sesión |
| GET | `/api/auth/me` | Obtener usuario autenticado |
| POST | `/api/auth/forgot-password` | Iniciar recuperación |
| POST | `/api/auth/reset-password` | Restablecer contraseña |
| POST | `/api/auth/change-password` | Cambiar contraseña autenticada |

Login:

```json
{
  "correo": "usuario@ejemplo.local",
  "password": "ClaveSegura123*"
}
```

Refresh:

```json
{
  "refreshToken": "TOKEN_OPACO"
}
```

Los refresh tokens rotan. Reutilizar uno anterior produce `REFRESH_TOKEN_REUTILIZADO` y revoca sesiones activas.

## Health y observabilidad

| Método | Ruta | Resultado |
|---|---|---|
| GET | `/health` | Gateway vivo |
| GET | `/health/live` | Liveness del Gateway |
| GET | `/health/ready` | Gateway y Core disponibles |
| GET | `/health/status` | Circuito y métricas del Gateway |
| GET | `/api/health` | Core a través del Gateway |
| GET | `/api/health/db` | Readiness de SQL Server |

`/api/health/db` debe responder 503 cuando SQL Server no está disponible.

## Catálogos

Requieren JWT.

| Método | Ruta |
|---|---|
| GET | `/api/catalogos/roles` |
| GET | `/api/catalogos/servicios` |
| GET | `/api/catalogos/prestadoras` |

## Ciudadanos

| Método | Ruta | Restricción |
|---|---|---|
| GET | `/api/ciudadanos/me` | Ciudadano autenticado |
| GET | `/api/ciudadanos` | Roles internos autorizados |
| GET | `/api/ciudadanos/{id}` | Rol o propietario |
| POST | `/api/ciudadanos` | Rol interno autorizado |
| PUT | `/api/ciudadanos/{id}` | Rol o propietario según política |

Un ciudadano no puede consultar ni modificar el perfil de otro.

## Reclamaciones

| Método | Ruta | Uso |
|---|---|---|
| GET | `/api/reclamaciones` | Bandeja filtrada por rol |
| GET | `/api/reclamaciones/{id}` | Detalle con propiedad |
| GET | `/api/reclamaciones/expediente/{numero}` | Buscar por expediente |
| POST | `/api/reclamaciones` | Crear reclamación |
| GET | `/api/reclamaciones/{id}/historial` | Historial |
| GET | `/api/reclamaciones/{id}/respuestas` | Respuestas de prestadora |
| PUT/PATCH | `/api/reclamaciones/{id}/estado` | Cambiar estado |
| POST | `/api/reclamaciones/{id}/respuesta-prestadora` | Respuesta autorizada |
| POST | `/api/reclamaciones/{id}/resolver` | Resolver |
| POST | `/api/reclamaciones/{id}/cerrar` | Cierre formal |
| GET | `/api/reclamaciones/{id}/transiciones` | Transiciones válidas por rol/estado |
| GET | `/api/reclamaciones/buscar` | Búsqueda paginada y filtros |
| GET | `/api/reclamaciones/sla/vencidas` | Casos vencidos |
| POST | `/api/reclamaciones/sla/marcar-vencidas` | Marcar SLA vencido |

Crear reclamación:

```json
{
  "ciudadanoId": 1,
  "prestadoraId": 1,
  "servicioTelecomId": 1,
  "canalRecepcion": "WEB",
  "prioridad": "MEDIA",
  "provincia": "Peravia",
  "municipio": "Baní",
  "titulo": "Falla del servicio",
  "descripcion": "Descripción completa del caso"
}
```

Cambiar estado:

```json
{
  "estadoNuevo": "VALIDADA",
  "comentario": "Documentación revisada"
}
```

Estados reconocidos:

```text
RECIBIDA
VALIDADA
OBSERVADA
ENVIADA_A_PRESTADORA
RESPONDIDA_POR_PRESTADORA
EN_REVISION
EN_REVISION_INDOTEL
RESUELTA
CERRADA
RECHAZADA
ARCHIVADA
VENCIDA
```

Estados finales: `CERRADA`, `RECHAZADA`, `ARCHIVADA`.

## Documentos

Máximo 5 MB. Formatos: PDF, JPG, JPEG y PNG. El contenido se valida por firma.

| Método | Ruta | Uso |
|---|---|---|
| GET | `/api/reclamaciones/{reclamacionId}/documentos` | Listar documentos autorizados |
| POST | `/api/reclamaciones/{reclamacionId}/documentos` | Subir `multipart/form-data`, campo `archivo` |
| GET | `/api/documentos/{id}/descargar` | Descargar con control de propiedad |

Códigos frecuentes:

```text
ARCHIVO_OBLIGATORIO
ARCHIVO_DEMASIADO_GRANDE
TIPO_ARCHIVO_NO_PERMITIDO
CONTENIDO_ARCHIVO_INVALIDO
DOCUMENTO_SIN_ACCESO
RECLAMACION_FINALIZADA
```

## Notificaciones

| Método | Ruta | Uso |
|---|---|---|
| GET | `/api/notificaciones` | Listar notificaciones del usuario |
| PATCH | `/api/notificaciones/{id}/leida` | Marcar como leída |

La consulta admite parámetros de paginación según el cliente.

## Reportes y auditoría

Rutas principales:

```text
GET /api/reportes/resumen
GET /api/reportes/reclamaciones-por-estado
GET /api/reportes/reclamaciones-por-prestadora
GET /api/auditorias
```

El Core contiene además reportes regulatorios de resoluciones, autorizaciones, certificaciones, espectro, licencias, SLA y rankings. Consultar Swagger en Development para el contrato exacto de esos módulos.

## Formato de error

Errores propios de Core y Gateway utilizan `application/problem+json`.

```json
{
  "status": 403,
  "title": "Solicitud no completada",
  "mensaje": "No tiene permiso para consultar este recurso",
  "codigo": "RECLAMACION_SIN_ACCESO",
  "correlationId": "referencia-unica",
  "fecha": "2026-07-13T12:00:00Z"
}
```

Campos mínimos que deben consumir Web y Caja:

```text
status
mensaje
codigo
correlationId
```

## Códigos HTTP

| Estado | Significado |
|---:|---|
| 200 | Consulta u operación correcta |
| 201 | Recurso creado |
| 204 | Operación correcta sin contenido |
| 400 | Datos inválidos |
| 401 | Credenciales o sesión inválida |
| 403 | Rol o propiedad insuficiente |
| 404 | Recurso no encontrado |
| 409 | Conflicto o transición inválida |
| 423 | Usuario bloqueado temporalmente |
| 429 | Límite de solicitudes excedido |
| 500 | Error inesperado |
| 502 | Respuesta inválida del servicio interno |
| 503 | Core, Gateway o SQL no disponible |
| 504 | Timeout del servicio interno |

## Errores principales de autenticación

```text
CREDENCIALES_OBLIGATORIAS
CREDENCIALES_INVALIDAS
USUARIO_BLOQUEADO
REFRESH_TOKEN_OBLIGATORIO
REFRESH_TOKEN_INVALIDO
REFRESH_TOKEN_EXPIRADO
REFRESH_TOKEN_REVOCADO
REFRESH_TOKEN_REUTILIZADO
USUARIO_INACTIVO
SESION_INVALIDA
CORREO_DUPLICADO
CEDULA_DUPLICADA
CLAVE_NO_VALIDA
```

## Errores principales del Gateway

```text
CORE_NO_DISPONIBLE
CORE_TIMEOUT
CIRCUITO_CORE_ABIERTO
LIMITE_SOLICITUDES_EXCEDIDO
RUTA_GATEWAY_NO_ENCONTRADA
```

## Regla para clientes

- No mostrar trazas técnicas.
- Mostrar `mensaje`.
- Conservar `codigo` para lógica controlada.
- Mostrar o copiar `correlationId` como referencia.
- No reintentar automáticamente escrituras.
- Tratar 502, 503, 504 y timeout como fallos temporales.
