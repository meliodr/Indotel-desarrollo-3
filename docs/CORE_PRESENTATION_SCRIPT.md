# Guion de presentacion del Core INDOTEL

## 1. Introduccion

Buenos dias. Esta parte corresponde al Core del Sistema Digital INDOTEL.

El Core es el backend principal del proyecto. Es la parte que se encarga de recibir las solicitudes del sistema, aplicar reglas de negocio, proteger los endpoints y guardar la informacion en la base de datos.

## 2. Problema que resuelve

INDOTEL necesita una plataforma que permita organizar servicios digitales relacionados con ciudadanos, prestadoras, reclamaciones, respuestas, historial y reportes.

Por eso se creo un Core central que puede ser consumido por tres partes:

- Portal Web.
- Caja o ventanilla interna.
- APIs y Gateway.

## 3. Tecnologia usada

Para el Core se utilizaron las siguientes tecnologias:

- ASP.NET Core Web API.
- C#.
- Entity Framework Core.
- SQL Server.
- Docker.
- JWT para autenticacion.
- Swagger para probar los servicios.
- GitHub para versionamiento.

## 4. Arquitectura

La arquitectura funciona asi:

El Portal Web, la Caja y el Gateway se comunican con el Core por medio de endpoints HTTP. El Core valida el token JWT, procesa la solicitud, usa Entity Framework Core y guarda o consulta datos en SQL Server.

## 5. Seguridad

El sistema usa login con JWT.

Primero el usuario inicia sesion en:

```text
POST /api/auth/login
```

Luego el sistema devuelve un token. Ese token se pega en Swagger con el boton Authorize y permite consumir los endpoints protegidos.

## 6. Base de datos

La base de datos se llama:

```text
IndotelCoreDb
```

Se ejecuta en SQL Server usando Docker.

Tablas principales:

- Usuarios.
- Roles.
- Ciudadanos.
- Prestadoras.
- ServiciosTelecom.
- Reclamaciones.
- HistorialReclamaciones.
- RespuestasPrestadora.
- Auditorias.

## 7. Flujo principal demostrado

El flujo principal es:

1. Iniciar sesion.
2. Autorizar Swagger con token.
3. Consultar catalogos.
4. Registrar ciudadano.
5. Crear reclamacion.
6. Cambiar estado de reclamacion.
7. Registrar respuesta de prestadora.
8. Consultar historial.
9. Consultar reportes.

## 8. Endpoints destacados

Auth:

- POST /api/auth/login
- GET /api/auth/me

Catalogos:

- GET /api/catalogos/roles
- GET /api/catalogos/servicios
- GET /api/catalogos/prestadoras

Ciudadanos:

- GET /api/ciudadanos
- POST /api/ciudadanos

Reclamaciones:

- GET /api/reclamaciones
- POST /api/reclamaciones
- PUT /api/reclamaciones/{id}/estado
- POST /api/reclamaciones/{id}/respuesta-prestadora
- GET /api/reclamaciones/{id}/historial

Reportes:

- GET /api/reportes/resumen
- GET /api/reportes/reclamaciones-por-estado
- GET /api/reportes/reclamaciones-por-prestadora

## 9. Resultado obtenido

El Core esta funcional. Permite autenticar usuarios, proteger endpoints, registrar datos, manejar reclamaciones, guardar historial y generar reportes.

## 10. Cierre

En conclusion, el Core funciona como el motor central del Sistema Digital INDOTEL y esta preparado para integrarse con la parte Web, la parte de Caja y la parte de APIs y Gateway.
