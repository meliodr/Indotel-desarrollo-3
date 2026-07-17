# Estado de validación antes de la prueba en Windows VM

Fecha de consolidación: 17 de julio de 2026.

## Confirmado en Ubuntu

```text
Validación estática Etapa 6: 0 errores, 0 advertencias
Core:                         compilación correcta
Pruebas Core:                 57/57
API Gateway:                  compilación correcta
Pruebas Gateway:              24/24
Portal Web:                   compilación correcta
Pruebas Web:                  13/13
Proveedor de pagos:           compilación correcta
SQL Server Docker:            healthy
Core Docker:                  healthy
Gateway Docker:               healthy
Web Docker:                   healthy
Proveedor de pagos Docker:    healthy
```

## Pendiente en la máquina virtual Windows

- Compilar Caja WPF con el Windows Desktop SDK.
- Compilar GUI administrativa WinForms.
- Abrir ambas aplicaciones.
- Validar credenciales y permisos.
- Abrir jornada de Caja y registrar monto inicial.
- Registrar entradas, salidas y cobros.
- Imprimir y reimprimir recibos, incluido Print to PDF.
- Probar operación offline y sincronización.
- Cerrar y cuadrar la jornada.
- Revisar auditoría en Core y Gateway.

## Topología de la prueba

```text
Windows VM Caja WPF ────────> Ubuntu Gateway 5185 ──> Core 5085 ──> SQL Server
Windows VM navegador ───────> Ubuntu Web 5234 ──────> Gateway
Windows VM Core WinForms ───> Ubuntu Core 5085
```

La base de datos y los servicios de servidor permanecen en Ubuntu. No se requiere Docker Desktop dentro de Windows VirtualBox.
