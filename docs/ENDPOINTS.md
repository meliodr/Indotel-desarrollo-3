# Endpoints principales del Core INDOTEL

Base local:

```text
http://localhost:5085
```

## Auth

### POST /api/auth/login

Inicia sesion y devuelve un token JWT.

Body:

```json
{
  "correo": "admin@indotel.test",
  "password": "Admin123*"
}
```

### GET /api/auth/me

Devuelve el usuario autenticado.

Requiere token.

## Catalogos

Todos requieren token.

- GET /api/catalogos/roles
- GET /api/catalogos/servicios
- GET /api/catalogos/prestadoras

## Ciudadanos

Todos requieren token.

- GET /api/ciudadanos
- GET /api/ciudadanos/{id}
- POST /api/ciudadanos

Body para crear ciudadano:

```json
{
  "cedula": "00112345678",
  "nombres": "Juan",
  "apellidos": "Perez",
  "telefono": "8095550000",
  "correo": "juan@test.local",
  "direccion": "Santo Domingo"
}
```

## Reclamaciones

Todos requieren token.

- GET /api/reclamaciones
- GET /api/reclamaciones/{id}
- POST /api/reclamaciones
- PUT /api/reclamaciones/{id}/estado
- GET /api/reclamaciones/{id}/historial
- GET /api/reclamaciones/{id}/respuestas
- POST /api/reclamaciones/{id}/respuesta-prestadora

Body para crear reclamacion:

```json
{
  "ciudadanoId": 1,
  "prestadoraId": 1,
  "servicioTelecomId": 1,
  "titulo": "Problema con servicio de internet",
  "descripcion": "El ciudadano reporta fallas constantes en el servicio."
}
```

Body para cambiar estado:

```json
{
  "estadoNuevo": "VALIDADA",
  "comentario": "La reclamacion fue revisada y validada por el analista."
}
```

Body para respuesta de prestadora:

```json
{
  "prestadoraId": 1,
  "respuesta": "La prestadora informa que reviso el caso.",
  "documentoSoporte": "orden-tecnica-001.pdf"
}
```

## Reportes

Todos requieren token.

- GET /api/reportes/resumen
- GET /api/reportes/reclamaciones-por-estado
- GET /api/reportes/reclamaciones-por-prestadora

## Estados principales de reclamacion

- RECIBIDA
- VALIDADA
- ENVIADA_A_PRESTADORA
- RESPONDIDA_POR_PRESTADORA
- EN_REVISION
- RESUELTA
- CERRADA
- RECHAZADA
