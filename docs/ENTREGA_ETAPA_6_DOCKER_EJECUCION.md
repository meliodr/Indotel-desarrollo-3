# Entrega Etapa 6 — Docker y ejecución conjunta

Fecha: 2026-07-16

Rama de trabajo: `integracion-core-web-api-caja-offline`.

## Artefacto completo

- Archivo: `INDOTEL_PROYECTO_COMPLETO_ETAPA_6.zip`
- SHA-256: `2aedee7526e1bab8b1566e80595aaf7f8f75cbacd93483ac5b9bd4d6caeb9c74`
- Archivos incluidos: 626
- Integridad ZIP: verificada sin errores

## Alcance

- SQL Server, Core, API Gateway, Web y procesador de pagos simulado en Docker Compose.
- Gateway sin dependencia de inicio respecto al Core.
- Persistencia para SQL Server, SQLite del Gateway, cargas del Core y claves Web.
- Health checks para todos los servicios de aplicación.
- Imágenes Core, Gateway, Web y Pagos Mock ejecutadas con usuario no root.
- Scripts Windows y Ubuntu para inicio, parada, estado, logs, respaldo y contingencia.
- Caja WPF y GUI WinForms fuera de Docker, con URLs configurables por variables de entorno.
- Docker Compose de release con Caddy y TLS interno.

## Validación

La revisión estática de las etapas 0–6 terminó con cero errores estructurales. La evidencia runtime previa confirma Core con 57 de 57 pruebas y sus migraciones aplicadas. La compilación runtime final de Gateway, Web, proveedor externo, Caja WPF y GUI WinForms debe ejecutarse en los equipos Ubuntu/Windows mediante los scripts incluidos.

El árbol completo del código se entrega dentro del ZIP. El conector de GitHub mantiene este manifiesto y los checkpoints, pero no realizó la carga masiva de los 626 archivos.
