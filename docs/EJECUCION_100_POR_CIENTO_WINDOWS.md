# Ejecucion 100 % en Windows

Fecha: 17 de julio de 2026.

## Arquitectura local

Todo se ejecuta dentro de la misma maquina Windows:

- SQL Server Express LocalDB.
- Core API ASP.NET Core en `http://localhost:5085`.
- API Gateway en `http://localhost:5185`.
- Portal Web en `http://localhost:5234`.
- Proveedor de pagos simulado en `http://localhost:5285`.
- Caja WPF.
- Core administrativo WinForms.

No se requiere Ubuntu, Docker Desktop, WSL ni acceso a otra computadora.

## Base de datos

El Core usa `(localdb)\MSSQLLocalDB` y crea `IndotelCoreDb` mediante migraciones de Entity Framework. El seed de desarrollo crea los roles, sucursal, caja, catalogos y usuarios de demostracion.

## Automatizacion

`01_PREPARAR_TODO_EN_WINDOWS.bat`:

1. Detecta Visual Studio 2022 o 2026, incluyendo Insiders.
2. Comprueba WPF, WinForms y ASP.NET.
3. Agrega SQL Server Express LocalDB mediante Visual Studio Installer.
4. Verifica o instala SDK .NET 8.
5. Inicia LocalDB.
6. Restaura y compila los nueve proyectos.
7. Ejecuta 57 pruebas Core, 24 Gateway y 13 Web.
8. Crea accesos directos.

`02_INICIAR_TODO_EN_WINDOWS.bat` inicia los cuatro servicios, espera sus health checks y abre Caja, Core WinForms, Web y Swagger.

## Datos locales

Los logs, PID y almacenamiento del Gateway se guardan en `.windows-runtime`.

## Detencion

`04_DETENER_TODO_EN_WINDOWS.bat` detiene los servicios, las interfaces y LocalDB.
