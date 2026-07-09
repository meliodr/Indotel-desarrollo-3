# Arquitectura del Core INDOTEL

## Objetivo

El Core es el backend principal del Sistema Digital INDOTEL. Su funcion es centralizar la logica, la seguridad, la base de datos y los servicios que consumen las otras partes del sistema.

## Diagrama general

```text
+----------------------+       +----------------------+       +----------------------+
| Portal Web           |       | Caja / Ventanilla    |       | APIs y Gateway       |
| Ciudadano            |       | Personal interno     |       | Integraciones        |
+----------+-----------+       +----------+-----------+       +----------+-----------+
           |                              |                              |
           | HTTP / JSON + JWT           | HTTP / JSON + JWT           | HTTP / JSON + JWT
           +------------------------------+------------------------------+
                                          |
                              +-----------v------------+
                              | INDOTEL Core API       |
                              | ASP.NET Core Web API   |
                              +-----------+------------+
                                          |
                              +-----------v------------+
                              | Entity Framework Core  |
                              +-----------+------------+
                                          |
                              +-----------v------------+
                              | SQL Server Docker      |
                              | IndotelCoreDb          |
                              +------------------------+
```

## Componentes

### 1. Portal Web

Parte publica/ciudadana. Permite que el ciudadano consulte servicios, registre datos y presente reclamaciones.

Consume endpoints como:

- POST /api/auth/login
- GET /api/catalogos/servicios
- POST /api/ciudadanos
- POST /api/reclamaciones
- GET /api/reclamaciones/{id}

### 2. Caja / Ventanilla

Parte interna para personal de atencion. Permite registrar ciudadanos, crear reclamaciones, cambiar estados y revisar historial.

Consume endpoints como:

- GET /api/ciudadanos
- POST /api/ciudadanos
- GET /api/reclamaciones
- PUT /api/reclamaciones/{id}/estado
- GET /api/reclamaciones/{id}/historial

### 3. APIs y Gateway

Capa intermedia para organizar integraciones o servicios externos simulados.

Puede consumir y exponer datos relacionados con:

- Validacion de ciudadano.
- Consulta de prestadoras.
- Simulacion de IMEI.
- Simulacion de respuestas externas.

### 4. Core API

Es la pieza central. Contiene:

- Seguridad JWT.
- Controladores.
- Reglas de negocio.
- Modelos.
- DbContext.
- Datos semilla.
- Reportes.

### 5. Base de datos

SQL Server se ejecuta con Docker y guarda la informacion en `IndotelCoreDb`.

Tablas principales:

- Roles.
- Usuarios.
- Ciudadanos.
- Prestadoras.
- ServiciosTelecom.
- Reclamaciones.
- RespuestasPrestadora.
- HistorialReclamaciones.
- Auditorias.

## Flujo principal de una reclamacion

```text
Ciudadano / Caja
      |
      v
Registrar ciudadano
      |
      v
Crear reclamacion
      |
      v
Estado: RECIBIDA
      |
      v
Analista valida
      |
      v
Estado: VALIDADA
      |
      v
Prestadora responde
      |
      v
Estado: RESPONDIDA_POR_PRESTADORA
      |
      v
Historial y reportes
```

## Seguridad

El Core usa JWT.

Flujo de autenticacion:

```text
POST /api/auth/login
      |
      v
Devuelve token
      |
      v
Swagger / Web / Caja envia Authorization: Bearer TOKEN
      |
      v
Core valida token
      |
      v
Permite consumir endpoints protegidos
```

## Tecnologias usadas

- C#.
- ASP.NET Core Web API.
- Entity Framework Core.
- SQL Server.
- Docker.
- JWT Bearer Authentication.
- Swagger / OpenAPI.
- GitHub.
- VS Code / Visual Studio.

## Resultado

El Core queda listo para integrarse con Web, Caja y Gateway porque ya expone endpoints protegidos y funcionales para el flujo principal del sistema.
