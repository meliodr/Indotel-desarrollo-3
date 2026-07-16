# Integración Core, Web, API Gateway y Caja

Rama de trabajo: `integracion-core-web-api-caja-offline`.

## Decisiones confirmadas

- Caja conserva su tecnología original: WPF sobre .NET 8 para Windows.
- Web y Caja consumen el API Gateway; no acceden directamente a SQL Server.
- La Caja continuará operando temporalmente cuando Core/Gateway no estén disponibles mediante caché local, cola duradera e idempotencia.
- Los recibos offline serán provisionales hasta que Core confirme la operación.
- La jornada debe abrirse en línea; una jornada ya abierta podrá continuar de forma controlada sin conexión.
- El cierre de jornada requerirá reconexión y sincronización completa.
- Se usarán los métodos EFECTIVO y TARJETA_SIMULADA en modo offline; los métodos que requieren conciliación bancaria permanecerán bloqueados sin conexión.
- El logo INDOTEL se incorporará en Web, Caja, GUI del Core y recibos.
- La entrega final se consolidará en un solo ZIP con Core, Web, Gateway, Caja, Docker, scripts y documentación.

## Estado

Trabajo en curso. Este archivo sirve como punto de control para evitar pérdida de decisiones mientras se completa y valida la integración.
