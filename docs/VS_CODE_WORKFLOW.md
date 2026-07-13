# Guia de trabajo con Visual Studio Code

Esta guia explica como trabajar el Core INDOTEL desde Visual Studio Code sin afectar a los companeros que usen Visual Studio.

## 1. Idea principal

No importa si una persona usa VS Code y otra usa Visual Studio. Lo importante es que todos trabajen con:

- El mismo repositorio.
- La misma rama o ramas acordadas.
- El mismo SDK de .NET.
- La misma solucion .sln.
- Los mismos proyectos .csproj.
- Los mismos comandos de build y run.

## 2. Por que no afecta

Visual Studio Code y Visual Studio son editores diferentes, pero el proyecto .NET se controla por archivos del repositorio:

- .sln: solucion.
- .csproj: configuracion del proyecto.
- appsettings.json: configuracion.
- Program.cs: entrada del backend.
- Migrations o scripts SQL: base de datos.

Mientras esos archivos esten correctos, el proyecto puede abrirse en ambos entornos.

## 3. Herramientas necesarias en VS Code

Instalar:

- .NET SDK.
- Visual Studio Code.
- Extension C# Dev Kit.
- Extension GitHub Pull Requests, opcional.
- SQL Server o SQL Server Express.
- Git.

## 4. Comandos base

Ver version de .NET:

```bash
dotnet --version
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

Crear migracion:

```bash
dotnet ef migrations add InitialCreate
```

Actualizar base de datos:

```bash
dotnet ef database update
```

## 5. Regla para evitar problemas con companeros

El repositorio debe incluir un archivo global.json para fijar la version del SDK aceptada.

Ejemplo:

```json
{
  "sdk": {
    "version": "8.0.302",
    "rollForward": "latestFeature"
  }
}
```

## 6. Flujo de trabajo recomendado

1. Cambiar a la rama core.
2. Hacer pull.
3. Crear o modificar archivos.
4. Ejecutar dotnet build.
5. Probar en Swagger.
6. Documentar el cambio.
7. Hacer commit.
8. Subir cambios.

## 7. Lo que no se debe subir

No subir:

- bin/
- obj/
- .vs/
- claves reales.
- cadenas de conexion personales.
- archivos .user.
- bases .mdf si no son parte acordada de la entrega.

## 8. Como aprender con este proyecto

Cada vez que creemos una parte nueva, se debe entender:

- Que archivo se creo.
- Que problema resuelve.
- Como se conecta con los demas archivos.
- Como probarlo en Swagger.
- Que regla de negocio aplica.

## 9. Meta personal

La meta no es solo que el proyecto funcione. La meta es que puedas abrir el proyecto en VS Code, leerlo, correrlo, probarlo y explicar cada parte frente al maestro.
