# Entrega de etapas 3, 4 y 5

Rama: `integracion-core-web-api-caja-offline`.

## Artefactos

- `INDOTEL_PROYECTO_ETAPAS_3_A_5.zip`
- SHA-256: `c9f9cf6d3c0c51f871de7c09525289902d6f8074df4acdd546e2d55701db058b`
- `PARCHE_ETAPAS_3_A_5.zip`
- SHA-256: `11695d14f73be78f26452d83b9282f5c75dede1641ce595de6ec2eda90391966`

## Etapa 3

- Gateway con SQLite persistente para caché, operaciones pendientes y auditoría.
- Catálogo propio de productos, servicios e inventario cuando Core no responde.
- Cola idempotente y sincronización ordenada al regresar Core.
- Auditoría Canal→Gateway, Gateway→Core y Gateway→proveedor externo.
- Procesador de pagos externo simulado.
- Inicio independiente del contenedor Core.

## Etapa 4

- Caja conserva WPF y .NET 8 para Windows.
- Sucursal obtenida del JWT y cajas filtradas por Core.
- Recibos confirmados y provisionales con logo, sucursal, impresión y reimpresión.
- Operación mediante Gateway y cola local solo cuando también falla el Gateway.
- Seguimiento y confirmación de recibos al sincronizar.
- Cierre bloqueado mientras haya contingencia u operaciones pendientes.

## Etapa 5

- Web usa exclusivamente `IndotelGateway`.
- Se eliminó `QueueBackgroundProcessor` y la cola destructiva del portal.
- Órdenes compuestas con creación/confirmación idempotentes.
- Pagos y órdenes aceptados con `202` se siguen por `operationId`.
- Simulación de pagos mediante el proveedor externo a través del Gateway.

## Validación disponible

- Validación estática etapas 0–2: correcta.
- Validación estática etapas 3–5: correcta.
- 339 archivos C# y 19 XAML revisados léxicamente.
- JSON, proyectos y Docker Compose válidos.
- ZIP completos sin errores de compresión.
- Evidencia previa: Core 0 errores/advertencias, 57/57 pruebas y migraciones aplicadas.

La compilación de Gateway, Web, mock y Caja WPF, y la prueba runtime de caída del Core, deben ejecutarse con los scripts incluidos en los equipos del proyecto. No se declara cumplimiento final hasta conservar esas salidas.
