# Entrega integrada Core, Web, API Gateway y Caja

Fecha de consolidación: 2026-07-16.

Rama de respaldo: `integracion-core-web-api-caja-offline`.

## Artefacto final

- Archivo: `INDOTEL_PROYECTO_COMPLETO.zip`
- SHA-256: `a26c158bb88af1414f98aa9a4eff81d8e54aa12c5e720f31ad5be348fe9dd7ec`
- Carpeta raíz: `INDOTEL-Proyecto-Completo`

Respaldo reducido de fuentes preparado para transferencia:

- Archivo: `INDOTEL_SOURCE_BACKUP_GITHUB.tar.xz`
- SHA-256: `1745b72c50e7c2ba060ca91a3b1b54a35c633525de717c7579898c11555510cd`

## Componentes consolidados

- Core ASP.NET Core y GUI administrativa WinForms.
- API Gateway.
- Portal Web MVC.
- Caja WPF sobre .NET 8, conservando su tecnología original.
- SQL Server y configuración Docker Compose.
- Scripts de inicio, detención, diagnóstico y validación.
- Logo INDOTEL en Web, Caja, GUI del Core y recibos.

## Caja y contingencia

- Emisión, impresión y reimpresión de recibos.
- Recibos provisionales identificados cuando se trabaja sin conexión.
- Caché local de jornada y deudas previamente consultadas.
- Cola duradera de operaciones offline.
- Identificador idempotente estable para evitar cobros duplicados.
- Sincronización automática y manual cuando Core/Gateway vuelven.
- Bloqueo de cierre de jornada hasta completar la sincronización.
- Apertura de jornadas nuevas únicamente en línea.
- Operaciones offline limitadas a efectivo y tarjeta simulada.
- Credenciales offline protegidas mediante PBKDF2 con sal y vigencia limitada.

## Validación disponible

La validación estática final reportó cero errores y cero advertencias estructurales. La línea base del Core había compilado sin advertencias y sus 40 pruebas pasaron. La consolidación final de Web, Gateway, Caja WPF y los contratos más recientes debe validarse en Windows con `VALIDAR_PROYECTO_WINDOWS.ps1`, ya que el entorno de preparación no dispone del SDK Windows Desktop necesario para ejecutar WPF/WinForms.

## Estado de GitHub

Esta rama contiene los checkpoints y la documentación de la integración. El árbol completo del artefacto no se ha transferido todavía a GitHub debido a las limitaciones del conector para cargas masivas. Debe copiarse desde el ZIP final a un clon local, validarse en Windows y luego confirmarse en esta misma rama.
