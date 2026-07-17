# Corrección de compilación de Caja WPF en Windows

Fecha: 17 de julio de 2026.

Rama: `integracion-core-web-api-caja-offline`.

## Evidencia

La solución restauró y compiló correctamente Core UI, proveedor de pagos, API Gateway, Web y Core API. La compilación de `Indotel.Caja` falló durante el proyecto temporal de WPF (`*_wpftmp.csproj`) con errores `CS0103` para `Path`, `File`, `Directory` y `HttpRequestException`, además de errores `CS0029` en el `switch` de tipos MIME.

## Causa

Cinco archivos de Caja dependían de importaciones implícitas que no estuvieron disponibles de forma consistente durante la compilación temporal XAML de Visual Studio 2026 Insiders.

## Corrección

Se requieren importaciones explícitas:

- `System.IO` en `CredencialOfflineService.cs`.
- `System.IO` en `LogLocalService.cs`.
- `System.IO` en `AlmacenLocalService.cs`.
- `System.IO` en `ApiClient.cs`.
- `System.Net.Http` en `ColaOfflineService.cs`.

Los errores `CS0029` eran errores en cascada causados por `Path` no resuelto; no se cambió la lógica de tipos MIME.

## Script

Ejecutar desde PowerShell en la raíz del repositorio:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\windows\CORREGIR_USINGS_CAJA_WPF.ps1
```

Después ejecutar:

```text
01_PREPARAR_TODO_EN_WINDOWS.bat
```

## Artefacto corregido

- `INDOTEL_TODO_WINDOWS_FINAL_V2_CORREGIDO.zip`
- SHA-256: `cbfb8eb25de307836ef43271c07db415963c495349871185de2f3fad6fa295d2`
- Archivos de proyecto: 666
- Integridad ZIP: verificada sin errores.

## Estado de validación

La corrección fue revisada estáticamente y empaquetada sin errores. La compilación final debe confirmarse en Windows, porque el entorno de empaquetado no dispone del SDK de Windows/WPF.
