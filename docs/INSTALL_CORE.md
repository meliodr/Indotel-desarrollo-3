# Instalacion inicial del Core INDOTEL

## Requisitos

- Git.
- .NET SDK 8.
- Visual Studio Code o Visual Studio.
- Extension C# Dev Kit si se usa VS Code.

## Rama de trabajo

La rama del Core es:

```text
core
```

## Carpeta del proyecto

```text
core-indotel/Indotel.Core
```

## Comandos principales

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

## Pruebas iniciales

Swagger:

```text
http://localhost:5085/swagger
```

Health:

```text
http://localhost:5085/health
```

La respuesta de health debe indicar estado OK.

## Problemas comunes

Si dotnet no aparece, falta instalar el SDK.

Si Swagger no abre, revisar que el ambiente este en Development.

Si el puerto esta ocupado, cambiar el puerto en launchSettings.json.
