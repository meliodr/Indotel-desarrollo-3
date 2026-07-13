# INDOTEL Core

Backend principal del Sistema Digital INDOTEL.

## Que hace este proyecto

Este Core guardara los datos principales, aplicara reglas de negocio y expondra endpoints para los demas modulos.

Por ahora contiene la base inicial:

- ASP.NET Core Web API.
- Swagger.
- Endpoint GET /health.
- Estructura de carpetas para Controllers, Models, DTOs, Data, Services y Helpers.

## Como ejecutarlo en VS Code

Entrar a la carpeta del proyecto:

```bash
cd core-indotel/Indotel.Core
```

Restaurar paquetes:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

Ejecutar:

```bash
dotnet run
```

Abrir Swagger:

```text
http://localhost:5085/swagger
```

Probar salud del servicio:

```text
http://localhost:5085/health
```

## Estructura

```text
Indotel.Core/
  Controllers/  -> endpoints HTTP
  Models/       -> tablas de base de datos
  DTOs/         -> datos de entrada y salida
  Data/         -> DbContext y configuracion de BD
  Services/     -> logica de negocio
  Helpers/      -> funciones auxiliares
```

## Primer objetivo cumplido

El primer bloque del Core estara listo cuando:

- El proyecto compile.
- dotnet run funcione.
- Swagger abra.
- GET /health responda OK.
