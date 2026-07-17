# Entrega final para Windows VM por WhatsApp

Fecha: 17 de julio de 2026.

Rama: `integracion-core-web-api-caja-offline`.

## Artefacto

- Archivo: `INDOTEL_PROYECTO_COMPLETO_VM_WHATSAPP_FINAL.zip`
- SHA-256: `e92f284e0701d708573d6e176df431281405bc1c46e323ed717e4e093d62a016`
- Archivos: 645
- Integridad ZIP: verificada sin errores.

## Modalidad de ejecución

- Ubuntu físico ejecuta SQL Server, Core API, API Gateway, Portal Web y proveedor de pagos mediante Docker.
- Windows 11 VirtualBox ejecuta Caja WPF, Core administrativo WinForms y navegador.
- Caja consume el Gateway.
- Core WinForms consume Core API.
- Ninguna aplicación de escritorio accede directamente a SQL Server.

## Uso

1. Enviar el ZIP como Documento por WhatsApp.
2. Descargar en Windows.
3. Extraer en Descargas.
4. Ejecutar `PREPARAR_VM_WINDOWS_COMPLETA.bat`.
5. Ejecutar `INICIAR_APLICACIONES_VM_WINDOWS.bat`.

## Automatización

Los scripts verifican Visual Studio y .NET 8, detectan Ubuntu, configuran URLs, prueban conectividad, compilan Caja/Core UI, crean accesos directos y dejan diagnóstico escrito.

## Evidencia previa

- Core: 57/57 pruebas.
- Gateway: 24/24 pruebas.
- Web: 13/13 pruebas.
- Proveedor externo: compilación correcta.
- SQL Server, Core, Gateway, Web y pagos: contenedores `healthy` en Ubuntu.
