# Flujo de demostracion del Core INDOTEL

Este documento indica el orden recomendado para demostrar el Core en Swagger.

## 1. Levantar la base de datos

Desde la raiz del repositorio:

```bash
sudo docker compose up -d
```

Verificar:

```bash
sudo docker ps
```

Debe aparecer el contenedor `indotel-sqlserver`.

## 2. Levantar el Core

```bash
cd core-indotel/Indotel.Core
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

Swagger:

```text
http://localhost:5085/swagger
```

## 3. Login

Endpoint:

```text
POST /api/auth/login
```

Body:

```json
{
  "correo": "admin@indotel.test",
  "password": "Admin123*"
}
```

Copiar el valor de `token` y pegarlo en el boton `Authorize` de Swagger.

## 4. Probar usuario autenticado

```text
GET /api/auth/me
```

Debe devolver el administrador autenticado.

## 5. Probar catalogos

```text
GET /api/catalogos/roles
GET /api/catalogos/servicios
GET /api/catalogos/prestadoras
```

## 6. Crear ciudadano

```text
POST /api/ciudadanos
```

Body:

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

## 7. Crear reclamacion

```text
POST /api/reclamaciones
```

Body:

```json
{
  "ciudadanoId": 1,
  "prestadoraId": 1,
  "servicioTelecomId": 1,
  "titulo": "Problema con servicio de internet",
  "descripcion": "El ciudadano reporta fallas constantes en el servicio."
}
```

## 8. Cambiar estado

```text
PUT /api/reclamaciones/1/estado
```

Body:

```json
{
  "estadoNuevo": "VALIDADA",
  "comentario": "La reclamacion fue revisada y validada por el analista."
}
```

## 9. Registrar respuesta de prestadora

```text
POST /api/reclamaciones/1/respuesta-prestadora
```

Body:

```json
{
  "prestadoraId": 1,
  "respuesta": "La prestadora informa que reviso el caso y abrio una orden tecnica.",
  "documentoSoporte": "orden-tecnica-001.pdf"
}
```

## 10. Ver historial

```text
GET /api/reclamaciones/1/historial
```

Debe mostrar el flujo de estados.

## 11. Reportes

```text
GET /api/reportes/resumen
GET /api/reportes/reclamaciones-por-estado
GET /api/reportes/reclamaciones-por-prestadora
```

## Resultado esperado

El Core demuestra:

- Autenticacion JWT.
- Base de datos SQL Server.
- Catalogos iniciales.
- Creacion de ciudadano.
- Creacion de reclamacion.
- Cambio de estado.
- Respuesta de prestadora.
- Historial de reclamacion.
- Reportes basicos.
